/**
 * MdExplorer - Panels Initialization
 * ===================================
 * Initializes TOC and References panels on page load
 *
 * Features:
 * - Initializes tocbot library
 * - Loads saved TOC/Refs settings from backend
 * - Applies saved panel widths (CSS custom properties)
 * - Shows/hides panels based on saved settings
 * - Populates References table with backlinks
 *
 * Global dependencies:
 * - window.currentDocumentSetting (from globals.js)
 * - tocbot library
 *
 * Backend API:
 * - GET /api/tabcontroller/GetTOCData
 * - GET /api/tabcontroller/GetRefsData
 *
 * DOM:
 * - #TOC: Table of contents panel
 * - #Refs: References panel
 * - #references: References table container
 * - #MdBody: Main body element with connectionid
 */

/**
 * Initialize TOC and References panels
 * Called on document ready
 */
$(function () {
    // Initialize tocbot for automatic TOC generation
    tocbot.init({
        tocSelector: '.js-toc',
        orderedList: true,
        hasInnerContainers: true,
        scrollSmooth: true,
        headingSelector: 'h1, h2, h3, h4, h5, h6',
        // Smooth scroll duration.
        scrollSmoothDuration: 220,
        positionFixedClass: 'is-position-fixed',

    });
    setTimeout(tocbot.refresh());

    // visualizzazione toc
    let $TOC = $("#TOC");

    let pathFile = $TOC.attr("mdeFullPathDocument");

    // This set TOC/References visible
    $.get("/api/tabcontroller/GetTOCData?fullPathFile=" + pathFile, function (documentSetting) {
        if (documentSetting == undefined) {
            return;
        }
        window.currentDocumentSetting = documentSetting;

        let $Toc = $('#TOC');
        let $Refs = $("#Refs");

        // Apply saved panel widths
        if (window.currentDocumentSetting.tocWidth != null && window.currentDocumentSetting.tocWidth != 0) {
            document.documentElement.style.setProperty("--toc-width", window.currentDocumentSetting.tocWidth + "px");

        }
        if (window.currentDocumentSetting.refsWidth != null && window.currentDocumentSetting.refsWidth != 0) {
            document.documentElement.style.setProperty("--refs-width", window.currentDocumentSetting.refsWidth + "px");
        }

        // Show/hide panels based on saved settings
        if (documentSetting.showTOC) {
            $Toc.show();
        } else {
            $Toc.hide();
        }
        if (documentSetting.showRefs) {
            $Refs.show();
        } else {
            $Refs.hide();
        }


    });

    // this populate References
    $.get("/api/tabcontroller/GetRefsData?fullPathFile=" + pathFile, function (references) {
        let $Refs = $("#Refs");
        let $body = $("#MdBody");

        // if there are NO references hide again
        if (references == undefined || references.length == 0) {
            $Refs.hide();
        }

        $ref = $("#references");
        $ref.append("<table>");
        $ref.append("<tr><th>Context</th><th>FileName</th><th>Link Type</th></tr>");
        if (references == undefined || references.length == 0) {
            $ref.append("<tr><td>No references</td></tr>")
        } else {





            references.forEach(_ => {
                let urlWithConnectionId = "/api/mdexplorer" + _.mdContext + "/" + _.markdownFile.fileName + "?connectionid=" + $body.attr("connectionid");

                $ref.append("<tr><td>" + _.mdContext + "</td><td><a class='mdExplorerLink' href='" + urlWithConnectionId +"'>" + _.markdownFile.fileName + "</a></td><td>" + _.linkType +"</td></tr>")
            });
        }

        $ref.append("</table>")
    });

});
