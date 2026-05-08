/**
 * Lesson registry — central index of all lessons Mark can play.
 *
 * ─── How to add a new lesson ───────────────────────────────────────────
 *  1. Create `lessons/your-lesson.ts` exporting a `MarkLesson` constant.
 *  2. Add an entry below in STATIC_LESSONS keyed by lesson.id (or, if the
 *     lesson needs callbacks into the service, expose a buildXxx() factory
 *     and wire it in `buildLessonRegistry()` below).
 *  3. Add the matching i18n keys under MARK.TOUR.* in it.json and en.json.
 *  4. (Optional) Add a trigger somewhere that calls
 *     markAssistant.launch('your-lesson-id') when appropriate.
 *
 * Lessons can declare a `context` ('projects-page' | 'main' | 'always')
 * to control where Mark is allowed to appear. Outside that context,
 * Mark goes to 'hidden' on navigation.
 *
 * Some lessons are "dynamic" — their steps reference the service itself
 * (e.g. action handlers calling launch). They can't be plain constants
 * without circular imports, so they're built via factory functions that
 * receive the necessary callbacks as parameters.
 */

import { MarkLesson } from '../mark-types';
import { WELCOME_TOUR } from './welcome-tour';
import { buildIdleMenu } from './idle-menu';
import { DEMO_CLONE_TOUR } from './demo-clone-tour';
import { MICRO_TIPS, MICRO_TIPS_DONE } from './micro-tips';

/** Pure / static lessons — safe to import as constants. */
const STATIC_LESSONS: { [id: string]: MarkLesson } = (() => {
  const out: { [id: string]: MarkLesson } = {
    [WELCOME_TOUR.id]: WELCOME_TOUR,
    [DEMO_CLONE_TOUR.id]: DEMO_CLONE_TOUR,
    [MICRO_TIPS_DONE.id]: MICRO_TIPS_DONE,
  };
  for (const tip of MICRO_TIPS) {
    out[tip.id] = tip;
  }
  return out;
})();

/**
 * Builds the full lesson registry, wiring dynamic lessons (those that need
 * to call back into the service) via callbacks. Called once by the service
 * in its constructor.
 */
export function buildLessonRegistry(deps: {
  launch: (id: string) => void | Promise<void>;
  launchNextMicroTip: () => void | Promise<void>;
}): { [id: string]: MarkLesson } {
  const idleMenu = buildIdleMenu(deps);
  return {
    ...STATIC_LESSONS,
    [idleMenu.id]: idleMenu,
  };
}

export { WELCOME_TOUR, MICRO_TIPS, MICRO_TIPS_DONE };
