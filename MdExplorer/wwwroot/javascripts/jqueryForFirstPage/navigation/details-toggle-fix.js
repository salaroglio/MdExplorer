/**
 * MdExplorer - Details/Summary Toggle Fix
 * ========================================
 * Prevents scroll jump when toggling <details> elements.
 *
 * Problem: Chromium adjusts scroll position when <details> opens/closes
 * to keep the interacted element visible. This causes unexpected jumps.
 *
 * Solution: Capture scrollY before toggle, restore after layout reflow.
 */
(function() {
    var savedScrollY = null;

    // Phase 1: Capture scroll position BEFORE the toggle happens
    document.addEventListener('click', function(e) {
        var summary = e.target.closest('summary');
        if (summary) {
            savedScrollY = window.scrollY;
        }
    }, true); // useCapture = true to run before default behavior

    // Phase 2: Restore scroll position AFTER the toggle completes
    document.addEventListener('toggle', function(e) {
        if (savedScrollY !== null) {
            var targetY = savedScrollY;
            savedScrollY = null;
            requestAnimationFrame(function() {
                window.scrollTo(0, targetY);
            });
        }
    }, true);
})();
