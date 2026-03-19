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
 * - Collapse/Expand sub-trees via toggle buttons on non-leaf boxes
 * - Collapse All / Expand All controls (top-right of SVG)
 * - ViewBox auto-resizes when collapsing/expanding
 * - Nested collapse: expanding a parent preserves child collapse state
 * - ESC key: first clears highlight, then expands all if no highlight
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
        // PlantUML versions may emit "3.0,3.0" or "3,3" (no decimals)
        var paths = svg.querySelectorAll('path');
        var hasDashedPaths = false;
        for (var j = 0; j < paths.length; j++) {
            var style = paths[j].getAttribute('style') || '';
            if (style.indexOf('stroke-dasharray:3.0,3.0') > -1 ||
                style.indexOf('stroke-dasharray:3,3') > -1) {
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

        // PlantUML generates TWO rects per box:
        // 1. fill="#F1F1F1" rx="5" - the background rect
        // 2. fill="none" stroke:#000000 - the outline/border rect (no rx)
        // We need to track BOTH to properly hide boxes.
        var outlineRects = Array.prototype.slice.call(
            svg.querySelectorAll('rect[fill="none"]')
        ).filter(function(r) {
            var style = r.getAttribute('style') || '';
            return style.indexOf('stroke:#000000') > -1;
        });

        for (var i = 0; i < boxRects.length; i++) {
            var rect = boxRects[i];

            var x = parseFloat(rect.getAttribute('x'));
            var y = parseFloat(rect.getAttribute('y'));
            var w = parseFloat(rect.getAttribute('width'));
            var h = parseFloat(rect.getAttribute('height'));

            // Find the matching outline rect (fill="none", stroke:#000000)
            // by overlapping position (same x,y origin within tolerance)
            var matchedOutline = null;
            for (var o = 0; o < outlineRects.length; o++) {
                var oRect = outlineRects[o];
                var ox = parseFloat(oRect.getAttribute('x'));
                var oy = parseFloat(oRect.getAttribute('y'));
                if (Math.abs(ox - x) < 3 && Math.abs(oy - y) < 3) {
                    matchedOutline = oRect;
                    outlineRects.splice(o, 1); // remove to avoid double-matching
                    break;
                }
            }

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
            if (matchedOutline) elements.push(matchedOutline);
            elements = elements.concat(textElements).concat(innerLines);

            boxes.push({
                rect: rect,
                outlineRect: matchedOutline,
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

            if (style.indexOf('stroke-dasharray:3.0,3.0') > -1 ||
                style.indexOf('stroke-dasharray:3,3') > -1) {
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

    // ================================================================
    // COLLAPSE / EXPAND FUNCTIONALITY
    // ================================================================

    /**
     * Check if a box is a leaf (no outgoing connections)
     * @param {Object} box
     * @param {Object} graph
     * @returns {boolean}
     */
    function isLeafBox(box, graph) {
        var out = graph.outgoing.get(box);
        return !out || out.length === 0;
    }

    /**
     * Compute which elements should be hidden based on the current set of collapsed boxes.
     * Uses DFS from each collapsed box's children to find all hidden descendants.
     * Handles nested collapse: if A is collapsed and contains B (also collapsed),
     * B's descendants are already hidden transitively.
     * @param {SVGElement} svg
     * @returns {{ hiddenBoxes: Set, hiddenConnections: Set }}
     */
    function computeHiddenElements(svg) {
        var data = svg._yamlData;
        var graph = data.graph;
        var collapsedBoxes = data.collapsedBoxes;
        var hiddenBoxes = new Set();
        var hiddenConnections = new Set();

        collapsedBoxes.forEach(function(collapsedBox) {
            // DFS from direct children of the collapsed box
            var outConns = graph.outgoing.get(collapsedBox) || [];
            var stack = [];

            for (var i = 0; i < outConns.length; i++) {
                hiddenConnections.add(outConns[i]);
                stack.push(outConns[i].targetBox);
            }

            while (stack.length > 0) {
                var current = stack.pop();
                if (hiddenBoxes.has(current)) continue;
                hiddenBoxes.add(current);

                var childConns = graph.outgoing.get(current) || [];
                for (var j = 0; j < childConns.length; j++) {
                    hiddenConnections.add(childConns[j]);
                    if (!hiddenBoxes.has(childConns[j].targetBox)) {
                        stack.push(childConns[j].targetBox);
                    }
                }
            }
        });

        return {
            hiddenBoxes: hiddenBoxes,
            hiddenConnections: hiddenConnections
        };
    }

    /**
     * Apply visibility to all elements based on the current collapsed state.
     * Hidden elements get the .yaml-collapsed-hidden class (display: none).
     * @param {SVGElement} svg
     */
    function hideElement(el) {
        el.style.display = 'none';
        el.setAttribute('display', 'none');
        el.style.visibility = 'hidden';
    }

    function showElement(el) {
        el.style.display = '';
        el.removeAttribute('display');
        el.style.visibility = '';
    }

    function isElementHidden(el) {
        return el.style.display === 'none' || el.getAttribute('display') === 'none';
    }

    function applyVisibility(svg) {
        var data = svg._yamlData;
        var hidden = computeHiddenElements(svg);

        // Apply to boxes - triple hiding: style.display + SVG display attr + visibility
        data.boxes.forEach(function(box) {
            var isHidden = hidden.hiddenBoxes.has(box);

            // Box elements (rect, text, lines)
            box.elements.forEach(function(el) {
                if (el) {
                    if (isHidden) { hideElement(el); } else { showElement(el); }
                }
            });

            // Toggle button for this box
            var toggleInfo = data.toggleElements.get(box);
            if (toggleInfo) {
                if (isHidden) { hideElement(toggleInfo.group); } else { showElement(toggleInfo.group); }
            }

            // Collapsed indicator class on box rect
            if (data.collapsedBoxes.has(box) && !isHidden) {
                box.elements.forEach(function(el) {
                    if (el) el.classList.add('yaml-box-collapsed');
                });
            } else {
                box.elements.forEach(function(el) {
                    if (el) el.classList.remove('yaml-box-collapsed');
                });
            }
        });

        // Apply to connections
        data.connections.forEach(function(conn) {
            var isHidden = hidden.hiddenConnections.has(conn);
            conn.elements.forEach(function(el) {
                if (el) {
                    if (isHidden) { hideElement(el); } else { showElement(el); }
                }
            });
        });

        // POST-APPLY diagnostic: verify hiding actually worked
        var allRects = svg.querySelectorAll('rect');
        var f1Rects = svg.querySelectorAll('rect[fill="#F1F1F1"][rx="5"]');
        var hiddenF1 = 0, visibleF1 = 0;
        f1Rects.forEach(function(r) {
            if (isElementHidden(r)) hiddenF1++;
            else visibleF1++;
        });
        var otherRectsCount = allRects.length - f1Rects.length;
        console.log('[Collapse POST-APPLY] F1F1F1 rects:', f1Rects.length,
            '| hidden:', hiddenF1, '| visible:', visibleF1,
            '| should-hide:', hidden.hiddenBoxes.size,
            '| other rects in SVG:', otherRectsCount);

        // Check for ghost rects (non-F1F1F1 rects that may look like borders)
        if (otherRectsCount > 0) {
            var ghostInfo = [];
            allRects.forEach(function(r) {
                if (r.getAttribute('fill') !== '#F1F1F1' || r.getAttribute('rx') !== '5') {
                    if (!r.classList.contains('yaml-toggle-bg') &&
                        !r.classList.contains('yaml-tooltip-bg') &&
                        !r.classList.contains('yaml-collapse-ctrl-bg')) {
                        ghostInfo.push('fill=' + r.getAttribute('fill') +
                            ' stroke=' + (r.getAttribute('style') || '').substring(0, 40) +
                            ' w=' + r.getAttribute('width') + ' h=' + r.getAttribute('height'));
                    }
                }
            });
            if (ghostInfo.length > 0) {
                console.log('[Collapse POST-APPLY] Ghost rects:', ghostInfo);
            }
        }

        // Verify no mismatches
        var mismatches = [];
        data.boxes.forEach(function(box) {
            var shouldHide = hidden.hiddenBoxes.has(box);
            var actuallyHidden = isElementHidden(box.rect);
            if (shouldHide !== actuallyHidden) {
                mismatches.push(box.label.substring(0, 30) + ' @(' + box.x + ',' + box.y + ')');
            }
        });
        if (mismatches.length > 0) {
            console.log('[Collapse POST-APPLY] MISMATCHES:', mismatches);
        }

        // Check all SVG lines/paths not tracked by any box or connection
        var trackedElements = new Set();
        data.boxes.forEach(function(b) { b.elements.forEach(function(e) { trackedElements.add(e); }); });
        data.connections.forEach(function(c) { c.elements.forEach(function(e) { trackedElements.add(e); }); });
        var untrackedLines = 0, untrackedPaths = 0;
        svg.querySelectorAll('line').forEach(function(l) { if (!trackedElements.has(l)) untrackedLines++; });
        svg.querySelectorAll('path').forEach(function(p) { if (!trackedElements.has(p)) untrackedPaths++; });
        if (untrackedLines > 0 || untrackedPaths > 0) {
            console.log('[Collapse POST-APPLY] Untracked elements: lines=' + untrackedLines + ' paths=' + untrackedPaths);
        }

        // If we have an active highlight selection and the selected box is now hidden, clear it
        if (svg.classList.contains('yaml-has-selection')) {
            var anySelectedHidden = false;
            svg.querySelectorAll('.yaml-box-source').forEach(function(el) {
                if (isElementHidden(el)) {
                    anySelectedHidden = true;
                }
            });
            if (anySelectedHidden) {
                clearSelection(svg);
            }
        }
    }

    /**
     * Save the original SVG dimensions (viewBox, width, height, style) on first call.
     * PlantUML SVGs have width/height attributes AND inline style with pixel dimensions,
     * all matching the viewBox 1:1. We must update all of them together to avoid zoom issues.
     * @param {SVGElement} svg
     */
    function saveOriginalDimensions(svg) {
        var data = svg._yamlData;
        if (data.originalViewBox) return; // already saved

        data.originalViewBox = svg.getAttribute('viewBox');
        data.originalWidth = svg.getAttribute('width');
        data.originalHeight = svg.getAttribute('height');
        data.originalStyle = svg.getAttribute('style');

        // Parse original viewBox to get the scale factor
        var parts = data.originalViewBox.split(/\s+/).map(Number);
        data.originalVbW = parts[2];
        data.originalVbH = parts[3];

        // Parse original pixel width/height (strip "px" suffix)
        var origW = parseFloat(data.originalWidth) || data.originalVbW;
        var origH = parseFloat(data.originalHeight) || data.originalVbH;
        data.scaleX = origW / data.originalVbW;
        data.scaleY = origH / data.originalVbH;
    }

    /**
     * Restore the original SVG dimensions (viewBox, width, height, style).
     * @param {SVGElement} svg
     */
    function restoreOriginalDimensions(svg) {
        var data = svg._yamlData;
        if (!data.originalViewBox) return;

        svg.setAttribute('viewBox', data.originalViewBox);
        if (data.originalWidth) svg.setAttribute('width', data.originalWidth);
        if (data.originalHeight) svg.setAttribute('height', data.originalHeight);
        if (data.originalStyle) svg.setAttribute('style', data.originalStyle);
    }

    /**
     * Apply zoomed dimensions to the SVG using data.zoomLevel, restoring the
     * original viewBox so all content is visible (used when nothing is collapsed).
     * @param {SVGElement} svg
     */
    function applyZoomedDimensions(svg) {
        var data = svg._yamlData;
        if (!data || !data.originalViewBox) return;
        var zoomLevel = data.zoomLevel || 1.0;

        svg.setAttribute('viewBox', data.originalViewBox);

        // Use the rendered (visual) base size so zoom steps are proportional
        // to what the user actually sees, not the large natural SVG attributes.
        var baseW = data.zoomBaseW || parseFloat(data.originalWidth) || data.originalVbW;
        var baseH = data.zoomBaseH || parseFloat(data.originalHeight) || data.originalVbH;
        var zoomedW = Math.round(baseW * zoomLevel);
        var zoomedH = Math.round(baseH * zoomLevel);

        svg.setAttribute('width', zoomedW + 'px');
        svg.setAttribute('height', zoomedH + 'px');

        // Override inline CSS via style property — this takes precedence over
        // setAttribute and overrides any autoFit rules (width:100%, max-width:100%)
        svg.style.width = zoomedW + 'px';
        svg.style.height = zoomedH + 'px';
        svg.style.maxWidth = 'none';
    }

    /**
     * Recalculate the SVG viewBox AND width/height to fit only visible elements.
     * Maintains the same zoom level by scaling width/height proportionally.
     * @param {SVGElement} svg
     */
    function recalcViewBox(svg) {
        var data = svg._yamlData;

        // Save original dimensions on first call
        saveOriginalDimensions(svg);

        // If nothing is collapsed, apply zoom (preserves user zoom level)
        if (data.collapsedBoxes.size === 0) {
            applyZoomedDimensions(svg);
            repositionControls(svg);
            return;
        }

        // Collect bounding boxes of all visible elements
        var minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
        var found = false;

        // Visible boxes
        data.boxes.forEach(function(box) {
            if (box.elements[0] && isElementHidden(box.elements[0])) return;
            minX = Math.min(minX, box.x);
            minY = Math.min(minY, box.y);
            maxX = Math.max(maxX, box.x + box.width);
            maxY = Math.max(maxY, box.y + box.height);
            found = true;
        });

        // Visible connections - use getBBox for accurate path bounds
        data.connections.forEach(function(conn) {
            if (conn.elements[0] && isElementHidden(conn.elements[0])) return;
            conn.elements.forEach(function(el) {
                if (el) {
                    try {
                        var bbox = el.getBBox();
                        if (bbox.width > 0 || bbox.height > 0) {
                            minX = Math.min(minX, bbox.x);
                            minY = Math.min(minY, bbox.y);
                            maxX = Math.max(maxX, bbox.x + bbox.width);
                            maxY = Math.max(maxY, bbox.y + bbox.height);
                            found = true;
                        }
                    } catch(e) { /* getBBox can throw on hidden elements */ }
                }
            });
        });

        // Include toggle buttons in bounds
        data.toggleElements.forEach(function(toggleInfo) {
            if (isElementHidden(toggleInfo.group)) return;
            try {
                var bbox = toggleInfo.group.getBBox();
                if (bbox.width > 0 || bbox.height > 0) {
                    minX = Math.min(minX, bbox.x);
                    minY = Math.min(minY, bbox.y);
                    maxX = Math.max(maxX, bbox.x + bbox.width);
                    maxY = Math.max(maxY, bbox.y + bbox.height);
                }
            } catch(e) {}
        });

        if (!found) {
            applyZoomedDimensions(svg);
            repositionControls(svg);
            return;
        }

        var padding = 20;
        var newVbX = minX - padding;
        var newVbY = minY - padding;
        var newVbW = maxX - minX + padding * 2;
        var newVbH = maxY - minY + padding * 2;

        var newViewBox = newVbX + ' ' + newVbY + ' ' + newVbW + ' ' + newVbH;
        svg.setAttribute('viewBox', newViewBox);

        // Use effective scale based on rendered size (zoomBase) if available,
        // so collapsed-mode zoom steps are proportional to what the user sees.
        var effectiveScaleX = data.zoomBaseW ? (data.zoomBaseW / data.originalVbW) : data.scaleX;
        var effectiveScaleY = data.zoomBaseH ? (data.zoomBaseH / data.originalVbH) : data.scaleY;
        var newPixelW = Math.round(newVbW * effectiveScaleX * (data.zoomLevel || 1.0));
        var newPixelH = Math.round(newVbH * effectiveScaleY * (data.zoomLevel || 1.0));

        svg.setAttribute('width', newPixelW + 'px');
        svg.setAttribute('height', newPixelH + 'px');

        // Override inline CSS via style property — overrides autoFit (width:100%, max-width:100%)
        svg.style.width = newPixelW + 'px';
        svg.style.height = newPixelH + 'px';
        svg.style.maxWidth = 'none';

        // Reposition controls if they exist
        repositionControls(svg);
    }

    /**
     * Toggle collapse/expand state for a box
     * @param {Object} box
     * @param {SVGElement} svg
     */
    function toggleCollapse(box, svg) {
        var data = svg._yamlData;
        var graph = data.graph;

        if (data.collapsedBoxes.has(box)) {
            // Expand: show only direct children (one level at a time)
            data.collapsedBoxes.delete(box);
            var toggleInfo = data.toggleElements.get(box);
            if (toggleInfo) {
                toggleInfo.symbol.textContent = '\u2212'; // minus sign
            }

            // Auto-collapse direct non-leaf children so only one level is revealed
            var outConns = graph.outgoing.get(box) || [];
            for (var i = 0; i < outConns.length; i++) {
                var child = outConns[i].targetBox;
                if (!isLeafBox(child, graph) && !data.collapsedBoxes.has(child)) {
                    data.collapsedBoxes.add(child);
                    var childToggle = data.toggleElements.get(child);
                    if (childToggle) {
                        childToggle.symbol.textContent = '+';
                    }
                }
            }
        } else {
            // Collapse
            data.collapsedBoxes.add(box);
            var toggleInfo = data.toggleElements.get(box);
            if (toggleInfo) {
                toggleInfo.symbol.textContent = '+';
            }
        }

        applyVisibility(svg);
        recalcViewBox(svg);
    }

    /**
     * Create toggle buttons for all non-leaf boxes
     * @param {SVGElement} svg
     * @param {Array} boxes
     * @param {Object} graph
     */
    function createToggleButtons(svg, boxes, graph) {
        var data = svg._yamlData;
        data.toggleElements = new Map();
        data.collapsedBoxes = new Set();

        boxes.forEach(function(box) {
            if (isLeafBox(box, graph)) return;

            var g = document.createElementNS(SVG_NS, 'g');
            g.classList.add('yaml-collapse-toggle');

            // Position on the right edge of the box
            var cx = box.x + box.width + 2;
            var cy = box.y + box.height / 2;
            var radius = 8;

            var circle = document.createElementNS(SVG_NS, 'circle');
            circle.setAttribute('cx', cx);
            circle.setAttribute('cy', cy);
            circle.setAttribute('r', radius);
            circle.classList.add('yaml-toggle-bg');

            var text = document.createElementNS(SVG_NS, 'text');
            text.setAttribute('x', cx);
            text.setAttribute('y', cy);
            text.setAttribute('text-anchor', 'middle');
            text.setAttribute('dominant-baseline', 'central');
            text.classList.add('yaml-toggle-symbol');
            text.textContent = '\u2212'; // minus sign (expanded state)

            g.appendChild(circle);
            g.appendChild(text);
            svg.appendChild(g);

            // Click handler - stopPropagation to avoid triggering box highlight
            g.addEventListener('click', function(e) {
                e.stopPropagation();
                e.preventDefault();
                toggleCollapse(box, svg);
            });

            data.toggleElements.set(box, { group: g, symbol: text });
        });
    }

    /**
     * Remove all toggle buttons from the SVG
     * @param {SVGElement} svg
     */
    function removeToggleButtons(svg) {
        var data = svg._yamlData;
        if (!data || !data.toggleElements) return;

        data.toggleElements.forEach(function(toggleInfo) {
            if (toggleInfo.group && toggleInfo.group.parentNode) {
                toggleInfo.group.parentNode.removeChild(toggleInfo.group);
            }
        });
        data.toggleElements.clear();
    }

    /**
     * Reposition the collapse/expand all controls relative to the current viewBox.
     * @param {SVGElement} svg
     */
    function repositionControls(svg) {
        var data = svg._yamlData;
        if (!data || !data.controlsGroup) return;

        var vb = svg.getAttribute('viewBox');
        if (!vb) return;
        var parts = vb.split(/\s+/).map(Number);
        var vbX = parts[0], vbY = parts[1], vbW = parts[2];

        // Position top-right of viewBox
        var ctrlX = vbX + vbW - 62;
        var ctrlY = vbY + 6;
        data.controlsGroup.setAttribute('transform', 'translate(' + ctrlX + ',' + ctrlY + ')');
    }

    /**
     * Create Collapse All / Expand All controls inside the SVG
     * @param {SVGElement} svg
     */
    function createCollapseControls(svg) {
        var data = svg._yamlData;

        // Only create if there are non-leaf boxes
        var hasNonLeaf = false;
        data.boxes.forEach(function(box) {
            if (!isLeafBox(box, data.graph)) hasNonLeaf = true;
        });
        if (!hasNonLeaf) return;

        var g = document.createElementNS(SVG_NS, 'g');
        g.classList.add('yaml-collapse-controls');

        // Collapse All button (left)
        var collapseBtn = document.createElementNS(SVG_NS, 'g');
        collapseBtn.classList.add('yaml-collapse-ctrl-btn');

        var collapseBg = document.createElementNS(SVG_NS, 'rect');
        collapseBg.setAttribute('x', '0');
        collapseBg.setAttribute('y', '0');
        collapseBg.setAttribute('width', '26');
        collapseBg.setAttribute('height', '22');
        collapseBg.setAttribute('rx', '4');
        collapseBg.setAttribute('ry', '4');
        collapseBg.classList.add('yaml-collapse-ctrl-bg');

        var collapseTxt = document.createElementNS(SVG_NS, 'text');
        collapseTxt.setAttribute('x', '13');
        collapseTxt.setAttribute('y', '11');
        collapseTxt.setAttribute('text-anchor', 'middle');
        collapseTxt.setAttribute('dominant-baseline', 'central');
        collapseTxt.classList.add('yaml-collapse-ctrl-text');
        collapseTxt.textContent = '\u229F'; // ⊟

        collapseBtn.appendChild(collapseBg);
        collapseBtn.appendChild(collapseTxt);

        collapseBtn.addEventListener('click', function(e) {
            e.stopPropagation();
            collapseAll(svg);
        });

        // Expand All button (right)
        var expandBtn = document.createElementNS(SVG_NS, 'g');
        expandBtn.classList.add('yaml-collapse-ctrl-btn');
        expandBtn.setAttribute('transform', 'translate(30,0)');

        var expandBg = document.createElementNS(SVG_NS, 'rect');
        expandBg.setAttribute('x', '0');
        expandBg.setAttribute('y', '0');
        expandBg.setAttribute('width', '26');
        expandBg.setAttribute('height', '22');
        expandBg.setAttribute('rx', '4');
        expandBg.setAttribute('ry', '4');
        expandBg.classList.add('yaml-collapse-ctrl-bg');

        var expandTxt = document.createElementNS(SVG_NS, 'text');
        expandTxt.setAttribute('x', '13');
        expandTxt.setAttribute('y', '11');
        expandTxt.setAttribute('text-anchor', 'middle');
        expandTxt.setAttribute('dominant-baseline', 'central');
        expandTxt.classList.add('yaml-collapse-ctrl-text');
        expandTxt.textContent = '\u229E'; // ⊞

        expandBtn.appendChild(expandBg);
        expandBtn.appendChild(expandTxt);

        expandBtn.addEventListener('click', function(e) {
            e.stopPropagation();
            expandAll(svg);
        });

        g.appendChild(collapseBtn);
        g.appendChild(expandBtn);
        svg.appendChild(g);

        data.controlsGroup = g;
        repositionControls(svg);
    }

    /**
     * Remove the collapse/expand all controls from the SVG
     * @param {SVGElement} svg
     */
    function removeCollapseControls(svg) {
        var data = svg._yamlData;
        if (!data || !data.controlsGroup) return;

        if (data.controlsGroup.parentNode) {
            data.controlsGroup.parentNode.removeChild(data.controlsGroup);
        }
        data.controlsGroup = null;
    }

    /**
     * Collapse all non-leaf boxes
     * @param {SVGElement} svg
     */
    function collapseAll(svg) {
        var data = svg._yamlData;

        data.boxes.forEach(function(box) {
            if (!isLeafBox(box, data.graph)) {
                data.collapsedBoxes.add(box);
                var toggleInfo = data.toggleElements.get(box);
                if (toggleInfo) {
                    toggleInfo.symbol.textContent = '+';
                }
            }
        });

        clearSelection(svg);
        applyVisibility(svg);
        recalcViewBox(svg);
    }

    /**
     * Expand all collapsed boxes
     * @param {SVGElement} svg
     */
    function expandAll(svg) {
        var data = svg._yamlData;

        data.collapsedBoxes.forEach(function(box) {
            var toggleInfo = data.toggleElements.get(box);
            if (toggleInfo) {
                toggleInfo.symbol.textContent = '\u2212'; // minus sign
            }
        });
        data.collapsedBoxes.clear();

        applyVisibility(svg);

        // Restore dimensions preserving user zoom level
        applyZoomedDimensions(svg);
        repositionControls(svg);
    }

    /**
     * Check if there are any collapsed boxes in the SVG
     * @param {SVGElement} svg
     * @returns {boolean}
     */
    function hasCollapsedBoxes(svg) {
        return svg._yamlData && svg._yamlData.collapsedBoxes && svg._yamlData.collapsedBoxes.size > 0;
    }

    // ================================================================
    // END COLLAPSE / EXPAND
    // ================================================================

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

        // Remove all highlight background rects
        svg.querySelectorAll('.yaml-row-highlight-bg').forEach(function(r) { r.remove(); });

        var classes = ['yaml-box-source', 'yaml-box-downstream', 'yaml-box-upstream',
                       'yaml-connection-highlighted', 'yaml-row-selected', 'yaml-row-leaf',
                       'yaml-row-source', 'yaml-row-upstream', 'yaml-row-downstream'];

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
     * Add a colored background rect behind a text element for row highlighting.
     * @param {SVGTextElement} textEl - The text element to highlight
     * @param {string} colorClass - CSS class for the fill color (e.g. 'yaml-row-bg-source')
     */
    function addRowHighlightRect(textEl, colorClass) {
        var bbox = textEl.getBBox();
        var rect = document.createElementNS(SVG_NS, 'rect');
        rect.setAttribute('x', bbox.x - 2);
        rect.setAttribute('y', bbox.y - 1);
        rect.setAttribute('width', bbox.width + 4);
        rect.setAttribute('height', bbox.height + 2);
        rect.setAttribute('rx', '2');
        rect.classList.add('yaml-row-highlight-bg', colorClass);
        // Insert BEFORE the text so the rect is behind it
        textEl.parentNode.insertBefore(rect, textEl);
    }

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
            // No outgoing connection for this row - leaf highlight + ancestors only
            clearSelection(svg);
            svg.classList.add('yaml-has-selection');
            applyBoxClass(box, 'yaml-box-source');
            var textY = parseFloat(textEl.getAttribute('y'));
            box.textElements.forEach(function(t) {
                var ty = parseFloat(t.getAttribute('y'));
                if (Math.abs(ty - textY) < 2) {
                    t.classList.add('yaml-row-leaf');
                    addRowHighlightRect(t, 'yaml-row-bg-leaf');
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
                t.classList.add('yaml-row-upstream');
                addRowHighlightRect(t, 'yaml-row-bg-upstream');
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
                t.classList.add('yaml-row-source');
                addRowHighlightRect(t, 'yaml-row-bg-source');
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
            t.classList.add('yaml-row-downstream');
            addRowHighlightRect(t, 'yaml-row-bg-downstream');
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
            t.classList.add('yaml-row-upstream');
            addRowHighlightRect(t, 'yaml-row-bg-upstream');
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
     * Setup Ctrl+wheel zoom on the SVG element.
     * The listener is on the SVG itself so it only fires when the mouse is over it.
     * @param {SVGElement} svg
     */
    function setupWheelZoom(svg) {
        var ZOOM_STEP = 0.2;
        var MIN_ZOOM = 0.2;
        var MAX_ZOOM = 5.0;

        var wheelHandler = function(e) {
            if (!e.ctrlKey) return;
            e.preventDefault(); // blocca zoom browser nativo nell'iframe

            var data = svg._yamlData;
            if (!data) return;

            // Capture current RENDERED size as zoom base on first wheel event.
            // Must happen before saveOriginalDimensions, which reads SVG attributes
            // (those may be the large natural size, while autoFit shows it smaller via CSS).
            if (!data.zoomBaseW) {
                var renderRect = svg.getBoundingClientRect();
                data.zoomBaseW = renderRect.width;
                data.zoomBaseH = renderRect.height;
            }

            // Ensure originalViewBox is captured (may not have been set if never collapsed)
            saveOriginalDimensions(svg);

            // Capture cursor position as a FRACTION of the SVG before resizing.
            // Using fractions works even if the SVG is centered (grows from center,
            // not from left edge), unlike the naive mouseRel * (zoomFactor-1) formula.
            var rect = svg.getBoundingClientRect();
            var fractionX = rect.width  > 0 ? (e.clientX - rect.left)  / rect.width  : 0.5;
            var fractionY = rect.height > 0 ? (e.clientY - rect.top)   / rect.height : 0.5;

            var prevZoom = data.zoomLevel;
            var direction = e.deltaY < 0 ? 1 : -1; // scroll up = zoom in
            data.zoomLevel = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM,
                data.zoomLevel + direction * ZOOM_STEP));

            if (data.collapsedBoxes.size === 0) {
                applyZoomedDimensions(svg);
                repositionControls(svg);
            } else {
                recalcViewBox(svg); // applica già zoomLevel
            }

            // After resize, getBoundingClientRect() forces a layout recalculation
            // and returns the SVG's actual new position (handles centered SVGs correctly).
            // Scroll so the content point at (fractionX, fractionY) stays under the cursor.
            var newRect = svg.getBoundingClientRect();
            window.scrollBy({
                left: (newRect.left + fractionX * newRect.width)  - e.clientX,
                top:  (newRect.top  + fractionY * newRect.height) - e.clientY,
                behavior: 'instant'
            });
        };

        svg.addEventListener('wheel', wheelHandler, { passive: false });
        svg._yamlData.wheelHandler = wheelHandler;
    }

    /**
     * Setup grab-to-pan on the SVG: mousedown + drag scrolls the iframe viewport.
     * A plain click (no drag) is not suppressed, so click-to-select still works.
     * @param {SVGElement} svg
     */
    function setupPanDrag(svg) {
        var DRAG_THRESHOLD = 4; // px before a mousedown is considered a drag

        var isPanning   = false;
        var hasDragged  = false;
        var lastX, lastY;

        svg.style.cursor = 'grab';

        var mousedownHandler = function(e) {
            if (e.button !== 0) return; // left button only
            // Don't hijack clicks on collapse toggle buttons / controls
            if (e.target.closest &&
                (e.target.closest('.yaml-collapse-toggle') ||
                 e.target.closest('.yaml-collapse-controls'))) return;

            isPanning  = true;
            hasDragged = false;
            lastX = e.clientX;
            lastY = e.clientY;
            e.preventDefault(); // prevent text-selection during drag
        };

        var mousemoveHandler = function(e) {
            if (!isPanning) return;
            var dx = e.clientX - lastX;
            var dy = e.clientY - lastY;

            if (!hasDragged &&
                (Math.abs(dx) > DRAG_THRESHOLD || Math.abs(dy) > DRAG_THRESHOLD)) {
                hasDragged = true;
                // Show grabbing cursor over entire document while dragging
                document.documentElement.style.setProperty('cursor', 'grabbing', 'important');
            }

            if (hasDragged) {
                window.scrollBy({ left: -dx, top: -dy, behavior: 'instant' });
                lastX = e.clientX;
                lastY = e.clientY;
            }
        };

        // One-shot click suppressor used after a drag ends
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
                // Prevent the click that fires after mouseup from selecting a box
                document.addEventListener('click', cancelNextClick, true);
            }
        };

        svg.addEventListener('mousedown', mousedownHandler);
        document.addEventListener('mousemove', mousemoveHandler);
        document.addEventListener('mouseup',   mouseupHandler);

        svg._yamlData.panHandlers = {
            mousedown: mousedownHandler,
            mousemove: mousemoveHandler,
            mouseup:   mouseupHandler
        };
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

        // Block browser/Electron Ctrl+wheel zoom for the whole iframe
        installGlobalCtrlWheelPrevention();

        // Store data for destroy
        svg._yamlData = {
            boxes: boxes,
            connections: connections,
            graph: graph,
            options: options,
            tippyInstances: [],
            zoomLevel: 1.0,
            wheelHandler: null,
            panHandlers: null,
            zoomBaseW: null,
            zoomBaseH: null
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

        // Create collapse/expand toggle buttons and controls
        createToggleButtons(svg, boxes, graph);
        createCollapseControls(svg);

        // ESC key to clear (priority: highlight first, then collapse)
        var escHandler = function(e) {
            if (e.key === 'Escape') {
                if (svg.classList.contains('yaml-has-selection')) {
                    clearSelection(svg);
                    if (options.onClear) options.onClear();
                } else if (hasCollapsedBoxes(svg)) {
                    expandAll(svg);
                }
            }
        };
        document.addEventListener('keydown', escHandler);
        svg._yamlData.escHandler = escHandler;

        setupWheelZoom(svg);
        setupPanDrag(svg);

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

        if (svg._yamlData && svg._yamlData.wheelHandler) {
            svg.removeEventListener('wheel', svg._yamlData.wheelHandler);
            svg._yamlData.wheelHandler = null;
        }

        if (svg._yamlData && svg._yamlData.panHandlers) {
            var ph = svg._yamlData.panHandlers;
            svg.removeEventListener('mousedown', ph.mousedown);
            document.removeEventListener('mousemove', ph.mousemove);
            document.removeEventListener('mouseup',   ph.mouseup);
            svg._yamlData.panHandlers = null;
        }

        if (svg._yamlData && svg._yamlData.escHandler) {
            document.removeEventListener('keydown', svg._yamlData.escHandler);
        }

        // Remove collapse/expand elements
        removeToggleButtons(svg);
        removeCollapseControls(svg);

        // Restore original dimensions (viewBox + width/height + style)
        restoreOriginalDimensions(svg);

        // Restore visibility: remove inline display/visibility and CSS classes
        if (svg._yamlData) {
            svg._yamlData.boxes.forEach(function(box) {
                box.elements.forEach(function(el) {
                    if (el) {
                        showElement(el);
                        el.classList.remove('yaml-box-collapsed');
                    }
                });
            });
            svg._yamlData.connections.forEach(function(conn) {
                conn.elements.forEach(function(el) {
                    if (el) showElement(el);
                });
            });
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
