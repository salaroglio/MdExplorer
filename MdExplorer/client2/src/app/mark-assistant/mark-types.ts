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

export interface MarkStep {
  /** ngx-translate key for the dialog text. */
  textKey: string;
  /** CSS selector for the spotlight target; null/undefined = no spotlight. */
  targetSelector: string | null;
  /** Pause after typewriter completes, before next step. Default 1700ms. */
  durationMs?: number;
}

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
  /** Called after the last step finishes (not on skip). */
  onComplete?: () => void;
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
