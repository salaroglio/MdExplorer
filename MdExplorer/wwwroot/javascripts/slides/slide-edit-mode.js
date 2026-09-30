/**
 * MdExplorer - What "Modifica veloce" does on a slide
 * ====================================================
 * The bar's first button (slide-toolbar.js) turns the mode on; this is what the page does then:
 *
 *  - the area that can be corrected is outlined under the pointer: exactly what inline-edit.js
 *    would correct (mdeInlineEdit.blockAt — a title, a paragraph, an item, a cell; not code, a
 *    diagram, a table or a quote as a whole);
 *  - a click on it starts the correction, the caret where the click was (mdeInlineEdit.start): type,
 *    Enter or a click elsewhere saves, Esc puts it back, as from the right-click menu;
 *  - press on a list item and move: the item is dragged (slide-list-drag.js, mdeListDrag.begin) and
 *    dropped where the line shows. The two gestures start the same way, so a distance decides:
 *    up to THRESHOLD pixels it is a click, farther it is a drag. Chrome sends the `click` even after
 *    a longer move when the button goes up on the same element (measured, sprint F0), so the
 *    distance is measured on the pointer's events, and the click that follows a move is swallowed —
 *    otherwise dropping an item would start a correction;
 *  - a link is not followed (a click corrects the text around it); capture on window comes before
 *    slide-navigation.js's handler;
 *  - the fragments not yet shown are all shown (CSS in slide-edit-mode.css; not
 *    Reveal.configure({fragments}), which loses the state of the ones already shown, measured), so
 *    they can be outlined and dragged. reveal.js's own state is not touched: leaving the mode
 *    brings the slide back as it was.
 *
 * While a correction is in progress the page is left to inline-edit.js: its click outside (a mousedown)
 * saves it, so pointerdown must not be cancelled then.
 *
 * With the mode off none of this exists: the page is the one of a deck without the bar.
 *
 * Sprint: docs-internal/Sprints/2026-09-30-Slide-Barra-Strumenti.md
 */
(function () {
    'use strict';

    var HOVER = 'mde-edit-hover';
    /** Farther than this the press is a drag, not a click. */
    var THRESHOLD = 5;
    var EDIT_MODE = 'mde-edit-mode';

    // No bar: not MdExplorer's view (a detached window, print, the page alone), where nothing here applies.
    if (!window.mdeSlideToolbar) return;
    if (!window.mdeInlineEdit || !window.mdeListDrag) {
        // The bar is there without what the mode needs: say it, do not leave a button that does nothing.
        console.error('[slide-edit-mode] inline-edit.js or slide-list-drag.js is not loaded: "Modifica veloce" does nothing.');
        return;
    }

    var press = null;          // {x, y, block, item, moved}
    var swallowClick = false;  // the click that follows a move is not a click

    function inMode() {
        return document.body.classList.contains(EDIT_MODE);
    }

    function correcting() {
        return document.body.hasAttribute('data-mde-inline-editing');
    }

    /** What belongs to the page's own controls, not to the slide's content: the bar, the handle, menus, the legend, notices. */
    function isControl(target) {
        return !!(target.closest && target.closest('#mde-slide-toolbar, .mde-list-handle, .mde-list-drop, #mde-slide-edit-menu, #mark-diagram-menu, .mde-slide-legend, .mde-inline-edit-toast'));
    }

    // ---- outline under the pointer ----

    var outlined = null;

    function outline(block) {
        if (block === outlined) return;
        if (outlined) outlined.classList.remove(HOVER);
        outlined = block;
        if (outlined) outlined.classList.add(HOVER);
    }

    document.addEventListener('mouseover', function (event) {
        if (!inMode() || press || correcting() || isControl(event.target)) {
            outline(null);
            return;
        }
        outline(window.mdeInlineEdit.blockAt(event.target));
    });

    document.documentElement.addEventListener('mouseleave', function () { outline(null); });
    if (window.Reveal) {
        Reveal.on('slidechanged', function () { outline(null); });
        Reveal.on('overviewshown', function () { outline(null); });
    }

    // ---- click to correct, press and move to drag ----

    window.addEventListener('pointerdown', function (event) {
        press = null;
        if (!inMode() || event.button !== 0 || correcting() || isControl(event.target)) return;
        var block = window.mdeInlineEdit.blockAt(event.target);
        var item = window.mdeListDrag.itemAt(event.target);
        if (!block && !item) return;
        // Not a text selection, not the browser's own drag of a link; and the keys to this page (Esc).
        event.preventDefault();
        try { window.focus(); } catch (e) { /* the keys stay where they were */ }
        press = { x: event.clientX, y: event.clientY, block: block, item: item, moved: false };
        outline(null);
    }, true);

    window.addEventListener('pointermove', function (event) {
        if (!press || press.moved) return;
        // The button went up where this page could not see it.
        if (event.buttons === 0) { press = null; return; }
        if (Math.hypot(event.clientX - press.x, event.clientY - press.y) < THRESHOLD) return;
        press.moved = true;
        swallowClick = true;
        // An item goes with the pointer; anything else that moves is neither a click nor a drag.
        if (press.item) window.mdeListDrag.begin(press.item, event);
    }, true);

    window.addEventListener('pointerup', function (event) {
        var pressed = press;
        press = null;
        if (!pressed || pressed.moved) return;
        // A click: the correction starts where it was made. What was outlined may have changed since.
        var block = window.mdeInlineEdit.blockAt(event.target) || pressed.block;
        if (block && block.isConnected) window.mdeInlineEdit.start(block, event.clientX, event.clientY);
    }, true);

    window.addEventListener('pointercancel', function () { press = null; }, true);

    // ---- clicks ----

    window.addEventListener('click', function (event) {
        if (!inMode()) { swallowClick = false; return; }
        if (swallowClick) {
            swallowClick = false;
            event.preventDefault();
            event.stopPropagation();
            return;
        }
        // A link is not followed — also when its click is the one that started the correction (it started on
        // the pointer's release, before this click arrives) or lands in the block being corrected.
        if (!isControl(event.target) && event.target.closest && event.target.closest('a[href]')) {
            event.preventDefault();
            event.stopPropagation();
        }
    }, true);

    // The move ended without a click after it (released elsewhere): the next real click is a click.
    window.addEventListener('pointerup', function () {
        setTimeout(function () { swallowClick = false; }, 0);
    });

    // ---- the mode goes off ----

    window.addEventListener('mde-edit-mode', function (event) {
        if (event.detail && event.detail.on) return;
        press = null;
        swallowClick = false;
        outline(null);
    });
})();
