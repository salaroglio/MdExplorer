/**
 * Il grafo del codice come pagina del visualizzatore (sprint 2026-10-09, D17, F6).
 *
 * - I nodi di primo livello sono i FILE della cartella (box), raggruppati per sottocartella (colore);
 *   «+» su un file mostra i suoi PUNTI D'INGRESSO (D24: per uno script, lo script stesso), «+» su un simbolo
 *   mostra chi chiama, anche in altri file: si scende a cascata. «−» richiude.
 * - Niente sparisce in silenzio: il numero accanto al «+» dice quanti simboli si aprirebbero, ciò che è fuori dal
 *   perimetro (cmdlet, metodi .NET, pacchetti) è elencato nella scheda del simbolo, e chi non è chiamato da nessuno
 *   ha il bordo arancione tratteggiato (codice morto, o un punto d'ingresso).
 * - Tasto destro su un simbolo: «Nascondi dal grafo» (D26, per Write-Log & co. chiamate da ovunque). I nascosti si
 *   ricordano per progetto e cartella, ma non spariscono in silenzio: «Nascosti: N» in alto li elenca e li rimette, e
 *   la scheda di chi li chiama li nomina.
 * - Tasto destro, MarkAgent (D27): «Chiedi a MarkAgent» manda `mde-mark.askAboutSymbol` al genitore (Angular,
 *   MarkDiagramService: la domanda va nella sessione del tab MarkAgent, la risposta nella finestra di Mark);
 *   «Analizza con Mark» chiede POST /api/codegraph/describe-symbol per quel solo simbolo e aggiorna la scheda.
 * - Tasto destro su un FILE (12/10): MarkAgent spiega il file e i suoi legami; «Analizza il file con Mark» scrive a che
 *   cosa serve il file (code:FileDescription, POST /api/codegraph/describe-file). Tasto destro su un ARCO: MarkAgent
 *   spiega che cosa vuol dire quel legame (quali punti, se certo o dedotto) — le sigle extracted/inferred non dicevano niente.
 * - ACCORDION dei file (12/10): il «▸» a sinistra del box apre sotto il file a che cosa serve, in breve (la sua
 *   code:FileDescription); le righe sotto scendono. «Utilità dei file» in alto le apre o le chiude tutte. Gli accordion
 *   aperti stanno nell'URL (parametro desc), come i file aperti.
 * - Disegno A SCALETTA (D26, poi D29), CALCOLATO e non simulato: ogni nodo sulla sua riga, rientrato rispetto a chi
 *   l'ha fatto comparire, come l'albero di un esplora risorse — con tante funzioni affiancate le etichette si
 *   sovrapponevano. Le chiamate fra nodi che non sono padre e figlio sono archi curvi di lato. Un nodo trascinato resta
 *   dove lo si lascia; se dopo un'apertura qualcosa esce dallo schermo, la vista si adatta.
 * - Gli archi hanno un colore per relazione e sono tratteggiati quando la confidenza è `inferred`
 *   o `ambiguous`; fra due file si sommano (il numero sull'arco).
 * - Il clic su un simbolo apre la scheda con firma, parametri e la descrizione scritta dalla skill;
 *   «Apri il sorgente» naviga il visualizzatore sul file (messaggio `md-navigate` al genitore).
 * - I dati arrivano da GET /api/codegraph/graph (D15: solo il DB); niente sorgenti letti qui.
 * - I testi stanno in CG_TEXTS, in italiano e in inglese; la lingua è quella dell'interfaccia, che il backend scrive in
 *   <html lang>. Un testo che manca in una lingua si vede come la sua chiave (il test è CodeGraphViewTexts_Should).
 */
const CG_TEXTS = {
    it: {
        loading: 'Carico il grafo…',
        noAnswer: 'MdExplorer non risponde: {detail}',
        error: 'Errore {status}',
        noSymbols: 'Nessun simbolo per «{folder}»: apri «Ambiente di sviluppo» sulla cartella e lancia l\'analisi.',
        wholeProject: 'tutto il progetto',
        statusFiles: '{n} file',
        statusFilesUndescribed: ' ({n} senza descrizione)',
        statusCounts: ', {symbols} simboli mostrati, {links} archi',
        statusUncalled: ' · {n} che nessuno chiama',
        statusUndescribed: ' · {n} senza descrizione',
        noDescription: 'Nessuna descrizione ancora. Tasto destro sul file → «Analizza il file con Mark», oppure «Analizza il codice con Mark» sulla cartella.',
        filesPurpose: 'Utilità dei file',
        filesPurposeClose: 'Chiudi le descrizioni dei file',
        filesPurposeOpen: 'Mostra sotto ogni file a che cosa serve',
        askFile: 'Chiedi a MarkAgent di spiegare «{name}» e i suoi legami',
        redoFile: 'Rifai l\'analisi del file con Mark',
        analyzeFile: 'Analizza il file con Mark',
        closePurpose: 'Chiudi l\'utilità del file',
        showPurpose: 'Mostra l\'utilità del file',
        closeSymbols: 'Chiudi i simboli',
        showSymbols: 'Mostra i simboli',
        openFile: 'Apri il file',
        askSymbol: 'Chiedi a MarkAgent di «{name}»',
        redoSymbol: 'Rifai l\'analisi con Mark',
        analyzeSymbol: 'Analizza con Mark',
        hideSymbol: 'Nascondi «{name}» dal grafo',
        closeCalls: 'Chiudi le chiamate',
        followCalls: 'Segui le chiamate ({n})',
        openSource: 'Apri il sorgente',
        askLink: 'Chiedi a MarkAgent: che cosa vuol dire «{from} → {to}»?',
        linkTip: ' — tasto destro: chiedi a MarkAgent',
        'rel.calls': 'chiama',
        'rel.imports': 'importa',
        'rel.references': 'usa il tipo',
        'rel.instantiates': 'crea',
        'rel.inherits': 'eredita da',
        'rel.implements': 'implementa',
        'conf.extracted': 'scritto nel codice',
        'conf.inferred': 'dedotto dal nome',
        'conf.ambiguous': 'ambiguo: più candidati',
        cannotOpen: 'Non si apre: {detail}',
        analyzing: 'Mark sta analizzando «{name}»… (può volerci un minuto)',
        analyzedBy: 'Analizzato da {engine}',
        rejectedLines: ' ({n} righe della risposta scartate)',
        analysisFailed: 'Analisi non riuscita: {detail}',
        hiddenChip: 'Nascosti: {n}',
        show: 'Mostra',
        showAll: 'Mostra tutti',
        moduleSummary: 'Lo script stesso: la sua descrizione è quella del file. I parametri con cui lo si lancia sono in «Dettagli».',
        fileUndescribed: 'Il file non ha ancora una descrizione.',
        moduleUndescribed: 'Lo script stesso: il codice fuori dalle funzioni, quello che gira quando lo si lancia. Non ha ancora una descrizione.',
        symbolUndescribed: 'Nessuna descrizione.',
        uncalled: 'Nessun simbolo del perimetro la chiama: è codice morto, oppure un punto d\'ingresso (chiamata da fuori, da un evento, per riflessione).',
        details: 'Dettagli',
        alsoCallsHidden: 'Chiama anche (nascoste):',
        'ext.calls': 'Chiama fuori dal progetto',
        'ext.instantiates': 'Crea oggetti esterni',
        'ext.references': 'Usa tipi esterni',
        'ext.inherits': 'Eredita da',
        'ext.implements': 'Implementa',
        'ext.imports': 'Importa',
        outOfScope: 'Fuori dal perimetro · {n}',
        loadingShort: 'Carico…',
        parameter: 'Parametro',
        type: 'Tipo',
        description: 'Descrizione',
        returns: 'Ritorna',
        staleDescription: 'La descrizione è di una firma precedente: da rivedere.',
        nothingElse: 'Niente altro da mostrare.',
        legendUncalled: 'nessuno la chiama',
        cannotCheckOutdated: 'Non riesco a controllare se il grafo è aggiornato: {detail}',
        outdatedOne: '{n} file analizzato con una versione precedente dell\'estrattore',
        outdatedMany: '{n} file analizzati con una versione precedente dell\'estrattore',
        outdatedOld: 'Per questi file stai guardando il grafo vecchio.',
        upgrade: 'Aggiorna l\'analisi',
        whichFiles: 'Quali file',
        upgrading: 'Aggiorno l\'analisi…',
        upgradedOne: 'Analisi aggiornata: {n} file riparsato.',
        upgradedMany: 'Analisi aggiornata: {n} file riparsati.',
        upgradeFailed: 'Aggiornamento fallito: {detail}'
    },
    en: {
        loading: 'Loading the graph…',
        noAnswer: 'MdExplorer does not answer: {detail}',
        error: 'Error {status}',
        noSymbols: 'No symbols for «{folder}»: open «Development environment» on the folder and run the analysis.',
        wholeProject: 'the whole project',
        statusFiles: '{n} files',
        statusFilesUndescribed: ' ({n} without a description)',
        statusCounts: ', {symbols} symbols shown, {links} edges',
        statusUncalled: ' · {n} that nobody calls',
        statusUndescribed: ' · {n} without a description',
        noDescription: 'No description yet. Right-click the file → «Analyze the file with Mark», or «Analyze the code with Mark» on the folder.',
        filesPurpose: 'What the files are for',
        filesPurposeClose: 'Close the descriptions of the files',
        filesPurposeOpen: 'Show under each file what it is for',
        askFile: 'Ask MarkAgent to explain «{name}» and its links',
        redoFile: 'Analyze the file with Mark again',
        analyzeFile: 'Analyze the file with Mark',
        closePurpose: 'Close what the file is for',
        showPurpose: 'Show what the file is for',
        closeSymbols: 'Close the symbols',
        showSymbols: 'Show the symbols',
        openFile: 'Open the file',
        askSymbol: 'Ask MarkAgent about «{name}»',
        redoSymbol: 'Analyze with Mark again',
        analyzeSymbol: 'Analyze with Mark',
        hideSymbol: 'Hide «{name}» from the graph',
        closeCalls: 'Close the calls',
        followCalls: 'Follow the calls ({n})',
        openSource: 'Open the source',
        askLink: 'Ask MarkAgent: what does «{from} → {to}» mean?',
        linkTip: ' — right-click: ask MarkAgent',
        'rel.calls': 'calls',
        'rel.imports': 'imports',
        'rel.references': 'uses the type',
        'rel.instantiates': 'creates',
        'rel.inherits': 'inherits from',
        'rel.implements': 'implements',
        'conf.extracted': 'written in the code',
        'conf.inferred': 'deduced from the name',
        'conf.ambiguous': 'ambiguous: several candidates',
        cannotOpen: 'It does not open: {detail}',
        analyzing: 'Mark is analyzing «{name}»… (it can take a minute)',
        analyzedBy: 'Analyzed by {engine}',
        rejectedLines: ' ({n} lines of the answer rejected)',
        analysisFailed: 'Analysis failed: {detail}',
        hiddenChip: 'Hidden: {n}',
        show: 'Show',
        showAll: 'Show all',
        moduleSummary: 'The script itself: its description is the one of the file. The parameters it is launched with are in «Details».',
        fileUndescribed: 'The file has no description yet.',
        moduleUndescribed: 'The script itself: the code outside the functions, what runs when it is launched. It has no description yet.',
        symbolUndescribed: 'No description.',
        uncalled: 'No symbol in scope calls it: it is dead code, or an entry point (called from outside, by an event, by reflection).',
        details: 'Details',
        alsoCallsHidden: 'Also calls (hidden):',
        'ext.calls': 'Calls outside the project',
        'ext.instantiates': 'Creates external objects',
        'ext.references': 'Uses external types',
        'ext.inherits': 'Inherits from',
        'ext.implements': 'Implements',
        'ext.imports': 'Imports',
        outOfScope: 'Out of scope · {n}',
        loadingShort: 'Loading…',
        parameter: 'Parameter',
        type: 'Type',
        description: 'Description',
        returns: 'Returns',
        staleDescription: 'The description is of an earlier signature: to review.',
        nothingElse: 'Nothing else to show.',
        legendUncalled: 'nobody calls it',
        cannotCheckOutdated: 'Cannot check whether the graph is up to date: {detail}',
        outdatedOne: '{n} file analyzed with an earlier version of the extractor',
        outdatedMany: '{n} files analyzed with an earlier version of the extractor',
        outdatedOld: 'For these files you are looking at the old graph.',
        upgrade: 'Update the analysis',
        whichFiles: 'Which files',
        upgrading: 'Updating the analysis…',
        upgradedOne: 'Analysis updated: {n} file re-parsed.',
        upgradedMany: 'Analysis updated: {n} files re-parsed.',
        upgradeFailed: 'Update failed: {detail}'
    }
};

(function () {
    'use strict';
    const LANG = CG_TEXTS[document.documentElement.lang] ? document.documentElement.lang : 'en';
    /** Il testo di una chiave nella lingua dell'interfaccia; {nome} nel testo si riempie da params. */
    function T(key, params) {
        const text = CG_TEXTS[LANG][key] || CG_TEXTS.en[key] || key;
        return params ? text.replace(/\{(\w+)\}/g, (whole, name) => (params[name] !== undefined ? params[name] : whole)) : text;
    }
    const qs = new URLSearchParams(location.search);
    const projectPath = qs.get('projectPath') || '';
    const folder = qs.get('folder') || '';
    const focus = qs.get('symbol') || '';
    let isDark = false;   // si legge quando il body esiste (lo script sta nell'head)
    const expanded = new Set((qs.get('expand') || '').split(',').filter(Boolean));
    const opened = new Set((qs.get('open') || '').split(',').filter(Boolean));
    const descOpen = new Set((qs.get('desc') || '').split(',').filter(Boolean));   // file con l'accordion aperto (percorsi)
    const FIT_MAX_ZOOM = 1.4;
    const DESC_FONT = '11px -apple-system, "Segoe UI", Inter, system-ui, sans-serif', DESC_LINE = 14, DESC_WIDTH = 380;
    const ROW_GAP = 34;   // a scaletta: ogni nodo sulla sua riga
    const INDENT = 44;    // rientro di un livello rispetto a chi l'ha fatto comparire
    let firstFit = true;   // al primo disegno la vista mostra tutto il grafo
    const dragged = new Map();   // id → {x, y}: un nodo spostato a mano resta dove l'utente l'ha messo
    // I nascosti si ricordano per progetto e cartella (comodità di chi guarda: se lo storage non c'è, si riparte da zero).
    const hiddenKey = 'mde-codegraph-hidden|' + projectPath + '|' + folder;
    const hidden = new Map();   // id → nome
    try { (JSON.parse(localStorage.getItem(hiddenKey) || '[]') || []).forEach(h => hidden.set(h.id, h.name)); } catch (e) { /* storage assente */ }
    function saveHidden() {
        try { localStorage.setItem(hiddenKey, JSON.stringify(Array.from(hidden, ([id, name]) => ({ id, name })))); } catch (e) { /* storage assente */ }
    }
    let graph = null, data = null, selected = null;

    const REL_COLORS = {
        calls: '#2563eb', references: '#7c3aed', inherits: '#dc2626', implements: '#059669',
        instantiates: '#d97706', imports: '#64748b', contains: '#94a3b8'
    };
    const KIND_ICON = {
        file: '📄', class: 'C', interface: 'I', record: 'R', struct: 'S', enum: 'E', method: 'ƒ', constructor: '⚙', property: 'P',
        function: 'ƒ', type: 'T', component: '◧', service: '⚒', directive: '◈', pipe: '|', ngmodule: '▣', module: '▦', external: '🌐'
    };
    const PALETTE = ['#60a5fa', '#34d399', '#fbbf24', '#f472b6', '#a78bfa', '#fb923c', '#2dd4bf', '#f87171', '#a3e635', '#38bdf8'];

    function folderColor(f) {
        if (!data) return PALETTE[0];
        const i = Math.max(0, data.folders.indexOf(f));
        return PALETTE[i % PALETTE.length];
    }

    function apiUrl() {
        const u = new URL('/api/codegraph/graph', location.origin);
        u.searchParams.set('projectPath', projectPath);
        u.searchParams.set('folder', folder);
        if (focus) u.searchParams.set('symbol', focus);
        if (expanded.size) u.searchParams.set('expand', Array.from(expanded).join(','));
        if (opened.size) u.searchParams.set('open', Array.from(opened).join(','));
        if (hidden.size) u.searchParams.set('hide', Array.from(hidden.keys()).join(','));
        return u.toString();
    }

    async function load() {
        isDark = document.body.classList.contains('dark-theme');
        const status = document.getElementById('cg-status');
        status.textContent = T('loading');
        let res;
        try { res = await fetch(apiUrl()); } catch (e) { status.textContent = T('noAnswer', { detail: e.message }); return; }
        if (!res.ok) { status.textContent = T('error', { status: res.status }) + ': ' + (await res.text()); return; }
        data = await res.json();
        if (!data.nodes.length) {
            status.textContent = T('noSymbols', { folder: folder || T('wholeProject') });
            return;
        }
        const syms = data.nodes.filter(n => n.kind !== 'file' && !n.external);
        const uncalled = syms.filter(n => n.uncalled).length;
        const undescribed = syms.filter(n => !n.summary && n.kind !== 'module').length;
        const filesUndescribed = data.nodes.filter(n => n.kind === 'file' && !n.summary).length;
        status.textContent = T('statusFiles', { n: data.nodes.filter(n => n.kind === 'file').length }) + (filesUndescribed ? T('statusFilesUndescribed', { n: filesUndescribed }) : '') +
            T('statusCounts', { symbols: syms.length, links: data.links.length }) +
            (uncalled ? T('statusUncalled', { n: uncalled }) : '') + (undescribed ? T('statusUndescribed', { n: undescribed }) : '');
        render();
        renderLegend();
        renderHidden();
        renderDescToggle();
        if (selected) { const again = graph.graphData().nodes.find(x => x.id === selected.id); select(again || null); }   // la scheda segue il nodo aggiornato
        else if (focus) { const n = graph.graphData().nodes.find(x => x.id === focus); if (n) { select(n); setTimeout(() => graph.centerAt(n.x, n.y, 600), 400); } }
    }

    function nodeWidth(n, ctx, scale) {
        const label = labelOf(n);
        ctx.font = (n.kind === 'file' ? '600 ' : '') + (12 / scale) + 'px -apple-system, "Segoe UI", Inter, system-ui, sans-serif';
        return ctx.measureText(label).width + 22 / scale;
    }
    function labelOf(n) { return (KIND_ICON[n.kind] || '•') + ' ' + n.name; }
    /** Il «+»/«−»: un file che ha simboli, un simbolo che ha qualcosa da aprire (o è già aperto). */
    function hasToggle(n) { return n.kind === 'file' ? n.hidden > 0 : !n.external && (n.hidden > 0 || n.expanded); }

    function render() {
        const el = document.getElementById('cg-graph');
        // zoom e centro scelti da chi guarda sopravvivono al ridisegno (aprire un nodo non deve far saltare la vista)
        const view = graph ? { k: graph.zoom(), c: graph.centerAt() } : null;
        el.innerHTML = '';
        const nodes = data.nodes.map(n => Object.assign({}, n));
        const byId = Object.fromEntries(nodes.map(n => [n.id, n]));
        const links = data.links.filter(l => byId[l.source] && byId[l.target]).map(l => Object.assign({}, l));
        // più relazioni fra gli stessi due nodi (es. imports e calls): ognuna la sua curva, sennò si coprono e quella sotto
        // non si vede né si clicca
        const pairs = {};
        links.forEach(l => { const k = l.source + '|' + l.target; l.__pair = (pairs[k] = (pairs[k] || 0) + 1) - 1; });
        layoutTree(nodes, byId);

        graph = ForceGraph()(el)
            .width(el.clientWidth).height(el.clientHeight)
            .backgroundColor('rgba(0,0,0,0)')
            .graphData({ nodes, links })
            .nodeId('id')
            .nodeLabel(n => n.qualifiedName + (n.summary ? '\n' + n.summary : ''))
            .nodeCanvasObjectMode(() => 'replace')
            .nodeCanvasObject((n, ctx, scale) => {
                if (!isFinite(n.x) || !isFinite(n.y)) return;
                const w = nodeWidth(n, ctx, scale), h = 20 / scale, r = 6 / scale;
                const isFile = n.kind === 'file';
                const fill = n.external ? (isDark ? '#1f2937' : '#f1f5f9') : isFile ? folderColor(n.folder) : (isDark ? '#111827' : '#ffffff');
                const stroke = n === selected ? '#facc15' : n.uncalled ? '#f97316' : isFile ? 'rgba(15,23,42,0.35)' : folderColor(n.folder);
                ctx.beginPath(); roundRect(ctx, n.x - w / 2, n.y - h / 2, w, h, r);
                ctx.fillStyle = fill; ctx.fill();
                ctx.setLineDash(n.uncalled && n !== selected ? [4 / scale, 3 / scale] : []);
                ctx.lineWidth = (n === selected ? 3 : n.uncalled ? 2 : 1.2) / scale; ctx.strokeStyle = stroke; ctx.stroke();
                ctx.setLineDash([]);
                ctx.fillStyle = isFile ? '#0f172a' : (isDark ? '#e6edf3' : '#0f172a');
                ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
                ctx.fillText(labelOf(n), n.x, n.y);
                n.__acc = null;
                if (isFile) {
                    // l'accordion: il triangolo a sinistra del box; aperto, sotto il box a che cosa serve il file
                    const open = descOpen.has(n.path), ax = n.x - w / 2 - 9 / scale, ar = 7 / scale;
                    ctx.beginPath(); ctx.arc(ax, n.y, ar, 0, 2 * Math.PI);
                    ctx.fillStyle = n.summary ? (isDark ? '#475569' : '#64748b') : (isDark ? '#334155' : '#cbd5e1'); ctx.fill();
                    ctx.fillStyle = '#fff'; ctx.font = (9 / scale) + 'px sans-serif';
                    ctx.fillText(open ? '▾' : '▸', ax, n.y + 0.5 / scale);
                    n.__acc = { x: ax, r: ar };
                    if (open) drawDescription(n, ctx, w);
                    ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
                }
                n.__btn = null;
                if (hasToggle(n)) {
                    // il «+» / «−» a destra del box, e quanti simboli si aprirebbero
                    const bx = n.x + w / 2 + 9 / scale, br = 7 / scale;
                    ctx.beginPath(); ctx.arc(bx, n.y, br, 0, 2 * Math.PI);
                    ctx.fillStyle = n.expanded ? '#ef4444' : '#22c55e'; ctx.fill();
                    ctx.fillStyle = '#fff'; ctx.font = 'bold ' + (11 / scale) + 'px sans-serif';
                    ctx.fillText(n.expanded ? '−' : '+', bx, n.y + 0.5 / scale);
                    if (!isFile && n.hidden > 0) {
                        ctx.fillStyle = isDark ? '#94a3b8' : '#475569'; ctx.font = (10 / scale) + 'px sans-serif'; ctx.textAlign = 'left';
                        ctx.fillText(String(n.hidden), bx + br + 3 / scale, n.y + 0.5 / scale);
                    }
                    n.__btn = { x: bx, r: br };
                }
                n.__w = w; n.__h = h;
            })
            .nodePointerAreaPaint((n, color, ctx, scale) => {
                // il box più il «+» a destra e, per un file, il «▸» a sinistra (simmetrico)
                const w = (n.__w || 60) + (n.kind === 'file' ? 48 : 24) / scale, h = n.__h || 20 / scale;
                ctx.fillStyle = color; ctx.fillRect(n.x - w / 2 + (n.kind === 'file' ? 0 : 12 / scale), n.y - h / 2, w, h);
            })
            .linkColor(l => REL_COLORS[l.relation] || '#94a3b8')
            // padre → figlio: dritto (è il gradino della scaletta); ogni altro arco passerebbe sopra le righe in mezzo: si curva di lato
            .linkCurvature(l => (l.source && l.target && l.target.parentId === l.source.id && !l.__pair) ? 0 : 0.6 + 0.35 * (l.__pair || 0))
            .linkWidth(l => Math.min(4, 1 + Math.log2(l.count || 1)))
            .linkLineDash(l => l.confidence === 'extracted' ? null : l.confidence === 'inferred' ? [6, 4] : [2, 4])
            .linkDirectionalArrowLength(6).linkDirectionalArrowRelPos(1)
            .linkLabel(l => relationText(l) + T('linkTip'))
            .onLinkRightClick((l, ev) => showLinkMenu(l, ev))
            .onNodeClick((n, ev) => {
                if (n.__acc) {
                    const rect = el.getBoundingClientRect();
                    const p = graph.screen2GraphCoords(ev.clientX - rect.left, ev.clientY - rect.top);
                    if (Math.hypot(p.x - n.__acc.x, p.y - n.y) <= n.__acc.r * 1.5) { toggleDesc(n); return; }
                }
                if (n.__btn) {
                    // clientX meno il riquadro del grafo: offsetX dipende dall'elemento che riceve il clic (non sempre il canvas)
                    const rect = el.getBoundingClientRect();
                    const p = graph.screen2GraphCoords(ev.clientX - rect.left, ev.clientY - rect.top);
                    if (Math.hypot(p.x - n.__btn.x, p.y - n.__btn.y) <= n.__btn.r * 1.5) { toggle(n); return; }
                }
                select(n);
            })
            .onBackgroundClick(() => { select(null); hideMenu(); })
            .onNodeRightClick((n, ev) => showMenu(n, ev))
            .onNodeDragEnd(n => { n.fx = n.x; n.fy = n.y; dragged.set(n.id, { x: n.x, y: n.y }); });
        // Le posizioni le decide layoutTree (fx/fy): niente forze che spostano i nodi.
        graph.d3Force('charge', null);
        graph.d3Force('link', null);
        graph.d3Force('center', null);
        graph.cooldownTicks(1);
        el.addEventListener('contextmenu', e => e.preventDefault());   // il menu del browser coprirebbe il nostro
        if (view) { graph.zoom(view.k, 0); graph.centerAt(view.c.x, view.c.y, 0); }
        fitIfOutside(el, nodes);
        window.addEventListener('resize', () => graph.width(el.clientWidth).height(el.clientHeight));
    }

    /**
     * La scaletta: ogni nodo pende da chi l'ha fatto comparire (parentId: il file per un punto d'ingresso, il chiamante
     * per gli altri); i file senza padre sono le radici. Visita in profondità: ogni nodo prende la riga successiva e il
     * rientro del suo livello. Il box si disegna centrato su x, quindi x = rientro + metà larghezza: i bordi sinistri
     * dei fratelli restano allineati. Deterministico: aprire un nodo inserisce le sue righe sotto di lui.
     */
    function layoutTree(nodes, byId) {
        const measure = document.createElement('canvas').getContext('2d');
        // lo stesso carattere del disegno (nodeWidth): i file in grassetto, i simboli no — sennò i bordi sinistri non si allineano
        const width = n => {
            measure.font = (n.kind === 'file' ? '600 ' : '') + '12px -apple-system, "Segoe UI", Inter, system-ui, sans-serif';
            return measure.measureText(labelOf(n)).width + 22;
        };
        const kids = {};
        const roots = [];
        nodes.forEach(n => {
            if (n.parentId && byId[n.parentId] && n.parentId !== n.id) (kids[n.parentId] = kids[n.parentId] || []).push(n);
            else roots.push(n);
        });
        let y = 0;
        const place = (n, level) => {
            n.__x = level * INDENT + width(n) / 2;
            n.__y = y;
            // un file con l'accordion aperto occupa anche le righe della sua descrizione
            n.__desc = n.kind === 'file' && descOpen.has(n.path) ? wrap(measure, descriptionText(n)) : null;
            y += ROW_GAP + (n.__desc ? n.__desc.length * DESC_LINE + 14 : 0);
            (kids[n.id] || []).forEach(c => place(c, level + 1));
        };
        roots.forEach(r => place(r, 0));
        nodes.forEach(n => {
            const d = dragged.get(n.id);
            n.x = n.fx = d ? d.x : n.__x;
            n.y = n.fy = d ? d.y : n.__y;
        });
    }

    // ---- accordion dei file

    function descriptionText(n) {
        return n.summary || T('noDescription');
    }

    /** Il testo spezzato in righe larghe al massimo DESC_WIDTH (unità del grafo: il riquadro scala con lo zoom come le righe). */
    function wrap(ctx, text) {
        ctx.font = DESC_FONT;
        const lines = [];
        let line = '';
        String(text).split(/\s+/).forEach(word => {
            const next = line ? line + ' ' + word : word;
            if (ctx.measureText(next).width > DESC_WIDTH && line) { lines.push(line); line = word; } else line = next;
        });
        if (line) lines.push(line);
        return lines.slice(0, 8);
    }

    function drawDescription(n, ctx, boxWidth) {
        const lines = n.__desc || wrap(ctx, descriptionText(n));
        const left = n.x - boxWidth / 2, top = n.y + 13, h = lines.length * DESC_LINE + 8;
        ctx.beginPath(); roundRect(ctx, left, top, DESC_WIDTH + 16, h, 5);
        ctx.fillStyle = isDark ? 'rgba(30,41,59,0.95)' : 'rgba(255,255,255,0.97)'; ctx.fill();
        ctx.lineWidth = 1; ctx.strokeStyle = folderColor(n.folder); ctx.stroke();
        ctx.font = (n.summary ? '' : 'italic ') + DESC_FONT;
        ctx.fillStyle = n.summary ? (isDark ? '#e2e8f0' : '#1e293b') : (isDark ? '#94a3b8' : '#64748b');
        ctx.textAlign = 'left'; ctx.textBaseline = 'top';
        lines.forEach((l, i) => ctx.fillText(l, left + 8, top + 4 + i * DESC_LINE));
    }

    function saveDescInUrl() {
        const u = new URL(location.href);
        if (descOpen.size) u.searchParams.set('desc', Array.from(descOpen).join(',')); else u.searchParams.delete('desc');
        history.replaceState(null, '', u.toString());
    }

    function toggleDesc(n) {
        if (descOpen.has(n.path)) descOpen.delete(n.path); else descOpen.add(n.path);
        saveDescInUrl();
        render(); renderDescToggle();
    }

    /** «Utilità dei file» nell'intestazione: apre o chiude tutti gli accordion. */
    function renderDescToggle() {
        let btn = document.getElementById('cg-desc-all');
        if (!btn) { btn = document.createElement('button'); btn.id = 'cg-desc-all'; btn.className = 'cg-chip'; document.getElementById('cg-header').appendChild(btn); }
        const files = data.nodes.filter(n => n.kind === 'file');
        const allOpen = files.length > 0 && files.every(f => descOpen.has(f.path));
        btn.textContent = (allOpen ? '▾' : '▸') + ' ' + T('filesPurpose');
        btn.title = allOpen ? T('filesPurposeClose') : T('filesPurposeOpen');
        btn.onclick = () => {
            if (allOpen) files.forEach(f => descOpen.delete(f.path)); else files.forEach(f => descOpen.add(f.path));
            saveDescInUrl(); render(); renderDescToggle();
        };
    }

    /** Dopo un'apertura: se un nodo è fuori dallo schermo, la vista si adatta per mostrarli tutti. */
    function fitIfOutside(el, nodes) {
        // dopo il primo disegno: prima il grafo non ha ancora zoom e misure, e le coordinate a schermo sono false
        // (onEngineStop non arriva: i nodi sono tutti fermi, la simulazione non ha niente da fermare)
        setTimeout(() => {
            const margin = 30;
            const outside = nodes.some(n => {
                const p = graph.graph2ScreenCoords(n.x, n.y);
                return p.x < margin || p.y < margin || p.x > el.clientWidth - margin || p.y > el.clientHeight - margin;
            });
            if (outside || firstFit) {
                const ms = firstFit ? 0 : 400;
                graph.zoomToFit(ms, 60);
                // con pochi nodi «adatta» zoomerebbe moltissimo: le descrizioni degli accordion (unità del grafo, come le
                // righe) diventerebbero enormi. L'adattamento si ferma a FIT_MAX_ZOOM; a mano si zooma quanto si vuole.
                setTimeout(() => { if (graph.zoom() > FIT_MAX_ZOOM) graph.zoom(FIT_MAX_ZOOM, 200); }, ms + 20);
            }
            firstFit = false;
        }, 300);
    }

    // ---- tasto destro

    function hideMenu() { const m = document.getElementById('cg-menu'); if (m) m.remove(); }

    function showMenu(n, ev) {
        hideMenu();
        if (n.external) return;
        const items = [];
        if (n.kind === 'file') {
            items.push(['ask', T('askFile', { name: n.name }), () => askMarkAgentAboutFile(n)]);
            items.push(['describe', n.summary ? T('redoFile') : T('analyzeFile'), () => describeFile(n)]);
            items.push(['desc', descOpen.has(n.path) ? T('closePurpose') : T('showPurpose'), () => toggleDesc(n)]);
            items.push(['toggle', n.expanded ? T('closeSymbols') : T('showSymbols'), () => toggle(n)]);
            items.push(['open', T('openFile'), () => openSource(n)]);
        } else {
            items.push(['ask', T('askSymbol', { name: n.name }), () => askMarkAgent(n)]);
            // sullo script stesso (module) l'analisi rifà la descrizione del file e i parametri del param() (D36)
            items.push(['describe', n.summary ? T('redoSymbol') : T('analyzeSymbol'), () => describeSymbol(n)]);
            items.push(['hide', T('hideSymbol', { name: n.name }), () => { hidden.set(n.id, n.name); saveHidden(); if (selected && selected.id === n.id) select(null); load(); }]);
            if (hasToggle(n)) items.push(['toggle', n.expanded ? T('closeCalls') : T('followCalls', { n: n.hidden }), () => toggle(n)]);
            items.push(['open', T('openSource'), () => openSource(n)]);
        }
        openMenu(items, ev);
    }

    /** Tasto destro su un arco: che cosa vuol dire quel legame, spiegato da MarkAgent. */
    function showLinkMenu(l, ev) {
        hideMenu();
        if (!l.source || !l.target || typeof l.source !== 'object') return;
        openMenu([['ask', T('askLink', { from: l.source.name, to: l.target.name }), () => askMarkAgentAboutLink(l)]], ev);
    }

    function openMenu(items, ev) {
        const menu = document.createElement('div');
        menu.id = 'cg-menu';
        items.forEach(([, label, run]) => {
            const b = document.createElement('button');
            b.textContent = label;
            b.onclick = () => { hideMenu(); run(); };
            menu.appendChild(b);
        });
        document.body.appendChild(menu);
        const x = Math.min(ev.clientX, innerWidth - menu.offsetWidth - 8), y = Math.min(ev.clientY, innerHeight - menu.offsetHeight - 8);
        menu.style.left = x + 'px'; menu.style.top = y + 'px';
    }

    const relLabel = r => (CG_TEXTS.en['rel.' + r] ? T('rel.' + r) : r);
    const confLabel = c => (CG_TEXTS.en['conf.' + c] ? T('conf.' + c) : c);
    function relationText(l) {
        return relLabel(l.relation) + ' · ' + confLabel(l.confidence) + (l.count > 1 ? ' · ×' + l.count : '');
    }

    /** I legami del file con gli altri nodi disegnati, in parole: vanno nella domanda, così MarkAgent sa di che cosa parliamo. */
    function linksOf(n) {
        const end = x => (typeof x === 'object' ? x : null);
        return graph.graphData().links.filter(l => end(l.source) && end(l.target) && (l.source.id === n.id || l.target.id === n.id))
            .map(l => (l.source.id === n.id ? '→ ' + (l.target.path || l.target.qualifiedName || l.target.name) : '← ' + (l.source.path || l.source.qualifiedName || l.source.name)) +
                ': ' + l.relation + ' (' + l.confidence + (l.count > 1 ? ', ×' + l.count : '') + ')');
    }

    /**
     * «Apri il sorgente». Un file del progetto si apre nel visualizzatore (md-navigate). Un file di una radice di codice
     * esterna (@alias/…, radici esterne F5) sta fuori dal progetto: si risolve il percorso su questa macchina e si apre
     * nell'IDE scelto per il progetto (api/AppSettings/OpenFile); se la radice non è collegata, la scheda dice perché.
     */
    async function openSource(n) {
        if (!n.path || n.path[0] !== '@') {
            window.parent.postMessage({ type: 'md-navigate', relativePath: n.path, name: n.path.split('/').pop(), line: n.line || null }, '*');
            return;
        }
        const card = document.getElementById('cg-card');
        const note = document.createElement('p');
        note.className = 'cg-muted';
        card.appendChild(note);
        try {
            const r = new URL('/api/coderoots/resolve', location.origin);
            r.searchParams.set('projectPath', projectPath); r.searchParams.set('path', n.path);
            const res = await fetch(r);
            const body = await res.json().catch(() => ({}));
            if (!res.ok) throw new Error(body.error || T('error', { status: res.status }));
            note.textContent = body.absolutePath + (n.line ? ':' + n.line : '');
            const o = new URL('/api/AppSettings/OpenFile', location.origin);
            o.searchParams.set('path', body.absolutePath);
            const cid = qs.get('connectionId'); if (cid) o.searchParams.set('connectionId', cid);
            const opened = await fetch(o);
            const ob = await opened.json().catch(() => ({}));
            if (ob && ob.openUrl) window.open(ob.openUrl, '_blank');
            else if (!opened.ok) throw new Error(ob.error || T('error', { status: opened.status }));
        } catch (e) {
            note.className = 'cg-warn';
            note.textContent = T('cannotOpen', { detail: e.message });
        }
    }

    /** «Chiedi a MarkAgent»: la domanda la fa Angular, nella sessione del tab MarkAgent (motore e sessione unici). */
    function askMarkAgent(n) {
        window.parent.postMessage({
            type: 'mde-mark.askAboutSymbol',
            context: {
                projectPath,
                symbol: { id: n.id, kind: n.kind, name: n.name, qualifiedName: n.qualifiedName || n.name, path: n.path || null, line: n.line || null, signature: n.signature || null, summary: n.summary || null },
            },
        }, '*');
    }

    function askMarkAgentAboutFile(n) {
        window.parent.postMessage({
            type: 'mde-mark.askAboutSymbol',
            context: {
                projectPath,
                symbol: { id: n.id, kind: 'file', name: n.name, qualifiedName: n.path, path: n.path, line: null, signature: null, summary: n.summary || null },
                links: linksOf(n),
            },
        }, '*');
    }

    function askMarkAgentAboutLink(l) {
        const a = l.source, b = l.target, name = (a.path || a.qualifiedName || a.name) + ' → ' + (b.path || b.qualifiedName || b.name);
        window.parent.postMessage({
            type: 'mde-mark.askAboutSymbol',
            context: {
                projectPath,
                symbol: { id: 'rel:' + a.id + '|' + l.relation + '|' + b.id, kind: 'relation', name, qualifiedName: name, path: a.path || null, line: null, signature: null, summary: null },
                relation: { relation: l.relation, confidence: l.confidence, count: l.count || 1, from: a.qualifiedName || a.path || a.name, fromKind: a.kind, to: b.qualifiedName || b.path || b.name, toKind: b.kind },
            },
        }, '*');
    }

    /** «Analizza il file con Mark»: a che cosa serve il file (code:FileDescription); poi l'accordion si apre e la mostra. */
    function describeFile(n) {
        return describeOn(n, '/api/codegraph/describe-file', { projectPath, path: n.path }, () => { descOpen.add(n.path); saveDescInUrl(); });
    }

    /** «Analizza con Mark» su un simbolo: si aspetta la fine, poi la scheda e il grafo mostrano la descrizione nuova. */
    function describeSymbol(n) { return describeOn(n, '/api/codegraph/describe-symbol', { projectPath, symbolId: n.id }); }

    async function describeOn(n, endpoint, body, onDone) {
        select(n);
        const card = document.getElementById('cg-card');
        const note = document.createElement('p');
        note.className = 'cg-muted';
        note.textContent = T('analyzing', { name: n.name });
        card.appendChild(note);
        const btn = document.getElementById('cg-describe'); if (btn) btn.disabled = true;
        try {
            const res = await fetch(endpoint, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
            const answer = await res.json().catch(() => ({}));
            if (!res.ok) throw new Error(answer.error || T('error', { status: res.status }));
            note.textContent = T('analyzedBy', { engine: answer.engine || 'Mark' }) + (answer.rejected ? T('rejectedLines', { n: answer.rejected }) : '') + '.';
            if (onDone) onDone();
            await load();   // la scheda si ricostruisce sul nodo aggiornato, con la descrizione
        } catch (e) {
            note.className = 'cg-warn';
            note.textContent = T('analysisFailed', { detail: e.message });
            if (btn) btn.disabled = false;
        }
    }

    /** «Nascosti: N» nell'intestazione: l'elenco, e il modo di rimetterli. */
    function renderHidden() {
        let chip = document.getElementById('cg-hidden');
        if (!chip) { chip = document.createElement('span'); chip.id = 'cg-hidden'; document.getElementById('cg-header').appendChild(chip); }
        if (!hidden.size) { chip.innerHTML = ''; return; }
        const esc = s => String(s == null ? '' : s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
        chip.innerHTML = '<button id="cg-hidden-btn" class="cg-chip">' + esc(T('hiddenChip', { n: hidden.size })) + ' ▾</button><div id="cg-hidden-list" class="hidden">' +
            Array.from(hidden, ([id, name]) => '<div><span>' + esc(name) + '</span> <button data-id="' + esc(id) + '" class="cg-btn cg-btn-secondary">' + esc(T('show')) + '</button></div>').join('') +
            '<div><button id="cg-hidden-all" class="cg-btn">' + esc(T('showAll')) + '</button></div></div>';
        document.getElementById('cg-hidden-btn').onclick = () => document.getElementById('cg-hidden-list').classList.toggle('hidden');
        chip.querySelectorAll('button[data-id]').forEach(b => b.onclick = () => { hidden.delete(b.dataset.id); saveHidden(); load(); });
        document.getElementById('cg-hidden-all').onclick = () => { hidden.clear(); saveHidden(); load(); };
    }

    function roundRect(ctx, x, y, w, h, r) {
        ctx.moveTo(x + r, y); ctx.lineTo(x + w - r, y); ctx.quadraticCurveTo(x + w, y, x + w, y + r);
        ctx.lineTo(x + w, y + h - r); ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
        ctx.lineTo(x + r, y + h); ctx.quadraticCurveTo(x, y + h, x, y + h - r);
        ctx.lineTo(x, y + r); ctx.quadraticCurveTo(x, y, x + r, y); ctx.closePath();
    }

    /** Apre o chiude un file (i suoi punti d'ingresso) o un simbolo (chi chiama). */
    function toggle(n) {
        if (n.kind === 'file') { if (expanded.has(n.path)) expanded.delete(n.path); else expanded.add(n.path); }
        else { if (opened.has(n.id)) opened.delete(n.id); else opened.add(n.id); }
        const u = new URL(location.href);
        u.searchParams.set('expand', Array.from(expanded).join(','));
        u.searchParams.set('open', Array.from(opened).join(','));
        history.replaceState(null, '', u.toString());
        load();
    }

    function select(n) {
        selected = n;
        const card = document.getElementById('cg-card');
        if (!n) { card.classList.add('hidden'); return; }
        card.classList.remove('hidden');
        const esc = s => String(s == null ? '' : s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
        let html = '<div class="cg-kind">' + esc(n.kind) + '</div><h3>' + esc(n.qualifiedName || n.name) + '</h3>';
        if (n.signature) html += '<code class="cg-sig">' + esc(n.signature) + '</code>';
        if (n.summary) html += '<p class="cg-summary">' + esc(n.summary) + '</p>' + (n.kind === 'module' ? '<p class="cg-muted">' + esc(T('moduleSummary')) + '</p>' : '');
        else if (n.kind === 'file') html += '<p class="cg-muted">' + esc(T('fileUndescribed')) + ' <button id="cg-describe" class="cg-btn cg-btn-secondary">' + esc(T('analyzeFile')) + '</button></p>';
        else if (n.kind === 'module') html += '<p class="cg-muted">' + esc(T('moduleUndescribed')) + ' <button id="cg-describe" class="cg-btn cg-btn-secondary">' + esc(T('analyzeSymbol')) + '</button></p>';
        else if (n.kind !== 'file' && !n.external) html += '<p class="cg-muted">' + esc(T('symbolUndescribed')) + ' <button id="cg-describe" class="cg-btn cg-btn-secondary">' + esc(T('analyzeSymbol')) + '</button></p>';
        if (n.uncalled) html += '<p class="cg-warn">' + esc(T('uncalled')) + '</p>';
        if (n.path) html += '<div class="cg-path">' + esc(n.path) + (n.line ? ':' + n.line : '') + '</div>';
        if (n.kind !== 'file' && !n.external) html += '<button id="cg-open" class="cg-btn">' + esc(T('openSource')) + '</button> ';
        if (n.kind === 'file') html += '<button id="cg-open" class="cg-btn">' + esc(T('openFile')) + '</button> <button id="cg-toggle" class="cg-btn">' + esc(n.expanded ? T('closeSymbols') : T('showSymbols')) + '</button>';
        if (n.kind !== 'file' && !n.external && hasToggle(n)) html += '<button id="cg-toggle" class="cg-btn cg-btn-secondary">' + esc(n.expanded ? T('closeCalls') : T('followCalls', { n: n.hidden })) + '</button> ';
        if (n.kind !== 'file' && !n.external) html += '<button id="cg-detail" class="cg-btn cg-btn-secondary">' + esc(T('details')) + '</button>';
        if (n.hiddenCalls && n.hiddenCalls.length)
            html += '<div class="cg-muted">' + esc(T('alsoCallsHidden')) + ' ' + n.hiddenCalls.map(h => '<code>' + esc(h) + '</code>').join(' ') + '</div>';
        if (n.externals && n.externals.length) {
            // ciò che è fuori dal perimetro non si disegna, ma si vede tutto qui: un nome sbagliato salta all'occhio
            const byRel = {};
            n.externals.forEach(x => { (byRel[x.relation] = byRel[x.relation] || []).push(x); });
            html += '<div class="cg-ext"><div class="cg-kind">' + esc(T('outOfScope', { n: n.externals.length })) + '</div>';
            Object.keys(byRel).forEach(rel => {
                html += '<div class="cg-ext-group"><strong>' + esc(CG_TEXTS.en['ext.' + rel] ? T('ext.' + rel) : rel) + '</strong> ' +
                    byRel[rel].map(x => '<code title="' + esc(x.qualifiedName + ' (' + x.confidence + ')') + '">' + esc(x.qualifiedName && x.qualifiedName.startsWith('.') ? x.qualifiedName : x.name) + '</code>').join(' ') + '</div>';
            });
            html += '</div>';
        }
        html += '<div id="cg-detail-body"></div>';
        card.innerHTML = html;
        const open = document.getElementById('cg-open');
        if (open) open.onclick = () => openSource(n);
        const tg = document.getElementById('cg-toggle'); if (tg) tg.onclick = () => toggle(n);
        const det = document.getElementById('cg-detail'); if (det) det.onclick = () => loadDetail(n);
        const desc = document.getElementById('cg-describe'); if (desc) desc.onclick = () => (n.kind === 'file' ? describeFile(n) : describeSymbol(n));
        if (graph) graph.refresh && graph.refresh();
    }

    async function loadDetail(n) {
        const body = document.getElementById('cg-detail-body');
        body.textContent = T('loadingShort');
        const u = new URL('/api/codegraph/symbols/' + encodeURIComponent(n.id), location.origin); u.searchParams.set('projectPath', projectPath);
        const res = await fetch(u); if (!res.ok) { body.textContent = T('error', { status: res.status }); return; }
        const s = await res.json();
        const esc = x => String(x == null ? '' : x).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
        let html = '';
        if (s.parameters && s.parameters.length) {
            html += '<table class="cg-params"><thead><tr><th>' + esc(T('parameter')) + '</th><th>' + esc(T('type')) + '</th><th>' + esc(T('description')) + '</th></tr></thead><tbody>';
            s.parameters.forEach(p => { html += '<tr><td>' + esc(p.name) + (p.direction && p.direction !== 'in' ? ' <em>' + esc(p.direction) + '</em>' : '') + '</td><td><code>' + esc(p.type) + '</code>' + (p.defaultValue != null ? ' = ' + esc(p.defaultValue) : '') + '</td><td>' + esc(p.description || '—') + '</td></tr>'; });
            html += '</tbody></table>';
        }
        if (s.returnType) html += '<div><strong>' + esc(T('returns')) + '</strong> <code>' + esc(s.returnType) + '</code>' + (s.returnsDescription ? ': ' + esc(s.returnsDescription) : '') + '</div>';
        if (s.remarks) html += '<div class="cg-remarks">⚠ ' + esc(s.remarks) + '</div>';
        if (s.descriptionStale) html += '<div class="cg-stale">' + esc(T('staleDescription')) + '</div>';
        if (s.writtenBy) html += '<div class="cg-muted">' + esc(s.writtenBy) + '</div>';
        body.innerHTML = html || '<div class="cg-muted">' + esc(T('nothingElse')) + '</div>';
    }

    function renderLegend() {
        const el = document.getElementById('cg-legend');
        el.innerHTML = Object.keys(REL_COLORS).filter(k => k !== 'contains').map(k => '<span title="' + k + '"><i style="background:' + REL_COLORS[k] + '"></i>' + relLabel(k) + '</span>').join('') +
            '<span class="cg-sep"></span><span title="extracted">— ' + confLabel('extracted') + '</span><span title="inferred">– – ' + confLabel('inferred') + '</span><span title="ambiguous">·· ' + confLabel('ambiguous') + '</span>' +
            '<span class="cg-sep"></span><span><i class="cg-uncalled"></i>' + T('legendUncalled') + '</span>' +
            (data.folders.length > 1 ? '<span class="cg-sep"></span>' + data.folders.map(f => '<span><i style="background:' + folderColor(f) + '"></i>' + (f || '/') + '</span>').join('') : '');
    }

    /**
     * D25: gli script scritti da una versione precedente dell'estrattore non si rifanno da soli all'apertura del
     * progetto. Qui si dice, prima di guardare il grafo, che per quei file il grafo è quello vecchio, e si chiede.
     */
    async function checkOutdated() {
        const banner = document.getElementById('cg-banner');
        const u = new URL('/api/codegraph/outdated', location.origin);
        u.searchParams.set('projectPath', projectPath); u.searchParams.set('folder', folder);
        let info;
        try { const res = await fetch(u); if (!res.ok) throw new Error(T('error', { status: res.status }) + ': ' + await res.text()); info = await res.json(); }
        catch (e) { banner.className = 'cg-banner-error'; banner.textContent = T('cannotCheckOutdated', { detail: e.message }); return; }
        if (!info.count) { banner.className = 'hidden'; return; }
        const esc = s => String(s == null ? '' : s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
        const what = info.changes.map(c => esc(c.language) + ': ' + esc(c.from) + ' → ' + esc(c.to) + ' (' + esc(T('statusFiles', { n: c.files })) + ')').join(', ');
        banner.className = '';
        banner.innerHTML = '<strong>⚠ ' + esc(T(info.count === 1 ? 'outdatedOne' : 'outdatedMany', { n: info.count })) + '</strong> — ' + what +
            '. ' + esc(T('outdatedOld')) +
            '<div class="cg-banner-actions"><button id="cg-upgrade" class="cg-btn">' + esc(T('upgrade')) + '</button> ' +
            '<button id="cg-upgrade-list" class="cg-btn cg-btn-secondary">' + esc(T('whichFiles')) + '</button></div><div id="cg-upgrade-files" class="hidden"></div>';
        document.getElementById('cg-upgrade-list').onclick = () => {
            const box = document.getElementById('cg-upgrade-files');
            box.innerHTML = info.scripts.map(s => '<code>' + esc(s.source) + '</code>').join(' ');
            box.classList.toggle('hidden');
        };
        document.getElementById('cg-upgrade').onclick = async () => {
            const btn = document.getElementById('cg-upgrade');
            btn.disabled = true; btn.textContent = T('upgrading');
            try {
                const res = await fetch('/api/codegraph/outdated/upgrade', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ projectPath, folder }) });
                if (!res.ok) throw new Error(T('error', { status: res.status }) + ': ' + await res.text());
                const done = await res.json();
                banner.className = 'cg-banner-ok';
                banner.textContent = T(done.reparsed === 1 ? 'upgradedOne' : 'upgradedMany', { n: done.reparsed });
                setTimeout(() => { banner.className = 'hidden'; }, 4000);
                await load();
                checkOutdated();
            } catch (e) {
                banner.className = 'cg-banner-error';
                banner.textContent = T('upgradeFailed', { detail: e.message });
            }
        };
    }

    document.addEventListener('DOMContentLoaded', () => { load(); checkOutdated(); });
})();
