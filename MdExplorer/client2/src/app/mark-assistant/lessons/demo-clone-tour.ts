import { MarkLesson } from '../mark-types';
import {
  buildDemoClonePath,
  clickElement,
  setInputValue,
  sleep,
  waitForElement,
  waitForElementGone,
  waitForRoute,
} from './auto-utils';

/**
 * Demo project clone tour — Mark talks while MDE drives itself: opens
 * the clone dialog, fills the URL, sets the destination path, presses
 * Clone, and waits for the project to be loaded. The user just watches.
 *
 * Why this works without a GitHub account:
 *   public Git repos are clonable over HTTPS anonymously. Our default
 *   demo URL (DEMO_REPO_URL below) points to a small public repository
 *   so the demo always works on a fresh install.
 *
 * Implementation notes:
 *   - autoExecute drives the UI via direct DOM manipulation (see
 *     lessons/auto-utils.ts). For ngModel inputs we set value via the
 *     native setter and dispatch 'input'/'change' events so Angular's
 *     change detection picks up the new value.
 *   - The localPath input is `readonly` (the clone dialog normally fills
 *     it via a folder picker). We bypass readonly temporarily.
 *   - We wait for navigation to /main/* as the "clone success" signal —
 *     ModernCloneProjectComponent navigates there once the clone backend
 *     completes.
 */

/**
 * Default demo repository — purpose-built for MDE onboarding (multiple
 * .md files, cross-references, plantuml, runnable code blocks, ...).
 * Owned by the MDE author so its content can evolve with the product.
 *
 * To change the demo target, edit just this constant.
 */
const DEMO_REPO_URL = 'https://github.com/salaroglio/mdexplorer-demo.git';

/**
 * Selector helpers — all relative to the open Modern Clone dialog.
 * The dialog uses .modern-clone-dialog as the root class.
 */
const ROOT = '.modern-clone-dialog';
const URL_INPUT = `${ROOT} mat-form-field:nth-of-type(1) input[matInput]`;     // first form field = repo URL
const PATH_INPUT = `${ROOT} input[readonly]`;                                  // localPath is the only readonly one
const CLONE_BTN = `${ROOT} mat-dialog-actions button[color="primary"]`;        // primary action button

export const DEMO_CLONE_TOUR: MarkLesson = {
  id: 'demo-clone-tour',
  // 'always' on purpose: the auto-clone step navigates the app to /main/...
  // mid-lesson. With context='projects-page' the route guard would hide
  // Mark just before the closing "Eccoci! Il progetto è aperto." step.
  context: 'always',
  withStatic: true,
  markAsCompleted: false,    // re-runnable
  // Watch-along tour: Mark pilots the clone dialog himself, the user just
  // observes. We suppress dim + spotlight so the dialog stays fully readable
  // while it's being auto-filled.
  dim: false,
  steps: [
    // Step 1 — narrative intro
    {
      textKey: 'MARK.TOUR.DEMO_CLONE.INTRO',
      targetSelector: null,
      durationMs: 2200,
    },

    // Step 2 — open the Clone dialog by clicking the card
    {
      textKey: 'MARK.TOUR.DEMO_CLONE.OPEN_DIALOG',
      targetSelector: '[data-test="clone-button"]',
      durationMs: 0,
      autoExecute: async () => {
        await sleep(900);           // let the user read the message
        await clickElement('[data-test="clone-button"]');
        // wait for the dialog to render
        await waitForElement(ROOT);
      },
    },

    // Step 3 — fill the URL
    {
      textKey: 'MARK.TOUR.DEMO_CLONE.FILL_URL',
      targetSelector: URL_INPUT,
      durationMs: 0,
      autoExecute: async () => {
        await sleep(800);
        await setInputValue(URL_INPUT, DEMO_REPO_URL);
        // ngModelChange triggers detectProviderFromUrl → "GitHub" badge appears
        await sleep(800);
      },
    },

    // Step 4 — set the destination path
    {
      textKey: 'MARK.TOUR.DEMO_CLONE.SET_PATH',
      targetSelector: PATH_INPUT,
      durationMs: 0,
      autoExecute: async () => {
        await sleep(600);
        const target = buildDemoClonePath('mdexplorer-demo');
        await setInputValue(PATH_INPUT, target, { bypassReadonly: true });
        await sleep(800);
      },
    },

    // Step 5 — press Clone
    {
      textKey: 'MARK.TOUR.DEMO_CLONE.PRESS_CLONE',
      targetSelector: CLONE_BTN,
      durationMs: 0,
      autoExecute: async () => {
        await sleep(800);
        // The button might be disabled briefly while ngModel propagates —
        // poll until it's enabled, then click. Before clicking we scroll
        // it into view: the clone dialog can be taller than the viewport
        // on smaller screens, leaving the primary action below the fold.
        const start = Date.now();
        while (Date.now() - start < 3000) {
          const btn = document.querySelector(CLONE_BTN) as HTMLButtonElement | null;
          if (btn && !btn.disabled) {
            try {
              btn.scrollIntoView({ behavior: 'smooth', block: 'end', inline: 'nearest' });
              await sleep(250);
            } catch { /* older engines: silently skip */ }
            btn.click();
            break;
          }
          await sleep(120);
        }
        // Wait either for the dialog to close (clone success path) or for
        // the route to switch to /main/* (project opened).
        await Promise.race([
          waitForElementGone(ROOT, 30000),
          waitForRoute('/main', 30000),
        ]).catch(() => { /* timeout — we still let the closing step run */ });
      },
    },

    // Step 6 — closing
    {
      textKey: 'MARK.TOUR.DEMO_CLONE.DONE',
      targetSelector: null,
      durationMs: 2500,
    },
  ],
};
