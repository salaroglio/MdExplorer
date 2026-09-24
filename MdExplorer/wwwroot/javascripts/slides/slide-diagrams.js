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
 * The colour legend: in a document it lives in the diagram's toolbar (image-transform.js), which a
 * slide does not have. Here each slide with interactive diagrams gets a 🎨 button in its top
 * right corner, opening the legend of that slide's diagrams: the same data (InteractiveSvg.getLegend),
 * the same texts and the same remembered open/closed state (toolbar-shared.js), the same classes.
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

    // ---- colour legend ----

    var legends = [];

    /** The legend of all the slide's interactive diagrams: each colour once, in getLegend's order. */
    function legendOf(slide) {
        var legend = { boxes: [], links: [] };
        var seen = { boxes: {}, links: {} };
        slide.querySelectorAll('svg.interactive-svg').forEach(function (svg) {
            var one = InteractiveSvg.getLegend(svg);
            ['boxes', 'links'].forEach(function (part) {
                one[part].forEach(function (item) {
                    if (!seen[part][item.key]) {
                        seen[part][item.key] = true;
                        legend[part].push(item);
                    }
                });
            });
        });
        return legend;
    }

    /** As _renderSvgLegend in image-transform.js, without the dark filter: slides are not inverted. */
    function renderLegend(panel, legend) {
        panel.textContent = '';
        function section(title, items, shape) {
            if (!items.length) return;
            var heading = document.createElement('div');
            heading.className = 'mde-svg-legend-title';
            heading.textContent = title;
            panel.appendChild(heading);
            items.forEach(function (item) {
                var swatch = document.createElement('span');
                swatch.className = 'mde-svg-legend-swatch mde-svg-legend-' + shape;
                if (shape === 'box') {
                    swatch.style.borderColor = item.color;
                    swatch.style.boxShadow = '0 0 4px ' + item.color;
                    if (item.key === 'note') swatch.style.background = item.color;
                } else {
                    swatch.style.background = item.color;
                }
                var label = document.createElement('span');
                label.textContent = _toolbarText(item.key);
                var row = document.createElement('div');
                row.className = 'mde-svg-legend-row';
                row.appendChild(swatch);
                row.appendChild(label);
                panel.appendChild(row);
            });
        }
        section(_toolbarText('boxes'), legend.boxes, 'box');
        section(_toolbarText('arrows'), legend.links, 'line');
    }

    function addLegend(slide) {
        var legend = legendOf(slide);
        if (!legend.boxes.length && !legend.links.length) return;

        var container = document.createElement('div');
        container.className = 'mde-slide-legend';
        var button = document.createElement('button');
        button.type = 'button';
        button.className = 'mde-slide-legend-toggle';
        button.textContent = '\uD83C\uDFA8';
        var panel = document.createElement('div');
        panel.className = 'mde-svg-legend';
        container.appendChild(button);
        container.appendChild(panel);
        slide.appendChild(container);

        // Not a click "outside" the diagram: the scripts would clear the selection the legend explains.
        container.addEventListener('click', function (event) { event.stopPropagation(); });
        button.addEventListener('click', function () {
            var open = !container.classList.contains('open');
            _rememberSvgLegend(open);
            legends.forEach(function (show) { show(open); });
        });

        function show(open) {
            container.classList.toggle('open', open);
            button.title = _toolbarText(open ? 'legendHide' : 'legendShow');
            if (open) renderLegend(panel, legendOf(slide));
        }
        legends.push(show);
        show(_svgLegendRemembered());
    }

    function addLegends() {
        if (typeof _toolbarText !== 'function' || typeof _svgLegendRemembered !== 'function') {
            console.error('[slide-diagrams] toolbar-shared.js is not loaded: no colour legend.');
            return;
        }
        Reveal.getSlides().forEach(addLegend);
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
        // After InteractiveSvg: getLegend reads the link types it has marked.
        if (window.InteractiveSvg) addLegends();
    }

    Reveal.on('ready', start);
})();
