/**
 * MdExplorer - Initialization Coordinator
 * ========================================
 * Final initialization after all modules are loaded
 *
 * This file should be loaded LAST to ensure all dependencies are available
 */

$(function() {
    console.log('[MdExplorer] All modules loaded and initialized');

    // Log loaded modules for debugging
    console.log('[MdExplorer] Module status:', {
        globals: typeof window.currentDocumentSetting !== 'undefined',
        navigation: typeof initializeInternalNavigation !== 'undefined',
        search: typeof toggleSearch !== 'undefined',
        drawing: typeof toggleMdCanvas !== 'undefined',
        toc: typeof toggleTOC !== 'undefined',
        interactiveSvg: typeof InteractiveSvg !== 'undefined',
        interactiveSvgSequence: typeof InteractiveSvgSequence !== 'undefined'
    });

    // Auto-initialize interactive SVG for all PlantUML diagrams on page
    // Component/class diagrams (legacy: elem_, cluster_, link_  |  new v1.2026.1+: g.entity, g.cluster, g.link)
    if (typeof InteractiveSvg !== 'undefined') {
        InteractiveSvg.initAll();
    }
    // Sequence diagrams (participant boxes and message arrows)
    if (typeof InteractiveSvgSequence !== 'undefined') {
        InteractiveSvgSequence.initAll();
    }
    // YAML diagram links (url, link, href keys become clickable)
    if (typeof InteractiveSvgYamlLinks !== 'undefined') {
        InteractiveSvgYamlLinks.initAll();
    }
    // YAML tree diagrams (click-to-highlight ancestors/descendants)
    if (typeof InteractiveSvgYaml !== 'undefined') {
        InteractiveSvgYaml.initAll();
    }

    // Generic zoom+pan fallback for all other SVG types (activity, state, ER, mindmap, etc.)
    // Runs after specific modules so they get priority; this handles anything they skip.
    (function() {
        var ZOOM_STEP = 0.2, MIN_ZOOM = 0.2, MAX_ZOOM = 5.0;
        var DRAG_THRESHOLD = 4;

        var _installed = false;
        function installGlobalPrevention() {
            if (_installed) return;
            _installed = true;
            window.addEventListener('wheel', function(e) { if (e.ctrlKey) e.preventDefault(); }, { passive: false });
        }

        function applyZoomPan(svg) {
            // Skip SVGs already handled by a specific module
            if (svg.classList.contains('interactive-svg') ||
                svg.classList.contains('interactive-svg-sequence') ||
                svg.classList.contains('interactive-svg-yaml') ||
                svg._genericZoomPan) return;

            svg._genericZoomPan = { zoomLevel: 1.0, zoomBaseW: null, zoomBaseH: null };
            var data = svg._genericZoomPan;

            installGlobalPrevention();
            svg.classList.add('interactive-svg-generic');
            svg.style.cursor = 'grab';

            // --- WHEEL ZOOM ---
            svg.addEventListener('wheel', function(e) {
                if (!e.ctrlKey) return;
                e.preventDefault();

                if (!data.zoomBaseW) {
                    var r = svg.getBoundingClientRect();
                    data.zoomBaseW = r.width;
                    data.zoomBaseH = r.height;
                }

                var rect = svg.getBoundingClientRect();
                var fx = rect.width  > 0 ? (e.clientX - rect.left) / rect.width  : 0.5;
                var fy = rect.height > 0 ? (e.clientY - rect.top)  / rect.height : 0.5;

                data.zoomLevel = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM,
                    data.zoomLevel + (e.deltaY < 0 ? 1 : -1) * ZOOM_STEP));

                svg.style.maxWidth = 'none';
                svg.style.width  = Math.round(data.zoomBaseW * data.zoomLevel) + 'px';
                svg.style.height = Math.round(data.zoomBaseH * data.zoomLevel) + 'px';

                var nr = svg.getBoundingClientRect();
                window.scrollBy({
                    left: (nr.left + fx * nr.width)  - e.clientX,
                    top:  (nr.top  + fy * nr.height) - e.clientY,
                    behavior: 'instant'
                });
            }, { passive: false });

            // --- DRAG PAN ---
            var isPanning = false, hasDragged = false, lastX, lastY;

            svg.addEventListener('mousedown', function(e) {
                if (e.button !== 0) return;
                isPanning = true; hasDragged = false;
                lastX = e.clientX; lastY = e.clientY;
                e.preventDefault();
            });

            document.addEventListener('mousemove', function(e) {
                if (!isPanning) return;
                var dx = e.clientX - lastX, dy = e.clientY - lastY;
                if (!hasDragged && (Math.abs(dx) > DRAG_THRESHOLD || Math.abs(dy) > DRAG_THRESHOLD)) {
                    hasDragged = true;
                    document.documentElement.style.setProperty('cursor', 'grabbing', 'important');
                }
                if (hasDragged) {
                    window.scrollBy({ left: -dx, top: -dy, behavior: 'instant' });
                    lastX = e.clientX; lastY = e.clientY;
                }
            });

            document.addEventListener('mouseup', function(e) {
                if (!isPanning) return;
                isPanning = false;
                document.documentElement.style.removeProperty('cursor');
                svg.style.cursor = 'grab';
                if (hasDragged) {
                    var cancelOnce = function(ev) {
                        ev.stopPropagation();
                        document.removeEventListener('click', cancelOnce, true);
                    };
                    document.addEventListener('click', cancelOnce, true);
                }
            });
        }

        // Apply to all SVGs on the page
        document.querySelectorAll('svg').forEach(applyZoomPan);
    })();

    // Initialize scroll tracking for TOC/Refs positioning
    window.addEventListener("scroll", function () {
        const scrollX = window.scrollX;
        document.documentElement.style.setProperty("--toc-scroll", Math.round(scrollX) + "px");
        document.documentElement.style.setProperty("--refs-scroll", Math.round(scrollX) + "px");
    });

    console.log('[MdExplorer] Initialization complete ✓');
});
