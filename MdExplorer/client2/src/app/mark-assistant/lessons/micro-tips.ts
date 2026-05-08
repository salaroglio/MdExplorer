import { MarkLesson } from '../mark-types';
import { WELCOME_TOUR } from './welcome-tour';

/**
 * Micropillole — short single-step lessons surfaced one at a time when
 * the user clicks "Prosegui" in the idle menu (or in any narrative step).
 * The service tracks an index in localStorage (`mark.nextTipIndex`) and
 * cycles through them; once exhausted, MICRO_TIPS_DONE is shown and the
 * cursor resets to 0.
 *
 * Adding a tip = creating an entry below + adding the i18n key under
 * MARK.TIPS.* in it.json/en.json. No code change required.
 *
 * All tips have `markAsCompleted: false` so each click on "Prosegui" can
 * surface them in order without permanently locking any of them.
 *
 * Note on WELCOME_TOUR at index 0: it's documentation about the 3 cards
 * on the projects page, equally worth re-surfacing in the rotation. The
 * autostart code in MarkAssistantService bumps `nextTipIndex` past it
 * after the very-first auto-show so the user doesn't see it twice in a
 * row at boot.
 */
export const MICRO_TIPS: MarkLesson[] = [
  // 1. Welcome (3 cards explanation) — only on /projects
  WELCOME_TOUR,

  // 2. Search field — only on /projects (the field exists only there)
  {
    id: 'micro-tip-search',
    context: 'projects-page',
    withStatic: false,
    markAsCompleted: false,
    onCompleteNext: 'next-tip',
    steps: [
      {
        textKey: 'MARK.TIPS.SEARCH',
        targetSelector: 'mat-form-field.search-field',
        durationMs: 2400,
      },
    ],
  },

  // 3. Drag the panel — works in any context (Mark is everywhere)
  {
    id: 'micro-tip-drag',
    context: 'always',
    withStatic: false,
    markAsCompleted: false,
    onCompleteNext: 'next-tip',
    steps: [
      {
        textKey: 'MARK.TIPS.DRAG',
        targetSelector: null,
        durationMs: 2400,
      },
    ],
  },

  // 4. Undock to a separate window — works in any context.
  // Self-spotlight on Mark's own wrap (gold halo around the panel) since
  // the actual undock button (⤴) lives INSIDE the wrap and the spotlight
  // z-index (9001) sits below the wrap (9100) — illuminating the button
  // directly would require a wider z-index refactor we'll do if/when needed.
  {
    id: 'micro-tip-undock',
    context: 'always',
    withStatic: false,
    markAsCompleted: false,
    onCompleteNext: 'next-tip',
    steps: [
      {
        textKey: 'MARK.TIPS.UNDOCK',
        targetSelector: '.mark-dialog-wrap',
        durationMs: 2400,
      },
    ],
  },

  // 5. (Hidden for V1) AI commit — feature still flaky, plus only relevant
  // when a project is open. Re-enable when stable.
  // {
  //   id: 'micro-tip-ai-commit',
  //   context: 'main',
  //   withStatic: false,
  //   markAsCompleted: false,
  //   onCompleteNext: 'next-tip',
  //   steps: [
  //     {
  //       textKey: 'MARK.TIPS.AI_COMMIT',
  //       targetSelector: null,
  //       durationMs: 2400,
  //     },
  //   ],
  // },
];

/**
 * "Nothing more for now" lesson — shown after all micro-tips are exhausted,
 * cursor wraps to 0 right after.
 */
export const MICRO_TIPS_DONE: MarkLesson = {
  id: 'micro-tips-done',
  context: 'always',
  withStatic: false,
  markAsCompleted: false,
  steps: [
    {
      textKey: 'MARK.TIPS.DONE',
      targetSelector: null,
      durationMs: 2400,
    },
  ],
};
