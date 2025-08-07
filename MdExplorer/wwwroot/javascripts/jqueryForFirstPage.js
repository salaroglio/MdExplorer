// Create snapshot

function createSnapshot(index) {
    let $camera_flash_Id = $('span[md-camera_flash_id=' + index + ']');
    let fullpath = $camera_flash_Id.attr('md-fullpath');

    let $dialog = $('<div></div>')
        .html('<p>Snapshot name:</p><input type="text" id="myTextbox" style="width: 100%;" />')
        .dialog({
            title: "Take a picture!",
            autoOpen: false,
            modal: true,
            buttons: {
                Ok: function () {
                    var textboxValue = $('#myTextbox').val();
                    $.post("/api/MdFiles/CreateSnapshot", { fullPath: fullpath, versioningdesc: textboxValue },
                        function (data) {
                        });
                    $(this).dialog("close");
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });

    // Open the dialog
    $dialog.dialog('open');
}


//Refs

$(function () {
    window.addEventListener("scroll", function () {
        const scrollX = window.scrollX;
        document.documentElement.style.setProperty("--toc-scroll", Math.round(scrollX) + "px");

    });
});




const cyrb53 = function (str, seed = 0) {
    let h1 = 0xdeadbeef ^ seed, h2 = 0x41c6ce57 ^ seed;
    for (let i = 0, ch; i < str.length; i++) {
        ch = str.charCodeAt(i);
        h1 = Math.imul(h1 ^ ch, 2654435761);
        h2 = Math.imul(h2 ^ ch, 1597334677);
    }
    h1 = Math.imul(h1 ^ (h1 >>> 16), 2246822507) ^ Math.imul(h2 ^ (h2 >>> 13), 3266489909);
    h2 = Math.imul(h2 ^ (h2 >>> 16), 2246822507) ^ Math.imul(h1 ^ (h1 >>> 13), 3266489909);
    return 4294967296 * (2097151 & h2) + (h1 >>> 0);
};


//Open link directly in the application
function openApplication(fullpath) {
    let $body = $("#MdBody");
    let toStringify = { fullPath: fullpath, connectionId: $body.attr("connectionid") };
    $.ajax({
        url: "/api/MdFiles/OpenFileInApplication",
        type: "POST",
        data: JSON.stringify(toStringify),//'{"linkHash": "1234", "cssHash": "5678", "Width": "100px", "Height": "50px","ClientX":"","ClientY":"" }', //
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            console.log(data);
        }
    });

}

currentDocumentSetting = {};

// Navigation history for internal links - stores only scroll positions
let arrayPosition = [];
let currentPositionIndex = -1;

function initializeInternalNavigation() {
    // Check if there are internal links in the document
    const internalLinks = document.querySelectorAll('a[href^="#"]');
    const navBackBtn = document.getElementById('navBack');
    const navForwardBtn = document.getElementById('navForward');
    
    if (internalLinks.length > 0) {
        // Show navigation buttons only if internal links exist
        if (navBackBtn && navBackBtn.parentElement) {
            navBackBtn.parentElement.parentElement.style.display = 'block';
        }
        if (navForwardBtn && navForwardBtn.parentElement) {
            navForwardBtn.parentElement.parentElement.style.display = 'block';
        }
        
        // Add click listener to all internal links
        internalLinks.forEach(link => {
            link.addEventListener('click', function(e) {
                // Save current scroll position BEFORE jumping
                const currentScrollY = window.scrollY;
                
                // If we're navigating from middle of history, remove future positions
                if (currentPositionIndex < arrayPosition.length - 1) {
                    arrayPosition = arrayPosition.slice(0, currentPositionIndex + 1);
                }
                
                // Add current position to array
                arrayPosition.push(currentScrollY);
                currentPositionIndex++;
                
                // Let the browser handle the jump to the anchor
                // Update buttons after a small delay to ensure jump completed
                setTimeout(updateNavigationButtons, 100);
            });
        });
    } else {
        // Hide navigation buttons if no internal links
        if (navBackBtn && navBackBtn.parentElement) {
            navBackBtn.parentElement.parentElement.style.display = 'none';
        }
        if (navForwardBtn && navForwardBtn.parentElement) {
            navForwardBtn.parentElement.parentElement.style.display = 'none';
        }
    }
    
    updateNavigationButtons();
}

function navigateBack() {
    if (currentPositionIndex > 0) {
        // Save current position before going back
        const currentScrollY = window.scrollY;
        
        // Only add to array if we're at the end (not already navigating in history)
        if (currentPositionIndex === arrayPosition.length - 1) {
            arrayPosition.push(currentScrollY);
        } else {
            // Replace the current position in array
            arrayPosition[currentPositionIndex + 1] = currentScrollY;
        }
        
        // Move index back
        currentPositionIndex--;
        
        // Scroll to previous position
        window.scrollTo({ 
            top: arrayPosition[currentPositionIndex], 
            behavior: 'smooth' 
        });
        
        updateNavigationButtons();
    }
}

function navigateForward() {
    if (currentPositionIndex < arrayPosition.length - 1) {
        // Move index forward
        currentPositionIndex++;
        
        // Scroll to next position
        window.scrollTo({ 
            top: arrayPosition[currentPositionIndex], 
            behavior: 'smooth' 
        });
        
        updateNavigationButtons();
    }
}

function updateNavigationButtons() {
    const navBackBtn = document.getElementById('navBack');
    const navForwardBtn = document.getElementById('navForward');
    
    if (navBackBtn && navBackBtn.parentElement) {
        if (currentPositionIndex <= 0) {
            navBackBtn.parentElement.style.opacity = '0.3';
            navBackBtn.parentElement.style.pointerEvents = 'none';
        } else {
            navBackBtn.parentElement.style.opacity = '1';
            navBackBtn.parentElement.style.pointerEvents = 'auto';
        }
    }
    
    if (navForwardBtn && navForwardBtn.parentElement) {
        if (currentPositionIndex >= arrayPosition.length - 1) {
            navForwardBtn.parentElement.style.opacity = '0.3';
            navForwardBtn.parentElement.style.pointerEvents = 'none';
        } else {
            navForwardBtn.parentElement.style.opacity = '1';
            navForwardBtn.parentElement.style.pointerEvents = 'auto';
        }
    }
}

// Initialize navigation when document is ready
$(document).ready(function() {
    setTimeout(function() {
        initializeInternalNavigation();
    }, 500); // Small delay to ensure DOM is fully loaded
});

// gestione tocbot
$(function () {
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
        currentDocumentSetting = documentSetting;

        let $Toc = $('#TOC');
        let $Refs = $("#Refs");
        if (currentDocumentSetting.tocWidth != null && currentDocumentSetting.tocWidth != 0) {
            document.documentElement.style.setProperty("--toc-width", currentDocumentSetting.tocWidth + "px");
            
        }
        if (currentDocumentSetting.refsWidth != null && currentDocumentSetting.refsWidth != 0) {
            document.documentElement.style.setProperty("--refs-width", currentDocumentSetting.refsWidth + "px");
        }

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

// array di tutti gli tooltip con emoji
let tippyDictPriority = {};
let tippyDictProcess = {};
// gestione tippy (tooltip), modo sbrigativo di associarli
$(function () {

    tippyDictPriority = tippy('[id*="Priority"][data-tippy-content]', {
        placement: 'left',
    });
    tippyDictProcess = tippy('[id*="Process"][data-tippy-content]', {
        placement: 'left'
    });

    tippyDictProcess.forEach(_ => {
        let tippyReferenceProcess = _.reference;
        tippyReferenceProcess.setAttribute('data-tippy-process-id', tippyDictProcess.indexOf(_));
        setTippyTypeProcess(tippyReferenceProcess, _);
    });

    tippyDictPriority.forEach(_ => {
        let tippyReferencePriority = _.reference;
        tippyReferencePriority.setAttribute('data-tippy-priority-id', tippyDictPriority.indexOf(_));
        setTippyTypePriority(tippyReferencePriority, _);
    });
});



function setTippyTypePriority(tippyReference, _) {
    if (tippyReference.getAttribute('data-tippy-content') == 'urgente') {
        _.setProps({ theme: 'priorityUrgente' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'annullata') {
        _.setProps({ theme: 'priorityAnnullato' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'fermata') {
        _.setProps({ theme: 'priorityFermata' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'conclusa') {
        _.setProps({ theme: 'priorityConclusa' });
    }

    if (tippyReference.getAttribute('data-tippy-content') == 'dubbio urgente') {
        _.setProps({ theme: 'priorityDaCapireUrgentemente' });
    }

    if (tippyReference.getAttribute('data-tippy-content') == 'da valutare') {
        _.setProps({ theme: 'priorityDaValutare' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'obbligatorio') {
        _.setProps({ theme: 'priorityObbligatorio' });
    }



}


function setTippyTypeProcess(tippyReference, _) {
    if (tippyReference.getAttribute('data-tippy-content') == 'approvato') {
        _.setProps({ theme: 'processok' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'work in progress') {
        _.setProps({ theme: 'processWIP' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'completato') {
        _.setProps({ theme: 'processCompletato' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'Info') {
        _.setProps({ theme: 'processInfo' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'attenzione') {
        _.setProps({ theme: 'processAttenzione' });
    }


}


// Manage Images

// function to manage readability 
var arrayReadabilityToggle = [];
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
    
    var buttonPressed = arrayReadabilityToggle.find(data => data.id == boxId);
    console.log('[toggleSeeMe] buttonPressed:', buttonPressed);
    console.log('[toggleSeeMe] arrayReadabilityToggle:', arrayReadabilityToggle);
    
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
        
        arrayReadabilityToggle.push(dataToStore);
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
        
        var currentIndex = arrayReadabilityToggle.findIndex(data => data.id == boxId);
        arrayReadabilityToggle.splice(currentIndex, 1);
        console.log('[toggleSeeMe] Restored original 100% size');
    }
}

// Magnifier/Zoom functionality
var magnifierActive = {};
var magnifierCanvas = null;
var magnifierContext = null;
var magnifierCache = {}; // Cache per immagini convertite
var magnifierRAF = null; // Request Animation Frame per ottimizzazione

function toggleMagnifier(stringMatchedHash) {
    console.log('[toggleMagnifier] called with stringMatchedHash:', stringMatchedHash);
    
    var $box = $('#' + stringMatchedHash);
    if ($box.length === 0) {
        console.error('[toggleMagnifier] Element not found with id:', stringMatchedHash);
        return;
    }
    
    // Toggle magnifier state
    if (magnifierActive[stringMatchedHash]) {
        // Disattiva magnifier
        console.log('[toggleMagnifier] Deactivating magnifier');
        magnifierActive[stringMatchedHash] = false;
        
        // Rimuovi event handlers
        $box.off('mousemove.magnifier');
        $box.off('mouseleave.magnifier');
        
        // Nascondi e rimuovi canvas
        if (magnifierCanvas) {
            $(magnifierCanvas).remove();
            magnifierCanvas = null;
            magnifierContext = null;
        }
        
        // Pulisci la cache per questo elemento
        var svgId = $box.attr('id') || 'svg_' + stringMatchedHash;
        if (magnifierCache[svgId]) {
            delete magnifierCache[svgId];
            console.log('[toggleMagnifier] Cache cleared for:', svgId);
        }
        
        // Cancella eventuali animazioni pendenti
        if (magnifierRAF) {
            cancelAnimationFrame(magnifierRAF);
            magnifierRAF = null;
        }
    } else {
        // Attiva magnifier
        console.log('[toggleMagnifier] Activating magnifier');
        magnifierActive[stringMatchedHash] = true;
        
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
            if (magnifierCanvas) {
                $(magnifierCanvas).hide();
            }
        });
    }
}

function createMagnifierCanvas() {
    // Rimuovi canvas esistente se presente
    if (magnifierCanvas) {
        $(magnifierCanvas).remove();
    }
    
    // Calcola dimensioni canvas (min 300x300, max 500x500)
    var canvasSize = Math.max(300, Math.min(500, window.innerWidth * 0.3));
    
    // Crea nuovo canvas
    magnifierCanvas = document.createElement('canvas');
    magnifierCanvas.width = canvasSize;
    magnifierCanvas.height = canvasSize;
    magnifierCanvas.style.cssText = `
        position: fixed;
        border: 2px solid #333;
        border-radius: 8px;
        pointer-events: none;
        z-index: 10000;
        display: none;
        box-shadow: 0 0 10px rgba(0,0,0,0.5);
    `;
    
    document.body.appendChild(magnifierCanvas);
    magnifierContext = magnifierCanvas.getContext('2d');
    
    console.log('[createMagnifierCanvas] Canvas created with size:', canvasSize);
}

function updateMagnifier(e, $box, $img) {
    if (!magnifierCanvas || !magnifierContext || !$img[0]) {
        console.log('[updateMagnifier] Missing requirements:', {
            magnifierCanvas: !!magnifierCanvas,
            magnifierContext: !!magnifierContext,
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
        $(magnifierCanvas).hide();
        return;
    }
    
    // Mostra il canvas
    $(magnifierCanvas).show();
    
    // Calcola posizione intelligente
    var canvasPos = calculateSmartPosition(e.clientX, e.clientY, magnifierCanvas.width, magnifierCanvas.height);
    $(magnifierCanvas).css({
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
        var sourceSize = magnifierCanvas.width / zoomFactor;
        var sourceX = naturalX - sourceSize / 2;
        var sourceY = naturalY - sourceSize / 2;
        
        console.log('[updateMagnifier] Draw parameters:', {
            naturalX: naturalX,
            naturalY: naturalY,
            sourceX: sourceX,
            sourceY: sourceY,
            sourceSize: sourceSize,
            canvasWidth: magnifierCanvas.width,
            canvasHeight: magnifierCanvas.height
        });
        
        // Clear canvas
        magnifierContext.clearRect(0, 0, magnifierCanvas.width, magnifierCanvas.height);
        
        // Riempimento di sfondo per debug
        magnifierContext.fillStyle = 'rgba(255, 255, 255, 0.9)';
        magnifierContext.fillRect(0, 0, magnifierCanvas.width, magnifierCanvas.height);
        
        // Disegna l'immagine zoomata (senza clipping circolare)
        try {
            console.log('[updateMagnifier] Drawing image...');
            magnifierContext.drawImage(
                img,
                sourceX, sourceY, sourceSize, sourceSize,
                0, 0, magnifierCanvas.width, magnifierCanvas.height
            );
            console.log('[updateMagnifier] Image drawn successfully');
        } catch (e) {
            console.error('[updateMagnifier] Error drawing image:', e);
            console.error('[updateMagnifier] Error details:', e.message);
        }
        
        // Aggiungi crosshair al centro
        magnifierContext.strokeStyle = 'rgba(255, 0, 0, 0.5)';
        magnifierContext.lineWidth = 1;
        magnifierContext.beginPath();
        magnifierContext.moveTo(magnifierCanvas.width/2 - 10, magnifierCanvas.height/2);
        magnifierContext.lineTo(magnifierCanvas.width/2 + 10, magnifierCanvas.height/2);
        magnifierContext.moveTo(magnifierCanvas.width/2, magnifierCanvas.height/2 - 10);
        magnifierContext.lineTo(magnifierCanvas.width/2, magnifierCanvas.height/2 + 10);
        magnifierContext.stroke();
    }
    // TODO: Gestire SVG se necessario
}

function handleSVGMagnifier(e, $box, svgElement) {
    if (!magnifierCanvas || !magnifierContext) return;
    
    // Cancella eventuali animazioni precedenti
    if (magnifierRAF) {
        cancelAnimationFrame(magnifierRAF);
    }
    
    var rect = svgElement.getBoundingClientRect();
    
    // Calcola posizione relativa del mouse sull'SVG
    var mouseX = e.clientX - rect.left;
    var mouseY = e.clientY - rect.top;
    
    // Verifica che il mouse sia sopra l'SVG
    if (mouseX < 0 || mouseY < 0 || mouseX > rect.width || mouseY > rect.height) {
        $(magnifierCanvas).hide();
        return;
    }
    
    // Mostra il canvas
    $(magnifierCanvas).show();
    
    // Calcola posizione intelligente
    var canvasPos = calculateSmartPosition(e.clientX, e.clientY, magnifierCanvas.width, magnifierCanvas.height);
    $(magnifierCanvas).css({
        left: canvasPos.left + 'px',
        top: canvasPos.top + 'px'
    });
    
    // Genera un ID univoco per questo SVG
    var svgId = $box.attr('id') || 'svg_' + Date.now();
    
    // Controlla se abbiamo già l'immagine in cache
    if (magnifierCache[svgId] && magnifierCache[svgId].complete) {
        // Usa l'immagine dalla cache
        drawMagnifiedImage(magnifierCache[svgId], mouseX, mouseY, rect);
    } else {
        // Se non è in cache, mostra un placeholder mentre si carica
        drawLoadingPlaceholder();
        
        // Converti SVG solo se non è già in cache
        if (!magnifierCache[svgId]) {
            try {
                var data = new XMLSerializer().serializeToString(svgElement);
                var DOMURL = window.URL || window.webkitURL || window;
                
                var img = new Image();
                var svgBlob = new Blob([data], {type: 'image/svg+xml;charset=utf-8'});
                var url = DOMURL.createObjectURL(svgBlob);
                
                img.onload = function () {
                    console.log('[handleSVGMagnifier] SVG converted to image and cached');
                    magnifierCache[svgId] = img;
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

// Funzione ottimizzata per disegnare l'immagine ingrandita
function drawMagnifiedImage(img, mouseX, mouseY, rect) {
    magnifierRAF = requestAnimationFrame(function() {
        // Fattore di zoom
        var zoomFactor = 2.5;
        
        // Calcola l'area da zoomare
        var sourceSize = magnifierCanvas.width / zoomFactor;
        var sourceX = (mouseX / rect.width) * img.width - sourceSize / 2;
        var sourceY = (mouseY / rect.height) * img.height - sourceSize / 2;
        
        // Clear canvas
        magnifierContext.clearRect(0, 0, magnifierCanvas.width, magnifierCanvas.height);
        
        // Sfondo bianco
        magnifierContext.fillStyle = 'white';
        magnifierContext.fillRect(0, 0, magnifierCanvas.width, magnifierCanvas.height);
        
        // Disegna l'immagine zoomata (senza clipping circolare)
        magnifierContext.drawImage(
            img,
            sourceX, sourceY, sourceSize, sourceSize,
            0, 0, magnifierCanvas.width, magnifierCanvas.height
        );
        
        // Aggiungi crosshair
        magnifierContext.strokeStyle = 'rgba(255, 0, 0, 0.5)';
        magnifierContext.lineWidth = 1;
        magnifierContext.beginPath();
        magnifierContext.moveTo(magnifierCanvas.width/2 - 10, magnifierCanvas.height/2);
        magnifierContext.lineTo(magnifierCanvas.width/2 + 10, magnifierCanvas.height/2);
        magnifierContext.moveTo(magnifierCanvas.width/2, magnifierCanvas.height/2 - 10);
        magnifierContext.lineTo(magnifierCanvas.width/2, magnifierCanvas.height/2 + 10);
        magnifierContext.stroke();
    });
}

// Mostra un placeholder durante il caricamento
function drawLoadingPlaceholder() {
    magnifierContext.clearRect(0, 0, magnifierCanvas.width, magnifierCanvas.height);
    
    // Sfondo grigio chiaro
    magnifierContext.fillStyle = '#f0f0f0';
    magnifierContext.fillRect(0, 0, magnifierCanvas.width, magnifierCanvas.height);
    
    // Testo di caricamento
    magnifierContext.fillStyle = 'black';
    magnifierContext.font = '14px Arial';
    magnifierContext.textAlign = 'center';
    magnifierContext.fillText('Loading...', magnifierCanvas.width/2, magnifierCanvas.height/2);
}

// Mostra messaggio di errore
function drawErrorMessage() {
    magnifierContext.clearRect(0, 0, magnifierCanvas.width, magnifierCanvas.height);
    
    // Sfondo bianco
    magnifierContext.fillStyle = 'white';
    magnifierContext.fillRect(0, 0, magnifierCanvas.width, magnifierCanvas.height);
    
    // Messaggio di errore
    magnifierContext.fillStyle = 'black';
    magnifierContext.font = '14px Arial';
    magnifierContext.textAlign = 'center';
    magnifierContext.fillText('SVG Zoom', magnifierCanvas.width/2, magnifierCanvas.height/2 - 20);
    magnifierContext.fillText('Not Available', magnifierCanvas.width/2, magnifierCanvas.height/2 + 20);
}

// Calcola posizione intelligente per evitare overflow della lente
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

// function to show/hide image's toolbar

function showImageToolbar(referenceId) {
    var $element = $('#' + referenceId);
    var divStyle = getComputedStyle($element[0]);
    var rect = $element[0].getBoundingClientRect();
    var test = rect.top;
    $element.attr("style", "display:block; position:absolute;");
}

function hideImageToolbar(referenceId) {
    var $element = $('#' + referenceId);
    $element.attr("style", "display:none;");
}


//Function Called by button to activate Move!
var arrayLinksMoveToggle = [];
var moving = false;
var image;

function activateMove(currentObject, linkHash, referenceId) {
    var toSend = currentObject.parentElement.parentElement;
    $movable = $(toSend);
    var buttonPressed = arrayLinksMoveToggle.find(data => data == linkHash);
    if (buttonPressed == undefined) {
        var newClass = $movable.attr('class', 'movable');
        arrayLinksMoveToggle.push(linkHash);
    } else {

        var currentIndex = arrayLinksMoveToggle.findIndex(data => data == linkHash);
        arrayLinksMoveToggle.splice(currentIndex, 1);
        var possibleMatch = currentObject.parentElement.nextSibling;
        resizeImage(possibleMatch);
        $movable.attr('class', 'movedAndFixed');

    }

    initialClick(toSend, referenceId);
}

function initialClick(currentObject, referenceId) {

    if (moving) {
        document.removeEventListener("mousemove", move);
        moving = !moving;
        return;
    }

    moving = !moving;
    image = currentObject;

    document.addEventListener("mousemove", move, false);

}

function move(e) {

    var newX = e.clientX - 76;
    var newY = e.clientY - 18;

    image.style.left = newX + "px";
    image.style.top = newY + "px";


}


// function called by button to activate resize
var arrayLinksResizeToggle = [];
function activateResize(linkHash) {
    // Find nodes    
    var buttonPressed = arrayLinksResizeToggle.find(data => data == linkHash);
    var $links = $('div[md-link-hash=' + linkHash + ']'); // shold exist only one link. I'm using each() because i'm lazy :-)
    $links.each(function (index) {
        if (buttonPressed == undefined) {
            var oldValue = $links[index].attributes['class'].value;
            $links[index].attributes['class'].value = oldValue + ' resizable';
            arrayLinksResizeToggle.push(linkHash);
        }
        else {


            var oldValue = $links[index].attributes['class'].value;
            $links[index].attributes['class'].value = oldValue.replace(' resizable', '');
            var currentIndex = arrayLinksResizeToggle.findIndex(data => data == linkHash);
            arrayLinksResizeToggle.splice(currentIndex, 1);

        }

    });
}

var cumulativeOffset = function (element) {
    var top = 0, left = 0;
    do {
        top += element.offsetTop || 0;
        left += element.offsetLeft || 0;
        element = element.offsetParent;
    } while (element);

    return {
        top: top,
        left: left
    };
};



// Function called by onMouseUp event to 
// write down on MD the new image dimension values.
function resizeImage(currentDiv) {

    // going inside the div
    var img = currentDiv.childNodes[0].childNodes[0];
    var divStyle = getComputedStyle(img.parentElement.parentElement.parentElement);
    var position = divStyle.position;// == "" ? "none" : img.style.position;
    // getting infos from attributes

    var currentHash = currentDiv.attributes['md-css-hash'].value;
    var pathFile = currentDiv.attributes['md-path-file'].value;
    var linkHash = currentDiv.attributes['md-link-hash'].value;
    var CurrentQueryRequest = currentDiv.attributes['md-CurrentQueryRequest'].value;

    var currentImageData = {
        pathFile: pathFile,
        linkHash: linkHash,
        cssHash: currentHash,
        Width: currentDiv.clientWidth,
        Height: currentDiv.scrollHeight,
        ClientX: cumulativeOffset(currentDiv).left,
        ClientY: cumulativeOffset(currentDiv).top,
        Position: position,
        CurrentQueryRequest: CurrentQueryRequest
    };
    $.ajax({
        url: "/api/WriteMD/SaveImgPositionAndSize",
        type: "POST",
        data: JSON.stringify(currentImageData),//'{"linkHash": "1234", "cssHash": "5678", "Width": "100px", "Height": "50px","ClientX":"","ClientY":"" }', //
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            currentDiv.attributes['md-css-hash'].value = data.cssHash;
            var $divs = $("div[md-css-hash='" + currentHash + "']");
            $divs.each(function (index) {
                $divs[index].attributes['md-css-hash'].value = data.cssHash;
            });
            //.attributes['md-css-hash'].value = data.cssHash
        }
    });
}



// gestione del floppy_disk e salvataggio
function activateSaveCopy(el, path) {
    $.get("/api/WriteMD/ActivateSaveCopy?pathFile=" + path, function () {
        alert('done');
    });
}

// gestione del sortable dentro le icone di priorità
$(function () {
    $(".sortable").sortable();
    $(".sortable").disableSelection();

    $(".sortable").on("sortstop", function (event, ui) {
        // controllo che l'item selezionato abbia il simbolo di priorità
        var previousNodeIndex = null;
        var nextNodeIndex = null;
        var currentNodeIndex = null;

        // nodo corrente
        currentNode = ui.item[0].childNodes[0];
        currentNodeIndex = currentNode.attributes['data-md-card-index'].value;
        currentTableGameIndex = currentNode.attributes['data-md-table-game-index'].value;

        // nodo precedente
        if (ui.item[0].previousElementSibling != null) {
            previousNode = ui.item[0].previousElementSibling.childNodes[0];
            previousNodeIndex = previousNode.attributes['data-md-card-index'].value;
        }
        // nodo successivo
        if (ui.item[0].nextElementSibling != null) {
            nextNode = ui.item[0].nextSibling.childNodes[0];
            nextNodeIndex = nextNode.attributes['data-md-card-index'].value;
        }

        var currentPathFile = ui.item[0].childNodes[0].attributes['data-md-pathfile'].value;
        var queryPreviousNodeIndex = previousNodeIndex == null ? '' : previousNodeIndex;
        var queryNextNodeIndex = nextNodeIndex == null ? '' : nextNodeIndex;
        $.get("/api/WriteMD/SetEmojiOrderPriority?currentNodeIndex=" + currentNodeIndex +
            "&previousNodeIndex=" + queryPreviousNodeIndex +
            "&nextNodeIndex=" + queryNextNodeIndex +
            "&pathFile=" + currentPathFile +
            "&tableGameIndex=" + currentTableGameIndex,
            function (data) {
                currentNode.attributes['data-md-card-index'].value = data.currentNodeIndex;
                if (data.previousNodeIndex != null) {
                    previousNode.attributes['data-md-card-index'].value = data.previousNodeIndex;
                }
                if (data.nextNodeIndex != null) {
                    nextNode.attributes['data-md-card-index'].value = data.nextNodeIndex;
                }
                // reconstruct process IDs
                var listProcess = $("span[id*='emojiProcess']");
                listProcess.each(function (index, element) {
                    element.attributes['data-md-process-index'].value = index;// listProcess.index(element);
                });
                var listPriority = $("span[id*='mojiPriority']");
                listPriority.each(function (index, element) {
                    element.attributes['data-md-priority-index'].value = index;// listProcess.index(element);
                });
                // Reconstruct index of table-card into table-game, foreach table-game
                var iTable = 0;
                var listPriorityItableGame = $("span[id*='mojiPriority'][data-md-table-game-index=" + iTable.toString() + "]");
                while (listPriorityItableGame.length > 0) {
                    listPriorityItableGame.each(function (index, element) {
                        element.attributes['data-md-card-index'].value = index;// listProcess.index(element);
                    });
                    iTable++;
                    var listPriorityItableGame = $("span[id*='mojiPriority'][data-md-table-game-index=" + iTable.toString() + "]");
                }




                console.log(data);
            });
        console.log('sortstop parents Event = ', event, '  ui = ', ui);
        console.log(ui.item);
        tocbot.refresh();
        //do sort of parents
    });
});
// serve per impostare il date-picker di boostrap ed evitare quello di jquery-ui
$.fn.datepicker.noConflict = function () {
    $.fn.datepicker = old;
    return this;
};

// funzione che memorizza l'ultima posizione della pagina
document.addEventListener("DOMContentLoaded", function (event) {
    // Memorizza la posizione corrente della pagina,perché sia riproposta dopo un refresh    
    var test3 = document.location.href;    
    var position = test3.indexOf('?');
    var position2 = test3.substring(0, position);
    var test2 = cyrb53(position2);
    var scrollpos1 = localStorage.getItem(test2);
    if (scrollpos1) window.scrollTo({ left: 0, top: scrollpos1, behavior: "instant" });

});

// gestione ultima posizione dello scroll
window.onbeforeunload = function (e) {
    var test3 = document.location.href;    
    var position = test3.indexOf('?');
    var position2 = test3.substring(0, position);
    var test2 = cyrb53(position2);
    localStorage.setItem(test2, window.scrollY);
};

// gestione dell'emoji :calendar:
function activateCalendar(el, index, target, dateformat, pathfile) {

    var data = $('#' + el.id).data('datepicker');
    if (data) { // esiste già il datepicker, lo uso        
        $('#' + el.id).datepicker('show');
    } else { // il datepicker non esiste, lo creo e lo inizializzo
        var currentDatePicker = $('#' + el.id).datepicker({
            format: dateformat, //'dd-mm-yyyy'
            todayHighlight: true,
            todayBtn: "linked"
        });

        currentDatePicker.on('changeDate', function (ev) {
            var toReplace = $('#' + el.id).data('datepicker').getFormattedDate(dateformat);
            $('#' + target).text(toReplace);
            currentDatePicker.datepicker('hide');
            $.get("/api/WriteMD/SetCalendar?index=" + index + "&pathFile=" + pathfile + "&toReplace=" + toReplace, function (data) {
                $(".result").html(data);
                console.log(data);
            });
        });
        currentDatePicker.datepicker('show');
    }
}



// gestione degli emoji di processo
function dynamicEmojiForProcess(el, index, pathfile) {



    let dataToSet;
    el.removeAttribute('data-tippy-content');
    if (el.innerText.trim() == 'ℹ️') {
        el.innerText = '🆗';
        el.setAttribute('data-tippy-content', 'approvato');
        dataToSet = 'approvato';
    } else
        if (el.innerText.trim() == '🆗') {
            el.innerText = '⚠️';
            el.setAttribute('data-tippy-content', 'attenzione');
            dataToSet = 'attenzione';

        } else
            if (el.innerText.trim() == '⚠️') {
                el.innerText = '🚧';
                el.setAttribute('data-tippy-content', 'work in progress');
                dataToSet = 'work in progress';
            } else
                if (el.innerText.trim() == '🚧') {
                    el.innerText = '✔️';
                    el.setAttribute('data-tippy-content', 'completato');
                    dataToSet = 'completato';
                } else
                    if (el.innerText.trim() == '✔️') {
                        el.innerText = 'ℹ️';
                        el.setAttribute('data-tippy-content', 'in valutazione');
                        dataToSet = 'Info';
                    }
    var currentIndex = el.attributes['data-md-process-index'].value;
    $.get("/api/WriteMD/SetEmojiProcess?index=" + currentIndex + "&pathFile=" + pathfile + "&toReplace=" + el.innerText.trim(), function (data) {
        $(".result").html(data);
        var oldData$ = $('div.hiddendataforeditorh1');
        for (i = 0; i < oldData$.length; i++) {
            let myData$ = $(oldData$.get(i));
            let check = myData$.attr("md-itemmatchindex");
            myData$.attr("md-itemmatchindex", data[i].itemMatchIndex);
        }

    });

    setTooltipProcess(dataToSet, el);
    tocbot.refresh();
}

function setTooltipPriority(dataToSet, el) {

    let $el = $(el);
    let attributeValue = $el.attr("data-tippy-priority-id");
    let currentPriority = tippyDictPriority[attributeValue]; //el.attributes[8].value
    currentPriority.setContent(dataToSet);
    currentPriority.reference.setAttribute('data-tippy-content', dataToSet)
    setTippyTypePriority(currentPriority.reference, currentPriority);
    currentPriority.show();
}

function setTooltipProcess(dataToSet, el) {
    let $el = $(el);
    let attributeValue = $el.attr("data-tippy-process-id");
    let current = tippyDictProcess[attributeValue]; //data-tippy-process-id //el.attributes[4].value
    current.setContent(dataToSet);
    current.reference.setAttribute('data-tippy-content', dataToSet)
    setTippyTypeProcess(current.reference, current);
    current.show();
}


// gestione degli emoji di priorità
function dynamicEmojiForPriority(el, index, pathfile) {

    if (el.innerText == '❓') {
        el.innerText = '❔';
        //el.title = 'da valutare';
        dataToSet = 'da valutare';
    } else
        if (el.innerText == '❔') {
            el.innerText = '❕';
            //el.title = 'obbligatorio';
            dataToSet = 'obbligatorio';

        } else
            if (el.innerText == '❕') {
                el.innerText = '❗';
                //el.title = 'urgente';
                dataToSet = 'urgente';
            } else
                if (el.innerText == '❗') {
                    el.innerText = '❌';
                    //el.title = 'annullata';
                    dataToSet = 'annullata';
                } else
                    if (el.innerText == '❌') {
                        el.innerText = '⛔';
                        //el.title = 'fermata';
                        dataToSet = 'fermata';
                        var element = $('#' + el.id).parent();
                    } else
                        if (el.innerText == '⛔') {
                            el.innerText = '❎';
                            //el.title = 'conclusa';
                            dataToSet = 'conclusa';
                            var element = $('#' + el.id).parent();
                            var check = element.parent().is('li');
                            if (check) {
                                element = element.parent();
                            }
                            //element.fadeOut(3000);
                        } else

                            if (el.innerText == '❎') {
                                el.innerText = '❓';
                                //el.title = 'da valutare';
                                dataToSet = 'dubbio urgente';
                                var element = $('#' + el.id).parent();
                                var check = element.is('li');
                                if (!check) {
                                    element = element.parent();
                                }
                                element.stop();
                                element.fadeIn();
                            }

    var currentIndex = el.attributes['data-md-priority-index'].value;
    $.get("/api/WriteMD/SetEmojiPriority?index=" + currentIndex + "&pathFile=" + pathfile + "&toReplace=" + el.innerText, function (data) {
        $(".result").html(data);

        var oldData$ = $('div.hiddendataforeditorh1');
        for (i = 0; i < oldData$.length; i++) {
            let myData$ = $(oldData$.get(i));
            let check = myData$.attr("md-itemmatchindex");
            myData$.attr("md-itemmatchindex", data[i].itemMatchIndex);
        }


    });

    setTooltipPriority(dataToSet, el);
    tocbot.refresh();
}

// gestione del box di ricerca della toc
//function filterToc() {
//    // Declare variables
//    var input, filter, ul, li, a, i, txtValue;
//    input = document.getElementById('tocInputFilter');
//    filter = input.value.toUpperCase();
//    ul = document.getElementById("ulToc");
//    li = ul.getElementsByTagName('li');

//    // Loop through all list items, and hide those who don't match the search query
//    for (i = 0; i < li.length; i++) {
//        a = li[i].getElementsByTagName("a")[0];
//        txtValue = a.textContent || a.innerText;
//        if (txtValue.toUpperCase().indexOf(filter) > -1) {
//            li[i].style.display = "";
//        } else {
//            li[i].style.display = "none";
//        }
//    }
//}

function toggleTOC(documentPath) {

    let $refs = $('#Refs');
    let $toc = $('#TOC');

    if ($('#Refs').is(":hidden") && $('#TOC').is(":hidden")) {
        $toc.fadeIn();
        currentDocumentSetting.showTOC = true;
        currentDocumentSetting.showRefs = false;

    } else if ($('#Refs').is(":hidden") && !$('#TOC').is(":hidden")) {
        $toc.fadeOut();
        currentDocumentSetting.showTOC = false;
        currentDocumentSetting.showRefs = false;
    } else if (!$('#Refs').is(":hidden") && $('#TOC').is(":hidden")) {
        $toc.fadeIn();
        $refs.fadeOut();
        currentDocumentSetting.showTOC = true;
        currentDocumentSetting.showRefs = false;
    } else if (!$('#Refs').is(":hidden") && !$('#TOC').is(":hidden")) {
        $toc.fadeIn();
        $refs.fadeOut();
        currentDocumentSetting.showTOC = true;
        currentDocumentSetting.showRefs = false;
    }

    $.ajax({
        url: "/api/tabcontroller/SaveTOCData",
        type: "POST",
        data: JSON.stringify(currentDocumentSetting),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {

            console.log(data);
        }
    });

}


function toggleReferences() {
    let $refs = $('#Refs');
    let $toc = $('#TOC');

    if ($('#Refs').is(":hidden") && $('#TOC').is(":hidden")) {
        $refs.fadeIn();
        currentDocumentSetting.showTOC = false;
        currentDocumentSetting.showRefs = true;

    } else if ($('#Refs').is(":hidden") && !$('#TOC').is(":hidden")) {
        $refs.fadeIn();
        $toc.fadeOut();
        currentDocumentSetting.showTOC = false;
        currentDocumentSetting.showRefs = true;
    } else if (!$('#Refs').is(":hidden") && $('#TOC').is(":hidden")) {
        $refs.fadeOut();
        currentDocumentSetting.showTOC = false;
        currentDocumentSetting.showRefs = false;
    } else if (!$('#Refs').is(":hidden") && !$('#TOC').is(":hidden")) {
        $toc.fadeOut();
        $refs.fadeIn();
        currentDocumentSetting.showTOC = false;
        currentDocumentSetting.showRefs = true;
    }

    $.ajax({
        url: "/api/tabcontroller/SaveTOCData",
        type: "POST",
        data: JSON.stringify(currentDocumentSetting),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {

            console.log(data);
        }
    });
}



// inizializzazione, al caricamnto della pagina,
// del canvas, tela per la matitina, fuori dal campo visivo dell'utente
$(function () {
    console.log("Create canvas");
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

// Crea la tavolozza colori
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

// Seleziona un colore
function selectColor(color) {
    window.currentColor = color;
    window.isErasing = false;
    // Feedback visivo
    document.querySelectorAll('#colorPalette button').forEach(btn => {
        btn.style.border = '2px solid #ccc';
    });
    event.target.style.border = '3px solid #000';
}

// Toggle modalità gomma
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

// gestione della matitina per evidenziare la pagina
function toggleMdCanvas(me) {
    const palette = document.getElementById('colorPalette');
    const buttonDiv = me.parentElement; // Il div con classe mdeLowerBarButton
    
    if (window.toggleCanvas) {
        me.children[0].src = "/assets/drawAnimated.gif";
        $(window.canvas).removeAttr('hidden');
        window.canvas.style.left = 0;
        palette.style.display = 'block'; // Mostra la tavolozza
        buttonDiv.classList.add('active'); // Aggiungi classe active

    } else {
        me.children[0].src = "/assets/drawStatic.png";
        window.canvas.setAttribute('hidden', 'hidden');
        palette.style.display = 'none'; // Nascondi la tavolozza
        buttonDiv.classList.remove('active'); // Rimuovi classe active
    }
    window.toggleCanvas = !window.toggleCanvas;
}

function scrollPosition(e) {
    scrollPos.x = window.pageXOffset || document.documentElement.scrollLeft;
    scrollPos.y = window.pageYOffset || document.documentElement.scrollTop;
}
// new position from mouse event
function setPosition(e) {
    // Aggiorna sempre la posizione dello scroll corrente
    scrollPos.x = window.pageXOffset || document.documentElement.scrollLeft;
    scrollPos.y = window.pageYOffset || document.documentElement.scrollTop;
    
    // Calcoliamo l'offset del canvas
    const canvasRect = window.canvas.getBoundingClientRect();
    
    // Posizione relativa al documento
    // clientX/Y sono relative alla viewport, quindi aggiungiamo lo scroll
    pos.x = e.clientX + scrollPos.x;
    pos.y = e.clientY + scrollPos.y;
}

// resize canvas
function resize() {
    // Salva il contenuto del canvas prima di ridimensionare
    const imageData = window.ctx.getImageData(0, 0, window.canvas.width, window.canvas.height);
    
    // Ridimensiona all'intero documento
    window.ctx.canvas.width = document.documentElement.scrollWidth;
    window.ctx.canvas.height = document.documentElement.scrollHeight;
    
    // Ripristina il contenuto
    window.ctx.putImageData(imageData, 0, 0);
}


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

//Gestione Clipboard *************
async function copyToClipboard(objectThis, relativePathFile, hashFile, step) {
    const test = await $.get("/api/plantumlextensions/GetPng?pathFile=" + relativePathFile +
        "&hashFile=" + hashFile +
        "&step=" + step, function (data) {
            console.log(data);
        });
    const response = await fetch(objectThis);  //'/assets/ConnectionLost.png'
    const blob = await response.blob();
    setToClipboard(blob);
}

const setToClipboard = async blob => {
    const data = [new ClipboardItem({ [blob.type]: blob })];
    await navigator.clipboard.write(data);
}
// *******************************

//Gestione presentazione Plantuml

async function presentationSVG(relativePathFile, hashFile) {
    var $forwardArrow = $('#forwardArrow' + hashFile);
    var trueStep = parseInt($forwardArrow.attr("data-step"));
    const result = await $.get("/api/plantumlextensions/PresentationSVG?pathFile=" + relativePathFile +
        "&hashFile=" + hashFile +
        "&step=" + trueStep);

    var totalStep = result.totalStep;
    const response = await fetch(result.generatedFileName);
    const text = await response.text();
    var nodeSvg = $.parseHTML(text);
    var $parent = $('#' + hashFile);
    var mySvg = $parent.find('svg'); // svg
    //var mySvg = childrens[0];
    mySvg.remove();

    $parent.append(nodeSvg);
    var $forwardArrow = $('#forwardArrow' + hashFile);
    trueStep = trueStep + 1;
    if (trueStep >= totalStep) {
        trueStep = 0;
    }
    $forwardArrow.attr('data-step', trueStep);
}

//resizeToc
hookedToc = false;
hookedRefs = false;
function resizeToc() {

    hookedToc = true;
}
function resizeRefs() {

    hookedRefs = true;
}


$(function () {
    document.addEventListener("mousemove", mouseMoveEvent, false);
    document.addEventListener("mouseup", mouseUpEvent, false);
});

function mouseUpEvent(event) {

    let toc$ = $('#TOC');
    let refs$ = $('#Refs');

    if (hookedToc) {
        hookedToc = false;
        let value = parseInt(event.clientX) + 30;
        currentDocumentSetting.tocWidth = parseInt(toc$.css("width").substring(0, toc$.css("width").length - 2));
        $.ajax({
            url: "/api/tabcontroller/SaveTOCData",
            type: "POST",
            data: JSON.stringify(currentDocumentSetting),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {

                console.log(data);
            }
        });

    }
    if (hookedRefs) {
        hookedRefs = false;
        let value = parseInt(event.clientX) + 30;
        
        currentDocumentSetting.refsWidth = parseInt(refs$.css("width").substring(0, refs$.css("width").length - 2));
        $.ajax({
            url: "/api/tabcontroller/SaveRefsData",
            type: "POST",
            data: JSON.stringify(currentDocumentSetting),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {

                console.log(data);
            }
        });

    }
}

function mouseMoveEvent(event) {

    let toc$ = $('#TOC');
    let refs$ = $('Refs');
    if (hookedToc) {
        let value = document.documentElement.scrollWidth - parseInt(event.clientX) - 30;
        let scrolldata = document.documentElement.scrollWidth - document.documentElement.clientWidth;
        console.log(scrolldata);
        value = value - scrolldata;
        document.documentElement.style.setProperty("--toc-width", value + "px");

    }
    if (hookedRefs) {
        let value = document.documentElement.scrollWidth - parseInt(event.clientX) - 30;
        let scrolldata = document.documentElement.scrollWidth - document.documentElement.clientWidth;
        console.log(scrolldata);
        value = value - scrolldata;
        document.documentElement.style.setProperty("--refs-width", value + "px");

    }
}
