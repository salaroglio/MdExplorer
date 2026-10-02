import { MarkAction, MarkLesson } from '../mark-types';
import {
  AgenticEnvironmentStatus,
  buildDemoClonePath,
  clickElement,
  probeAgenticEnvironments,
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
 *
 * The demo opens already configured for the agentic environment of THIS
 * computer (sprint 2026-10-02-Demo-Ambiente-Agentico-Rilevato):
 *   - before cloning, Mark asks the service to probe Copilot CLI, Claude Code
 *     and opencode;
 *   - one usable → that one; more than one → Mark asks the user; none → the
 *     demo opens with no environment and Mark says why. Never a silent
 *     default to Copilot;
 *   - the choice is handed to ProjectsService bound to the clone path, so the
 *     open request made by the clone dialog carries it. The service writes it
 *     in the project's .development.yml, as it does when a project is created.
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
 * The demo exists in one version per language, each on its own branch of the same repository.
 * Mark downloads the one of the language MdExplorer is set to. To add a language: translate the
 * demo on a new branch and add a line here.
 */
const DEMO_BRANCH_BY_LANGUAGE: { [language: string]: string } = {
  it: 'main',
  en: 'en',
};
/** MdExplorer's own default language is English (LanguageService): the demo follows the same rule. */
const DEMO_BRANCH_DEFAULT = 'en';
const demoBranchFor = (language: string): string => DEMO_BRANCH_BY_LANGUAGE[language] || DEMO_BRANCH_DEFAULT;

/**
 * Selector helpers — all relative to the open Modern Clone dialog.
 * The dialog uses .modern-clone-dialog as the root class.
 */
const ROOT = '.modern-clone-dialog';
const URL_INPUT = `${ROOT} mat-form-field:nth-of-type(1) input[matInput]`;     // first form field = repo URL
const PATH_INPUT = `${ROOT} input[readonly]`;                                  // localPath is the only readonly one
const BRANCH_INPUT = `${ROOT} input[data-test="clone-branch-input"]`;          // optional branch
const CLONE_BTN = `${ROOT} mat-dialog-actions button[color="primary"]`;        // primary action button

/** Product names: the same in every language. */
const ENVIRONMENT_NAMES: { [id: string]: string } = {
  copilot: 'GitHub Copilot',
  claude: 'Claude Code',
  opencode: 'opencode',
};
const nameOf = (id: string): string => ENVIRONMENT_NAMES[id] || id;

/** Harness id for "no agentic environment": an explicit choice, written in the project. */
const NO_ENVIRONMENT = 'none';

/** What the probe step found, read by the steps that follow it. Reset at every run. */
interface DemoTourState {
  environments: AgenticEnvironmentStatus[];
  usable: string[];
  /** The probe itself failed (service unreachable, unexpected answer): not the same as "none found". */
  probeError: string | null;
  /** Harness id the demo will be opened with. */
  chosen: string;
}

const emptyState = (): DemoTourState => ({ environments: [], usable: [], probeError: null, chosen: NO_ENVIRONMENT });

/**
 * Built via factory because the tour hands the chosen environment to ProjectsService, and a
 * lesson cannot import the service without a circular import — the callback is passed in.
 */
export function buildDemoCloneTour(deps: {
  setHarnessForPath: (path: string, harness: string) => void;
  /** Language MdExplorer is set to ('it', 'en'): it decides which branch of the demo is downloaded. */
  currentLanguage: () => string;
}): MarkLesson {
  let state = emptyState();

  /** Installed but not ready: Mark names them, so the user knows what to fix. */
  const notReady = (): AgenticEnvironmentStatus[] => state.environments.filter(e => e.installed && !e.usable);

  return {
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
        // The tour is re-runnable: what the previous run found must not leak into this one.
        resolve: () => { state = emptyState(); return {}; },
      },

      // Step 2 — probe the agentic environments of this computer
      {
        textKey: 'MARK.TOUR.DEMO_CLONE.PROBE',
        targetSelector: null,
        durationMs: 400,
        autoExecute: async () => {
          try {
            const probe = await probeAgenticEnvironments();
            state.environments = probe.environments;
            state.usable = probe.usable;
          } catch (err) {
            state.probeError = (err as Error)?.message || String(err);
          }
        },
      },

      // Step 3a — exactly one usable environment: say which, and use it
      {
        textKey: 'MARK.TOUR.DEMO_CLONE.FOUND_ONE',
        targetSelector: null,
        durationMs: 2600,
        autoExecute: async () => { /* timed step: the text is the whole point */ },
        resolve: () => {
          if (state.probeError || state.usable.length !== 1) return null;
          state.chosen = state.usable[0];
          return { textParams: { name: nameOf(state.chosen) } };
        },
      },

      // Step 3b — more than one: the user chooses, there is no built-in preference
      {
        textKey: 'MARK.TOUR.DEMO_CLONE.FOUND_MANY',
        targetSelector: null,
        resolve: () => {
          if (state.probeError || state.usable.length < 2) return null;
          const actions: MarkAction[] = state.usable.map(id => ({
            labelKey: 'MARK.ENVIRONMENT.' + id.toUpperCase(),
            icon: '🤖',
            handler: () => { state.chosen = id; },
          }));
          return { actions };
        },
      },

      // Step 3c — none usable: the demo opens with no environment, and Mark says why
      {
        textKey: 'MARK.TOUR.DEMO_CLONE.FOUND_NONE',
        targetSelector: null,
        resolve: () => {
          if (state.probeError || state.usable.length > 0) return null;
          state.chosen = NO_ENVIRONMENT;
          const pending = notReady();
          if (pending.length === 0) return {};
          if (pending.length > 1) {
            return {
              textKey: 'MARK.TOUR.DEMO_CLONE.NOT_READY.SEVERAL',
              textParams: { names: pending.map(e => nameOf(e.id)).join(', ') },
            };
          }
          const one = pending[0];
          const known = ['not-logged-in', 'no-models', 'timeout'];
          const reason = known.indexOf(one.reason || '') >= 0 ? (one.reason as string) : 'error';
          return {
            textKey: 'MARK.TOUR.DEMO_CLONE.NOT_READY.' + reason.toUpperCase().replace(/-/g, '_'),
            textParams: { name: nameOf(one.id), detail: one.detail || '' },
          };
        },
      },

      // Step 3d — the probe itself failed: said out loud, not passed off as "none found"
      {
        textKey: 'MARK.TOUR.DEMO_CLONE.PROBE_FAILED',
        targetSelector: null,
        resolve: () => {
          if (!state.probeError) return null;
          state.chosen = NO_ENVIRONMENT;
          return { textParams: { error: state.probeError } };
        },
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

      // Step 3b — choose the branch: the demo in the language MdExplorer is set to
      {
        textKey: 'MARK.TOUR.DEMO_CLONE.SET_BRANCH',
        targetSelector: BRANCH_INPUT,
        durationMs: 0,
        resolve: () => ({ textParams: { branch: demoBranchFor(deps.currentLanguage()) } }),
        autoExecute: async () => {
          await sleep(600);
          await setInputValue(BRANCH_INPUT, demoBranchFor(deps.currentLanguage()));
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
          const target = await buildDemoClonePath('mdexplorer-demo');
          await setInputValue(PATH_INPUT, target, { bypassReadonly: true });
          // The clone dialog opens the project by path and knows nothing about environments:
          // the choice travels bound to this path, and only to it.
          deps.setHarnessForPath(target, state.chosen);
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

      // Closing — says what the demo was configured for
      {
        textKey: 'MARK.TOUR.DEMO_CLONE.DONE',
        targetSelector: null,
        durationMs: 2500,
        resolve: () => state.chosen === NO_ENVIRONMENT
          ? { textKey: 'MARK.TOUR.DEMO_CLONE.DONE_NO_ENVIRONMENT' }
          : { textParams: { name: nameOf(state.chosen) } },
      },
    ],
  };
}
