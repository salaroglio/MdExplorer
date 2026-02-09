/**
 * MdExplorer - Interactive SVG for PlantUML YAML Tree Diagrams
 * =============================================================
 * Makes PlantUML-generated @startyaml tree diagram SVGs interactive with click-to-highlight.
 *
 * Features:
 * - Click on any box to highlight its full path (ancestors + descendants)
 * - Click on a specific text row (bold function name) to follow only that branch
 * - Ancestors (upstream) = RED, Descendants (downstream) = GREEN
 * - Selected box = BLUE, Selected row = CYAN, Highlighted connections = ORANGE
 * - Non-related elements are dimmed
 * - ESC key or click outside to clear selection
 * - Coexists with InteractiveSvgYamlLinks (clickable url/link/href values)
 *
 * PlantUML @startyaml SVG Structure:
 * - Boxes: single <rect fill="#F1F1F1" rx="5" ry="5" style="stroke:#F1F1F1;stroke-width:1.5;">
 * - Text: <text> elements inside boxes (bold keys, regular values)
 * - Separators: <line> elements inside boxes
 * - Connections: groups of 3 SVG elements:
 *   1. <path> dashed (stroke-dasharray:3.0,3.0) - the arrow line
 *   2. <path> filled (#000000) - the arrowhead
 *   3. <ellipse> filled (rx="3" ry="3") - the origin dot
 *
 * Usage:
 *   InteractiveSvgYaml.init(svgElement);
 *   InteractiveSvgYaml.initAll();
 *   InteractiveSvgYaml.destroy(svgElement);
 *
 * CSS Required:
 *   Include interactive-svg-yaml.css for visual effects
 */

var InteractiveSvgYaml = (function() {
    'use strict';

    // Track initialized SVGs to avoid double-initialization
    var initializedSvgs = new WeakSet();

    /**
     * Check if an SVG is a YAML tree diagram
     * @param {SVGElement} svg
     * @returns {boolean}
     */
    function isYamlDiagram(svg) {
        // Must NOT be a component diagram (has elem_, cluster_, link_ groups)
        if (svg.querySelector('g[id^="elem_"], g[id^="cluster_"], g[id^="link_"]')) {
            return false;
        }

        // Must NOT be a sequence diagram (has participant boxes + dashed lifelines)
        var seqBoxes = svg.querySelectorAll('rect[fill="#E2E2F0"]');
        var seqLifelines = svg.querySelectorAll('line[style*="stroke-dasharray"]');
        if (seqBoxes.length > 0 && seqLifelines.length > 0) {
            return false;
        }

        // Must HAVE YAML tree characteristics:
        // - F1F1F1 rects with rounded corners (rx="5")
        var boxRects = svg.querySelectorAll('rect[fill="#F1F1F1"][rx="5"]');
        if (boxRects.length === 0) return false;

        // - Dashed paths (connections)
        var paths = svg.querySelectorAll('path');
        var hasDashedPaths = false;
        for (var j = 0; j < paths.length; j++) {
            var style = paths[j].getAttribute('style') || '';
            if (style.indexOf('stroke-dasharray:3.0,3.0') > -1) {
                hasDashedPaths = true;
                break;
            }
        }
        if (!hasDashedPaths) return false;

        // - Ellipses (origin dots)
        var ellipses = svg.querySelectorAll('ellipse[rx="3"]');
        if (ellipses.length === 0) return false;

        return true;
    }

    /**
     * Parse all boxes from the SVG.
     * Each box is a single <rect fill="#F1F1F1" rx="5" ry="5">.
     * @param {SVGElement} svg
     * @returns {Array} Array of box objects
     */
    function parseBoxes(svg) {
        var boxes = [];
        var boxRects = svg.querySelectorAll('rect[fill="#F1F1F1"][rx="5"]');
        var allTexts = Array.prototype.slice.call(svg.querySelectorAll('text'));
        var allLines = Array.prototype.slice.call(svg.querySelectorAll('line'));

        for (var i = 0; i < boxRects.length; i++) {
            var rect = boxRects[i];

            var x = parseFloat(rect.getAttribute('x'));
            var y = parseFloat(rect.getAttribute('y'));
            var w = parseFloat(rect.getAttribute('width'));
            var h = parseFloat(rect.getAttribute('height'));

            // Find text elements inside this box
            var textElements = [];
            var labelParts = [];
            for (var t = 0; t < allTexts.length; t++) {
                var txt = allTexts[t];
                var tx = parseFloat(txt.getAttribute('x'));
                var ty = parseFloat(txt.getAttribute('y'));
                if (tx >= x - 2 && tx <= x + w + 2 && ty >= y - 2 && ty <= y + h + 2) {
                    textElements.push(txt);
                    var content = (txt.textContent || '').trim();
                    if (content) labelParts.push(content);
                }
            }

            // Find separator lines inside this box
            var innerLines = [];
            for (var l = 0; l < allLines.length; l++) {
                var ln = allLines[l];
                var lx1 = parseFloat(ln.getAttribute('x1'));
                var ly1 = parseFloat(ln.getAttribute('y1'));
                var lx2 = parseFloat(ln.getAttribute('x2'));
                // Line is inside the box bounds
                if (lx1 >= x - 2 && lx2 <= x + w + 2 && ly1 >= y - 2 && ly1 <= y + h + 2) {
                    innerLines.push(ln);
                }
            }

            // Collect all SVG elements belonging to this box
            var elements = [rect];
            elements = elements.concat(textElements).concat(innerLines);

            boxes.push({
                rect: rect,
                textElements: textElements,
                lines: innerLines,
                elements: elements,
                x: x,
                y: y,
                width: w,
                height: h,
                label: labelParts.join(' ')
            });
        }

        return boxes;
    }

    /**
     * Extract the first M (moveto) coordinates from a path's d attribute
     * @param {string} d - The path's d attribute
     * @returns {{x: number, y: number}|null}
     */
    function extractFirstPoint(d) {
        var match = d.match(/M\s*([\d.]+)[,\s]+([\d.]+)/);
        if (match) {
            return { x: parseFloat(match[1]), y: parseFloat(match[2]) };
        }
        return null;
    }

    /**
     * Extract the last coordinate pair from a path's d attribute.
     * Handles M, L, and C commands (cubic bezier endpoint is the last pair).
     * @param {string} d - The path's d attribute
     * @returns {{x: number, y: number}|null}
     */
    function extractEndPoint(d) {
        // Extract all numbers from the path data
        var numbers = d.match(/[\d.]+/g);
        if (numbers && numbers.length >= 2) {
            return {
                x: parseFloat(numbers[numbers.length - 2]),
                y: parseFloat(numbers[numbers.length - 1])
            };
        }
        return null;
    }

    /**
     * Compute the minimum distance from a point to a box's nearest edge
     * @param {number} px
     * @param {number} py
     * @param {Object} box
     * @returns {number}
     */
    function distanceToBox(px, py, box) {
        // Clamp point to box range
        var closestX = Math.max(box.x, Math.min(px, box.x + box.width));
        var closestY = Math.max(box.y, Math.min(py, box.y + box.height));
        var dx = px - closestX;
        var dy = py - closestY;
        return Math.sqrt(dx * dx + dy * dy);
    }

    /**
     * Find the closest box to a point (within a maximum distance)
     * @param {number} px
     * @param {number} py
     * @param {Array} boxes
     * @param {number} maxDist - Maximum allowed distance
     * @returns {Object|null}
     */
    function findClosestBox(px, py, boxes, maxDist) {
        var closest = null;
        var minDist = maxDist;

        for (var i = 0; i < boxes.length; i++) {
            var dist = distanceToBox(px, py, boxes[i]);
            if (dist < minDist) {
                minDist = dist;
                closest = boxes[i];
            }
        }

        return closest;
    }

    /**
     * Parse connections between boxes using geometric matching.
     * Connections consist of: dashed path + arrowhead path + ellipse origin dot.
     * Elements are matched by proximity, not DOM order.
     * @param {SVGElement} svg
     * @param {Array} boxes
     * @returns {Array} Array of connection objects
     */
    function parseConnections(svg, boxes) {
        var connections = [];
        var allPaths = Array.prototype.slice.call(svg.querySelectorAll('path'));
        var allEllipses = Array.prototype.slice.call(svg.querySelectorAll('ellipse[rx="3"]'));
        var boxTolerance = 15;

        // Separate dashed paths and arrowhead paths
        var dashedPaths = [];
        var arrowheadPaths = [];

        for (var i = 0; i < allPaths.length; i++) {
            var p = allPaths[i];
            var style = p.getAttribute('style') || '';
            var fill = p.getAttribute('fill') || '';

            if (style.indexOf('stroke-dasharray:3.0,3.0') > -1) {
                dashedPaths.push(p);
            } else if (fill === '#000000' && style.indexOf('stroke-dasharray') === -1) {
                arrowheadPaths.push(p);
            }
        }

        // For each dashed path, find matching ellipse, arrowhead, source and target boxes
        for (var di = 0; di < dashedPaths.length; di++) {
            var path = dashedPaths[di];
            var d = path.getAttribute('d') || '';

            var startPoint = extractFirstPoint(d);
            var endPoint = extractEndPoint(d);
            if (!startPoint || !endPoint) continue;

            // Find nearest ellipse to start point
            var ellipse = null;
            var bestEllipseDist = 10;
            for (var ei = 0; ei < allEllipses.length; ei++) {
                var el = allEllipses[ei];
                var cx = parseFloat(el.getAttribute('cx'));
                var cy = parseFloat(el.getAttribute('cy'));
                var dist = Math.sqrt(Math.pow(cx - startPoint.x, 2) + Math.pow(cy - startPoint.y, 2));
                if (dist < bestEllipseDist) {
                    bestEllipseDist = dist;
                    ellipse = el;
                }
            }

            // Find nearest arrowhead to end point
            var arrowhead = null;
            var bestArrowDist = 20;
            for (var ai = 0; ai < arrowheadPaths.length; ai++) {
                var ah = arrowheadPaths[ai];
                var ahD = ah.getAttribute('d') || '';
                // Use centroid of arrowhead (average of all points)
                var ahNums = ahD.match(/[\d.]+/g);
                if (!ahNums || ahNums.length < 2) continue;
                var sumX = 0, sumY = 0, count = 0;
                for (var ni = 0; ni < ahNums.length - 1; ni += 2) {
                    sumX += parseFloat(ahNums[ni]);
                    sumY += parseFloat(ahNums[ni + 1]);
                    count++;
                }
                if (count === 0) continue;
                var centX = sumX / count;
                var centY = sumY / count;
                var adist = Math.sqrt(Math.pow(centX - endPoint.x, 2) + Math.pow(centY - endPoint.y, 2));
                if (adist < bestArrowDist) {
                    bestArrowDist = adist;
                    arrowhead = ah;
                }
            }

            // Find source box (near start point)
            var sourceBox = findClosestBox(startPoint.x, startPoint.y, boxes, boxTolerance);

            // Find target box (near end point or arrowhead)
            var targetBox = findClosestBox(endPoint.x, endPoint.y, boxes, boxTolerance);

            // If target not found by end point, try arrowhead centroid
            if (!targetBox && arrowhead) {
                var arrowTip = extractFirstPoint(arrowhead.getAttribute('d') || '');
                if (arrowTip) {
                    targetBox = findClosestBox(arrowTip.x, arrowTip.y, boxes, boxTolerance);
                }
            }

            if (sourceBox && targetBox && sourceBox !== targetBox) {
                var elements = [path];
                if (arrowhead) elements.push(arrowhead);
                if (ellipse) elements.push(ellipse);

                connections.push({
                    dashedPath: path,
                    arrowhead: arrowhead,
                    ellipse: ellipse,
                    sourceBox: sourceBox,
                    targetBox: targetBox,
                    startY: startPoint.y,
                    elements: elements
                });
            }
        }

        return connections;
    }

    /**
     * Build adjacency graph from boxes and connections
     * @param {Array} boxes
     * @param {Array} connections
     * @returns {Object} Graph with outgoing and incoming maps
     */
    function buildGraph(boxes, connections) {
        var outgoing = new Map(); // box -> [connections going out]
        var incoming = new Map(); // box -> [connections coming in]

        boxes.forEach(function(box) {
            outgoing.set(box, []);
            incoming.set(box, []);
        });

        connections.forEach(function(conn) {
            outgoing.get(conn.sourceBox).push(conn);
            incoming.get(conn.targetBox).push(conn);
        });

        return {
            outgoing: outgoing,
            incoming: incoming
        };
    }

    /**
     * Find the outgoing connection that corresponds to a specific text row.
     * Connection startY is ~6px above the text baseline Y.
     * @param {SVGTextElement} textEl - The clicked text element
     * @param {Object} box - The box containing the text
     * @param {Object} graph - The graph
     * @returns {Object|null} The matching connection, or null if no match
     */
    function findConnectionForRow(textEl, box, graph) {
        var textY = parseFloat(textEl.getAttribute('y'));
        var outConns = graph.outgoing.get(box) || [];
        if (outConns.length === 0) return null;

        var bestConn = null;
        var bestDist = 15; // max Y distance threshold

        for (var i = 0; i < outConns.length; i++) {
            var conn = outConns[i];
            var dist = Math.abs(conn.startY - textY);
            if (dist < bestDist) {
                bestDist = dist;
                bestConn = conn;
            }
        }

        return bestConn;
    }

    /**
     * Get all descendants starting from a single connection (follows only that branch)
     * @param {Object} conn - The starting connection
     * @param {Object} graph - The graph
     * @returns {{ boxes: Array, connections: Array }}
     */
    function getDescendantsFromConnection(conn, graph) {
        var visitedBoxes = new Set();
        var visitedConnections = [conn];
        var rowTexts = []; // specific row texts in intermediate boxes
        var queue = [conn.targetBox];

        while (queue.length > 0) {
            var current = queue.shift();
            if (visitedBoxes.has(current)) continue;
            visitedBoxes.add(current);

            var outConns = graph.outgoing.get(current) || [];
            for (var i = 0; i < outConns.length; i++) {
                var c = outConns[i];
                visitedConnections.push(c);

                // Find the specific row text in this box for the outgoing connection
                var connY = c.startY;
                var sourceTexts = current.textElements;
                var bestText = null;
                var bestDist = 15;
                for (var t = 0; t < sourceTexts.length; t++) {
                    var txt = sourceTexts[t];
                    if (txt.getAttribute('font-weight') !== 'bold') continue;
                    var ty = parseFloat(txt.getAttribute('y'));
                    var dist = Math.abs(ty - connY);
                    if (dist < bestDist) {
                        bestDist = dist;
                        bestText = txt;
                    }
                }
                if (bestText) {
                    rowTexts.push(bestText);
                    // Also include non-bold sibling on same Y
                    var bestY = parseFloat(bestText.getAttribute('y'));
                    for (var s = 0; s < sourceTexts.length; s++) {
                        var sib = sourceTexts[s];
                        if (sib === bestText) continue;
                        var sibY = parseFloat(sib.getAttribute('y'));
                        if (Math.abs(sibY - bestY) < 2) {
                            rowTexts.push(sib);
                        }
                    }
                }

                if (!visitedBoxes.has(c.targetBox)) {
                    queue.push(c.targetBox);
                }
            }
        }

        return {
            boxes: Array.from(visitedBoxes),
            connections: visitedConnections,
            rowTexts: rowTexts
        };
    }

    /**
     * Get all descendants (downstream) of a box via BFS
     * @param {Object} box - Starting box
     * @param {Object} graph - Graph from buildGraph
     * @returns {{ boxes: Array, connections: Array }}
     */
    function getDescendants(box, graph) {
        var visitedBoxes = new Set();
        var visitedConnections = [];
        var queue = [box];

        while (queue.length > 0) {
            var current = queue.shift();
            if (visitedBoxes.has(current)) continue;
            visitedBoxes.add(current);

            var outConns = graph.outgoing.get(current) || [];
            for (var i = 0; i < outConns.length; i++) {
                var conn = outConns[i];
                visitedConnections.push(conn);
                if (!visitedBoxes.has(conn.targetBox)) {
                    queue.push(conn.targetBox);
                }
            }
        }

        // Remove the starting box from the result
        visitedBoxes.delete(box);

        return {
            boxes: Array.from(visitedBoxes),
            connections: visitedConnections
        };
    }

    /**
     * Get all ancestors (upstream) of a box via BFS
     * @param {Object} box - Starting box
     * @param {Object} graph - Graph from buildGraph
     * @returns {{ boxes: Array, connections: Array }}
     */
    function getAncestors(box, graph) {
        var visitedBoxes = new Set();
        var visitedConnections = [];
        var queue = [box];

        while (queue.length > 0) {
            var current = queue.shift();
            if (visitedBoxes.has(current)) continue;
            visitedBoxes.add(current);

            var inConns = graph.incoming.get(current) || [];
            for (var i = 0; i < inConns.length; i++) {
                var conn = inConns[i];
                visitedConnections.push(conn);
                if (!visitedBoxes.has(conn.sourceBox)) {
                    queue.push(conn.sourceBox);
                }
            }
        }

        // Remove the starting box from the result
        visitedBoxes.delete(box);

        return {
            boxes: Array.from(visitedBoxes),
            connections: visitedConnections
        };
    }

    /**
     * Get all ancestors (upstream) with row-level precision.
     * For each ancestor connection, finds the specific bold text row in the source box
     * that corresponds to the connection's startY.
     * @param {Object} box - Starting box
     * @param {Object} graph - Graph from buildGraph
     * @returns {{ boxes: Array, connections: Array, rowTexts: Array }}
     */
    function getAncestorsWithRows(box, graph) {
        var visitedBoxes = new Set();
        var visitedConnections = [];
        var rowTexts = []; // specific text elements to highlight in ancestor boxes
        var queue = [box];

        while (queue.length > 0) {
            var current = queue.shift();
            if (visitedBoxes.has(current)) continue;
            visitedBoxes.add(current);

            var inConns = graph.incoming.get(current) || [];
            for (var i = 0; i < inConns.length; i++) {
                var conn = inConns[i];
                visitedConnections.push(conn);

                if (!visitedBoxes.has(conn.sourceBox)) {
                    queue.push(conn.sourceBox);

                    // Find the specific row text in the source box for this connection
                    var connY = conn.startY;
                    var bestText = null;
                    var bestDist = 15;
                    var sourceTexts = conn.sourceBox.textElements;
                    for (var t = 0; t < sourceTexts.length; t++) {
                        var txt = sourceTexts[t];
                        if (txt.getAttribute('font-weight') !== 'bold') continue;
                        var ty = parseFloat(txt.getAttribute('y'));
                        var dist = Math.abs(ty - connY);
                        if (dist < bestDist) {
                            bestDist = dist;
                            bestText = txt;
                        }
                    }
                    if (bestText) {
                        rowTexts.push(bestText);
                        // Also include the non-bold sibling text on same Y
                        var bestY = parseFloat(bestText.getAttribute('y'));
                        for (var s = 0; s < sourceTexts.length; s++) {
                            var sib = sourceTexts[s];
                            if (sib === bestText) continue;
                            var sibY = parseFloat(sib.getAttribute('y'));
                            if (Math.abs(sibY - bestY) < 2) {
                                rowTexts.push(sib);
                            }
                        }
                    }
                }
            }
        }

        // Remove the starting box from the result
        visitedBoxes.delete(box);

        return {
            boxes: Array.from(visitedBoxes),
            connections: visitedConnections,
            rowTexts: rowTexts
        };
    }

    /**
     * Clear all highlight classes from the SVG
     * @param {SVGElement} svg
     */
    function clearSelection(svg) {
        destroyTooltips(svg);

        var classes = ['yaml-box-source', 'yaml-box-downstream', 'yaml-box-upstream',
                       'yaml-connection-highlighted', 'yaml-row-selected', 'yaml-row-leaf'];

        svg.querySelectorAll('.' + classes.join(', .'))
            .forEach(function(el) {
                classes.forEach(function(cls) {
                    el.classList.remove(cls);
                });
            });

        svg.classList.remove('yaml-has-selection');
    }

    /**
     * Find the bold text label in a box that corresponds to a connection's startY.
     * @param {Object} conn - The connection
     * @returns {string} The function name, or empty string
     */
    function getRowLabelForConnection(conn) {
        var connY = conn.startY;
        var sourceTexts = conn.sourceBox.textElements;
        var bestText = null;
        var bestDist = 15;

        for (var t = 0; t < sourceTexts.length; t++) {
            var txt = sourceTexts[t];
            if (txt.getAttribute('font-weight') !== 'bold') continue;
            var ty = parseFloat(txt.getAttribute('y'));
            var dist = Math.abs(ty - connY);
            if (dist < bestDist) {
                bestDist = dist;
                bestText = txt;
            }
        }

        return bestText ? (bestText.textContent || '').trim() : '';
    }

    var SVG_NS = 'http://www.w3.org/2000/svg';

    /**
     * Show an SVG tooltip (rect + text) above a box inside the same SVG.
     * @param {SVGElement} svg - The SVG element
     * @param {Object} box - The box to show tooltip above
     * @param {string} label - The tooltip text
     */
    function showSvgTooltip(svg, box, label) {
        // Remove any existing tooltip
        hideSvgTooltip(svg);

        var padding = 8;
        var fontSize = 15;
        var arrowSize = 6;

        // Create group
        var g = document.createElementNS(SVG_NS, 'g');
        g.setAttribute('id', 'yaml-tooltip');
        g.style.pointerEvents = 'none';

        // Create text first to measure it
        var text = document.createElementNS(SVG_NS, 'text');
        text.setAttribute('font-size', fontSize);
        text.setAttribute('fill', '#ffffff');
        text.setAttribute('font-weight', 'bold');
        text.setAttribute('font-family', 'Consolas, monospace');
        text.classList.add('yaml-tooltip-text');
        // Strip "(NN righe)" suffix and show only the function name
        var cleanLabel = label.replace(/\s*\(\d+\s*righe?\)\s*$/, '').trim();
        text.textContent = cleanLabel;
        g.appendChild(text);
        svg.appendChild(g);

        // Measure text
        var textBBox = text.getBBox();
        var rectW = textBBox.width + padding * 2;
        var rectH = textBBox.height + padding * 2;

        // Position: centered above the box
        var tooltipX = box.x + box.width / 2 - rectW / 2;
        var tooltipY = box.y - rectH - arrowSize - 4;

        // Background rect
        var rect = document.createElementNS(SVG_NS, 'rect');
        rect.setAttribute('x', tooltipX);
        rect.setAttribute('y', tooltipY);
        rect.setAttribute('width', rectW);
        rect.setAttribute('height', rectH);
        rect.setAttribute('rx', '5');
        rect.setAttribute('ry', '5');
        rect.setAttribute('fill', '#1a1a2e');
        rect.setAttribute('stroke', '#FF9800');
        rect.setAttribute('stroke-width', '1.5');
        rect.classList.add('yaml-tooltip-bg');
        rect.style.filter = 'drop-shadow(0 2px 6px rgba(0,0,0,0.5))';
        g.insertBefore(rect, text);

        // Position text inside rect
        text.setAttribute('x', tooltipX + padding);
        text.setAttribute('y', tooltipY + padding + textBBox.height * 0.8);

        // Arrow (small triangle pointing down)
        var arrow = document.createElementNS(SVG_NS, 'polygon');
        var arrowCx = box.x + box.width / 2;
        var arrowTop = tooltipY + rectH;
        var points = (arrowCx - arrowSize) + ',' + arrowTop + ' ' +
                     arrowCx + ',' + (arrowTop + arrowSize) + ' ' +
                     (arrowCx + arrowSize) + ',' + arrowTop;
        arrow.setAttribute('points', points);
        arrow.setAttribute('fill', '#1a1a2e');
        arrow.classList.add('yaml-tooltip-arrow');
        g.appendChild(arrow);
    }

    /**
     * Hide the SVG tooltip
     * @param {SVGElement} svg
     */
    function hideSvgTooltip(svg) {
        var existing = svg.querySelector('#yaml-tooltip');
        if (existing) existing.remove();
    }

    /**
     * Create hover handlers on highlighted boxes showing their parent function name as SVG tooltip.
     * @param {Array} boxes - Highlighted boxes
     * @param {Array} connections - The connections in the path
     * @param {SVGElement} svg - The SVG element
     */
    function createParentTooltips(boxes, connections, svg) {
        // Build a map: targetBox → connection (to find parent info)
        var boxToConn = new Map();
        connections.forEach(function(conn) {
            if (!boxToConn.has(conn.targetBox)) {
                boxToConn.set(conn.targetBox, conn);
            }
        });

        var handlers = svg._yamlData.tooltipHandlers || [];

        boxes.forEach(function(box) {
            var conn = boxToConn.get(box);
            if (!conn) return;

            var parentLabel = getRowLabelForConnection(conn);
            if (!parentLabel) return;

            var hideTimeout = null;

            var showHandler = function() {
                if (hideTimeout) { clearTimeout(hideTimeout); hideTimeout = null; }
                showSvgTooltip(svg, box, parentLabel);
            };

            var hideHandler = function() {
                hideTimeout = setTimeout(function() {
                    hideSvgTooltip(svg);
                }, 80);
            };

            // Attach to ALL elements of the box (rect, texts, lines)
            box.elements.forEach(function(el) {
                if (!el) return;
                el.addEventListener('mouseenter', showHandler);
                el.addEventListener('mouseleave', hideHandler);
                handlers.push({ el: el, show: showHandler, hide: hideHandler });
            });
        });

        svg._yamlData.tooltipHandlers = handlers;
    }

    /**
     * Destroy all tooltip handlers for the current selection
     * @param {SVGElement} svg
     */
    function destroyTooltips(svg) {
        if (!svg._yamlData || !svg._yamlData.tooltipHandlers) return;

        svg._yamlData.tooltipHandlers.forEach(function(h) {
            h.el.removeEventListener('mouseenter', h.show);
            h.el.removeEventListener('mouseleave', h.hide);
        });
        svg._yamlData.tooltipHandlers = [];

        hideSvgTooltip(svg);
    }

    /**
     * Apply a CSS class to all elements of a box
     * @param {Object} box
     * @param {string} className
     */
    function applyBoxClass(box, className) {
        box.elements.forEach(function(el) {
            if (el) el.classList.add(className);
        });
    }

    /**
     * Apply a CSS class to all elements of a connection
     * @param {Object} conn
     * @param {string} className
     */
    function applyConnectionClass(conn, className) {
        conn.elements.forEach(function(el) {
            if (el) el.classList.add(className);
        });
    }

    /**
     * Handle click on a box
     * @param {Object} box - The clicked box
     * @param {SVGElement} svg - The SVG element
     * @param {Object} graph - The graph structure
     * @param {Object} options - Options
     */
    function handleBoxClick(box, svg, graph, options) {
        clearSelection(svg);

        // Mark SVG as having an active selection
        svg.classList.add('yaml-has-selection');

        // Selected box → BLUE
        applyBoxClass(box, 'yaml-box-source');

        // Descendants (downstream) → GREEN
        var descendants = getDescendants(box, graph);
        descendants.boxes.forEach(function(b) {
            applyBoxClass(b, 'yaml-box-downstream');
        });
        descendants.connections.forEach(function(c) {
            applyConnectionClass(c, 'yaml-connection-highlighted');
        });

        // Ancestors (upstream) → RED
        var ancestors = getAncestors(box, graph);
        ancestors.boxes.forEach(function(b) {
            applyBoxClass(b, 'yaml-box-upstream');
        });
        ancestors.connections.forEach(function(c) {
            applyConnectionClass(c, 'yaml-connection-highlighted');
        });

        // Create tooltips on all highlighted boxes (source + downstream + upstream)
        var allHighlightedConns = descendants.connections.concat(ancestors.connections);
        var allHighlightedBoxes = [box].concat(descendants.boxes).concat(ancestors.boxes);
        createParentTooltips(allHighlightedBoxes, allHighlightedConns, svg);

        if (options && options.onSelect) {
            options.onSelect({
                type: 'yaml-box',
                label: box.label,
                ancestorCount: ancestors.boxes.length,
                descendantCount: descendants.boxes.length
            });
        }
    }

    /**
     * Handle click on a specific text row (bold function name) within a box.
     * Only follows the single outgoing connection for that row.
     * @param {SVGTextElement} textEl - The clicked text element
     * @param {Object} box - The box containing the text
     * @param {SVGElement} svg - The SVG element
     * @param {Object} graph - The graph structure
     * @param {Object} options - Options
     */
    function handleRowClick(textEl, box, svg, graph, options) {
        var conn = findConnectionForRow(textEl, box, graph);
        if (!conn) {
            // No outgoing connection for this row - yellow highlight + ancestors only
            clearSelection(svg);
            svg.classList.add('yaml-has-selection');
            applyBoxClass(box, 'yaml-box-source');
            var textY = parseFloat(textEl.getAttribute('y'));
            box.textElements.forEach(function(t) {
                var ty = parseFloat(t.getAttribute('y'));
                if (Math.abs(ty - textY) < 2) {
                    t.classList.add('yaml-row-leaf');
                }
            });

            // Still trace ancestors upstream
            var ancestors = getAncestorsWithRows(box, graph);
            ancestors.boxes.forEach(function(b) {
                applyBoxClass(b, 'yaml-box-upstream');
            });
            ancestors.connections.forEach(function(c) {
                applyConnectionClass(c, 'yaml-connection-highlighted');
            });
            ancestors.rowTexts.forEach(function(t) {
                t.classList.add('yaml-row-selected');
            });

            // Tooltips on ancestor boxes + source box
            var allConns = ancestors.connections;
            var allBoxes = [box].concat(ancestors.boxes);
            createParentTooltips(allBoxes, allConns, svg);
            return;
        }

        clearSelection(svg);
        svg.classList.add('yaml-has-selection');

        // Highlight the source box
        applyBoxClass(box, 'yaml-box-source');

        // Highlight the clicked row text (and its sibling on the same Y)
        var textY = parseFloat(textEl.getAttribute('y'));
        box.textElements.forEach(function(t) {
            var ty = parseFloat(t.getAttribute('y'));
            if (Math.abs(ty - textY) < 2) {
                t.classList.add('yaml-row-selected');
            }
        });

        // Follow only this connection's branch downstream
        var descendants = getDescendantsFromConnection(conn, graph);
        descendants.boxes.forEach(function(b) {
            applyBoxClass(b, 'yaml-box-downstream');
        });
        descendants.connections.forEach(function(c) {
            applyConnectionClass(c, 'yaml-connection-highlighted');
        });
        // Highlight specific row texts in descendant boxes
        descendants.rowTexts.forEach(function(t) {
            t.classList.add('yaml-row-selected');
        });

        // Ancestors (upstream) - row-aware
        var ancestors = getAncestorsWithRows(box, graph);
        ancestors.boxes.forEach(function(b) {
            applyBoxClass(b, 'yaml-box-upstream');
        });
        ancestors.connections.forEach(function(c) {
            applyConnectionClass(c, 'yaml-connection-highlighted');
        });
        // Highlight specific row texts in ancestor boxes
        ancestors.rowTexts.forEach(function(t) {
            t.classList.add('yaml-row-selected');
        });

        // Create tooltips on all highlighted boxes (source + downstream + upstream)
        var allHighlightedConns = descendants.connections.concat(ancestors.connections);
        var allHighlightedBoxes = [box].concat(descendants.boxes).concat(ancestors.boxes);
        createParentTooltips(allHighlightedBoxes, allHighlightedConns, svg);

        if (options && options.onSelect) {
            options.onSelect({
                type: 'yaml-row',
                label: textEl.textContent,
                boxLabel: box.label,
                descendantCount: descendants.boxes.length
            });
        }
    }

    /**
     * Initialize interactivity on a YAML tree diagram SVG
     * @param {SVGElement} svg - The SVG element
     * @param {Object} options - Optional configuration
     * @returns {boolean} Whether initialization succeeded
     */
    function init(svg, options) {
        if (!svg || svg.tagName !== 'svg') {
            return false;
        }

        if (!isYamlDiagram(svg)) {
            return false;
        }

        if (initializedSvgs.has(svg)) {
            return true;
        }

        options = options || {};

        console.log('[InteractiveSvgYaml] Parsing YAML tree diagram...');

        // Parse diagram structure
        var boxes = parseBoxes(svg);
        var connections = parseConnections(svg, boxes);
        var graph = buildGraph(boxes, connections);

        console.log('[InteractiveSvgYaml] Found', boxes.length, 'boxes and', connections.length, 'connections');

        if (boxes.length === 0) {
            console.warn('[InteractiveSvgYaml] No boxes found');
            return false;
        }

        // Mark as initialized
        initializedSvgs.add(svg);
        svg.classList.add('interactive-svg-yaml');

        // Store data for destroy
        svg._yamlData = {
            boxes: boxes,
            connections: connections,
            graph: graph,
            options: options,
            tippyInstances: []
        };

        // Add click handlers to box elements
        boxes.forEach(function(box) {
            box.elements.forEach(function(el) {
                if (el) {
                    el.style.cursor = 'pointer';
                    el.addEventListener('click', function(e) {
                        // Don't interfere with InteractiveSvgYamlLinks clickable text
                        if (e.target.classList.contains('yaml-link-text')) return;
                        e.stopPropagation();

                        // If clicking on a bold text element (function name), do row-level selection
                        if (e.target.tagName === 'text' && e.target.getAttribute('font-weight') === 'bold') {
                            handleRowClick(e.target, box, svg, graph, options);
                        } else {
                            handleBoxClick(box, svg, graph, options);
                        }
                    });
                }
            });
        });

        // Click on SVG background to clear
        svg.addEventListener('click', function(e) {
            // Only clear if clicking on the SVG background itself
            if (e.target === svg || e.target.tagName === 'svg') {
                clearSelection(svg);
                if (options.onClear) options.onClear();
            }
        });

        // ESC key to clear
        var escHandler = function(e) {
            if (e.key === 'Escape' && svg.classList.contains('yaml-has-selection')) {
                clearSelection(svg);
                if (options.onClear) options.onClear();
            }
        };
        document.addEventListener('keydown', escHandler);
        svg._yamlData.escHandler = escHandler;

        console.log('[InteractiveSvgYaml] Initialized successfully');
        return true;
    }

    /**
     * Initialize all YAML tree diagram SVGs on page
     * @param {Object} options - Optional configuration
     */
    function initAll(options) {
        document.querySelectorAll('svg').forEach(function(svg) {
            init(svg, options);
        });
    }

    /**
     * Destroy interactivity on a YAML tree diagram SVG
     * @param {SVGElement} svg
     */
    function destroy(svg) {
        if (!svg || !initializedSvgs.has(svg)) return;

        if (svg._yamlData && svg._yamlData.escHandler) {
            document.removeEventListener('keydown', svg._yamlData.escHandler);
        }

        clearSelection(svg);
        svg.classList.remove('interactive-svg-yaml');
        delete svg._yamlData;
        initializedSvgs.delete(svg);
    }

    // Public API
    return {
        init: init,
        initAll: initAll,
        destroy: destroy,
        isYamlDiagram: isYamlDiagram
    };

})();
