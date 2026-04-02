// HTML Preview Tabs - Auto-resize iframes and highlight source code
$(document).ready(function () {

    // Auto-resize all preview iframes once they load
    function resizeIframe(iframe) {
        try {
            var doc = iframe.contentDocument || iframe.contentWindow.document;
            if (doc && doc.body) {
                // Force layout recalculation
                var h = Math.max(
                    doc.body.scrollHeight,
                    doc.body.offsetHeight,
                    doc.documentElement.scrollHeight,
                    doc.documentElement.offsetHeight
                );
                if (h > 0) {
                    iframe.style.height = (h + 20) + 'px';
                }
            }
        } catch (e) {
            // Cross-origin fallback: can't access contentDocument
            console.log('[HTML-PREVIEW] Cannot access iframe contentDocument:', e.message);
        }
    }

    // Attach onload to all preview iframes (current and future)
    function initIframes() {
        var iframes = document.querySelectorAll('.mde-html-preview-iframe');
        iframes.forEach(function (iframe) {
            if (iframe.dataset.resizeAttached) return;
            iframe.dataset.resizeAttached = 'true';

            iframe.addEventListener('load', function () {
                // Resize immediately
                resizeIframe(iframe);
                // Resize again after a short delay (images, fonts, SVG rendering)
                setTimeout(function () { resizeIframe(iframe); }, 200);
                setTimeout(function () { resizeIframe(iframe); }, 600);
            });

            // If already loaded (e.g., cached), resize now
            if (iframe.contentDocument && iframe.contentDocument.readyState === 'complete') {
                resizeIframe(iframe);
            }
        });
    }

    // Init on document ready
    initIframes();
    // Also init after a delay in case DOM is still building
    setTimeout(initIframes, 500);

    // Re-highlight source code when Source tab is shown (Prism may not have run on hidden content)
    $(document).on('shown.bs.tab', '.mde-html-preview-tabs a[data-bs-toggle="tab"]', function (e) {
        var targetId = $(e.target).attr('href');
        if (targetId && targetId.indexOf('mde-source-') > -1) {
            var codeBlock = $(targetId).find('code.language-markup');
            if (codeBlock.length > 0 && typeof Prism !== 'undefined') {
                Prism.highlightElement(codeBlock[0]);
            }
        }
    });

    // Copy file path to clipboard
    $(document).on('click', '.mde-copy-path-btn', function (e) {
        e.preventDefault();
        var btn = $(this);
        var filePath = btn.data('filepath');
        if (!filePath) return;

        navigator.clipboard.writeText(filePath).then(function () {
            btn.addClass('copied');
            setTimeout(function () {
                btn.removeClass('copied');
            }, 1500);
        });
    });
});
