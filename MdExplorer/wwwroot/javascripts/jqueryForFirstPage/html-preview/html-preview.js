// HTML Preview Tabs - Auto-resize iframes and highlight source code
$(document).ready(function () {

    // Auto-resize all preview iframes once they load
    function resizeIframe(iframe) {
        // Skip resize if flagged (e.g. after manual reload)
        if (iframe.dataset.skipResize) return;

        try {
            var doc = iframe.contentDocument || iframe.contentWindow.document;
            if (doc && doc.body) {
                // Expand to full window height first, so 100vh content
                // renders at proper size (same as what happens after fullscreen exit)
                var maxH = window.innerHeight || 900;
                iframe.style.height = maxH + 'px';

                // Force reflow so the content lays out at this size
                void doc.body.offsetHeight;

                // Now measure the actual content height
                var h = Math.max(
                    doc.body.scrollHeight,
                    doc.documentElement.scrollHeight
                );

                // Cap to window height to prevent feedback loops
                if (h > maxH) h = maxH;

                if (h > 0) {
                    iframe.style.height = h + 'px';
                } else {
                    iframe.style.height = '';
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

    // SVG icons for fullscreen toggle
    var svgMaximize = '<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 3 21 3 21 9"></polyline><polyline points="9 21 3 21 3 15"></polyline><line x1="21" y1="3" x2="14" y2="10"></line><line x1="3" y1="21" x2="10" y2="14"></line></svg>';
    var svgMinimize = '<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="4 14 10 14 10 20"></polyline><polyline points="20 10 14 10 14 4"></polyline><line x1="14" y1="10" x2="21" y2="3"></line><line x1="3" y1="21" x2="10" y2="14"></line></svg>';

    // Toggle fullscreen using the Fullscreen API (escapes iframe boundaries)
    $(document).on('click', '.mde-fullscreen-btn', function (e) {
        e.preventDefault();
        var btn = $(this);
        var container = btn.closest('.mde-html-preview-container')[0];

        if (document.fullscreenElement) {
            document.exitFullscreen();
        } else {
            container.requestFullscreen().catch(function (err) {
                console.log('[HTML-PREVIEW] Fullscreen request failed:', err.message);
            });
        }
    });

    // Update icon and resize iframe when fullscreen state changes
    $(document).on('fullscreenchange', function () {
        var isFullscreen = !!document.fullscreenElement;
        var container = isFullscreen
            ? $(document.fullscreenElement)
            : $('.mde-html-preview-container');

        container.find('.mde-fullscreen-btn').html(isFullscreen ? svgMinimize : svgMaximize);

        if (isFullscreen) {
            $(document.fullscreenElement).addClass('mde-fullscreen');
        } else {
            $('.mde-html-preview-container.mde-fullscreen').removeClass('mde-fullscreen');
        }

        // Resize iframe after fullscreen transition
        var iframe = container.find('.mde-html-preview-iframe')[0];
        if (iframe) {
            setTimeout(function () { resizeIframe(iframe); }, 200);
        }
    });

    // Reload iframe content (preserve current height to avoid feedback loops)
    $(document).on('click', '.mde-reload-btn', function (e) {
        e.preventDefault();
        var iframe = $(this).closest('.mde-html-preview-container').find('.mde-html-preview-iframe')[0];
        if (iframe) {
            var srcdoc = iframe.getAttribute('srcdoc');
            var currentHeight = iframe.style.height;
            // Flag to skip auto-resize on reload
            iframe.dataset.skipResize = 'true';
            iframe.removeAttribute('srcdoc');
            setTimeout(function () {
                iframe.style.height = currentHeight;
                iframe.setAttribute('srcdoc', srcdoc);
                // Remove flag after load completes
                iframe.addEventListener('load', function onReload() {
                    iframe.removeEventListener('load', onReload);
                    delete iframe.dataset.skipResize;
                }, { once: true });
            }, 50);
        }
    });

    // Zip HTML and copy to clipboard (calls backend)
    $(document).on('click', '.mde-zip-copy-btn', function (e) {
        e.preventDefault();
        var btn = $(this);
        var filePath = btn.data('filepath');
        if (!filePath || btn.hasClass('zipping')) return;

        btn.addClass('zipping');
        // Store original SVG to restore later
        var originalHtml = btn.html();
        // Show spinner
        btn.html('<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="mde-spin"><line x1="12" y1="2" x2="12" y2="6"></line><line x1="12" y1="18" x2="12" y2="22"></line><line x1="4.93" y1="4.93" x2="7.76" y2="7.76"></line><line x1="16.24" y1="16.24" x2="19.07" y2="19.07"></line><line x1="2" y1="12" x2="6" y2="12"></line><line x1="18" y1="12" x2="22" y2="12"></line><line x1="4.93" y1="19.07" x2="7.76" y2="16.24"></line><line x1="16.24" y1="7.76" x2="19.07" y2="4.93"></line></svg>');

        fetch('/api/mdfiles/ZipAndCopyToClipboard', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ filePath: filePath })
        })
        .then(function (response) {
            if (!response.ok) {
                return response.json().then(function (body) {
                    throw new Error(body.error || 'Server error');
                });
            }
            return response.json();
        })
        .then(function (data) {
            console.log('[HTML-PREVIEW] Zip & copy OK:', data.zipPath);
            btn.addClass('copied');
            btn.html(originalHtml);
            btn.attr('title', 'Copied to clipboard: ' + data.zipPath);
            setTimeout(function () {
                btn.removeClass('copied zipping');
                btn.attr('title', 'Zip and copy to clipboard');
            }, 2000);
        })
        .catch(function (err) {
            console.error('[HTML-PREVIEW] Zip & copy failed:', err.message);
            btn.addClass('errored');
            btn.html(originalHtml);
            btn.attr('title', 'Error: ' + err.message);
            setTimeout(function () {
                btn.removeClass('errored zipping');
                btn.attr('title', 'Zip and copy to clipboard');
            }, 3000);
        });
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
