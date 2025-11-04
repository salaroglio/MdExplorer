/**
 * MdExplorer - Scroll Position Management
 * ========================================
 * Saves and restores scroll position per document using localStorage
 *
 * Features:
 * - Restore scroll position on page load
 * - Auto-save on scroll stop (500ms debounce)
 * - Backup save on page unload
 * - URL-based key using hash function
 * - Prevents saving Y=0 (page load/reset)
 *
 * Dependencies:
 * - cyrb53() hash function (from utilities.js)
 */

// funzione che memorizza l'ultima posizione della pagina
$(function () {
    console.log('[Scroll Restore] ========== INIZIO RIPRISTINO ==========');
    try {
        // Ottieni URL senza query parameters e anchor
        var fullUrl = document.location.href;
        console.log('[Scroll Restore] URL completo:', fullUrl);

        var queryIndex = fullUrl.indexOf('?');
        var anchorIndex = fullUrl.indexOf('#');
        console.log('[Scroll Restore] Query index:', queryIndex, 'Anchor index:', anchorIndex);

        // Trova il primo delimitatore (? o #)
        var endIndex = -1;
        if (queryIndex !== -1 && anchorIndex !== -1) {
            endIndex = Math.min(queryIndex, anchorIndex);
        } else if (queryIndex !== -1) {
            endIndex = queryIndex;
        } else if (anchorIndex !== -1) {
            endIndex = anchorIndex;
        }

        // Estrai URL base (senza query/anchor)
        var baseUrl = endIndex !== -1 ? fullUrl.substring(0, endIndex) : fullUrl;
        console.log('[Scroll Restore] URL base:', baseUrl);

        // Genera hash univoco per questo URL
        var urlHash = cyrb53(baseUrl);
        console.log('[Scroll Restore] Hash generato:', urlHash);

        // Recupera posizione salvata (compatibile con vecchio formato)
        var savedPosition = localStorage.getItem(urlHash);
        console.log('[Scroll Restore] Valore localStorage (raw):', savedPosition);

        if (savedPosition) {
            console.log('[Scroll Restore] ✅ Posizione trovata, ripristino...');

            // Determina il formato: numero puro o oggetto JSON
            var scrollY = 0;

            // Prova a parsare come JSON
            try {
                var parsed = JSON.parse(savedPosition);

                // Se è un oggetto con proprietà x/y, è il nuovo formato
                if (typeof parsed === 'object' && parsed !== null) {
                    console.log('[Scroll Restore] Formato JSON oggetto rilevato, ripristino X:', parsed.x, 'Y:', parsed.y);
                    scrollY = parsed.y || 0;
                } else {
                    // È un numero (vecchio formato o JSON.parse di numero)
                    console.log('[Scroll Restore] Formato numero rilevato (parsed):', parsed);
                    scrollY = parsed;
                }
            } catch (e) {
                // Non è un JSON valido, prova come numero puro
                scrollY = parseInt(savedPosition) || 0;
                console.log('[Scroll Restore] Formato stringa numero rilevato:', scrollY);
            }

            console.log('[Scroll Restore] 🎯 Ripristino a Y:', scrollY);
            window.scrollTo({
                left: 0,
                top: scrollY,
                behavior: "instant"
            });

            console.log('[Scroll Restore] ✅ Ripristino completato!');
        } else {
            console.log('[Scroll Restore] ⚠️ Nessuna posizione salvata per questo URL');
        }
    } catch (e) {
        console.error('[Scroll Restore] ❌ Errore nel ripristino posizione:', e);
    }
    console.log('[Scroll Restore] ========== FINE RIPRISTINO ==========');

    // Salva posizione automaticamente quando lo scroll si ferma
    var scrollTimeout;

    console.log('[Scroll Save] 🎯 Attivazione listener scroll su window');

    window.addEventListener('scroll', function() {
        console.log('[Scroll Save] 📜 Evento scroll rilevato! Posizione Y:', window.scrollY);

        // Cancella il timeout precedente
        clearTimeout(scrollTimeout);

        // Imposta un nuovo timeout: salva dopo 500ms di inattività
        scrollTimeout = setTimeout(function() {
            console.log('[Scroll Save] ⏱️ Scroll fermato, inizio salvataggio...');
            try {
                // Ottieni URL base
                var fullUrl = document.location.href;
                var queryIndex = fullUrl.indexOf('?');
                var anchorIndex = fullUrl.indexOf('#');

                var endIndex = -1;
                if (queryIndex !== -1 && anchorIndex !== -1) {
                    endIndex = Math.min(queryIndex, anchorIndex);
                } else if (queryIndex !== -1) {
                    endIndex = queryIndex;
                } else if (anchorIndex !== -1) {
                    endIndex = anchorIndex;
                }

                var baseUrl = endIndex !== -1 ? fullUrl.substring(0, endIndex) : fullUrl;
                var urlHash = cyrb53(baseUrl);

                // Salva posizione corrente (compatibile con vecchio formato: solo Y)
                var scrollY = window.scrollY || window.pageYOffset || 0;
                console.log('[Scroll Save] 💾 Posizione rilevata - ScrollY:', scrollY);

                // NON salvare se siamo a Y=0 (pagina appena caricata o reset automatico)
                if (scrollY > 0) {
                    console.log('[Scroll Save] 💾 Salvataggio - URL:', baseUrl);
                    console.log('[Scroll Save] 💾 Hash:', urlHash);
                    localStorage.setItem(urlHash, scrollY);
                    console.log('[Scroll Save] ✅ Posizione salvata!');
                } else {
                    console.log('[Scroll Save] ⚠️ Posizione Y=0, skip salvataggio (pagina appena caricata)');
                }
            } catch (e) {
                console.error('[Scroll Save] ❌ Errore durante salvataggio:', e);
            }
        }, 500); // Aspetta 500ms dopo l'ultimo movimento di scroll
    }, {passive: true});

    console.log('[Scroll Save] ✅ Listener scroll registrato');
});

// Salvataggio aggiuntivo su onbeforeunload (come backup)
window.onbeforeunload = function (e) {
    try {
        var fullUrl = document.location.href;
        var queryIndex = fullUrl.indexOf('?');
        var anchorIndex = fullUrl.indexOf('#');

        var endIndex = -1;
        if (queryIndex !== -1 && anchorIndex !== -1) {
            endIndex = Math.min(queryIndex, anchorIndex);
        } else if (queryIndex !== -1) {
            endIndex = queryIndex;
        } else if (anchorIndex !== -1) {
            endIndex = anchorIndex;
        }

        var baseUrl = endIndex !== -1 ? fullUrl.substring(0, endIndex) : fullUrl;
        var urlHash = cyrb53(baseUrl);

        // Salva posizione corrente (compatibile con vecchio formato: solo Y)
        var scrollY = window.scrollY || window.pageYOffset || 0;

        // NON salvare se siamo a Y=0 (pagina appena caricata o reset automatico)
        if (scrollY > 0) {
            localStorage.setItem(urlHash, scrollY);
        }
    } catch (e) {
        // Ignora errori silenziosamente
    }
};
