/**
 * MdExplorer - Global Variables
 * ==============================
 * Centralized global state for all jqueryForFirstPage modules
 *
 * DO NOT add new globals without documenting here
 */

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

// Image Magnifier
window.magnifierActive = {};
window.magnifierCanvas = null;
window.magnifierContext = null;
window.magnifierCache = new Map();
window.magnifierRAF = null;

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
