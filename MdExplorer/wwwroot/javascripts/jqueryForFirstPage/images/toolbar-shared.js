/**
 * MdExplorer - Testi della barra dei diagrammi e memoria della legenda
 * =====================================================================
 * In comune fra la barra delle immagini dei documenti (image-transform.js, che li usa da
 * sempre) e la legenda dei diagrammi nelle slide (javascripts/slides/slide-diagrams.js):
 * una sola fonte per le traduzioni IT/EN e per «legenda aperta o chiusa».
 * Nessuna dipendenza (niente jQuery): la pagina delle slide non lo carica.
 */

// Le scritte della barra nella lingua dell'app. La pagina del diagramma è servita
// dallo stesso indirizzo di client2, quindi legge la stessa chiave che vi scrive
// LanguageService (language.service.ts); se manca, la stessa regola di ripiego:
// italiano se il sistema è in italiano, altrimenti inglese. Letta a ogni uso, così
// un cambio di lingua nelle Impostazioni vale dalla volta dopo, senza ricaricare.
var TOOLBAR_TEXTS = {
    en: {
        lightOn: 'Turn on the light (view in light mode)',
        lightOff: 'Turn off the light (back to dark mode)',
        legendShow: 'Show colour legend',
        legendHide: 'Hide colour legend',
        boxes: 'Boxes',
        arrows: 'Arrows',
        selected: 'Selected',
        incoming: 'Points to the selected one',
        outgoing: 'Receives from the selected one',
        note: 'Note',
        cluster: 'Inside the selected package',
        extension: 'Inheritance / realization',
        composition: 'Composition',
        aggregation: 'Aggregation',
        dependency: 'Dependency / directed (-->)',
        association: 'Association',
        other: 'Other relation',
        relation: 'Relation'
    },
    it: {
        lightOn: 'Accendi la luce (vedi il diagramma a colori chiari)',
        lightOff: 'Spegni la luce (torna al tema scuro)',
        legendShow: 'Mostra la legenda dei colori',
        legendHide: 'Nascondi la legenda dei colori',
        boxes: 'Box',
        arrows: 'Frecce',
        selected: 'Selezionato',
        incoming: 'Punta verso il selezionato',
        outgoing: 'Riceve dal selezionato',
        note: 'Nota',
        cluster: 'Dentro il package selezionato',
        extension: 'Ereditarietà / realizzazione',
        composition: 'Composizione',
        aggregation: 'Aggregazione',
        dependency: 'Dipendenza / freccia (-->)',
        association: 'Associazione',
        other: 'Altra relazione',
        relation: 'Relazione'
    }
};

function _toolbarLang() {
    var saved = null;
    try { saved = window.localStorage.getItem('mdexplorer_language'); } catch (e) { /* storage negato */ }
    if (saved && TOOLBAR_TEXTS[saved]) return saved;
    return (navigator.language || '').indexOf('it') === 0 ? 'it' : 'en';
}

function _toolbarText(key) {
    var texts = TOOLBAR_TEXTS[_toolbarLang()];
    return texts[key] || TOOLBAR_TEXTS.en[key] || key;
}

// Legenda dei colori dei diagrammi interattivi (F7): aperta o chiusa, uguale per
// tutti i diagrammi e ricordata dal browser. Se il browser non concede lo storage,
// parte chiusa.
var SVG_LEGEND_STORAGE_KEY = 'mde.svgLegendOpen';

function _svgLegendRemembered() {
    try { return window.localStorage.getItem(SVG_LEGEND_STORAGE_KEY) === '1'; } catch (e) { return false; }
}

function _rememberSvgLegend(open) {
    try { window.localStorage.setItem(SVG_LEGEND_STORAGE_KEY, open ? '1' : '0'); } catch (e) { /* solo comodità */ }
}
