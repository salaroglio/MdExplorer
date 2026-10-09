/**
 * MdExplorer - Scroll chaining out of a table's own scroll box
 * ===========================================================
 * Tables taller than the viewport become a box with an internal scrollbar
 * (`max-height: 75vh; overflow: auto` in MdCustomCSS.css). Reaching the bottom
 * of such a box with the wheel used to stop the whole page: the document behind
 * the table stayed put until the user lifted their fingers off the wheel and
 * started a new gesture.
 *
 * That is Chrome's *scroll latching*: the scroller under the cursor when a wheel
 * gesture begins keeps every scroll update of that gesture, even after it has
 * hit its own end — deliberate, so a fast flick inside a small box cannot run
 * away with the page. For a table that fills most of the screen the effect is
 * the opposite of helpful: the reader is stuck.
 *
 * Measured (Chrome headless, the viewer's own CSS, 2026-09-07): one continuous
 * gesture drove the table from 0 to its last pixel (6755) and left the document
 * at 86; only a *second* gesture, after the pause, moved the document at all.
 *
 * So when the box is already at the end in the direction of the wheel, this
 * module cancels the (latched, useless) default and scrolls the nearest
 * ancestor that can still move. One delegated listener on the document, so
 * tables rendered later are covered without re-registering anything.
 */
(function () {
    if (window.__mdeTableScrollChainLoaded) return;
    window.__mdeTableScrollChainLoaded = true;

    var TABLE_SELECTOR = '.mdeItemMainPageCenter table.table';
    var EDGE_TOLERANCE = 1; // sub-pixel layout: scrollTop rarely lands exactly on the end

    // Wheel deltas are not always pixels. Chromium reports pixels (mode 0), but
    // the other modes exist and would otherwise scroll by a couple of pixels.
    function deltaInPixels(e, box) {
        if (e.deltaMode === 1) return e.deltaY * 16;            // lines
        if (e.deltaMode === 2) return e.deltaY * box.clientHeight; // pages
        return e.deltaY;
    }

    function canScrollVertically(el) {
        if (!el || el === document.documentElement || el === document.body) return false;
        if (el.scrollHeight - el.clientHeight <= EDGE_TOLERANCE) return false;
        var overflowY = window.getComputedStyle(el).overflowY;
        return overflowY === 'auto' || overflowY === 'scroll' || overflowY === 'overlay';
    }

    function hasRoom(el, direction) {
        return direction > 0
            ? el.scrollTop + el.clientHeight < el.scrollHeight - EDGE_TOLERANCE
            : el.scrollTop > EDGE_TOLERANCE;
    }

    // The element that should take over: the first ancestor still able to move
    // that way, otherwise the document itself.
    function scrollerToTakeOver(table, direction) {
        for (var el = table.parentElement; el; el = el.parentElement) {
            if (canScrollVertically(el) && hasRoom(el, direction)) return el;
        }
        var page = document.scrollingElement || document.documentElement;
        return hasRoom(page, direction) ? page : null;
    }

    document.addEventListener('wheel', function (e) {
        // Someone else already claimed this wheel (ctrl+wheel zooms a diagram).
        if (e.defaultPrevented || e.ctrlKey) return;

        var target = e.target;
        var table = target && target.closest ? target.closest(TABLE_SELECTOR) : null;
        if (!table) return;

        // A sideways gesture belongs to the table's horizontal scrollbar.
        if (Math.abs(e.deltaY) <= Math.abs(e.deltaX)) return;

        // Not a scroll box (short table): the browser chains on its own.
        if (table.scrollHeight - table.clientHeight <= EDGE_TOLERANCE) return;

        var delta = deltaInPixels(e, table);
        if (!delta) return;

        // Still room inside the table: leave it to the browser.
        if (hasRoom(table, delta)) return;

        var scroller = scrollerToTakeOver(table, delta);
        if (!scroller) return; // nothing left to scroll: don't swallow the event

        e.preventDefault();
        scroller.scrollTop += delta;
    }, { passive: false });
})();
