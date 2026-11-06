/**
 * MdExplorer - Choosing the transition of a slide or of the whole deck
 * =====================================================================
 * A button on the bar (slide-toolbar.js, shown while "Modifica" is on) opens a panel with the transitions reveal.js documents, each with a
 * small animation that shows what it does (hover or focus an option and it plays). Two scopes:
 *
 *  - "Questa slide": how THAT slide enters and leaves. reveal.js has no transition "between A and B": each
 *    of the two slides animates by its own rule at the same time, the one that leaves by its own and the
 *    one that comes in by its own (its CSS keys on each section's data-transition, measured on 6.0.2). A
 *    value written on a slide comes first over the deck's. "Come il deck" takes the slide's own away.
 *    In a vertical stack a slide's value is used for the moves up and down; the move sideways is the
 *    stack's, which has no place in the file: the deck's.
 *  - "Tutto il deck": reveal.config.transition in the front matter, for the slides that do not say.
 *
 * The choice is written in the .md by the server (POST api/mdfiles/SetSlideTransition,
 * SlideTransitionEditor: the deck is rendered again and only that must have changed); the view then
 * reloads as after any change of the file. A value written by hand that is not one of these (a
 * "zoom-in fade-out") shows nothing selected, and choosing one replaces it.
 *
 * The small animations are the CSS reveal.js uses for each transition (same transforms, same curve and
 * duration), on two little boxes: not reveal.js itself, so they cost nothing and cannot touch the deck.
 *
 * Only inside MdExplorer's view, as the bar.
 *
 * Sprint: docs-internal/Sprints/2026-09-30-Slide-Barra-Strumenti.md
 */
(function () {
    'use strict';

    if (!window.mdeSlideToolbar || !window.Reveal) return;

    var PANEL_ID = 'mde-slide-transition-panel';
    // The words are in toolbar-shared.js (IT/EN), read when they are used: 'tr.<value>'.
    var VALUES = ['none', 'fade', 'slide', 'convex', 'concave', 'zoom'];

    function T(key, params) {
        return _toolbarText(key, params);
    }

    function isChoice(value) {
        return VALUES.indexOf(value) >= 0;
    }

    /** The name of a transition in the app's language; a value that is not in the list is shown as written. */
    function labelOf(value) {
        return isChoice(value) ? T('tr.' + value) : value;
    }
    /** reveal.js's own default, when the deck says nothing. */
    var DEFAULT_TRANSITION = 'slide';
    var CYCLE_MS = 1700;
    /** The last transition chosen, kept in the browser: it is offered again on the next slide. */
    var LAST_KEY = 'mdexplorer_slide_last_transition';

    function rememberLast(value) {
        try { window.localStorage.setItem(LAST_KEY, value); } catch (e) { /* no storage: it forgets, it works */ }
    }

    /** The last transition chosen, or null (nothing chosen yet, or a value that is no longer one of the list). */
    function recallLast() {
        try {
            var value = window.localStorage.getItem(LAST_KEY);
            return value && isChoice(value) ? value : null;
        } catch (e) {
            return null;
        }
    }

    var panel = null;
    var scope = 'slide';
    var button = null;

    // ---- what the deck says now ----

    function section() {
        return Reveal.getCurrentSlide();
    }

    function slideLine() {
        var line = parseInt(section() && section().getAttribute('data-mde-line-start'), 10);
        return isNaN(line) ? null : line;
    }

    function inStack() {
        var current = section();
        return !!(current && current.parentElement && current.parentElement.tagName === 'SECTION');
    }

    function slideValue() {
        return (section() && section().getAttribute('data-transition')) || null;
    }

    function deckValue() {
        return Reveal.getConfig().transition || DEFAULT_TRANSITION;
    }

    // ---- the panel ----

    function open() {
        if (panel) return;
        scope = slideLine() ? 'slide' : 'deck';
        panel = document.createElement('div');
        panel.id = PANEL_ID;
        panel.className = 'mde-tr-panel';
        // reveal.js and the bar leave the keys to it while it is open (slide-diagrams.js, slide-toolbar.js): Esc closes it.
        panel.setAttribute('data-mde-holds-keys', '');
        document.body.appendChild(panel);
        render();
        button.classList.add('active');
    }

    function close() {
        if (!panel) return;
        panel.remove();
        panel = null;
        if (button) button.classList.remove('active');
    }

    function place() {
        if (!panel) return;
        var bar = window.mdeSlideToolbar.element.getBoundingClientRect();
        var box = panel.getBoundingClientRect();
        var left = Math.min(window.innerWidth - box.width - 8, Math.max(8, bar.right - box.width));
        var top = bar.bottom + 6;
        // No room below: above the bar, or as low as it goes.
        if (top + box.height > window.innerHeight - 8) top = Math.max(8, Math.min(bar.top - box.height - 6, window.innerHeight - box.height - 8));
        panel.style.left = left + 'px';
        panel.style.top = top + 'px';
    }

    function el(tag, className, text) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined) node.textContent = text;
        return node;
    }

    function render() {
        panel.textContent = '';
        var canSlide = slideLine() !== null;
        if (scope === 'slide' && !canSlide) scope = 'deck';

        var scopes = el('div', 'mde-tr-scopes');
        [['slide', T('tr.scopeSlide')], ['deck', T('tr.scopeDeck')]].forEach(function (pair) {
            var tab = el('button', 'mde-tr-scope', pair[1]);
            tab.type = 'button';
            tab.setAttribute('aria-pressed', scope === pair[0] ? 'true' : 'false');
            if (pair[0] === 'slide' && !canSlide) {
                tab.disabled = true;
                tab.title = T('tr.slideNotInFile');
            }
            tab.addEventListener('click', function () { scope = pair[0]; render(); });
            scopes.appendChild(tab);
        });
        panel.appendChild(scopes);

        var hint = scope === 'deck' ? T('tr.hintDeck') : (inStack() ? T('tr.hintStack') : T('tr.hintSlide'));
        panel.appendChild(el('div', 'mde-tr-hint', hint));

        var list = el('div', 'mde-tr-list');
        var current = scope === 'deck' ? deckValue() : slideValue();
        var last = recallLast();

        // The next slide: the last transition chosen is one click away, without looking for it in the list.
        if (scope === 'slide' && last && current !== last) {
            var suggest = el('button', 'mde-tr-suggest', T('tr.useLast', { name: labelOf(last) }));
            suggest.type = 'button';
            suggest.setAttribute('data-value', last);
            suggest.title = T('tr.useLastTitle');
            suggest.addEventListener('click', function () { apply(last); });
            panel.appendChild(suggest);
        }

        if (scope === 'slide') {
            list.appendChild(option(null, T('tr.asDeck', { name: labelOf(deckValue()) }), deckValue(), current === null));
        }
        VALUES.forEach(function (value) {
            var one = option(value, labelOf(value), value, current === value);
            if (value === last) {
                one.classList.add('mde-tr-last');
                one.title = T('tr.lastMark');
            }
            list.appendChild(one);
        });
        panel.appendChild(list);

        var custom = scope === 'slide' && current && !isChoice(current);
        if (custom) panel.appendChild(el('div', 'mde-tr-hint', T('tr.custom', { value: current })));
        place();
    }

    function option(value, label, demoValue, selected) {
        var choice = el('button', 'mde-tr-option');
        choice.type = 'button';
        choice.setAttribute('aria-pressed', selected ? 'true' : 'false');
        choice.setAttribute('data-value', value === null ? '' : value);
        choice.appendChild(demo(demoValue));
        choice.appendChild(el('span', 'mde-tr-name', label));
        choice.addEventListener('click', function () { apply(value); });
        return choice;
    }

    // ---- the small animation ----

    /** Two boxes, A on stage and B waiting: it plays A leaving and B coming in, and back. */
    function demo(transition) {
        var stage = el('div', 'mde-tr-demo');
        stage.setAttribute('data-t', transition);
        var a = el('div', 'mde-tr-s mde-tr-a present', 'A');
        var b = el('div', 'mde-tr-s mde-tr-b future', 'B');
        stage.appendChild(a);
        stage.appendChild(b);
        var timer = null;
        var forward = false;
        function step() {
            forward = !forward;
            a.className = 'mde-tr-s mde-tr-a ' + (forward ? 'past' : 'present');
            b.className = 'mde-tr-s mde-tr-b ' + (forward ? 'present' : 'future');
        }
        function play() {
            if (timer || (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches)) return;
            step();
            timer = setInterval(step, CYCLE_MS);
        }
        function stop() {
            clearInterval(timer);
            timer = null;
            forward = false;
            a.className = 'mde-tr-s mde-tr-a present';
            b.className = 'mde-tr-s mde-tr-b future';
        }
        // The option around the stage takes the hover and the focus.
        setTimeout(function () {
            var owner = stage.parentElement;
            if (!owner) return;
            ['mouseenter', 'focus'].forEach(function (name) { owner.addEventListener(name, play); });
            ['mouseleave', 'blur'].forEach(function (name) { owner.addEventListener(name, stop); });
        }, 0);
        return stage;
    }

    // ---- writing it ----

    function showNotice(message) {
        var notice = document.createElement('div');
        notice.className = 'mde-inline-edit-toast';
        notice.textContent = message;
        notice.addEventListener('click', function () { notice.remove(); });
        document.body.appendChild(notice);
        setTimeout(function () { if (notice.isConnected) notice.remove(); }, 8000);
    }

    function apply(value) {
        var body = document.body;
        var line = scope === 'slide' ? slideLine() : 0;
        close();
        fetch('/api/mdfiles/SetSlideTransition', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                connectionId: body.getAttribute('ConnectionId'),
                documentPath: body.getAttribute('DocumentPath'),
                sourceHash: body.getAttribute('data-mde-source-hash'),
                scope: scope,
                line: line,
                transition: value
            })
        }).then(function (response) {
            return response.json().catch(function () { return {}; }).then(function (result) {
                // The file is written; the view reloads by itself. On a refusal the deck stays as it was.
                // What was chosen is remembered (not "Come il deck", which is no transition): the next slide is offered it.
                if (response.ok && value) rememberLast(value);
                if (!response.ok) showNotice(_toolbarServerMessage('SlideTransition', result) || result.message || T('tr.failed', { detail: 'HTTP ' + response.status }));
            });
        }).catch(function (error) {
            console.error('[slide-transitions] The transition could not be changed:', error);
            showNotice(T('tr.failed', { detail: error.message }));
        });
    }

    // ---- closing it ----

    window.addEventListener('keydown', function (event) {
        if (event.key !== 'Escape' || !panel || event.defaultPrevented) return;
        event.preventDefault();
        event.stopPropagation();
        close();
    }, true);

    document.addEventListener('mousedown', function (event) {
        if (!panel) return;
        if (panel.contains(event.target) || (button && button.contains(event.target))) return;
        close();
    }, true);

    Reveal.on('slidechanged', close);
    Reveal.on('overviewshown', close);
    window.addEventListener('resize', place);

    button = window.mdeSlideToolbar.addButton({
        text: '\uD83C\uDFAC',
        className: 'mde-tb-transitions',
        title: T('tr.button'),
        onClick: function () { if (panel) close(); else open(); }
    });

    // The transitions are a way of editing: the button is there with "Modifica", not in "Presenta" (a bar with
    // two buttons to choose from is a bar one can read). Leaving the mode, or full screen, closes the panel.
    function follow() {
        var editing = window.mdeSlideToolbar.isEditMode();
        button.hidden = !editing;
        if (!editing) close();
    }
    window.addEventListener('mde-edit-mode', follow);
    document.addEventListener('fullscreenchange', function () { if (document.fullscreenElement) close(); });
    follow();
})();
