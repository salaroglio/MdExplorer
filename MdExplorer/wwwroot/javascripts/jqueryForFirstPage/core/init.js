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
        toc: typeof toggleTOC !== 'undefined'
    });

    // Initialize scroll tracking for TOC/Refs positioning
    window.addEventListener("scroll", function () {
        const scrollX = window.scrollX;
        document.documentElement.style.setProperty("--toc-scroll", Math.round(scrollX) + "px");
    });

    console.log('[MdExplorer] Initialization complete ✓');
});
