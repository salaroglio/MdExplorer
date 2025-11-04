/**
 * MdExplorer - Image Magnifier / Zoom Tool
 * =========================================
 * Provides 2.5x zoom lens for images and SVG diagrams
 *
 * Features:
 * - Floating magnifier canvas with smart positioning
 * - 2.5x zoom factor with crosshair indicator
 * - SVG-to-Image conversion with caching
 * - Request Animation Frame for performance
 * - Auto-hide when mouse leaves element
 *
 * Global dependencies:
 * - window.magnifierActive (from globals.js)
 * - window.magnifierCanvas (from globals.js)
 * - window.magnifierContext (from globals.js)
 * - window.magnifierCache (from globals.js)
 * - window.magnifierRAF (from globals.js)
 *
 * DOM:
 * - Creates floating canvas element dynamically
 * - Canvas size: 300-500px (30% viewport, bounded)
 */

/**
 * Toggle magnifier on/off for specified element
 * Manages event handlers and canvas lifecycle
 *
 * @param {string} stringMatchedHash - ID of the image container element
 */
function toggleMagnifier(stringMatchedHash) {
    console.log('[toggleMagnifier] called with stringMatchedHash:', stringMatchedHash);

    var $box = $('#' + stringMatchedHash);
    if ($box.length === 0) {
        console.error('[toggleMagnifier] Element not found with id:', stringMatchedHash);
        return;
    }

    // Toggle magnifier state
    if (window.magnifierActive[stringMatchedHash]) {
        // Disattiva magnifier
        console.log('[toggleMagnifier] Deactivating magnifier');
        window.magnifierActive[stringMatchedHash] = false;

        // Rimuovi event handlers
        $box.off('mousemove.magnifier');
        $box.off('mouseleave.magnifier');

        // Nascondi e rimuovi canvas
        if (window.magnifierCanvas) {
            $(window.magnifierCanvas).remove();
            window.magnifierCanvas = null;
            window.magnifierContext = null;
        }

        // Pulisci la cache per questo elemento
        var svgId = $box.attr('id') || 'svg_' + stringMatchedHash;
        if (window.magnifierCache.has && window.magnifierCache.has(svgId)) {
            window.magnifierCache.delete(svgId);
            console.log('[toggleMagnifier] Cache cleared for:', svgId);
        } else if (window.magnifierCache[svgId]) {
            delete window.magnifierCache[svgId];
            console.log('[toggleMagnifier] Cache cleared for:', svgId);
        }

        // Cancella eventuali animazioni pendenti
        if (window.magnifierRAF) {
            cancelAnimationFrame(window.magnifierRAF);
            window.magnifierRAF = null;
        }
    } else {
        // Attiva magnifier
        console.log('[toggleMagnifier] Activating magnifier');
        window.magnifierActive[stringMatchedHash] = true;

        // Crea canvas per lo zoom
        createMagnifierCanvas();

        // Trova l'immagine o SVG
        var $img = $box.find('img, svg').first();
        if ($img.length === 0) {
            console.error('[toggleMagnifier] No image or SVG found');
            return;
        }

        // Aggiungi event handlers
        $box.on('mousemove.magnifier', function(e) {
            updateMagnifier(e, $box, $img);
        });

        $box.on('mouseleave.magnifier', function() {
            if (window.magnifierCanvas) {
                $(window.magnifierCanvas).hide();
            }
        });
    }
}

/**
 * Create floating canvas element for magnifier
 * Canvas size: min 300px, max 500px, or 30% of viewport width
 */
function createMagnifierCanvas() {
    // Rimuovi canvas esistente se presente
    if (window.magnifierCanvas) {
        $(window.magnifierCanvas).remove();
    }

    // Calcola dimensioni canvas (min 300x300, max 500x500)
    var canvasSize = Math.max(300, Math.min(500, window.innerWidth * 0.3));

    // Crea nuovo canvas
    window.magnifierCanvas = document.createElement('canvas');
    window.magnifierCanvas.width = canvasSize;
    window.magnifierCanvas.height = canvasSize;
    window.magnifierCanvas.style.cssText = `
        position: fixed;
        border: 2px solid #333;
        border-radius: 8px;
        pointer-events: none;
        z-index: 10000;
        display: none;
        box-shadow: 0 0 10px rgba(0,0,0,0.5);
    `;

    document.body.appendChild(window.magnifierCanvas);
    window.magnifierContext = window.magnifierCanvas.getContext('2d');

    console.log('[createMagnifierCanvas] Canvas created with size:', canvasSize);
}

/**
 * Update magnifier view on mouse movement
 * Dispatches to IMG or SVG handler based on element type
 *
 * @param {MouseEvent} e - Mouse event
 * @param {jQuery} $box - Container element
 * @param {jQuery} $img - Image or SVG element
 */
function updateMagnifier(e, $box, $img) {
    if (!window.magnifierCanvas || !window.magnifierContext || !$img[0]) {
        console.log('[updateMagnifier] Missing requirements:', {
            magnifierCanvas: !!window.magnifierCanvas,
            magnifierContext: !!window.magnifierContext,
            img: !!$img[0]
        });
        return;
    }

    var img = $img[0];
    console.log('[updateMagnifier] Image element:', img);
    console.log('[updateMagnifier] Image tagName:', img.tagName);
    console.log('[updateMagnifier] Image src:', img.src);

    // Se è un SVG, gestiscilo diversamente
    if (img.tagName === 'svg' || img.tagName === 'SVG') {
        console.log('[updateMagnifier] Found SVG element, handling zoom for SVG');
        handleSVGMagnifier(e, $box, img);
        return;
    }

    var rect = img.getBoundingClientRect();
    console.log('[updateMagnifier] Image rect:', rect);

    // Calcola posizione relativa del mouse sull'immagine
    var mouseX = e.clientX - rect.left;
    var mouseY = e.clientY - rect.top;

    // Verifica che il mouse sia sopra l'immagine
    if (mouseX < 0 || mouseY < 0 || mouseX > rect.width || mouseY > rect.height) {
        $(window.magnifierCanvas).hide();
        return;
    }

    // Mostra il canvas
    $(window.magnifierCanvas).show();

    // Calcola posizione intelligente
    var canvasPos = calculateSmartPosition(e.clientX, e.clientY, window.magnifierCanvas.width, window.magnifierCanvas.height);
    $(window.magnifierCanvas).css({
        left: canvasPos.left + 'px',
        top: canvasPos.top + 'px'
    });

    // Fattore di zoom
    var zoomFactor = 2.5;

    // Se è un'immagine normale
    if (img.tagName === 'IMG') {
        console.log('[updateMagnifier] Processing IMG element');
        console.log('[updateMagnifier] Image natural dimensions:', img.naturalWidth, 'x', img.naturalHeight);
        console.log('[updateMagnifier] Image complete:', img.complete);

        // Verifica che l'immagine sia caricata
        if (!img.complete || img.naturalWidth === 0) {
            console.log('[updateMagnifier] Image not loaded yet');
            // Prova a ricaricare l'immagine
            img.onload = function() {
                console.log('[updateMagnifier] Image loaded, retrying');
            };
            return;
        }

        // Calcola le coordinate sull'immagine originale
        var naturalX = (mouseX / rect.width) * img.naturalWidth;
        var naturalY = (mouseY / rect.height) * img.naturalHeight;

        // Area da zoomare
        var sourceSize = window.magnifierCanvas.width / zoomFactor;
        var sourceX = naturalX - sourceSize / 2;
        var sourceY = naturalY - sourceSize / 2;

        console.log('[updateMagnifier] Draw parameters:', {
            naturalX: naturalX,
            naturalY: naturalY,
            sourceX: sourceX,
            sourceY: sourceY,
            sourceSize: sourceSize,
            canvasWidth: window.magnifierCanvas.width,
            canvasHeight: window.magnifierCanvas.height
        });

        // Clear canvas
        window.magnifierContext.clearRect(0, 0, window.magnifierCanvas.width, window.magnifierCanvas.height);

        // Riempimento di sfondo per debug
        window.magnifierContext.fillStyle = 'rgba(255, 255, 255, 0.9)';
        window.magnifierContext.fillRect(0, 0, window.magnifierCanvas.width, window.magnifierCanvas.height);

        // Disegna l'immagine zoomata (senza clipping circolare)
        try {
            console.log('[updateMagnifier] Drawing image...');
            window.magnifierContext.drawImage(
                img,
                sourceX, sourceY, sourceSize, sourceSize,
                0, 0, window.magnifierCanvas.width, window.magnifierCanvas.height
            );
            console.log('[updateMagnifier] Image drawn successfully');
        } catch (e) {
            console.error('[updateMagnifier] Error drawing image:', e);
            console.error('[updateMagnifier] Error details:', e.message);
        }

        // Aggiungi crosshair al centro
        window.magnifierContext.strokeStyle = 'rgba(255, 0, 0, 0.5)';
        window.magnifierContext.lineWidth = 1;
        window.magnifierContext.beginPath();
        window.magnifierContext.moveTo(window.magnifierCanvas.width/2 - 10, window.magnifierCanvas.height/2);
        window.magnifierContext.lineTo(window.magnifierCanvas.width/2 + 10, window.magnifierCanvas.height/2);
        window.magnifierContext.moveTo(window.magnifierCanvas.width/2, window.magnifierCanvas.height/2 - 10);
        window.magnifierContext.lineTo(window.magnifierCanvas.width/2, window.magnifierCanvas.height/2 + 10);
        window.magnifierContext.stroke();
    }
    // TODO: Gestire SVG se necessario
}

/**
 * Handle magnifier for SVG elements
 * Converts SVG to Image with caching for performance
 *
 * @param {MouseEvent} e - Mouse event
 * @param {jQuery} $box - Container element
 * @param {SVGElement} svgElement - SVG element to magnify
 */
function handleSVGMagnifier(e, $box, svgElement) {
    if (!window.magnifierCanvas || !window.magnifierContext) return;

    // Cancella eventuali animazioni precedenti
    if (window.magnifierRAF) {
        cancelAnimationFrame(window.magnifierRAF);
    }

    var rect = svgElement.getBoundingClientRect();

    // Calcola posizione relativa del mouse sull'SVG
    var mouseX = e.clientX - rect.left;
    var mouseY = e.clientY - rect.top;

    // Verifica che il mouse sia sopra l'SVG
    if (mouseX < 0 || mouseY < 0 || mouseX > rect.width || mouseY > rect.height) {
        $(window.magnifierCanvas).hide();
        return;
    }

    // Mostra il canvas
    $(window.magnifierCanvas).show();

    // Calcola posizione intelligente
    var canvasPos = calculateSmartPosition(e.clientX, e.clientY, window.magnifierCanvas.width, window.magnifierCanvas.height);
    $(window.magnifierCanvas).css({
        left: canvasPos.left + 'px',
        top: canvasPos.top + 'px'
    });

    // Genera un ID univoco per questo SVG
    var svgId = $box.attr('id') || 'svg_' + Date.now();

    // Controlla se abbiamo già l'immagine in cache
    if (window.magnifierCache[svgId] && window.magnifierCache[svgId].complete) {
        // Usa l'immagine dalla cache
        drawMagnifiedImage(window.magnifierCache[svgId], mouseX, mouseY, rect);
    } else {
        // Se non è in cache, mostra un placeholder mentre si carica
        drawLoadingPlaceholder();

        // Converti SVG solo se non è già in cache
        if (!window.magnifierCache[svgId]) {
            try {
                var data = new XMLSerializer().serializeToString(svgElement);
                var DOMURL = window.URL || window.webkitURL || window;

                var img = new Image();
                var svgBlob = new Blob([data], {type: 'image/svg+xml;charset=utf-8'});
                var url = DOMURL.createObjectURL(svgBlob);

                img.onload = function () {
                    console.log('[handleSVGMagnifier] SVG converted to image and cached');
                    window.magnifierCache[svgId] = img;
                    DOMURL.revokeObjectURL(url);

                    // Disegna l'immagine appena caricata
                    drawMagnifiedImage(img, mouseX, mouseY, rect);
                };

                img.onerror = function() {
                    console.error('[handleSVGMagnifier] Failed to load SVG as image');
                    DOMURL.revokeObjectURL(url);
                };

                img.src = url;

            } catch (e) {
                console.error('[handleSVGMagnifier] Error handling SVG:', e);
                drawErrorMessage();
            }
        }
    }
}

/**
 * Draw magnified image on canvas (optimized with RAF)
 * Used by SVG magnifier after conversion
 *
 * @param {Image} img - Image to draw
 * @param {number} mouseX - Mouse X position relative to image
 * @param {number} mouseY - Mouse Y position relative to image
 * @param {DOMRect} rect - Bounding rect of original element
 */
function drawMagnifiedImage(img, mouseX, mouseY, rect) {
    window.magnifierRAF = requestAnimationFrame(function() {
        // Fattore di zoom
        var zoomFactor = 2.5;

        // Calcola l'area da zoomare
        var sourceSize = window.magnifierCanvas.width / zoomFactor;
        var sourceX = (mouseX / rect.width) * img.width - sourceSize / 2;
        var sourceY = (mouseY / rect.height) * img.height - sourceSize / 2;

        // Clear canvas
        window.magnifierContext.clearRect(0, 0, window.magnifierCanvas.width, window.magnifierCanvas.height);

        // Sfondo bianco
        window.magnifierContext.fillStyle = 'white';
        window.magnifierContext.fillRect(0, 0, window.magnifierCanvas.width, window.magnifierCanvas.height);

        // Disegna l'immagine zoomata (senza clipping circolare)
        window.magnifierContext.drawImage(
            img,
            sourceX, sourceY, sourceSize, sourceSize,
            0, 0, window.magnifierCanvas.width, window.magnifierCanvas.height
        );

        // Aggiungi crosshair
        window.magnifierContext.strokeStyle = 'rgba(255, 0, 0, 0.5)';
        window.magnifierContext.lineWidth = 1;
        window.magnifierContext.beginPath();
        window.magnifierContext.moveTo(window.magnifierCanvas.width/2 - 10, window.magnifierCanvas.height/2);
        window.magnifierContext.lineTo(window.magnifierCanvas.width/2 + 10, window.magnifierCanvas.height/2);
        window.magnifierContext.moveTo(window.magnifierCanvas.width/2, window.magnifierCanvas.height/2 - 10);
        window.magnifierContext.lineTo(window.magnifierCanvas.width/2, window.magnifierCanvas.height/2 + 10);
        window.magnifierContext.stroke();
    });
}

/**
 * Show loading placeholder while SVG converts
 */
function drawLoadingPlaceholder() {
    window.magnifierContext.clearRect(0, 0, window.magnifierCanvas.width, window.magnifierCanvas.height);

    // Sfondo grigio chiaro
    window.magnifierContext.fillStyle = '#f0f0f0';
    window.magnifierContext.fillRect(0, 0, window.magnifierCanvas.width, window.magnifierCanvas.height);

    // Testo di caricamento
    window.magnifierContext.fillStyle = 'black';
    window.magnifierContext.font = '14px Arial';
    window.magnifierContext.textAlign = 'center';
    window.magnifierContext.fillText('Loading...', window.magnifierCanvas.width/2, window.magnifierCanvas.height/2);
}

/**
 * Show error message if SVG conversion fails
 */
function drawErrorMessage() {
    window.magnifierContext.clearRect(0, 0, window.magnifierCanvas.width, window.magnifierCanvas.height);

    // Sfondo bianco
    window.magnifierContext.fillStyle = 'white';
    window.magnifierContext.fillRect(0, 0, window.magnifierCanvas.width, window.magnifierCanvas.height);

    // Messaggio di errore
    window.magnifierContext.fillStyle = 'black';
    window.magnifierContext.font = '14px Arial';
    window.magnifierContext.textAlign = 'center';
    window.magnifierContext.fillText('SVG Zoom', window.magnifierCanvas.width/2, window.magnifierCanvas.height/2 - 20);
    window.magnifierContext.fillText('Not Available', window.magnifierCanvas.width/2, window.magnifierCanvas.height/2 + 20);
}

/**
 * Calculate smart position for magnifier canvas
 * Avoids viewport overflow and cursor occlusion
 *
 * @param {number} mouseX - Mouse X position
 * @param {number} mouseY - Mouse Y position
 * @param {number} canvasWidth - Canvas width
 * @param {number} canvasHeight - Canvas height
 * @returns {{left: number, top: number}} Position for canvas
 */
function calculateSmartPosition(mouseX, mouseY, canvasWidth, canvasHeight) {
    // Margini di sicurezza dai bordi
    var margin = 10;
    var offsetFromCursor = 20; // Distanza dal cursore

    // Dimensioni viewport
    var viewportWidth = window.innerWidth;
    var viewportHeight = window.innerHeight;

    // Posizione di default (a destra del cursore)
    var left = mouseX + offsetFromCursor;
    var top = mouseY - canvasHeight / 2;

    // Controlla overflow a destra
    if (left + canvasWidth + margin > viewportWidth) {
        // Prova a sinistra del cursore
        left = mouseX - canvasWidth - offsetFromCursor;

        // Se anche a sinistra non c'è spazio, posiziona sopra/sotto
        if (left < margin) {
            left = mouseX - canvasWidth / 2;

            // Posiziona sopra il cursore
            if (mouseY > viewportHeight / 2) {
                top = mouseY - canvasHeight - offsetFromCursor;
            } else {
                // Posiziona sotto il cursore
                top = mouseY + offsetFromCursor;
            }
        }
    }

    // Controlla overflow a sinistra
    if (left < margin) {
        left = margin;
    }

    // Controlla overflow in alto
    if (top < margin) {
        top = margin;
    }

    // Controlla overflow in basso
    if (top + canvasHeight + margin > viewportHeight) {
        top = viewportHeight - canvasHeight - margin;
    }

    // Se la lente coprirebbe il cursore, aggiusta la posizione
    var cursorCovered = mouseX >= left && mouseX <= left + canvasWidth &&
                       mouseY >= top && mouseY <= top + canvasHeight;

    if (cursorCovered) {
        // Sposta la lente per non coprire il cursore
        if (mouseX < viewportWidth / 2) {
            // Cursore a sinistra, metti lente a destra
            left = mouseX + offsetFromCursor * 2;
        } else {
            // Cursore a destra, metti lente a sinistra
            left = mouseX - canvasWidth - offsetFromCursor * 2;
        }
    }

    console.log('[calculateSmartPosition] Position calculated:', {
        mouseX: mouseX,
        mouseY: mouseY,
        left: left,
        top: top,
        viewportWidth: viewportWidth,
        viewportHeight: viewportHeight
    });

    return {
        left: Math.round(left),
        top: Math.round(top)
    };
}
