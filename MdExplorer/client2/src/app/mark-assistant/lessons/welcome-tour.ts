import { MarkLesson } from '../mark-types';

/**
 * The first-ever lesson: the 5-second wow tour of the projects page.
 * Triggered automatically when the user opens MDE for the very first time
 * (zero projects + no completion flag in localStorage), and always available
 * on demand from the title-bar "?" button.
 */
export const WELCOME_TOUR: MarkLesson = {
  id: 'welcome-tour',
  context: 'projects-page',
  withStatic: true,
  // markAsCompleted=false so this lesson can re-appear in the micro-tip
  // rotation (the spotlight on the 3 cards is documentation worth seeing
  // again after a while). The "auto-show on first launch" guard uses a
  // SEPARATE flag (mark.welcomeAutoShown) to avoid replaying it on boot.
  markAsCompleted: false,
  // After the user finishes the very-first welcome, hand off to the micro-tip
  // chain — pressing "Prosegui" on the closing step rolls into pill N of
  // the suggestions queue (the autostart code bumps nextTipIndex past the
  // welcome itself so it doesn't replay immediately).
  onCompleteNext: 'next-tip',
  steps: [
    {
      textKey: 'MARK.TOUR.WELCOME.INTRO',
      targetSelector: null,
      durationMs: 1400,
    },
    {
      textKey: 'MARK.TOUR.WELCOME.CREATE',
      targetSelector: '[data-test="new-folder-button"]',
      durationMs: 1800,
    },
    {
      textKey: 'MARK.TOUR.WELCOME.CLONE',
      targetSelector: '[data-test="clone-button"]',
      durationMs: 1700,
    },
    {
      textKey: 'MARK.TOUR.WELCOME.SETTINGS',
      targetSelector: '[data-test="settings-button"]',
      durationMs: 1700,
    },
    {
      textKey: 'MARK.TOUR.WELCOME.CLOSING',
      targetSelector: null,
      durationMs: 2000,
    },
  ],
  onComplete: () => {
    localStorage.setItem('mark.welcomeTour.completed', 'true');
  },
};
