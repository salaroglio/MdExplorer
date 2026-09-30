/**
 * MdExplorer - Choosing the transition of a slide or of the whole deck
 * =====================================================================
 * A button on the bar (slide-toolbar.js) opens a panel with the transitions reveal.js documents, each with a
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
    var CHOICES = [
        { value: 'none', label: 'Nessuna' },
        { value: 'fade', label: 'Dissolvenza' },
        { value: 'slide', label: 'Scorrimento' },
        { value: 'convex', label: 'Convessa' },
        { value: 'concave', label: 'Concava' },
        { value: 'zoom', label: 'Zoom' }
    ];
    var LABELS = {};
    CHOICES.forEach(function (choice) { LABELS[choice.value] = choice.label; });
    /** reveal.js's own default, when the deck says nothing. */
    var DEFAULT_TRANSITION = 'slide';
    var CYCLE_MS = 1700;

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
        [['slide', 'Questa slide'], ['deck', 'Tutto il deck']].forEach(function (pair) {
            var tab = el('button', 'mde-tr-scope', pair[1]);
            tab.type = 'button';
            tab.setAttribute('aria-pressed', scope === pair[0] ? 'true' : 'false');
            if (pair[0] === 'slide' && !canSlide) {
                tab.disabled = true;
                tab.title = 'Questa slide non si può cambiare da qui: la scrive un comando, non è nel file.';
            }
            tab.addEventListener('click', function () { scope = pair[0]; render(); });
            scopes.appendChild(tab);
        });
        panel.appendChild(scopes);

        var hint = scope === 'deck'
            ? 'Vale per le slide che non hanno una transizione propria.'
            : (inStack()
                ? 'Come entra ed esce questa slide. In una pila vale per i passaggi in verticale; in orizzontale conta quella del deck.'
                : 'Come entra ed esce questa slide.');
        panel.appendChild(el('div', 'mde-tr-hint', hint));

        var list = el('div', 'mde-tr-list');
        var current = scope === 'deck' ? deckValue() : slideValue();
        if (scope === 'slide') {
            list.appendChild(option(null, 'Come il deck (' + (LABELS[deckValue()] || deckValue()) + ')', deckValue(), current === null));
        }
        CHOICES.forEach(function (choice) {
            list.appendChild(option(choice.value, choice.label, choice.value, current === choice.value));
        });
        panel.appendChild(list);

        var custom = scope === 'slide' && current && !LABELS[current];
        if (custom) panel.appendChild(el('div', 'mde-tr-hint', 'Scritta a mano nel file: «' + current + '». Sceglierne una la sostituisce.'));
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
                if (!response.ok) showNotice(result.message || result.error || ('Non sono riuscito a cambiare la transizione (HTTP ' + response.status + ').'));
            });
        }).catch(function (error) {
            console.error('[slide-transitions] The transition could not be changed:', error);
            showNotice('Non sono riuscito a cambiare la transizione: ' + error.message);
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
        title: 'Transizione delle slide',
        onClick: function () { if (panel) close(); else open(); }
    });
})();
