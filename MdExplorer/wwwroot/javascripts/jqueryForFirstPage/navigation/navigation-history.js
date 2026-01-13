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

// Initialize navigation when document is ready
$(document).ready(function() {
    initializeInternalNavigation();
    initializeConnectionIdForLinks();
});
