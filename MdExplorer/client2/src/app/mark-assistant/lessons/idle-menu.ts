import { MarkLesson } from '../mark-types';

/**
 * Idle menu lesson — shown when the user clicks the "?" button in the
 * Angular title-bar. Replaces the previous behaviour (which auto-replayed
 * the welcome-tour every time): now the welcome-tour is reserved for the
 * very first launch only, and the manual button surfaces a quiet,
 * action-oriented menu.
 *
 * The lesson has a single step that pauses on action buttons — it never
 * auto-completes, only progresses when the user clicks one of the choices.
 *
 * Built via factory because the action handlers need to call methods on
 * MarkAssistantService (launch / launchNextMicroTip) without a circular
 * import — the service passes the callbacks explicitly at construction.
 */
export function buildIdleMenu(deps: {
  launch: (id: string) => void | Promise<void>;
  launchNextMicroTip: () => void | Promise<void>;
}): MarkLesson {
  return {
    id: 'idle-menu',
    context: 'always',
    withStatic: false,    // no "krsshhh" — the user invoked Mark on purpose
    markAsCompleted: false,
    steps: [
      {
        textKey: 'MARK.MENU.PROMPT',
        targetSelector: null,
        actions: [
          {
            labelKey: 'MARK.MENU.DEMO_CLONE',
            icon: '🪐',
            handler: () => deps.launch('demo-clone-tour'),
          },
          {
            // Same label as the in-lesson "Continue" button: pressing
            // either advances the micro-tip stream. The two are the same
            // semantic action ("show me the next suggestion").
            labelKey: 'MARK.PROSEGUI',
            icon: '▶',
            handler: () => deps.launchNextMicroTip(),
          },
        ],
      },
    ],
  };
}
