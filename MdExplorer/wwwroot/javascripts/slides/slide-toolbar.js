/**
 * MdExplorer - The toolbar of a slide deck
 * =========================================
 * A small floating bar, as the one of the images and diagrams of a document (same look, same
 * grip: image-toolbar.css): the grip is the only place it is dragged from, so a click on a button
 * stays a click. It starts in the top right corner and goes where the user puts it.
 *
 * Two states, one of them always on, with their names on them: "Presenta" (the deck as it always was: links
 * and moving between decks work, nothing is corrected by a click — it is the state a deck opens in) and
 * "Modifica" ("Modifica veloce": the page is for correcting). Then ⛶, full screen: the bar and every tool are
 * hidden and Esc comes back (the deck's iframe has allowfullscreen). Other scripts add their buttons
 * (mdeSlideToolbar.addButton), that come before ⛶. On, the body carries `mde-edit-mode` and
 * the window gets an `mde-edit-mode` event ({detail: {on}}): what the mode does on the page
 * (highlighting, click to correct, dragging the items) is done by the scripts that listen to it.
 * Off, the page is the one of a deck without this bar.
 *
 * Every write to the .md reloads the deck, so the bar's place and the mode are remembered in the
 * browser (localStorage, per window); without storage it still works, it just forgets.
 *
 * Esc leaves the mode — but only when nothing else is waiting for it: a correction in progress
 * (its own Esc cancels it first), an open menu or a drag in progress (data-mde-holds-keys), or a
 * handler that already took the key (defaultPrevented). Handled here in capture on window, so
 * reveal.js does not open its overview on the Esc that leaves the mode.
 *
 * Only inside MdExplorer's view (its page around the slide is the app, /client2/): not in the
 * speaker view, not in a detached window, not in print (as slide-edit.js).
 *
 * Sprint: docs-internal/Sprints/2026-09-30-Slide-Barra-Strumenti.md
 */
(function () {
    'use strict';

    var POSITION_KEY = 'mdexplorer_slide_toolbar_position';
    var MODE_KEY = 'mdexplorer_slide_edit_mode';
    var MARGIN_X = 12;
    var MARGIN_Y = 8;
    var EDIT_MODE = 'mde-edit-mode';

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

    function enabled() {
        return inMdExplorerView() && !isPrint()
            && !!document.body.getAttribute('data-mde-source-hash')
            && !!document.body.getAttribute('DocumentPath')
            && !!document.body.getAttribute('ConnectionId');
    }
    if (!enabled()) return;

    // ---- what is remembered ----

    function remember(key, value) {
        try { window.localStorage.setItem(key, value); } catch (e) { /* no storage: it forgets, it works */ }
    }

    function recall(key) {
        try { return window.localStorage.getItem(key); } catch (e) { return null; }
    }

    function clamp01(value) {
        return typeof value === 'number' && isFinite(value) ? Math.min(1, Math.max(0, value)) : null;
    }

    /** Where the bar sits, as a fraction of the room it has (0 = left/top, 1 = right/bottom). */
    function recalledPosition() {
        try {
            var saved = JSON.parse(recall(POSITION_KEY));
            var fx = clamp01(saved && saved.fx), fy = clamp01(saved && saved.fy);
            if (fx !== null && fy !== null) return { fx: fx, fy: fy };
        } catch (e) { /* a damaged value is the same as none */ }
        return { fx: 1, fy: 0 };
    }

    // ---- the bar ----

    var bar = document.createElement('div');
    bar.id = 'mde-slide-toolbar';
    bar.className = 'mde-img-toolbar mde-slide-toolbar';

    var grip = document.createElement('span');
    grip.className = 'mde-img-toolbar-grip';
    grip.textContent = '\u2807';
    grip.title = 'Trascina la barra';
    bar.appendChild(grip);

    // The two states: exactly one is on. The names are on the buttons: a bare icon did not say it was a mode.
    var modes = document.createElement('div');
    modes.className = 'mde-tb-modes';
    modes.setAttribute('role', 'group');
    modes.setAttribute('aria-label', 'Modalità');

    function modeButton(className, icon, label, title) {
        var b = document.createElement('button');
        b.type = 'button';
        b.className = className;
        b.title = title;
        var i = document.createElement('span');
        i.className = 'mde-tb-icon';
        i.textContent = icon;
        var t = document.createElement('span');
        t.className = 'mde-tb-label';
        t.textContent = label;
        b.appendChild(i);
        b.appendChild(t);
        return b;
    }

    var presentButton = modeButton('mde-tb-present active', '\u25B6', 'Presenta', 'Presenta: la presentazione com\'è, con i link e il passaggio da un deck all\'altro');
    presentButton.setAttribute('aria-pressed', 'true');
    var editButton = modeButton('mde-tb-edit', '\u270F\uFE0F', 'Modifica', 'Modifica veloce: clic per correggere il testo, trascina le voci (Esc per uscire)');
    editButton.setAttribute('aria-pressed', 'false');
    modes.appendChild(presentButton);
    modes.appendChild(editButton);
    bar.appendChild(modes);

    // Full screen: an icon of four corners, drawn here (a character for it is missing in some fonts).
    var fullscreenButton = document.createElement('button');
    fullscreenButton.type = 'button';
    fullscreenButton.className = 'mde-tb-fullscreen';
    fullscreenButton.title = 'Schermo intero (Esc per uscire)';
    fullscreenButton.innerHTML = '<svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="M2 6V2h4M10 2h4v4M14 10v4h-4M6 14H2v-4" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/></svg>';
    bar.appendChild(fullscreenButton);

    document.body.appendChild(bar);

    var position = recalledPosition();

    function place() {
        var roomX = Math.max(0, window.innerWidth - bar.offsetWidth - 2 * MARGIN_X);
        var roomY = Math.max(0, window.innerHeight - bar.offsetHeight - 2 * MARGIN_Y);
        bar.style.left = (MARGIN_X + position.fx * roomX) + 'px';
        bar.style.top = (MARGIN_Y + position.fy * roomY) + 'px';
    }

    place();
    window.addEventListener('resize', place);
    // The bar changes width when a button comes or goes (the transitions' one, with the mode): put again where
    // its remembered place says, or, at the right edge, it would stick out by the width that came. Not while it is dragged.
    if (window.ResizeObserver) {
        new ResizeObserver(function () { if (!moving) place(); }).observe(bar);
    }

    // ---- moving the bar (from the grip only) ----

    var moving = null;

    function onMove(event) {
        if (!moving) return;
        // The button went up where this page could not see it: stop, do not stay stuck to the pointer.
        if (event.buttons === 0) { stopMoving(); return; }
        var roomX = Math.max(0, window.innerWidth - bar.offsetWidth - 2 * MARGIN_X);
        var roomY = Math.max(0, window.innerHeight - bar.offsetHeight - 2 * MARGIN_Y);
        var left = Math.min(window.innerWidth - bar.offsetWidth - MARGIN_X, Math.max(MARGIN_X, moving.left + event.clientX - moving.x));
        var top = Math.min(window.innerHeight - bar.offsetHeight - MARGIN_Y, Math.max(MARGIN_Y, moving.top + event.clientY - moving.y));
        bar.style.left = left + 'px';
        bar.style.top = top + 'px';
        position = {
            fx: roomX ? clamp01((left - MARGIN_X) / roomX) : 1,
            fy: roomY ? clamp01((top - MARGIN_Y) / roomY) : 0
        };
    }

    function stopMoving() {
        if (!moving) return;
        document.removeEventListener('pointermove', onMove, true);
        document.removeEventListener('pointerup', stopMoving, true);
        document.removeEventListener('pointercancel', stopMoving, true);
        bar.classList.remove('mde-img-toolbar-dragging');
        moving = null;
        remember(POSITION_KEY, JSON.stringify(position));
    }

    grip.addEventListener('pointerdown', function (event) {
        if (event.button !== 0) return;
        event.preventDefault();   // without it the browser starts selecting the text below
        event.stopPropagation();
        // Cancelling pointerdown also cancels the mousedown that gives this page the keyboard.
        try { window.focus(); } catch (e) { /* the keys stay where they were */ }
        var rect = bar.getBoundingClientRect();
        moving = { x: event.clientX, y: event.clientY, left: rect.left, top: rect.top };
        bar.classList.add('mde-img-toolbar-dragging');
        // On the document, not on the grip: pointer capture is not what this relies on (measured in slide-list-drag.js).
        document.addEventListener('pointermove', onMove, true);
        document.addEventListener('pointerup', stopMoving, true);
        document.addEventListener('pointercancel', stopMoving, true);
    });

    // ---- the mode ----

    function isEditMode() {
        return document.body.classList.contains(EDIT_MODE);
    }

    function setEditMode(on) {
        on = !!on;
        if (on === isEditMode()) return;
        document.body.classList.toggle(EDIT_MODE, on);
        editButton.classList.toggle('active', on);
        editButton.setAttribute('aria-pressed', on ? 'true' : 'false');
        presentButton.classList.toggle('active', !on);
        presentButton.setAttribute('aria-pressed', on ? 'false' : 'true');
        remember(MODE_KEY, on ? '1' : '0');
        window.dispatchEvent(new CustomEvent('mde-edit-mode', { detail: { on: on } }));
    }

    editButton.addEventListener('click', function () {
        setEditMode(true);
        // The keys go back to the deck: a focused button would also take Space and Enter.
        editButton.blur();
    });

    presentButton.addEventListener('click', function () {
        setEditMode(false);
        presentButton.blur();
    });

    // ---- full screen ----

    function showNotice(message) {
        var notice = document.createElement('div');
        notice.className = 'mde-inline-edit-toast';
        notice.textContent = message;
        notice.addEventListener('click', function () { notice.remove(); });
        document.body.appendChild(notice);
        setTimeout(function () { if (notice.isConnected) notice.remove(); }, 8000);
    }

    fullscreenButton.addEventListener('click', function () {
        fullscreenButton.blur();
        if (document.fullscreenElement) {
            document.exitFullscreen();
            return;
        }
        if (!document.documentElement.requestFullscreen) {
            showNotice('Lo schermo intero non è disponibile qui.');
            return;
        }
        document.documentElement.requestFullscreen().catch(function (error) {
            console.error('[slide-toolbar] Full screen was refused:', error);
            showNotice('Lo schermo intero non è disponibile qui.');
        });
    });

    // Full screen, from the button or from reveal.js's own key (F): the bar and the tools go away, and it is a presentation.
    document.addEventListener('fullscreenchange', function () {
        var on = !!document.fullscreenElement;
        document.body.classList.toggle('mde-fullscreen', on);
        if (on) setEditMode(false);
    });

    window.addEventListener('keydown', function (event) {
        if (event.key !== 'Escape' || !isEditMode() || event.defaultPrevented) return;
        // A correction in progress: its own Esc cancels it, the mode stays.
        if (document.body.hasAttribute('data-mde-inline-editing')) return;
        // A menu or a drag holds the keys: theirs first.
        if (document.querySelector('[data-mde-holds-keys]')) return;
        event.preventDefault();
        event.stopPropagation();
        setEditMode(false);
    }, true);

    /**
     * Another button on the bar (slide-transitions.js): {text, title, className, onClick(button, event)}, put
     * before the full screen one. The bar is placed again, its width changed. Returns the button.
     */
    function addButton(options) {
        var button = document.createElement('button');
        button.type = 'button';
        if (options.className) button.className = options.className;
        button.textContent = options.text;
        button.title = options.title || '';
        button.addEventListener('click', function (event) {
            options.onClick(button, event);
            // The keys go back to the deck: a focused button would also take Space and Enter.
            button.blur();
        });
        bar.insertBefore(button, fullscreenButton);
        place();
        return button;
    }

    window.mdeSlideToolbar = { isEditMode: isEditMode, setEditMode: setEditMode, addButton: addButton, element: bar };

    // The mode the user left on before the deck reloaded.
    if (recall(MODE_KEY) === '1') setEditMode(true);
})();
