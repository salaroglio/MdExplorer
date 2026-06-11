// mde-text-include: fullscreen handler for ```text(path) blocks.
// Copy-Path button is handled by html-preview.js (global .mde-copy-path-btn handler) — we reuse it.
// Prism highlighting happens automatically on page load for any <code class="language-*">.

$(document).ready(function () {

    // Fullscreen toggle scoped to .mde-text-include-container
    $(document).on('click', '.mde-text-include-fullscreen-btn', function (e) {
        e.preventDefault();
        var container = $(this).closest('.mde-text-include-container')[0];
        if (!container) return;

        if (document.fullscreenElement) {
            document.exitFullscreen();
        } else {
            container.requestFullscreen().catch(function (err) {
                console.log('[TEXT-INCLUDE] Fullscreen request failed:', err.message);
            });
        }
    });

    // Force Prism re-highlight on dynamic content (some flows insert the block after page load)
    function highlightAll() {
        if (typeof Prism === 'undefined') return;
        document.querySelectorAll('.mde-text-include-pre code').forEach(function (el) {
            if (el.dataset.highlighted) return;
            el.dataset.highlighted = '1';
            try { Prism.highlightElement(el); } catch (_) { /* ignore unknown language */ }
        });
    }

    highlightAll();
    setTimeout(highlightAll, 200);
    setTimeout(highlightAll, 600);
});
