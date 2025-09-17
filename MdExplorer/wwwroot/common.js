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

    loadScriptOnce('/javascripts/jqueryForFirstPage.js');
    loadScriptOnce('/TocBot/tocbot.min.js');
    loadScriptOnce('/jspreadsheet_ce/jsuites.js');
    loadScriptOnce('/jspreadsheet_ce/jexcel.js');
    loadScriptOnce('/tippy/popper.js');
    loadScriptOnce('/tippy/tippy.js');
    loadScriptOnce('/highlight_within_textarea/highlight-within-textarea.js');

    console.log('=== COMMON.JS END ===');
}