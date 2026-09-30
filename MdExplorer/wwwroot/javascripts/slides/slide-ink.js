/**
 * MdExplorer - Annotating the slides while presenting
 * ====================================================
 * In "Presenta" the bar has one more button, 🖍: it opens a small panel — pen, highlighter, eraser, a palette of
 * six colours, "Azzera" — and the slide can be drawn on. Nothing is written in the .md: it is what a presenter
 * does with a pointer, on the slide being shown.
 *
 *  - The strokes belong to a SLIDE (its indices, h/v), kept in memory until the page is reloaded, and are
 *    stored relative to the slide's area (0..1 of the box reveal.js scales), so they follow a window that
 *    is resized or a screen that is not the one they were drawn on. Drawn on one canvas over the page, repainted
 *    when the slide or the size changes.
 *  - Pen and highlighter draw; the eraser takes away the whole stroke it touches (a stroke is what was drawn
 *    without lifting the pointer); "Azzera" takes away the annotations of the slide on screen.
 *  - From the keyboard too, while presenting: A annotations on/off, E eraser, C clear the slide. Those are keys
 *    reveal.js does not use; nothing happens while something is being typed.
 *  - reveal.js keeps its arrows while drawing (the panel does not hold the keys): the strokes are the slide's,
 *    the presenter moves on. Esc closes the panel.
 *  - A diagram shown on the whole page (the eye of slide-svg-zoom.js) is a surface of its own: what is drawn on it
 *    stays with it, relative to the diagram, and is not on the slide when the diagram goes back.
 *  - With "Modifica" on the annotations are off and the button is not there: one thing at a time.
 *  - On an HTML page of the project shown in MdExplorer's view (no reveal.js) the surface is the whole document:
 *    the strokes are relative to it and scroll with it.
 *
 * Only inside MdExplorer's view, as the bar.
 *
 * Sprint: docs-internal/Sprints/2026-09-30-Slide-Barra-Strumenti.md
 */
(function () {
    'use strict';

    if (!window.mdeSlideToolbar) return;
    /** An HTML page of the project: no reveal.js, the document is the one surface. */
    var htmlPage = !window.Reveal && document.documentElement.hasAttribute('data-mde-html-page');
    if (!window.Reveal && !htmlPage) return;
    if (typeof _toolbarText !== 'function') {
        console.error('[slide-ink] toolbar-shared.js is not loaded: no texts, no annotations.');
        return;
    }

    var T = _toolbarText;
    var COLORS = [
        { name: 'yellow', value: '#ffd60a' },
        { name: 'red', value: '#ff3b30' },
        { name: 'green', value: '#34c759' },
        { name: 'blue', value: '#0a84ff' },
        { name: 'orange', value: '#ff9500' },
        { name: 'purple', value: '#af52de' }
    ];
    /** Stroke widths in slide pixels (the slide is 960 wide unless the deck says otherwise), scaled with the slide. */
    var WIDTH = { pen: 4, highlighter: 22 };
    var HIGHLIGHT_ALPHA = 0.38;
    /** How near the eraser has to pass to a stroke, in screen pixels. */
    var ERASER_RADIUS = 16;
    var COLOR_KEY = 'mdexplorer_slide_ink_color';
    var TOOL_KEY = 'mdexplorer_slide_ink_tool';
    var POSITION_KEY = 'mdexplorer_slide_ink_position';
    var MARGIN = 12;

    function remember(key, value) {
        try { window.localStorage.setItem(key, value); } catch (e) { /* no storage: it forgets, it works */ }
    }

    function recall(key) {
        try { return window.localStorage.getItem(key); } catch (e) { return null; }
    }

    var strokes = {};          // slide key -> [{tool, color, width, points: [[nx, ny], ...]}]
    var active = false;
    var tool = recall(TOOL_KEY) === 'highlighter' ? 'highlighter' : 'pen';
    var color = (COLORS.filter(function (c) { return c.value === recall(COLOR_KEY); })[0] || COLORS[1]).value;
    var drawing = null;        // the stroke being drawn
    var erasing = false;

    // ---- the slide: which one, and where ----

    /** A diagram shown on the whole page (slide-svg-zoom.js): what is drawn then is drawn on it, not on the slide. */
    function fullPageDiagram() {
        return document.querySelector('.mde-svg-fullpage');
    }

    function slideKey() {
        if (htmlPage) return 'page';
        var i = Reveal.getIndices();
        var key = i.h + '/' + (i.v || 0);
        var diagram = fullPageDiagram();
        return diagram ? key + '#' + (diagram.getAttribute('data-mde-ink-key') || 'diagram') : key;
    }

    function current() {
        var key = slideKey();
        return strokes[key] || (strokes[key] = []);
    }

    /** What the strokes are relative to: the box reveal.js scales, or the diagram shown on the whole page. */
    function box() {
        var diagram = fullPageDiagram();
        var svg = diagram && diagram.querySelector('svg');
        if (svg) return svg.getBoundingClientRect();
        if (htmlPage) {
            // The whole document, in the viewport's coordinates: what scrolls away has a negative top.
            var root = document.documentElement;
            return { left: -window.scrollX, top: -window.scrollY, width: Math.max(root.scrollWidth, root.clientWidth), height: Math.max(root.scrollHeight, root.clientHeight) };
        }
        return Reveal.getSlidesElement().getBoundingClientRect();
    }

    function scale() {
        return htmlPage ? 1 : (Reveal.getScale() || 1);
    }

    function toSlide(x, y) {
        var b = box();
        return [(x - b.left) / b.width, (y - b.top) / b.height];
    }

    function toScreen(point) {
        var b = box();
        return [b.left + point[0] * b.width, b.top + point[1] * b.height];
    }

    // ---- the canvas ----

    var canvas = document.createElement('canvas');
    canvas.id = 'mde-ink-canvas';
    canvas.className = 'mde-ink-canvas';
    document.body.appendChild(canvas);
    var ctx = canvas.getContext('2d');

    function fit() {
        var ratio = window.devicePixelRatio || 1;
        canvas.width = Math.round(window.innerWidth * ratio);
        canvas.height = Math.round(window.innerHeight * ratio);
        canvas.style.width = window.innerWidth + 'px';
        canvas.style.height = window.innerHeight + 'px';
        ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
    }

    function paint(stroke, upTo) {
        var pts = stroke.points;
        var count = upTo === undefined ? pts.length : upTo;
        if (!count) return;
        ctx.save();
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        ctx.strokeStyle = stroke.color;
        ctx.fillStyle = stroke.color;
        ctx.globalAlpha = stroke.tool === 'highlighter' ? HIGHLIGHT_ALPHA : 1;
        // A share of the surface's width: on a diagram zoomed on the whole page the stroke grows with what it marks.
        var width = stroke.widthShare * box().width;
        ctx.lineWidth = width;
        var first = toScreen(pts[0]);
        if (count === 1) {
            // A dot: a click without moving.
            ctx.beginPath();
            ctx.arc(first[0], first[1], width / 2, 0, Math.PI * 2);
            ctx.fill();
        } else {
            ctx.beginPath();
            ctx.moveTo(first[0], first[1]);
            // Through the middles of the segments: a smooth line, not a broken one.
            for (var i = 1; i < count - 1; i++) {
                var a = toScreen(pts[i]), b = toScreen(pts[i + 1]);
                ctx.quadraticCurveTo(a[0], a[1], (a[0] + b[0]) / 2, (a[1] + b[1]) / 2);
            }
            var last = toScreen(pts[count - 1]);
            ctx.lineTo(last[0], last[1]);
            ctx.stroke();
        }
        ctx.restore();
    }

    function redraw() {
        ctx.clearRect(0, 0, window.innerWidth, window.innerHeight);
        if (!htmlPage && Reveal.isOverview()) return;
        (strokes[slideKey()] || []).forEach(function (stroke) { paint(stroke); });
        if (drawing) paint(drawing);
    }

    // ---- drawing and erasing ----

    function distanceToSegment(px, py, ax, ay, bx, by) {
        var dx = bx - ax, dy = by - ay;
        var lengthSquared = dx * dx + dy * dy;
        var t = lengthSquared ? Math.max(0, Math.min(1, ((px - ax) * dx + (py - ay) * dy) / lengthSquared)) : 0;
        return Math.hypot(px - (ax + t * dx), py - (ay + t * dy));
    }

    /** Takes away every stroke of the slide the eraser is over at this point. */
    function erase(x, y) {
        var list = current();
        var kept = list.filter(function (stroke) {
            var reach = ERASER_RADIUS + (stroke.widthShare * box().width) / 2;
            var pts = stroke.points.map(toScreen);
            if (pts.length === 1) return Math.hypot(x - pts[0][0], y - pts[0][1]) > reach;
            for (var i = 1; i < pts.length; i++) {
                if (distanceToSegment(x, y, pts[i - 1][0], pts[i - 1][1], pts[i][0], pts[i][1]) <= reach) return false;
            }
            return true;
        });
        if (kept.length !== list.length) {
            strokes[slideKey()] = kept;
            redraw();
        }
    }

    function begin(event) {
        if (!active || event.button > 0) return;
        event.preventDefault();
        event.stopPropagation();
        try { window.focus(); } catch (e) { /* the keys stay where they were */ }
        try { canvas.setPointerCapture(event.pointerId); } catch (e) { /* the moves come to the canvas anyway */ }
        if (tool === 'eraser') {
            erasing = true;
            erase(event.clientX, event.clientY);
            return;
        }
        // The width is kept as a share of the surface's width (the slide, or the diagram on the whole page): a stroke
        // is the same on a bigger window, and follows a diagram that is zoomed.
        drawing = { tool: tool, color: color, widthShare: (WIDTH[tool] * scale()) / box().width, points: [toSlide(event.clientX, event.clientY)] };
        paint(drawing);
    }

    function move(event) {
        if (erasing) {
            if (event.buttons === 0) { finish(); return; }
            erase(event.clientX, event.clientY);
            return;
        }
        if (!drawing) return;
        // The button went up where this page could not see it.
        if (event.buttons === 0) { finish(); return; }
        var point = toSlide(event.clientX, event.clientY);
        var last = drawing.points[drawing.points.length - 1];
        var b = box();
        // Not a point per pixel: a hand moves smoothly, and a long stroke would grow for nothing.
        if (Math.hypot((point[0] - last[0]) * b.width, (point[1] - last[1]) * b.height) < 1.5) return;
        drawing.points.push(point);
        redraw();
    }

    function finish() {
        erasing = false;
        if (!drawing) return;
        current().push(drawing);
        drawing = null;
        redraw();
    }

    canvas.addEventListener('pointerdown', begin);
    canvas.addEventListener('pointermove', move);
    canvas.addEventListener('pointerup', finish);
    canvas.addEventListener('pointercancel', finish);
    // Released outside the window: not left drawing.
    document.addEventListener('pointerup', finish, true);

    // ---- the panel ----

    var panel = document.createElement('div');
    panel.id = 'mde-ink-panel';
    panel.className = 'mde-img-toolbar mde-ink-panel';
    panel.hidden = true;

    function el(tag, className, text) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined) node.textContent = text;
        return node;
    }

    var grip = el('span', 'mde-img-toolbar-grip', '⠇');
    grip.title = T('slide.grip');
    panel.appendChild(grip);

    var toolButtons = {};
    [['pen', '✏️', 'ink.pen'], ['highlighter', '🖍️', 'ink.highlighter'], ['eraser', '🧽', 'ink.eraser']].forEach(function (t) {
        var b = el('button', 'mde-ink-tool', t[1]);
        b.type = 'button';
        b.setAttribute('data-tool', t[0]);
        b.title = T(t[2]);
        b.addEventListener('click', function () { setTool(t[0]); b.blur(); });
        toolButtons[t[0]] = b;
        panel.appendChild(b);
    });

    var palette = el('span', 'mde-ink-palette');
    palette.setAttribute('role', 'group');
    palette.setAttribute('aria-label', T('ink.colors'));
    var swatches = {};
    COLORS.forEach(function (c) {
        var s = el('button', 'mde-ink-swatch');
        s.type = 'button';
        s.setAttribute('data-color', c.value);
        s.style.background = c.value;
        s.title = T('ink.color', { name: T('ink.' + c.name) });
        s.addEventListener('click', function () { setColor(c.value); s.blur(); });
        swatches[c.value] = s;
        palette.appendChild(s);
    });
    panel.appendChild(palette);

    var clear = el('button', 'mde-ink-clear', '🗑️ ' + T('ink.reset'));
    clear.type = 'button';
    clear.title = T('ink.resetTitle');
    clear.addEventListener('click', function () { clearSlide(); clear.blur(); });
    panel.appendChild(clear);

    var closeButton = el('button', 'mde-ink-close', '✕');
    closeButton.type = 'button';
    closeButton.title = T('ink.close');
    closeButton.addEventListener('click', function () { setActive(false); });
    panel.appendChild(closeButton);

    document.body.appendChild(panel);

    function setTool(name) {
        tool = name;
        remember(TOOL_KEY, name === 'eraser' ? (recall(TOOL_KEY) || 'pen') : name);
        Object.keys(toolButtons).forEach(function (k) { toolButtons[k].setAttribute('aria-pressed', k === name ? 'true' : 'false'); });
        canvas.classList.toggle('mde-ink-erasing', name === 'eraser');
        // A colour is for the pen and the highlighter: the eraser has none to choose.
        palette.classList.toggle('mde-ink-palette-off', name === 'eraser');
    }

    function setColor(value) {
        color = value;
        remember(COLOR_KEY, value);
        Object.keys(swatches).forEach(function (k) { swatches[k].setAttribute('aria-pressed', k === value ? 'true' : 'false'); });
        // Choosing a colour while erasing goes back to drawing: that is what one means.
        if (tool === 'eraser') setTool(recall(TOOL_KEY) === 'highlighter' ? 'highlighter' : 'pen');
    }

    function clearSlide() {
        strokes[slideKey()] = [];
        drawing = null;
        redraw();
    }

    // ---- where the panel is: a bit above the bottom, in the middle, and where the user puts it ----

    var position = (function () {
        try {
            var saved = JSON.parse(recall(POSITION_KEY));
            if (saved && typeof saved.fx === 'number' && typeof saved.fy === 'number') {
                return { fx: Math.min(1, Math.max(0, saved.fx)), fy: Math.min(1, Math.max(0, saved.fy)) };
            }
        } catch (e) { /* damaged: the same as none */ }
        return { fx: 0.5, fy: 1 };
    })();

    function place() {
        var roomX = Math.max(0, window.innerWidth - panel.offsetWidth - 2 * MARGIN);
        var roomY = Math.max(0, window.innerHeight - panel.offsetHeight - 2 * MARGIN);
        panel.style.left = (MARGIN + position.fx * roomX) + 'px';
        panel.style.top = (MARGIN + position.fy * roomY) + 'px';
    }

    var moving = null;

    function onMove(event) {
        if (!moving) return;
        if (event.buttons === 0) { stopMoving(); return; }
        var roomX = Math.max(0, window.innerWidth - panel.offsetWidth - 2 * MARGIN);
        var roomY = Math.max(0, window.innerHeight - panel.offsetHeight - 2 * MARGIN);
        var left = Math.min(window.innerWidth - panel.offsetWidth - MARGIN, Math.max(MARGIN, moving.left + event.clientX - moving.x));
        var top = Math.min(window.innerHeight - panel.offsetHeight - MARGIN, Math.max(MARGIN, moving.top + event.clientY - moving.y));
        panel.style.left = left + 'px';
        panel.style.top = top + 'px';
        position = { fx: roomX ? (left - MARGIN) / roomX : 0.5, fy: roomY ? (top - MARGIN) / roomY : 1 };
    }

    function stopMoving() {
        if (!moving) return;
        document.removeEventListener('pointermove', onMove, true);
        document.removeEventListener('pointerup', stopMoving, true);
        document.removeEventListener('pointercancel', stopMoving, true);
        panel.classList.remove('mde-img-toolbar-dragging');
        moving = null;
        remember(POSITION_KEY, JSON.stringify(position));
    }

    grip.addEventListener('pointerdown', function (event) {
        if (event.button !== 0) return;
        event.preventDefault();
        event.stopPropagation();
        try { window.focus(); } catch (e) { /* the keys stay where they were */ }
        var rect = panel.getBoundingClientRect();
        moving = { x: event.clientX, y: event.clientY, left: rect.left, top: rect.top };
        panel.classList.add('mde-img-toolbar-dragging');
        document.addEventListener('pointermove', onMove, true);
        document.addEventListener('pointerup', stopMoving, true);
        document.addEventListener('pointercancel', stopMoving, true);
    });

    if (window.ResizeObserver) {
        new ResizeObserver(function () { if (!moving && !panel.hidden) place(); }).observe(panel);
    }

    // ---- on and off ----

    var button = window.mdeSlideToolbar.addButton({
        text: '🖍️',
        className: 'mde-tb-ink',
        title: T(htmlPage ? 'ink.buttonPage' : 'ink.button'),
        onClick: function () { setActive(!active); }
    });

    function setActive(on) {
        on = !!on;
        if (on && window.mdeSlideToolbar.isEditMode()) return;   // one thing at a time
        active = on;
        finish();
        panel.hidden = !on;
        canvas.classList.toggle('mde-ink-on', on);
        document.body.classList.toggle('mde-ink-active', on);
        button.classList.toggle('active', on);
        if (on) place();
    }

    /** "Modifica" and the annotations are exclusive: with the mode on the button is not there. */
    function follow() {
        var editing = window.mdeSlideToolbar.isEditMode();
        button.hidden = editing;
        if (editing) setActive(false);
    }
    window.addEventListener('mde-edit-mode', follow);

    // Keys reveal.js does not use, to annotate without going to the bar. Not while something is being typed.
    window.addEventListener('keydown', function (event) {
        if (event.defaultPrevented || event.ctrlKey || event.metaKey || event.altKey) return;
        var target = event.target;
        if (target && (target.isContentEditable || /^(INPUT|TEXTAREA|SELECT)$/.test(target.tagName))) return;
        if (window.mdeSlideToolbar.isEditMode()) return;
        var key = event.key.length === 1 ? event.key.toLowerCase() : event.key;
        if (key === 'a') { event.preventDefault(); setActive(!active); }
        else if (active && key === 'e') { event.preventDefault(); setTool(tool === 'eraser' ? (recall(TOOL_KEY) === 'highlighter' ? 'highlighter' : 'pen') : 'eraser'); }
        else if (active && key === 'c') { event.preventDefault(); clearSlide(); }
        else if (active && key === 'Escape') {
            // Before reveal.js sees it: the first Esc closes the annotations, it does not open the overview.
            event.preventDefault();
            event.stopPropagation();
            setActive(false);
        }
    }, true);

    if (htmlPage) {
        // The strokes are the document's: they scroll with it.
        window.addEventListener('scroll', redraw, { passive: true });
        fit();
        redraw();
    } else {
        Reveal.on('slidechanged', function () { drawing = null; redraw(); });
        // A diagram went to the whole page, or came back, or was fitted again: another surface, or the same one elsewhere.
        window.addEventListener('mde-svg-fullpage', function () { drawing = null; redraw(); });
        Reveal.on('resize', redraw);
        Reveal.on('overviewshown', redraw);
        Reveal.on('overviewhidden', redraw);
        Reveal.on('ready', function () { fit(); redraw(); });
    }
    window.addEventListener('resize', function () { fit(); redraw(); if (!panel.hidden) place(); });

    fit();
    setTool(tool);
    setColor(color);
    follow();
})();
