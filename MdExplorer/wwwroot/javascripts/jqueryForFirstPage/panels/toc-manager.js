/**
 * MdExplorer - Table of Contents Manager
 * =======================================
 * Manages the visibility and state of the TOC panel
 *
 * Features:
 * - Toggle TOC visibility with fadeIn/fadeOut animations
 * - Mutually exclusive with References panel (only one visible at a time)
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
 * Open TOC panel (triggered on hover).
 * Closes Refs if open (panels are mutually exclusive).
 */
function openTOC() {
    var $toc = $('#TOC');
    if (!$toc.is(":hidden")) return; // already open

    $toc.fadeIn();
    $('#Refs').fadeOut();
    window.currentDocumentSetting.showTOC = true;
    window.currentDocumentSetting.showRefs = false;
    _savePanelState();
}

/**
 * Close TOC panel (triggered on click).
 */
function closeTOC() {
    var $toc = $('#TOC');
    if ($toc.is(":hidden")) return; // already closed

    $toc.fadeOut();
    window.currentDocumentSetting.showTOC = false;
    _savePanelState();
}

/**
 * Toggle TOC panel visibility (kept for backward compatibility).
 * @param {string} documentPath
 */
function toggleTOC(documentPath) {
    if ($('#TOC').is(":hidden")) {
        openTOC();
    } else {
        closeTOC();
    }
}

/** Persist panel state to backend */
function _savePanelState() {
    $.ajax({
        url: "/api/tabcontroller/SaveTOCData",
        type: "POST",
        data: JSON.stringify(window.currentDocumentSetting),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            console.log(data);
        }
    });
}
