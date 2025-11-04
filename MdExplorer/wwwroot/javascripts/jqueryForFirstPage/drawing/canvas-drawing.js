/**
 * MdExplorer - Canvas Drawing Tool
 * =================================
 * Overlay canvas for annotating/highlighting documents with brush
 *
 * Features:
 * - Full-page canvas overlay for drawing
 * - Color palette with 10 colors
 * - Eraser mode with visual feedback
 * - Adjustable brush size (1-20px)
 * - Zoom-aware coordinate system
 * - Auto-resize with content preservation
 * - Animated GIF indicator when active
 *
 * Global dependencies:
 * - window.toggleCanvas (from globals.js)
 * - window.canvas (from globals.js)
 * - window.ctx (from globals.js)
 * - window.pos (from globals.js)
 * - window.scrollPos (from globals.js)
 * - window.currentColor (from globals.js)
 * - window.isErasing (from globals.js)
 * - window.brushSize (from globals.js)
 */

/**
 * Initialize canvas on page load
 * Creates canvas element, color palette, and event listeners
 */
$(function () {
    if (window.toggleCanvas == 'undefined') {
        window.toggleCanvas = false;
    }

    window.toggleCanvas = !window.toggleCanvas;
    window.canvas = document.createElement('canvas');
    window.canvas.setAttribute('id', 'writeCanvas');
    window.canvas.setAttribute('class', 'canvasForWriting'); // setting z-index to 100
    document.body.appendChild(canvas);

    // Crea la tavolozza colori
    createColorPalette();

    // some hotfixes... ( ≖_≖)
    //document.body.style.margin = 0;
    window.canvas.setAttribute('hidden', 'hidden');
    window.canvas.style.position = 'absolute';  // torniamo ad absolute per seguire il contenuto
    window.canvas.style.top = 0;
    window.canvas.style.left = 0;
    window.canvas.width = document.documentElement.scrollWidth;
    window.canvas.height = document.documentElement.scrollHeight;  // intero documento

    // get canvas 2D context and set him correct size
    window.ctx = canvas.getContext('2d');
    resize();

    // last known position
    window.pos = { x: 0, y: 0 };
    window.scrollPos = { x: 0, y: 0 };

    // Drawing settings
    window.currentColor = '#2bc02d'; // Verde di default
    window.isErasing = false;
    window.brushSize = 5;

    window.addEventListener('resize', resize);
    document.addEventListener('mousemove', draw);
    document.addEventListener('mousedown', setPosition);
    document.addEventListener('mouseenter', setPosition);
    document.addEventListener('scroll', scrollPosition);
});

/**
 * Create floating color palette UI
 * Positioned at bottom-center with 10 colors + eraser + size slider
 */
function createColorPalette() {
    const palette = document.createElement('div');
    palette.id = 'colorPalette';
    palette.style.cssText = `
        position: fixed;
        bottom: 20px;
        left: 50%;
        transform: translateX(-50%);
        background: white;
        border: 2px solid #ccc;
        border-radius: 8px;
        padding: 10px;
        display: none;
        z-index: 101;
        box-shadow: 0 2px 10px rgba(0,0,0,0.2);
    `;

    // Colori disponibili
    const colors = [
        '#2bc02d', // Verde (default)
        '#ff0000', // Rosso
        '#0000ff', // Blu
        '#ffff00', // Giallo
        '#ff00ff', // Magenta
        '#00ffff', // Ciano
        '#000000', // Nero
        '#ffffff', // Bianco (per correzioni)
        '#ffa500', // Arancione
        '#800080'  // Viola
    ];

    // Crea i bottoni colore
    colors.forEach(color => {
        const colorBtn = document.createElement('button');
        colorBtn.style.cssText = `
            width: 30px;
            height: 30px;
            margin: 2px;
            border: 2px solid #ccc;
            cursor: pointer;
            background-color: ${color};
        `;
        colorBtn.onclick = () => selectColor(color);
        palette.appendChild(colorBtn);
    });

    // Separatore
    const separator = document.createElement('div');
    separator.style.cssText = 'width: 100%; height: 1px; background: #ccc; margin: 5px 0;';
    palette.appendChild(separator);

    // Bottone gomma
    const eraserBtn = document.createElement('button');
    eraserBtn.innerHTML = '🧹 Gomma';
    eraserBtn.style.cssText = `
        padding: 5px 10px;
        margin: 2px;
        cursor: pointer;
        background: #f0f0f0;
        border: 2px solid #ccc;
    `;
    eraserBtn.onclick = toggleEraser;
    palette.appendChild(eraserBtn);

    // Selezione dimensione pennello
    const sizeLabel = document.createElement('span');
    sizeLabel.innerHTML = ' Dimensione: ';
    sizeLabel.style.marginLeft = '10px';
    palette.appendChild(sizeLabel);

    const sizeInput = document.createElement('input');
    sizeInput.type = 'range';
    sizeInput.min = '1';
    sizeInput.max = '20';
    sizeInput.value = '5';
    sizeInput.style.width = '80px';
    sizeInput.oninput = (e) => { window.brushSize = parseInt(e.target.value); };
    palette.appendChild(sizeInput);

    document.body.appendChild(palette);
}

/**
 * Select a color for drawing
 * Updates currentColor and provides visual feedback
 *
 * @param {string} color - Hex color code
 */
function selectColor(color) {
    window.currentColor = color;
    window.isErasing = false;
    // Feedback visivo
    document.querySelectorAll('#colorPalette button').forEach(btn => {
        btn.style.border = '2px solid #ccc';
    });
    event.target.style.border = '3px solid #000';
}

/**
 * Toggle eraser mode on/off
 * Changes button appearance and sets isErasing flag
 */
function toggleEraser() {
    window.isErasing = !window.isErasing;
    const eraserBtn = event.target;
    if (window.isErasing) {
        eraserBtn.style.background = '#ffa500';
        eraserBtn.innerHTML = '🧹 Gomma ON';
    } else {
        eraserBtn.style.background = '#f0f0f0';
        eraserBtn.innerHTML = '🧹 Gomma';
    }
}

/**
 * Toggle canvas drawing tool on/off
 * Shows/hides canvas and color palette
 * Changes button icon between animated GIF and static PNG
 *
 * @param {HTMLElement} me - Button element that triggered toggle
 */
function toggleMdCanvas(me) {
    const palette = document.getElementById('colorPalette');
    const buttonDiv = me.parentElement; // Il div con classe mdeLowerBarButton

    if (window.toggleCanvas) {
        me.children[0].src = "/assets/drawAnimated.gif";
        $(window.canvas).removeAttr('hidden');
        window.canvas.style.left = 0;
        palette.style.display = 'block'; // Mostra la tavolozza
        buttonDiv.classList.add('active'); // Aggiungi classe active

        // IMPORTANTE: Ridimensiona il canvas quando lo attiviamo
        // Questo assicura che le dimensioni siano corrette per lo zoom corrente
        resize();

    } else {
        me.children[0].src = "/assets/drawStatic.png";
        window.canvas.setAttribute('hidden', 'hidden');
        palette.style.display = 'none'; // Nascondi la tavolozza
        buttonDiv.classList.remove('active'); // Rimuovi classe active
    }
    window.toggleCanvas = !window.toggleCanvas;
}

/**
 * Update scroll position on scroll event
 * Used for coordinate calculations during drawing
 *
 * @param {Event} e - Scroll event
 */
function scrollPosition(e) {
    scrollPos.x = window.pageXOffset || document.documentElement.scrollLeft;
    scrollPos.y = window.pageYOffset || document.documentElement.scrollTop;
}

/**
 * Set current position from mouse event
 * Accounts for zoom, scroll, and canvas transform
 * Critical for accurate drawing at any zoom level
 *
 * @param {MouseEvent} e - Mouse event
 */
function setPosition(e) {
    // Aggiorna sempre la posizione dello scroll corrente
    scrollPos.x = window.pageXOffset || document.documentElement.scrollLeft;
    scrollPos.y = window.pageYOffset || document.documentElement.scrollTop;

    // Ottieni la posizione del canvas rispetto alla viewport (considera zoom e transform)
    const canvasRect = window.canvas.getBoundingClientRect();

    // Calcola il fattore di scala tra dimensioni CSS e dimensioni interne del canvas
    // Questo è necessario perché il canvas può essere scalato dal browser con lo zoom
    const scaleX = window.canvas.width / canvasRect.width;
    const scaleY = window.canvas.height / canvasRect.height;

    // Calcola coordinate nel sistema CSS (viewport)
    // clientX/Y sono viewport-relative, aggiungere scrollPos per convertire a document-relative
    const cssX = e.clientX + scrollPos.x;
    const cssY = e.clientY + scrollPos.y;

    // Scala le coordinate dal sistema CSS al sistema interno del canvas
    // Questo compensa il fatto che il canvas è scalato dal browser con lo zoom
    pos.x = cssX * scaleX;
    pos.y = cssY * scaleY;
}

/**
 * Resize canvas to match document dimensions
 * Preserves existing content and scales if zoom changed
 * Called on window resize and when activating canvas
 */
function resize() {
    // Salva le vecchie dimensioni
    const oldWidth = window.canvas.width;
    const oldHeight = window.canvas.height;

    // Calcola le nuove dimensioni basate sul documento corrente
    const newWidth = document.documentElement.scrollWidth;
    const newHeight = document.documentElement.scrollHeight;

    // Se le dimensioni sono già corrette, non fare nulla
    if (Math.abs(oldWidth - newWidth) < 2 && Math.abs(oldHeight - newHeight) < 2) {
        return;
    }

    // Salva il contenuto del canvas prima di ridimensionare
    const imageData = window.ctx.getImageData(0, 0, oldWidth, oldHeight);

    // Ridimensiona all'intero documento
    window.ctx.canvas.width = newWidth;
    window.ctx.canvas.height = newHeight;

    // Calcola il fattore di scala se le dimensioni sono cambiate significativamente
    const scaleX = newWidth / oldWidth;
    const scaleY = newHeight / oldHeight;

    // Se lo zoom è cambiato significativamente, scala il contenuto
    if (Math.abs(scaleX - 1.0) > 0.05 || Math.abs(scaleY - 1.0) > 0.05) {
        // Crea un canvas temporaneo per scalare il contenuto
        const tempCanvas = document.createElement('canvas');
        tempCanvas.width = oldWidth;
        tempCanvas.height = oldHeight;
        const tempCtx = tempCanvas.getContext('2d');
        tempCtx.putImageData(imageData, 0, 0);

        // Scala e disegna sul canvas principale
        window.ctx.scale(scaleX, scaleY);
        window.ctx.drawImage(tempCanvas, 0, 0);
        window.ctx.setTransform(1, 0, 0, 1, 0, 0); // Reset transform
    } else {
        // Ripristina il contenuto senza scalare
        window.ctx.putImageData(imageData, 0, 0);
    }
}

/**
 * Draw on canvas based on mouse movement
 * Implements both drawing mode (colored lines) and eraser mode (clear areas)
 * Only draws when left mouse button is pressed (buttons === 1)
 *
 * @param {MouseEvent} e - Mouse event with position and button state
 */
function draw(e) {
    if (!window.toggleCanvas) {
        // mouse left button must be pressed
        if (e.buttons !== 1) return;

        if (window.isErasing) {
            // Modalità gomma - usa clearRect per cancellare
            window.ctx.save();
            window.ctx.globalCompositeOperation = 'destination-out';
            window.ctx.beginPath();
            window.ctx.arc(pos.x, pos.y, window.brushSize * 2, 0, Math.PI * 2);
            window.ctx.fill();
            window.ctx.restore();
            setPosition(e);
        } else {
            // Modalità disegno normale
            window.ctx.beginPath();
            window.ctx.lineWidth = window.brushSize;
            window.ctx.lineCap = 'round';
            window.ctx.strokeStyle = window.currentColor;

            window.ctx.moveTo(pos.x, pos.y);
            setPosition(e);
            window.ctx.lineTo(pos.x, pos.y);

            window.ctx.stroke();
        }
    }
}
