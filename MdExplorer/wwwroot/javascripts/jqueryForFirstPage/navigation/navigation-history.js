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

        // Skip P2P links - handled by initializeP2PLinkHandling
        if (href.indexOf('.p2pshare/') !== -1) {
            return;
        }

        // Open external links in system browser via backend
        if (/^https?:\/\/(?!localhost)/i.test(href)) {
            e.preventDefault();
            var $body = $("#MdBody");
            $.ajax({
                url: "/api/MdFiles/OpenUrlInBrowser",
                type: "POST",
                data: JSON.stringify({ url: href, connectionId: $body.attr("connectionid") }),
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            });
            return;
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
 * Simple debounce function
 * @param {Function} func - Function to debounce
 * @param {number} wait - Wait time in milliseconds
 * @returns {Function} Debounced function
 */
function debounce(func, wait) {
    var timeout;
    return function() {
        var context = this;
        var args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(function() {
            func.apply(context, args);
        }, wait);
    };
}

// Cache for P2P status to avoid repeated API calls
var p2pStatusCache = {};
var P2P_CACHE_TTL = 30000; // 30 seconds cache

/**
 * Initialize P2P link handling for .p2pshare/ links
 * When clicking a P2P link:
 * - Check if file exists locally
 * - If exists, open the file
 * - If not exists, send message to Angular parent to initiate download
 */
function initializeP2PLinkHandling() {
    console.log('[MdExplorer P2P] Initializing P2P link handling...');

    // Check how many P2P links exist on page load
    var p2pLinks = $('a[href*=".p2pshare/"]');
    console.log('[MdExplorer P2P] Found', p2pLinks.length, 'P2P links on page');

    // Intercept clicks on P2P share links
    $(document).on('click', 'a[href*=".p2pshare/"]', function(e) {
        var href = $(this).attr('href');

        // Skip if no href
        if (!href) return;

        // Prevent default navigation - we'll handle it
        e.preventDefault();
        e.stopPropagation();

        // Extract filename from the path (remove querystring if present)
        var filename = href.split('/').pop().split('?')[0];

        // Get project path from body attribute (set by backend)
        var projectPath = $('body').attr('ProjectPath') || '';

        console.log('[MdExplorer P2P] Click intercepted:', { href: href, filename: filename, projectPath: projectPath });

        // Call check-file API directly to see if file exists locally
        $.ajax({
            url: '/api/P2P/check-file',
            type: 'GET',
            data: { path: href, projectPath: projectPath },
            success: function(result) {
                console.log('[MdExplorer P2P] check-file result:', result);
                if (result.exists && result.fullPath) {
                    // File exists locally - open it directly using existing openApplication function
                    console.log('[MdExplorer P2P] Opening file:', result.fullPath);
                    if (typeof openApplication === 'function') {
                        openApplication(result.fullPath);
                    } else {
                        console.error('[MdExplorer P2P] openApplication function not found');
                    }
                } else {
                    // File doesn't exist - send message to Angular for download handling
                    console.log('[MdExplorer P2P] File not found locally, requesting download...');
                    if (window.parent && window.parent !== window) {
                        window.parent.postMessage({
                            type: 'p2p-link-click',
                            href: href,
                            filename: filename,
                            projectPath: projectPath,
                            needsDownload: true
                        }, '*');
                    }
                }
            },
            error: function(xhr, status, error) {
                console.error('[MdExplorer P2P] Error checking file:', error);
                // Fallback: send to Angular to handle
                if (window.parent && window.parent !== window) {
                    window.parent.postMessage({
                        type: 'p2p-link-click',
                        href: href,
                        filename: filename,
                        projectPath: projectPath
                    }, '*');
                }
            }
        });
    });

    // Debounced function to request P2P status
    var debouncedStatusRequest = debounce(function($link, href, filename, projectPath) {
        var linkId = $link.attr('id');
        var cacheKey = projectPath + '/' + filename;

        // Check cache first
        var cached = p2pStatusCache[cacheKey];
        if (cached && (Date.now() - cached.timestamp) < P2P_CACHE_TTL) {
            // Use cached status
            applyP2PStatus($link, cached.status);
            return;
        }

        // Request status from Angular parent
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'p2p-link-hover',
                href: href,
                filename: filename,
                projectPath: projectPath,
                linkId: linkId
            }, '*');
        }
    }, 300); // 300ms debounce delay

    // Setup hover/tooltip for P2P links (send message to get status)
    $(document).on('mouseenter', 'a[href*=".p2pshare/"]', function(e) {
        var $link = $(this);
        var link = $link[0];
        var href = $link.attr('href');
        var filename = href.split('/').pop().split('?')[0];  // Remove querystring
        var projectPath = $('body').attr('ProjectPath') || '';

        console.log('[MdExplorer P2P] Hover on P2P link:', { href: href, filename: filename, projectPath: projectPath });

        // Ensure link has an ID for status updates
        if (!$link.attr('id')) {
            $link.attr('id', 'p2p-' + Math.random().toString(36).substr(2, 9));
        }

        // Initialize Tippy if not already done
        if (!link._tippy) {
            var cacheKey = projectPath + '/' + filename;
            var cached = p2pStatusCache[cacheKey];
            var initialContent = cached ? getP2PTooltipText(cached.status) : 'P2P: Verifica stato...';
            var initialTheme = cached ? 'p2p-' + cached.status.statusClass : 'p2p-unknown';

            tippy(link, {
                content: initialContent,
                theme: initialTheme,
                placement: 'top',
                arrow: true,
                delay: [200, 0],
                allowHTML: true
            });
        }

        // Request status with debounce
        debouncedStatusRequest($link, href, filename, projectPath);
    });

    // Listen for messages from Angular parent (e.g., tooltip updates)
    window.addEventListener('message', function(event) {
        if (event.data && event.data.type === 'p2p-link-status') {
            console.log('[MdExplorer P2P] Received status from Angular:', event.data);

            var linkId = event.data.linkId;
            var status = event.data.status;
            var $link = $('#' + linkId);

            if ($link.length) {
                console.log('[MdExplorer P2P] Applying status to link:', linkId, status);
                // Get filename for caching (remove querystring if present)
                var href = $link.attr('href');
                var filename = href ? href.split('/').pop().split('?')[0] : '';
                var projectPath = $('body').attr('ProjectPath') || '';
                var cacheKey = projectPath + '/' + filename;

                // Cache the status
                p2pStatusCache[cacheKey] = {
                    status: status,
                    timestamp: Date.now()
                };

                // Apply the status to the link
                applyP2PStatus($link, status);
            }
        }
    });

    console.log('[MdExplorer P2P] P2P link handling initialized');
}

/**
 * Apply P2P status to a link element
 * @param {jQuery} $link - The link element
 * @param {Object} status - Status object with state, statusClass, numPeers, etc.
 */
function applyP2PStatus($link, status) {
    var link = $link[0];
    var tooltipText = getP2PTooltipText(status);
    var theme = 'p2p-' + status.statusClass;

    // Update Tippy tooltip if it exists
    if (link._tippy) {
        link._tippy.setContent(tooltipText);
        link._tippy.setProps({ theme: theme });
    }

    // Update visual styling (icon only, not link color)
    $link.removeClass('p2p-local p2p-seeding p2p-to-download p2p-downloading p2p-no-peers p2p-unknown');
    $link.addClass('p2p-' + status.statusClass);
}

/**
 * Get tooltip text based on P2P status (HTML format for Tippy)
 */
function getP2PTooltipText(status) {
    if (!status) return 'P2P Shared File';

    switch (status.state) {
        case 'local':
            return '<b>File disponibile</b><br>Click per aprire<br><small>' + formatBytes(status.size) + '</small>';
        case 'seeding':
            return '<b>In condivisione</b><br>Peer: ' + status.numPeers + '<br><small>↑ ' + formatSpeed(status.uploadSpeed) + '</small>';
        case 'to_download':
            return '<b>Da scaricare</b><br>Click per avviare<br><small>' + formatBytes(status.size) + ' · ' + status.numPeers + ' peer</small>';
        case 'downloading':
            return '<b>Download ' + Math.round(status.progress * 100) + '%</b><br><small>↓ ' + formatSpeed(status.downloadSpeed) + '</small>';
        case 'no_peers':
            return '<b>Nessun peer</b><br><small>File non disponibile</small>';
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
