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
 * - Disegno ad albero dall'alto in basso (D26), CALCOLATO e non simulato: i file sulla prima riga, sotto ogni nodo
 *   in fila quelli che ha fatto comparire, un livello più giù; niente sovrapposizioni, niente nodi che scappano. Un
 *   nodo trascinato resta dove lo si lascia; se dopo un'apertura qualcosa esce dallo schermo, la vista si adatta.
 * - Gli archi hanno un colore per relazione e sono tratteggiati quando la confidenza è `inferred`
 *   o `ambiguous`; fra due file si sommano (il numero sull'arco).
 * - Il clic su un simbolo apre la scheda con firma, parametri e la descrizione scritta dalla skill;
 *   «Apri il sorgente» naviga il visualizzatore sul file (messaggio `md-navigate` al genitore).
 * - I dati arrivano da GET /api/codegraph/graph (D15: solo il DB); niente sorgenti letti qui.
 */
(function () {
    'use strict';
    const qs = new URLSearchParams(location.search);
    const projectPath = qs.get('projectPath') || '';
    const folder = qs.get('folder') || '';
    const focus = qs.get('symbol') || '';
    let isDark = false;   // si legge quando il body esiste (lo script sta nell'head)
    const expanded = new Set((qs.get('expand') || '').split(',').filter(Boolean));
    const opened = new Set((qs.get('open') || '').split(',').filter(Boolean));
    const LEVEL_GAP = 110; // distanza verticale fra un livello e il successivo
    const SIBLING_GAP = 28; // spazio orizzontale fra due nodi vicini
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
        status.textContent = 'Carico il grafo…';
        let res;
        try { res = await fetch(apiUrl()); } catch (e) { status.textContent = 'MdExplorer non risponde: ' + e.message; return; }
        if (!res.ok) { status.textContent = 'Errore ' + res.status + ': ' + (await res.text()); return; }
        data = await res.json();
        if (!data.nodes.length) {
            status.textContent = 'Nessun simbolo per «' + (folder || 'tutto il progetto') + '»: apri «Ambiente di sviluppo» sulla cartella e lancia l\'analisi.';
            return;
        }
        const syms = data.nodes.filter(n => n.kind !== 'file' && !n.external);
        const uncalled = syms.filter(n => n.uncalled).length;
        const undescribed = syms.filter(n => !n.summary && n.kind !== 'module').length;
        status.textContent = data.nodes.filter(n => n.kind === 'file').length + ' file, ' + syms.length + ' simboli mostrati, ' + data.links.length + ' archi' +
            (uncalled ? ' · ' + uncalled + ' che nessuno chiama' : '') + (undescribed ? ' · ' + undescribed + ' senza descrizione' : '');
        render();
        renderLegend();
        renderHidden();
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
                const w = (n.__w || 60) + 24 / scale, h = n.__h || 20 / scale;
                ctx.fillStyle = color; ctx.fillRect(n.x - w / 2 + 0, n.y - h / 2, w, h);
            })
            .linkColor(l => REL_COLORS[l.relation] || '#94a3b8')
            // fra nodi della stessa riga la linea dritta passerebbe sopra i vicini: si curva
            .linkCurvature(l => (l.source && l.target && l.source.depth === l.target.depth) ? 0.35 : 0)
            .linkWidth(l => Math.min(4, 1 + Math.log2(l.count || 1)))
            .linkLineDash(l => l.confidence === 'extracted' ? null : l.confidence === 'inferred' ? [6, 4] : [2, 4])
            .linkDirectionalArrowLength(6).linkDirectionalArrowRelPos(1)
            .linkLabel(l => l.relation + ' (' + l.confidence + (l.count > 1 ? ', ×' + l.count : '') + ')')
            .onNodeClick((n, ev) => {
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
     * L'albero: ogni nodo pende da chi l'ha fatto comparire (parentId: il file per un punto d'ingresso, il chiamante per
     * gli altri); i file senza padre sono le radici, in fila sulla prima riga. Le foglie prendono il posto successivo
     * da sinistra, un genitore sta sopra il centro dei suoi figli; y = livello × LEVEL_GAP. Deterministico: aprire un
     * nodo allarga il suo ramo, ma l'ordine resta quello.
     */
    function layoutTree(nodes, byId) {
        const measure = document.createElement('canvas').getContext('2d');
        measure.font = '600 12px -apple-system, "Segoe UI", Inter, system-ui, sans-serif';
        const width = n => measure.measureText(labelOf(n)).width + 22 + (hasToggle(n) ? 30 : 0);
        const kids = {};
        const roots = [];
        nodes.forEach(n => {
            if (n.parentId && byId[n.parentId] && n.parentId !== n.id) (kids[n.parentId] = kids[n.parentId] || []).push(n);
            else roots.push(n);
        });
        let cursor = 0;
        const place = n => {
            const ch = kids[n.id] || [];
            const w = width(n);
            if (!ch.length) { n.__x = cursor + w / 2; cursor += w + SIBLING_GAP; return; }
            const start = cursor;
            ch.forEach(place);
            n.__x = (ch[0].__x + ch[ch.length - 1].__x) / 2;
            // un genitore più largo dei suoi figli non deve invadere il vicino
            if (cursor - start < w + SIBLING_GAP) { n.__x = start + w / 2; cursor = start + w + SIBLING_GAP; }
        };
        roots.forEach(place);
        const shift = cursor / 2;   // l'albero centrato sull'origine
        nodes.forEach(n => {
            const d = dragged.get(n.id);
            n.x = n.fx = d ? d.x : n.__x - shift;
            n.y = n.fy = d ? d.y : (n.depth || 0) * LEVEL_GAP;
        });
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
            if (outside || firstFit) graph.zoomToFit(firstFit ? 0 : 400, 60);
            firstFit = false;
        }, 300);
    }

    // ---- tasto destro

    function hideMenu() { const m = document.getElementById('cg-menu'); if (m) m.remove(); }

    function showMenu(n, ev) {
        hideMenu();
        if (n.kind === 'file' || n.external) return;
        const menu = document.createElement('div');
        menu.id = 'cg-menu';
        const items = [['hide', 'Nascondi «' + n.name + '» dal grafo']];
        if (hasToggle(n)) items.push(['toggle', n.expanded ? 'Chiudi le chiamate' : 'Segui le chiamate (' + n.hidden + ')']);
        items.push(['open', 'Apri il sorgente']);
        items.forEach(([action, label]) => {
            const b = document.createElement('button');
            b.textContent = label;
            b.onclick = () => {
                hideMenu();
                if (action === 'hide') { hidden.set(n.id, n.name); saveHidden(); if (selected && selected.id === n.id) select(null); load(); }
                else if (action === 'toggle') toggle(n);
                else window.parent.postMessage({ type: 'md-navigate', relativePath: n.path, name: n.path.split('/').pop(), line: n.line || null }, '*');
            };
            menu.appendChild(b);
        });
        document.body.appendChild(menu);
        const x = Math.min(ev.clientX, innerWidth - menu.offsetWidth - 8), y = Math.min(ev.clientY, innerHeight - menu.offsetHeight - 8);
        menu.style.left = x + 'px'; menu.style.top = y + 'px';
    }

    /** «Nascosti: N» nell'intestazione: l'elenco, e il modo di rimetterli. */
    function renderHidden() {
        let chip = document.getElementById('cg-hidden');
        if (!chip) { chip = document.createElement('span'); chip.id = 'cg-hidden'; document.getElementById('cg-header').appendChild(chip); }
        if (!hidden.size) { chip.innerHTML = ''; return; }
        const esc = s => String(s == null ? '' : s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
        chip.innerHTML = '<button id="cg-hidden-btn" class="cg-chip">Nascosti: ' + hidden.size + ' ▾</button><div id="cg-hidden-list" class="hidden">' +
            Array.from(hidden, ([id, name]) => '<div><span>' + esc(name) + '</span> <button data-id="' + esc(id) + '" class="cg-btn cg-btn-secondary">Mostra</button></div>').join('') +
            '<div><button id="cg-hidden-all" class="cg-btn">Mostra tutti</button></div></div>';
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
        if (n.summary) html += '<p class="cg-summary">' + esc(n.summary) + '</p>';
        else if (n.kind === 'module') html += '<p class="cg-muted">Lo script stesso: il codice fuori dalle funzioni, quello che gira quando lo si lancia. I moduli non si descrivono; si descrivono le funzioni che chiama.</p>';
        else if (n.kind !== 'file' && !n.external) html += '<p class="cg-muted">Nessuna descrizione: «Descrivi il codice con Mark» nella finestra Ambiente di sviluppo.</p>';
        if (n.uncalled) html += '<p class="cg-warn">Nessun simbolo del perimetro la chiama: è codice morto, oppure un punto d\'ingresso (chiamata da fuori, da un evento, per riflessione).</p>';
        if (n.path) html += '<div class="cg-path">' + esc(n.path) + (n.line ? ':' + n.line : '') + '</div>';
        if (n.kind !== 'file' && !n.external) html += '<button id="cg-open" class="cg-btn">Apri il sorgente</button> ';
        if (n.kind === 'file') html += '<button id="cg-open" class="cg-btn">Apri il file</button> <button id="cg-toggle" class="cg-btn">' + (n.expanded ? 'Chiudi i simboli' : 'Mostra i simboli') + '</button>';
        if (n.kind !== 'file' && !n.external && hasToggle(n)) html += '<button id="cg-toggle" class="cg-btn cg-btn-secondary">' + (n.expanded ? 'Chiudi le chiamate' : 'Segui le chiamate (' + n.hidden + ')') + '</button> ';
        if (n.kind !== 'file' && !n.external) html += '<button id="cg-detail" class="cg-btn cg-btn-secondary">Dettagli</button>';
        if (n.hiddenCalls && n.hiddenCalls.length)
            html += '<div class="cg-muted">Chiama anche (nascoste): ' + n.hiddenCalls.map(h => '<code>' + esc(h) + '</code>').join(' ') + '</div>';
        if (n.externals && n.externals.length) {
            // ciò che è fuori dal perimetro non si disegna, ma si vede tutto qui: un nome sbagliato salta all'occhio
            const byRel = {};
            n.externals.forEach(x => { (byRel[x.relation] = byRel[x.relation] || []).push(x); });
            const REL_IT = { calls: 'Chiama fuori dal progetto', instantiates: 'Crea oggetti esterni', references: 'Usa tipi esterni', inherits: 'Eredita da', implements: 'Implementa', imports: 'Importa' };
            html += '<div class="cg-ext"><div class="cg-kind">Fuori dal perimetro · ' + n.externals.length + '</div>';
            Object.keys(byRel).forEach(rel => {
                html += '<div class="cg-ext-group"><strong>' + esc(REL_IT[rel] || rel) + '</strong> ' +
                    byRel[rel].map(x => '<code title="' + esc(x.qualifiedName + ' (' + x.confidence + ')') + '">' + esc(x.qualifiedName && x.qualifiedName.startsWith('.') ? x.qualifiedName : x.name) + '</code>').join(' ') + '</div>';
            });
            html += '</div>';
        }
        html += '<div id="cg-detail-body"></div>';
        card.innerHTML = html;
        const open = document.getElementById('cg-open');
        if (open) open.onclick = () => window.parent.postMessage({ type: 'md-navigate', relativePath: n.path, name: n.path.split('/').pop(), line: n.line || null }, '*');
        const tg = document.getElementById('cg-toggle'); if (tg) tg.onclick = () => toggle(n);
        const det = document.getElementById('cg-detail'); if (det) det.onclick = () => loadDetail(n);
        if (graph) graph.refresh && graph.refresh();
    }

    async function loadDetail(n) {
        const body = document.getElementById('cg-detail-body');
        body.textContent = 'Carico…';
        const u = new URL('/api/codegraph/symbols/' + encodeURIComponent(n.id), location.origin); u.searchParams.set('projectPath', projectPath);
        const res = await fetch(u); if (!res.ok) { body.textContent = 'Errore ' + res.status; return; }
        const s = await res.json();
        const esc = x => String(x == null ? '' : x).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
        let html = '';
        if (s.parameters && s.parameters.length) {
            html += '<table class="cg-params"><thead><tr><th>Parametro</th><th>Tipo</th><th>Descrizione</th></tr></thead><tbody>';
            s.parameters.forEach(p => { html += '<tr><td>' + esc(p.name) + (p.direction && p.direction !== 'in' ? ' <em>' + esc(p.direction) + '</em>' : '') + '</td><td><code>' + esc(p.type) + '</code>' + (p.defaultValue != null ? ' = ' + esc(p.defaultValue) : '') + '</td><td>' + esc(p.description || '—') + '</td></tr>'; });
            html += '</tbody></table>';
        }
        if (s.returnType) html += '<div><strong>Ritorna</strong> <code>' + esc(s.returnType) + '</code>' + (s.returnsDescription ? ': ' + esc(s.returnsDescription) : '') + '</div>';
        if (s.remarks) html += '<div class="cg-remarks">⚠ ' + esc(s.remarks) + '</div>';
        if (s.descriptionStale) html += '<div class="cg-stale">La descrizione è di una firma precedente: da rivedere.</div>';
        if (s.writtenBy) html += '<div class="cg-muted">' + esc(s.writtenBy) + '</div>';
        body.innerHTML = html || '<div class="cg-muted">Niente altro da mostrare.</div>';
    }

    function renderLegend() {
        const el = document.getElementById('cg-legend');
        el.innerHTML = Object.keys(REL_COLORS).filter(k => k !== 'contains').map(k => '<span><i style="background:' + REL_COLORS[k] + '"></i>' + k + '</span>').join('') +
            '<span class="cg-sep"></span><span>— extracted</span><span>– – inferred</span><span>·· ambiguous</span>' +
            '<span class="cg-sep"></span><span><i class="cg-uncalled"></i>nessuno la chiama</span>' +
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
        try { const res = await fetch(u); if (!res.ok) throw new Error('Errore ' + res.status + ': ' + await res.text()); info = await res.json(); }
        catch (e) { banner.className = 'cg-banner-error'; banner.textContent = 'Non riesco a controllare se il grafo è aggiornato: ' + e.message; return; }
        if (!info.count) { banner.className = 'hidden'; return; }
        const esc = s => String(s == null ? '' : s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
        const what = info.changes.map(c => esc(c.language) + ': ' + esc(c.from) + ' → ' + esc(c.to) + ' (' + c.files + ' file)').join(', ');
        banner.className = '';
        banner.innerHTML = '<strong>⚠ ' + info.count + (info.count === 1 ? ' file analizzato' : ' file analizzati') + ' con una versione precedente dell\'estrattore</strong> — ' + what +
            '. Per questi file stai guardando il grafo vecchio.' +
            '<div class="cg-banner-actions"><button id="cg-upgrade" class="cg-btn">Aggiorna l\'analisi</button> ' +
            '<button id="cg-upgrade-list" class="cg-btn cg-btn-secondary">Quali file</button></div><div id="cg-upgrade-files" class="hidden"></div>';
        document.getElementById('cg-upgrade-list').onclick = () => {
            const box = document.getElementById('cg-upgrade-files');
            box.innerHTML = info.scripts.map(s => '<code>' + esc(s.source) + '</code>').join(' ');
            box.classList.toggle('hidden');
        };
        document.getElementById('cg-upgrade').onclick = async () => {
            const btn = document.getElementById('cg-upgrade');
            btn.disabled = true; btn.textContent = 'Aggiorno l\'analisi…';
            try {
                const res = await fetch('/api/codegraph/outdated/upgrade', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ projectPath, folder }) });
                if (!res.ok) throw new Error('Errore ' + res.status + ': ' + await res.text());
                const done = await res.json();
                banner.className = 'cg-banner-ok';
                banner.textContent = 'Analisi aggiornata: ' + done.reparsed + (done.reparsed === 1 ? ' file riparsato.' : ' file riparsati.');
                setTimeout(() => { banner.className = 'hidden'; }, 4000);
                await load();
                checkOutdated();
            } catch (e) {
                banner.className = 'cg-banner-error';
                banner.textContent = 'Aggiornamento fallito: ' + e.message;
            }
        };
    }

    document.addEventListener('DOMContentLoaded', () => { load(); checkOutdated(); });
})();
