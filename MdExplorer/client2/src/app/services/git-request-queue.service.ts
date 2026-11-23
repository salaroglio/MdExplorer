import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subscription, of, timer } from 'rxjs';
import { distinctUntilChanged, map, share, switchMap } from 'rxjs/operators';

/** How a git request stands in the line. */
export type GitRequestKind =
  /** A read nobody asked for with a click (polling, counters): it waits its turn at the back. */
  | 'background'
  /** A read the user is waiting for (a diff just opened): next in line. */
  | 'asked'
  /** A command the user gave (commit, push, pull, discard): it leaves at once. */
  | 'command';

export interface GitQueueState {
  running: number;
  queued: number;
}

interface Job {
  key: string | null;
  background: boolean;
  started: boolean;
  start: () => void;
}

/**
 * The line every request about git goes through, in this window.
 *
 * The browser keeps six requests open towards the service, no more. On a machine where git answers
 * in tens of seconds the git readings alone took all six, and a document, with the hundred scripts
 * of its page, waited behind them (measured on 23/11/2026: 0,3 s with three open, over 38 s with
 * six). Here at most {@link MAX_AT_ONCE} leave together: the others wait, and the rest of MdExplorer
 * always finds free connections.
 *
 * Two readings with the same address are never in flight together, and never more than one waits:
 * whoever asks again while one is already waiting gets that one's answer. It starts after the one
 * in flight has ended, so the answer is never older than the question.
 */
@Injectable({ providedIn: 'root' })
export class GitRequestQueueService {
  static readonly MAX_AT_ONCE = 3;
  /** A wait shorter than this is not worth showing: the counters would flicker at every poll. */
  static readonly SHOWN_AFTER_MS = 800;

  private running = 0;
  private queue: Job[] = [];
  private runningKeys = new Set<string>();
  private waitingByKey = new Map<string, Observable<any>>();

  private stateSubject = new BehaviorSubject<GitQueueState>({ running: 0, queued: 0 });
  /** How many git requests are in flight and how many wait behind them. */
  readonly state$: Observable<GitQueueState> = this.stateSubject.asObservable();

  /** True while an answer from git has been awaited for more than {@link SHOWN_AFTER_MS}. */
  readonly waiting$: Observable<boolean> = this.stateSubject.pipe(
    map(s => s.running + s.queued > 0),
    distinctUntilChanged(),
    switchMap(busy => busy
      ? timer(GitRequestQueueService.SHOWN_AFTER_MS).pipe(map(() => true))
      : of(false)),
    distinctUntilChanged(),
  );

  /**
   * @param key the identity of a reading (its full address): readings with the same key are merged.
   *            Null for what must never be merged.
   * @param ownCopy makes, for each one who asked, a copy of what arrives. A merged answer reaches
   *            several callers: without it they would hold the very same object, and a change made
   *            by one would show up in the others.
   */
  schedule<T>(key: string | null, kind: GitRequestKind, work: () => Observable<T>, ownCopy?: (value: T) => T): Observable<T> {
    const forEachCaller = (source: Observable<T>) => ownCopy ? source.pipe(map(ownCopy)) : source;
    if (key !== null) {
      const waiting = this.waitingByKey.get(key);
      if (waiting) return forEachCaller(waiting);
    }

    const queued: Observable<T> = new Observable<T>(subscriber => {
      let inner: Subscription | null = null;
      let ended = false;

      const job: Job = {
        key,
        background: kind === 'background',
        started: false,
        start: () => {
          job.started = true;
          this.running++;
          if (key !== null) {
            this.runningKeys.add(key);
            if (this.waitingByKey.get(key) === shared) this.waitingByKey.delete(key);
          }
          this.publish();
          inner = work().subscribe({
            next: v => subscriber.next(v),
            error: e => { end(); subscriber.error(e); },
            complete: () => { end(); subscriber.complete(); },
          });
        },
      };

      const end = () => {
        if (ended) return;
        ended = true;
        if (job.started) {
          this.running--;
          if (key !== null) this.runningKeys.delete(key);
        } else {
          this.queue = this.queue.filter(j => j !== job);
          if (key !== null && this.waitingByKey.get(key) === shared) this.waitingByKey.delete(key);
        }
        this.pump();
      };

      if (kind === 'command') job.start();
      else {
        if (kind === 'asked') {
          // Ahead of the background readings, behind what the user asked before.
          const firstBackground = this.queue.findIndex(j => j.background);
          if (firstBackground < 0) this.queue.push(job); else this.queue.splice(firstBackground, 0, job);
        } else {
          this.queue.push(job);
        }
        this.pump();
      }

      return () => { inner?.unsubscribe(); end(); };
    });

    const shared = key !== null ? queued.pipe(share()) : queued;
    if (key !== null) this.waitingByKey.set(key, shared);
    return forEachCaller(shared);
  }

  private pump(): void {
    while (this.running < GitRequestQueueService.MAX_AT_ONCE) {
      const index = this.queue.findIndex(j => j.key === null || !this.runningKeys.has(j.key));
      if (index < 0) break;
      const [job] = this.queue.splice(index, 1);
      job.start();
    }
    this.publish();
  }

  private publish(): void {
    const next = { running: this.running, queued: this.queue.length };
    const now = this.stateSubject.value;
    if (now.running !== next.running || now.queued !== next.queued) this.stateSubject.next(next);
  }
}
