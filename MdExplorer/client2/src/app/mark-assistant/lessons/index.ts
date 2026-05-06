/**
 * Lesson registry — central index of all lessons Mark can play.
 *
 * ─── How to add a new lesson ───────────────────────────────────────────
 *  1. Create `lessons/your-lesson.ts` exporting a `MarkLesson` constant.
 *  2. Add an entry below in LESSONS keyed by lesson.id.
 *  3. Add the matching i18n keys under MARK.TOUR.* in it.json and en.json.
 *  4. (Optional) Add a trigger somewhere that calls
 *     markAssistant.launch('your-lesson-id') when appropriate.
 *
 * Lessons can declare a `context` ('projects-page' | 'main' | 'always')
 * to control where Mark is allowed to appear. Outside that context,
 * Mark goes to 'hidden' on navigation.
 */

import { MarkLesson } from '../mark-types';
import { WELCOME_TOUR } from './welcome-tour';

export const LESSONS: { readonly [id: string]: MarkLesson } = {
  [WELCOME_TOUR.id]: WELCOME_TOUR,
  // ← future lessons go here
  //   [PROJECT_OPENED_TOUR.id]: PROJECT_OPENED_TOUR,
  //   [FIRST_FILE_SAVED.id]:    FIRST_FILE_SAVED,
};

export { WELCOME_TOUR };
