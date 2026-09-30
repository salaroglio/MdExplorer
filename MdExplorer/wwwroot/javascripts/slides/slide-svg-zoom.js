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
        return svg._interactiveSvgData || svg._sequenceData || svg._yamlData || null;
    }

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

    function refresh() {
        if (!current) return;
        reset.textContent = Math.round(levelOf(current) * 100) + '%';
        place();
    }

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
