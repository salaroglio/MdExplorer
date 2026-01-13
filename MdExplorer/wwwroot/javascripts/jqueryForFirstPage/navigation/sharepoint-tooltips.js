/**
 * SharePoint Link Tooltips
 * Mostra l'owner del documento sui link SharePoint/OneDrive personali
 * e il nome del team per i siti SharePoint di team
 */

(function() {
    'use strict';

    /**
     * Estrae l'owner dall'URL SharePoint personale
     * /personal/nome_cognome_dominio_tld/ → nome.cognome@dominio.tld
     */
    function extractSharePointOwner(url) {
        const match = url.match(/\/personal\/([^\/]+)\//i);
        if (!match) return null;

        const parts = match[1].split('_');
        if (parts.length < 3) return null;

        const nome = parts[0];
        const cognome = parts[1];
        const dominio = parts.slice(2).join('.');

        return nome + '.' + cognome + '@' + dominio;
    }

    /**
     * Estrae il nome del team/sito dall'URL SharePoint
     * /sites/TeamName/ → TeamName
     */
    function extractSharePointTeam(url) {
        const match = url.match(/\/sites\/([^\/]+)\//i);
        if (!match) return null;

        // Converti camelCase/PascalCase in spazi per leggibilità
        // es: TeamRebuildAnagrafeUnica → Team Rebuild Anagrafe Unica
        return match[1].replace(/([a-z])([A-Z])/g, '$1 $2');
    }

    /**
     * Verifica se un URL è SharePoint/OneDrive
     */
    function isSharePointUrl(url) {
        return url && (
            url.includes('sharepoint.com') ||
            url.includes('onedrive.com')
        );
    }

    /**
     * Genera il contenuto e il tema del tooltip per un URL SharePoint
     * @returns {object|null} { content: string, theme: string } o null
     */
    function getTooltipInfo(url) {
        // Prima prova a estrarre l'owner (link personali)
        const owner = extractSharePointOwner(url);
        if (owner) {
            return {
                content: 'Owner: ' + owner,
                theme: 'sharepoint-owner'  // Blu
            };
        }

        // Poi prova a estrarre il team (siti di team)
        const team = extractSharePointTeam(url);
        if (team) {
            return {
                content: 'Team: ' + team,
                theme: 'sharepoint-team'  // Arancione
            };
        }

        return null;
    }

    /**
     * Inizializza i tooltip sui link SharePoint
     */
    window.initializeSharePointTooltips = function() {
        const links = document.querySelectorAll('a[href]');

        links.forEach(function(link) {
            const href = link.getAttribute('href');

            // Salta se non è SharePoint o se ha già un tooltip
            if (!isSharePointUrl(href) || link.hasAttribute('data-tippy-sharepoint')) {
                return;
            }

            const tooltipInfo = getTooltipInfo(href);
            if (tooltipInfo) {
                // Marca come processato
                link.setAttribute('data-tippy-sharepoint', 'true');
                link.setAttribute('data-tippy-content', tooltipInfo.content);

                // Inizializza Tippy su questo link con il tema appropriato
                tippy(link, {
                    placement: 'top',
                    theme: tooltipInfo.theme,
                    arrow: true,
                    delay: [300, 0]
                });
            }
        });
    };

    // Inizializza quando il DOM è pronto
    $(function() {
        window.initializeSharePointTooltips();
        // Retry per contenuto caricato dinamicamente
        setTimeout(window.initializeSharePointTooltips, 500);
        setTimeout(window.initializeSharePointTooltips, 1500);
    });

})();
