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
 * Toggle TOC panel visibility
 * Handles 4 states based on current visibility of TOC and Refs panels:
 * 1. Both hidden → Show TOC
 * 2. TOC visible, Refs hidden → Hide TOC
 * 3. TOC hidden, Refs visible → Show TOC, hide Refs
 * 4. Both visible → Show TOC, hide Refs (shouldn't happen normally)
 *
 * @param {string} documentPath - Path of current document (unused but kept for compatibility)
 */
function toggleTOC(documentPath) {

    let $refs = $('#Refs');
    let $toc = $('#TOC');

    if ($('#Refs').is(":hidden") && $('#TOC').is(":hidden")) {
        $toc.fadeIn();
        window.currentDocumentSetting.showTOC = true;
        window.currentDocumentSetting.showRefs = false;

    } else if ($('#Refs').is(":hidden") && !$('#TOC').is(":hidden")) {
        $toc.fadeOut();
        window.currentDocumentSetting.showTOC = false;
        window.currentDocumentSetting.showRefs = false;
    } else if (!$('#Refs').is(":hidden") && $('#TOC').is(":hidden")) {
        $toc.fadeIn();
        $refs.fadeOut();
        window.currentDocumentSetting.showTOC = true;
        window.currentDocumentSetting.showRefs = false;
    } else if (!$('#Refs').is(":hidden") && !$('#TOC').is(":hidden")) {
        $toc.fadeIn();
        $refs.fadeOut();
        window.currentDocumentSetting.showTOC = true;
        window.currentDocumentSetting.showRefs = false;
    }

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
