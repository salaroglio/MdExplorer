/**
 * MdExplorer - Floating horizontal scrollbar for wide markdown tables
 * ===================================================================
 * Problem: a table wider than the viewport gets a native horizontal scrollbar
 * pinned to the table's BOTTOM edge. On a tall table that bottom is far below
 * the fold, so while reading the top rows the scrollbar is unreachable.
 *
 * Solution: a single CUSTOM scrollbar (our own track+thumb, not the browser's,
 * so it never auto-hides and is theme-aware) fixed to the iframe viewport
 * bottom. It drives table.scrollLeft of the tall+wide table currently most in
 * view. Only tables TALLER than the viewport are managed by the floating bar;
 * short wide tables keep their native (reachable) bottom scrollbar.
 *
 * Anti-blinking: the thumb is a pure display element and table.scrollLeft is
 * the single source of truth, so there is no scroll<->scroll feedback loop.
 * Native bars are hidden once (never toggled per-scroll) to avoid flicker.
 *
 * Fallback: the native bar is hidden only on tables this script manages
 * (class mde-hscroll-managed). If the script never runs, native bars remain.
 */
(function () {
    if (window.__mdeTableHScrollLoaded) return;
    window.__mdeTableHScrollLoaded = true;

    var SELECTOR = '.mdeItemMainPageCenter table.table';
    var MIN_THUMB = 24;

    var bar = null, thumb = null;
    var tables = [];      // all markdown tables currently on the page
    var active = null;    // the wide table the bar currently drives
    var rafPending = false;

    function isWide(t) { return (t.scrollWidth - t.clientWidth) > 1; }
    function viewportH() { return window.innerHeight || document.documentElement.clientHeight; }
    function isTall(t) { return t.getBoundingClientRect().height > viewportH(); }
    // The floating bar only manages tables TALLER than the viewport, whose native
    // bottom scrollbar is unreachable while reading the top rows. Short wide tables
    // keep their native bar (reachable at their own bottom), so when several wide
    // tables share the screen each stays scrollable and the floating bar — which
    // can drive only one at a time — never leaves a visible table without control.
    function isManaged(t) { return isWide(t) && isTall(t); }
    function scrollRange(t) { return t.scrollWidth - t.clientWidth; }

    // ---- bar / thumb DOM ------------------------------------------------
    function buildBar() {
        if (bar) return;
        bar = document.createElement('div');
        bar.className = 'mde-hscroll';
        thumb = document.createElement('div');
        thumb.className = 'mde-hscroll__thumb';
        bar.appendChild(thumb);
        document.body.appendChild(bar);
        wireBar();
    }

    function hideBar() { if (bar) bar.style.display = 'none'; }

    // Place the bar horizontally under the active table, clamped to viewport.
    function layoutBar() {
        if (!active) { hideBar(); return; }
        var r = active.getBoundingClientRect();
        var vw = document.documentElement.clientWidth;
        var left = Math.max(0, r.left);
        var right = Math.min(vw, r.right);
        var w = right - left;
        if (w <= 0) { hideBar(); return; }
        bar.style.left = left + 'px';
        bar.style.width = w + 'px';
        bar.style.display = 'block';
        layoutThumb();
    }

    // Size + position the thumb to mirror active.scrollLeft.
    function layoutThumb() {
        if (!active || bar.style.display === 'none') return;
        var barW = bar.clientWidth;
        var ratio = active.clientWidth / active.scrollWidth;
        var tw = Math.max(MIN_THUMB, Math.floor(ratio * barW));
        var maxLeft = barW - tw;
        var sr = scrollRange(active);
        var tl = sr > 0 ? (active.scrollLeft / sr) * maxLeft : 0;
        thumb.style.width = tw + 'px';
        thumb.style.left = Math.round(Math.max(0, Math.min(maxLeft, tl))) + 'px';
    }

    // ---- choose the wide table that is most visible ---------------------
    function pickActive() {
        var vh = document.documentElement.clientHeight;
        var best = null, bestVis = 0;
        for (var i = 0; i < tables.length; i++) {
            var t = tables[i];
            if (!isManaged(t)) continue;
            var r = t.getBoundingClientRect();
            var vis = Math.min(r.bottom, vh) - Math.max(r.top, 0);
            if (vis > bestVis) { bestVis = vis; best = t; }
        }
        return bestVis > 0 ? best : null;
    }

    function refresh() {
        rafPending = false;
        active = pickActive();
        if (active) layoutBar(); else hideBar();
    }

    function scheduleRefresh() {
        if (rafPending) return;
        rafPending = true;
        window.requestAnimationFrame(refresh);
    }

    // ---- interactions on the bar ----------------------------------------
    function scrollTo(clientX, thumbCenter) {
        if (!active) return;
        var tw = thumb.clientWidth;
        var maxLeft = bar.clientWidth - tw;
        if (maxLeft <= 0) return;
        var rect = bar.getBoundingClientRect();
        var target = (clientX - rect.left) - (thumbCenter ? tw / 2 : 0);
        target = Math.max(0, Math.min(maxLeft, target));
        active.scrollLeft = (target / maxLeft) * scrollRange(active);
        layoutThumb();
    }

    function wireBar() {
        var dragging = false, startX = 0, startScroll = 0;

        thumb.addEventListener('mousedown', function (e) {
            if (e.button !== 0 || !active) return;
            dragging = true;
            startX = e.clientX;
            startScroll = active.scrollLeft;
            thumb.classList.add('dragging');
            e.preventDefault();
            e.stopPropagation();
        });

        document.addEventListener('mousemove', function (e) {
            if (!dragging || !active) return;
            var tw = thumb.clientWidth;
            var maxLeft = bar.clientWidth - tw;
            if (maxLeft <= 0) return;
            var dScroll = ((e.clientX - startX) / maxLeft) * scrollRange(active);
            active.scrollLeft = startScroll + dScroll;
            layoutThumb();
            e.preventDefault();
        });

        document.addEventListener('mouseup', function () {
            if (!dragging) return;
            dragging = false;
            thumb.classList.remove('dragging');
        });

        // Click on the track (not the thumb) jumps the thumb centre there.
        bar.addEventListener('mousedown', function (e) {
            if (e.target === thumb || !active) return;
            scrollTo(e.clientX, true);
        });

        // Wheel over the bar scrolls the active table horizontally.
        bar.addEventListener('wheel', function (e) {
            if (!active) return;
            active.scrollLeft += (e.deltaY || e.deltaX);
            layoutThumb();
            e.preventDefault();
        }, { passive: false });
    }

    // ---- discovery + observers ------------------------------------------
    var ro = ('ResizeObserver' in window) ? new ResizeObserver(scheduleRefresh) : null;

    function scan() {
        var found = Array.prototype.slice.call(document.querySelectorAll(SELECTOR));
        found.forEach(function (t) {
            if (!t.__mdeHScrollBound) {
                t.__mdeHScrollBound = true;
                t.addEventListener('scroll', function () {
                    if (t === active) layoutThumb();
                }, { passive: true });
                if (ro) ro.observe(t);
            }
            // Hide the native far-bottom bar only on tall wide tables (the ones the
            // floating bar drives). Toggled here in scan (load/resize), never per
            // scroll, so no flicker.
            if (isManaged(t)) t.classList.add('mde-hscroll-managed');
            else t.classList.remove('mde-hscroll-managed');
        });
        tables = found;
        scheduleRefresh();
    }

    function init() {
        if (!document.querySelector('.mdeItemMainPageCenter')) return;
        buildBar();
        scan();
        window.addEventListener('scroll', scheduleRefresh, { passive: true });
        // Resize changes the viewport height -> re-evaluate which tables are "tall".
        window.addEventListener('resize', scan);
        // Re-scan after late layout (images/fonts inside tables changing width).
        window.addEventListener('load', scan);
    }

    // Public hook (e.g. if content is injected dynamically).
    window.MdeTableHScroll = { init: init, rescan: scan };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
