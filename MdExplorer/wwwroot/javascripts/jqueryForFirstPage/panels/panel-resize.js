/**
 * MdExplorer - Panel Resize Functionality
 * ========================================
 * Enables drag-to-resize for TOC and References panels
 *
 * Features:
 * - Live drag resizing using CSS custom properties
 * - Persists width to backend on mouse release
 * - Uses hookedToc/hookedRefs flags to track active resize
 * - Accounts for scrollbar width in calculations
 *
 * Global dependencies:
 * - window.hookedToc (from globals.js)
 * - window.hookedRefs (from globals.js)
 * - window.currentDocumentSetting (from globals.js)
 *
 * DOM dependencies:
 * - #TOC: Table of contents panel
 * - #Refs: References panel
 *
 * CSS Custom Properties:
 * - --toc-width: Dynamic TOC panel width
 * - --refs-width: Dynamic References panel width
 *
 * Backend API:
 * - POST /api/tabcontroller/SaveTOCData (for TOC)
 * - POST /api/tabcontroller/SaveRefsData (for Refs)
 */

/**
 * Activate TOC panel resize mode
 * Sets hookedToc flag to enable mousemove tracking
 */
function resizeToc() {
    window.hookedToc = true;
}

/**
 * Activate References panel resize mode
 * Sets hookedRefs flag to enable mousemove tracking
 */
function resizeRefs() {
    window.hookedRefs = true;
}

/**
 * Initialize resize event listeners
 * Attaches mousemove and mouseup handlers to document
 */
$(function () {
    document.addEventListener("mousemove", mouseMoveEvent, false);
    document.addEventListener("mouseup", mouseUpEvent, false);
});

/**
 * Handle mouse release after resize drag
 * Saves final panel width to backend and resets hooked flags
 *
 * @param {MouseEvent} event - Mouse event with clientX position
 */
function mouseUpEvent(event) {

    let toc$ = $('#TOC');
    let refs$ = $('#Refs');

    if (window.hookedToc) {
        window.hookedToc = false;
        let value = parseInt(event.clientX) + 30;
        window.currentDocumentSetting.tocWidth = parseInt(toc$.css("width").substring(0, toc$.css("width").length - 2));
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
    if (window.hookedRefs) {
        window.hookedRefs = false;
        let value = parseInt(event.clientX) + 30;

        window.currentDocumentSetting.refsWidth = parseInt(refs$.css("width").substring(0, refs$.css("width").length - 2));
        $.ajax({
            url: "/api/tabcontroller/SaveRefsData",
            type: "POST",
            data: JSON.stringify(window.currentDocumentSetting),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {

                console.log(data);
            }
        });

    }
}

/**
 * Handle mouse movement during resize drag
 * Updates CSS custom properties for live panel width adjustment
 * Calculation: scrollWidth - clientX - 30 - scrollbarWidth
 *
 * @param {MouseEvent} event - Mouse event with clientX position
 */
function mouseMoveEvent(event) {

    let toc$ = $('#TOC');
    let refs$ = $('#Refs');
    if (window.hookedToc) {
        let value = document.documentElement.scrollWidth - parseInt(event.clientX) - 30;
        let scrolldata = document.documentElement.scrollWidth - document.documentElement.clientWidth;
        console.log(scrolldata);
        value = value - scrolldata;
        document.documentElement.style.setProperty("--toc-width", value + "px");

    }
    if (window.hookedRefs) {
        let value = document.documentElement.scrollWidth - parseInt(event.clientX) - 30;
        let scrolldata = document.documentElement.scrollWidth - document.documentElement.clientWidth;
        console.log(scrolldata);
        value = value - scrolldata;
        document.documentElement.style.setProperty("--refs-width", value + "px");

    }
}
