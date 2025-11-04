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
 * Toggle References panel visibility
 * Handles 4 states based on current visibility of Refs and TOC panels:
 * 1. Both hidden → Show Refs
 * 2. Refs hidden, TOC visible → Show Refs, hide TOC
 * 3. Refs visible, TOC hidden → Hide Refs
 * 4. Both visible → Hide TOC, show Refs (shouldn't happen normally)
 */
function toggleReferences() {
    let $refs = $('#Refs');
    let $toc = $('#TOC');

    if ($('#Refs').is(":hidden") && $('#TOC').is(":hidden")) {
        $refs.fadeIn();
        window.currentDocumentSetting.showTOC = false;
        window.currentDocumentSetting.showRefs = true;

    } else if ($('#Refs').is(":hidden") && !$('#TOC').is(":hidden")) {
        $refs.fadeIn();
        $toc.fadeOut();
        window.currentDocumentSetting.showTOC = false;
        window.currentDocumentSetting.showRefs = true;
    } else if (!$('#Refs').is(":hidden") && $('#TOC').is(":hidden")) {
        $refs.fadeOut();
        window.currentDocumentSetting.showTOC = false;
        window.currentDocumentSetting.showRefs = false;
    } else if (!$('#Refs').is(":hidden") && !$('#TOC').is(":hidden")) {
        $toc.fadeOut();
        $refs.fadeIn();
        window.currentDocumentSetting.showTOC = false;
        window.currentDocumentSetting.showRefs = true;
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
