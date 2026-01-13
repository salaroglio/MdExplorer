// Prevent multiple loads of common.js
if (window.commonJsLoaded) {
    console.log('=== COMMON.JS ALREADY LOADED - SKIPPING ===');
} else {
    window.commonJsLoaded = true;

    console.log('=== COMMON.JS START ===');
    console.log('User Agent:', navigator.userAgent);
    console.log('Is Electron:', navigator.userAgent.includes('Electron'));

    // Function to check if a script is already loaded
    function isScriptLoaded(src) {
        const scripts = document.querySelectorAll('script');
        for (let script of scripts) {
            if (script.src && script.src.includes(src)) {
                console.log(`Script already loaded: ${src}`);
                return true;
            }
        }
        return false;
    }

    // Function to load script only if not already loaded
    function loadScriptOnce(src) {
        if (!isScriptLoaded(src)) {
            document.write(`<script src='${src}'></script>`);
        }
    }

    // Load scripts only if not already loaded
    loadScriptOnce('/bootstrap/jquery-3.6.0.js');
    loadScriptOnce('/bootstrap/js/bootstrap.bundle.js');
    loadScriptOnce('/jquery_ui/jquery-ui.js');
    loadScriptOnce('/bootstrap_datepicker/js/bootstrap-datepicker.js');
    loadScriptOnce('/highlightjs/highlight.min.js');

    // CSS can be loaded multiple times without issues
    document.write("<link href='/prismjs/prism-tomorrow.min.css' rel='stylesheet' />");

    loadScriptOnce('/prismjs/prism.min.js');
    loadScriptOnce('/prismjs/prism-java.min.js');
    loadScriptOnce('/prismjs/prism-csharp.min.js');
    loadScriptOnce('/prismjs/prism-javascript.min.js');
    loadScriptOnce('/prismjs/prism-python.min.js');
    loadScriptOnce('/prismjs/prism-sql.min.js');

    // Always load mermaid but configure it differently based on environment
    const isElectron = navigator.userAgent.includes('Electron');

    if (isElectron) {
        console.log('Running in Electron - configuring mermaid carefully');
        // Pre-configure mermaid BEFORE loading to prevent auto-init
        window.mermaid = {
            startOnLoad: false,
            mermaid: {
                startOnLoad: false
            }
        };
        window.mermaidConfig = {
            startOnLoad: false
        };
        // Also set on global
        if (typeof global !== 'undefined') {
            global.mermaid = window.mermaid;
        }
    } else {
        console.log('Running in web browser - mermaid will auto-initialize');
    }

    console.log('Loading mermaid.min.js...');
    loadScriptOnce('/mermaid/mermaid.min.js');

    // In Electron, immediately after loading, ensure mermaid doesn't auto-start
    if (isElectron) {
        document.write(`<script>
            if (typeof mermaid !== 'undefined' && mermaid.initialize) {
                console.log('Forcing mermaid startOnLoad to false after load');
                mermaid.initialize({ startOnLoad: false });
            }
        </script>`);
    }

    // ============================================================================
    // jqueryForFirstPage.js Loading Strategy
    // ============================================================================
    // Feature flag: Set to true to use new modular structure, false for monolithic file
    const USE_MODULAR_STRUCTURE = true; // Modular structure activated

    if (USE_MODULAR_STRUCTURE) {
        console.log('=== LOADING MODULAR jqueryForFirstPage FILES ===');

        // Load in dependency order (globals first, init last)
        // CORE: Foundation - global variables and utilities
        loadScriptOnce('/javascripts/jqueryForFirstPage/core/globals.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/core/utilities.js');

        // TIPPY: Must load BEFORE emoji modules (emoji-tooltips.js depends on tippy())
        console.log('🔍 [COMMON.JS] Loading Tippy.js libraries...');
        loadScriptOnce('/tippy/popper.js');
        loadScriptOnce('/tippy/tippy.js');
        console.log('🔍 [COMMON.JS] Tippy.js loading queued');

        // EMOJIS: Tippy tooltips, sortable, and interactions
        console.log('🔍 [COMMON.JS] Loading emoji modules...');
        loadScriptOnce('/javascripts/jqueryForFirstPage/emojis/emoji-tooltips.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/emojis/emoji-sortable.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/emojis/emoji-interactions.js');
        console.log('🔍 [COMMON.JS] Emoji modules loading queued');

        // IMAGES: Readability, magnifier, transform (move/resize)
        loadScriptOnce('/javascripts/jqueryForFirstPage/images/image-readability.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/images/image-magnifier.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/images/image-transform.js');

        // NAVIGATION: History and scroll position management
        loadScriptOnce('/javascripts/jqueryForFirstPage/navigation/navigation-history.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/navigation/scroll-position.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/navigation/sharepoint-tooltips.js');

        // SEARCH: Full-text search with highlighting
        loadScriptOnce('/javascripts/jqueryForFirstPage/search/search-functionality.js');

        // PANELS: TOC, references, and resize functionality
        loadScriptOnce('/javascripts/jqueryForFirstPage/panels/toc-manager.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/panels/references-manager.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/panels/panel-resize.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/panels/panels-initialization.js');

        // CALENDAR: Datepicker conflict resolution
        loadScriptOnce('/javascripts/jqueryForFirstPage/calendar/calendar-picker.js');

        // DRAWING: Canvas drawing tool with color palette
        loadScriptOnce('/javascripts/jqueryForFirstPage/drawing/canvas-drawing.js');

        // DIAGRAMS: PlantUML, Mermaid, and syntax highlighting
        loadScriptOnce('/javascripts/jqueryForFirstPage/diagrams/plantuml-integration.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/highlighting/syntax-highlighting.js');
        loadScriptOnce('/javascripts/jqueryForFirstPage/diagrams/mermaid-rendering.js');

        // INTERACTIVE-SVG: Click-to-highlight for PlantUML diagrams
        document.write("<link href='/javascripts/jqueryForFirstPage/interactive-svg/interactive-svg.css' rel='stylesheet' />");
        loadScriptOnce('/javascripts/jqueryForFirstPage/interactive-svg/interactive-svg.js');
        document.write("<link href='/javascripts/jqueryForFirstPage/interactive-svg/interactive-svg-sequence.css' rel='stylesheet' />");
        loadScriptOnce('/javascripts/jqueryForFirstPage/interactive-svg/interactive-svg-sequence.js');

        // CORE: Initialization coordinator (MUST BE LAST)
        loadScriptOnce('/javascripts/jqueryForFirstPage/core/init.js');

        console.log('=== MODULAR FILES LOADED (23 files) ===');
    } else {
        console.log('=== LOADING MONOLITHIC jqueryForFirstPage.js ===');
        loadScriptOnce('/javascripts/jqueryForFirstPage.js');
    }

    loadScriptOnce('/TocBot/tocbot.min.js');
    loadScriptOnce('/jspreadsheet_ce/jsuites.js');
    loadScriptOnce('/jspreadsheet_ce/jexcel.js');
    // Tippy.js moved earlier in modular structure loading (lines 98-99)
    // Still load here for backward compatibility if modular structure is disabled
    if (!USE_MODULAR_STRUCTURE) {
        loadScriptOnce('/tippy/popper.js');
        loadScriptOnce('/tippy/tippy.js');
    }
    loadScriptOnce('/highlight_within_textarea/highlight-within-textarea.js');

    console.log('=== COMMON.JS END ===');
}