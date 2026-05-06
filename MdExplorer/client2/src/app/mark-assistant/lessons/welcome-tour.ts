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
