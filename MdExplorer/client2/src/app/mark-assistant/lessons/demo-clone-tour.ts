import { MarkAction, MarkLesson } from '../mark-types';
import {
  AgenticEnvironmentStatus,
  buildDemoClonePath,
  probeAgenticEnvironments,
  sleep,
  waitForRoute,
} from './auto-utils';

/**
 * Demo project tour — Mark talks while MDE creates the demo project from its template
 * (docs-internal/Sprints/2026-10-17-Template-Di-Progetto.md, P1: the demo is a template like the others) and opens it.
 * The user just watches.
 *
 * Why this works without a GitHub account:
 *   the demo template is a public repository, downloaded over HTTPS anonymously, so the demo always works on a
 *   fresh install. The project gets a local origin next to its folder: the agents deliver there, and the template
 *   stays the source of the updates («Scarica gli aggiornamenti»).
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
 * The demo template — purpose-built for MDE onboarding (multiple .md files, cross-references, plantuml, the agent
 * city, ...). Owned by the MDE author so its content can evolve with the product. Until the list of templates (P4)
 * exists, its address lives here.
 */
const DEMO_TEMPLATE_URL = 'https://github.com/salaroglio/mdexplorer-demo.git';

/**
 * The languages the demo template has; its manifest says which branch each one is. Mark takes the language
 * MdExplorer is set to. To add a language: translate the demo on a new branch, name it in the manifest, add it here.
 */
const DEMO_LANGUAGES = ['it', 'en'];
/** MdExplorer's own default language is English (LanguageService): the demo follows the same rule. */
const DEMO_LANGUAGE_DEFAULT = 'en';
const demoLanguageFor = (language: string): string => DEMO_LANGUAGES.indexOf(language) >= 0 ? language : DEMO_LANGUAGE_DEFAULT;

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
  /** Why the project could not be created, for the person; null = created. */
  createError: string | null;
}

const emptyState = (): DemoTourState => ({ environments: [], usable: [], probeError: null, chosen: NO_ENVIRONMENT, createError: null });

/**
 * Built via factory because the tour hands the chosen environment to ProjectsService, and a
 * lesson cannot import the service without a circular import — the callback is passed in.
 */
export function buildDemoCloneTour(deps: {
  setHarnessForPath: (path: string, harness: string) => void;
  /** Creates the project from the template and opens it; rejects with the reason, for the person. */
  createFromTemplateAndOpen: (path: string, templateUrl: string, language: string) => Promise<void>;
  /** Language MdExplorer is set to ('it', 'en'): it decides which language of the demo is taken. */
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

      // Create the project from the demo template, then open it
      {
        textKey: 'MARK.TOUR.DEMO_CLONE.CREATE',
        targetSelector: null,
        durationMs: 0,
        autoExecute: async () => {
          await sleep(900);           // let the user read the message
          try {
            const target = await buildDemoClonePath('mdexplorer-demo');
            // The project is opened by path: the chosen environment travels bound to this path, and only to it.
            deps.setHarnessForPath(target, state.chosen);
            await deps.createFromTemplateAndOpen(target, DEMO_TEMPLATE_URL, demoLanguageFor(deps.currentLanguage()));
            await waitForRoute('/main', 30000).catch(() => { /* timeout — we still let the closing step run */ });
          } catch (err) {
            state.createError = (err as Error)?.message || String(err);
          }
        },
      },

      // The project was not created: said out loud, with the service's reason
      {
        textKey: 'MARK.TOUR.DEMO_CLONE.CREATE_FAILED',
        targetSelector: null,
        resolve: () => state.createError ? { textParams: { error: state.createError } } : null,
      },

      // Closing — says what the demo was configured for
      {
        textKey: 'MARK.TOUR.DEMO_CLONE.DONE',
        targetSelector: null,
        durationMs: 2500,
        resolve: () => state.createError ? null : state.chosen === NO_ENVIRONMENT
          ? { textKey: 'MARK.TOUR.DEMO_CLONE.DONE_NO_ENVIRONMENT' }
          : { textParams: { name: nameOf(state.chosen) } },
      },
    ],
  };
}
