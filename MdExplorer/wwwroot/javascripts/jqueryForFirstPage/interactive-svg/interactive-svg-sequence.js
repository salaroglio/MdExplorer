/**
 * MdExplorer - Interactive SVG for PlantUML Sequence Diagrams
 * ============================================================
 * Makes PlantUML-generated sequence diagram SVGs interactive with click-to-highlight.
 *
 * Features:
 * - Click on any participant to highlight all its messages
 * - Click on any message to highlight source and target participants
 * - Outgoing messages = GREEN, Incoming messages = RED
 * - Note boxes = YELLOW
 * - Non-related elements are dimmed
 * - ESC key or click outside to clear selection
 *
 * PlantUML Sequence Diagram Structure:
 * - Participants: rect with fill="#E2E2F0" at top
 * - Lifelines: dashed vertical lines
 * - Activation boxes: narrow white rectangles (width=10)
 * - Messages: horizontal lines with polygon arrowheads
 * - Self-calls: lines that loop back
 * - Notes: rect with specific fills
 *
 * Usage:
 *   InteractiveSvgSequence.init(svgElement);
 *   InteractiveSvgSequence.initAll();
 *   InteractiveSvgSequence.destroy(svgElement);
 */

var InteractiveSvgSequence = (function() {
    'use strict';

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
     * Setup Ctrl+wheel zoom on the SVG element.
     * @param {SVGElement} svg
     */
    function setupWheelZoom(svg) {
        var ZOOM_STEP = 0.2;
        var MIN_ZOOM  = 0.2;
        var MAX_ZOOM  = 5.0;
        var data = svg._sequenceData;

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

            // Scroll to keep the content point under the cursor stable
            var newRect = svg.getBoundingClientRect();
            window.scrollBy({
                left: (newRect.left + fractionX * newRect.width)  - e.clientX,
                top:  (newRect.top  + fractionY * newRect.height) - e.clientY,
                behavior: 'instant'
            });
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
        var data = svg._sequenceData;
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
                window.scrollBy({ left: -dx, top: -dy, behavior: 'instant' });
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
     * Check if SVG is a sequence diagram (no elem_, cluster_, link_ elements)
     */
    function isSequenceDiagram(svg) {
        // If it has PlantUML component diagram elements, it's not a sequence diagram
        var hasComponentElements = svg.querySelector('g[id^="elem_"], g[id^="cluster_"], g[id^="link_"]');
        if (hasComponentElements) return false;

        // Check for sequence diagram characteristics:
        // - Participant boxes (fill #E2E2F0)
        // - Dashed vertical lines (lifelines)
        var participantBoxes = svg.querySelectorAll('rect[fill="#E2E2F0"]');
        var dashedLines = svg.querySelectorAll('line[style*="stroke-dasharray"]');

        return participantBoxes.length > 0 && dashedLines.length > 0;
    }

    /**
     * Parse participants from SVG
     * Participants are rect elements with fill="#E2E2F0" positioned at the top
     */
    function parseParticipants(svg) {
        var participants = [];
        var rects = svg.querySelectorAll('rect[fill="#E2E2F0"]');
        var texts = svg.querySelectorAll('text');

        // Group rects by their approximate x position (within 5px)
        var rectGroups = {};
        rects.forEach(function(rect) {
            var x = parseFloat(rect.getAttribute('x'));
            var y = parseFloat(rect.getAttribute('y'));
            var width = parseFloat(rect.getAttribute('width'));
            var centerX = x + width / 2;

            // Only consider top rects (y < 100 typically)
            if (y < 100) {
                var key = Math.round(centerX);
                if (!rectGroups[key]) {
                    rectGroups[key] = { rect: rect, centerX: centerX, x: x, width: width, y: y };
                }
            }
        });

        // Find text labels for each participant
        Object.keys(rectGroups).forEach(function(key) {
            var group = rectGroups[key];
            var rect = group.rect;
            var rectX = group.x;
            var rectWidth = group.width;
            var rectY = group.y;

            // Find text elements inside or near this rect
            var labels = [];
            var textElements = [];
            texts.forEach(function(text) {
                var textX = parseFloat(text.getAttribute('x'));
                var textY = parseFloat(text.getAttribute('y'));

                // Check if text is within the rect bounds (with some tolerance)
                if (textX >= rectX - 5 && textX <= rectX + rectWidth + 5 &&
                    textY >= rectY && textY <= rectY + 80) {
                    labels.push(text.textContent);
                    textElements.push(text);
                }
            });

            participants.push({
                rect: rect,
                centerX: group.centerX,
                x: rectX,
                width: rectWidth,
                y: rectY,
                name: labels.join(' ') || 'Participant',
                elements: [rect].concat(textElements),
                textElements: textElements
            });
        });

        // Sort by x position
        participants.sort(function(a, b) { return a.centerX - b.centerX; });

        return participants;
    }

    /**
     * Find the bottom participant boxes (duplicates at bottom of diagram)
     */
    function findBottomParticipantBoxes(svg, participants) {
        var rects = svg.querySelectorAll('rect[fill="#E2E2F0"]');
        var texts = svg.querySelectorAll('text');
        var maxY = 0;

        // Find the maximum y position
        rects.forEach(function(rect) {
            var y = parseFloat(rect.getAttribute('y'));
            if (y > maxY) maxY = y;
        });

        // Associate bottom rects and their text labels with participants by x position
        rects.forEach(function(rect) {
            var y = parseFloat(rect.getAttribute('y'));
            if (y > maxY - 100) { // Bottom rects
                var x = parseFloat(rect.getAttribute('x'));
                var width = parseFloat(rect.getAttribute('width'));
                var height = parseFloat(rect.getAttribute('height')) || 50;
                var centerX = x + width / 2;

                // Find matching participant
                participants.forEach(function(p) {
                    if (Math.abs(p.centerX - centerX) < 10) {
                        p.elements.push(rect);
                        p.bottomRect = rect;

                        // Also find text labels inside this bottom rect
                        texts.forEach(function(text) {
                            var textX = parseFloat(text.getAttribute('x'));
                            var textY = parseFloat(text.getAttribute('y'));

                            if (textX >= x - 5 && textX <= x + width + 5 &&
                                textY >= y && textY <= y + height + 10) {
                                p.elements.push(text);
                            }
                        });
                    }
                });
            }
        });
    }

    /**
     * Find lifelines for each participant
     */
    function findLifelines(svg, participants) {
        var lifelines = svg.querySelectorAll('line[style*="stroke-dasharray"]');

        lifelines.forEach(function(line) {
            var x1 = parseFloat(line.getAttribute('x1'));

            // Find participant with closest centerX
            var closest = null;
            var minDist = Infinity;
            participants.forEach(function(p) {
                var dist = Math.abs(p.centerX - x1);
                if (dist < minDist) {
                    minDist = dist;
                    closest = p;
                }
            });

            if (closest && minDist < 20) {
                closest.lifeline = line;
                closest.elements.push(line);
            }
        });
    }

    /**
     * Find activation boxes for each participant
     */
    function findActivationBoxes(svg, participants) {
        var activations = svg.querySelectorAll('rect[fill="#FFFFFF"]');

        activations.forEach(function(rect) {
            var width = parseFloat(rect.getAttribute('width'));
            // Activation boxes are narrow (width = 10)
            if (width <= 15) {
                var x = parseFloat(rect.getAttribute('x'));
                var centerX = x + width / 2;

                // Find participant with closest centerX
                var closest = null;
                var minDist = Infinity;
                participants.forEach(function(p) {
                    var dist = Math.abs(p.centerX - centerX);
                    if (dist < minDist) {
                        minDist = dist;
                        closest = p;
                    }
                });

                if (closest && minDist < 20) {
                    if (!closest.activations) closest.activations = [];
                    closest.activations.push(rect);
                    closest.elements.push(rect);
                }
            }
        });
    }

    /**
     * Parse messages from SVG
     * Messages are horizontal lines with polygon arrowheads
     */
    function parseMessages(svg, participants) {
        var messages = [];
        var lines = svg.querySelectorAll('line');
        var polygons = svg.querySelectorAll('polygon');
        var texts = svg.querySelectorAll('text');

        // Find horizontal message lines (not lifelines)
        lines.forEach(function(line) {
            var style = line.getAttribute('style') || '';
            // Skip dashed lines (lifelines) and very short lines
            if (style.indexOf('stroke-dasharray:5') > -1) return;

            var x1 = parseFloat(line.getAttribute('x1'));
            var y1 = parseFloat(line.getAttribute('y1'));
            var x2 = parseFloat(line.getAttribute('x2'));
            var y2 = parseFloat(line.getAttribute('y2'));

            // Horizontal lines (y1 ≈ y2) that span between participants
            if (Math.abs(y1 - y2) < 5 && Math.abs(x2 - x1) > 30) {
                var fromX = Math.min(x1, x2);
                var toX = Math.max(x1, x2);

                // Find source and target participants
                var fromParticipant = findParticipantByX(participants, fromX);
                var toParticipant = findParticipantByX(participants, toX);

                if (fromParticipant && toParticipant) {
                    // Determine direction based on arrow position
                    var isReturn = style.indexOf('stroke-dasharray:2') > -1;

                    // Find associated polygon (arrowhead)
                    var arrowhead = findArrowhead(polygons, x1, x2, y1);

                    // Find associated text label
                    var label = findMessageLabel(texts, fromX, toX, y1);

                    messages.push({
                        line: line,
                        arrowhead: arrowhead,
                        labelElement: label ? label.element : null,
                        label: label ? label.text : '',
                        y: y1,
                        from: fromParticipant,
                        to: toParticipant,
                        isReturn: isReturn,
                        elements: [line, arrowhead, label ? label.element : null].filter(Boolean)
                    });
                }
            }
        });

        // Sort messages by y position (top to bottom)
        messages.sort(function(a, b) { return a.y - b.y; });

        return messages;
    }

    /**
     * Find participant by x coordinate
     */
    function findParticipantByX(participants, x) {
        var closest = null;
        var minDist = Infinity;

        participants.forEach(function(p) {
            var dist = Math.abs(p.centerX - x);
            if (dist < minDist) {
                minDist = dist;
                closest = p;
            }
        });

        return minDist < 50 ? closest : null;
    }

    /**
     * Find arrowhead polygon for a message
     */
    function findArrowhead(polygons, x1, x2, y) {
        var targetX = x2; // Arrow points to x2
        var found = null;

        polygons.forEach(function(polygon) {
            var points = polygon.getAttribute('points');
            if (points) {
                // Parse first point to get approximate position
                var firstPoint = points.split(' ')[0].split(',');
                var px = parseFloat(firstPoint[0]);
                var py = parseFloat(firstPoint[1]);

                if (Math.abs(py - y) < 10 && Math.abs(px - x1) < 20 || Math.abs(px - x2) < 20) {
                    found = polygon;
                }
            }
        });

        return found;
    }

    /**
     * Find message label text
     */
    function findMessageLabel(texts, fromX, toX, y) {
        var found = null;
        var midX = (fromX + toX) / 2;

        texts.forEach(function(text) {
            var textX = parseFloat(text.getAttribute('x'));
            var textY = parseFloat(text.getAttribute('y'));

            // Label should be above the message line and between from/to
            if (textY < y && textY > y - 50 &&
                textX >= fromX - 10 && textX <= toX + 10) {
                // Prefer labels closest to the line
                if (!found || Math.abs(textY - y) < Math.abs(parseFloat(found.element.getAttribute('y')) - y)) {
                    found = { element: text, text: text.textContent };
                }
            }
        });

        return found;
    }

    /**
     * Find self-call messages (loops back to same participant)
     */
    function findSelfCalls(svg, participants, messages) {
        var lines = svg.querySelectorAll('line');
        var processedLines = new Set(messages.map(function(m) { return m.line; }));

        // Self-calls are made of multiple line segments forming a loop
        // They have the same x1 and x2 (approximately) and different y values
        var verticalLines = [];
        lines.forEach(function(line) {
            if (processedLines.has(line)) return;

            var style = line.getAttribute('style') || '';
            if (style.indexOf('stroke-dasharray:5') > -1) return; // Skip lifelines

            var x1 = parseFloat(line.getAttribute('x1'));
            var y1 = parseFloat(line.getAttribute('y1'));
            var x2 = parseFloat(line.getAttribute('x2'));
            var y2 = parseFloat(line.getAttribute('y2'));

            // Vertical or near-vertical lines that could be part of self-calls
            if (Math.abs(x1 - x2) < 5 && Math.abs(y2 - y1) > 10) {
                verticalLines.push({ line: line, x: x1, y1: Math.min(y1, y2), y2: Math.max(y1, y2) });
            }
        });

        // Group self-call components (not implemented fully for now)
        // This is complex and would require more analysis
    }

    /**
     * Find note boxes
     */
    function findNotes(svg) {
        var notes = [];
        // Notes often have a different fill color or specific shape
        // PlantUML notes typically have a folded corner effect
        // For now, we'll identify them by elimination or specific patterns

        return notes;
    }
    /**
     * Clear all selections
     */
    function clearSelection(svg) {
        var classes = ['seq-selected', 'seq-participant-source', 'seq-participant-incoming',
                       'seq-participant-outgoing', 'seq-message-highlighted', 'seq-element-highlighted'];
        svg.querySelectorAll('.' + classes.join(', .'))
            .forEach(function(el) {
                classes.forEach(function(cls) {
                    el.classList.remove(cls);
                });
            });
        svg.classList.remove('seq-has-selection');
    }

    /**
     * Apply class to all elements of a participant
     */
    function applyParticipantClass(participant, className) {
        participant.elements.forEach(function(el) {
            if (el) el.classList.add(className);
        });
    }

    /**
     * Apply class to all elements of a message
     */
    function applyMessageClass(message, className) {
        message.elements.forEach(function(el) {
            if (el) el.classList.add(className);
        });
    }

    /**
     * Handle participant click
     */
    function handleParticipantClick(participant, svg, participants, messages, options) {
        clearSelection(svg);

        applyParticipantClass(participant, 'seq-participant-source');
        svg.classList.add('seq-has-selection');

        var outgoingMessages = [];
        var incomingMessages = [];

        messages.forEach(function(m) {
            if (m.from === participant) {
                applyMessageClass(m, 'seq-message-highlighted');
                outgoingMessages.push(m);
                if (m.to !== participant) {
                    applyParticipantClass(m.to, 'seq-participant-outgoing');
                }
            }
            if (m.to === participant) {
                applyMessageClass(m, 'seq-message-highlighted');
                incomingMessages.push(m);
                if (m.from !== participant) {
                    applyParticipantClass(m.from, 'seq-participant-incoming');
                }
            }
        });

        if (options && options.onSelect) {
            options.onSelect({
                type: 'participant',
                name: participant.name,
                outgoingCount: outgoingMessages.length,
                incomingCount: incomingMessages.length
            });
        }
    }

    /**
     * Handle message click
     */
    function handleMessageClick(message, svg, participants, messages, options) {
        clearSelection(svg);

        applyMessageClass(message, 'seq-message-highlighted');
        svg.classList.add('seq-has-selection');

        applyParticipantClass(message.from, 'seq-participant-incoming');
        applyParticipantClass(message.to, 'seq-participant-outgoing');

        if (options && options.onSelect) {
            options.onSelect({
                type: 'message',
                from: message.from.name,
                to: message.to.name,
                label: message.label
            });
        }
    }

    /**
     * Initialize interactivity on a sequence diagram SVG
     */
    function init(svg, options) {
        if (!svg || svg.tagName !== 'svg') {
            console.warn('[InteractiveSvgSequence] Invalid SVG element');
            return false;
        }

        if (!isSequenceDiagram(svg)) {
            console.log('[InteractiveSvgSequence] Not a sequence diagram, skipping');
            return false;
        }

        if (initializedSvgs.has(svg)) {
            console.log('[InteractiveSvgSequence] Already initialized');
            return true;
        }

        options = options || {};

        console.log('[InteractiveSvgSequence] Parsing sequence diagram...');

        // Parse diagram structure
        var participants = parseParticipants(svg);
        findBottomParticipantBoxes(svg, participants);
        findLifelines(svg, participants);
        findActivationBoxes(svg, participants);
        var messages = parseMessages(svg, participants);

        console.log('[InteractiveSvgSequence] Found', participants.length, 'participants and', messages.length, 'messages');

        if (participants.length === 0) {
            console.warn('[InteractiveSvgSequence] No participants found');
            return false;
        }

        // Mark as initialized
        initializedSvgs.add(svg);
        svg.classList.add('interactive-svg-sequence');

        // Store data
        svg._sequenceData = {
            participants: participants,
            messages: messages,
            options: options
        };

        // Block browser/Electron Ctrl+wheel zoom for the whole iframe
        installGlobalCtrlWheelPrevention();

        // Setup zoom and pan
        setupWheelZoom(svg);
        setupPanDrag(svg);

        // Add click handlers to participant boxes
        participants.forEach(function(p) {
            p.elements.forEach(function(el) {
                if (el) {
                    el.style.cursor = 'pointer';
                    el.addEventListener('click', function(e) {
                        e.stopPropagation();
                        handleParticipantClick(p, svg, participants, messages, options);
                    });
                }
            });
        });

        // Add click handlers to messages
        messages.forEach(function(m) {
            m.elements.forEach(function(el) {
                if (el) {
                    el.style.cursor = 'pointer';
                    // Add wider hit area for lines
                    if (el.tagName === 'line') {
                        el.setAttribute('stroke-width', '10');
                        el.setAttribute('stroke', 'transparent');
                        el.style.pointerEvents = 'stroke';
                    }
                    el.addEventListener('click', function(e) {
                        e.stopPropagation();
                        handleMessageClick(m, svg, participants, messages, options);
                    });
                }
            });
        });

        // Click outside to clear
        svg.addEventListener('click', function(e) {
            if (!e.target.closest('.seq-participant, .seq-message, rect, line, polygon')) {
                clearSelection(svg);
                if (options.onClear) options.onClear();
            }
        });

        // ESC to clear
        var escHandler = function(e) {
            if (e.key === 'Escape' && svg.classList.contains('seq-has-selection')) {
                clearSelection(svg);
                if (options.onClear) options.onClear();
            }
        };
        document.addEventListener('keydown', escHandler);
        svg._sequenceData.escHandler = escHandler;

        console.log('[InteractiveSvgSequence] Initialized successfully');
        return true;
    }

    /**
     * Initialize all sequence diagram SVGs on page
     */
    function initAll(options) {
        document.querySelectorAll('svg').forEach(function(svg) {
            init(svg, options);
        });
    }

    /**
     * Destroy interactivity
     */
    function destroy(svg) {
        if (!svg || !initializedSvgs.has(svg)) return;

        if (svg._sequenceData && svg._sequenceData.escHandler) {
            document.removeEventListener('keydown', svg._sequenceData.escHandler);
        }

        // Remove zoom handler
        if (svg._sequenceData && svg._sequenceData.wheelHandler) {
            svg.removeEventListener('wheel', svg._sequenceData.wheelHandler);
        }

        // Remove pan handlers
        if (svg._sequenceData && svg._sequenceData.panHandlers) {
            svg.removeEventListener('mousedown', svg._sequenceData.panHandlers.mousedown);
            document.removeEventListener('mousemove', svg._sequenceData.panHandlers.mousemove);
            document.removeEventListener('mouseup',   svg._sequenceData.panHandlers.mouseup);
        }

        clearSelection(svg);
        svg.classList.remove('interactive-svg-sequence');
        delete svg._sequenceData;
        initializedSvgs.delete(svg);
    }

    return {
        init: init,
        initAll: initAll,
        destroy: destroy,
        isSequenceDiagram: isSequenceDiagram
    };

})();
