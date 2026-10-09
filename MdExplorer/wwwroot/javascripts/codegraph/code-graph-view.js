/**
 * Il grafo del codice come pagina del visualizzatore (sprint 2026-10-09, D17, F6).
 *
 * - I nodi di primo livello sono i FILE della cartella (box), raggruppati per sottocartella (colore);
 *   «+» su un file apre i suoi simboli (classi, metodi…), «−» li richiude.
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
        status.textContent = data.nodes.filter(n => n.kind === 'file').length + ' file, ' +
            data.nodes.filter(n => n.kind !== 'file' && !n.external).length + ' simboli mostrati, ' + data.links.length + ' archi';
        render();
        renderLegend();
        if (focus) { const n = data.nodes.find(x => x.id === focus); if (n) { select(n); setTimeout(() => graph.centerAt(n.x, n.y, 600), 400); } }
    }

    function nodeWidth(n, ctx, scale) {
        const label = labelOf(n);
        ctx.font = (n.kind === 'file' ? '600 ' : '') + (12 / scale) + 'px -apple-system, "Segoe UI", Inter, system-ui, sans-serif';
        return ctx.measureText(label).width + 22 / scale;
    }
    function labelOf(n) { return (KIND_ICON[n.kind] || '•') + ' ' + n.name; }

    function render() {
        const el = document.getElementById('cg-graph');
        el.innerHTML = '';
        const nodes = data.nodes.map(n => Object.assign({}, n));
        const byId = Object.fromEntries(nodes.map(n => [n.id, n]));
        const links = data.links.filter(l => byId[l.source] && byId[l.target]).map(l => Object.assign({}, l));
        // i simboli di un file nascono accanto al loro file
        nodes.forEach(n => { if (n.parentId && byId[n.parentId]) { n.fx = undefined; } });

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
                const stroke = n === selected ? '#facc15' : isFile ? 'rgba(15,23,42,0.35)' : folderColor(n.folder);
                ctx.beginPath(); roundRect(ctx, n.x - w / 2, n.y - h / 2, w, h, r);
                ctx.fillStyle = fill; ctx.fill();
                ctx.lineWidth = (n === selected ? 3 : 1.2) / scale; ctx.strokeStyle = stroke; ctx.stroke();
                ctx.fillStyle = isFile ? '#0f172a' : (isDark ? '#e6edf3' : '#0f172a');
                ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
                ctx.fillText(labelOf(n), n.x, n.y);
                if (isFile && n.hidden > 0) {
                    // il «+» / «−» a destra del box
                    const bx = n.x + w / 2 + 9 / scale, br = 7 / scale;
                    ctx.beginPath(); ctx.arc(bx, n.y, br, 0, 2 * Math.PI);
                    ctx.fillStyle = n.expanded ? '#ef4444' : '#22c55e'; ctx.fill();
                    ctx.fillStyle = '#fff'; ctx.font = 'bold ' + (11 / scale) + 'px sans-serif';
                    ctx.fillText(n.expanded ? '−' : '+', bx, n.y + 0.5 / scale);
                    n.__btn = { x: bx, r: br };
                }
                n.__w = w; n.__h = h;
            })
            .nodePointerAreaPaint((n, color, ctx, scale) => {
                const w = (n.__w || 60) + 24 / scale, h = n.__h || 20 / scale;
                ctx.fillStyle = color; ctx.fillRect(n.x - w / 2 + 0, n.y - h / 2, w, h);
            })
            .linkColor(l => REL_COLORS[l.relation] || '#94a3b8')
            .linkWidth(l => Math.min(4, 1 + Math.log2(l.count || 1)))
            .linkLineDash(l => l.confidence === 'extracted' ? null : l.confidence === 'inferred' ? [6, 4] : [2, 4])
            .linkDirectionalArrowLength(6).linkDirectionalArrowRelPos(1)
            .linkLabel(l => l.relation + ' (' + l.confidence + (l.count > 1 ? ', ×' + l.count : '') + ')')
            .onNodeClick((n, ev) => {
                if (n.kind === 'file' && n.__btn) {
                    const p = graph.screen2GraphCoords(ev.offsetX, ev.offsetY);
                    if (Math.hypot(p.x - n.__btn.x, p.y - n.__btn.y) <= n.__btn.r * 1.5) { toggle(n); return; }
                }
                select(n);
            })
            .onBackgroundClick(() => select(null));
        // d3Force(name) torna la forza, non il grafo: si configura DOPO aver tenuto il riferimento al grafo.
        graph.d3Force('charge').strength(-420).distanceMax(600);
        graph.d3Force('link').distance(l => (l.source.kind === 'file' && l.target.kind === 'file') ? 160 : 90);
        graph.d3VelocityDecay(0.3);
        window.addEventListener('resize', () => graph.width(el.clientWidth).height(el.clientHeight));
    }

    function roundRect(ctx, x, y, w, h, r) {
        ctx.moveTo(x + r, y); ctx.lineTo(x + w - r, y); ctx.quadraticCurveTo(x + w, y, x + w, y + r);
        ctx.lineTo(x + w, y + h - r); ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
        ctx.lineTo(x + r, y + h); ctx.quadraticCurveTo(x, y + h, x, y + h - r);
        ctx.lineTo(x, y + r); ctx.quadraticCurveTo(x, y, x + r, y); ctx.closePath();
    }

    function toggle(fileNode) {
        if (expanded.has(fileNode.path)) expanded.delete(fileNode.path); else expanded.add(fileNode.path);
        const u = new URL(location.href); u.searchParams.set('expand', Array.from(expanded).join(','));
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
        else if (n.kind !== 'file' && !n.external) html += '<p class="cg-muted">Nessuna descrizione: «Descrivi il codice con Mark» nella finestra Ambiente di sviluppo.</p>';
        if (n.path) html += '<div class="cg-path">' + esc(n.path) + (n.line ? ':' + n.line : '') + '</div>';
        if (n.kind !== 'file' && !n.external) html += '<button id="cg-open" class="cg-btn">Apri il sorgente</button> ';
        if (n.kind === 'file') html += '<button id="cg-open" class="cg-btn">Apri il file</button> <button id="cg-toggle" class="cg-btn">' + (n.expanded ? 'Chiudi i simboli' : 'Mostra i simboli') + '</button>';
        if (n.kind !== 'file' && !n.external) html += '<button id="cg-detail" class="cg-btn cg-btn-secondary">Dettagli</button>';
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
            (data.folders.length > 1 ? '<span class="cg-sep"></span>' + data.folders.map(f => '<span><i style="background:' + folderColor(f) + '"></i>' + (f || '/') + '</span>').join('') : '');
    }

    document.addEventListener('DOMContentLoaded', load);
})();
