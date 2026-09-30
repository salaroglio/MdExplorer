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
        relation: 'Relation',
        // ---- the bar of a slide deck, the transitions, the corrections (slides/*.js, inline-edit.js)
        "slide.modes": "Mode",
        "slide.present": "Present",
        "slide.presentTitle": "Present: the deck as it is, with its links and moving between decks",
        "slide.edit": "Edit",
        "slide.editTitle": "Quick edit: click to correct the text, drag the items (Esc to leave)",
        "slide.grip": "Drag the bar",
        "slide.fullscreen": "Full screen (Esc to leave)",
        "slide.fullscreenNo": "Full screen is not available here.",
        "tr.button": "Slide transition",
        "tr.scopeSlide": "This slide",
        "tr.scopeDeck": "Whole deck",
        "tr.slideNotInFile": "This slide cannot be changed from here: a command writes it, it is not in the file.",
        "tr.hintDeck": "Applies to the slides that have no transition of their own.",
        "tr.hintSlide": "How this slide comes in and goes out.",
        "tr.hintStack": "How this slide comes in and goes out. In a stack it applies to moves up and down; sideways, the deck's one counts.",
        "tr.asDeck": "Like the deck ({name})",
        "tr.useLast": "Use the last choice: {name}",
        "tr.useLastTitle": "The transition chosen last, even on another slide",
        "tr.lastMark": "Last choice",
        "tr.custom": "Written by hand in the file: \"{value}\". Choosing one replaces it.",
        "tr.none": "None",
        "tr.fade": "Fade",
        "tr.slide": "Slide",
        "tr.convex": "Convex",
        "tr.concave": "Concave",
        "tr.zoom": "Zoom",
        "tr.failed": "The transition could not be changed ({detail}).",
        "drag.handle": "Drag to move the item",
        "drag.failed": "The item could not be moved ({detail}).",
        "conflict": "The document changed after the page was loaded: reload it and try again.",
        "refusal.MoveListItem.NotAListItem": "This item is no longer in the file: reload the page and try again.",
        "refusal.MoveListItem.SingleItem": "The list has one item only: there is nowhere to move it.",
        "refusal.MoveListItem.PositionOutOfRange": "That position does not exist in the list: reload the page and try again.",
        "refusal.MoveListItem.MarkerWidthDiffers": "In this numbered list the move would change the width of the number (9 to 10 or back) and the item's lines would no longer line up: move it in the file.",
        "refusal.MoveListItem.EmptyFirstLine": "This item starts with an empty line: move it in the file.",
        "refusal.MoveListItem.ChangesOtherBlocks": "Moving this item would change something else in the file (a list laid out differently, another block): move it in the file.",
        "refusal.MoveListItem.FragmentOrderFixed": "The items of this list have a hand-written data-fragment-index: moving them would not change the order they appear in. Change the indexes in the file.",
        "refusal.SlideTransition.UnknownTransition": "That transition does not exist: choose one from the list.",
        "refusal.SlideTransition.NotADeck": "This file is not a slide deck (no front matter with document_type: slides).",
        "refusal.SlideTransition.NotASlide": "This slide is no longer in the file (maybe a command wrote it): reload the page and try again.",
        "refusal.SlideTransition.UnsupportedLayout": "The file is written in a form that cannot be safely edited from here (a .slide: comment on several lines, or a one-line front matter): change the transition in the file.",
        "refusal.SlideTransition.ChangesOtherContent": "Writing the transition would change something else in the deck: change it in the file.",
        "refusal.EditRenderedText.BlockNotFound": "This block is no longer in the file: reload the page and try again.",
        "refusal.EditRenderedText.UnsupportedContent": "This block contains elements that cannot be corrected from the page (formulas, notes…): correct it in the file.",
        "refusal.EditRenderedText.RenderedTextMismatch": "The page shows this block differently from how it is written in the file (for example with interactive emoji): correct it in the file.",
        "refusal.EditRenderedText.LineBreakNotAllowed": "From the page only the text is corrected: line breaks are not added or removed.",
        "refusal.EditRenderedText.ProtectedContentTouched": "Emoji, images, checkboxes and link addresses cannot be changed from the page: correct only the text around them.",
        "refusal.EditRenderedText.FormattingNotAllowed": "The text you typed has formatting (bold, italic, link) that is not there: formatting cannot be added from the page.",
        "refusal.EditRenderedText.EditTooLarge": "The correction changes too much text at once: do it in several steps.",
        "refusal.EditRenderedText.BlockDeletionNotAllowed": "Deleting all the text here does not safely remove the block (an item with sub-items, an underlined title, a block inside a quote): remove it in the file.",
        "refusal.EditRenderedText.VerificationFailed": "What you typed would be read as Markdown (a link, an emoji, a symbol) and the page would not show it like that: change it and try again.",
        "edit.menuEdit": "Edit text",
        "edit.menuAsk": "Ask MarkAgent",
        "edit.menuHint": "Shift + right click: the browser menu",
        "edit.noText": "There is no text to correct here.",
        "edit.hint": "Enter or click outside: save · Esc: cancel",
        "edit.saving": "Saving…",
        "edit.retry": "Fix it and press Enter · Esc: cancel",
        "edit.notSaved": "The correction was not saved ({detail})",
        "nav.linkFailed": "The link could not be opened in the browser: {href}",
        "paste.menuPaste": "Paste image here",
        "paste.failed": "Paste failed: {detail}",
        "paste.line": "the image will go here"
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
        relation: 'Relazione',
        // ---- la barra delle slide, le transizioni, le correzioni (slides/*.js, inline-edit.js)
        "slide.modes": "Modalità",
        "slide.present": "Presenta",
        "slide.presentTitle": "Presenta: la presentazione com'è, con i link e il passaggio da un deck all'altro",
        "slide.edit": "Modifica",
        "slide.editTitle": "Modifica veloce: clic per correggere il testo, trascina le voci (Esc per uscire)",
        "slide.grip": "Trascina la barra",
        "slide.fullscreen": "Schermo intero (Esc per uscire)",
        "slide.fullscreenNo": "Lo schermo intero non è disponibile qui.",
        "tr.button": "Transizione delle slide",
        "tr.scopeSlide": "Questa slide",
        "tr.scopeDeck": "Tutto il deck",
        "tr.slideNotInFile": "Questa slide non si può cambiare da qui: la scrive un comando, non è nel file.",
        "tr.hintDeck": "Vale per le slide che non hanno una transizione propria.",
        "tr.hintSlide": "Come entra ed esce questa slide.",
        "tr.hintStack": "Come entra ed esce questa slide. In una pila vale per i passaggi in verticale; in orizzontale conta quella del deck.",
        "tr.asDeck": "Come il deck ({name})",
        "tr.useLast": "Usa l'ultima scelta: {name}",
        "tr.useLastTitle": "La transizione scelta per ultima, anche su un'altra slide",
        "tr.lastMark": "Ultima scelta",
        "tr.custom": "Scritta a mano nel file: «{value}». Sceglierne una la sostituisce.",
        "tr.none": "Nessuna",
        "tr.fade": "Dissolvenza",
        "tr.slide": "Scorrimento",
        "tr.convex": "Convessa",
        "tr.concave": "Concava",
        "tr.zoom": "Zoom",
        "tr.failed": "Non sono riuscito a cambiare la transizione ({detail}).",
        "drag.handle": "Trascina per spostare la voce",
        "drag.failed": "Non sono riuscito a spostare la voce ({detail}).",
        "conflict": "Il documento è cambiato dopo che la pagina è stata caricata: ricaricala e riprova.",
        "refusal.MoveListItem.NotAListItem": "Non trovo più questa voce nel file: ricarica la pagina e riprova.",
        "refusal.MoveListItem.SingleItem": "L'elenco ha una voce sola: non c'è dove spostarla.",
        "refusal.MoveListItem.PositionOutOfRange": "Quella posizione non esiste nell'elenco: ricarica la pagina e riprova.",
        "refusal.MoveListItem.MarkerWidthDiffers": "In questo elenco numerato lo spostamento cambierebbe la larghezza del numero (da 9 a 10 o viceversa) e le righe della voce non sarebbero più allineate: spostala nel file.",
        "refusal.MoveListItem.EmptyFirstLine": "Questa voce inizia con la riga vuota: spostala nel file.",
        "refusal.MoveListItem.ChangesOtherBlocks": "Spostare questa voce cambierebbe altro nel file (un elenco impaginato diversamente, un altro blocco): spostala nel file.",
        "refusal.MoveListItem.FragmentOrderFixed": "Le voci di questo elenco hanno un data-fragment-index scritto a mano: spostarle non cambierebbe l'ordine in cui compaiono. Cambia gli indici nel file.",
        "refusal.SlideTransition.UnknownTransition": "Questa transizione non esiste: scegline una dell'elenco.",
        "refusal.SlideTransition.NotADeck": "Questo file non è una presentazione (manca il front matter con document_type: slides).",
        "refusal.SlideTransition.NotASlide": "Non trovo più questa slide nel file (forse è stata scritta da un comando): ricarica la pagina e riprova.",
        "refusal.SlideTransition.UnsupportedLayout": "Il file è scritto in una forma che da qui non si modifica in modo sicuro (un commento .slide: su più righe, o il front matter su una riga): cambia la transizione nel file.",
        "refusal.SlideTransition.ChangesOtherContent": "Scrivere la transizione cambierebbe altro nella presentazione: cambiala nel file.",
        "refusal.EditRenderedText.BlockNotFound": "Non trovo più questo blocco nel file: ricarica la pagina e riprova.",
        "refusal.EditRenderedText.UnsupportedContent": "Questo blocco contiene elementi che non si correggono dalla pagina (formule, note…): correggilo nel file.",
        "refusal.EditRenderedText.RenderedTextMismatch": "La pagina mostra questo blocco diversamente da com'è scritto nel file (per esempio con emoji interattive): correggilo nel file.",
        "refusal.EditRenderedText.LineBreakNotAllowed": "Dalla pagina si corregge solo il testo: gli a capo non si aggiungono né si tolgono.",
        "refusal.EditRenderedText.ProtectedContentTouched": "Emoji, immagini, caselle e indirizzi dei link non si cambiano dalla pagina: correggi solo il testo intorno.",
        "refusal.EditRenderedText.FormattingNotAllowed": "Il testo scritto ha una formattazione (grassetto, corsivo, link) che lì non c'è: dalla pagina non si aggiunge formattazione.",
        "refusal.EditRenderedText.EditTooLarge": "La correzione cambia troppo testo in una volta: falla in più passi.",
        "refusal.EditRenderedText.BlockDeletionNotAllowed": "Qui cancellare tutto il testo non toglie il blocco in modo sicuro (una voce con sotto-voci, un titolo sottolineato, un blocco in una citazione): toglilo nel file.",
        "refusal.EditRenderedText.VerificationFailed": "Quello che hai scritto verrebbe letto come Markdown (un link, un'emoji, un simbolo) e la pagina non lo mostrerebbe così: cambialo e riprova.",
        "edit.menuEdit": "Modifica testo",
        "edit.menuAsk": "Chiedi a MarkAgent",
        "edit.menuHint": "Shift + tasto destro: menu del browser",
        "edit.noText": "Qui non c'è testo da correggere.",
        "edit.hint": "Invio o clic fuori: salva · Esc: annulla",
        "edit.saving": "Salvataggio…",
        "edit.retry": "Correggi e premi Invio · Esc: annulla",
        "edit.notSaved": "Correzione non salvata ({detail})",
        "nav.linkFailed": "Non sono riuscito ad aprire il link nel browser: {href}",
        "paste.menuPaste": "Incolla immagine qui",
        "paste.failed": "Incolla non riuscito: {detail}",
        "paste.line": "l'immagine andrà qui"
    }
};

function _toolbarLang() {
    var saved = null;
    try { saved = window.localStorage.getItem('mdexplorer_language'); } catch (e) { /* storage negato */ }
    if (saved && TOOLBAR_TEXTS[saved]) return saved;
    return (navigator.language || '').indexOf('it') === 0 ? 'it' : 'en';
}

/**
 * The text of a key in the app's language. {name}, {detail}… in the text are filled from params.
 * A key that has no text in the language falls back to English, then to the key itself: a missing
 * translation shows as the key, which is seen and fixed (the test that IT and EN have the same keys is in
 * ToolbarTexts_Should).
 */
function _toolbarText(key, params) {
    var texts = TOOLBAR_TEXTS[_toolbarLang()];
    var text = texts[key] || TOOLBAR_TEXTS.en[key] || key;
    if (params) {
        text = text.replace(/\{(\w+)\}/g, function (whole, name) { return params[name] !== undefined ? params[name] : whole; });
    }
    return text;
}

function _toolbarHas(key) {
    return !!(TOOLBAR_TEXTS.en[key] || TOOLBAR_TEXTS.it[key]);
}

/**
 * Why the server said no, in the app's language: the server sends a code (refusal) and, for a page that
 * changed under the reader, error 'document-changed'; the words are here. scope is the endpoint
 * ('MoveListItem', 'SlideTransition', 'EditRenderedText'). Null when it sent nothing that has a text here:
 * the caller says its own generic thing.
 */
function _toolbarServerMessage(scope, result) {
    result = result || {};
    if (result.error === 'document-changed') return _toolbarText('conflict');
    var key = 'refusal.' + scope + '.' + result.refusal;
    if (result.refusal && _toolbarHas(key)) return _toolbarText(key);
    return null;
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
