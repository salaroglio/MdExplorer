/**
 * MdExplorer - Navigation History
 * ================================
 * Manages back/forward navigation for internal anchor links
 *
 * Features:
 * - Tracks scroll positions when clicking internal links
 * - Provides back/forward navigation
 * - Updates navigation button states (enabled/disabled)
 *
 * Global dependencies:
 * - window.navigationHistory (from globals.js)
 * - window.currentHistoryIndex (from globals.js)
 * - window.hasNavigationStarted (from globals.js)
 */

/**
 * Initialize internal navigation tracking for anchor links
 * Sets up event delegation to track all internal link clicks
 */
function initializeInternalNavigation() {
    // Usa jQuery per event delegation - cattura anche link aggiunti dinamicamente
    $(document).on('click', 'a[href^="#"]', function(e) {
        // Prevent default navigation behavior (critical for Electron compatibility)
        e.preventDefault();
        e.stopPropagation();

        // Save current scroll position BEFORE jumping
        const currentScrollY = window.scrollY;

        // If navigating from middle of history, trim future entries
        if (window.currentHistoryIndex < window.navigationHistory.length - 1) {
            window.navigationHistory = window.navigationHistory.slice(0, window.currentHistoryIndex + 1);
        }

        // Se lo stack è vuoto, questo è il primo click
        if (window.navigationHistory.length === 0) {
            window.navigationHistory.push({currentScrollY: currentScrollY,typePosition:'link'});
            window.currentHistoryIndex = 0;
            window.hasNavigationStarted = 1; // Primo click effettuato
        } else {
            // Tolgo l'ultimo elemento se si tratta di un back.
            lastPosition = window.navigationHistory.pop();
            console.log(lastPosition);
            if(lastPosition.typePosition === 'link'){
                window.navigationHistory.push(lastPosition);
            }
            // Aggiungi la posizione corrente (da dove parti) allo stack
            window.navigationHistory.push({currentScrollY: currentScrollY,typePosition:'link'});
            window.currentHistoryIndex = window.navigationHistory.length - 1;
            window.hasNavigationStarted++;// = navigationHistory.length - 1;//++; // Incrementa il contatore dei click
        }

        // Aggiorna subito i pulsanti di navigazione
        updateNavigationButtons();

        // Manually scroll to the target anchor element
        const targetId = $(this).attr('href').substring(1); // Remove the '#'
        const targetElement = document.getElementById(targetId);
        if (targetElement) {
            targetElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    });

    // Inizializza subito la navigazione
    const $internalLinks = $('a[href^="#"]');
    const $navButtons = $('.mdeNavButton');

    // I pulsanti sono già visibili dal CSS ma disabilitati
    // Non serve fare show() perché il CSS li mostra già

    // Lo stack parte vuoto, si popola solo con i click sui link

    updateNavigationButtons();
}

/**
 * Navigate back in history
 * Returns to the previous scroll position
 */
function navigateBack() {
    if (window.navigationHistory.length > 0) {
        // Salva la posizione corrente prima di tornare indietro
        const currentScrollY = window.scrollY;

        // Aggiungi la posizione corrente solo se:
        // 1. Siamo all'ultima posizione dello stack (non siamo nel mezzo della storia)
        // 2. La posizione corrente è diversa dall'ultima salvata (non veniamo da un forward)
        if (window.currentHistoryIndex === window.navigationHistory.length - 1) {
            // Controlla se la posizione corrente è diversa dall'ultima nello stack
            const lastPosition = window.navigationHistory[window.navigationHistory.length - 1];
            if (Math.abs(currentScrollY - lastPosition.currentScrollY) > 5) { // Tolleranza di 5px
                window.navigationHistory.push({currentScrollY: currentScrollY,typePosition:'back'});
            }
            window.currentHistoryIndex = window.navigationHistory.length - 1;
        }

        // Vai alla prima posizione (indice 0)
        window.currentHistoryIndex--;// = 0;
        console.log('navigate back before:' + window.hasNavigationStarted);
        // Decrementa hasNavigationStarted quando torniamo indietro
        if (window.hasNavigationStarted > 0) {
            window.hasNavigationStarted--;
        }

        // Scroll to first saved position
        window.scrollTo({
            top: window.navigationHistory[window.currentHistoryIndex].currentScrollY,
            behavior: 'smooth'
        });

        updateNavigationButtons();
    }
}

/**
 * Navigate forward in history
 * Advances to the next scroll position
 */
function navigateForward() {
    if (window.currentHistoryIndex < window.navigationHistory.length - 1) {
        // Move index forward
        window.currentHistoryIndex++;

        // Incrementa hasNavigationStarted quando andiamo avanti
        window.hasNavigationStarted++;

        // Scroll to next position
        window.scrollTo({
            top: window.navigationHistory[window.currentHistoryIndex].currentScrollY,
            behavior: 'smooth'
        });

        updateNavigationButtons();
    }
}

/**
 * Update navigation button states (enabled/disabled)
 * Controls visual feedback and interaction based on navigation state
 */
function updateNavigationButtons() {
    // Debug: mostra lo stato delle variabili

    // Trova i contenitori dei pulsanti (div.mdeNavButton)
    const navBackContainer = document.querySelector('#navBack')?.closest('.mdeNavButton');
    const navForwardContainer = document.querySelector('#navForward')?.closest('.mdeNavButton');

    if (navBackContainer) {
        // Back si disabilita se: mai cliccato OR siamo alla posizione iniziale dopo il primo click
        if ((window.hasNavigationStarted === 0 && window.navigationHistory.length>-1)) {
            navBackContainer.style.opacity = '0.3';
            navBackContainer.style.pointerEvents = 'none';
        } else {
            navBackContainer.style.opacity = '1';
            navBackContainer.style.pointerEvents = 'auto';
        }
    }

    if (navForwardContainer) {
        if ((window.hasNavigationStarted === 0 && window.currentHistoryIndex===-1) || window.hasNavigationStarted >= window.navigationHistory.length - 1) {
            navForwardContainer.style.opacity = '0.3';
            navForwardContainer.style.pointerEvents = 'none';

        } else {
            navForwardContainer.style.opacity = '1';
            navForwardContainer.style.pointerEvents = 'auto';

        }
    }

}

/**
 * Initialize ConnectionId injection for relative links
 * Ensures all relative links include the ConnectionId from the body attribute
 * This is CRITICAL for proper routing on the server side
 */
function initializeConnectionIdForLinks() {
    // Get connectionId from body attribute
    var connectionId = $('body').attr('connectionid');
    if (!connectionId) {
        console.warn('[MdExplorer] ConnectionId not found on body element - relative links may not work correctly');
        return;
    }

    // Intercept clicks on all links that are NOT anchor links and NOT external links
    $(document).on('click', 'a:not([href^="#"])', function(e) {
        var href = $(this).attr('href');

        // Skip if no href
        if (!href) return;

        // Skip external links (http:// or https:// except localhost)
        if (/^https?:\/\/(?!localhost)/i.test(href)) {
            return; // Let the default behavior handle external links
        }

        // Skip javascript: links
        if (/^javascript:/i.test(href)) {
            return;
        }

        // Skip mailto: links
        if (/^mailto:/i.test(href)) {
            return;
        }

        // Skip if connectionId is already in the URL
        if (/[?&]connectionId=/i.test(href)) {
            return;
        }

        // Prevent default navigation
        e.preventDefault();
        e.stopPropagation();

        // Add connectionId to the URL
        var separator = href.indexOf('?') === -1 ? '?' : '&';
        var newHref = href + separator + 'connectionId=' + connectionId;

        console.log('[MdExplorer] Injecting connectionId into link:', href, '->', newHref);

        // Navigate to the new URL
        window.location.href = newHref;
    });

    console.log('[MdExplorer] ConnectionId link injection initialized');
}

/**
 * Initialize P2P link handling for .p2pshare/ links
 * When clicking a P2P link:
 * - Check if file exists locally
 * - If exists, open the file
 * - If not exists, send message to Angular parent to initiate download
 */
function initializeP2PLinkHandling() {
    // Intercept clicks on P2P share links
    $(document).on('click', 'a[href*=".p2pshare/"]', function(e) {
        var href = $(this).attr('href');

        // Skip if no href
        if (!href) return;

        // Prevent default navigation - we'll handle it
        e.preventDefault();
        e.stopPropagation();

        // Extract filename from the path
        var filename = href.split('/').pop();

        // Get project path from body attribute (set by backend)
        var projectPath = $('body').attr('ProjectPath') || '';

        console.log('[MdExplorer P2P] Intercepted P2P link click:', href, 'filename:', filename);

        // Send message to Angular parent to handle the P2P link
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'p2p-link-click',
                href: href,
                filename: filename,
                projectPath: projectPath
            }, '*');
        } else {
            console.warn('[MdExplorer P2P] No parent window found for P2P message');
        }
    });

    // Setup hover/tooltip for P2P links (send message to get status)
    $(document).on('mouseenter', 'a[href*=".p2pshare/"]', function(e) {
        var $link = $(this);
        var href = $link.attr('href');
        var filename = href.split('/').pop();
        var projectPath = $('body').attr('ProjectPath') || '';

        // Add visual indicator that this is a P2P link
        if (!$link.hasClass('p2p-link-styled')) {
            $link.addClass('p2p-link-styled');
            // Small P2P icon or indicator
            $link.attr('title', 'P2P Shared File - Loading status...');
        }

        // Request status from Angular parent
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'p2p-link-hover',
                href: href,
                filename: filename,
                projectPath: projectPath,
                linkId: $link.attr('id') || ('p2p-' + Math.random().toString(36).substr(2, 9))
            }, '*');

            // Store ID for later tooltip update
            if (!$link.attr('id')) {
                $link.attr('id', 'p2p-' + Math.random().toString(36).substr(2, 9));
            }
        }
    });

    // Listen for messages from Angular parent (e.g., tooltip updates)
    window.addEventListener('message', function(event) {
        if (event.data && event.data.type === 'p2p-link-status') {
            var linkId = event.data.linkId;
            var status = event.data.status;
            var $link = $('#' + linkId);

            if ($link.length) {
                // Update tooltip based on status
                var tooltipText = getP2PTooltipText(status);
                $link.attr('title', tooltipText);

                // Update visual styling based on status
                $link.removeClass('p2p-local p2p-seeding p2p-download p2p-downloading p2p-no-peers');
                $link.addClass('p2p-' + status.statusClass);
            }
        }
    });

    console.log('[MdExplorer P2P] P2P link handling initialized');
}

/**
 * Get tooltip text based on P2P status
 */
function getP2PTooltipText(status) {
    if (!status) return 'P2P Shared File';

    switch (status.state) {
        case 'local':
            return 'File disponibile - Click per aprire\nDimensione: ' + formatBytes(status.size);
        case 'seeding':
            return 'In condivisione\nPeer connessi: ' + status.numPeers + '\nUpload: ' + formatSpeed(status.uploadSpeed);
        case 'to_download':
            return 'Click per scaricare\nDimensione: ' + formatBytes(status.size) + '\nPeer disponibili: ' + status.numPeers;
        case 'downloading':
            return 'Download in corso... ' + Math.round(status.progress * 100) + '%\nVelocità: ' + formatSpeed(status.downloadSpeed);
        case 'no_peers':
            return 'Nessun peer disponibile al momento\nRiprova più tardi';
        default:
            return 'P2P Shared File';
    }
}

/**
 * Format bytes to human readable string
 */
function formatBytes(bytes) {
    if (!bytes || bytes === 0) return '0 B';
    var k = 1024;
    var sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    var i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

/**
 * Format speed to human readable string
 */
function formatSpeed(bytesPerSecond) {
    return formatBytes(bytesPerSecond) + '/s';
}

// Initialize navigation when document is ready
$(document).ready(function() {
    initializeInternalNavigation();
    initializeConnectionIdForLinks();
    initializeP2PLinkHandling();
});
