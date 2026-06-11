/**
 * Input handler registry — what Mark does when the user writes back.
 *
 * ─── How to add a new handler ──────────────────────────────────────────
 *  1. Create `handlers/your-handler.ts` implementing `MarkInputHandler`.
 *     Keep it `@Injectable({ providedIn: 'root' })` so DI gives a singleton.
 *  2. Add the class to the list below.
 *  3. (Optional) Use `priority` to control ordering — higher runs first.
 *     The default EchoHandler has priority 0 and always returns true,
 *     so make sure your handler has priority > 0 if it should win.
 *
 * MarkAssistantService registers all listed handlers at construction.
 */

import { Type } from '@angular/core';
import { MarkInputHandler } from '../mark-types';
import { EchoHandler } from './echo-handler';

/**
 * Ordered list of handler classes. The service injects each via DI
 * and sorts them by priority (descending) at registration time.
 */
export const INPUT_HANDLER_TYPES: Array<Type<MarkInputHandler>> = [
  EchoHandler,
  // ← future handlers go here, e.g.:
  //   AiConversationHandler,   // priority 100, talks to backend AI provider
  //   LessonSearchHandler,     // priority 50, e.g. "/tour clone" runs the clone-tour lesson
  //   CommandPaletteHandler,   // priority 80, "/settings" → opens settings dialog
];

export { EchoHandler };
