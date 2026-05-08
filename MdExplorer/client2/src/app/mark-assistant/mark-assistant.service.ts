import { Injectable, Injector } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject, Observable, Subscription, combineLatest, firstValueFrom } from 'rxjs';
import { filter, skip, take } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { ProjectsService } from '../md-explorer/services/projects.service';
import {
  MarkAction,
  MarkContext,
  MarkInputContext,
  MarkInputHandler,
  MarkLesson,
  MarkState,
  MarkStep,
  SpotlightRect,
} from './mark-types';
import { buildLessonRegistry, MICRO_TIPS, MICRO_TIPS_DONE, WELCOME_TOUR } from './lessons';
import { INPUT_HANDLER_TYPES } from './handlers';

const COMPLETED_KEY_PREFIX = 'mark.lesson.';
const DEFAULT_STEP_DURATION_MS = 1700;
const NEXT_TIP_INDEX_KEY = 'mark.nextTipIndex';
/**
 * Independent flag for the welcome-tour autostart guard. Kept SEPARATE
 * from the markLessonCompleted flags because the welcome-tour now lives
 * in the micro-tip rotation (so it must remain re-surfaceable), but it
 * still must auto-play exactly once on the very-first MDE launch.
 */
const WELCOME_AUTO_SHOWN_KEY = 'mark.welcomeAutoShown';

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
   * Action buttons for the current step (when set, the lesson loop is paused
   * waiting for the user to click one). The component renders these below
   * the dialog text.
   */
  private readonly _actions = new BehaviorSubject<MarkAction[] | null>(null);
  readonly actions$: Observable<MarkAction[] | null> = this._actions.asObservable();

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
  /** Resolver for the in-flight "wait for the user to click an action" promise. */
  private actionResolve: ((picked: MarkAction | null) => void) | null = null;

  /** Lesson registry built once at construction (includes dynamic ones like idle-menu). */
  private readonly lessonRegistry: { [id: string]: MarkLesson };

  constructor(
    private translate: TranslateService,
    private projectsService: ProjectsService,
    private router: Router,
    private injector: Injector,
  ) {
    this.lessonRegistry = buildLessonRegistry({
      launch: (id: string) => this.launch(id),
      launchNextMicroTip: () => this.launchNextMicroTip(),
    });
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

    // Forward every subsequent state change to the mark-window. We watch all
    // observables that drive a visible piece of the dialog UI (text, static
    // mode, responding flag, action buttons, continue arrow, state) so the
    // floating window stays in lock-step with what would be on screen if Mark
    // were still docked. Without actions$/continueArrow$/state$ in this list
    // an undock during an active "Prosegui" step would strand the user — the
    // dialog text would be there but the button to advance wouldn't.
    const sub = combineLatest([
      this.text$,
      this.staticMode$,
      this.isResponding$,
      this.actions$,
      this.continueArrow$,
      this.state$,
    ]).subscribe(() => {
      api.pushState(this.snapshotState());
    });
    this.undockSubs.push(sub);

    // Route user input typed in the mark-window back into the dispatch chain
    this.undockCleanups.push(api.onUserInput((text: string) => {
      this.submitUserInput(text);
    }));

    // Action button clicks in the mark-window come back here so the lesson
    // loop's waitForAction() resolves and the handler runs.
    if (typeof api.onActionSubmit === 'function') {
      this.undockCleanups.push(api.onActionSubmit((index: number) => {
        this.submitAction(index);
      }));
    }

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

  /** Serialize the visible state for the standalone mark-window.
   *
   * Action labels are pre-translated here because the mark-window is a static
   * HTML page without ngx-translate — it just renders whatever strings the
   * snapshot carries. The handler stays on this side; the window only
   * reports back the picked index.
   */
  private snapshotState(): {
    text: string;
    staticMode: boolean;
    isResponding: boolean;
    state: MarkState;
    continueArrow: boolean;
    actions: { label: string; icon?: string }[] | null;
    transmittingLabel: string;
    inputPlaceholder: string;
  } {
    const rawActions = this._actions.getValue();
    const actions = rawActions
      ? rawActions.map(a => ({
          label: this.translate.instant(a.labelKey),
          icon: a.icon,
        }))
      : null;
    return {
      text: this._text.getValue(),
      staticMode: this._staticMode.getValue(),
      isResponding: this._isResponding.getValue(),
      state: this._state.getValue(),
      continueArrow: this._continueArrow.getValue(),
      actions,
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
   * the projects page with zero projects. Uses the dedicated
   * WELCOME_AUTO_SHOWN_KEY flag (NOT the per-lesson completion key)
   * because welcome-tour is re-runnable from the micro-tip rotation.
   */
  private checkAutoStart(): void {
    if (localStorage.getItem(WELCOME_AUTO_SHOWN_KEY) === 'true') return;

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
          // Mark the welcome as auto-shown BEFORE launching so it doesn't
          // re-trigger on a second mdProjects emit, and bump nextTipIndex
          // past the welcome itself so the very-first "Prosegui" surfaces
          // the next pill, not a replay of the tour the user just saw.
          localStorage.setItem(WELCOME_AUTO_SHOWN_KEY, 'true');
          if (this.readNextTipIndex() === 0) {
            this.writeNextTipIndex(1);
          }
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
    const lesson = this.lessonRegistry[lessonId];
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
    this._actions.next(null);
    this.actionResolve = null;

    // 1. Dim — opt-out for "watch-along" lessons (e.g. demo-clone-tour) so
    // the spotlight cutout doesn't obscure the dialog Mark is auto-filling.
    const useDim = lesson.dim !== false;
    this._state.next('playing');
    this._dim.next(useDim);
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
      // with the target as the user scrolls or resizes the window).
      // For lessons opted out of dim (watch-along tours) we suppress the
      // spotlight entirely — its huge box-shadow IS the darkening, and on
      // a Material dialog with single-input targets it would also produce
      // tiny off-centre highlights.
      this.currentSpotlightSelector = useDim ? (step.targetSelector ?? null) : null;
      if (useDim && step.targetSelector) {
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

      // 3 cases for advancing past a step:
      //   (a) step.actions defined  → pause, render the buttons, wait for click
      //   (b) step.autoExecute      → run side-effect, then time-based advance
      //   (c) plain narrative step  → synthesize a "Continue" button
      //                                so the user paces the reading himself
      if (step.actions && step.actions.length > 0) {
        this._actions.next(step.actions);
        await this.waitForAction();
        this._actions.next(null);
        if (this.abortFlag) return;
        // Note: action handlers often call launch(otherLessonId) which sets
        // abortFlag → next iteration's check exits the loop. Skip trailing sleep.
        continue;
      }

      if (step.autoExecute) {
        try {
          await step.autoExecute();
        } catch (err) {
          console.warn('[Mark] autoExecute failed for step', step.textKey, err);
        }
        if (this.abortFlag) return;
        await this.sleep(step.durationMs ?? DEFAULT_STEP_DURATION_MS);
        continue;
      }

      // Narrative step (no actions, no autoExecute) — wait for user click on
      // a synthesized "Prosegui" button. No time-based advance: the user
      // reads at his own pace.
      this._actions.next([{ labelKey: 'MARK.PROSEGUI', icon: '▶', handler: () => { /* noop */ } }]);
      await this.waitForAction();
      this._actions.next(null);
      if (this.abortFlag) return;
    }

    // 5. Done — apply onCompleteNext transition.
    // Welcome and micro-tips chain into 'next-tip' (so Prosegui keeps
    // surfacing new pills); demo / done / menu fall back to 'minimize'.
    this.currentSpotlightSelector = null;
    this._spotlight.next(null);
    this._dim.next(false);
    this._text.next('');
    this._continueArrow.next(false);
    this._actions.next(null);
    if (lesson.markAsCompleted !== false) {
      this.markLessonCompleted(lesson.id);
    }
    if (lesson.onComplete) lesson.onComplete();

    const next = lesson.onCompleteNext ?? 'minimize';
    switch (next) {
      case 'next-tip':
        // Fire-and-forget — launchNextMicroTip() will pick the right tip
        // (or the "all done" closing lesson) and run it.
        this.launchNextMicroTip();
        break;
      case 'hide':
        this._state.next('hidden');
        break;
      case 'minimize':
      default:
        this._state.next('minimized');
        break;
    }
  }

  /**
   * Open Mark in idle/menu mode — shows the menu lesson where the user picks
   * what to do next (demo project clone, micro-tip "next", etc.).
   * Wired to the `?` button in the title-bar.
   */
  async openMenu(): Promise<void> {
    return this.launch('idle-menu');
  }

  /**
   * Plays the "next" applicable micro-tip in the queue.
   *
   * The pointer (mark.nextTipIndex) is incremented on each call and wraps
   * when the queue is exhausted (playing the "all done" closing lesson
   * and resetting to 0). Tips whose `context` doesn't match the current
   * route are SKIPPED — e.g. if the user is on /main, welcome and search
   * (both 'projects-page') are silently jumped, and only drag / undock
   * play; once those are exhausted the closing lesson is shown.
   *
   * This means the user gets a context-relevant stream of suggestions
   * without ever seeing a tip that talks about a UI element that's not
   * on screen.
   */
  async launchNextMicroTip(): Promise<void> {
    let idx = this.readNextTipIndex();
    const url = this.router.url;

    // Skip tips whose context doesn't match the current route
    while (idx < MICRO_TIPS.length) {
      const tip = MICRO_TIPS[idx];
      const ctx = tip.context ?? 'always';
      if (this.isContextActive(ctx, url)) break;
      idx++;
    }

    if (idx >= MICRO_TIPS.length) {
      // No more applicable tips → "all done" message + reset cursor
      this.writeNextTipIndex(0);
      return this.launch(MICRO_TIPS_DONE.id);
    }

    const tip = MICRO_TIPS[idx];
    this.writeNextTipIndex(idx + 1);
    return this.launch(tip.id);
  }

  private readNextTipIndex(): number {
    const raw = localStorage.getItem(NEXT_TIP_INDEX_KEY);
    const n = raw ? parseInt(raw, 10) : 0;
    return isNaN(n) || n < 0 ? 0 : n;
  }

  private writeNextTipIndex(n: number): void {
    localStorage.setItem(NEXT_TIP_INDEX_KEY, String(n));
  }

  /**
   * Skip the current run. Mark gets minimized and the lesson is recorded as
   * "seen" only if the lesson opted into completion tracking. Skipping the
   * idle-menu, the demo-clone-tour or a micro-tip should not lock those
   * out — only the welcome-tour gets that treatment.
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
    this._actions.next(null);
    this.resolveAction(null);
    if (this.currentLesson && this.currentLesson.markAsCompleted !== false) {
      this.markLessonCompleted(this.currentLesson.id);
    }
  }

  /**
   * Called by the component when the user clicks one of the action buttons.
   * The lesson loop awaits inside waitForAction() — resolving here lets it
   * proceed (or the action handler can transition to a different lesson).
   */
  async submitAction(index: number): Promise<void> {
    const actions = this._actions.getValue();
    if (!actions || index < 0 || index >= actions.length) return;
    const picked = actions[index];
    this._actions.next(null);
    this.resolveAction(picked);
    try {
      await picked.handler();
    } catch (err) {
      console.warn('[Mark] action handler failed:', err);
    }
  }

  private waitForAction(): Promise<MarkAction | null> {
    return new Promise<MarkAction | null>(resolve => {
      this.actionResolve = resolve;
    });
  }

  private resolveAction(value: MarkAction | null): void {
    if (this.actionResolve) {
      const r = this.actionResolve;
      this.actionResolve = null;
      r(value);
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
    this._actions.next(null);
    this.resolveAction(null);
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
    this._actions.next(null);
    this.resolveAction(null);

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
