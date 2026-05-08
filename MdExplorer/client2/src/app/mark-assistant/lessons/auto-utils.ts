/**
 * Auto-play utilities for lessons that drive the MDE UI on the user's behalf.
 *
 * These helpers are used by autoExecute steps (e.g. demo-clone-tour) to
 * click buttons, fill form inputs, wait for dialogs to open, and detect
 * route changes — all via direct DOM manipulation.
 *
 * Two notes on the model:
 *  - We don't reach into Angular components: we work on the rendered DOM
 *    using selectors. That keeps these helpers reusable across any UI bit.
 *  - For ngModel-bound inputs, setting `value` is not enough — we must
 *    `dispatchEvent(new Event('input', { bubbles: true }))` so Angular
 *    notices the change. That's what setInputValue() does.
 */

const POLL_INTERVAL_MS = 100;
const DEFAULT_WAIT_TIMEOUT_MS = 8000;

export const sleep = (ms: number): Promise<void> =>
  new Promise(r => setTimeout(r, ms));

/**
 * Wait for an element matching the selector to appear in the DOM.
 * Resolves with the element, rejects if not found within the timeout.
 */
export async function waitForElement<T extends HTMLElement = HTMLElement>(
  selector: string,
  timeoutMs: number = DEFAULT_WAIT_TIMEOUT_MS,
): Promise<T> {
  const start = Date.now();
  while (Date.now() - start < timeoutMs) {
    const el = document.querySelector(selector) as T | null;
    if (el) return el;
    await sleep(POLL_INTERVAL_MS);
  }
  throw new Error(`[Mark/auto] Element not found within ${timeoutMs}ms: ${selector}`);
}

/**
 * Wait until at least one element matching `selector` is GONE from the DOM.
 * Useful to wait for a dialog to close.
 */
export async function waitForElementGone(
  selector: string,
  timeoutMs: number = DEFAULT_WAIT_TIMEOUT_MS,
): Promise<void> {
  const start = Date.now();
  while (Date.now() - start < timeoutMs) {
    if (!document.querySelector(selector)) return;
    await sleep(POLL_INTERVAL_MS);
  }
  throw new Error(`[Mark/auto] Element still present after ${timeoutMs}ms: ${selector}`);
}

/** Click the first element matching the selector (must exist). */
export async function clickElement(selector: string): Promise<void> {
  const el = await waitForElement<HTMLElement>(selector);
  el.click();
}

/**
 * Set the value of a ngModel-bound input/textarea.
 * Optionally bypasses `readonly` (used for the clone dialog's localPath
 * which is rendered readonly because the picker normally fills it).
 */
export async function setInputValue(
  selector: string,
  value: string,
  options: { bypassReadonly?: boolean } = {},
): Promise<void> {
  const el = await waitForElement<HTMLInputElement>(selector);
  const wasReadOnly = el.readOnly;
  if (wasReadOnly && options.bypassReadonly) {
    el.readOnly = false;
  }
  // focus first — some Material directives require it for proper change detection
  el.focus();
  // Use the native setter so Angular's ngModel picks up the change
  const proto = Object.getPrototypeOf(el);
  const nativeSetter = Object.getOwnPropertyDescriptor(proto, 'value')?.set;
  if (nativeSetter) nativeSetter.call(el, value);
  else el.value = value;
  el.dispatchEvent(new Event('input', { bubbles: true }));
  el.dispatchEvent(new Event('change', { bubbles: true }));
  if (wasReadOnly && options.bypassReadonly) {
    el.readOnly = true;
  }
  el.blur();
}

/**
 * Wait for the router to navigate to a URL starting with `prefix`.
 * Resolves true on match within timeout, false otherwise. Non-throwing —
 * the caller decides what to do (autoExecute continues regardless).
 */
export async function waitForRoute(
  prefix: string,
  timeoutMs: number = DEFAULT_WAIT_TIMEOUT_MS,
): Promise<boolean> {
  const start = Date.now();
  while (Date.now() - start < timeoutMs) {
    if (window.location.hash.includes(prefix) || window.location.pathname.startsWith(prefix)) {
      return true;
    }
    // Angular may use hash routing or path routing — check both.
    await sleep(POLL_INTERVAL_MS);
  }
  return false;
}

/**
 * Build a default destination path for the demo clone. Uses Electron's
 * userData/temp dirs when available, otherwise a Windows-friendly fallback.
 * The path includes a timestamp so multiple runs don't collide.
 */
export function buildDemoClonePath(repoName = 'mdexplorer-demo'): string {
  const stamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19);
  // Electron exposes via process.env.USERPROFILE on Windows
  const home =
    (typeof process !== 'undefined' && (process as any)?.env?.USERPROFILE) ||
    'C:\\Users\\Public';
  return `${home}\\Documents\\${repoName}-${stamp}`;
}
