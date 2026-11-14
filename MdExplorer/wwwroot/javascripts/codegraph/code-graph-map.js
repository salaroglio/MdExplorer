/**
 * La mappa per livelli del grafo del codice (vista guidata, passo 2 del 14/10/2026). Mai migliaia di nodi: al livello
 * del progetto i programmi, dentro una cartella le sue sottocartelle (le catene con un figlio solo compattate), con le
 * frecce SOMMATE fra le parti — quante, di che relazione, con che certezza. Clic = si entra; tasto destro = «Mostra i
 * file» (la vista a scaletta di code-graph-view.js) o «Chiedi a MarkAgent di questa parte». I numeri li dà il backend
 * (GET /api/codegraph/map), non si stimano qui.
 *
 * Modo della pagina: ?view=map (predefinito) o ?view=files; con ?symbol=… si apre sui file, come prima.
 */
const CG_MAP_TEXTS = {
    it: {
        project: 'Progetto',
        loading: 'Leggo la mappa…',
        error: 'La mappa non si legge ({status})',
        empty: 'Niente da mostrare qui: la cartella non ha file analizzati.',
        status: '{parts} parti · {files} file · {symbols} simboli',
        statusMore: ' · {n} cartelle raccolte in «+{n} altre»',
        files: '{n} file',
        symbols: '{n} simboli',
        loose: 'File di questa cartella',
        more: '+{n} altre cartelle',
        outside: 'Fuori da qui',
        showFiles: '📄 Mostra i file',
        showFilesTip: 'La vista a scaletta dei {n} file di questa cartella, con i loro simboli',
        backToMap: '🗺 Mappa',
        backToMapTip: 'Torna alla mappa per livelli',
        enter: 'Entra in «{name}»',
        openFiles: 'Mostra i file di «{name}»',
        askPart: 'Chiedi a MarkAgent di «{name}»',
        goUp: 'Torna su',
        arrow: '{from} → {to}: {n} legami',
        arrowCertain: '{n} certi',
        arrowInferred: '{n} dedotti',
        arrowAmbiguous: '{n} ambigui',
        arrowLlm: '{n} scelti dall\'LLM',
        legend: 'Frecce: legami sommati fra le parti (chiamate, tipi usati, ereditarietà, implementazioni, creazioni; senza gli import) · spessore = quanti · tratteggio = per lo più non certi',
        moreTitle: 'Le cartelle raccolte',
    },
    en: {
        project: 'Project',
        loading: 'Reading the map…',
        error: 'The map cannot be read ({status})',
        empty: 'Nothing to show here: the folder has no analysed files.',
        status: '{parts} parts · {files} files · {symbols} symbols',
        statusMore: ' · {n} folders gathered in “+{n} more”',
        files: '{n} files',
        symbols: '{n} symbols',
        loose: 'Files of this folder',
        more: '+{n} more folders',
        outside: 'Outside of here',
        showFiles: '📄 Show the files',
        showFilesTip: 'The step view of the {n} files of this folder, with their symbols',
        backToMap: '🗺 Map',
        backToMapTip: 'Back to the level map',
        enter: 'Enter “{name}”',
        openFiles: 'Show the files of “{name}”',
        askPart: 'Ask MarkAgent about “{name}”',
        goUp: 'Go up',
        arrow: '{from} → {to}: {n} links',
        arrowCertain: '{n} certain',
        arrowInferred: '{n} inferred',
        arrowAmbiguous: '{n} ambiguous',
        arrowLlm: '{n} chosen by the LLM',
        legend: 'Arrows: links summed between the parts (calls, used types, inheritance, implementations, creations; no imports) · width = how many · dashed = mostly not certain',
        moreTitle: 'The gathered folders',
    },
};

(function () {
    'use strict';
    const qs = new URLSearchParams(location.search);
    const MODE = qs.get('view') || (qs.get('symbol') ? 'files' : 'map');
    if (MODE !== 'map') {
        // nella vista a file: un pulsante per tornare alla mappa della stessa cartella
        document.addEventListener('DOMContentLoaded', () => {
            const lang = CG_MAP_TEXTS[document.documentElement.lang] ? document.documentElement.lang : 'en';
            const b = document.createElement('button');
            b.className = 'cg-chip'; b.id = 'cg-to-map';
            b.textContent = CG_MAP_TEXTS[lang].backToMap; b.title = CG_MAP_TEXTS[lang].backToMapTip;
            b.onclick = () => go(qs.get('folder') || '', 'map');
            document.getElementById('cg-header').insertBefore(b, document.getElementById('cg-status'));
        });
        return;
    }

    const LANG = CG_MAP_TEXTS[document.documentElement.lang] ? document.documentElement.lang : 'en';
    function T(key, params) {
        const text = CG_MAP_TEXTS[LANG][key] || CG_MAP_TEXTS.en[key] || key;
        return params ? text.replace(/\{(\w+)\}/g, (whole, name) => (params[name] !== undefined ? params[name] : whole)) : text;
    }
    const projectPath = qs.get('projectPath') || '';
    const folder = qs.get('folder') || '';
    const nf = new Intl.NumberFormat(LANG);
    const KIND_ICON = { program: '📦', folder: '📁', files: '📄', more: '➕', outside: '↗' };
    const REL_TEXT = r => (typeof CG_TEXTS !== 'undefined' && CG_TEXTS[LANG] && CG_TEXTS[LANG]['rel.' + r]) || r;
    let map = null, graph = null;

    /** La stessa pagina su un'altra cartella o in un altro modo: l'URL resta il posto unico dello stato. */
    function go(target, view) {
        const u = new URL(location.href);
        u.searchParams.set('folder', target || '');
        u.searchParams.set('view', view);
        ['symbol', 'expand', 'open', 'desc'].forEach(k => u.searchParams.delete(k));
        location.href = u.toString();
    }

    function ask(target) {
        window.parent.postMessage({ type: 'mde-mark.askAboutGraph', context: { projectPath, folder: target || '' } }, '*');
    }

    function labelOf(g) {
        if (g.kind === 'files') return T('loose');
        if (g.kind === 'more') return T('more', { n: map.hiddenGroups });
        if (g.kind === 'outside') return T('outside');
        return g.label;
    }
    function subOf(g) {
        if (g.kind === 'outside') return T('goUp');
        return T('files', { n: nf.format(g.files) }) + ' · ' + T('symbols', { n: nf.format(g.symbols) });
    }

    function renderHeader() {
        const h2 = document.querySelector('#cg-header h2');
        h2.textContent = '';
        const crumbs = [[T('project'), '']];
        let acc = '';
        (map.folder || '').split('/').filter(Boolean).forEach(seg => { acc = acc ? acc + '/' + seg : seg; crumbs.push([seg, acc]); });
        crumbs.forEach(([label, target], i) => {
            if (i > 0) h2.appendChild(document.createTextNode(' › '));
            if (i === crumbs.length - 1) { const s = document.createElement('span'); s.textContent = label; h2.appendChild(s); return; }
            const a = document.createElement('a');
            a.href = '#'; a.textContent = label; a.className = 'cg-crumb';
            a.onclick = e => { e.preventDefault(); go(target, 'map'); };
            h2.appendChild(a);
        });
        const status = document.getElementById('cg-status');
        status.textContent = T('status', { parts: map.groups.filter(g => g.kind !== 'outside').length, files: nf.format(map.files), symbols: nf.format(map.symbols) }) +
            (map.hiddenGroups ? T('statusMore', { n: map.hiddenGroups }) : '');
        const header = document.getElementById('cg-header');
        if (map.level === 'folders' && map.files > 0) {
            const b = document.createElement('button');
            b.className = 'cg-chip'; b.textContent = T('showFiles'); b.title = T('showFilesTip', { n: nf.format(map.files) });
            b.onclick = () => go(map.folder, 'files');
            header.appendChild(b);
        }
        const askBtn = document.createElement('button');
        askBtn.className = 'cg-chip'; askBtn.id = 'cg-ask-graph';
        askBtn.textContent = (CG_TEXTS[LANG] || CG_TEXTS.en).askGraph; askBtn.title = (CG_TEXTS[LANG] || CG_TEXTS.en).askGraphTip;
        askBtn.onclick = () => ask(map.folder);
        header.appendChild(askBtn);
        document.getElementById('cg-legend').textContent = T('legend');
    }

    function tooltip(l) {
        const from = labelOf(l.source), to = labelOf(l.target);
        const rel = Object.entries(l.byRelation).sort((a, b) => b[1] - a[1]).map(([r, n]) => REL_TEXT(r) + ' ' + nf.format(n)).join(', ');
        const c = l.byConfidence;
        const conf = [c.extracted ? T('arrowCertain', { n: nf.format(c.extracted) }) : null, c.inferred ? T('arrowInferred', { n: nf.format(c.inferred) }) : null,
            c['llm-resolved'] ? T('arrowLlm', { n: nf.format(c['llm-resolved']) }) : null,
            c.ambiguous ? T('arrowAmbiguous', { n: nf.format(c.ambiguous) }) : null].filter(Boolean).join(', ');
        return T('arrow', { from, to, n: nf.format(l.count) }) + '<br>' + rel + '<br>' + conf;
    }

    function showMore(ev) {
        const card = document.getElementById('cg-card');
        card.innerHTML = '';
        const h = document.createElement('h3'); h.textContent = T('moreTitle'); card.appendChild(h);
        map.more.forEach(g => {
            const b = document.createElement('button');
            b.className = 'cg-btn cg-btn-secondary';
            b.textContent = '📁 ' + g.label + ' · ' + T('files', { n: nf.format(g.files) });
            b.onclick = () => go(g.folder, 'map');
            card.appendChild(b);
        });
        card.classList.remove('hidden');
    }

    function hideMenu() { const m = document.getElementById('cg-menu'); if (m) m.remove(); }
    function openMenu(items, ev) {
        hideMenu();
        const menu = document.createElement('div');
        menu.id = 'cg-menu';
        items.forEach(([label, run]) => {
            const b = document.createElement('button');
            b.textContent = label; b.onclick = () => { hideMenu(); run(); };
            menu.appendChild(b);
        });
        document.body.appendChild(menu);
        menu.style.left = Math.min(ev.clientX, innerWidth - menu.offsetWidth - 8) + 'px';
        menu.style.top = Math.min(ev.clientY, innerHeight - menu.offsetHeight - 8) + 'px';
    }

    function activate(g, ev) {
        if (g.kind === 'more') { showMore(ev); return; }
        if (g.kind === 'files') { go(map.folder, 'files'); return; }
        if (g.kind === 'outside') { go(g.folder || '', 'map'); return; }
        go(g.folder, 'map');
    }

    function menuFor(g, ev) {
        const name = labelOf(g);
        if (g.kind === 'program' || g.kind === 'folder') {
            openMenu([[T('enter', { name }), () => go(g.folder, 'map')], [T('openFiles', { name }), () => go(g.folder, 'files')], [T('askPart', { name }), () => ask(g.folder)]], ev);
        } else if (g.kind === 'files') {
            openMenu([[T('openFiles', { name: map.folder }), () => go(map.folder, 'files')], [T('askPart', { name: map.folder }), () => ask(map.folder)]], ev);
        }
    }

    const FONT_TITLE = '600 13px -apple-system, "Segoe UI", Inter, system-ui, sans-serif';
    const FONT_SUB = '11px -apple-system, "Segoe UI", Inter, system-ui, sans-serif';
    const BOX_H = 46, GAP_X = 36, GAP_Y = 110, MAX_PER_ROW = 6;

    /**
     * Strati fissi, non forze (con le forze i box si sovrapponevano e coprivano le frecce): in alto chi usa, in basso chi è
     * usato — le frecce scendono, la direzione delle dipendenze si legge a colpo d'occhio. Strato = cammino più lungo
     * da una parte che nessuno usa (i cicli si spezzano); «fuori da qui» in fondo a destra; le parti senza legami in
     * un'ultima riga. Deterministico: la stessa mappa ha sempre lo stesso disegno.
     */
    function layout(nodes, links) {
        const measure = document.createElement('canvas').getContext('2d');
        nodes.forEach(n => {
            measure.font = FONT_TITLE; const w1 = measure.measureText((KIND_ICON[n.kind] || '') + ' ' + labelOf(n)).width;
            measure.font = FONT_SUB; const w2 = measure.measureText(subOf(n)).width;
            n.__bw = Math.max(w1, w2) + 28;
        });
        const inner = nodes.filter(n => n.kind !== 'outside');
        const out = {};
        inner.forEach(n => { out[n.id] = true; });
        const connected = new Set(links.flatMap(l => [l.from, l.to]));
        // per gli strati conta la direzione PREVALENTE di una coppia: 97 legami Commands→Interfaces e 3 all'indietro
        // fanno stare Interfaces sotto (sennò il ciclo si spezzava a caso e la mappa si capovolgeva)
        const countOf = (a, b) => (links.find(l => l.from === a && l.to === b) || { count: 0 }).count;
        const ranking = links.filter(l => out[l.from] && out[l.to] && l.from !== l.to &&
            (countOf(l.to, l.from) < l.count || (countOf(l.to, l.from) === l.count && l.from < l.to)));
        // cammino più lungo con visita in profondità: un arco verso un nodo «in visita» chiude un ciclo e si ignora
        const rank = {}, state = {};
        function visit(id) {
            if (state[id] === 2) return rank[id];
            if (state[id] === 1) return -1;
            state[id] = 1;
            let r = 0;
            const preds = ranking.filter(l => l.to === id).map(l => l.from);
            preds.forEach(p => { const pr = visit(p); if (pr >= 0) r = Math.max(r, pr + 1); });
            state[id] = 2; rank[id] = r;
            return r;
        }
        inner.filter(n => connected.has(n.id)).forEach(n => visit(n.id));
        const rows = [];
        const bySize = (a, b) => (b.files || 0) - (a.files || 0) || a.id.localeCompare(b.id);
        const layers = {};
        inner.filter(n => connected.has(n.id)).forEach(n => (layers[rank[n.id]] = layers[rank[n.id]] || []).push(n));
        Object.keys(layers).map(Number).sort((a, b) => a - b).forEach(r => {
            const list = layers[r].sort(bySize);
            for (let i = 0; i < list.length; i += MAX_PER_ROW) rows.push(list.slice(i, i + MAX_PER_ROW));
        });
        const outside = nodes.find(n => n.kind === 'outside');
        if (outside) { if (rows.length) rows[rows.length - 1].push(outside); else rows.push([outside]); }
        const loose = inner.filter(n => !connected.has(n.id)).sort(bySize);
        for (let i = 0; i < loose.length; i += MAX_PER_ROW) rows.push(loose.slice(i, i + MAX_PER_ROW));
        rows.forEach((row, ri) => {
            const width = row.reduce((s, n) => s + n.__bw, 0) + GAP_X * (row.length - 1);
            let x = -width / 2;
            row.forEach(n => { n.fx = n.x = x + n.__bw / 2; n.fy = n.y = ri * GAP_Y; x += n.__bw + GAP_X; });
        });
    }

    function render() {
        const el = document.getElementById('cg-graph');
        const isDark = document.body.classList.contains('dark-theme');
        const nodes = map.groups.map(g => Object.assign({}, g));
        const byId = Object.fromEntries(nodes.map(n => [n.id, n]));
        const links = map.links.filter(l => byId[l.from] && byId[l.to]).map(l => Object.assign({}, l, { source: l.from, target: l.to }));
        layout(nodes, map.links.filter(l => byId[l.from] && byId[l.to]));
        graph = ForceGraph()(el)
            .width(el.clientWidth).height(el.clientHeight)
            .backgroundColor(isDark ? '#0d1117' : '#f8fafc')
            .graphData({ nodes, links })
            .nodeId('id')
            .nodeLabel(n => labelOf(n) + (n.folder && n.kind !== 'outside' ? '<br>' + n.folder : ''))
            .nodeCanvasObject((n, ctx, scale) => {
                // misure in unità del grafo (come il layout): lo zoom ingrandisce tutto insieme, i box non si toccano mai
                const title = (KIND_ICON[n.kind] || '') + ' ' + labelOf(n), sub = subOf(n);
                const w = n.__bw, h = BOX_H;
                const fill = n.kind === 'program' ? (isDark ? '#1e3a8a' : '#dbeafe') : n.kind === 'outside' ? (isDark ? '#1f2937' : '#f1f5f9')
                    : n.kind === 'more' ? (isDark ? '#312e81' : '#ede9fe') : (isDark ? '#1e293b' : '#ffffff');
                ctx.beginPath();
                if (ctx.roundRect) ctx.roundRect(n.x - w / 2, n.y - h / 2, w, h, 8); else ctx.rect(n.x - w / 2, n.y - h / 2, w, h);
                ctx.fillStyle = fill; ctx.fill();
                ctx.lineWidth = 1.5; ctx.strokeStyle = isDark ? '#475569' : '#94a3b8';
                if (n.kind === 'outside' || n.kind === 'more') ctx.setLineDash([5, 4]);
                ctx.stroke(); ctx.setLineDash([]);
                ctx.fillStyle = isDark ? '#e6edf3' : '#0f172a'; ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
                ctx.font = FONT_TITLE;
                ctx.fillText(title, n.x, n.y - 9);
                ctx.fillStyle = isDark ? '#94a3b8' : '#475569';
                ctx.font = FONT_SUB;
                ctx.fillText(sub, n.x, n.y + 10);
                n.__w = w; n.__h = h;
            })
            .nodePointerAreaPaint((n, color, ctx) => { ctx.fillStyle = color; ctx.fillRect(n.x - (n.__w || 80) / 2, n.y - (n.__h || 40) / 2, n.__w || 80, n.__h || 40); })
            .linkColor(() => isDark ? '#64748b' : '#64748b')
            .linkWidth(l => 1 + Math.log2(l.count))
            .linkLineDash(l => (l.byConfidence.extracted || 0) * 2 >= l.count ? null : [6, 4])
            // nella stessa riga o all'insù (un ciclo) la freccia si curva, sennò passerebbe sopra i box in mezzo
            .linkCurvature(l => (l.source.y === l.target.y || l.source.y > l.target.y || map.links.some(o => o.from === l.to && o.to === l.from)) ? 0.3 : 0)
            // la punta si ferma sul bordo del box: per force-graph il nodo è un cerchio di raggio nodeRelSize·√val ≈ metà box
            .nodeRelSize(4).nodeVal(() => Math.pow(BOX_H / 2 / 4, 2))
            .linkDirectionalArrowLength(l => 7 + Math.log2(l.count)).linkDirectionalArrowRelPos(1)
            .linkDirectionalArrowColor(() => isDark ? '#cbd5e1' : '#334155')
            .linkLabel(tooltip)
            .onNodeClick((n, ev) => activate(n, ev))
            .onNodeRightClick((n, ev) => menuFor(n, ev))
            .onBackgroundClick(() => { hideMenu(); document.getElementById('cg-card').classList.add('hidden'); })
            .cooldownTicks(1);
        graph.d3Force('charge', null); graph.d3Force('link', null); graph.d3Force('center', null);
        setTimeout(() => graph.zoomToFit(300, 50), 50);
        el.addEventListener('contextmenu', e => e.preventDefault());
        window.addEventListener('resize', () => graph.width(el.clientWidth).height(el.clientHeight));
    }

    async function load() {
        const status = document.getElementById('cg-status');
        status.textContent = T('loading');
        const u = new URL('/api/codegraph/map', location.origin);
        u.searchParams.set('projectPath', projectPath);
        u.searchParams.set('folder', folder);
        let res;
        try { res = await fetch(u); } catch (e) { status.textContent = T('error', { status: e.message }); return; }
        if (!res.ok) { status.textContent = T('error', { status: res.status }) + ': ' + (await res.text()); return; }
        map = await res.json();
        // una cartella senza sottocartelle sarebbe un nodo solo: si va dritti ai suoi file (replace: «indietro» non torna qui)
        if (map.level === 'folders' && map.groups.length > 0 && map.groups.every(g => g.kind === 'files' || g.kind === 'outside')) {
            const u2 = new URL(location.href); u2.searchParams.set('folder', map.folder); u2.searchParams.set('view', 'files');
            location.replace(u2.toString());
            return;
        }
        renderHeader();
        if (!map.groups.length) { status.textContent = T('empty'); return; }
        render();
    }

    document.addEventListener('DOMContentLoaded', () => {
        load();
        window.parent.postMessage({ type: 'mde-mark.graphOpened', context: { projectPath, folder } }, '*');
    });
})();
