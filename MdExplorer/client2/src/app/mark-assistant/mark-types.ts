/**
 * Shared types for the Mark assistant.
 * Kept separate from the service so lessons can import them without circular deps.
 */

import { Observable } from 'rxjs';

export type MarkState = 'hidden' | 'playing' | 'minimized';

/**
 * Where Mark is "allowed" to be visible.
 * - 'projects-page'  → visible on /projects(/...)
 * - 'main'           → visible on /main/*  (a project is open)
 * - 'always'         → never auto-hidden by route changes
 *
 * When the route leaves the current lesson's context, Mark goes to 'hidden'
 * (this clears even the minimized badge — the user opens a project, Mark
 * disappears entirely until manually re-summoned via the title-bar "?").
 */
export type MarkContext = 'projects-page' | 'main' | 'always';

/**
 * An action button rendered below the dialog text.
 * If a step has `actions`, the lesson loop pauses on that step and waits
 * for the user to click one of them — this is how the idle menu offers
 * "Crea progetto demo" / "Next" choices.
 */
export interface MarkAction {
  /** ngx-translate key for the button label. */
  labelKey: string;
  /** Optional icon (Unicode character or short string) shown before the label. */
  icon?: string;
  /**
   * Handler invoked when the user clicks the button. Common patterns:
   *  - `() => mark.launch('some-other-lesson')`
   *  - `() => mark.skip()`
   *  - any custom logic
   * The current lesson stops as soon as the handler runs (the for-loop's
   * abortFlag is set during launch()), so the handler typically transitions
   * to a different lesson.
   */
  handler: () => void | Promise<void>;
}

export interface MarkStep {
  /** ngx-translate key for the dialog text. */
  textKey: string;
  /** CSS selector for the spotlight target; null/undefined = no spotlight. */
  targetSelector: string | null;
  /** Pause after typewriter completes, before next step. Default 1700ms. */
  durationMs?: number;
  /**
   * Buttons rendered below the dialog text. When set, the step does NOT
   * auto-advance — it waits for the user to pick one of the actions.
   */
  actions?: MarkAction[];
  /**
   * Imperative side-effect to run after the typewriter finishes (and after
   * showing actions if any). Used by the demo-clone-tour to drive the UI:
   * click buttons, fill inputs, wait for a dialog to open, etc. Runs in the
   * lesson's async loop, so awaiting works as expected.
   */
  autoExecute?: () => Promise<void>;
}

/**
 * What the service should do *after* a lesson's last step completes.
 *  - 'next-tip'  → fire-and-forget launch of the next micro-tip (chains them)
 *  - 'minimize'  → shrink Mark to the corner icon (default for terminal lessons)
 *  - 'hide'      → close Mark entirely (state → 'hidden')
 *
 * NOTE: this is NOT triggered by the user's idle-menu choice — that's an
 * action handler that calls launch(...) directly. This decides what to do
 * automatically when a lesson runs to its natural end.
 */
export type MarkOnCompleteNext = 'next-tip' | 'minimize' | 'hide';

export interface MarkLesson {
  id: string;
  steps: MarkStep[];
  /**
   * Where this lesson is allowed to be visible.
   * Default 'always'. Welcome tour uses 'projects-page' so that opening
   * a project hides Mark (he has nothing useful to say there yet).
   */
  context?: MarkContext;
  /** Emit "krsshhh..." static before the first step. Default true. */
  withStatic?: boolean;
  /**
   * Whether the lesson uses Mark's dim+spotlight darkening. Default true.
   * Set to false for "watch-along" lessons (e.g. demo-clone-tour) where Mark
   * pilots the UI by himself and the user just observes — in that case the
   * spotlight cutout would obscure the very dialog Mark is filling out.
   */
  dim?: boolean;
  /** Called after the last step finishes (not on skip). */
  onComplete?: () => void;
  /**
   * If false, completing the lesson does NOT mark it as "seen" in localStorage.
   * Default true. Set to false for menus / interactive flows that the user
   * can re-trigger many times (idle-menu, demo-clone-tour, micro-tips).
   * The welcome-tour leaves it true so it doesn't auto-replay.
   */
  markAsCompleted?: boolean;
  /**
   * What happens after the last step. Default 'minimize'. The welcome-tour
   * and the micro-tips set 'next-tip' to chain into the next pill, so the
   * user reading "Prosegui" gets a continuous stream of suggestions
   * instead of bouncing back to the action menu.
   */
  onCompleteNext?: MarkOnCompleteNext;
}

/** Snapshot of a target element's position, used for the spotlight rect. */
export interface SpotlightRect {
  top: number;
  left: number;
  width: number;
  height: number;
  /** Right edge in viewport coords — used to position the pointer arrow. */
  right: number;
}

// ─────────────────────────────────────────────────────────────────────────
//  USER INPUT HANDLER PROTOCOL
//  Mark's dialog has an input textbox below the speech area where the user
//  can write back. Submitted text goes through a chain of registered
//  handlers — the first one that returns true from canHandle() processes
//  the input. This keeps V0 simple (one EchoHandler fallback) while making
//  it trivial to plug in V3 (AI conversation handler) or V2 (lesson search,
//  command palette) without touching the service or the component.
// ─────────────────────────────────────────────────────────────────────────

export interface MarkInputContext {
  /** Current viewport route (router.url at the moment of submit). */
  routeUrl: string;
  /** The lesson currently active, if any. */
  activeLessonId: string | null;
}

export interface MarkInputHandler {
  /** Stable identifier — useful for logging and for replacing handlers later. */
  readonly id: string;
  /** Higher-priority handlers are consulted first. Default fallback = 0. */
  readonly priority: number;
  /** True if this handler wants to process the input. */
  canHandle(input: string, ctx: MarkInputContext): boolean;
  /**
   * Process the input and stream back a single response (the typewriter
   * will render it as Mark's reply). Returning Observable allows V3 AI
   * handlers to integrate with streaming endpoints.
   */
  handle(input: string, ctx: MarkInputContext): Observable<string>;
}
