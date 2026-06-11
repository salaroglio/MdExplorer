/**
 * MdExplorer — Runnable Fenced Code Blocks
 * =========================================
 * Finds `.mde-exec-block` wrappers emitted by the server-side
 * FromExecutableCodeBlockToRunnable command and hooks up the Run button.
 *
 * Flow:
 *   1. Click ▶ Run → postMessage to parent Angular with block metadata.
 *   2. Angular enforces trust, optionally opens a parameter dialog, then
 *      calls /api/MdExecution/Run with the filled-in parameters.
 *   3. SignalR streams output back to Angular → Angular postMessages chunks
 *      here → we render via ansi_up into the output pane.
 *
 * Message contract with parent Angular (all messages carry `type`):
 *   Out: mde-exec.requestRun  { blockId, lang, code, params }
 *   In:  mde-exec.output      { blockId, stream: "stdout"|"stderr", chunk }
 *   In:  mde-exec.completed   { blockId, exitCode, durationMs, timedOut }
 *   In:  mde-exec.error       { blockId, message }
 *   In:  mde-exec.cancelled   { blockId }     // dialog dismissed before run
 *   In:  mde-exec.denied      { blockId, reason }
 */
(function () {
    'use strict';

    var blocksById = new Map();

    function decodeBase64Utf8(b64) {
        if (!b64) return '';
        try {
            // atob returns a byte string; decode as UTF-8
            var binary = atob(b64);
            var bytes = new Uint8Array(binary.length);
            for (var i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
            if (typeof TextDecoder !== 'undefined') {
                return new TextDecoder('utf-8').decode(bytes);
            }
            return binary;
        } catch (e) {
            console.warn('[mde-exec] base64 decode failed:', e);
            return '';
        }
    }

    function parseBlock(element) {
        var paramsRaw = decodeBase64Utf8(element.dataset.params);
        var params = [];
        try { params = paramsRaw ? JSON.parse(paramsRaw) : []; }
        catch (e) { console.warn('[mde-exec] params JSON parse failed:', e); }
        return {
            element: element,
            blockId: element.dataset.blockId,
            lang: element.dataset.lang,
            code: decodeBase64Utf8(element.dataset.code),
            params: params,
            ansi: (typeof AnsiUp !== 'undefined') ? new AnsiUp() : null,
        };
    }

    function initBlocks() {
        var blocks = document.querySelectorAll('.mde-exec-block');
        blocks.forEach(function (element) {
            if (element.dataset.execBound) return;
            element.dataset.execBound = '1';

            var parsed = parseBlock(element);
            if (!parsed.blockId) return;
            blocksById.set(parsed.blockId, parsed);

            // Wire secret-toggle buttons next to password inputs.
            element.querySelectorAll('.mde-param-toggle').forEach(function (btn) {
                btn.addEventListener('click', function (evt) {
                    evt.preventDefault();
                    var input = btn.parentElement && btn.parentElement.querySelector('input.mde-param-input');
                    if (!input) return;
                    input.type = input.type === 'password' ? 'text' : 'password';
                });
            });

            // Wire path-picker buttons: click → ask parent Angular to open the project file browser.
            element.querySelectorAll('.mde-param-picker').forEach(function (btn) {
                btn.addEventListener('click', function (evt) {
                    evt.preventDefault();
                    requestPathPicker(parsed, btn);
                });
            });

            // Pressing Enter inside a param input triggers Run.
            element.querySelectorAll('input.mde-param-input').forEach(function (inp) {
                inp.addEventListener('keydown', function (evt) {
                    if (evt.key === 'Enter') {
                        evt.preventDefault();
                        var runBtn = element.querySelector('.mde-run-btn');
                        if (runBtn && !runBtn.disabled) runBtn.click();
                    }
                });
            });

            var runBtn = element.querySelector('.mde-run-btn');
            if (runBtn) {
                runBtn.addEventListener('click', function (evt) {
                    evt.preventDefault();
                    // When a service is running this block, the main button acts as Stop.
                    if (element.classList.contains('is-service-running')) {
                        requestStopService(parsed);
                    } else {
                        requestRun(parsed, 'batch');
                    }
                });
            }

            // Split-button caret → toggle the "Run as service" menu.
            var caret = element.querySelector('.mde-run-caret');
            var menu = element.querySelector('.mde-run-menu');
            if (caret && menu) {
                caret.addEventListener('click', function (evt) {
                    evt.preventDefault();
                    evt.stopPropagation();
                    menu.hidden = !menu.hidden;
                });
            }
            var svcBtn = element.querySelector('.mde-run-service');
            if (svcBtn) {
                svcBtn.addEventListener('click', function (evt) {
                    evt.preventDefault();
                    if (menu) menu.hidden = true;
                    requestRun(parsed, 'service');
                });
            }
        });

        // On (re)load, ask the parent which services are running for THIS document so blocks
        // whose service survived a document switch / backend restart re-render in their Stop state.
        queryRunningServices();
    }

    // Ask Angular which services are currently running for this document. The reply comes back
    // as one `mde-exec.serviceStarted` per running block (reusing the existing handler), scoped
    // by documentPath so an identical block in another document is never wrongly flipped.
    function queryRunningServices() {
        try {
            if (blocksById.size === 0) return;
            var documentPath = document.body ? (document.body.getAttribute('DocumentPath') || '') : '';
            window.parent.postMessage({
                type: 'mde-exec.queryServices',
                documentPath: documentPath,
            }, '*');
        } catch (e) {
            console.error('[mde-exec] queryServices postMessage failed:', e);
        }
    }

    // Close any open run-menu when clicking elsewhere in the document.
    document.addEventListener('click', function () {
        document.querySelectorAll('.mde-run-menu').forEach(function (m) { m.hidden = true; });
    });

    function harvestInlineValues(element, declaredParams) {
        // If the toolbar renders param inputs, harvest current values; otherwise
        // fall back to the server-declared defaults.
        var inputs = element.querySelectorAll('.mde-exec-params input[data-param-name]');
        if (!inputs.length) return { params: declaredParams, inline: false };
        var byName = {};
        inputs.forEach(function (inp) {
            byName[inp.getAttribute('data-param-name')] = inp.value;
        });
        var merged = (declaredParams || []).map(function (p) {
            return {
                name: p.name,
                defaultValue: Object.prototype.hasOwnProperty.call(byName, p.name) ? byName[p.name] : (p.defaultValue || ''),
                isSecret: p.isSecret,
                description: p.description,
                kind: p.kind,
            };
        });
        return { params: merged, inline: true };
    }

    function requestPathPicker(parsed, btn) {
        var paramName = btn.getAttribute('data-param-name');
        var mode = btn.getAttribute('data-picker-type') || 'file';
        var hidden = parsed.element.querySelector('input.mde-param-input[data-param-name="' + cssEscape(paramName) + '"]');
        var currentValue = hidden ? hidden.value : '';
        try {
            var projectPath = document.body ? (document.body.getAttribute('ProjectPath') || '') : '';
            window.parent.postMessage({
                type: 'mde-exec.requestPathPicker',
                blockId: parsed.blockId,
                paramName: paramName,
                mode: mode,
                projectPath: projectPath,
                currentValue: currentValue,
            }, '*');
        } catch (e) {
            console.error('[mde-exec] path-picker postMessage failed:', e);
        }
    }

    function applyPickedPath(parsed, paramName, path) {
        var hidden = parsed.element.querySelector('input.mde-param-input[data-param-name="' + cssEscape(paramName) + '"]');
        if (hidden) hidden.value = path || '';
        var btn = parsed.element.querySelector('.mde-param-picker[data-param-name="' + cssEscape(paramName) + '"]');
        if (btn) {
            var label = btn.querySelector('.mde-param-picker-label');
            if (label) {
                if (path) {
                    label.textContent = shortenPath(path);
                    btn.setAttribute('title', path);
                } else {
                    var mode = btn.getAttribute('data-picker-type') || 'file';
                    label.textContent = mode === 'dir' ? 'Choose folder…' : 'Choose file…';
                    btn.removeAttribute('title');
                }
            }
        }
    }

    function shortenPath(p) {
        if (!p) return '';
        if (p.length <= 38) return p;
        return '…' + p.substring(p.length - 37);
    }

    // Minimal CSS.escape polyfill subset — we only need to escape characters that appear in
    // parameter names (already validated by the backend as `[A-Za-z][A-Za-z0-9_-]*`), so the
    // identity is fine. Wrapper kept for clarity if the rules ever relax.
    function cssEscape(s) {
        return (s || '').replace(/(["\\])/g, '\\$1');
    }

    function requestRun(parsed, mode) {
        mode = mode || 'batch';
        markRunning(parsed, mode);
        try {
            // Project/document paths are written on <body ProjectPath="..." DocumentPath="...">
            // by the server when the iframe is rendered. The Angular parent still validates them.
            var projectPath = document.body ? (document.body.getAttribute('ProjectPath') || '') : '';
            var documentPath = document.body ? (document.body.getAttribute('DocumentPath') || '') : '';
            var harvested = harvestInlineValues(parsed.element, parsed.params);
            window.parent.postMessage({
                type: 'mde-exec.requestRun',
                blockId: parsed.blockId,
                lang: parsed.lang,
                code: parsed.code,
                params: harvested.params,
                paramsInline: harvested.inline,
                projectPath: projectPath,
                documentPath: documentPath,
                mode: mode,
            }, '*');
        } catch (e) {
            console.error('[mde-exec] postMessage to parent failed:', e);
            markIdle(parsed, 'communication error');
        }
    }

    function requestStopService(parsed) {
        try {
            window.parent.postMessage({
                type: 'mde-exec.requestStopService',
                blockId: parsed.blockId,
                serviceId: parsed.serviceId || null,
            }, '*');
        } catch (e) {
            console.error('[mde-exec] stop-service postMessage failed:', e);
        }
    }

    function enterServiceRunningState(parsed, serviceId) {
        parsed.serviceId = serviceId;
        parsed.element.classList.remove('is-running');
        parsed.element.classList.add('is-service-running');
        var btn = parsed.element.querySelector('.mde-run-btn');
        if (btn) {
            btn.disabled = false;
            var label = btn.querySelector('.mde-run-label');
            var icon = btn.querySelector('.mde-run-icon');
            if (label) label.textContent = ' Stop';
            if (icon) icon.innerHTML = '&#9632;'; // ■
        }
        var statusEl = parsed.element.querySelector('.mde-exec-output-status');
        if (statusEl) statusEl.textContent = 'Service running…';
    }

    function exitServiceRunningState(parsed, status) {
        parsed.serviceId = null;
        parsed.element.classList.remove('is-service-running');
        var btn = parsed.element.querySelector('.mde-run-btn');
        if (btn) {
            btn.disabled = false;
            var label = btn.querySelector('.mde-run-label');
            var icon = btn.querySelector('.mde-run-icon');
            if (label) label.textContent = ' Run';
            if (icon) icon.innerHTML = '&#9654;'; // ▶
        }
        var statusEl = parsed.element.querySelector('.mde-exec-output-status');
        if (statusEl) statusEl.textContent = status || '';
    }

    function markRunning(parsed, mode) {
        var output = parsed.element.querySelector('.mde-exec-output');
        var content = parsed.element.querySelector('.mde-exec-output-content');
        var status = parsed.element.querySelector('.mde-exec-output-status');
        if (content) content.textContent = '';
        if (output) output.hidden = false;
        if (status) status.textContent = (mode === 'service') ? 'Starting service…' : 'Running…';
        parsed.element.classList.add('is-running');
        parsed.element.classList.remove('is-error');
        var btn = parsed.element.querySelector('.mde-run-btn');
        // For service mode we keep the button enabled (it will flip to Stop once started).
        if (btn) btn.disabled = (mode !== 'service');
        // Reset ANSI parser state for a fresh run
        parsed.ansi = (typeof AnsiUp !== 'undefined') ? new AnsiUp() : null;
    }

    function markIdle(parsed, status, isError) {
        parsed.element.classList.remove('is-running');
        if (isError) parsed.element.classList.add('is-error');
        var btn = parsed.element.querySelector('.mde-run-btn');
        if (btn) btn.disabled = false;
        var statusEl = parsed.element.querySelector('.mde-exec-output-status');
        if (statusEl) statusEl.textContent = status || '';
    }

    function appendOutput(parsed, stream, chunk) {
        var content = parsed.element.querySelector('.mde-exec-output-content');
        if (!content) return;
        var html = parsed.ansi ? parsed.ansi.ansi_to_html(chunk) : escapeHtml(chunk);
        var span = document.createElement('span');
        span.className = (stream === 'stderr') ? 'mde-exec-stderr' : 'mde-exec-stdout';
        span.innerHTML = html;
        content.appendChild(span);
        var output = parsed.element.querySelector('.mde-exec-output');
        if (output) output.scrollTop = output.scrollHeight;
    }

    function escapeHtml(s) {
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function formatCompletedStatus(data) {
        var parts = [];
        if (data.timedOut) parts.push('TIMED OUT');
        parts.push('exit ' + data.exitCode);
        if (typeof data.durationMs === 'number') {
            parts.push(data.durationMs + ' ms');
        }
        return parts.join(' · ');
    }

    window.addEventListener('message', function (event) {
        var data = event.data;
        if (!data || !data.type) return;

        var parsed = data.blockId ? blocksById.get(data.blockId) : null;
        if (!parsed) return;

        switch (data.type) {
            case 'mde-exec.output':
                appendOutput(parsed, data.stream, data.chunk);
                break;
            case 'mde-exec.completed':
                markIdle(parsed, formatCompletedStatus(data), data.exitCode !== 0 || data.timedOut);
                break;
            case 'mde-exec.error':
                appendOutput(parsed, 'stderr', (data.message || 'error') + '\n');
                markIdle(parsed, 'error', true);
                break;
            case 'mde-exec.cancelled':
                markIdle(parsed, '', false);
                parsed.element.querySelector('.mde-exec-output').hidden = true;
                break;
            case 'mde-exec.denied':
                appendOutput(parsed, 'stderr', (data.reason || 'execution denied') + '\n');
                markIdle(parsed, 'denied', true);
                break;
            case 'mde-exec.pathPicked':
                // path === null/'' means the picker dialog was cancelled — leave current value as-is.
                if (data.path && data.paramName) {
                    applyPickedPath(parsed, data.paramName, data.path);
                }
                break;
            case 'mde-exec.serviceStarted':
                enterServiceRunningState(parsed, data.serviceId);
                break;
            case 'mde-exec.serviceStopped':
                // Ignore a stale "stopped" for a previous service id (e.g. during stop+restart),
                // otherwise it would revert the block while the new service is actually running.
                if (parsed.serviceId && data.serviceId && data.serviceId !== parsed.serviceId) break;
                var parts = [];
                if (data.status === 'killed') parts.push('stopped');
                else parts.push('service exited');
                if (typeof data.exitCode === 'number') parts.push('exit ' + data.exitCode);
                exitServiceRunningState(parsed, parts.join(' · '));
                break;
        }
    });

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initBlocks);
    } else {
        initBlocks();
    }
    // Safety re-scan (aligns with the pattern used by syntax-highlighting.js, etc.)
    setTimeout(initBlocks, 500);
})();
