/**
 * MdExplorer - Correcting the text of a slide
 * ============================================
 * The same correction as in a document (inline-edit.js, unchanged): right-click on a block →
 * "Modifica testo", type, Enter or a click elsewhere saves, Esc puts it back. In a document the
 * menu belongs to clipboard-paste.js, a script of the document view; this is the slides' way in.
 *
 * Only inside MdExplorer's view (its page around the slide is the app, /client2/): not in the
 * speaker view, not in a detached window, not in print. The page carries the file's fingerprint
 * and each block its file line (SlideDeckRenderer); the server refuses what it cannot correct safely.
 *
 * reveal.js ignores the keys typed in the block by itself; the keys that end the correction, and
 * those pressed while this menu is open, are held back by slide-diagrams.js (data-mde-holds-keys).
 *
 * Sprint: docs-internal/Sprints/2026-09-24-Slide-Modifica-Testo.md
 */
(function () {
    'use strict';

    var MENU_ID = 'mde-slide-edit-menu';

    function inMdExplorerView() {
        if (window.parent === window) return false;
        try {
            return /\/client2\//.test(window.parent.location.pathname);
        } catch (e) {
            return false;
        }
    }

    function canCorrect() {
        return inMdExplorerView()
            && !!window.mdeInlineEdit
            && !!document.body.getAttribute('data-mde-source-hash')
            && !/print-pdf|view=print/.test(window.location.search);
    }

    function closeMenu() {
        var menu = document.getElementById(MENU_ID);
        if (menu) menu.remove();
    }

    /** The document's menu (clipboard-paste.js), with its classes, and one entry. */
    function openMenu(x, y, onEdit) {
        closeMenu();
        var menu = document.createElement('div');
        menu.id = MENU_ID;
        menu.className = 'mde-paste-menu';
        menu.setAttribute('data-mde-holds-keys', '');

        var item = document.createElement('button');
        item.type = 'button';
        item.className = 'mde-paste-menu-item';
        item.innerHTML = '<span class="mde-paste-menu-icon">✏️</span>Modifica testo';
        item.addEventListener('click', function () {
            closeMenu();
            onEdit();
        });
        menu.appendChild(item);

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
        if (event.shiftKey || !canCorrect()) return;
        var block = window.mdeInlineEdit.blockAt(event.target);
        if (!block) return;
        event.preventDefault();
        var x = event.clientX, y = event.clientY;
        openMenu(x, y, function () { window.mdeInlineEdit.start(block, x, y); });
    });

    document.addEventListener('mousedown', function (event) {
        if (!event.target.closest || !event.target.closest('#' + MENU_ID)) closeMenu();
    }, true);

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') closeMenu();
    });

    if (window.Reveal) {
        Reveal.on('slidechanged', closeMenu);
    }
})();
