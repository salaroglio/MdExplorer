/**
 * MdExplorer - The right-click menu of a slide
 * ============================================
 * Right-click on a point of a slide (a title, an item, a paragraph, a cell):
 *  - "Modifica testo": the same correction as in a document (inline-edit.js, unchanged): type,
 *    Enter or a click elsewhere saves, Esc puts it back. Only on blocks that can be corrected
 *    safely: the page carries the file's fingerprint and each block its file line (SlideDeckRenderer);
 *    the server refuses what it cannot correct.
 *  - "Chiedi a MarkAgent": MarkAgent explains that point from the project's documents, in Mark's
 *    dialog (MarkDiagramService, api/markdiagram/explain-point). Any point with text, also one a
 *    command wrote or a block of code.
 * On a box of a diagram the menu is mark-diagram-context.js's, started here: in a deck the server
 * explains the box from the project's documents too.
 *
 * Only inside MdExplorer's view (its page around the slide is the app, /client2/): not in the
 * speaker view, not in a detached window, not in print.
 *
 * reveal.js ignores the keys typed in the block by itself; the keys that end the correction, and
 * those pressed while a menu is open, are held back by slide-diagrams.js (data-mde-holds-keys,
 * #mark-diagram-menu).
 *
 * Sprints: docs-internal/Sprints/2026-09-24-Slide-Modifica-Testo.md,
 *          docs-internal/Sprints/2026-09-25-Slide-Chiedi-A-MarkAgent.md
 */
(function () {
    'use strict';

    var MENU_ID = 'mde-slide-edit-menu';
    var MSG_ASK_POINT = 'mde-mark.askAboutPoint';
    var POINT_SELECTOR = 'p, h1, h2, h3, h4, h5, h6, li, td, th, pre, blockquote';
    var LABEL_LENGTH = 60;

    function inMdExplorerView() {
        if (window.parent === window) return false;
        try {
            return /\/client2\//.test(window.parent.location.pathname);
        } catch (e) {
            return false;
        }
    }

    function isPrint() {
        return /print-pdf|view=print/.test(window.location.search);
    }

    function canCorrect() {
        return inMdExplorerView()
            && !!window.mdeInlineEdit
            && !!document.body.getAttribute('data-mde-source-hash')
            && !isPrint();
    }

    function canAsk() {
        return inMdExplorerView() && !isPrint() && !!document.body.getAttribute('DocumentPath');
    }

    // ---- the point ----

    /** The text of a block, without the lists nested in it (an item is itself, not its sub-items). */
    function ownText(block) {
        var clone = block.cloneNode(true);
        clone.querySelectorAll('ul, ol').forEach(function (list) { list.remove(); });
        return clone.textContent.replace(/\s+/g, ' ').trim();
    }

    /** The point under the pointer — the cell, the item, not the table or the list — or null. */
    function pointAt(target) {
        var el = target && target.nodeType === Node.ELEMENT_NODE ? target : (target && target.parentElement);
        if (!el || el.closest('svg, aside.notes, [contenteditable="true"]')) return null;
        var content = document.querySelector('[data-mde-content]');
        var block = el.closest(POINT_SELECTOR);
        if (!content || !block || !content.contains(block)) return null;
        return ownText(block) ? block : null;
    }

    function labelOf(text) {
        return text.length <= LABEL_LENGTH ? text : text.substring(0, LABEL_LENGTH).trim() + '…';
    }

    /** The point, with the slide it sits in: its title, and all its visible text (not the notes). */
    function contextOf(block) {
        var text = ownText(block);
        var slide = block.closest('section');
        var title = slide && slide.querySelector('h1, h2, h3');
        return {
            documentPath: document.body.getAttribute('DocumentPath') || '',
            projectPath: document.body.getAttribute('ProjectPath') || '',
            point: {
                label: labelOf(text),
                text: text,
                kind: 'text',
                slideTitle: title ? title.textContent.trim() : null,
                // innerText leaves out what is not shown: the speaker notes, a closed legend.
                slideText: slide ? slide.innerText.trim() : null
            }
        };
    }

    function askMarkAgent(block) {
        try {
            window.parent.postMessage({ type: MSG_ASK_POINT, context: contextOf(block) }, '*');
        } catch (e) {
            console.error('[slide-edit] postMessage fallito:', e);
        }
    }

    // ---- the menu ----

    function closeMenu() {
        var menu = document.getElementById(MENU_ID);
        if (menu) menu.remove();
    }

    function addItem(menu, icon, text, onClick) {
        var item = document.createElement('button');
        item.type = 'button';
        item.className = 'mde-paste-menu-item';
        item.innerHTML = '<span class="mde-paste-menu-icon">' + icon + '</span>' + text;
        item.addEventListener('click', function () {
            closeMenu();
            onClick();
        });
        menu.appendChild(item);
    }

    /** The document's menu (clipboard-paste.js), with its classes. */
    function openMenu(x, y, items) {
        closeMenu();
        var menu = document.createElement('div');
        menu.id = MENU_ID;
        menu.className = 'mde-paste-menu';
        menu.setAttribute('data-mde-holds-keys', '');

        items.forEach(function (item) { addItem(menu, item.icon, item.text, item.onClick); });

        var hint = document.createElement('div');
        hint.className = 'mde-paste-menu-hint';
        hint.textContent = 'Shift + tasto destro: menu del browser';
        menu.appendChild(hint);

        document.body.appendChild(menu);
        var rect = menu.getBoundingClientRect();
        menu.style.left = Math.max(8, Math.min(x, window.innerWidth - rect.width - 8)) + 'px';
        menu.style.top = Math.max(8, Math.min(y, window.innerHeight - rect.height - 8)) + 'px';
    }

    document.addEventListener('contextmenu', function (event) {
        closeMenu();
        if (event.shiftKey) return;
        var x = event.clientX, y = event.clientY;
        var items = [];

        var editable = canCorrect() ? window.mdeInlineEdit.blockAt(event.target) : null;
        if (editable) {
            items.push({ icon: '✏️', text: 'Modifica testo', onClick: function () { window.mdeInlineEdit.start(editable, x, y); } });
        }
        var point = canAsk() ? pointAt(event.target) : null;
        if (point) {
            items.push({ icon: '💬', text: 'Chiedi a MarkAgent', onClick: function () { askMarkAgent(point); } });
        }

        if (!items.length) return;
        event.preventDefault();
        openMenu(x, y, items);
    });

    document.addEventListener('mousedown', function (event) {
        if (!event.target.closest || !event.target.closest('#' + MENU_ID)) closeMenu();
    }, true);

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') closeMenu();
    });

    if (window.Reveal) {
        Reveal.on('slidechanged', closeMenu);
        // The menu of a diagram's boxes: its postMessage goes to MdExplorer's page around the slide.
        Reveal.on('ready', function () {
            if (!canAsk()) return;
            if (window.MarkDiagramContext && typeof MarkDiagramContext.initAll === 'function') {
                MarkDiagramContext.initAll();
            } else {
                console.error('[slide-edit] mark-diagram-context.js is not loaded: no "Ask to MarkAgent" on the boxes.');
            }
        });
    }
})();
