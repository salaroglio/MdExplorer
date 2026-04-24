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

            var runBtn = element.querySelector('.mde-run-btn');
            if (runBtn) {
                runBtn.addEventListener('click', function (evt) {
                    evt.preventDefault();
                    requestRun(parsed);
                });
            }
        });
    }

    function requestRun(parsed) {
        markRunning(parsed);
        try {
            // Project path is written on <body ProjectPath="..."> by the server when the iframe
            // is rendered. The Angular parent still validates it.
            var projectPath = document.body ? (document.body.getAttribute('ProjectPath') || '') : '';
            window.parent.postMessage({
                type: 'mde-exec.requestRun',
                blockId: parsed.blockId,
                lang: parsed.lang,
                code: parsed.code,
                params: parsed.params,
                projectPath: projectPath,
            }, '*');
        } catch (e) {
            console.error('[mde-exec] postMessage to parent failed:', e);
            markIdle(parsed, 'communication error');
        }
    }

    function markRunning(parsed) {
        var output = parsed.element.querySelector('.mde-exec-output');
        var content = parsed.element.querySelector('.mde-exec-output-content');
        var status = parsed.element.querySelector('.mde-exec-output-status');
        if (content) content.textContent = '';
        if (output) output.hidden = false;
        if (status) status.textContent = 'Running…';
        parsed.element.classList.add('is-running');
        parsed.element.classList.remove('is-error');
        var btn = parsed.element.querySelector('.mde-run-btn');
        if (btn) btn.disabled = true;
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
