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
 * The eye shows the diagram on the whole page: the svg itself is moved into a layer over the page (its
 * listeners go with it, so a click on a box still lights its links) and sized to fit the window; the eye again,
 * or Esc, puts it back in its slide at its original size. The bar of the deck stays above the layer, so the
 * annotations (slide-ink.js) can be drawn on the large diagram: they are kept apart from the slide's own, and
 * relative to the diagram. While it is on, reveal.js leaves the keys alone (the layer holds them). The zoom works
 * there too (buttons and Ctrl + wheel, 100% = the diagram filling the window) — its own, not the scripts', whose
 * sizes are the slide's; larger than the window, the diagram is dragged or scrolled with the wheel.
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
    // A diagram on the whole page, and back.
    var eye = button('\uD83D\uDC41\uFE0F', 'svg.fullPage');
    eye.className = 'mde-svg-zoom-eye';
    [out, reset, zoomIn, eye].forEach(function (b) { bar.appendChild(b); });
    document.body.appendChild(bar);

    /** The diagram shown on the whole page: {svg, placeholder, cssText, layer, observer}; null when none is. */
    var full = null;

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

    function resetZoom(svg) {
        var steps = Math.round((levelOf(svg) - 1) / STEP);
        for (var i = 0; i < Math.abs(steps); i++) step(svg, steps > 0 ? -1 : 1);
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
        if (!current || full) return;
        reset.textContent = Math.round(levelOf(current) * 100) + '%';
        if (!overBar) place();
    }

    bar.addEventListener('mouseenter', function () { overBar = true; });
    bar.addEventListener('mouseleave', function () { overBar = false; if (current && !full) place(); });

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
        if (full) return;   // pinned while a diagram is on the whole page
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
        if (isPrint() || full) return;
        if (bar.contains(event.target)) { cancelHide(); return; }
        var svg = zoomableAt(event.target);
        if (svg) show(svg);
        else scheduleHide();
    });

    document.documentElement.addEventListener('mouseleave', hide);

    out.addEventListener('click', function () {
        if (full) zoomFull(-1);
        else if (current) { step(current, -1); refresh(); }
    });
    zoomIn.addEventListener('click', function () {
        if (full) zoomFull(1);
        else if (current) { step(current, 1); refresh(); }
    });
    reset.addEventListener('click', function () {
        if (full) { zoomFull(0); return; }
        if (!current) return;
        resetZoom(current);
        refresh();
    });

    // ---- the diagram on the whole page ----

    /** Room left around the diagram: the deck's bar is at the top. */
    var FULL_MARGIN = { top: 56, side: 24, bottom: 24 };

    /** The size at which the diagram fills the window, keeping its shape: its 100% while it is on the whole page. */
    function fitSize(svg) {
        var view = svg.viewBox && svg.viewBox.baseVal;
        var w = view && view.width > 0 ? view.width : svg.getBoundingClientRect().width;
        var h = view && view.height > 0 ? view.height : svg.getBoundingClientRect().height;
        if (!(w > 0 && h > 0)) return null;
        var scale = Math.min((window.innerWidth - 2 * FULL_MARGIN.side) / w, (window.innerHeight - FULL_MARGIN.top - FULL_MARGIN.bottom) / h);
        return { w: w * scale, h: h * scale };
    }

    /**
     * The diagram at its size on the whole page: the one that fills the window, times the zoom. The point of the
     * diagram under the anchor (the pointer for the wheel, the middle of the window for the buttons) stays there:
     * larger than the window, the layer scrolls.
     */
    function fit(anchor) {
        if (!full) return;
        var svg = full.svg, layer = full.layer;
        var size = fitSize(svg);
        if (!size) return;
        var ax = anchor ? anchor.x : window.innerWidth / 2, ay = anchor ? anchor.y : window.innerHeight / 2;
        var before = svg.getBoundingClientRect();
        var fx = before.width > 0 ? (ax - before.left) / before.width : 0.5;
        var fy = before.height > 0 ? (ay - before.top) / before.height : 0.5;
        var width = Math.max(1, Math.round(size.w * full.level)) + 'px', height = Math.max(1, Math.round(size.h * full.level)) + 'px';
        // Written only when it changes: the observer below watches the style.
        if (svg.style.width !== width || svg.style.height !== height || svg.style.maxWidth !== 'none' || svg.style.maxHeight !== 'none') {
            svg.style.maxWidth = 'none';
            svg.style.maxHeight = 'none';
            svg.style.width = width;
            svg.style.height = height;
            var after = svg.getBoundingClientRect();
            layer.scrollLeft += (after.left + fx * after.width) - ax;
            layer.scrollTop += (after.top + fy * after.height) - ay;
        }
        reset.textContent = Math.round(full.level * 100) + '%';
        notifyFull();
    }

    /** Whoever draws over the diagram (slide-ink.js) follows its new place and size. */
    function notifyFull() {
        window.dispatchEvent(new CustomEvent('mde-svg-fullpage', { detail: { on: !!full } }));
    }

    /** The zoom while on the whole page: the same step and limits as in the slide. */
    function zoomFull(direction, anchor) {
        if (!full) return;
        var level = direction === 0 ? 1 : full.level + direction * STEP;
        full.level = Math.round(Math.max(0.2, Math.min(5.0, level)) * 10) / 10;
        fit(anchor);
    }

    function enterFull(svg) {
        if (full || !svg) return;
        // Back in the slide it must be at its original size: the zoom goes to 100% first.
        resetZoom(svg);
        var slide = Reveal.getCurrentSlide();
        var index = slide ? Array.prototype.indexOf.call(slide.querySelectorAll('svg[data-diagram-type]'), svg) : 0;
        var placeholder = document.createComment('mde-svg-fullpage');
        svg.parentNode.insertBefore(placeholder, svg);

        var layer = document.createElement('div');
        layer.className = 'mde-svg-fullpage';
        // reveal.js leaves the keys alone while this is here (slide-diagrams.js): the arrows do not change the slide under it.
        layer.setAttribute('data-mde-holds-keys', '');
        // The annotations drawn on it are its own, not the slide's (slide-ink.js).
        layer.setAttribute('data-mde-ink-key', 'diagram' + Math.max(0, index));
        var background = window.getComputedStyle(document.body).backgroundColor;
        layer.style.background = !background || /rgba\(0, 0, 0, 0\)|transparent/.test(background) ? '#fff' : background;
        // Ctrl + wheel zooms here too, around the pointer — this zoom, not the script's (its sizes are the slide's).
        // The wheel alone scrolls the layer, when the diagram is larger than the window.
        layer.addEventListener('wheel', function (event) {
            if (!event.ctrlKey) return;
            event.preventDefault();
            event.stopPropagation();
            zoomFull(event.deltaY < 0 ? 1 : -1, { x: event.clientX, y: event.clientY });
        }, { capture: true, passive: false });
        layer.addEventListener('scroll', notifyFull);
        panByDrag(layer);

        full = { svg: svg, placeholder: placeholder, cssText: svg.style.cssText, layer: layer, observer: null, level: 1 };
        layer.appendChild(svg);
        document.body.appendChild(layer);
        document.body.classList.add('mde-svg-fullpage-on');

        // A tree that folds (YAML, JSON) is resized by its script: fitted again.
        if (window.MutationObserver) {
            full.observer = new MutationObserver(function () { fit(); });
            full.observer.observe(svg, { attributes: true, attributeFilter: ['style', 'viewBox', 'width', 'height'] });
        }

        cancelHide();
        current = svg;
        overBar = false;
        eye.classList.add('active');
        eye.title = _toolbarText('svg.fullPageExit');
        bar.classList.add('mde-svg-zoom-pinned');
        bar.hidden = false;
        bar.style.left = '12px';
        bar.style.top = '8px';
        fit();
    }

    function exitFull() {
        if (!full) return;
        var was = full;
        full = null;
        if (was.observer) was.observer.disconnect();
        was.svg.style.cssText = was.cssText;
        if (was.placeholder.parentNode) {
            was.placeholder.parentNode.insertBefore(was.svg, was.placeholder);
            was.placeholder.remove();
        }
        was.layer.remove();
        document.body.classList.remove('mde-svg-fullpage-on');
        // reveal.js centres a slide by its content: laid out while the diagram was away (a window resized), the
        // slide was centred without it, and the diagram came back lower than it was (measured: 205 px).
        Reveal.layout();
        eye.classList.remove('active');
        eye.title = _toolbarText('svg.fullPage');
        bar.classList.remove('mde-svg-zoom-pinned');
        hide();
        notifyFull();
    }

    /**
     * Press and move on the large diagram: it is dragged under the pointer (the layer scrolls). A press without a
     * move stays a click — the diagram's own scripts light a box on it, and drop the click that ends a drag.
     */
    function panByDrag(layer) {
        var from = null, dragged = false;
        layer.addEventListener('mousedown', function (event) {
            if (event.button !== 0) return;
            from = { x: event.clientX, y: event.clientY, left: layer.scrollLeft, top: layer.scrollTop };
            dragged = false;
        });
        document.addEventListener('mousemove', function (event) {
            if (!from || !full || full.layer !== layer) return;
            if (event.buttons === 0) { from = null; layer.classList.remove('mde-svg-fullpage-panning'); return; }
            var dx = event.clientX - from.x, dy = event.clientY - from.y;
            if (!dragged && Math.hypot(dx, dy) < 4) return;
            dragged = true;
            layer.classList.add('mde-svg-fullpage-panning');
            layer.scrollLeft = from.left - dx;
            layer.scrollTop = from.top - dy;
        });
        document.addEventListener('mouseup', function () {
            from = null;
            layer.classList.remove('mde-svg-fullpage-panning');
        });
    }

    eye.addEventListener('click', function () {
        if (full) exitFull();
        else if (current) enterFull(current);
    });

    // Esc puts it back — after whoever else is waiting for it (the annotations close first, a menu, a correction).
    window.addEventListener('keydown', function (event) {
        if (event.key !== 'Escape' || !full || event.defaultPrevented) return;
        if (document.body.classList.contains('mde-ink-active')) return;
        if (document.querySelector('[data-mde-holds-keys]:not(.mde-svg-fullpage)') || document.body.hasAttribute('data-mde-inline-editing')) return;
        event.preventDefault();
        event.stopPropagation();
        exitFull();
    }, true);

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

    Reveal.on('slidechanged', function () { exitFull(); hide(); });
    Reveal.on('overviewshown', function () { exitFull(); hide(); });
    window.addEventListener('resize', function () {
        if (full) fit();
        else if (current) refresh();
    });
})();
