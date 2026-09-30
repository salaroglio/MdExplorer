/**
 * MdExplorer - Zoom buttons on the diagrams of a slide
 * =====================================================
 * A diagram of a slide (class, sequence, YAML tree: the interactive-svg scripts) has always been zoomed with
 * Ctrl + wheel, which nothing on the page says. Hover it and a small bar shows above it: − , the zoom (a click on it
 * goes back to the original size) and +.
 *
 * It does not zoom by itself: it sends the diagram's own Ctrl + wheel event, so the three scripts keep their
 * zoom (same step, same limits, same care of the point being watched) and their state (data.zoomLevel of
 * svg._interactiveSvgData, _sequenceData or _yamlData). Going back to the original size is the same number of
 * steps the other way: the scripts move by 0.2 from 1.0, so it lands on 1.0 exactly.
 *
 * The diagrams none of the three scripts takes (activity, mindmap, gantt…) had no zoom at all in a slide: in a
 * document core/init.js gives them a generic one, and the slide page does not load it (it is jQuery's). Here
 * they get the same zoom (same step and limits, state in svg._genericZoomPan as there), so the bar is on
 * every diagram of the deck.
 *
 * Documents have their own bar over a diagram (image-transform.js); the slides' had none, by choice of the
 * first sprint (Slide-SVG-Interattivi): the bar is tied to jQuery and to the document's layout.
 *
 * Sprint: docs-internal/Sprints/2026-09-30-Slide-Barra-Strumenti.md
 */
(function () {
    'use strict';

    if (!window.Reveal) return;
    if (typeof _toolbarText !== 'function') {
        console.error('[slide-svg-zoom] toolbar-shared.js is not loaded: no texts, no zoom buttons.');
        return;
    }

    var STEP = 0.2;
    var HIDE_MS = 350;

    var bar = document.createElement('div');
    bar.className = 'mde-img-toolbar mde-svg-zoom';
    bar.hidden = true;

    function button(text, titleKey) {
        var b = document.createElement('button');
        b.type = 'button';
        b.textContent = text;
        b.title = _toolbarText(titleKey);
        return b;
    }

    var out = button('\u2212', 'svg.zoomOut');
    var reset = button('100%', 'svg.zoomReset');
    reset.className = 'mde-svg-zoom-level';
    var zoomIn = button('+', 'svg.zoomIn');
    [out, reset, zoomIn].forEach(function (b) { bar.appendChild(b); });
    document.body.appendChild(bar);

    var current = null;
    var hideTimer = null;

    function isPrint() {
        return /print-pdf|view=print/.test(window.location.search);
    }

    /** The zoom state of a diagram, kept by whichever script drew it; null when it is not one of them. */
    function stateOf(svg) {
        return svg._interactiveSvgData || svg._sequenceData || svg._yamlData || svg._genericZoomPan || null;
    }

    /**
     * The zoom of a diagram no script took, as core/init.js gives it in a document: Ctrl + wheel, 0.2 a step,
     * between 0.2 and 5. The base is the size in CSS pixels (the slide is scaled by reveal.js).
     */
    function giveGenericZoom(svg) {
        var state = svg._genericZoomPan = { zoomLevel: 1.0, zoomBaseW: null, zoomBaseH: null };
        svg.addEventListener('wheel', function (event) {
            if (!event.ctrlKey) return;
            event.preventDefault();
            if (!state.zoomBaseW) {
                var rect = svg.getBoundingClientRect();
                var pageScale = Reveal.getScale() || 1;
                state.zoomBaseW = rect.width / pageScale;
                state.zoomBaseH = rect.height / pageScale;
            }
            state.zoomLevel = Math.max(0.2, Math.min(5.0, state.zoomLevel + (event.deltaY < 0 ? 1 : -1) * STEP));
            // Float noise (0.2 * 3) would show as 60.00000000000001%.
            state.zoomLevel = Math.round(state.zoomLevel * 10) / 10;
            svg.style.maxWidth = 'none';
            svg.style.width = Math.round(state.zoomBaseW * state.zoomLevel) + 'px';
            svg.style.height = Math.round(state.zoomBaseH * state.zoomLevel) + 'px';
        }, { passive: false });
    }

    // After the diagram scripts have taken theirs (slide-diagrams.js registered on 'ready' before this file).
    Reveal.on('ready', function () {
        var any = false;
        document.querySelectorAll('.slides svg[data-diagram-type]').forEach(function (svg) {
            if (stateOf(svg)) return;
            giveGenericZoom(svg);
            any = true;
        });
        // Ctrl + wheel beside the diagram must not zoom the whole page (the three scripts do the same for theirs).
        if (any) window.addEventListener('wheel', function (event) { if (event.ctrlKey) event.preventDefault(); }, { passive: false });
    });

    function levelOf(svg) {
        var state = stateOf(svg);
        return (state && state.zoomLevel) || 1;
    }

    function zoomableAt(target) {
        var el = target && target.nodeType === Node.ELEMENT_NODE ? target : (target && target.parentElement);
        var svg = el && el.closest && el.closest('svg');
        return svg && stateOf(svg) && svg.closest('.slides') ? svg : null;
    }

    /** Where the diagram is on screen: its centre, within the window (a zoomed one is larger than it). */
    function visibleCentre(svg) {
        var rect = svg.getBoundingClientRect();
        var left = Math.max(rect.left, 0), right = Math.min(rect.right, window.innerWidth);
        var top = Math.max(rect.top, 0), bottom = Math.min(rect.bottom, window.innerHeight);
        return { x: (left + right) / 2, y: (top + bottom) / 2 };
    }

    /** One step in (+1) or out (-1): the diagram's own Ctrl + wheel, so its script does the zoom. */
    function step(svg, direction) {
        var centre = visibleCentre(svg);
        svg.dispatchEvent(new WheelEvent('wheel', {
            ctrlKey: true, deltaY: direction > 0 ? -100 : 100, clientX: centre.x, clientY: centre.y, bubbles: true, cancelable: true
        }));
    }

    /**
     * The pointer is on the bar: the bar stays where it is, whatever the diagram does under it. It follows the
     * diagram's corner, and each click changes the diagram's size: moved at once, the next click without moving
     * the mouse landed on another button, or on nothing (and the bar, left behind, went away).
     */
    var overBar = false;

    function refresh() {
        if (!current) return;
        reset.textContent = Math.round(levelOf(current) * 100) + '%';
        if (!overBar) place();
    }

    bar.addEventListener('mouseenter', function () { overBar = true; });
    bar.addEventListener('mouseleave', function () { overBar = false; if (current) place(); });

    function place() {
        if (!current) return;
        var rect = current.getBoundingClientRect();
        var box = bar.getBoundingClientRect();
        var gap = 6;
        // Out of the diagram, at its right edge: over a small diagram the bar would sit on its boxes and take their clicks.
        // Above it if there is room, else below; only when it is larger than the window, inside its corner.
        var left = Math.max(8, Math.min(rect.right, window.innerWidth - 8) - box.width);
        var top;
        if (rect.top - box.height - gap >= 8) {
            top = rect.top - box.height - gap;
        } else if (rect.bottom + gap + box.height <= window.innerHeight - 8) {
            top = rect.bottom + gap;
        } else {
            top = Math.max(8, Math.min(Math.max(rect.top, 0) + 8, window.innerHeight - box.height - 8));
        }
        bar.style.left = left + 'px';
        bar.style.top = top + 'px';
    }

    function show(svg) {
        cancelHide();
        if (current === svg && !bar.hidden) return;
        current = svg;
        bar.hidden = false;
        refresh();
    }

    function hide() {
        cancelHide();
        current = null;
        overBar = false;
        bar.hidden = true;
    }

    function cancelHide() {
        clearTimeout(hideTimer);
        hideTimer = null;
    }

    function scheduleHide() {
        if (hideTimer || bar.hidden) return;
        hideTimer = setTimeout(hide, HIDE_MS);
    }

    document.addEventListener('mouseover', function (event) {
        if (isPrint()) return;
        if (bar.contains(event.target)) { cancelHide(); return; }
        var svg = zoomableAt(event.target);
        if (svg) show(svg);
        else scheduleHide();
    });

    document.documentElement.addEventListener('mouseleave', hide);

    out.addEventListener('click', function () { if (current) { step(current, -1); refresh(); } });
    zoomIn.addEventListener('click', function () { if (current) { step(current, 1); refresh(); } });
    reset.addEventListener('click', function () {
        if (!current) return;
        var steps = Math.round((levelOf(current) - 1) / STEP);
        for (var i = 0; i < Math.abs(steps); i++) step(current, steps > 0 ? -1 : 1);
        refresh();
    });

    // What the page does around the diagram: a click on a button is not a click on the diagram (or the slide).
    bar.addEventListener('mousedown', function (event) {
        event.preventDefault();
        event.stopPropagation();
        try { window.focus(); } catch (e) { /* the keys stay where they were */ }
    });

    // Ctrl + wheel on the diagram itself: the level changed under the bar.
    document.addEventListener('wheel', function (event) {
        if (event.ctrlKey && current) setTimeout(refresh, 0);
    }, true);

    Reveal.on('slidechanged', hide);
    Reveal.on('overviewshown', hide);
    window.addEventListener('resize', function () { if (current) refresh(); });
})();
