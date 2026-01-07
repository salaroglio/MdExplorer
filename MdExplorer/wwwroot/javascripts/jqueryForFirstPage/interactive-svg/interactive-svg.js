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
 * PlantUML SVG Conventions:
 * - cluster_* : Package/container elements
 * - elem_*    : Component/element boxes
 * - GMN*      : Note elements
 * - link_*    : Connection arrows (format: link_SourceName_TargetName)
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

    /**
     * Parse PlantUML link ID to extract source and target names
     * PlantUML generates IDs like: link_SourceName_TargetName or link_SourceName_TargetName-1
     *
     * @param {string} linkId - The link element ID
     * @returns {Object|null} - { from: string, to: string } or null if invalid
     */
    function parseLinkId(linkId) {
        // Remove "link_" prefix
        var withoutPrefix = linkId.replace(/^link_/, '');
        // Remove any suffix like "-1", "-2" etc.
        var withoutSuffix = withoutPrefix.replace(/-\d+$/, '');
        // Split by underscore - first part is source, rest is target
        var parts = withoutSuffix.split('_');
        if (parts.length >= 2) {
            return {
                from: parts[0],
                to: parts.slice(1).join('_')  // Handle target names with underscores
            };
        }
        return null;
    }

    /**
     * Get element name from its ID (strips prefix)
     *
     * @param {string} id - Element ID
     * @returns {string} - Clean element name
     */
    function getElementName(id) {
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

        svg.querySelectorAll('g[id^="link_"]').forEach(function(link) {
            // Try data attributes first, fallback to parsing ID
            var from = link.dataset.from;
            var to = link.dataset.to;

            if (!from || !to) {
                var parsed = parseLinkId(link.id);
                if (parsed) {
                    from = from || parsed.from;
                    to = to || parsed.to;
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

        svg.classList.remove('has-selection', 'interactive-svg-active');
    }

    /**
     * Handle click on a box element
     *
     * @param {Element} boxElement - The clicked box element
     * @param {SVGElement} svg - The parent SVG
     * @param {Object} linkMap - The link map for this SVG
     * @param {Function} onSelect - Optional callback when element is selected
     */
    function handleBoxClick(boxElement, svg, linkMap, onSelect) {
        clearSelection(svg);

        var boxId = boxElement.id;
        var boxName = getElementName(boxId);

        // Mark source as selected (BLUE)
        boxElement.classList.add('source-selected');
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

            var destElem = svg.querySelector('#elem_' + item.to) ||
                           svg.querySelector('#cluster_' + item.to) ||
                           svg.querySelector('#' + item.to);
            if (destElem) {
                // Note boxes (GMN*) = YELLOW, others = GREEN (receiving info)
                if (item.to.startsWith('GMN')) {
                    destElem.classList.add('destination-note');
                } else {
                    destElem.classList.add('destination-outgoing');
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

            var sourceElem = svg.querySelector('#elem_' + item.from) ||
                             svg.querySelector('#cluster_' + item.from) ||
                             svg.querySelector('#' + item.from);
            if (sourceElem && sourceElem !== boxElement) {
                // Note boxes (GMN*) = YELLOW, others = RED (sending info)
                if (item.from.startsWith('GMN')) {
                    sourceElem.classList.add('destination-note');
                } else {
                    sourceElem.classList.add('destination-incoming');
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
        svg.querySelectorAll('g[id^="link_"]').forEach(function(linkGroup) {
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

        var links = Array.from(svg.querySelectorAll('g[id^="link_"]'));
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

        // Click handlers for boxes
        svg.querySelectorAll('g[id^="elem_"], g[id^="cluster_"], g[id^="GMN"]').forEach(function(box) {
            box.addEventListener('click', function(e) {
                e.stopPropagation();
                handleBoxClick(box, svg, linkMap, options.onSelect);
            });
        });

        // Click handlers for links
        svg.querySelectorAll('g[id^="link_"]').forEach(function(linkGroup) {
            linkGroup.addEventListener('click', function(e) {
                e.stopPropagation();
                clearSelection(svg);
                linkGroup.classList.add('link-highlighted');
                svg.classList.add('has-selection');
                svg.classList.add('interactive-svg-active');

                if (options.onSelect) {
                    var from = linkGroup.dataset.from;
                    var to = linkGroup.dataset.to;
                    if (!from || !to) {
                        var parsed = parseLinkId(linkGroup.id);
                        if (parsed) {
                            from = from || parsed.from;
                            to = to || parsed.to;
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
            var clickedBox = e.target.closest('g[id^="elem_"], g[id^="cluster_"], g[id^="GMN"]');
            var clickedLink = e.target.closest('g[id^="link_"]');
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
        // Look for SVGs that contain PlantUML elements
        document.querySelectorAll('svg').forEach(function(svg) {
            var hasPlantUmlElements = svg.querySelector('g[id^="elem_"], g[id^="cluster_"], g[id^="link_"]');
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
