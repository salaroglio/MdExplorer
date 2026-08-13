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

    // Detached standalone window: there is no Angular parent to honor run
    // requests (the page is top-level, so postMessage to "parent" goes nowhere).
    // We therefore disable the Run/picker controls instead of leaving buttons
    // that spin forever waiting for a reply that will never come.
    var IS_DETACHED = (function () {
        try { return new URLSearchParams(window.location.search).get('detached') === 'true'; }
        catch (e) { return false; }
    })();

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

        if (IS_DETACHED) {
            blocks.forEach(function (element) {
                if (element.dataset.detachedDisabled) return;
                element.dataset.detachedDisabled = '1';
                element.classList.add('mde-detached-disabled');
                element.querySelectorAll('.mde-run-btn, .mde-run-caret, .mde-run-service, .mde-param-picker, .mde-copy-btn')
                    .forEach(function (btn) {
                        btn.disabled = true;
                        btn.style.opacity = '0.5';
                        btn.style.cursor = 'not-allowed';
                        btn.title = 'Not available in a detached window';
                    });
            });
            return;
        }

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

            var copyBtn = element.querySelector('.mde-copy-btn');
            if (copyBtn) {
                copyBtn.addEventListener('click', function (evt) {
                    evt.preventDefault();
                    requestCopy(parsed);
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
        // <select> covers the enum parameters (`@param X — a | b`); the picker button also
        // carries data-param-name but its value lives in the hidden input next to it.
        var inputs = element.querySelectorAll(
            '.mde-exec-params input[data-param-name], .mde-exec-params select[data-param-name]');
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

    // Copy is resolved and written to the system clipboard by the backend (same substitution the
    // runner applies), so what you paste is exactly what Run would execute.
    //
    // The call goes STRAIGHT to the backend from here, not through the Angular parent: the iframe
    // is same-origin with the server, this is the pattern the other in-document clipboard actions
    // already use (clipboard-paste.js, html-preview.js → ZipAndCopyToClipboard), and it keeps the
    // button working regardless of what the shell around the document is running.
    function requestCopy(parsed) {
        markCopyState(parsed, 'pending');
        var harvested = harvestInlineValues(parsed.element, parsed.params);
        var parameters = {};
        (harvested.params || []).forEach(function (p) {
            parameters[p.name] = p.defaultValue || '';
        });

        fetch('/api/MdExecution/Copy', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                blockId: parsed.blockId,
                language: parsed.lang,
                code: parsed.code,
                parameters: parameters,
            }),
        })
            .then(function (response) {
                if (!response.ok) {
                    return response.json()
                        .catch(function () { return {}; })
                        .then(function (body) {
                            throw new Error([body.error, body.hint].filter(Boolean).join(' — ')
                                || ('Server error ' + response.status));
                        });
                }
                return response.json();
            })
            .then(function () {
                markCopyState(parsed, 'done');
            })
            .catch(function (err) {
                console.error('[mde-exec] copy failed:', err);
                markCopyState(parsed, 'error', err.message || 'Copy failed');
            });
    }

    // Material Icons glyphs (filled, fill=currentColor) — the same solid family the Angular
    // toolbar uses. Thin outline icons read washed out next to them at this size.
    var SVG_OPEN = '<svg class="mde-copy-icon" xmlns="http://www.w3.org/2000/svg" width="13" height="13"'
        + ' viewBox="0 0 24 24" fill="currentColor">';
    var ICON_CHECK = SVG_OPEN + '<path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"></path></svg>';
    var ICON_ERROR = SVG_OPEN + '<path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59'
        + ' 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"></path></svg>';

    // Replaces the glyph only, leaving the " Copy" label in place — the button keeps the same
    // shape as Run next to it, just changing what it says.
    function setCopyIcon(btn, html) {
        var icon = btn.querySelector('.mde-copy-icon');
        if (icon) icon.outerHTML = html;
        else btn.insertAdjacentHTML('afterbegin', html);
    }

    // 'pending' | 'done' | 'error' — the button reports the backend's answer (✓ only once the
    // clipboard write has actually confirmed), then returns to the copy icon.
    function markCopyState(parsed, state, message) {
        var btn = parsed.element.querySelector('.mde-copy-btn');
        if (!btn) return;
        var label = btn.querySelector('.mde-copy-label');
        // Captured once, so restoring never has to re-declare the icon the server rendered.
        if (!parsed.copyIconHtml) {
            var restIcon = btn.querySelector('.mde-copy-icon');
            parsed.copyIconHtml = restIcon ? restIcon.outerHTML : '';
        }
        if (parsed.copyResetTimer) {
            clearTimeout(parsed.copyResetTimer);
            parsed.copyResetTimer = null;
        }
        btn.classList.remove('is-copied', 'is-copy-error', 'is-copying');

        if (state === 'pending') {
            btn.disabled = true;
            btn.classList.add('is-copying');
            // Never wait forever: if the request hangs, the button would stay disabled with no
            // explanation. Report it instead — a late answer just overwrites this.
            parsed.copyResetTimer = setTimeout(function () {
                parsed.copyResetTimer = null;
                markCopyState(parsed, 'error', 'No answer from the server');
            }, 8000);
            return;
        }

        btn.disabled = false;
        if (state === 'done') {
            btn.classList.add('is-copied');
            setCopyIcon(btn, ICON_CHECK);
            if (label) label.textContent = ' Copied';
            btn.title = 'Copied to the clipboard';
        } else {
            btn.classList.add('is-copy-error');
            setCopyIcon(btn, ICON_ERROR);
            if (label) label.textContent = ' Failed';
            btn.title = message || 'Copy failed';
        }
        parsed.copyResetTimer = setTimeout(function () {
            btn.classList.remove('is-copied', 'is-copy-error');
            setCopyIcon(btn, parsed.copyIconHtml);
            if (label) label.textContent = ' Copy';
            btn.title = 'Copy the command with the current parameter values';
            parsed.copyResetTimer = null;
        }, 2000);
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

    // Detached standalone window: the native Electron frame is hidden (frame:false), so we
    // inject a lightweight app-bar with a draggable region + Refresh and Close controls.
    // Pure DOM, no Angular. Idempotent. In a plain web tab (window.open) the drag region is
    // ignored by the browser but Refresh/Close still work.
    function injectDetachedAppBar() {
        if (!IS_DETACHED || !document.body) return;
        if (document.getElementById('mde-detached-appbar')) return;

        var isDark = false;
        try { isDark = (new URLSearchParams(window.location.search).get('theme') || '').toLowerCase().indexOf('dark') !== -1; }
        catch (e) { }
        var bg = isDark ? '#1e1e1e' : '#f3f3f3';
        var fg = isDark ? '#e0e0e0' : '#333333';
        var border = isDark ? '#333333' : '#dddddd';
        var hover = isDark ? '#333333' : '#e0e0e0';
        var BAR_H = 38;

        var bar = document.createElement('div');
        bar.id = 'mde-detached-appbar';
        bar.style.cssText = [
            'position:fixed', 'top:0', 'left:0', 'right:0', 'height:' + BAR_H + 'px',
            'display:flex', 'align-items:center', 'gap:2px', 'padding:0 6px',
            'box-sizing:border-box', 'background:' + bg, 'color:' + fg,
            'border-bottom:1px solid ' + border,
            'font-family:system-ui,Segoe UI,sans-serif', 'font-size:13px',
            'z-index:2147483647', 'user-select:none', '-webkit-app-region:drag'
        ].join(';');

        var title = document.createElement('div');
        title.style.cssText = 'flex:1;padding-left:6px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;opacity:0.8;';
        try { title.textContent = decodeURIComponent((window.location.pathname.split('/').pop() || 'MdExplorer')); }
        catch (e) { title.textContent = 'MdExplorer'; }
        bar.appendChild(title);

        function mkBtn(glyph, tip, onClick, danger) {
            var b = document.createElement('button');
            b.type = 'button';
            b.title = tip;
            b.innerHTML = glyph;
            b.style.cssText = [
                '-webkit-app-region:no-drag', 'border:none', 'background:transparent',
                'color:' + fg, 'cursor:pointer', 'width:34px', 'height:30px',
                'border-radius:4px', 'font-size:16px', 'line-height:1',
                'display:flex', 'align-items:center', 'justify-content:center'
            ].join(';');
            b.addEventListener('mouseenter', function () { b.style.background = danger ? '#e81123' : hover; if (danger) b.style.color = '#ffffff'; });
            b.addEventListener('mouseleave', function () { b.style.background = 'transparent'; b.style.color = fg; });
            b.addEventListener('click', onClick);
            return b;
        }

        bar.appendChild(mkBtn('&#x21bb;', 'Refresh', function () { location.reload(); }, false));
        bar.appendChild(mkBtn('&#x2715;', 'Close', function () { window.close(); }, true));

        document.body.appendChild(bar);
        // Push page content below the fixed bar (once).
        if (!document.body.dataset.mdeDetachedPadded) {
            document.body.dataset.mdeDetachedPadded = '1';
            var cur = parseFloat(getComputedStyle(document.body).paddingTop) || 0;
            document.body.style.paddingTop = (cur + BAR_H) + 'px';
        }
    }

    function bootstrap() {
        initBlocks();
        injectDetachedAppBar();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', bootstrap);
    } else {
        bootstrap();
    }
    // Safety re-scan (aligns with the pattern used by syntax-highlighting.js, etc.)
    setTimeout(bootstrap, 500);
})();
