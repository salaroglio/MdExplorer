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
 * - GMN*      : Note elements
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
                       'destination-incoming', 'destination-note', 'link-highlighted'];

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

        svg.classList.remove('has-selection', 'interactive-svg-active');
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
     */
    function findBoxByName(svg, name) {
        // Legacy format
        var el = svg.querySelector('#elem_' + name) ||
                 svg.querySelector('#cluster_' + name) ||
                 svg.querySelector('#' + name);
        if (el) return el;

        // New format: find by data-qualified-name
        el = svg.querySelector('g[data-qualified-name="' + name + '"]');
        return el;
    }

    function handleBoxClick(boxElement, svg, linkMap, onSelect) {
        clearSelection(svg);

        var boxName = getElementName(boxElement);

        // Mark source as selected (BLUE)
        boxElement.classList.add('source-selected');
        highlightEllipses(boxElement, '#2196F3');
        svg.classList.add('has-selection');
        svg.classList.add('interactive-svg-active');

        var connectedBoxes = [];
        var outCount = 0, inCount = 0;

        // Find all OUTGOING links (selected -> other = GREEN for receivers)
        var outgoing = linkMap.outgoing[boxName] || [];
        outgoing.forEach(function(item) {
            item.link.classList.add('link-highlighted');
            outCount++;
            if (connectedBoxes.indexOf(item.to) === -1) {
                connectedBoxes.push(item.to);
            }

            var destElem = findBoxByName(svg, item.to);
            if (destElem) {
                // Note boxes (GMN*) = YELLOW, others = GREEN (receiving info)
                if (item.to.startsWith('GMN')) {
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
            item.link.classList.add('link-highlighted');
            inCount++;
            if (connectedBoxes.indexOf(item.from) === -1) {
                connectedBoxes.push(item.from);
            }

            var sourceElem = findBoxByName(svg, item.from);
            if (sourceElem && sourceElem !== boxElement) {
                // Note boxes (GMN*) = YELLOW, others = RED (sending info)
                if (item.from.startsWith('GMN')) {
                    sourceElem.classList.add('destination-note');
                    highlightEllipses(sourceElem, '#FFC107');
                } else {
                    sourceElem.classList.add('destination-incoming');
                    highlightEllipses(sourceElem, '#f44336');
                }
            }
        });

        // Call callback if provided
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
    return {
        init: init,
        initAll: initAll,
        destroy: destroy,
        selectElement: selectElement,
        clear: clear
    };

})();
