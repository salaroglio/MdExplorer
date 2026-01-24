/**
 * MdExplorer - Image Readability Toggle
 * ======================================
 * Optimizes image display for viewport readability
 *
 * Features:
 * - Toggle between 100% size and viewport-adapted size
 * - Stores original styles for restoration
 * - Handles both IMG and SVG elements
 * - Applies max-width constraints with auto-height
 * - Auto-fit images wider than viewport on load
 *
 * Global dependencies:
 * - window.arrayReadabilityToggle (from globals.js)
 *
 * State format:
 * { id: string, divStyle: string, imgStyle: string, imgClass: string }
 */

/**
 * Get the natural/intrinsic width of an element (IMG or SVG)
 */
function getElementNaturalWidth(element) {
    var tagName = element.tagName.toUpperCase();

    if (tagName === 'IMG') {
        return element.naturalWidth;
    } else if (tagName === 'SVG') {
        // Try to get width from various SVG sources
        // 1. Check width attribute
        var widthAttr = element.getAttribute('width');
        if (widthAttr && !widthAttr.includes('%')) {
            var parsed = parseFloat(widthAttr);
            if (!isNaN(parsed)) return parsed;
        }

        // 2. Check viewBox attribute
        var viewBox = element.getAttribute('viewBox');
        if (viewBox) {
            var parts = viewBox.split(/[\s,]+/);
            if (parts.length >= 4) {
                var vbWidth = parseFloat(parts[2]);
                if (!isNaN(vbWidth)) return vbWidth;
            }
        }

        // 3. Try getBBox() for rendered SVG
        try {
            var bbox = element.getBBox();
            if (bbox && bbox.width > 0) return bbox.width;
        } catch (e) {
            // getBBox can fail if SVG is not rendered
        }

        // 4. Fallback to getBoundingClientRect
        var rect = element.getBoundingClientRect();
        if (rect.width > 0) return rect.width;
    }

    return 0;
}

/**
 * Auto-fit images/SVGs that are wider than the viewport
 * Called automatically when elements load
 */
function autoFitElementIfNeeded(element) {
    // Get the viewport width with some margin
    var viewportWidth = window.innerWidth - 50; // 25px margin per side

    // Get the element's natural width
    var naturalWidth = getElementNaturalWidth(element);

    console.log('[autoFitElementIfNeeded] Checking element:', element.tagName, 'naturalWidth:', naturalWidth, 'viewportWidth:', viewportWidth);

    // If element is wider than viewport, apply autofit
    if (naturalWidth > viewportWidth) {
        var $element = $(element);
        var $container = $element.closest('div[id]');

        if ($container.length > 0) {
            var containerId = $container.attr('id');

            // Check if not already in readability mode
            var alreadyToggled = window.arrayReadabilityToggle.find(data => data.id == containerId);
            if (!alreadyToggled) {
                console.log('[autoFitElementIfNeeded] Element wider than viewport, applying autofit:', naturalWidth, '>', viewportWidth);

                // Store original styles for potential manual toggle later
                var dataToStore = {
                    id: containerId,
                    divStyle: $container.attr('style') || '',
                    imgStyle: $element.attr('style') || '',
                    imgClass: $element.attr('class') || '',
                    autoApplied: true // Mark as auto-applied
                };

                // Apply autofit styles to container
                $container.css({
                    'width': 'auto',
                    'height': 'auto',
                    'max-width': '100%'
                });

                // Apply autofit styles to element
                $element.css({
                    'width': '100%',
                    'height': 'auto',
                    'max-width': '100%'
                });

                window.arrayReadabilityToggle.push(dataToStore);
                console.log('[autoFitElementIfNeeded] Autofit applied to element in container:', containerId);
            }
        }
    }
}

/**
 * Initialize auto-fit for all images and SVGs in the document
 * Called on document ready
 */
function initAutoFitImages() {
    console.log('[initAutoFitImages] Initializing auto-fit for images and SVGs...');

    // Find all images in containers (both simple and with curly brackets)
    $('div.simpleImgContainer img, div.defaultImg img').each(function() {
        var img = this;

        // If image is already loaded, check immediately
        if (img.complete && img.naturalWidth > 0) {
            autoFitElementIfNeeded(img);
        } else {
            // Otherwise, wait for the image to load
            $(img).on('load', function() {
                autoFitElementIfNeeded(this);
            });
        }
    });

    // Find all SVGs in containers
    $('div.simpleImgContainer svg, div.defaultImg svg').each(function() {
        // SVGs don't have a load event, check immediately
        autoFitElementIfNeeded(this);
    });

    console.log('[initAutoFitImages] Auto-fit initialization complete');
}

// Initialize auto-fit when document is ready
$(document).ready(function() {
    // Small delay to ensure all elements are in the DOM and rendered
    setTimeout(initAutoFitImages, 200);
});

/**
 * Toggle image readability mode
 * First click: Adapts image to viewport (with 100px margins)
 * Second click: Restores original 100% size
 *
 * @param {string} stringMatchedHash - ID of the image container element
 */
function toggleSeeMe(stringMatchedHash) {
    console.log('[toggleSeeMe] ========== FUNCTION CALLED ==========');
    console.log('[toggleSeeMe] called with stringMatchedHash:', stringMatchedHash);

    var $box = $('#' + stringMatchedHash);
    console.log('[toggleSeeMe] $box element:', $box);
    console.log('[toggleSeeMe] $box length:', $box.length);

    if ($box.length === 0) {
        console.error('[toggleSeeMe] Element not found with id:', stringMatchedHash);
        return;
    }

    // Debug: mostra la struttura HTML del div
    console.log('[toggleSeeMe] Box HTML:', $box.html());
    console.log('[toggleSeeMe] Box children:', $box.children());

    // Trova l'immagine all'interno del div (potrebbe essere in elementi annidati)
    var $img = $box.find('img');
    console.log('[toggleSeeMe] Found image with find:', $img);

    // Se non trova con find, prova con un selettore più specifico
    if ($img.length === 0) {
        $img = $box.find('svg');
        console.log('[toggleSeeMe] Found SVG instead:', $img);
    }

    // Verifica se abbiamo trovato qualcosa
    if ($img.length > 0) {
        console.log('[toggleSeeMe] Element type:', $img[0].tagName);
        if ($img[0].tagName === 'IMG') {
            console.log('[toggleSeeMe] Image natural width:', $img[0].naturalWidth);
            console.log('[toggleSeeMe] Image natural height:', $img[0].naturalHeight);
        }
    } else {
        console.log('[toggleSeeMe] No image or SVG found in the box');
    }

    console.log('[toggleSeeMe] Viewport width:', window.innerWidth);
    console.log('[toggleSeeMe] Document width:', document.documentElement.clientWidth);

    var boxId = $box[0].id;
    console.log('[toggleSeeMe] boxId:', boxId);
    console.log('[toggleSeeMe] current div style:', $box.attr('style'));
    if ($img.length > 0) {
        console.log('[toggleSeeMe] current img/svg style:', $img.attr('style'));
    }

    var buttonPressed = window.arrayReadabilityToggle.find(data => data.id == boxId);
    console.log('[toggleSeeMe] buttonPressed:', buttonPressed);
    console.log('[toggleSeeMe] arrayReadabilityToggle:', window.arrayReadabilityToggle);

    if (buttonPressed == undefined) {
        // Prima volta - passa da 100% a dimensioni adattate al viewport
        var dataToStore = {
            id: boxId,
            divStyle: $box.attr('style') || '',
            imgStyle: $img.length > 0 ? ($img.attr('style') || '') : '',
            imgClass: $img.length > 0 ? ($img.attr('class') || '') : ''
        };
        console.log('[toggleSeeMe] Storing original styles (100%):', dataToStore);

        // Calcola la larghezza disponibile (considerando margini e padding)
        var availableWidth = window.innerWidth - 100; // 50px di margine per lato
        var maxWidth = Math.min(availableWidth, document.documentElement.clientWidth - 100);

        console.log('[toggleSeeMe] Calculated max width:', maxWidth);

        // Applica stile adattato al viewport sul div contenitore
        $box.css({
            'width': maxWidth + 'px',
            'height': 'auto',
            'max-width': '100%',
            'overflow': 'hidden'
        });

        // Se c'è un'immagine o SVG, adattala al contenitore
        if ($img.length > 0) {
            $img.css({
                'width': '100%',
                'height': 'auto',
                'max-width': '100%',
                'object-fit': 'contain'
            });
        }

        window.arrayReadabilityToggle.push(dataToStore);
        console.log('[toggleSeeMe] Applied viewport-adapted size');
    }
    else {
        // Seconda volta - torna alle dimensioni originali (100%)
        console.log('[toggleSeeMe] Restoring original styles (100%):', buttonPressed);

        // Ripristina gli stili originali
        $box.attr('style', buttonPressed.divStyle);

        if ($img.length > 0) {
            $img.attr('style', buttonPressed.imgStyle);
            if (buttonPressed.imgClass) {
                $img.attr('class', buttonPressed.imgClass);
            }
        }

        var currentIndex = window.arrayReadabilityToggle.findIndex(data => data.id == boxId);
        window.arrayReadabilityToggle.splice(currentIndex, 1);
        console.log('[toggleSeeMe] Restored original 100% size');
    }
}
