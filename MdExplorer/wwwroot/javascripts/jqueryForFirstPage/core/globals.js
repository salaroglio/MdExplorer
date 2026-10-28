/**
 * MdExplorer - Global Variables
 * ==============================
 * Centralized global state for all jqueryForFirstPage modules
 *
 * DO NOT add new globals without documenting here
 */

// ============================================================================
// STATIC EXPORT
// ============================================================================
// The page is a file of the static HTML export (a zip opened without MdExplorer: data-mde-export on
// <html>). Nothing is saved there — there is no service to save to: the modules that write (panel widths,
// image position) check this first. Sprint docs-internal/Sprints/2026-09-30-Slide-Export-HTML.md
window.mdeStaticExport = document.documentElement.hasAttribute('data-mde-export');

/**
 * The address of one of MdExplorer's files a script sets by itself (an icon it swaps): from the site's root
 * in MdExplorer, from the zip's _mde/ folder in an export. Each file named this way must be in
 * DocumentViewAssets.ScriptAssets, which the export copies (a test reads the scripts for mdeAsset('…')).
 */
window.mdeAsset = function (path) {
    var root = document.documentElement.getAttribute('data-mde-export-root');
    return window.mdeStaticExport ? (root || '') + '_mde/' + path : '/' + path;
};

// ============================================================================
// DOCUMENT SETTINGS
// ============================================================================
window.currentDocumentSetting = {};

// ============================================================================
// NAVIGATION & HISTORY
// ============================================================================
window.navigationHistory = [];
window.currentHistoryIndex = -1;
window.hasNavigationStarted = 0; // 0 = no clicks yet, 1+ = has been clicked

// ============================================================================
// SEARCH FUNCTIONALITY
// ============================================================================
window.searchResults = [];
window.currentSearchIndex = -1;
window.originalContent = null;

// ============================================================================
// IMAGE MANAGEMENT
// ============================================================================
window.arrayReadabilityToggle = [];
window.arrayLinksMoveToggle = [];
window.arrayLinksResizeToggle = [];
window.moving = false;
window.image = null;

// Auto-fit state tracking
// Stores { id: string, originalDivStyle: string, originalImgStyle: string, originalImgClass: string, isAutoFit: boolean }
window.arrayAutoFitState = [];

// SVG Text Search
window.svgSearchActive = {};

// ============================================================================
// CANVAS DRAWING TOOL
// ============================================================================
window.toggleCanvas = false;
window.canvas = null;
window.ctx = null;
window.pos = { x: 0, y: 0 };
window.scrollPos = { x: 0, y: 0 };
window.currentColor = '#FF0000';
window.isErasing = false;
window.brushSize = 3;

// ============================================================================
// TOC & REFERENCES PANEL RESIZE
// ============================================================================
window.hookedToc = false;
window.hookedRefs = false;

// ============================================================================
// TOOLTIP MANAGEMENT (Tippy.js)
// ============================================================================
window.tippyDictPriority = [];
window.tippyDictProcess = [];
