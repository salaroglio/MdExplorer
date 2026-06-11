/**
 * MdExplorer - References Panel Manager
 * ======================================
 * Manages the visibility and state of the References panel
 *
 * Features:
 * - Toggle References visibility with fadeIn/fadeOut animations
 * - Mutually exclusive with TOC panel (only one visible at a time)
 * - Persists state to backend via API
 * - Updates currentDocumentSetting global state
 *
 * Global dependencies:
 * - window.currentDocumentSetting (from globals.js)
 *
 * DOM dependencies:
 * - #TOC: Table of contents panel
 * - #Refs: References panel
 *
 * Backend API:
 * - POST /api/tabcontroller/SaveTOCData
 */

/**
 * Open References panel (triggered on hover).
 * Closes TOC if open (panels are mutually exclusive).
 */
function openReferences() {
    var $refs = $('#Refs');
    if (!$refs.is(":hidden")) return; // already open

    $refs.fadeIn();
    $('#TOC').fadeOut();
    window.currentDocumentSetting.showTOC = false;
    window.currentDocumentSetting.showRefs = true;
    _savePanelState();

}

/**
 * Close References panel (triggered on click).
 */
function closeReferences() {
    var $refs = $('#Refs');
    if ($refs.is(":hidden")) return; // already closed

    $refs.fadeOut();
    window.currentDocumentSetting.showRefs = false;
    _savePanelState();
}

/**
 * Toggle References panel visibility (kept for backward compatibility).
 */
function toggleReferences() {
    if ($('#Refs').is(":hidden")) {
        openReferences();
    } else {
        closeReferences();
    }
}
