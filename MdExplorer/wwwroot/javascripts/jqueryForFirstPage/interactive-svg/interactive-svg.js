/**
 * MdExplorer - Interactive SVG for PlantUML Diagrams
 * ===================================================
 * Makes PlantUML-generated SVG diagrams interactive with click-to-highlight functionality.
 *
 * Features:
 * - Click on any box (cluster, element, note) to highlight all connections
 * - Source box highlighted in BLUE
 * - Connected boxes highlighted in RED
 * - Links highlighted in ORANGE
 * - Non-related elements are dimmed
 * - ESC key or click outside to clear selection
 * - Works with any PlantUML SVG following standard naming conventions
 *
 * PlantUML SVG Conventions (legacy format, pre-2026):
 * - cluster_* : Package/container elements
 * - elem_*    : Component/element boxes
 * - notes     : recognised by shape (folded corner), not by name — see isNoteBox()
 * - link_*    : Connection arrows (format: link_SourceName_TargetName)
 *
 * PlantUML SVG Conventions (new format, v1.2026.1+):
 * - g.cluster  : Package/container elements (data-qualified-name)
 * - g.entity   : Component/element boxes (data-qualified-name)
 * - g.link     : Connection arrows (data-entity-1, data-entity-2)
 *
 * Usage:
 *   InteractiveSvg.init(svgElement);        // Initialize on a single SVG
 *   InteractiveSvg.initAll();               // Initialize all SVGs on page
 *   InteractiveSvg.destroy(svgElement);     // Remove interactivity
 *
 * CSS Required:
 *   Include interactive-svg.css for visual effects
 */

var InteractiveSvg = (function() {
    'use strict';

    // Track initialized SVGs to avoid double-initialization
    var initializedSvgs = new WeakSet();

    // Selectors for both legacy (pre-2026) and new (v1.2026.1+) PlantUML SVG formats
    var SEL_BOXES_LEGACY = 'g[id^="elem_"], g[id^="cluster_"], g[id^="GMN"]';
    var SEL_BOXES_NEW    = 'g.entity, g.cluster';
    var SEL_BOXES        = SEL_BOXES_LEGACY + ', ' + SEL_BOXES_NEW;
    var SEL_LINKS_LEGACY = 'g[id^="link_"]';
    var SEL_LINKS_NEW    = 'g.link';
    var SEL_LINKS        = SEL_LINKS_LEGACY + ', ' + SEL_LINKS_NEW;

    /**
     * Detect if SVG uses the new PlantUML v1.2026.1+ format
     */
    function isNewFormat(svg) {
        return !!svg.querySelector('g.entity, g.cluster, g.link');
    }

    /**
     * Build a map from entity IDs (ent0001) to qualified names for new format SVGs
     */
    function buildEntityIdMap(svg) {
        var map = {};
        svg.querySelectorAll('g.entity, g.cluster').forEach(function(el) {
            var qname = el.getAttribute('data-qualified-name');
            if (qname && el.id) {
                map[el.id] = qname;
            }
        });
        return map;
    }

    /**
     * Apply format-agnostic marker classes so the CSS can target boxes and links
     * without knowing whether the SVG uses the legacy (id^="elem_"/"cluster_"/"link_"/"GMN")
     * or the new (g.entity/g.cluster/g.link) PlantUML naming.
     *
     *   - .interactive-svg-box   : on every clickable box (entity, cluster, GMN note, elem_*)
     *   - .interactive-svg-link  : on every link group
     *   - .interactive-svg-note  : extra marker for note boxes, recognised by SHAPE (see isNoteBox)
     */
    function applyMarkerClasses(svg) {
        svg.querySelectorAll(SEL_BOXES).forEach(function(box) {
            box.classList.add('interactive-svg-box');
            if (isNoteBox(box)) {
                box.classList.add('interactive-svg-note');
            }
        });
        svg.querySelectorAll(SEL_LINKS).forEach(function(link) {
            link.classList.add('interactive-svg-link');
            // New format only: the UML kind drives the highlight colour (CSS .link-type-*)
            var type = link.getAttribute('data-link-type');
            if (type) link.classList.add('link-type-' + type);
        });
    }

    /**
     * Is this box a PlantUML note?
     *
     * The name cannot tell: a note may be anonymous ("GMN62", legacy id "elem_GMN62")
     * or named ("note as NotaDocumentoGara"). The shape can: a note has no <rect>,
     * its outline is a <path>, and the next <path> is the folded corner — a right
     * triangle M(x,y) L(x,y+h) L(x+h,y+h) L(x,y). Same drawing in both SVG formats.
     */
    function isNoteBox(box) {
        if (box.querySelector(':scope > rect')) return false;
        var paths = box.querySelectorAll(':scope > path');
        if (paths.length < 2) return false;
        var nums = (paths[1].getAttribute('d') || '').match(/-?\d+(?:\.\d+)?/g);
        if (!nums || nums.length !== 8) return false;
        var p = nums.map(parseFloat);
        return p[0] === p[2] && p[3] > p[1] &&      // down the left side of the fold
               p[5] === p[3] && p[4] > p[2] &&      // across the bottom of the fold
               p[6] === p[0] && p[7] === p[1];      // back to the start
    }

    function isNote(el) {
        return !!(el && el.classList && el.classList.contains('interactive-svg-note'));
    }

    // How far (SVG units) a speech-bubble tip may sit from the box border it points at.
    // Measured on real diagrams: <= 0.4.
    var NOTE_TAIL_TOLERANCE = 3;

    /**
     * Vertices of a note outline path ("M x,y L x,y A rx,ry rot large sweep x,y …").
     */
    function pathVertices(d) {
        var pts = [];
        var re = /([MLA])([^MLAZ]*)/gi, m;
        while ((m = re.exec(d || '')) !== null) {
            var n = (m[2].match(/-?\d+(?:\.\d+)?/g) || []).map(parseFloat);
            if (n.length >= 2) pts.push({ x: n[n.length - 2], y: n[n.length - 1] });
        }
        return pts;
    }

    /**
     * Tip of a note's speech bubble, or null when the note has none.
     *
     * A note with a single relation is drawn by PlantUML as a bubble: no g.link exists,
     * the tail is three extra vertices inside the note outline. The tip is the only
     * vertex lying outside the bounding box of all the others (= the note body).
     */
    function findNoteTailTip(noteBox) {
        var outline = noteBox.querySelector(':scope > path');
        if (!outline) return null;
        var pts = pathVertices(outline.getAttribute('d'));
        var tips = pts.filter(function(p, i) {
            var others = pts.filter(function(_, j) { return j !== i; });
            var minX = Math.min.apply(null, others.map(function(o) { return o.x; }));
            var maxX = Math.max.apply(null, others.map(function(o) { return o.x; }));
            var minY = Math.min.apply(null, others.map(function(o) { return o.y; }));
            var maxY = Math.max.apply(null, others.map(function(o) { return o.y; }));
            return p.x < minX - 1 || p.x > maxX + 1 || p.y < minY - 1 || p.y > maxY + 1;
        });
        return tips.length === 1 ? tips[0] : null;
    }

    /**
     * Frame of a box: its <rect> (classes, packages) or, failing that, its bbox.
     */
    function boxFrame(box) {
        var r = box.querySelector(':scope > rect');
        if (r) {
            return {
                x: parseFloat(r.getAttribute('x')), y: parseFloat(r.getAttribute('y')),
                w: parseFloat(r.getAttribute('width')), h: parseFloat(r.getAttribute('height'))
            };
        }
        try {
            var b = box.getBBox();
            return (b.width || b.height) ? { x: b.x, y: b.y, w: b.width, h: b.height } : null;
        } catch (e) {
            return null;   // not rendered (e.g. hidden iframe): no geometry to compare
        }
    }

    /**
     * Distance from a point to the BORDER of a frame (0 when on it). A point inside a
     * package is far from its border, so a class touched by the tip wins over the
     * package that contains it.
     */
    function distanceToBorder(f, x, y) {
        var dx = Math.max(f.x - x, 0, x - (f.x + f.w));
        var dy = Math.max(f.y - y, 0, y - (f.y + f.h));
        if (dx > 0 || dy > 0) return Math.sqrt(dx * dx + dy * dy);
        return Math.min(x - f.x, f.x + f.w - x, y - f.y, f.y + f.h - y);
    }

    /**
     * Pair every speech-bubble note with the box its tip touches.
     * Classes win over packages; an unclear match is reported and skipped, never guessed.
     *
     * @returns {Array<{note: Element, target: Element}>}
     */
    function findNoteTails(svg) {
        var candidates = [];
        svg.querySelectorAll('.interactive-svg-box:not(.interactive-svg-note)').forEach(function(box) {
            var f = boxFrame(box);
            if (f) candidates.push({ box: box, frame: f, cluster: isClusterElement(box) });
        });

        var tails = [];
        svg.querySelectorAll('.interactive-svg-note').forEach(function(note) {
            var tip = findNoteTailTip(note);
            if (!tip) return;

            var near = candidates
                .map(function(c) { return { c: c, d: distanceToBorder(c.frame, tip.x, tip.y) }; })
                .filter(function(h) { return h.d <= NOTE_TAIL_TOLERANCE; });
            if (near.some(function(h) { return !h.c.cluster; })) {
                near = near.filter(function(h) { return !h.c.cluster; });
            }
            near.sort(function(a, b) { return a.d - b.d; });

            if (near.length === 0 || (near.length > 1 && near[1].d - near[0].d < 0.5)) {
                console.warn('[InteractiveSvg] Note "' + getElementName(note) + '": ' +
                    (near.length ? 'tip touches more than one box' : 'no box at the tip') +
                    ' (' + tip.x + ',' + tip.y + '), not attached');
                return;
            }
            tails.push({ note: note, target: near[0].c.box });
        });
        return tails;
    }

    /**
     * Remove the marker classes applied by applyMarkerClasses (used by destroy()).
     */
    function removeMarkerClasses(svg) {
        svg.querySelectorAll('.interactive-svg-box, .interactive-svg-link, .interactive-svg-note')
           .forEach(function(el) {
               el.classList.remove('interactive-svg-box', 'interactive-svg-link', 'interactive-svg-note');
               var type = el.getAttribute('data-link-type');
               if (type) el.classList.remove('link-type-' + type);
           });
    }

    // Safe CSS ident pattern (no Unicode, but covers all PlantUML-generated identifiers)
    var RE_CSS_IDENT = /^-?[A-Za-z_][\w-]*$/;

    function isValidCssIdent(name) {
        return typeof name === 'string' && RE_CSS_IDENT.test(name);
    }

    function escapeAttrValue(s) {
        // Escape backslash and double-quote for use inside an [attr="..."] selector
        return String(s).replace(/\\/g, '\\\\').replace(/"/g, '\\"');
    }

    var _wrapperStyleInjected = false;
    function injectWrapperStyle() {
        if (_wrapperStyleInjected) return;
        _wrapperStyleInjected = true;
        var s = document.createElement('style');
        s.textContent = '.svg-zoom-viewport::-webkit-scrollbar{display:none}';
        document.head.appendChild(s);
    }

    function ensureWrapper(svg) {
        if (svg._zoomWrapper) return svg._zoomWrapper;
        injectWrapperStyle();
        var parent = svg.parentNode;
        var wrapper = document.createElement('div');
        wrapper.className = 'svg-zoom-viewport';
        wrapper.style.maxWidth = '100%';
        wrapper.style.overflow = 'auto';
        wrapper.style.scrollbarWidth = 'none';
        parent.insertBefore(wrapper, svg);
        wrapper.appendChild(svg);
        svg._zoomWrapper = wrapper;
        return wrapper;
    }

    // Global Ctrl+wheel prevention: blocks browser/Electron page zoom for the
    // entire iframe so Ctrl+wheel only works on the SVG (via the SVG-level handler).
    var _globalCtrlWheelHandler = null;

    function installGlobalCtrlWheelPrevention() {
        if (_globalCtrlWheelHandler) return;
        _globalCtrlWheelHandler = function(e) {
            if (e.ctrlKey) e.preventDefault();
        };
        window.addEventListener('wheel', _globalCtrlWheelHandler, { passive: false });
    }

    /**
     * Setup Ctrl+wheel zoom on the SVG element.
     * @param {SVGElement} svg
     */
    function setupWheelZoom(svg) {
        var ZOOM_STEP = 0.2;
        var MIN_ZOOM  = 0.2;
        var MAX_ZOOM  = 5.0;
        var data = svg._interactiveSvgData;

        var wheelHandler = function(e) {
            if (!e.ctrlKey) return;
            e.preventDefault();

            if (!data.zoomLevel) data.zoomLevel = 1.0;

            // Capture current rendered size as zoom base on first wheel event
            if (!data.zoomBaseW) {
                var renderRect = svg.getBoundingClientRect();
                data.zoomBaseW = renderRect.width;
                data.zoomBaseH = renderRect.height;
            }

            // Capture cursor position as fraction of SVG before resizing
            var rect = svg.getBoundingClientRect();
            var fractionX = rect.width  > 0 ? (e.clientX - rect.left)  / rect.width  : 0.5;
            var fractionY = rect.height > 0 ? (e.clientY - rect.top)   / rect.height : 0.5;

            var direction = e.deltaY < 0 ? 1 : -1;
            data.zoomLevel = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM,
                data.zoomLevel + direction * ZOOM_STEP));

            svg.style.maxWidth = 'none';
            svg.style.width  = Math.round(data.zoomBaseW * data.zoomLevel) + 'px';
            svg.style.height = Math.round(data.zoomBaseH * data.zoomLevel) + 'px';

            var newRect = svg.getBoundingClientRect();
            var dxComp = (newRect.left + fractionX * newRect.width)  - e.clientX;
            var dyComp = (newRect.top  + fractionY * newRect.height) - e.clientY;
            var wrapper = svg._zoomWrapper;
            if (wrapper && wrapper.scrollWidth > wrapper.clientWidth) {
                wrapper.scrollLeft += dxComp;
            } else {
                window.scrollBy({ left: dxComp, behavior: 'instant' });
            }
            window.scrollBy({ top: dyComp, behavior: 'instant' });
        };

        svg.addEventListener('wheel', wheelHandler, { passive: false });
        data.wheelHandler = wheelHandler;
    }

    /**
     * Setup grab-to-pan on the SVG: mousedown + drag scrolls the iframe viewport.
     * A plain click (no drag) is not suppressed, so click-to-select still works.
     * @param {SVGElement} svg
     */
    function setupPanDrag(svg) {
        var DRAG_THRESHOLD = 4;
        var data = svg._interactiveSvgData;
        var isPanning  = false;
        var hasDragged = false;
        var lastX, lastY;

        svg.style.cursor = 'grab';

        var mousedownHandler = function(e) {
            if (e.button !== 0) return;
            isPanning  = true;
            hasDragged = false;
            lastX = e.clientX;
            lastY = e.clientY;
            e.preventDefault();
        };

        var mousemoveHandler = function(e) {
            if (!isPanning) return;
            var dx = e.clientX - lastX;
            var dy = e.clientY - lastY;

            if (!hasDragged &&
                (Math.abs(dx) > DRAG_THRESHOLD || Math.abs(dy) > DRAG_THRESHOLD)) {
                hasDragged = true;
                document.documentElement.style.setProperty('cursor', 'grabbing', 'important');
            }

            if (hasDragged) {
                var wrapper = svg._zoomWrapper;
                if (wrapper && wrapper.scrollWidth > wrapper.clientWidth) {
                    wrapper.scrollLeft -= dx;
                } else {
                    window.scrollBy({ left: -dx, behavior: 'instant' });
                }
                window.scrollBy({ top: -dy, behavior: 'instant' });
                lastX = e.clientX;
                lastY = e.clientY;
            }
        };

        var cancelNextClick = function(e) {
            e.stopPropagation();
            document.removeEventListener('click', cancelNextClick, true);
        };

        var mouseupHandler = function(e) {
            if (!isPanning) return;
            isPanning = false;
            document.documentElement.style.removeProperty('cursor');
            svg.style.cursor = 'grab';

            if (hasDragged) {
                document.addEventListener('click', cancelNextClick, true);
            }
        };

        svg.addEventListener('mousedown', mousedownHandler);
        document.addEventListener('mousemove', mousemoveHandler);
        document.addEventListener('mouseup',   mouseupHandler);

        data.panHandlers = {
            mousedown: mousedownHandler,
            mousemove: mousemoveHandler,
            mouseup:   mouseupHandler
        };
    }

    /**
     * Parse PlantUML link ID to extract source and target names
     * PlantUML generates IDs like: link_SourceName_TargetName or link_SourceName_TargetName-1
     *
     * @param {string} linkId - The link element ID
     * @returns {Object|null} - { from: string, to: string } or null if invalid
     */
    function parseLinkId(linkId, knownNames) {
        // Remove "link_" prefix
        var withoutPrefix = linkId.replace(/^link_/, '');
        // Remove any suffix like "-1", "-2" etc.
        var withoutSuffix = withoutPrefix.replace(/-\d+$/, '');
        var parts = withoutSuffix.split('_');

        if (parts.length < 2) return null;

        // Context-aware parsing: when element names contain underscores (e.g. API_CLIENT),
        // try all possible split points and match against known element names
        if (knownNames && knownNames.size > 0) {
            for (var i = 1; i < parts.length; i++) {
                var candidateFrom = parts.slice(0, i).join('_');
                var candidateTo = parts.slice(i).join('_');
                if (knownNames.has(candidateFrom) && knownNames.has(candidateTo)) {
                    return { from: candidateFrom, to: candidateTo };
                }
            }
        }

        return null;
    }

    /**
     * Get element name from a <g> element.
     * New format: uses data-qualified-name attribute.
     * Legacy format: strips prefix from ID.
     *
     * @param {Element|string} elOrId - DOM element or element ID
     * @returns {string} - Clean element name
     */
    function getElementName(elOrId) {
        // If a DOM element is passed, prefer data-qualified-name (new format)
        if (elOrId && typeof elOrId === 'object' && elOrId.getAttribute) {
            var qname = elOrId.getAttribute('data-qualified-name');
            if (qname) return qname;
            elOrId = elOrId.id || '';
        }
        var id = elOrId;
        if (id.startsWith('elem_')) return id.replace('elem_', '');
        if (id.startsWith('cluster_')) return id.replace('cluster_', '');
        if (id.startsWith('GMN')) return id;
        return id;
    }

    /**
     * Format name for display (add spaces before capitals)
     *
     * @param {string} name - Element name
     * @returns {string} - Formatted name
     */
    function formatName(name) {
        return name
            .replace(/([A-Z])/g, ' $1')
            .replace(/^./, function(s) { return s.toUpperCase(); })
            .trim();
    }

    /**
     * Build a map of all links for quick lookup
     *
     * @param {SVGElement} svg - The SVG element
     * @returns {Object} - { outgoing: {}, incoming: {} }
     */
    function buildLinkMap(svg) {
        var linkMap = { outgoing: {}, incoming: {} };
        var useNew = isNewFormat(svg);
        var entityIdMap = useNew ? buildEntityIdMap(svg) : {};

        // Collect all known element names for context-aware link parsing
        var knownNames = new Set();
        svg.querySelectorAll(SEL_BOXES_LEGACY + ', ' + SEL_BOXES_NEW).forEach(function(el) {
            knownNames.add(getElementName(el));
        });
        linkMap.knownNames = knownNames;

        // Map from entity IDs to qualified names (for new format link resolution)
        linkMap.entityIdMap = entityIdMap;

        svg.querySelectorAll(SEL_LINKS).forEach(function(link) {
            var from, to;

            if (useNew && link.classList.contains('link')) {
                // New format: data-entity-1 / data-entity-2 contain entity IDs like "ent0004"
                var eid1 = link.getAttribute('data-entity-1');
                var eid2 = link.getAttribute('data-entity-2');
                from = entityIdMap[eid1] || eid1;
                to = entityIdMap[eid2] || eid2;
            } else {
                // Legacy format: parse from ID or data-from/data-to
                from = link.dataset.from;
                to = link.dataset.to;
                if (!from || !to) {
                    var parsed = parseLinkId(link.id, knownNames);
                    if (parsed) {
                        from = from || parsed.from;
                        to = to || parsed.to;
                    }
                }
            }

            if (from && to) {
                if (!linkMap.outgoing[from]) linkMap.outgoing[from] = [];
                if (!linkMap.incoming[to]) linkMap.incoming[to] = [];
                linkMap.outgoing[from].push({ link: link, to: to });
                linkMap.incoming[to].push({ link: link, from: from });
            }
        });

        // Speech-bubble notes have no g.link: add a virtual note -> box relation
        // (link: null, nothing to paint) so both clicks find each other.
        linkMap.noteTails = findNoteTails(svg).map(function(t) {
            var note = getElementName(t.note), target = getElementName(t.target);
            if (!linkMap.outgoing[note]) linkMap.outgoing[note] = [];
            if (!linkMap.incoming[target]) linkMap.incoming[target] = [];
            linkMap.outgoing[note].push({ link: null, to: target });
            linkMap.incoming[target].push({ link: null, from: note });
            return { from: note, to: target };
        });

        return linkMap;
    }

    /**
     * Clear all selection classes from SVG
     *
     * @param {SVGElement} svg - The SVG element
     */
    function clearSelection(svg) {
        var classes = ['selected', 'selected-green', 'selected-orange', 'selected-cyan',
                       'source-selected', 'destination', 'destination-outgoing',
                       'destination-incoming', 'destination-note', 'cluster-contained',
                       'link-highlighted'];

        svg.querySelectorAll('.' + classes.join(', .')).forEach(function(el) {
            classes.forEach(function(cls) {
                el.classList.remove(cls);
            });
        });

        // Restore original inline styles on ellipses
        svg.querySelectorAll('ellipse[data-orig-style]').forEach(function(el) {
            el.setAttribute('style', el.getAttribute('data-orig-style'));
            el.removeAttribute('data-orig-style');
        });

        svg.classList.remove('has-selection', 'interactive-svg-active', 'interactive-svg-note-lit');
    }

    /**
     * Flag the SVG while at least one note is lit. In dark theme the CSS then moves the
     * invert filter from the <svg> onto its elements, sparing the lit notes: under the
     * root filter a bright yellow does not exist (it comes out #4A3B00).
     */
    function markLitNotes(svg) {
        svg.classList.toggle('interactive-svg-note-lit', !!svg.querySelector('.destination-note'));
    }

    /**
     * Apply highlight styles directly on ellipse elements inside a group.
     * PlantUML use-case diagrams render use cases as <ellipse> with inline styles
     * that can prevent CSS class-based overrides from taking effect.
     */
    function highlightEllipses(gElement, color) {
        gElement.querySelectorAll('ellipse').forEach(function(el) {
            if (!el.hasAttribute('data-orig-style')) {
                el.setAttribute('data-orig-style', el.getAttribute('style') || '');
            }
            el.style.stroke = color;
            el.style.strokeWidth = '3';
            el.style.filter = 'drop-shadow(0 0 8px ' + color + ') drop-shadow(0 0 16px ' + color + ')';
        });
    }

    /**
     * Find a box element by its qualified name, supporting both old and new formats.
     *
     * The new format (PlantUML v1.2026.1+) can produce qualified names containing
     * spaces, dots and other characters that are NOT valid CSS identifiers
     * (e.g. "Layer 1 . COBOL.Programma . Chiamate Esterne.CobolFunction"). Building
     * a `#elem_<name>` selector from such a name makes querySelector throw a
     * SyntaxError, which previously aborted the click handler mid-forEach and broke
     * incoming/outgoing highlighting.
     *
     * Order matters here:
     *   1. attribute selector — always safe, just escape `\` and `"` in the value
     *   2. ID selectors — only attempted when the name is a valid CSS identifier
     */
    function findBoxByName(svg, name) {
        if (!name) return null;

        // 1. New format: g[data-qualified-name="..."]
        var el = svg.querySelector('g[data-qualified-name="' + escapeAttrValue(name) + '"]');
        if (el) return el;

        // 2. Legacy format: #elem_*, #cluster_*, #GMN* — only if name is a valid ident
        if (isValidCssIdent(name)) {
            el = svg.querySelector('#elem_' + name) ||
                 svg.querySelector('#cluster_' + name) ||
                 svg.querySelector('#' + name);
            if (el) return el;
        }

        return null;
    }

    /**
     * Is this box a cluster (package/container)?
     */
    function isClusterElement(el) {
        if (!el) return false;
        if (el.classList && el.classList.contains('cluster')) return true;       // new format
        if (el.id && el.id.indexOf('cluster_') === 0) return true;                // legacy
        return false;
    }

    /**
     * Find all DOM siblings that conceptually live INSIDE a cluster.
     *
     * PlantUML does NOT nest g.cluster/g.entity DOM-wise — they are all flat siblings.
     * The containment is encoded only in the `data-qualified-name` attribute
     * (new format v1.2026.1+): every box inside "Layer 1 . COBOL" has a qname
     * that begins with "Layer 1 . COBOL." (note the trailing dot separator).
     *
     * For legacy format we cannot determine descendants from the ID alone, so
     * we return an empty array — the cluster click then degrades to the normal
     * entity behaviour (highlight outgoing/incoming of the cluster itself).
     *
     * @returns {{ qnames: Set<string>, els: Element[] }}
     */
    function findClusterDescendants(svg, cluster) {
        var qname = cluster.getAttribute && cluster.getAttribute('data-qualified-name');
        if (!qname) return { qnames: new Set(), els: [] };

        var prefix = qname + '.';
        var qnames = new Set();
        var els = [];
        svg.querySelectorAll('g.entity, g.cluster').forEach(function(el) {
            if (el === cluster) return;
            var q = el.getAttribute('data-qualified-name');
            if (q && q.indexOf(prefix) === 0) {
                qnames.add(q);
                els.push(el);
            }
        });
        return { qnames: qnames, els: els };
    }

    /**
     * Resolve a link group's endpoints to qualified names (or legacy names),
     * using the linkMap context (entityIdMap, knownNames).
     */
    function resolveLinkEndpoints(linkEl, linkMap) {
        var from, to;
        if (linkEl.classList.contains('link')) {
            // New format
            var eid1 = linkEl.getAttribute('data-entity-1');
            var eid2 = linkEl.getAttribute('data-entity-2');
            from = linkMap.entityIdMap[eid1] || eid1;
            to = linkMap.entityIdMap[eid2] || eid2;
        } else {
            from = linkEl.dataset.from;
            to = linkEl.dataset.to;
            if (!from || !to) {
                var parsed = parseLinkId(linkEl.id, linkMap.knownNames);
                if (parsed) {
                    from = from || parsed.from;
                    to = to || parsed.to;
                }
            }
        }
        return { from: from, to: to };
    }

    /**
     * Highlight a cluster: mark every contained box and every link that touches
     * the cluster (internal, outgoing, or incoming). External endpoints reached
     * via outgoing/incoming links are coloured GREEN/RED as for entity click.
     */
    function handleClusterClick(cluster, clusterName, svg, linkMap, onSelect) {
        var descendants = findClusterDescendants(svg, cluster);

        // 1. Light up all descendants (cluster-contained — BLU soft)
        descendants.els.forEach(function(el) {
            el.classList.add('cluster-contained');
            highlightEllipses(el, '#64B5F6');
        });

        // 2. Walk every link to classify it relative to the cluster boundary
        var internalCount = 0, outCount = 0, inCount = 0;
        var externalConnected = [];

        function markExternal(name, kind) {
            if (externalConnected.indexOf(name) !== -1) return;
            externalConnected.push(name);
            var el = findBoxByName(svg, name);
            if (!el || el === cluster) return;
            if (isNote(el)) {
                el.classList.add('destination-note');
                highlightEllipses(el, '#FFC107');
            } else if (kind === 'outgoing') {
                el.classList.add('destination-outgoing');
                highlightEllipses(el, '#4CAF50');
            } else {
                el.classList.add('destination-incoming');
                highlightEllipses(el, '#f44336');
            }
        }

        svg.querySelectorAll('g.interactive-svg-link').forEach(function(linkEl) {
            var ep = resolveLinkEndpoints(linkEl, linkMap);
            if (!ep.from || !ep.to) return;

            var fromInside = (ep.from === clusterName) || descendants.qnames.has(ep.from);
            var toInside   = (ep.to   === clusterName) || descendants.qnames.has(ep.to);

            if (fromInside && toInside) {
                linkEl.classList.add('link-highlighted');
                internalCount++;
            } else if (fromInside && !toInside) {
                linkEl.classList.add('link-highlighted');
                outCount++;
                markExternal(ep.to, 'outgoing');
            } else if (!fromInside && toInside) {
                linkEl.classList.add('link-highlighted');
                inCount++;
                markExternal(ep.from, 'incoming');
            }
        });

        // Speech-bubble notes: same boundary rule, no link to paint
        (linkMap.noteTails || []).forEach(function(t) {
            var fromInside = (t.from === clusterName) || descendants.qnames.has(t.from);
            var toInside   = (t.to   === clusterName) || descendants.qnames.has(t.to);
            if (fromInside && !toInside) {
                outCount++;
                markExternal(t.to, 'outgoing');
            } else if (!fromInside && toInside) {
                inCount++;
                markExternal(t.from, 'incoming');
            }
        });

        markLitNotes(svg);

        if (onSelect && typeof onSelect === 'function') {
            onSelect({
                type: 'cluster',
                element: cluster,
                name: clusterName,
                formattedName: formatName(clusterName),
                descendantCount: descendants.els.length,
                internalLinkCount: internalCount,
                outgoingCount: outCount,
                incomingCount: inCount,
                connectedBoxes: externalConnected.map(formatName)
            });
        }
    }

    function handleBoxClick(boxElement, svg, linkMap, onSelect) {
        clearSelection(svg);

        var boxName = getElementName(boxElement);

        // Mark source as selected (BLUE)
        boxElement.classList.add('source-selected');
        highlightEllipses(boxElement, '#2196F3');
        svg.classList.add('has-selection');
        svg.classList.add('interactive-svg-active');

        // Cluster click → light up all contents + all touching links
        if (isClusterElement(boxElement)) {
            return handleClusterClick(boxElement, boxName, svg, linkMap, onSelect);
        }

        var connectedBoxes = [];
        var outCount = 0, inCount = 0;

        // Find all OUTGOING links (selected -> other = GREEN for receivers)
        var outgoing = linkMap.outgoing[boxName] || [];
        outgoing.forEach(function(item) {
            if (item.link) item.link.classList.add('link-highlighted');
            outCount++;
            if (connectedBoxes.indexOf(item.to) === -1) {
                connectedBoxes.push(item.to);
            }

            var destElem = findBoxByName(svg, item.to);
            if (destElem) {
                // Note boxes = YELLOW, others = GREEN (receiving info)
                if (isNote(destElem)) {
                    destElem.classList.add('destination-note');
                    highlightEllipses(destElem, '#FFC107');
                } else {
                    destElem.classList.add('destination-outgoing');
                    highlightEllipses(destElem, '#4CAF50');
                }
            }
        });

        // Find all INCOMING links (other -> selected = RED for senders)
        var incoming = linkMap.incoming[boxName] || [];
        incoming.forEach(function(item) {
            if (item.link) item.link.classList.add('link-highlighted');
            inCount++;
            if (connectedBoxes.indexOf(item.from) === -1) {
                connectedBoxes.push(item.from);
            }

            var sourceElem = findBoxByName(svg, item.from);
            if (sourceElem && sourceElem !== boxElement) {
                // Note boxes = YELLOW, others = RED (sending info)
                if (isNote(sourceElem)) {
                    sourceElem.classList.add('destination-note');
                    highlightEllipses(sourceElem, '#FFC107');
                } else {
                    sourceElem.classList.add('destination-incoming');
                    highlightEllipses(sourceElem, '#f44336');
                }
            }
        });

        // Call callback if provided
        markLitNotes(svg);

        if (onSelect && typeof onSelect === 'function') {
            onSelect({
                element: boxElement,
                name: boxName,
                formattedName: formatName(boxName),
                outgoingCount: outCount,
                incomingCount: inCount,
                connectedBoxes: connectedBoxes.map(formatName)
            });
        }
    }

    /**
     * Add hit areas to links for easier clicking
     *
     * @param {SVGElement} svg - The SVG element
     */
    function addLinkHitAreas(svg) {
        svg.querySelectorAll(SEL_LINKS).forEach(function(linkGroup) {
            // Check if hit area already exists
            if (linkGroup.querySelector('.hit-area')) return;

            var path = linkGroup.querySelector('path');
            if (path) {
                var hitArea = path.cloneNode();
                hitArea.classList.add('hit-area');
                hitArea.setAttribute('stroke', '#000000');
                hitArea.setAttribute('stroke-opacity', '0');
                hitArea.setAttribute('stroke-width', '30');
                hitArea.setAttribute('fill', 'none');
                hitArea.removeAttribute('stroke-dasharray');
                hitArea.style.pointerEvents = 'painted';
                hitArea.style.cursor = 'pointer';
                linkGroup.appendChild(hitArea);
            }
        });
    }

    /**
     * Move links to end of SVG so they render on top of boxes
     *
     * @param {SVGElement} svg - The SVG element
     */
    function reorderLinks(svg) {
        var mainGroup = svg.querySelector('g');
        if (!mainGroup) return;

        var links = Array.from(svg.querySelectorAll(SEL_LINKS));
        links.forEach(function(link) {
            mainGroup.appendChild(link);
        });
    }

    /**
     * Initialize interactivity on an SVG element
     *
     * @param {SVGElement} svg - The SVG element to initialize
     * @param {Object} options - Optional configuration
     * @param {Function} options.onSelect - Callback when element is selected
     * @param {Function} options.onClear - Callback when selection is cleared
     */
    function init(svg, options) {
        if (!svg || svg.tagName !== 'svg') {
            console.warn('[InteractiveSvg] Invalid SVG element provided');
            return;
        }

        if (initializedSvgs.has(svg)) {
            console.log('[InteractiveSvg] SVG already initialized, skipping');
            return;
        }

        options = options || {};

        // Mark as initialized
        initializedSvgs.add(svg);
        svg.classList.add('interactive-svg');

        // Tag every box and link with format-agnostic marker classes so the CSS
        // works for both legacy (id^="elem_"…) and new (g.entity/g.link) formats.
        applyMarkerClasses(svg);

        // Prepare SVG
        reorderLinks(svg);
        addLinkHitAreas(svg);

        // Build link map
        var linkMap = buildLinkMap(svg);

        // Store data on SVG for later access
        svg._interactiveSvgData = {
            linkMap: linkMap,
            options: options
        };

        // Block browser/Electron Ctrl+wheel zoom for the whole iframe
        installGlobalCtrlWheelPrevention();

        // Setup zoom and pan
        ensureWrapper(svg);
        setupWheelZoom(svg);
        setupPanDrag(svg);

        // Click handlers for boxes (both legacy and new format)
        svg.querySelectorAll(SEL_BOXES).forEach(function(box) {
            box.addEventListener('click', function(e) {
                e.stopPropagation();
                handleBoxClick(box, svg, linkMap, options.onSelect);
            });
        });

        // Click handlers for links (both legacy and new format)
        svg.querySelectorAll(SEL_LINKS).forEach(function(linkGroup) {
            linkGroup.addEventListener('click', function(e) {
                e.stopPropagation();
                clearSelection(svg);
                linkGroup.classList.add('link-highlighted');
                svg.classList.add('has-selection');
                svg.classList.add('interactive-svg-active');

                if (options.onSelect) {
                    var from, to;
                    if (linkGroup.classList.contains('link')) {
                        // New format
                        var eid1 = linkGroup.getAttribute('data-entity-1');
                        var eid2 = linkGroup.getAttribute('data-entity-2');
                        from = linkMap.entityIdMap[eid1] || eid1;
                        to = linkMap.entityIdMap[eid2] || eid2;
                    } else {
                        // Legacy format
                        from = linkGroup.dataset.from;
                        to = linkGroup.dataset.to;
                        if (!from || !to) {
                            var parsed = parseLinkId(linkGroup.id, linkMap.knownNames);
                            if (parsed) {
                                from = from || parsed.from;
                                to = to || parsed.to;
                            }
                        }
                    }
                    options.onSelect({
                        type: 'link',
                        element: linkGroup,
                        from: formatName(from || 'Unknown'),
                        to: formatName(to || 'Unknown')
                    });
                }
            });
        });

        // Click outside to clear
        svg.addEventListener('click', function(e) {
            var clickedBox = e.target.closest(SEL_BOXES);
            var clickedLink = e.target.closest(SEL_LINKS);
            if (!clickedBox && !clickedLink) {
                clearSelection(svg);
                if (options.onClear) options.onClear();
            }
        });

        // ESC key handler (document level, but only for this SVG)
        var escHandler = function(e) {
            if (e.key === 'Escape' && svg.classList.contains('interactive-svg-active')) {
                clearSelection(svg);
                if (options.onClear) options.onClear();
            }
        };
        document.addEventListener('keydown', escHandler);
        svg._interactiveSvgData.escHandler = escHandler;

        console.log('[InteractiveSvg] Initialized SVG with', Object.keys(linkMap.outgoing).length, 'source elements');
    }

    /**
     * Initialize all PlantUML SVGs on the page
     *
     * @param {Object} options - Optional configuration for all SVGs
     */
    function initAll(options) {
        // Look for SVGs that contain PlantUML component/class diagram elements (legacy or new format)
        document.querySelectorAll('svg').forEach(function(svg) {
            // Skip sequence diagrams (handled by InteractiveSvgSequence)
            var diagramType = svg.getAttribute('data-diagram-type');
            if (diagramType === 'SEQUENCE') return;

            var hasPlantUmlElements = svg.querySelector(SEL_BOXES + ', ' + SEL_LINKS);
            if (hasPlantUmlElements) {
                init(svg, options);
            }
        });
    }

    /**
     * Remove interactivity from an SVG
     *
     * @param {SVGElement} svg - The SVG element
     */
    function destroy(svg) {
        if (!svg || !initializedSvgs.has(svg)) return;

        // Remove ESC handler
        if (svg._interactiveSvgData && svg._interactiveSvgData.escHandler) {
            document.removeEventListener('keydown', svg._interactiveSvgData.escHandler);
        }

        // Remove zoom handler
        if (svg._interactiveSvgData && svg._interactiveSvgData.wheelHandler) {
            svg.removeEventListener('wheel', svg._interactiveSvgData.wheelHandler);
        }

        // Remove pan handlers
        if (svg._interactiveSvgData && svg._interactiveSvgData.panHandlers) {
            svg.removeEventListener('mousedown', svg._interactiveSvgData.panHandlers.mousedown);
            document.removeEventListener('mousemove', svg._interactiveSvgData.panHandlers.mousemove);
            document.removeEventListener('mouseup',   svg._interactiveSvgData.panHandlers.mouseup);
        }

        // Clear selection and classes
        clearSelection(svg);
        svg.classList.remove('interactive-svg');

        // Remove the format-agnostic marker classes added in init()
        removeMarkerClasses(svg);

        // Remove hit areas
        svg.querySelectorAll('.hit-area').forEach(function(el) {
            el.remove();
        });

        // Clean up
        delete svg._interactiveSvgData;
        initializedSvgs.delete(svg);

        console.log('[InteractiveSvg] Destroyed SVG interactivity');
    }

    /**
     * Programmatically select an element
     *
     * @param {SVGElement} svg - The SVG element
     * @param {string} elementId - ID of element to select
     */
    function selectElement(svg, elementId) {
        if (!svg || !initializedSvgs.has(svg)) return;

        var element = svg.querySelector('#' + elementId);
        if (element && svg._interactiveSvgData) {
            handleBoxClick(element, svg, svg._interactiveSvgData.linkMap,
                          svg._interactiveSvgData.options.onSelect);
        }
    }

    /**
     * Clear selection on an SVG
     *
     * @param {SVGElement} svg - The SVG element
     */
    function clear(svg) {
        if (!svg) return;
        clearSelection(svg);
        if (svg._interactiveSvgData && svg._interactiveSvgData.options.onClear) {
            svg._interactiveSvgData.options.onClear();
        }
    }

    // Public API
    // Box colours as painted by interactive-svg.css (stroke/glow of the state classes).
    // ⚠️ Keep in step with the CSS: box colours are literals there, not variables.
    var LEGEND_BOXES = [
        { key: 'selected', color: '#2196F3', label: 'Selected' },
        { key: 'incoming', color: '#f44336', label: 'Points to the selected one' },
        { key: 'outgoing', color: '#4CAF50', label: 'Receives from the selected one' },
        { key: 'note',     color: '#FFEB3B', label: 'Note' },
        { key: 'cluster',  color: '#64B5F6', label: 'Inside the selected package' }
    ];

    var LEGEND_LINK_LABELS = {
        extension:   'Inheritance / realization',
        composition: 'Composition',
        aggregation: 'Aggregation',
        dependency:  'Dependency / directed (-->)',
        association: 'Association'
    };

    var LINK_DEFAULT_COLOR = '#FF9800';

    /**
     * What the colours mean in THIS diagram: the box states it can show and only the
     * relation kinds it contains. Link colours are read from --link-hl on the real
     * elements, so the CSS stays the single source for them.
     *
     * @returns {{boxes: Array<{key,color,label}>, links: Array<{key,color,label}>}}
     */
    function getLegend(svg) {
        var hasLinks = !!svg.querySelector('.interactive-svg-link');
        var boxes = LEGEND_BOXES.filter(function(b) {
            if (b.key === 'note') return !!svg.querySelector('.interactive-svg-note');
            if (b.key === 'cluster') return !!svg.querySelector('g.cluster, g[id^="cluster_"]');
            return hasLinks;
        });

        var links = [], seen = {}, hasOther = false;
        svg.querySelectorAll('.interactive-svg-link').forEach(function(link) {
            var type = link.getAttribute('data-link-type');
            if (!type || !LEGEND_LINK_LABELS[type]) { hasOther = true; return; }
            if (seen[type]) return;
            seen[type] = true;
            var color = getComputedStyle(link).getPropertyValue('--link-hl').trim();
            links.push({ key: type, color: color || LINK_DEFAULT_COLOR, label: LEGEND_LINK_LABELS[type] });
        });
        var order = Object.keys(LEGEND_LINK_LABELS);
        links.sort(function(a, b) { return order.indexOf(a.key) - order.indexOf(b.key); });
        if (hasOther) {
            // 'other' next to typed arrows, plain 'relation' when no arrow has a type (legacy SVG)
            links.push(links.length
                ? { key: 'other',    color: LINK_DEFAULT_COLOR, label: 'Other relation' }
                : { key: 'relation', color: LINK_DEFAULT_COLOR, label: 'Relation' });
        }
        return { boxes: boxes, links: links };
    }

    return {
        init: init,
        initAll: initAll,
        destroy: destroy,
        selectElement: selectElement,
        clear: clear,
        getLegend: getLegend
    };

})();
