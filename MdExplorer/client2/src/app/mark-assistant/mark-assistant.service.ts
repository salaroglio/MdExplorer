import { Injectable, Injector } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject, Observable, Subscription, combineLatest, firstValueFrom } from 'rxjs';
import { filter, skip, take } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { ProjectsService } from '../md-explorer/services/projects.service';
import {
  MarkContext,
  MarkInputContext,
  MarkInputHandler,
  MarkLesson,
  MarkState,
  SpotlightRect,
} from './mark-types';
import { LESSONS, WELCOME_TOUR } from './lessons';
import { INPUT_HANDLER_TYPES } from './handlers';

const COMPLETED_KEY_PREFIX = 'mark.lesson.';
const DEFAULT_STEP_DURATION_MS = 1700;

/**
 * MarkAssistantService — Mark's brain.
 *
 * Responsibilities:
 *   • State machine (hidden / playing / minimized).
 *   • Lesson playback (typewriter + spotlight + step pacing).
 *   • Context-awareness — hides Mark when the route leaves the lesson's context.
 *   • User input dispatch — runs registered handlers when the user writes back.
 *   • Auto-trigger of the welcome tour on the very first MDE launch.
 */
@Injectable({ providedIn: 'root' })
export class MarkAssistantService {
  // ── Public observable state (subscribed by the component) ────────────────
  private readonly _state = new BehaviorSubject<MarkState>('hidden');
  readonly state$: Observable<MarkState> = this._state.asObservable();
  get currentState(): MarkState { return this._state.getValue(); }

  private readonly _text = new BehaviorSubject<string>('');
  readonly text$: Observable<string> = this._text.asObservable();

  private readonly _staticMode = new BehaviorSubject<boolean>(false);
  readonly staticMode$: Observable<boolean> = this._staticMode.asObservable();

  private readonly _spotlight = new BehaviorSubject<SpotlightRect | null>(null);
  readonly spotlight$: Observable<SpotlightRect | null> = this._spotlight.asObservable();

  private readonly _dim = new BehaviorSubject<boolean>(false);
  readonly dim$: Observable<boolean> = this._dim.asObservable();

  private readonly _continueArrow = new BehaviorSubject<boolean>(false);
  readonly continueArrow$: Observable<boolean> = this._continueArrow.asObservable();

  /** True while waiting for an input handler's response (typewriter still going). */
  private readonly _isResponding = new BehaviorSubject<boolean>(false);
  readonly isResponding$: Observable<boolean> = this._isResponding.asObservable();

  /**
   * True when Mark is detached into a separate Electron BrowserWindow.
   * While undocked, the in-app overlay hides itself and the service
   * forwards every state emit to the floating window via Electron IPC.
   */
  private readonly _isUndocked = new BehaviorSubject<boolean>(false);
  readonly isUndocked$: Observable<boolean> = this._isUndocked.asObservable();
  get isUndocked(): boolean { return this._isUndocked.getValue(); }

  // ── Internal ─────────────────────────────────────────────────────────────
  private abortFlag = false;
  private autoStartChecked = false;
  private currentLesson: MarkLesson | null = null;
  private inputHandlers: MarkInputHandler[] = [];
  /** Selector of the element the spotlight is currently anchored to. */
  private currentSpotlightSelector: string | null = null;
  /** RAF batching guard for spotlight recompute on scroll/resize. */
  private rafScheduled = false;
  /** Active subscriptions while undocked — disposed on redock. */
  private undockSubs: Subscription[] = [];
  /** Cleanup callbacks for Electron IPC listeners. */
  private undockCleanups: Array<() => void> = [];

  constructor(
    private translate: TranslateService,
    private projectsService: ProjectsService,
    private router: Router,
    private injector: Injector,
  ) {
    this.registerInputHandlers();
    this.subscribeRouteChanges();
    this.subscribeViewportChanges();
    this.checkAutoStart();
  }

  // ── Electron undock / redock ─────────────────────────────────────────────

  /** True if the Electron preload exposes the undock API (running inside Electron shell). */
  get canUndock(): boolean {
    return !!(window as any)?.electronAPI?.mark;
  }

  /**
   * Detach Mark into a separate Electron BrowserWindow. State stays here
   * (the lesson keeps running in this renderer); we just push every
   * observable change to the floating window via IPC, and route user
   * input back through submitUserInput().
   */
  async undock(): Promise<void> {
    if (!this.canUndock) return;
    if (this.isUndocked) return;
    const api = (window as any).electronAPI.mark;

    // Open the floating window with an initial state snapshot
    await api.undock(this.snapshotState());
    this._isUndocked.next(true);

    // Forward every subsequent state change to the mark-window
    const sub = combineLatest([
      this.text$,
      this.staticMode$,
      this.isResponding$,
    ]).subscribe(() => {
      api.pushState(this.snapshotState());
    });
    this.undockSubs.push(sub);

    // Route user input typed in the mark-window back into the dispatch chain
    this.undockCleanups.push(api.onUserInput((text: string) => {
      this.submitUserInput(text);
    }));

    // If the mark-window is closed externally (Alt+F4, OS gesture, redock btn).
    // Idempotent — multiple firings are silently ignored.
    this.undockCleanups.push(api.onClose(() => {
      if (!this._isUndocked.getValue()) return;
      this.cleanupUndockSubs();
      this._isUndocked.next(false);
    }));
  }

  /** Re-attach Mark to the main MDE window. */
  redock(): void {
    if (!this.canUndock) return;
    if (!this._isUndocked.getValue()) return;
    const api = (window as any).electronAPI.mark;
    api.redock();   // the 'onClose' listener above will flip _isUndocked back
  }

  /** Tear down all undock-time subscriptions and IPC listeners. */
  private cleanupUndockSubs(): void {
    this.undockSubs.forEach(s => s.unsubscribe());
    this.undockSubs = [];
    this.undockCleanups.forEach(fn => { try { fn(); } catch { /* noop */ } });
    this.undockCleanups = [];
  }

  /** Serialize the visible state for the standalone mark-window. */
  private snapshotState(): {
    text: string;
    staticMode: boolean;
    isResponding: boolean;
    transmittingLabel: string;
    inputPlaceholder: string;
  } {
    return {
      text: this._text.getValue(),
      staticMode: this._staticMode.getValue(),
      isResponding: this._isResponding.getValue(),
      transmittingLabel: this.translate.instant('MARK.TRANSMITTING'),
      inputPlaceholder: this.translate.instant('MARK.INPUT.PLACEHOLDER'),
    };
  }

  // ── Spotlight reactive anchoring ─────────────────────────────────────────

  /**
   * Re-read the current target's bounding rect when the viewport changes
   * (resize or scroll, including scrolls happening on inner containers
   * thanks to the capture-phase scroll listener). Coordinates flow back
   * through the spotlight$ observable, so the highlight box follows the
   * card even when the layout shifts.
   */
  private subscribeViewportChanges(): void {
    window.addEventListener('resize', this.scheduleSpotlightRecompute, { passive: true });
    // capture phase catches scrolls on any nested scrollable container
    window.addEventListener('scroll', this.scheduleSpotlightRecompute, { passive: true, capture: true });
  }

  private scheduleSpotlightRecompute = (): void => {
    if (this.rafScheduled) return;
    if (!this.currentSpotlightSelector) return;
    this.rafScheduled = true;
    requestAnimationFrame(() => {
      this.rafScheduled = false;
      this.recomputeSpotlight();
    });
  };

  private recomputeSpotlight(): void {
    if (!this.currentSpotlightSelector) return;
    const target = document.querySelector(this.currentSpotlightSelector) as HTMLElement | null;
    if (!target) {
      this._spotlight.next(null);
      return;
    }
    const r = target.getBoundingClientRect();
    this._spotlight.next({
      top: r.top, left: r.left, width: r.width, height: r.height, right: r.right,
    });
  }

  // ── Handler registry ─────────────────────────────────────────────────────

  /**
   * Resolves all handler classes from the registry via DI, then sorts them
   * by priority (descending). Higher-priority handlers are consulted first.
   */
  private registerInputHandlers(): void {
    const handlers = INPUT_HANDLER_TYPES.map(t => this.injector.get(t));
    handlers.sort((a, b) => b.priority - a.priority);
    this.inputHandlers = handlers;
  }

  // ── Context-awareness ────────────────────────────────────────────────────

  /**
   * When the user navigates away from the active lesson's context, Mark
   * disappears entirely (clears even the minimized badge). The "?" in the
   * title-bar always brings him back on demand.
   */
  private subscribeRouteChanges(): void {
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e: NavigationEnd) => {
      if (this.currentState === 'hidden') return;
      const ctx = this.currentLesson?.context ?? 'always';
      if (!this.isContextActive(ctx, e.urlAfterRedirects)) {
        this.hide();
      }
    });
  }

  private isContextActive(ctx: MarkContext, url: string): boolean {
    if (ctx === 'always') return true;
    if (ctx === 'projects-page') {
      return url === '/projects' || url.startsWith('/projects/');
    }
    if (ctx === 'main') return url.startsWith('/main');
    return false;
  }

  // ── Auto-start on first MDE launch ────────────────────────────────────────

  /**
   * Auto-start the welcome tour the very first time the user lands on
   * the projects page with zero projects and no completion flag.
   */
  private checkAutoStart(): void {
    if (this.isLessonCompleted(WELCOME_TOUR.id)) return;

    // mdProjects is a BehaviorSubject seeded with [] at service construction.
    // skip(1) ignores that init emit and waits for the first real fetch result
    // (triggered by ProjectsComponent.ngOnInit -> fetchProjects()).
    this.projectsService.mdProjects.pipe(
      skip(1),
      filter(p => p !== null && p !== undefined),
      take(1)
    ).subscribe(projects => {
      if (this.autoStartChecked) return;
      this.autoStartChecked = true;
      const hasNoProjects = !projects || projects.length === 0;
      if (!hasNoProjects) return;
      // Wait a moment for the projects page to be fully rendered so [data-test] anchors exist
      setTimeout(() => {
        if (document.querySelector('[data-test="new-folder-button"]')) {
          this.launch(WELCOME_TOUR.id);
        }
      }, 1000);
    });
  }

  isLessonCompleted(id: string): boolean {
    return localStorage.getItem(COMPLETED_KEY_PREFIX + id) === 'true';
  }

  private markLessonCompleted(id: string): void {
    localStorage.setItem(COMPLETED_KEY_PREFIX + id, 'true');
  }

  // ── Public API ───────────────────────────────────────────────────────────

  /**
   * Start (or restart) a lesson. Aborts any running lesson.
   * Safe to call from anywhere (title-bar button, internal auto-trigger, etc.).
   */
  async launch(lessonId: string): Promise<void> {
    const lesson = LESSONS[lessonId];
    if (!lesson) {
      console.warn('[Mark] Unknown lesson id:', lessonId);
      return;
    }

    // Abort any in-flight run
    this.abortFlag = true;
    await this.sleep(80);
    this.abortFlag = false;

    this.currentLesson = lesson;

    // Reset visual state
    this.currentSpotlightSelector = null;
    this._spotlight.next(null);
    this._text.next('');
    this._staticMode.next(false);
    this._continueArrow.next(false);
    this._isResponding.next(false);

    // 1. Dim
    this._state.next('playing');
    this._dim.next(true);
    await this.sleep(500);
    if (this.abortFlag) return;

    // 2. Slide-in (CSS transition is triggered by state change)
    await this.sleep(700);
    if (this.abortFlag) return;

    // 3. Static prefix
    if (lesson.withStatic !== false) {
      const staticTxt = await this.translateKey('MARK.TOUR.WELCOME.STATIC');
      this._staticMode.next(true);
      this._text.next(staticTxt);
      await this.sleep(500);
      if (this.abortFlag) return;
      this._staticMode.next(false);
    }

    // 4. Iterate steps
    for (const step of lesson.steps) {
      if (this.abortFlag) return;

      // Place spotlight (anchored — recomputeSpotlight() will keep it in sync
      // with the target as the user scrolls or resizes the window)
      this.currentSpotlightSelector = step.targetSelector ?? null;
      if (step.targetSelector) {
        if (!document.querySelector(step.targetSelector)) {
          console.warn('[Mark] Spotlight target not found:', step.targetSelector);
        }
        this.recomputeSpotlight();
      } else {
        this._spotlight.next(null);
      }

      // Typewriter the step text
      const text = await this.translateKey(step.textKey);
      await this.typewriter(text);
      if (this.abortFlag) return;

      // Pause before next step
      await this.sleep(step.durationMs ?? DEFAULT_STEP_DURATION_MS);
    }

    // 5. Done — minimize and remember
    this.currentSpotlightSelector = null;
    this._spotlight.next(null);
    this._dim.next(false);
    this._state.next('minimized');
    this._text.next('');
    this._continueArrow.next(false);
    this.markLessonCompleted(lesson.id);
    if (lesson.onComplete) lesson.onComplete();
  }

  /**
   * Skip the current run. Mark gets minimized and the lesson is recorded as
   * "seen" so it does not auto-restart next time.
   */
  skip(): void {
    this.abortFlag = true;
    this.currentSpotlightSelector = null;
    this._spotlight.next(null);
    this._dim.next(false);
    this._state.next('minimized');
    this._text.next('');
    this._continueArrow.next(false);
    this._isResponding.next(false);
    if (this.currentLesson) {
      this.markLessonCompleted(this.currentLesson.id);
    }
  }

  /** Hide Mark completely (also wired to context-mismatch route changes). */
  hide(): void {
    this.abortFlag = true;
    this.currentSpotlightSelector = null;
    this._state.next('hidden');
    this._dim.next(false);
    this._spotlight.next(null);
    this._text.next('');
    this._continueArrow.next(false);
    this._isResponding.next(false);
    this.currentLesson = null;
  }

  /**
   * Process user-typed input. Walks the registered handler chain — first
   * handler whose canHandle() returns true wins. Result is typewritered
   * back as Mark's reply.
   */
  async submitUserInput(text: string): Promise<void> {
    const trimmed = (text ?? '').trim();
    if (!trimmed) return;
    if (this._isResponding.getValue()) return; // ignore re-entry while responding

    const ctx: MarkInputContext = {
      routeUrl: this.router.url,
      activeLessonId: this.currentLesson?.id ?? null,
    };

    const handler = this.inputHandlers.find(h => h.canHandle(trimmed, ctx));
    if (!handler) return;

    // Abort any running lesson playback so the answer takes the stage
    this.abortFlag = true;
    await this.sleep(60);
    this.abortFlag = false;

    this._isResponding.next(true);
    this.currentSpotlightSelector = null;
    this._spotlight.next(null);
    this._continueArrow.next(false);
    this._staticMode.next(false);

    // Make sure Mark is visible (if minimized → expand; if hidden → playing)
    if (this.currentState !== 'playing') {
      this._state.next('playing');
      this._dim.next(false);
      await this.sleep(300);
    }

    try {
      const reply = await firstValueFrom(handler.handle(trimmed, ctx));
      await this.typewriter(reply);
    } catch (err) {
      console.warn('[Mark] Input handler error:', err);
    } finally {
      this._isResponding.next(false);
    }
  }

  // ── helpers ──────────────────────────────────────────────────────────────

  private async typewriter(text: string, charDelay = 32): Promise<void> {
    this._text.next('');
    this._continueArrow.next(false);
    let buf = '';
    for (const ch of text) {
      if (this.abortFlag) return;
      buf += ch;
      this._text.next(buf);
      await this.sleep(ch === ' ' ? charDelay / 2 : charDelay);
    }
    this._continueArrow.next(true);
  }

  private translateKey(key: string): Promise<string> {
    return firstValueFrom(this.translate.get(key));
  }

  private sleep(ms: number): Promise<void> {
    return new Promise(r => setTimeout(r, ms));
  }
}
