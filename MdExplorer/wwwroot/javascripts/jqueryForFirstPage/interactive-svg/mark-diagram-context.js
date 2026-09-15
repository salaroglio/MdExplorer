/**
 * MdExplorer - Ask to MarkAgent (context menu on PlantUML diagram boxes)
 * ======================================================================
 * Right-click on a box of a PlantUML diagram opens a small menu with
 * "Ask to MarkAgent". Choosing it collects everything the page already knows
 * about that box and hands it to Angular via postMessage, which then asks the
 * configured LLM to explain the box in at most ten sentences.
 *
 * Why this file exists separately from interactive-svg.js:
 *   interactive-svg.js owns *highlighting*. This file owns *asking*. It uses
 *   the public API (InteractiveSvg.selectElement) to reuse the highlighting
 *   instead of duplicating it.
 *
 * Where the context comes from — all of it is already in the SVG, no backend
 * call and no PlantUML parser needed (PlantUML v1.2026.1+):
 *   - g.entity / g.cluster  → data-qualified-name  (the box name)
 *   - g.link                → data-entity-1, data-entity-2, data-link-type
 *                             (who is connected to whom, and *how*:
 *                              extension / composition / aggregation /
 *                              association / dependency)
 *   - data-source-line      → position of the element in the PlantUML source
 *   - <?plantuml-src ...?>  → the whole PlantUML source, deflate-compressed
 *                             and encoded in PlantUML's own base64 alphabet
 *
 * Gotcha worth knowing: `<?plantuml-src?>` is a processing instruction, but
 * HTML has no processing instructions — when the browser parses the inlined
 * SVG it turns it into a *comment* node whose data starts with "?plantuml-src".
 * Both shapes are handled below.
 *
 * Usage:
 *   MarkDiagramContext.initAll();       // after InteractiveSvg.initAll()
 */

var MarkDiagramContext = (function () {
    'use strict';

    var MENU_ID = 'mark-diagram-menu';
    var MSG_ASK = 'mde-mark.askAboutBox';

    var SEL_BOXES = 'g[id^="elem_"], g[id^="cluster_"], g[id^="GMN"], g.entity, g.cluster';
    var SEL_LINKS = 'g[id^="link_"], g.link';

    var initializedSvgs = new WeakSet();

    // ─────────────────────────────────────────────────────────────────────
    //  PlantUML source, extracted from the SVG itself
    // ─────────────────────────────────────────────────────────────────────

    /** PlantUML's own base64 alphabet — not the standard one. */
    var PUML_ALPHABET = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_';

    /**
     * Find the encoded PlantUML source inside the SVG.
     * Returns the encoded payload, or null when the diagram carries none
     * (older PlantUML, or rendering with -nometadata).
     */
    function findEncodedSource(svg) {
        var walker = document.createNodeIterator(
            svg,
            NodeFilter.SHOW_PROCESSING_INSTRUCTION | NodeFilter.SHOW_COMMENT
        );
        var node;
        while ((node = walker.nextNode())) {
            // XML-parsed: a real processing instruction.
            if (node.nodeType === Node.PROCESSING_INSTRUCTION_NODE &&
                node.target === 'plantuml-src') {
                return node.data.trim();
            }
            // HTML-parsed: the same thing arrived as a bogus comment.
            if (node.nodeType === Node.COMMENT_NODE) {
                var m = /^\?plantuml-src\s+([^?\s]+)\??$/.exec(node.data.trim());
                if (m) return m[1];
            }
        }
        return null;
    }

    /**
     * Decode PlantUML's base64 variant into bytes.
     */
    function decodePumlBase64(encoded) {
        var bits = '';
        for (var i = 0; i < encoded.length; i++) {
            var idx = PUML_ALPHABET.indexOf(encoded[i]);
            if (idx < 0) continue;   // padding / stray characters
            bits += idx.toString(2).padStart(6, '0');
        }
        var byteCount = Math.floor(bits.length / 8);
        var bytes = new Uint8Array(byteCount);
        for (var b = 0; b < byteCount; b++) {
            bytes[b] = parseInt(bits.substr(b * 8, 8), 2);
        }
        return bytes;
    }

    /**
     * Inflate raw-deflate bytes into a string.
     * Throws when the browser has no DecompressionStream — we say so rather
     * than silently returning a diagram without its source.
     */
    async function inflateRaw(bytes) {
        if (typeof DecompressionStream === 'undefined') {
            throw new Error(
                'DecompressionStream non disponibile in questo runtime: ' +
                'impossibile leggere il sorgente PlantUML incorporato nell\'SVG.'
            );
        }
        var stream = new Blob([bytes]).stream()
            .pipeThrough(new DecompressionStream('deflate-raw'));
        var buffer = await new Response(stream).arrayBuffer();
        return new TextDecoder('utf-8').decode(buffer);
    }

    /**
     * Full PlantUML source of a diagram, or null when the SVG carries none.
     */
    async function readPlantumlSource(svg) {
        var encoded = findEncodedSource(svg);
        if (!encoded) return null;
        return await inflateRaw(decodePumlBase64(encoded));
    }

    // ─────────────────────────────────────────────────────────────────────
    //  The context of one box
    // ─────────────────────────────────────────────────────────────────────

    /** Map ent0001 → "Qualified.Name" for v1.2026.1+ diagrams. */
    function buildEntityIdMap(svg) {
        var map = {};
        svg.querySelectorAll('g.entity, g.cluster').forEach(function (el) {
            var qname = el.getAttribute('data-qualified-name');
            if (qname && el.id) map[el.id] = qname;
        });
        return map;
    }

    function boxName(box) {
        return box.getAttribute('data-qualified-name') ||
               // Legacy format: the name lives in the id, after the prefix.
               (box.id || '').replace(/^(elem_|cluster_)/, '') ||
               null;
    }

    /**
     * Relations of the selected box, with their UML type when the SVG states it.
     *
     * `type` is null on legacy (pre-2026) diagrams, which carry no
     * data-link-type. It stays null instead of being guessed: a wrong
     * "composition" is worse than an admitted unknown.
     */
    function collectRelations(svg, box) {
        var idMap = buildEntityIdMap(svg);
        var relations = [];

        svg.querySelectorAll(SEL_LINKS).forEach(function (link) {
            var e1 = link.getAttribute('data-entity-1');
            var e2 = link.getAttribute('data-entity-2');
            if (!e1 || !e2) return;              // legacy link, resolved below
            if (e1 !== box.id && e2 !== box.id) return;

            var outgoing = (e1 === box.id);
            relations.push({
                direction: outgoing ? 'outgoing' : 'incoming',
                other: idMap[outgoing ? e2 : e1] || (outgoing ? e2 : e1),
                type: link.getAttribute('data-link-type') || null,
                label: (link.querySelector('text') || {}).textContent || null,
                sourceLine: link.getAttribute('data-source-line') || null
            });
        });

        return relations;
    }

    /**
     * Everything MarkAgent needs to explain one box.
     */
    async function buildContext(svg, box) {
        var name = boxName(box);
        if (!name) {
            throw new Error('Box senza nome riconoscibile: impossibile interrogare MarkAgent.');
        }

        var titleEl = svg.querySelector('title');

        return {
            documentPath: document.body ? (document.body.getAttribute('DocumentPath') || '') : '',
            projectPath:  document.body ? (document.body.getAttribute('ProjectPath')  || '') : '',
            diagramTitle: titleEl ? titleEl.textContent : null,
            diagramType:  svg.getAttribute('data-diagram-type') || null,
            // 'modern' diagrams state their relation types; legacy ones do not.
            svgFormat:    svg.querySelector('g.entity, g.cluster, g.link') ? 'plantuml-2026' : 'legacy',
            box: {
                name: name,
                kind: box.classList.contains('cluster') ? 'cluster' : 'entity',
                sourceLine: box.getAttribute('data-source-line') || null
            },
            relations: collectRelations(svg, box),
            plantumlSource: await readPlantumlSource(svg)
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    //  The menu
    // ─────────────────────────────────────────────────────────────────────

    function closeMenu() {
        var existing = document.getElementById(MENU_ID);
        if (existing) existing.remove();
    }

    function openMenu(x, y, onAsk) {
        closeMenu();

        var menu = document.createElement('div');
        menu.id = MENU_ID;
        menu.className = 'mark-diagram-menu';

        var item = document.createElement('button');
        item.type = 'button';
        item.className = 'mark-diagram-menu-item';
        item.innerHTML = '<span class="mark-diagram-menu-icon">💬</span>Ask to MarkAgent';
        item.addEventListener('click', function () {
            closeMenu();
            onAsk();
        });

        menu.appendChild(item);
        document.body.appendChild(menu);

        // Keep the menu inside the viewport.
        var rect = menu.getBoundingClientRect();
        var left = Math.min(x, window.innerWidth  - rect.width  - 8);
        var top  = Math.min(y, window.innerHeight - rect.height - 8);
        menu.style.left = Math.max(8, left) + 'px';
        menu.style.top  = Math.max(8, top)  + 'px';
    }

    // Closing the menu is global: one listener, not one per diagram.
    var globalClosersInstalled = false;
    function installGlobalClosers() {
        if (globalClosersInstalled) return;
        globalClosersInstalled = true;
        document.addEventListener('click', function (e) {
            if (!e.target.closest('#' + MENU_ID)) closeMenu();
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') closeMenu();
        });
        window.addEventListener('blur', closeMenu);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Wiring
    // ─────────────────────────────────────────────────────────────────────

    function askAbout(svg, box) {
        buildContext(svg, box).then(function (context) {
            try {
                window.parent.postMessage({ type: MSG_ASK, context: context }, '*');
            } catch (e) {
                console.error('[MarkDiagramContext] postMessage fallito:', e);
            }
        }).catch(function (err) {
            // The user asked a question and gets no answer: say why, in the
            // console at least, rather than failing silently.
            console.error('[MarkDiagramContext] Impossibile costruire il contesto del box:', err);
        });
    }

    function init(svg) {
        if (!svg || svg.tagName !== 'svg') return;
        if (initializedSvgs.has(svg)) return;
        initializedSvgs.add(svg);

        installGlobalClosers();

        svg.addEventListener('contextmenu', function (e) {
            var box = e.target.closest(SEL_BOXES);
            if (!box) return;              // right-click on empty canvas: browser menu

            e.preventDefault();
            e.stopPropagation();

            // Right-click selects *and* opens the menu, so "Ask to MarkAgent"
            // never runs without a box under it.
            if (typeof InteractiveSvg !== 'undefined' && box.id) {
                InteractiveSvg.selectElement(svg, box.id);
            }

            openMenu(e.clientX, e.clientY, function () {
                askAbout(svg, box);
            });
        });
    }

    function initAll() {
        document.querySelectorAll('svg').forEach(function (svg) {
            if (svg.getAttribute('data-diagram-type') === 'SEQUENCE') return;
            if (svg.querySelector(SEL_BOXES)) init(svg);
        });
    }

    // Public API
    return {
        init: init,
        initAll: initAll,
        // exposed for diagnostics and for the handler that composes the prompt
        buildContext: buildContext,
        readPlantumlSource: readPlantumlSource
    };

})();
