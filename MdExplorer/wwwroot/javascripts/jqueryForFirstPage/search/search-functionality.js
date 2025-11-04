/**
 * MdExplorer - Search Functionality
 * ==================================
 * Full-text search with highlighting and navigation
 *
 * Features:
 * - Real-time search as you type (2+ chars)
 * - Yellow highlight for all matches
 * - Orange highlight for current match
 * - Navigate with Enter (next) / Shift+Enter (previous)
 * - Close with Escape or close button
 * - Animated search bar (0.3s ease)
 * - Results counter (e.g., "3 di 15")
 * - Global Ctrl+F shortcut support (Angular + iframe)
 *
 * Global dependencies:
 * - window.searchResults (from globals.js)
 * - window.currentSearchIndex (from globals.js)
 * - window.originalContent (from globals.js)
 *
 * DOM dependencies:
 * - #searchContainer: Search bar container
 * - #searchInput: Text input for search term
 * - #searchButton: Toggle button
 * - #searchResultCount: Result counter display
 * - .mdeItemMainPageCenter: Main content container
 */

/**
 * Toggle search bar visibility
 * Opens with animation and focuses input, closes with animation
 */
function toggleSearch() {
    const searchContainer = document.getElementById('searchContainer');
    const searchInput = document.getElementById('searchInput');
    const searchButton = document.getElementById('searchButton');

    if (searchContainer.style.display === 'none' || searchContainer.style.display === '') {
        // Mostra la barra di ricerca con animazione
        searchContainer.style.display = 'flex';
        searchContainer.style.opacity = '0';
        searchContainer.style.width = '0';

        setTimeout(() => {
            searchContainer.style.transition = 'all 0.3s ease';
            searchContainer.style.opacity = '1';
            searchContainer.style.width = '400px';
        }, 10);

        searchInput.focus();

        // Aggiungi classe attiva al pulsante
        if (searchButton) {
            searchButton.parentElement.classList.add('active');
        }

        // Aggiungi event listener per la ricerca in tempo reale
        searchInput.addEventListener('input', performSearch);
        searchInput.addEventListener('keydown', handleSearchKeydown);
    } else {
        closeSearch();
    }
}

/**
 * Close search bar with animation
 * Clears search results and removes highlights
 */
function closeSearch() {
    const searchContainer = document.getElementById('searchContainer');
    const searchInput = document.getElementById('searchInput');
    const searchButton = document.getElementById('searchButton');

    // Animazione di chiusura
    searchContainer.style.transition = 'all 0.3s ease';
    searchContainer.style.opacity = '0';
    searchContainer.style.width = '0';

    setTimeout(() => {
        searchContainer.style.display = 'none';
        searchInput.value = '';
        clearSearch();
    }, 300);

    // Rimuovi classe attiva dal pulsante
    if (searchButton) {
        searchButton.parentElement.classList.remove('active');
    }
}

/**
 * Execute search and highlight results
 * Called on input event (real-time search)
 * Minimum 2 characters required
 */
function performSearch() {
    const searchInput = document.getElementById('searchInput');
    const searchTerm = searchInput.value.trim();

    if (searchTerm.length === 0) {
        clearSearch();
        return;
    }

    if (searchTerm.length < 2) {
        return; // Non cercare per termini troppo brevi
    }

    // Pulisci risultati precedenti
    clearSearch();

    // Salva il contenuto originale se non già salvato
    const contentElement = document.querySelector('.mdeItemMainPageCenter');
    if (!window.originalContent) {
        window.originalContent = contentElement.innerHTML;
    }

    // Esegui la ricerca e evidenzia
    window.searchResults = [];
    highlightSearchTerm(contentElement, searchTerm);

    // Aggiorna il contatore dei risultati
    updateSearchResultCount();

    // Se ci sono risultati, vai al primo
    if (window.searchResults.length > 0) {
        window.currentSearchIndex = 0;
        scrollToSearchResult(window.currentSearchIndex);
    }
}

/**
 * Highlight search term in content
 * Uses TreeWalker to traverse text nodes only
 * Wraps matches in <mark> elements with yellow background
 *
 * @param {HTMLElement} element - Container to search within
 * @param {string} searchTerm - Term to find and highlight
 */
function highlightSearchTerm(element, searchTerm) {
    const walker = document.createTreeWalker(
        element,
        NodeFilter.SHOW_TEXT,
        {
            acceptNode: function(node) {
                // Ignora nodi dentro script, style, e la barra di ricerca stessa
                const parent = node.parentElement;
                if (parent.tagName === 'SCRIPT' ||
                    parent.tagName === 'STYLE' ||
                    parent.closest('#searchContainer') ||
                    parent.closest('.mdeSearchContainer')) {
                    return NodeFilter.FILTER_REJECT;
                }
                return NodeFilter.FILTER_ACCEPT;
            }
        }
    );

    const nodesToReplace = [];
    let node;

    // Raccogli tutti i nodi di testo che contengono il termine di ricerca
    while (node = walker.nextNode()) {
        const text = node.textContent;
        const regex = new RegExp(escapeRegExp(searchTerm), 'gi');
        if (regex.test(text)) {
            nodesToReplace.push(node);
        }
    }

    // Sostituisci i nodi con versioni evidenziate
    nodesToReplace.forEach(textNode => {
        const text = textNode.textContent;
        const regex = new RegExp(`(${escapeRegExp(searchTerm)})`, 'gi');
        const parts = text.split(regex);

        if (parts.length > 1) {
            const span = document.createElement('span');

            parts.forEach((part, index) => {
                if (index % 2 === 1) { // Parte che corrisponde alla ricerca
                    const highlight = document.createElement('mark');
                    highlight.className = 'mdeSearchHighlight';
                    highlight.style.backgroundColor = '#ffff00';
                    highlight.style.padding = '2px';
                    highlight.style.borderRadius = '2px';
                    highlight.textContent = part;
                    span.appendChild(highlight);
                    window.searchResults.push(highlight);
                } else if (part) { // Testo normale
                    span.appendChild(document.createTextNode(part));
                }
            });

            textNode.parentNode.replaceChild(span, textNode);
        }
    });
}

/**
 * Navigate to next or previous search result
 * Wraps around at boundaries (circular navigation)
 *
 * @param {number} direction - 1 for next, -1 for previous
 */
function navigateSearchResult(direction) {
    if (window.searchResults.length === 0) return;

    // Rimuovi evidenziazione corrente
    if (window.currentSearchIndex >= 0 && window.currentSearchIndex < window.searchResults.length) {
        window.searchResults[window.currentSearchIndex].style.backgroundColor = '#ffff00';
    }

    // Calcola il nuovo indice
    window.currentSearchIndex += direction;

    // Wrap around
    if (window.currentSearchIndex < 0) {
        window.currentSearchIndex = window.searchResults.length - 1;
    } else if (window.currentSearchIndex >= window.searchResults.length) {
        window.currentSearchIndex = 0;
    }

    // Evidenzia e scrolla al risultato corrente
    scrollToSearchResult(window.currentSearchIndex);
}

/**
 * Scroll to specific search result
 * Highlights current result with orange background
 *
 * @param {number} index - Index of result to scroll to
 */
function scrollToSearchResult(index) {
    if (index < 0 || index >= window.searchResults.length) return;

    const result = window.searchResults[index];

    // Evidenzia il risultato corrente con un colore diverso
    result.style.backgroundColor = '#ff9900';

    // Scrolla al risultato
    result.scrollIntoView({
        behavior: 'smooth',
        block: 'center',
        inline: 'nearest'
    });

    // Aggiorna il contatore
    updateSearchResultCount();
}

/**
 * Update search result counter display
 * Shows "X di Y" or "Nessun risultato"
 */
function updateSearchResultCount() {
    const countElement = document.getElementById('searchResultCount');

    if (window.searchResults.length === 0) {
        countElement.textContent = 'Nessun risultato';
        countElement.style.color = '#999';
    } else {
        const currentDisplay = window.currentSearchIndex + 1;
        countElement.textContent = `${currentDisplay} di ${window.searchResults.length}`;
        countElement.style.color = '#333';
    }
}

/**
 * Clear all search highlights and restore original content
 * Resets search state variables
 */
function clearSearch() {
    // Ripristina il contenuto originale se disponibile
    if (window.originalContent) {
        const contentElement = document.querySelector('.mdeItemMainPageCenter');
        contentElement.innerHTML = window.originalContent;
        window.originalContent = null;
    }

    window.searchResults = [];
    window.currentSearchIndex = -1;

    const countElement = document.getElementById('searchResultCount');
    if (countElement) {
        countElement.textContent = '';
    }
}

/**
 * Handle keyboard shortcuts in search input
 * - Enter: Next result
 * - Shift+Enter: Previous result
 * - Escape: Close search
 *
 * @param {KeyboardEvent} event - Keyboard event
 */
function handleSearchKeydown(event) {
    if (event.key === 'Enter') {
        event.preventDefault();
        if (event.shiftKey) {
            navigateSearchResult(-1);
        } else {
            navigateSearchResult(1);
        }
    } else if (event.key === 'Escape') {
        closeSearch();
    }
}

/**
 * Escape special regex characters in string
 * Used for literal string matching in search
 *
 * @param {string} string - String to escape
 * @returns {string} Escaped string safe for RegExp
 */
function escapeRegExp(string) {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

// ============================================================================
// GLOBAL CTRL+F INTEGRATION
// ============================================================================

/**
 * Listen for postMessage from Angular parent to trigger search
 * Allows Angular to activate search even when iframe doesn't have focus
 */
window.addEventListener('message', function(event) {
    console.log('[iframe] Message received:', event.data);
    if (event.data && event.data.action === 'toggleSearch') {
        console.log('[iframe] Triggering search from parent message');
        toggleSearch();
    }
});

/**
 * Listen for Ctrl+F keyboard shortcut within iframe (fallback)
 * Prevents browser default search and triggers custom search instead
 */
document.addEventListener('keydown', function(event) {
    if ((event.ctrlKey || event.metaKey) && event.key === 'f') {
        console.log('[iframe] Ctrl+F detected within iframe, preventing default and triggering search');
        event.preventDefault();
        event.stopPropagation();
        toggleSearch();
    }
});
