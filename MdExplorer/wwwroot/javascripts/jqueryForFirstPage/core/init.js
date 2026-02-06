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
    // Component diagrams (with elem_, cluster_, link_ elements)
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

    // Initialize scroll tracking for TOC/Refs positioning
    window.addEventListener("scroll", function () {
        const scrollX = window.scrollX;
        document.documentElement.style.setProperty("--toc-scroll", Math.round(scrollX) + "px");
        document.documentElement.style.setProperty("--refs-scroll", Math.round(scrollX) + "px");
    });

    console.log('[MdExplorer] Initialization complete ✓');
});
