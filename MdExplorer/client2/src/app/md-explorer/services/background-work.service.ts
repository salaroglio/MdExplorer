import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest } from 'rxjs';
import { distinctUntilChanged, map } from 'rxjs/operators';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { ProjectsService } from './projects.service';
import { IndexingProgressService, IndexingProgressState } from './indexing-progress.service';
import { CodeGraphService, DescriptionRun } from './code-graph.service';

/** Una riga del pannello dei lavori in sfondo. */
export interface BackgroundRow {
  id: string;
  kind: 'indexing' | 'scan' | 'description';
  folder?: string;
  /** running = lavoro vivo; interrupted = MdExplorer chiuso a metà; stopped = fermata dall'utente; failed; done = appena finita. */
  state: 'running' | 'interrupted' | 'stopped' | 'failed' | 'done';
  /** 0-100, null = durata sconosciuta (barra indeterminata). */
  percent: number | null;
  index?: number;
  total?: number;
  currentFile?: string;
  engine?: string;
  /** Tempo che manca, stimato dalla velocità di QUESTA sessione; null finché non c'è almeno un file fatto. */
  etaMs?: number | null;
  updatedAt?: string;
  filesDescribed?: number;
  error?: string;
}

/**
 * Il pannello «Costruzione della conoscenza» con un lavoro per riga (punto 2d del 14/10/2026): indicizzazione dei
 * documenti, scansione del codice, descrizione del codice. Prima era una MatSnackBar per la sola indicizzazione, ma
 * Material ne tiene UNA alla volta: qualunque altro avviso la chiudeva, e una descrizione dura ore.
 *
 * Le righe del codice vengono dal backend: GET /api/codegraph/background all'apertura del progetto e a ogni
 * riconnessione (anche dopo un riavvio: le descrizioni interrotte sono lì, con «Riprendi» e «Lascia stare»), poi gli
 * eventi SignalR codeDescriptionStatus / codeScanStatus mandati a tutte le finestre.
 */
@Injectable({ providedIn: 'root' })
export class BackgroundWorkService {

  private readonly indexingVisible$ = new BehaviorSubject<boolean>(false);
  private readonly codeRows$ = new BehaviorSubject<BackgroundRow[]>([]);
  private projectPath: string | null = null;
  private doneTimers = new Map<string, any>();

  /** Le righe da mostrare: prima l'indicizzazione, poi scansioni e descrizioni. */
  public readonly rows$: Observable<BackgroundRow[]>;

  constructor(
    private serverMessages: MdServerMessagesService,
    private projects: ProjectsService,
    private indexing: IndexingProgressService,
    private codeGraph: CodeGraphService,
  ) {
    this.rows$ = combineLatest([this.indexing.state$, this.indexingVisible$, this.codeRows$]).pipe(
      map(([state, visible, code]) => visible ? [this.indexingRow(state), ...code] : code),
    );

    this.serverMessages.parsingProjectStart$.subscribe(() => { this.indexing.reset(); this.indexingVisible$.next(true); });
    this.serverMessages.knowledgeProgress$.subscribe(d => this.indexing.setProgress(d?.processed ?? 0, d?.total ?? 0, d?.percent ?? 0));
    this.serverMessages.parsingProjectStop$.subscribe(() => {
      this.indexing.setComplete();
      setTimeout(() => this.indexingVisible$.next(false), 1500);
    });

    this.projects.currentProjects$.pipe(map(p => p?.path ?? null), distinctUntilChanged()).subscribe(path => {
      this.projectPath = path;
      this.codeRows$.next([]);
      this.reload();
    });
    // dopo una riconnessione gli eventi persi si recuperano rileggendo lo stato
    this.serverMessages.connectionId$.subscribe(() => this.reload());

    this.serverMessages.codeDescriptionStatus$.subscribe(e => {
      if (!this.mine(e?.projectPath) || !e?.run) { return; }
      const run: DescriptionRun = e.run;
      const state = run.status === 'done' ? 'done' : run.status === 'cancelled' ? 'stopped' : run.status === 'failed' ? 'failed' : 'running';
      this.upsert(this.descriptionRow(run, state));
      if (state === 'done') { this.removeLater('description:' + run.programFolder, 6000); }
    });
    this.serverMessages.codeScanStatus$.subscribe(e => {
      if (!this.mine(e?.projectPath)) { return; }
      const id = 'scan:' + e.folder;
      if (e.phase === 'started') {
        this.upsert({ id, kind: 'scan', folder: e.folder, state: 'running', percent: null });
      } else if (e.phase === 'failed') {
        this.upsert({ id, kind: 'scan', folder: e.folder, state: 'failed', percent: null, error: e.error });
      } else {
        this.upsert({ id, kind: 'scan', folder: e.folder, state: 'done', percent: 100, total: e.files, index: e.reparsed });
        this.removeLater(id, 4000);
      }
    });
  }

  /** «Riprendi» da una riga interrotta, fermata o fallita. */
  resume(row: BackgroundRow): Observable<any> {
    return this.codeGraph.resumeDescribe(this.projectPath, row.folder, this.serverMessages.connectionId);
  }

  /** «Lascia stare»: la traccia si toglie e la riga sparisce. */
  dismiss(row: BackgroundRow): Observable<any> {
    return this.codeGraph.dismissDescribe(this.projectPath, row.folder).pipe(map(r => { this.remove(row.id); return r; }));
  }

  /** «Ferma» una descrizione in corso, da qualunque finestra. */
  stop(row: BackgroundRow): Observable<any> {
    return this.codeGraph.stopDescribe(this.projectPath, row.folder);
  }

  /** Chiude una riga che non ha più niente da fare (scansione fallita, letta). */
  close(row: BackgroundRow): void { this.remove(row.id); }

  private reload(): void {
    const path = this.projectPath;
    if (!path) { return; }
    this.codeGraph.background(path).subscribe({
      next: b => {
        if (this.projectPath !== path) { return; }
        const rows: BackgroundRow[] = [
          ...(b.scans ?? []).map(s => ({ id: 'scan:' + s.folder, kind: 'scan' as const, folder: s.folder, state: 'running' as const, percent: null })),
          ...(b.descriptions ?? []).map(d => this.descriptionRow(d.run, d.state)),
        ];
        this.codeRows$.next(rows);
      },
      // il pannello non deve bloccare niente: l'errore si vede come riga
      error: err => this.upsert({ id: 'error', kind: 'description', state: 'failed', percent: null, error: err?.error?.error || err?.message || String(err) }),
    });
  }

  private descriptionRow(run: DescriptionRun, state: BackgroundRow['state']): BackgroundRow {
    const total = run.fileTotal || 0;
    // le domande FINITE: dal basso finiscono fuori ordine, e la prima non finita (fileIndex) restava ferma su una lenta
    const index = run.questionsDone ?? run.fileIndex ?? 0;
    return {
      id: 'description:' + run.programFolder, kind: 'description', folder: run.programFolder, state,
      percent: total > 0 ? Math.round(index * 100 / total) : null,
      index, total, currentFile: run.currentFile, engine: run.engine, updatedAt: run.updatedAt, filesDescribed: run.filesDescribed, error: run.error,
      etaMs: state === 'running' ? this.eta(run) : null,
    };
  }

  private eta(run: DescriptionRun): number | null {
    if (!run.sessionStartedAt) { return null; }
    const doneNow = run.questionsDone ?? run.fileIndex ?? 0;
    const done = doneNow - (run.sessionStartIndex || 0);
    if (done <= 0) { return null; }
    const elapsed = Date.now() - new Date(run.sessionStartedAt).getTime();
    return Math.max(0, elapsed / done * ((run.fileTotal || 0) - doneNow));
  }

  private indexingRow(state: IndexingProgressState): BackgroundRow {
    return { id: 'indexing', kind: 'indexing', state: state.done ? 'done' : 'running', percent: state.percent, index: state.processed, total: state.total };
  }

  private mine(projectPath: string): boolean {
    return !!projectPath && !!this.projectPath && projectPath.toLowerCase() === this.projectPath.toLowerCase();
  }

  private upsert(row: BackgroundRow): void {
    const t = this.doneTimers.get(row.id);
    if (t) { clearTimeout(t); this.doneTimers.delete(row.id); }
    // al suo posto, così le righe non saltano a ogni file
    const rows = this.codeRows$.value;
    const at = rows.findIndex(r => r.id === row.id);
    this.codeRows$.next(at < 0 ? [...rows, row] : rows.map((r, i) => i === at ? row : r));
  }

  private remove(id: string): void {
    this.codeRows$.next(this.codeRows$.value.filter(r => r.id !== id));
  }

  private removeLater(id: string, ms: number): void {
    this.doneTimers.set(id, setTimeout(() => { this.doneTimers.delete(id); this.remove(id); }, ms));
  }
}
