/**
 * MdExplorer - Panels Initialization
 * ===================================
 * Initializes TOC and Knowledge Graph panels on page load
 *
 * Features:
 * - Initializes tocbot library
 * - Loads saved TOC/Refs settings from backend
 * - Applies saved panel widths (CSS custom properties)
 * - Shows/hides panels based on saved settings
 * - Triggers Knowledge Graph initialization when its panel is visible
 *
 * Global dependencies:
 * - window.currentDocumentSetting (from globals.js)
 * - tocbot library
 *
 * Backend API:
 * - GET /api/tabcontroller/GetTOCData
 * - GET /api/tabcontroller/GetKnowledgeGraph (via kg-manager.js)
 *
 * DOM:
 * - #TOC: Table of contents panel
 * - #Refs: Side panel hosting the Knowledge Graph
 * - #kg-canvas: Knowledge Graph render target
 * - #MdBody: Main body element with connectionid
 */

$(function () {
    // Initialize tocbot for automatic TOC generation
    tocbot.init({
        tocSelector: '.js-toc',
        orderedList: true,
        hasInnerContainers: true,
        scrollSmooth: true,
        headingSelector: 'h1, h2, h3, h4, h5, h6',
        scrollSmoothDuration: 220,
        positionFixedClass: 'is-position-fixed',
    });
    setTimeout(tocbot.refresh());

    let $TOC = $("#TOC");
    let pathFile = $TOC.attr("mdeFullPathDocument");

    // Load TOC/Refs settings and visibility
    $.get("/api/tabcontroller/GetTOCData?fullPathFile=" + pathFile, function (documentSetting) {
        if (documentSetting == undefined) {
            return;
        }
        window.currentDocumentSetting = documentSetting;

        let $Toc = $('#TOC');
        let $Refs = $("#Refs");

        if (window.currentDocumentSetting.tocWidth != null && window.currentDocumentSetting.tocWidth != 0) {
            document.documentElement.style.setProperty("--toc-width", window.currentDocumentSetting.tocWidth + "px");
        }
        if (window.currentDocumentSetting.refsWidth != null && window.currentDocumentSetting.refsWidth != 0) {
            document.documentElement.style.setProperty("--refs-width", window.currentDocumentSetting.refsWidth + "px");
        }

        if (documentSetting.showTOC) {
            $Toc.show();
        } else {
            $Toc.hide();
        }
        // Knowledge Graph is opened on demand via the K.G. button (fullscreen overlay).
        // Legacy #Refs side panel removed.
    });
});
