/**
 * MdExplorer - Correct the text of a block on the rendered page
 * ==============================================================
 * Right-click → "Modifica testo" (the menu of clipboard-paste.js) makes ONE block editable: a
 * paragraph, a heading, a list item, a table cell. Only the text already there changes — no
 * formatting, no new lines. Enter or a click elsewhere saves; Esc puts the block back.
 *
 * The page never sends HTML. It sends the block's runs — each text node with the tags around
 * it, and the elements with no text of their own (img, br, input) — as they were when editing
 * started and as they are now. POST /api/mdfiles/EditRenderedText turns the difference into
 * edits of exactly those characters in the .md (RenderedTextEditor), checks that the new file
 * renders what was typed, or refuses (422) and changes nothing. A page older than the file: 409.
 *
 * The block is addressed as the source map sees it: p / h1-h6 / li by their own
 * data-mde-line-start; a cell by its table's line, its row (header = 0) and its column.
 * A block without the attribute (generated content, a paragraph split by the image toolbar)
 * is not offered: never guess.
 */

(function () {
    'use strict';

    if (window.mdeInlineEdit) {
        return;
    }

    // The document's content area, or one a page declares as its own (the slides of a deck).
    var CONTENT_SELECTOR = '.mdeItemMainPageCenter, [data-mde-content]';
    var LEAF_SELECTOR = 'p, h1, h2, h3, h4, h5, h6, li, td, th';
    var OBJECT_TAGS = { img: true, br: true, input: true };
    // Blocks inside the one being corrected (the sub-list of a list item) are not its text.
    var NESTED_BLOCK_TAGS = { ul: true, ol: true, p: true, blockquote: true, pre: true, table: true, div: true };
    var SKIPPED_TAGS = { script: true, style: true };
    // What the browser creates while typing next to formatted text: shown bold, meant bold.
    var BROWSER_FORMATTING = { b: 'strong', i: 'em', s: 'del', strike: 'del' };
    // Anything else (typing, deleting, cut, undo) is a text change; formatting, new paragraphs,
    // line breaks and drops are not.
    var ALLOWED_INPUT = /^(insertText|insertReplacementText|insertCompositionText|delete(Content|Word|SoftLine|HardLine)(Backward|Forward)|deleteByCut|historyUndo|historyRedo)$/;
    var HINT_ID = 'mde-inline-edit-hint';
    var TOAST_ID = 'mde-inline-edit-toast';

    var active = null;

    // ─────────────────────────────────────────────────────────────────────
    //  The page
    // ─────────────────────────────────────────────────────────────────────

    /** A text file shown as source (body data-mde-view) is not a markdown document. */
    function isMarkdownPage() {
        return !document.body.hasAttribute('data-mde-view');
    }

    function getSourceHash() {
        return document.body.getAttribute('data-mde-source-hash') || null;
    }

    function parseLine(el) {
        var line = parseInt(el.getAttribute('data-mde-line-start'), 10);
        return isNaN(line) || line < 1 ? null : line;
    }

    function elementIndex(el, tagNames) {
        return Array.prototype.filter.call(el.parentElement.children, function (child) {
            return tagNames.indexOf(child.tagName) >= 0;
        }).indexOf(el);
    }

    /** What the backend needs to find the block in the file, or null when the page cannot say. */
    function targetOf(block) {
        var tag = block.tagName;
        if (tag === 'TD' || tag === 'TH') {
            var row = block.parentElement;
            var section = row && row.parentElement;
            var table = section && section.parentElement;
            if (!row || row.tagName !== 'TR' || !table || table.tagName !== 'TABLE') return null;
            var tableLine = parseLine(table);
            if (tableLine === null) return null;
            var rowIndex;
            if (section.tagName === 'THEAD') rowIndex = 0;
            else if (section.tagName === 'TBODY') rowIndex = elementIndex(row, ['TR']) + 1; // pipe tables: one header row
            else return null;
            return { line: tableLine, row: rowIndex, column: elementIndex(block, ['TD', 'TH']) };
        }
        var line = parseLine(block);
        return line === null ? null : { line: line };
    }

    /**
     * The block's runs. <paramref>original</paramref>: the elements that were there when editing
     * started; a b/i/s the browser created since counts as the formatting it shows.
     */
    function runsOf(block, original) {
        var runs = [];
        (function collect(node, path) {
            for (var child = node.firstChild; child; child = child.nextSibling) {
                if (child.nodeType === Node.TEXT_NODE) {
                    runs.push({ text: child.nodeValue, path: path.slice() });
                } else if (child.nodeType === Node.ELEMENT_NODE) {
                    var tag = child.tagName.toLowerCase();
                    if (SKIPPED_TAGS[tag]) continue;
                    if (OBJECT_TAGS[tag]) {
                        runs.push({ object: tag, path: path.slice() });
                    } else if (NESTED_BLOCK_TAGS[tag]) {
                        continue;
                    } else if (tag === 'mark' && child.classList.contains('mdeSearchHighlight')) {
                        collect(child, path); // the in-page search, not ==highlight==
                    } else {
                        if (original && !original.has(child) && BROWSER_FORMATTING[tag]) tag = BROWSER_FORMATTING[tag];
                        collect(child, path.concat(tag));
                    }
                }
            }
        })(block, []);
        return runs;
    }

    function hasText(runs) {
        return runs.some(function (run) { return run.text && run.text.trim().length > 0; });
    }

    /**
     * The block to correct under a point of the page — the cell, the list item, not the table
     * or the list — or null when there is none that can be corrected safely.
     */
    function editableBlockAt(target) {
        if (active || !isMarkdownPage() || !getSourceHash()) return null;
        var el = target && target.nodeType === Node.ELEMENT_NODE ? target : (target && target.parentElement);
        if (!el || el.closest('svg, pre, [contenteditable="true"]')) return null;
        var content = document.querySelector(CONTENT_SELECTOR);
        if (!content || !content.contains(el)) return null;
        var block = el.closest(LEAF_SELECTOR);
        if (!block || !content.contains(block) || !targetOf(block)) return null;
        return hasText(runsOf(block)) ? block : null;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Editing
    // ─────────────────────────────────────────────────────────────────────

    function start(block, clientX, clientY) {
        if (active) return;
        var target = targetOf(block);
        if (!target) return;
        var before = runsOf(block);
        if (!hasText(before)) {
            showToast('Qui non c\'è testo da correggere.');
            return;
        }

        active = {
            block: block,
            target: target,
            before: before,
            html: block.innerHTML,
            sourceHash: getSourceHash(),
            original: new Set(block.querySelectorAll('*')),
            locked: [],
            saving: false
        };

        // Islands: images, checkboxes and nested blocks stay as they are.
        block.querySelectorAll('img, input, ul, ol, p, blockquote, pre, table, div').forEach(function (el) {
            if (!el.hasAttribute('contenteditable')) {
                el.setAttribute('contenteditable', 'false');
                active.locked.push(el);
            }
        });

        block.setAttribute('contenteditable', 'true');
        block.classList.add('mde-inline-editing');
        document.body.setAttribute('data-mde-inline-editing', '');

        block.addEventListener('keydown', onKeyDown);
        block.addEventListener('beforeinput', onBeforeInput);
        block.addEventListener('paste', onPaste);
        block.addEventListener('drop', onDrop);
        block.addEventListener('click', onClickInside, true);
        document.addEventListener('mousedown', onMouseDownOutside, true);
        window.addEventListener('blur', onWindowBlur);

        showHint(block, 'Invio o clic fuori: salva · Esc: annulla');
        block.focus();
        placeCaret(block, clientX, clientY);
    }

    function placeCaret(block, x, y) {
        var range = null;
        if (document.caretRangeFromPoint) {
            range = document.caretRangeFromPoint(x, y);
        } else if (document.caretPositionFromPoint) {
            var position = document.caretPositionFromPoint(x, y);
            if (position) {
                range = document.createRange();
                range.setStart(position.offsetNode, position.offset);
                range.collapse(true);
            }
        }
        if (range && block.contains(range.startContainer)) {
            var selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(range);
        }
    }

    function onKeyDown(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            commit();
        } else if (e.key === 'Escape') {
            e.preventDefault();
            cancel();
        } else if ((e.ctrlKey || e.metaKey) && /^[biu]$/i.test(e.key)) {
            e.preventDefault(); // no formatting
        }
    }

    function onBeforeInput(e) {
        if (!ALLOWED_INPUT.test(e.inputType)) {
            e.preventDefault();
        }
    }

    /** Plain text on one line: a pasted paragraph must not bring its markup or its line breaks. */
    function onPaste(e) {
        e.preventDefault();
        var text = (e.clipboardData && e.clipboardData.getData('text/plain')) || '';
        text = text.replace(/\r\n|\r|\n/g, ' ');
        if (text) {
            document.execCommand('insertText', false, text);
        }
    }

    function onDrop(e) {
        e.preventDefault();
    }

    /** A click places the caret: it does not follow a link or run the page's click handlers. */
    function onClickInside(e) {
        if (e.target.closest && e.target.closest('a')) {
            e.preventDefault();
        }
        e.stopPropagation();
    }

    function onMouseDownOutside(e) {
        if (!active || active.saving || active.block.contains(e.target)) return;
        commit();
    }

    function onWindowBlur() {
        commit();
    }

    /** Chromium keeps an emptied element open with a <br>: not a line break the user typed. */
    function withoutPlaceholderBreak(runs, before) {
        var beforeEndsWithBreak = before.length > 0 && before[before.length - 1].object === 'br';
        var result = runs.slice();
        while (result.length > 0 && !beforeEndsWithBreak) {
            var last = result[result.length - 1];
            if (last.object === 'br' || (last.text !== undefined && last.text.trim() === '')) {
                result.pop();
            } else {
                break;
            }
        }
        return result;
    }

    async function commit() {
        if (!active || active.saving) return;
        var edit = active;
        var after = withoutPlaceholderBreak(runsOf(edit.block, edit.original), edit.before);

        // Trailing layout whitespace (a list item ends with its newline) is not text: the backend
        // ignores it too.
        if (JSON.stringify(after) === JSON.stringify(withoutPlaceholderBreak(edit.before, edit.before))) {
            finish(false);
            return;
        }

        edit.saving = true;
        var selection = window.getSelection();
        edit.range = selection && selection.rangeCount > 0 ? selection.getRangeAt(0).cloneRange() : null;
        edit.block.setAttribute('contenteditable', 'false');
        edit.block.classList.add('mde-inline-saving');
        showHint(edit.block, 'Salvataggio…');

        var body = {
            connectionId: document.body.getAttribute('ConnectionId'),
            documentPath: document.body.getAttribute('DocumentPath'),
            sourceHash: edit.sourceHash,
            line: edit.target.line,
            before: edit.before,
            after: after
        };
        if (edit.target.row !== undefined) {
            body.row = edit.target.row;
            body.column = edit.target.column;
        }

        try {
            var response = await fetch('/api/mdfiles/EditRenderedText', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            var result = null;
            try { result = await response.json(); } catch (parseError) { /* not JSON */ }

            if (response.ok) {
                console.log('[inline-edit.js] ' + (result && result.status) + ' at line ' + edit.target.line, edit.target);
                // The page now shows the file: its fingerprint moves with it, so a second
                // correction works before the reload SignalR asks for.
                if (result && result.sourceHash) {
                    document.body.setAttribute('data-mde-source-hash', result.sourceHash);
                }
                finish(false);
                return;
            }

            console.warn('[inline-edit.js] Correction refused:', response.status, result);
            var message = (result && result.message) || ('Correzione non salvata (HTTP ' + response.status + ')');
            if (response.status === 422) {
                // What was typed is not lost: the message says what to change, Enter tries again.
                resume(edit);
            } else {
                // 409: the page is older than the file, no save can work until it is reloaded.
                finish(true);
            }
            showToast(message);
        } catch (error) {
            console.error('[inline-edit.js] Error calling EditRenderedText:', error);
            resume(edit);
            showToast('Correzione non salvata: ' + error.message);
        }
    }

    /** Back to editing after a refusal, with the typed text and the caret where they were. */
    function resume(edit) {
        if (active !== edit) return;
        edit.saving = false;
        edit.block.setAttribute('contenteditable', 'true');
        edit.block.classList.remove('mde-inline-saving');
        showHint(edit.block, 'Correggi e premi Invio · Esc: annulla');
        edit.block.focus();
        if (edit.range) {
            var selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(edit.range);
        }
    }

    function cancel() {
        if (!active || active.saving) return;
        finish(true);
    }

    /** Ends the editing. restore: put back the block as it was (nothing was saved). */
    function finish(restore) {
        var edit = active;
        if (!edit) return;
        active = null;

        document.removeEventListener('mousedown', onMouseDownOutside, true);
        window.removeEventListener('blur', onWindowBlur);
        var block = edit.block;
        block.removeEventListener('keydown', onKeyDown);
        block.removeEventListener('beforeinput', onBeforeInput);
        block.removeEventListener('paste', onPaste);
        block.removeEventListener('drop', onDrop);
        block.removeEventListener('click', onClickInside, true);

        if (restore) {
            block.innerHTML = edit.html;
        } else {
            edit.locked.forEach(function (el) { el.removeAttribute('contenteditable'); });
        }
        block.removeAttribute('contenteditable');
        block.classList.remove('mde-inline-editing', 'mde-inline-saving');
        document.body.removeAttribute('data-mde-inline-editing');
        removeHint();
        var selection = window.getSelection();
        if (selection) selection.removeAllRanges();
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Hint and notice
    // ─────────────────────────────────────────────────────────────────────

    /**
     * At the bottom of the window, where the notices go. Above the block it covered what the
     * user needs to see while correcting — on a cell, the column header (seen in the browser).
     */
    function showHint(block, text) {
        removeHint();
        var hint = document.createElement('div');
        hint.id = HINT_ID;
        hint.className = 'mde-inline-edit-hint';
        hint.textContent = '✏️ ' + text;
        document.body.appendChild(hint);
    }

    function removeHint() {
        var hint = document.getElementById(HINT_ID);
        if (hint) hint.remove();
    }

    function showToast(message) {
        var existing = document.getElementById(TOAST_ID);
        if (existing) existing.remove();
        var toast = document.createElement('div');
        toast.id = TOAST_ID;
        toast.className = 'mde-inline-edit-toast';
        toast.textContent = message;
        toast.addEventListener('click', function () { toast.remove(); });
        document.body.appendChild(toast);
        setTimeout(function () { if (toast.isConnected) toast.remove(); }, 8000);
    }

    window.mdeInlineEdit = {
        /** The block that "Modifica testo" would correct at this point, or null. */
        blockAt: editableBlockAt,
        /** Starts correcting the block, with the caret at the point of the right-click. */
        start: start
    };

    console.log('[inline-edit.js] Text correction on the page ready');
})();
