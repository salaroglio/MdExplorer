/**
 * MdExplorer - PlantUML diagrams in a slide deck
 * ===============================================
 * The slide page (SlideDeckRenderer) loads the same interactive-svg scripts as a document —
 * click-to-highlight, sequence, YAML trees and links, "Ask to MarkAgent" — and this file starts
 * them. In a document core/init.js does it, inside jQuery's ready: the slide page has no jQuery,
 * and the scripts do not need it.
 *
 * One thing is the slides' own: ESC. The scripts clear a selection on ESC, reveal.js opens the
 * overview on ESC, and both happened at once. reveal.js is told to leave ESC alone while the
 * current slide has a selection: the first ESC clears it, the next one opens the overview.
 * The selection is read before anyone handles the key (capture on window): by the time reveal.js
 * asks its keyboardCondition, the scripts have already cleared it (measured, sprint F0).
 *
 * Sprint: docs-internal/Sprints/2026-09-24-Slide-SVG-Interattivi.md
 */
(function () {
    'use strict';

    var SCRIPTS = ['InteractiveSvg', 'MarkDiagramContext', 'InteractiveSvgSequence', 'InteractiveSvgYamlLinks', 'InteractiveSvgYaml'];
    var SELECTION = 'svg.has-selection, svg.seq-has-selection, svg.yaml-has-selection';
    var ESC = 27;

    var selectionAtKey = false;

    function currentSlideHasSelection() {
        var slide = Reveal.getCurrentSlide();
        return !!(slide && slide.querySelector(SELECTION));
    }

    window.addEventListener('keydown', function () {
        selectionAtKey = currentSlideHasSelection();
    }, true);

    /** The deck's own keyboardCondition ('focused' is the only one YAML can write) still applies. */
    function leaveEscToTheDiagram(previous) {
        return function (event) {
            if (event.keyCode === ESC && selectionAtKey) return false;
            if (typeof previous === 'function') return previous(event);
            if (previous === 'focused') return Reveal.isFocused();
            return true;
        };
    }

    function start() {
        Reveal.configure({ keyboardCondition: leaveEscToTheDiagram(Reveal.getConfig().keyboardCondition) });
        SCRIPTS.forEach(function (name) {
            var script = window[name];
            if (script && typeof script.initAll === 'function') {
                script.initAll();
            } else {
                console.error('[slide-diagrams] ' + name + ' is not loaded: its diagrams stay still.');
            }
        });
    }

    Reveal.on('ready', start);
})();
