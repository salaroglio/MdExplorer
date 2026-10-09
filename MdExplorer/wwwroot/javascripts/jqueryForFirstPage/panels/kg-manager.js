/**
 * MdExplorer - Knowledge Graph (fullscreen, 2D)
 * ==============================================
 * Fullscreen interactive force-directed graph of links between markdown files,
 * drawn on a canvas as a top-down map. Files are grouped by folder inside a
 * dashed circle; a circle can be dragged and takes its files with it.
 * (The 3D view was removed on 2026-10-09: it was not used.)
 *
 * Backend:
 * - GET /api/tabcontroller/GetKnowledgeGraph?fullPathFile=...&depth=1&connectionid=...
 *
 * UMD globals required:
 * - ForceGraph     (force-graph@1.51)
 *
 * Public API:
 * - window.openKnowledgeGraph()
 * - window.closeKnowledgeGraph()
 * - window.toggleKnowledgeGraph()
 * - window.MdeKnowledgeGraph.{open,close,toggle,refresh,resize,folders}
 */
(function () {
    'use strict';

    let _overlay = null;
    let _graph = null;          // active ForceGraph instance
    let _source = 'files';      // 'files' = links between markdown files (default, legacy)
                                 // 'concepts' = concept graph from Neo4j (.kg.md payloads)
    let _data = null;
    let _layout = null;         // cluster layout for current data
    let _isDark = false;
    let _currentProjectId = null;
    let _availableNamespaces = [];
    let _selectedNamespace = ''; // '' = all namespaces (for concept source)
    let _fullData = null;        // pristine concept graph; _data is the rendered (possibly focused) subset
    let _focusId = null;         // when set, isolate this node's directed neighborhood
    let _handOverHalo = false;   // the mouse is over a folder circle (hand cursor), see bindHaloDrag

    // ---- Palette ------------------------------------------------------------
    const PALETTE = [
        '#0ea5e9', '#22c55e', '#f97316', '#a855f7', '#ec4899', '#06b6d4',
        '#84cc16', '#eab308', '#ef4444', '#14b8a6', '#6366f1', '#f59e0b'
    ];
    const CENTER_COLOR = '#facc15';
    const CENTER_RING  = '#b45309';

    function hashStr(s) {
        let h = 0;
        if (!s) return 0;
        for (let i = 0; i < s.length; i++) h = ((h << 5) - h + s.charCodeAt(i)) | 0;
        return Math.abs(h);
    }

    function bucketFor(node) {
        if (node && node.cluster && String(node.cluster).trim().length > 0) {
            return node.cluster;
        }
        if (node && node.mdContext && String(node.mdContext).trim().length > 0) {
            return node.mdContext;
        }
        return '__root__';
    }

    function friendlyBucketLabel(b) {
        if (!b) return '';
        if (b === '__root__') return (_data && _data.projectName) || 'root';   // the project folder's name
        const parts = String(b).split(/[\\/]/).filter(function (p) { return p.length > 0; });
        return parts.length ? parts[parts.length - 1] : b;
    }

    function hexToRgba(hex, alpha) {
        if (!hex || hex[0] !== '#') return hex;
        const h = hex.length === 4
            ? '#' + hex[1] + hex[1] + hex[2] + hex[2] + hex[3] + hex[3]
            : hex;
        const r = parseInt(h.substring(1, 3), 16);
        const g = parseInt(h.substring(3, 5), 16);
        const b = parseInt(h.substring(5, 7), 16);
        return 'rgba(' + r + ',' + g + ',' + b + ',' + alpha + ')';
    }

    function detectDark() {
        return !!(document.body && document.body.classList && document.body.classList.contains('dark-theme'));
    }

    function escapeHtml(s) {
        if (s == null) return '';
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    // Render minimal markdown inside the TLDR (paragraphs + unordered/ordered list items
    // starting with - * or 1.). Anything else is treated as plain text and escaped.
    function renderTldrHtml(tldr) {
        if (!tldr) return '';
        const lines = String(tldr).split(/\n/);
        const html = [];
        let inList = false;
        const paraBuf = [];
        function flushPara() {
            if (paraBuf.length) {
                html.push('<p>' + escapeHtml(paraBuf.join(' ')) + '</p>');
                paraBuf.length = 0;
            }
        }
        function flushList() {
            if (inList) { html.push('</ul>'); inList = false; }
        }
        for (let i = 0; i < lines.length; i++) {
            const raw = lines[i];
            const line = raw.trim();
            if (!line) { flushPara(); flushList(); continue; }
            const li = line.match(/^(?:[-*]|\d+\.)\s+(.*)$/);
            if (li) {
                flushPara();
                if (!inList) { html.push('<ul>'); inList = true; }
                html.push('<li>' + escapeHtml(li[1]) + '</li>');
            } else {
                flushList();
                paraBuf.push(line);
            }
        }
        flushPara();
        flushList();
        return html.join('');
    }

    function buildNodeTooltip(node) {
        const label = escapeHtml(node.label || node.id || '');
        const isExt = !!node.isExternal;
        let meta = isExt
            ? (node.cluster ? escapeHtml(node.cluster) : 'external')
            : (node.mdContext ? escapeHtml(node.mdContext) : 'root');
        if (node.nodeType) meta += ' · ' + escapeHtml(node.nodeType);
        if (node.kind)     meta += ' · ' + escapeHtml(node.kind);
        const icon = isExt ? '🌐 ' : (node.isCenter ? '◈ ' : '📄 ');
        let srcBlock = '';
        if (node.docPath) {
            let src = escapeHtml(node.docPath);
            if (node.lineStart != null) {
                src += ' : ' + node.lineStart +
                    (node.lineEnd != null && node.lineEnd !== node.lineStart ? '-' + node.lineEnd : '');
            }
            srcBlock = '<div class="kgTipMeta">' + src + '</div>';
        }
        // The transformation rule is the logic — show it first when present.
        const ruleHtml = renderTldrHtml(node.rule);
        const ruleBlock = ruleHtml ? '<div class="kgTipTldr">' + ruleHtml + '</div>' : '';
        const descHtml = renderTldrHtml(node.tldr);
        const descBlock = (descHtml && node.tldr !== node.rule)
            ? '<div class="kgTipTldr">' + descHtml + '</div>'
            : ((!ruleBlock && !isExt) ? '<div class="kgTipNote">No description</div>' : '');
        return '<div class="kgTooltip">' +
            '<div class="kgTipHead">' +
                '<span class="kgTipIcon">' + icon + '</span>' +
                '<span class="kgTipLabel">' + label + '</span>' +
            '</div>' +
            '<div class="kgTipMeta">' + meta + '</div>' +
            srcBlock +
            ruleBlock +
            descBlock +
        '</div>';
    }

    function buildLinkTooltip(link) {
        const t = escapeHtml((link.relType || link.linkType || 'link').toUpperCase()) +
            (link.role ? ' · ' + escapeHtml(link.role) : '');
        const descHtml = renderTldrHtml(link.description);
        const descBlock = descHtml ? '<div class="kgTipTldr">' + descHtml + '</div>' : '';
        return '<div class="kgTooltip">' +
            '<div class="kgTipHead">' +
                '<span class="kgTipIcon">&rarr; </span>' +
                '<span class="kgTipLabel">' + t + '</span>' +
            '</div>' +
            descBlock +
        '</div>';
    }

    // ---- Folder layout: circles that CONTAIN their files ------------------------
    //
    // Each folder is a circle with a centre and a radius decided before the simulation runs,
    // from the size of its boxes. A folder whose path is under another folder's path is drawn
    // inside it; the parent's own files live in the ring around the children. The project root
    // ("__root__", labelled with the project folder's name) and the external hosts are top-level
    // circles like any other, never parents. The current document is a file like the others, inside
    // the circle of its folder: that circle (its top-level ancestor, the "home") sits at the origin
    // and the other top-level circles on a ring around it, each with the angular room its radius
    // needs, so circles never overlap and a file is always inside its own circle and outside its
    // siblings' and children's (user decisions 2026-10-09).

    /** The nearest ancestor folder that is itself a bucket; null for a top-level one. */
    function parentBucketOf(b, present) {
        if (!b || b === '__root__' || b.charAt(0) !== '/') return null;
        let p = b;
        for (;;) {
            const i = p.lastIndexOf('/');
            if (i <= 0) return null;
            p = p.substring(0, i);
            if (present[p]) return p;
        }
    }

    /** Half the diagonal of a box, or the drawn circle plus its label (Concepts view). */
    function nodeExtent(n) {
        return n._boxW ? Math.hypot(n._boxW / 2, n._boxH / 2) : nodeRadius(n) + 28;
    }

    /** Name order: the current document first, then the files by name (numbers in numeric order). */
    function sortedMembers(f) {
        return f.members.slice().sort(function (p, q) {
            if (!!p.isCenter !== !!q.isCenter) return p.isCenter ? -1 : 1;
            return displayName(p).localeCompare(displayName(q), undefined, { numeric: true, sensitivity: 'base' });
        });
    }

    /**
     * The grid of a folder's boxes: the columns/rows split with the smallest enclosing circle.
     * Cells take the widest and tallest box, so no two boxes can overlap. Shared by sizeLayout
     * (radius) and seedPositions (places), so what is drawn is exactly what was measured.
     */
    function gridFor(members) {
        const n = members.length;
        if (!n) return null;
        const cellW = Math.max.apply(null, members.map(function (m) { return m._boxW || 2 * nodeExtent(m); })) + 16;
        const cellH = Math.max.apply(null, members.map(function (m) { return m._boxH || 2 * nodeExtent(m); })) + 12;
        let best = null;
        for (let c = 1; c <= n; c++) {
            const r = Math.ceil(n / c);
            const W = c * cellW, H = r * cellH;
            const R = Math.hypot(W, H) / 2;
            if (!best || R < best.R) best = { cols: c, rows: r, cellW: cellW, cellH: cellH, W: W, H: H, R: R };
        }
        return best;
    }

    function computeLayout(data) {
        const present = Object.create(null);
        const buckets = [];
        for (let i = 0; i < data.nodes.length; i++) {
            const b = bucketFor(data.nodes[i]);
            if (!present[b]) { present[b] = true; buckets.push(b); }
        }
        const folders = Object.create(null);
        buckets.forEach(function (b) {
            folders[b] = { key: b, parent: null, children: [], members: [], cx: 0, cy: 0, r: 0, ringMid: 0, dx: 0, dy: 0 };
        });
        buckets.forEach(function (b) {
            const p = parentBucketOf(b, present);
            if (p) { folders[b].parent = p; folders[p].children.push(b); }
        });
        data.nodes.forEach(function (n) { folders[bucketFor(n)].members.push(n); });
        const top = buckets.filter(function (b) { return !folders[b].parent; });
        // The home circle: the top-level folder that holds the current document.
        const center = data.nodes.find(function (n) { return n.isCenter; });
        let home = center ? bucketFor(center) : null;
        while (home && folders[home].parent) home = folders[home].parent;
        const layout = { buckets: buckets, folders: folders, top: top, home: home, halos: Object.create(null) };
        sizeLayout(layout);   // estimates until the boxes are measured; build2D calls it again
        return layout;
    }

    /** Radii bottom-up from the boxes, then positions top-down. Safe to call again after measuring. */
    function sizeLayout(layout) {
        const PAD = 18;
        const folders = layout.folders;

        function radiusOf(f) {
            f.children.forEach(function (c) { radiusOf(folders[c]); });
            const ext = f.members.map(nodeExtent);
            const maxExt = ext.length ? Math.max.apply(null, ext) : 0;
            let inner = 0;   // radius taken by the children, packed on a small ring (one child: at the centre)
            if (f.children.length) {
                const kids = f.children.map(function (c) { return folders[c]; });
                const maxR = Math.max.apply(null, kids.map(function (k) { return k.r; }));
                const rho = kids.length === 1 ? 0 : maxR / Math.sin(Math.PI / kids.length) * 1.08 + PAD;
                kids.forEach(function (k, i) {
                    const a = (i / kids.length) * 2 * Math.PI - Math.PI / 2;
                    k.dx = Math.cos(a) * rho;
                    k.dy = Math.sin(a) * rho;
                });
                inner = rho + maxR;
            }
            let r;
            if (!inner) {
                const grid = gridFor(f.members);
                r = Math.max(grid ? grid.R + PAD : 0, maxExt + PAD);
                f.ringMid = 0;
            } else if (!ext.length) {
                r = inner + PAD;
                f.ringMid = 0;
            } else {
                // the folder's own boxes live in the ring around the children: one box wide, long enough for all
                const needed = ext.reduce(function (s, e) { return s + 2 * e * 1.15; }, 0);
                f.ringMid = Math.max(inner + PAD + maxExt, needed / (2 * Math.PI));
                r = f.ringMid + maxExt + PAD;
            }
            f.r = Math.max(r, 60);
        }
        layout.top.forEach(function (b) { radiusOf(folders[b]); });

        // The home circle at the origin, the other top-level circles on a ring around it, angular room by radius.
        const homeF = layout.home ? folders[layout.home] : null;
        if (homeF) { homeF.cx = 0; homeF.cy = 0; }
        const tops = layout.top.filter(function (b) { return b !== layout.home; }).map(function (b) { return folders[b]; });
        const maxTopR = tops.length ? Math.max.apply(null, tops.map(function (f) { return f.r; })) : 0;
        const GAP = 0.08;
        let ringR = Math.max(260, maxTopR + (homeF ? homeF.r : 0) + 40);
        const need = function () {
            return tops.reduce(function (s, f) { return s + 2 * Math.asin(Math.min(1, f.r / ringR)) + GAP; }, 0);
        };
        for (let iter = 0; iter < 60 && tops.length > 1 && need() > 2 * Math.PI; iter++) ringR *= 1.06;
        const slack = tops.length ? Math.max(0, 2 * Math.PI - need()) / tops.length : 0;
        let angle = -Math.PI / 2;
        tops.forEach(function (f) {
            const half = Math.asin(Math.min(1, f.r / ringR)) + GAP / 2 + slack / 2;
            angle += half;
            f.cx = Math.cos(angle) * ringR;
            f.cy = Math.sin(angle) * ringR;
            angle += half;
        });
        layout.ringR = ringR;

        function place(f) {
            f.children.forEach(function (c) {
                const k = folders[c];
                k.cx = f.cx + k.dx;
                k.cy = f.cy + k.dy;
                place(k);
            });
        }
        layout.top.forEach(function (b) { place(folders[b]); });
    }

    /**
     * The place of every node is decided here, not by a simulation: inside its circle, in name
     * order (the current document first), on a centred grid (gridFor), read like a list; in a
     * folder with children, around the ring between the children and the edge.
     */
    function seedPositions(data, layout) {
        layout.buckets.forEach(function (b) {
            const f = layout.folders[b];
            const members = sortedMembers(f);
            const n = members.length;
            if (!n) return;
            const grid = gridFor(members);
            members.forEach(function (m, i) {
                if (m.fx != null) return;
                if (f.ringMid) {
                    const a = (i / n) * 2 * Math.PI - Math.PI / 2;
                    m.x = f.cx + Math.cos(a) * f.ringMid;
                    m.y = f.cy + Math.sin(a) * f.ringMid;
                    return;
                }
                const row = Math.floor(i / grid.cols), col = i % grid.cols;
                const inRow = Math.min(grid.cols, n - row * grid.cols);   // a short last row is centred too
                const rowW = inRow * grid.cellW;
                m.x = f.cx - rowW / 2 + grid.cellW * (col + 0.5);
                m.y = f.cy - grid.H / 2 + grid.cellH * (row + 0.5);
            });
        });
    }

    /**
     * Frame the whole picture: the folder circles (zoomToFit only looks at the nodes and would
     * cut the circles) and the current document.
     */
    function fitToFolders(g, container, ms) {
        if (!_layout || !_layout.top.length) { g.zoomToFit(ms, 60); return; }
        let x0 = Infinity, y0 = Infinity, x1 = -Infinity, y1 = -Infinity;
        _layout.top.forEach(function (b) {
            const f = _layout.folders[b];
            x0 = Math.min(x0, f.cx - f.r); x1 = Math.max(x1, f.cx + f.r);
            y0 = Math.min(y0, f.cy - f.r - 24); y1 = Math.max(y1, f.cy + f.r);   // 24: the label above the circle
        });
        const rect = container.getBoundingClientRect();
        const pad = 30;
        const k = Math.min((rect.width - 2 * pad) / (x1 - x0), (rect.height - 2 * pad) / (y1 - y0));
        g.centerAt((x0 + x1) / 2, (y0 + y1) / 2, ms);
        g.zoom(Math.max(0.05, Math.min(k, 2)), ms);
    }

    /** Moves a circle, the circles inside it and all their nodes (pinned ones too). */
    function shiftFolder(f, dx, dy) {
        f.cx += dx; f.cy += dy;
        f.members.forEach(function (n) {
            if (isFinite(n.x)) { n.x += dx; n.y += dy; }
            if (n.fx != null) { n.fx += dx; n.fy += dy; }
        });
        f.children.forEach(function (c) { shiftFolder(_layout.folders[c], dx, dy); });
    }

    /**
     * The circles are rigid: while one is dragged, the siblings it runs into are pushed away
     * with their files, and a child never leaves its parent. Position-level, run on every mouse
     * move of a circle drag.
     */
    function pushCirclesApart(draggedKey) {
        const folders = _layout.folders;
        const GAPPX = 12;
        const groups = [_layout.top].concat(_layout.buckets.map(function (b) { return folders[b].children; })
            .filter(function (c) { return c.length > 1; }));
        for (let iter = 0; iter < 30; iter++) {
            let moved = false;
            groups.forEach(function (group) {
                for (let i = 0; i < group.length; i++) {
                    for (let j = i + 1; j < group.length; j++) {
                        const a = folders[group[i]], b = folders[group[j]];
                        let ux = b.cx - a.cx, uy = b.cy - a.cy;
                        let d = Math.hypot(ux, uy);
                        if (d < 1e-6) { ux = 1; uy = 0; d = 1; }
                        const min = a.r + b.r + GAPPX;
                        if (d >= min) continue;
                        const aFixed = group[i] === draggedKey, bFixed = group[j] === draggedKey;
                        if (aFixed && bFixed) continue;
                        const push = (min - d) + 0.5;
                        const shareA = aFixed ? 0 : (bFixed ? 1 : 0.5), shareB = 1 - shareA;
                        shiftFolder(a, -ux / d * push * shareA, -uy / d * push * shareA);
                        shiftFolder(b,  ux / d * push * shareB,  uy / d * push * shareB);
                        moved = true;
                    }
                }
            });
            _layout.buckets.forEach(function (b) {
                const f = folders[b];
                if (!f.parent) return;
                const p = folders[f.parent];
                const room = Math.max(0, p.r - f.r - 6);
                const ox = f.cx - p.cx, oy = f.cy - p.cy, od = Math.hypot(ox, oy);
                if (od > room + 0.5) { shiftFolder(f, ox / od * (room - od), oy / od * (room - od)); moved = true; }
            });
            if (!moved) break;
        }
    }

    /** Every node of a folder and of the folders inside it. */
    function descendantsOf(layout, b) {
        const out = [];
        (function walk(key) {
            const f = layout.folders[key];
            if (!f) return;
            out.push.apply(out, f.members);
            f.children.forEach(walk);
        })(b);
        return out;
    }

    /**
     * The folder force is two constraints, not an attraction: a node that crosses the edge of
     * its circle is pushed back in, a node inside a child's circle is pushed out. They do not
     * fade with alpha. Nothing pulls a node that is already in place, so moving one node (or
     * one circle) never moves the others, except to make room.
     */
    function folderForce2D() {
        if (!_data || !_layout) return;
        const FIRM = 0.35;
        const folders = _layout.folders;
        const nodes = _data.nodes;
        for (let i = 0; i < nodes.length; i++) {
            const n = nodes[i];
            if (!isFinite(n.x) || !isFinite(n.y)) continue;
            const f = folders[bucketFor(n)];
            if (!f) continue;
            const ext = nodeExtent(n);
            const dx = n.x - f.cx, dy = n.y - f.cy;
            const d = Math.hypot(dx, dy) || 1e-6;
            const limit = f.r - ext - 4;
            if (d > limit) {
                const s = (d - limit) * FIRM / d;
                n.vx = (n.vx || 0) - dx * s;
                n.vy = (n.vy || 0) - dy * s;
            }
            for (let c = 0; c < f.children.length; c++) {
                const g = folders[f.children[c]];
                const ex = n.x - g.cx, ey = n.y - g.cy;
                const dd = Math.hypot(ex, ey) || 1e-6;
                const min = g.r + ext + 4;
                if (dd < min) {
                    const s = (min - dd) * FIRM / dd;
                    n.vx = (n.vx || 0) + ex * s;
                    n.vy = (n.vy || 0) + ey * s;
                }
            }
        }
    }

    /** Position-level version of the constraints, for the boxes settled by hand at engine stop. */
    function clampIntoFolder(n) {
        const f = _layout && _layout.folders[bucketFor(n)];
        if (!f) return false;
        const ext = nodeExtent(n);
        let moved = false;
        const dx = n.x - f.cx, dy = n.y - f.cy;
        const d = Math.hypot(dx, dy) || 1e-6;
        const limit = f.r - ext - 4;
        if (d > limit) { n.x = f.cx + dx * limit / d; n.y = f.cy + dy * limit / d; moved = true; }
        for (let c = 0; c < f.children.length; c++) {
            const g = _layout.folders[f.children[c]];
            const ex = n.x - g.cx, ey = n.y - g.cy;
            const dd = Math.hypot(ex, ey) || 1e-6;
            const min = g.r + ext + 4;
            if (dd < min) { n.x = g.cx + ex * min / dd; n.y = g.cy + ey * min / dd; moved = true; }
        }
        return moved;
    }

    function nodeColor(node) {
        if (node.isCenter) return CENTER_COLOR;
        return PALETTE[hashStr(bucketFor(node)) % PALETTE.length];
    }

    function linkColor(link) {
        switch (link.linkType) {
            case 'publication': return '#db2777';
            case 'excerpt':     return '#d97706';
            case 'plantuml':    return '#059669';
            default:            return '#3b82f6';
        }
    }

    function nodeRadius(node) {
        if (node.isCenter) return 14;
        return 6 + Math.min(8, (node.inDegree || 0) + (node.outDegree || 0));
    }

    // ---- File boxes (2D "Files" view) ------------------------------------------
    // Each file node is an HTML box laid over the canvas: ▸, the icon of its type and its
    // name, with the TL;DR below when opened. The canvas keeps positions, links, zoom and
    // pan; the boxes follow their node every frame and scale with the zoom, so the layout
    // is the same at any zoom (the collision force works in unscaled box pixels).

    const TEXT_EXTENSIONS = ['yaml', 'yml', 'xml', 'xsd', 'xslt', 'ttl', 'nt', 'n3', 'nq', 'rdf', 'owl',
        'sql', 'cypher', 'sparql', 'cs', 'ts', 'js', 'java', 'kt', 'py', 'sh', 'bash', 'ps1',
        'css', 'scss', 'txt', 'csv', 'log', 'html', 'htm', 'cob', 'cbl', 'cpy'];

    /** The file name, not the path: the last segment on "/" or "\" (Windows paths on any OS). */
    function displayName(node) {
        if (node.isExternal) return node.label || node.externalUrl || node.id || '';
        const src = node.relativePath || node.fullPath || node.label || node.id || '';
        const parts = String(src).split(/[\\/]/).filter(function (p) { return p.length > 0; });
        return parts.length ? parts[parts.length - 1] : String(src);
    }

    function extensionOf(name) {
        const m = /\.([A-Za-z0-9]+)$/.exec(name || '');
        return m ? m[1].toLowerCase() : '';
    }

    /** The kind of file, for its icon. The backend's node.kind wins when present. */
    function fileKind(node) {
        if (node.kind) return node.kind;
        if (node.isExternal) return 'web';
        const ext = extensionOf(displayName(node));
        switch (ext) {
            case 'md': return 'markdown';
            case 'json': case 'jsonld': return 'json';
            case 'doc': case 'docx': return 'word';
            case 'ppt': case 'pptx': return 'powerpoint';
            case 'xls': case 'xlsx': return 'excel';
            case 'pdf': return 'pdf';
            case 'png': case 'jpg': case 'jpeg': case 'gif': case 'svg': case 'webp': case 'bmp': return 'image';
            default: return TEXT_EXTENSIONS.indexOf(ext) >= 0 ? 'text' : 'other';
        }
    }

    function fileIconHtml(node) {
        const kind = fileKind(node);
        const ext = extensionOf(displayName(node));
        const glyphs = {
            markdown: 'M↓', json: '{ }', word: 'W', powerpoint: 'P', excel: 'X', pdf: 'PDF',
            image: '🖼', web: '🌐', other: '📄'
        };
        const glyph = glyphs[kind] || (ext ? ext.toUpperCase().slice(0, 4) : '📄');
        const title = kind === 'text' && ext ? ext.toUpperCase() : kind;
        return '<span class="kgFileIcon kgFileIcon-' + escapeHtml(kind) + '" title="' + escapeHtml(title) + '">' + escapeHtml(glyph) + '</span>';
    }

    /**
     * The boxes are built and measured BEFORE the graph exists (the folder circles are sized from
     * them), so the handlers reach the graph through gRef.g, set once ForceGraph is created.
     */
    function buildBoxLayer(container, data, gRef) {
        const layer = document.createElement('div');
        layer.className = 'kgBoxLayer';
        container.appendChild(layer);
        data.nodes.forEach(function (node) {
            const el = buildBox(node, container, gRef);
            layer.appendChild(el);
            node._box = el;
            measureBox(node);
        });
        // The boxes cover the canvas: a wheel over a box still zooms the graph.
        layer.addEventListener('wheel', function (e) {
            const canvas = container.querySelector('canvas');
            if (!canvas) return;
            e.preventDefault();
            canvas.dispatchEvent(new WheelEvent('wheel', e));
        }, { passive: false });
        return layer;
    }

    function buildBox(node, container, gRef) {
        const el = document.createElement('div');
        const missing = node.exists === false;
        el.className = 'kgBox' + (node.isCenter ? ' kgBoxCenter' : '') + (missing ? ' kgBoxMissing' : '');
        el.setAttribute('data-folder', bucketFor(node));
        el.style.setProperty('--kg-accent', nodeColor(node));
        const name = displayName(node);
        const hasTldr = !!(node.tldr && String(node.tldr).trim());
        const nameTitle = missing
            ? 'File non trovato: ' + (node.relativePath || name)
            : (node.relativePath || node.externalUrl || name);
        el.innerHTML =
            '<div class="kgBoxHead">' +
                '<button type="button" class="kgBoxToggle" aria-expanded="false"' +
                    (hasTldr ? ' title="TL;DR"' : ' disabled title="Nessun TL;DR"') + '>▸</button>' +
                fileIconHtml(node) +
                '<span class="kgBoxName" title="' + escapeHtml(nameTitle) + '">' + escapeHtml(name) + '</span>' +
            '</div>' +
            (hasTldr ? '<div class="kgBoxBody">' + renderTldrHtml(node.tldr) + '</div>' : '');

        const toggle = el.querySelector('.kgBoxToggle');
        // The TL;DR opens as a balloon out of the flow (see .kgBoxBody): the box keeps the
        // size of its head, so nothing is re-measured and the graph does not move.
        toggle.addEventListener('click', function (e) {
            e.stopPropagation();
            if (toggle.disabled) return;
            const open = el.classList.toggle('kgBoxOpen');
            toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
        el.querySelectorAll('.kgFileIcon, .kgBoxName').forEach(function (target) {
            target.addEventListener('click', function (e) {
                e.stopPropagation();
                handleNodeClick(node);
            });
        });
        el.querySelector('.kgBoxHead').addEventListener('mousedown', function (e) {
            if (e.button !== 0 || !gRef.g || e.target.closest('.kgBoxToggle, .kgFileIcon, .kgBoxName')) return;
            startBoxDrag(e, node, container, gRef.g);
        });
        return el;
    }

    /** Box size in unscaled pixels (offsetWidth ignores the zoom transform). */
    function measureBox(node) {
        const el = node._box;
        if (!el) return;
        node._boxW = el.offsetWidth;
        node._boxH = el.offsetHeight;
        const head = el.querySelector('.kgBoxHead');
        node._boxHeadH = head ? head.offsetHeight : el.offsetHeight;
    }

    /** Head centered on the node; the TL;DR opens downward. */
    function positionBoxes(g) {
        if (!_data) return;
        const k = g.zoom();
        _data.nodes.forEach(function (node) {
            const el = node._box;
            if (!el || !isFinite(node.x) || !isFinite(node.y)) return;
            const p = g.graph2ScreenCoords(node.x, node.y);
            el.style.transform = 'translate(' + p.x + 'px,' + p.y + 'px) scale(' + k + ') translate(-50%,' + (-(node._boxHeadH || 0) / 2) + 'px)';
        });
    }

    /** A dragged box stays where it is dropped (pinned), so boxes can be arranged by hand. */
    function startBoxDrag(e, node, container, g) {
        e.preventDefault();
        e.stopPropagation();
        const rect = container.getBoundingClientRect();
        const start = g.screen2GraphCoords(e.clientX - rect.left, e.clientY - rect.top);
        const offX = (node.x || 0) - start.x;
        const offY = (node.y || 0) - start.y;
        node._box.classList.add('kgBoxDragging');
        function move(ev) {
            const p = g.screen2GraphCoords(ev.clientX - rect.left, ev.clientY - rect.top);
            node.fx = p.x + offX;
            node.fy = p.y + offY;
            try { g.d3ReheatSimulation(); } catch (err) { /* noop */ }
        }
        function up() {
            document.removeEventListener('mousemove', move);
            document.removeEventListener('mouseup', up);
            if (node._box) node._box.classList.remove('kgBoxDragging');
        }
        document.addEventListener('mousemove', move);
        document.addEventListener('mouseup', up);
    }

    function boxCenterOffsetY(n) { return ((n._boxH || 0) - (n._boxHeadH || 0)) / 2; }

    /** Boxes must not overlap: two overlapping boxes are pushed apart along the axis where they overlap least. */
    function boxCollideForce2D() {
        if (!_data) return;
        const nodes = _data.nodes;
        const pad = 16, strength = 1.0;
        for (let i = 0; i < nodes.length; i++) {
            const a = nodes[i];
            if (!a._boxW || typeof a.x !== 'number') continue;
            for (let j = i + 1; j < nodes.length; j++) {
                const b = nodes[j];
                if (!b._boxW || typeof b.x !== 'number') continue;
                const dx = b.x - a.x;
                const dy = (b.y + boxCenterOffsetY(b)) - (a.y + boxCenterOffsetY(a));
                const ox = (a._boxW + b._boxW) / 2 + pad - Math.abs(dx);
                const oy = (a._boxH + b._boxH) / 2 + pad - Math.abs(dy);
                if (ox <= 0 || oy <= 0) continue;
                if (ox < oy) {
                    const s = (dx >= 0 ? 1 : -1) * ox * strength / 2;
                    a.vx = (a.vx || 0) - s;
                    b.vx = (b.vx || 0) + s;
                } else {
                    const s = (dy >= 0 ? 1 : -1) * oy * strength / 2;
                    a.vy = (a.vy || 0) - s;
                    b.vy = (b.vy || 0) + s;
                }
            }
        }
    }

    /**
     * When the simulation stops the forces may still leave boxes overlapping (the folder pull
     * wins over the collision): move the boxes themselves until none overlaps, keeping each one
     * inside its folder circle and outside the children's. A pinned box (dragged by hand) stays
     * where it is and the other one moves.
     */
    function settleBoxes() {
        if (!_data) return;
        const nodes = _data.nodes.filter(function (n) { return n._boxW && isFinite(n.x) && isFinite(n.y); });
        const pad = 16;
        for (let iter = 0; iter < 80; iter++) {
            let moved = false;
            for (let i = 0; i < nodes.length; i++) {
                const n = nodes[i];
                if (n.fx == null && clampIntoFolder(n)) moved = true;
            }
            for (let i = 0; i < nodes.length; i++) {
                const a = nodes[i];
                for (let j = i + 1; j < nodes.length; j++) {
                    const b = nodes[j];
                    const dx = b.x - a.x;
                    const dy = (b.y + boxCenterOffsetY(b)) - (a.y + boxCenterOffsetY(a));
                    const ox = (a._boxW + b._boxW) / 2 + pad - Math.abs(dx);
                    const oy = (a._boxH + b._boxH) / 2 + pad - Math.abs(dy);
                    if (ox <= 0 || oy <= 0) continue;
                    const aPinned = a.fx != null, bPinned = b.fx != null;
                    if (aPinned && bPinned) continue;
                    const horizontal = ox < oy;
                    const push = (horizontal ? ox : oy) + 0.5;
                    const sign = (horizontal ? dx : dy) >= 0 ? 1 : -1;
                    const shareA = aPinned ? 0 : (bPinned ? 1 : 0.5);
                    const shareB = 1 - shareA;
                    if (horizontal) {
                        a.x -= sign * push * shareA;
                        b.x += sign * push * shareB;
                    } else {
                        a.y -= sign * push * shareA;
                        b.y += sign * push * shareB;
                    }
                    moved = true;
                }
            }
            if (!moved) break;
        }
    }

    /** The arrow tip on the edge of the target box, not at its hidden center. */
    function boxArrowRelPos(link) {
        const s = link.source, t = link.target;
        if (!s || !t || !isFinite(s.x) || !isFinite(s.y) || !isFinite(t.x) || !isFinite(t.y) || !t._boxW) return 0.92;
        const dx = t.x - s.x, dy = t.y - s.y;
        if (Math.hypot(dx, dy) < 1) return 1;
        const halfW = t._boxW / 2 + 3;
        const above = (t._boxHeadH || 0) / 2 + 3;
        const below = t._boxH - (t._boxHeadH || 0) / 2 + 3;
        const halfH = dy > 0 ? above : below;   // coming from above → top edge
        const fx = dx !== 0 ? halfW / Math.abs(dx) : Infinity;
        const fy = dy !== 0 ? halfH / Math.abs(dy) : Infinity;
        return Math.max(0, Math.min(1, 1 - Math.min(fx, fy)));
    }

    // ---- Helpers ------------------------------------------------------------
    function getDocumentPath() {
        const anchor = document.getElementById('KGAnchor');
        if (anchor) return anchor.getAttribute('mdeFullPathDocument');
        const body = document.getElementById('MdBody');
        if (body) return body.getAttribute('documentpath');
        return null;
    }

    function isExternalUrl(s) {
        if (!s) return false;
        // any URI scheme other than a Windows drive letter (C:\...)
        return /^[a-z][a-z0-9+\-.]*:(\/\/|[^\\/])/i.test(s) && !/^[a-zA-Z]:[\\/]/.test(s);
    }

    // ---- Opening a file node ---------------------------------------------------
    // What a click does is decided by the backend (node.openWith, KnowledgeGraphFiles), with
    // the rules of the page: a .md or a text file opens in the page, the project's
    // application extensions (.mdapplicationtoopen) with their application, a URL in the
    // system browser, a missing file nowhere.
    //
    // "In the page" goes through Angular, as a click in the tree: the md-navigate message
    // (MainContentComponent.handleMdNavigate). Navigating the iframe itself, as this graph
    // used to, showed a .json as raw bytes (the colored view needs source=angular) and left it
    // out of the title-bar arrows; through Angular the page is colored and the arrows get the
    // entry (markdownfileisprocessed). Application and browser change no page: no entry.

    function openFileNode(node) {
        if (!node || node.isCenter) return;
        switch (node.openWith) {
            case 'page':        openInPage(node); return;
            case 'application': openWithApplication(node); return;
            case 'browser':     openInSystemBrowser(node); return;
            case 'none':        showNotice('File non trovato: ' + (node.relativePath || displayName(node))); return;
            default:
                console.error('[KG] node without a valid openWith — page and backend out of step:', node.openWith, node);
        }
    }

    function openInPage(node) {
        const rel = node.relativePath;
        if (!rel) {
            console.error('[KG] openWith=page without a relative path:', node);
            return;
        }
        closeOverlay();
        if (window.parent && window.parent !== window) {
            // fullPath: the title-bar history tells entries apart by it.
            window.parent.postMessage({ type: 'md-navigate', relativePath: rel, name: displayName(node), fullPath: node.fullPath }, '*');
            return;
        }
        // A detached window has no Angular around it: it opens the file itself, as a detached
        // window (same parameters, so the backend still skips the main window's side effects).
        const current = new URLSearchParams(window.location.search);
        const params = new URLSearchParams();
        ['ConnectionId', 'connectionId', 'theme'].forEach(function (k) { if (current.get(k)) params.set(k, current.get(k)); });
        params.set('time', String(Date.now() / 1000));
        params.set('source', 'detached');
        params.set('detached', 'true');
        window.location.href = '/api/mdexplorer/' + rel.split('/').map(encodeURIComponent).join('/') + '?' + params.toString();
    }

    function openWithApplication(node) {
        if (typeof openApplication !== 'function') {
            console.error('[KG] openApplication (core/utilities.js) is not loaded');
            return;
        }
        openApplication(node.fullPath);
    }

    function openInSystemBrowser(node) {
        $.ajax({
            url: '/api/MdFiles/OpenUrlInBrowser',
            type: 'POST',
            data: JSON.stringify({ url: node.externalUrl, connectionId: $('#MdBody').attr('connectionid') }),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json'
        }).fail(function (xhr) {
            showNotice('Impossibile aprire il link (HTTP ' + (xhr ? xhr.status : '?') + ')');
        });
    }

    /** A short notice inside the graph panel. */
    function showNotice(message) {
        if (!_overlay) return;
        let notice = _overlay.querySelector('.kgNotice');
        if (!notice) {
            notice = document.createElement('div');
            notice.className = 'kgNotice';
            _overlay.appendChild(notice);
        }
        notice.textContent = message;
        notice.classList.add('kgNoticeShown');
        clearTimeout(notice._timer);
        notice._timer = setTimeout(function () { notice.classList.remove('kgNoticeShown'); }, 4000);
    }

    function normalize(raw) {
        if (!raw || !raw.nodes) return { nodes: [], links: [] };
        return {
            projectName: raw.projectName || '',
            nodes: raw.nodes.map(function (n) {
                return {
                    id: n.id,
                    label: n.label,
                    fullPath: n.fullPath,
                    relativePath: n.relativePath,
                    mdContext: n.mdContext,
                    cluster: n.cluster,
                    isCenter: !!n.isCenter,
                    isExternal: !!n.isExternal,
                    externalUrl: n.externalUrl,
                    tldr: n.tldr,
                    kind: n.kind || '',          // icon (backend: KnowledgeGraphFiles)
                    openWith: n.openWith || '',  // page | application | browser | none
                    exists: n.exists !== false,
                    inDegree: n.inDegree || 0,
                    outDegree: n.outDegree || 0
                };
            }),
            links: (raw.links || []).map(function (l) {
                return { source: l.source, target: l.target, linkType: l.linkType };
            })
        };
    }

    // ---- Canvas helpers -------------------------------------------------------
    function drawRoundedRect(ctx, x, y, w, h, r) {
        ctx.beginPath();
        ctx.moveTo(x + r, y);
        ctx.lineTo(x + w - r, y);
        ctx.quadraticCurveTo(x + w, y, x + w, y + r);
        ctx.lineTo(x + w, y + h - r);
        ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
        ctx.lineTo(x + r, y + h);
        ctx.quadraticCurveTo(x, y + h, x, y + h - r);
        ctx.lineTo(x, y + r);
        ctx.quadraticCurveTo(x, y, x + r, y);
        ctx.closePath();
    }

    // ---- 2D builder ---------------------------------------------------------
    function build2D(container, data) {
        if (typeof ForceGraph !== 'function') {
            console.error('[KG] ForceGraph (2D) not loaded');
            return null;
        }
        const rect = container.getBoundingClientRect();
        const w = Math.max(rect.width, 400);
        const h = Math.max(rect.height, 400);
        // Files view: nodes are HTML boxes (buildBoxLayer). Concepts keep the drawn circles.
        const useBoxes = _source === 'files';

        // Boxes first: the folder circles are sized from their measured boxes, and every node
        // starts inside its own circle (the layer stays above the canvas by its z-index).
        const gRef = { g: null };
        const layer = useBoxes ? buildBoxLayer(container, data, gRef) : null;
        if (_layout) { sizeLayout(_layout); seedPositions(data, _layout); }

        const g = ForceGraph()(container)
            .width(w).height(h)
            .backgroundColor('rgba(0,0,0,0)') // transparent over our CSS gradient
            .nodeRelSize(6)
            .nodeLabel(useBoxes ? null : buildNodeTooltip)
            .nodeColor(nodeColor)
            .nodeVal(function (n) { return n.isCenter ? 14 : 3 + Math.min(10, (n.inDegree || 0) + (n.outDegree || 0)); })
            .nodeCanvasObjectMode(function () { return 'replace'; })
            .nodeCanvasObject(function (node, ctx, globalScale) {
                if (useBoxes) return;   // the box is the node
                // force-graph can draw the first frame before the simulation has placed the
                // nodes (x/y undefined, measured 17/09/2026 on a slowed CPU). createRadialGradient
                // throws on them, and the exception stops force-graph's render loop for good: the
                // empty panel of the first K.G. after start. A node not placed yet is not drawn;
                // the next frame has its position.
                if (!isFinite(node.x) || !isFinite(node.y)) return;
                const r = nodeRadius(node);
                const isCenter = !!node.isCenter;
                const color = nodeColor(node);

                // Soft glow for the center
                if (isCenter) {
                    const grad = ctx.createRadialGradient(node.x, node.y, r, node.x, node.y, r * 2.6);
                    grad.addColorStop(0, 'rgba(250,204,21,0.55)');
                    grad.addColorStop(1, 'rgba(250,204,21,0)');
                    ctx.fillStyle = grad;
                    ctx.beginPath();
                    ctx.arc(node.x, node.y, r * 2.6, 0, 2 * Math.PI);
                    ctx.fill();
                }

                // Filled circle
                ctx.beginPath();
                ctx.arc(node.x, node.y, r, 0, 2 * Math.PI);
                ctx.fillStyle = color;
                ctx.fill();
                ctx.lineWidth = isCenter ? 2.5 : 1.5;
                ctx.strokeStyle = isCenter ? CENTER_RING : (_isDark ? 'rgba(255,255,255,0.32)' : 'rgba(15,23,42,0.25)');
                ctx.stroke();

                // Label (with globe prefix for external links)
                const rawLabel = node.label || node.id || '';
                const text = node.isExternal ? '🌐 ' + rawLabel : rawLabel;
                const fontSize = (isCenter ? 13 : 11) / globalScale;
                ctx.font = (isCenter ? 'bold ' : '600 ') + fontSize + 'px -apple-system, "Segoe UI", Inter, system-ui, sans-serif';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'top';
                const padX = 4 / globalScale;
                const padY = 2 / globalScale;
                const tw = ctx.measureText(text).width;
                const labelY = node.y + r + 3 / globalScale;
                // Dark-aware pill background
                const pillBg     = isCenter ? 'rgba(254, 240, 138, 0.95)' : (_isDark ? 'rgba(17,24,39,0.88)' : 'rgba(255,255,255,0.92)');
                const pillStroke = isCenter ? CENTER_RING                : (_isDark ? 'rgba(255,255,255,0.22)' : 'rgba(15,23,42,0.12)');
                const pillText   = isCenter ? '#451a03'                  : (_isDark ? '#e6edf3' : '#0f172a');
                ctx.fillStyle = pillBg;
                ctx.strokeStyle = pillStroke;
                ctx.lineWidth = 1 / globalScale;
                const pillX = node.x - tw / 2 - padX;
                const pillY = labelY;
                const pillW = tw + 2 * padX;
                const pillH = fontSize + 2 * padY;
                const pillR = pillH / 2;
                drawRoundedRect(ctx, pillX, pillY, pillW, pillH, pillR);
                ctx.fill();
                ctx.stroke();
                ctx.fillStyle = pillText;
                ctx.fillText(text, node.x, pillY + padY);
            })
            .nodePointerAreaPaint(function (node, color, ctx) {
                if (useBoxes) return;   // the box handles its own pointer
                if (!isFinite(node.x) || !isFinite(node.y)) return;   // not placed yet (see nodeCanvasObject)
                const r = nodeRadius(node);
                ctx.beginPath();
                ctx.arc(node.x, node.y, r + 2, 0, 2 * Math.PI);
                ctx.fillStyle = color;
                ctx.fill();
            })
            .linkLabel(buildLinkTooltip)
            .linkColor(linkColor)
            .linkWidth(function (l) { return 1.4; })
            .linkCurvature(0.08)
            .linkDirectionalArrowLength(5)
            .linkDirectionalArrowRelPos(useBoxes ? boxArrowRelPos : 0.92)
            .linkDirectionalArrowColor(linkColor)
            .linkDirectionalParticles(2)
            .linkDirectionalParticleSpeed(0.006)
            .linkDirectionalParticleWidth(2)
            .linkDirectionalParticleColor(linkColor)
            .onNodeClick(handleNodeClick)
            // force-graph reports the hover from its render loop, after our mousemove: it must not wipe the hand.
            .onNodeHover(function (node) { container.style.cursor = node ? 'pointer' : (_handOverHalo ? 'grab' : null); })
            // Files view: the layout is settled in the warm-up, before the first frame; the
            // cooldown only serves the drags (each reheat runs this many ticks of local tidying).
            .warmupTicks(useBoxes ? 200 : 0)
            .cooldownTicks(useBoxes ? 40 : 120)
            .onRenderFramePre(function (ctx, globalScale) {
                drawClusterHalos2D(ctx, globalScale);
            })
            .onRenderFramePost(function () {
                if (useBoxes) positionBoxes(g);
            })
            .graphData(data);

        try {
            if (g.d3Force) {
                // No global forces in the Files view: the circles decide where things are, the
                // links are only drawn. Concepts keep charge and links to spread the circles.
                const charge = g.d3Force('charge'); if (charge) charge.strength(useBoxes ? 0 : -280);
                const linkF  = g.d3Force('link');   if (linkF) { linkF.distance(80); if (useBoxes) linkF.strength(0); }
                g.d3Force('center', null);   // force-graph's default centering would drag the whole picture
                g.d3Force('cluster', folderForce2D);
                if (useBoxes) g.d3Force('boxCollide', boxCollideForce2D);
            }
        } catch (e) { /* noop */ }
        gRef.g = g;
        // ForceGraph empties the container when it is created: the measured layer goes back in, above the canvas.
        if (layer) container.appendChild(layer);
        g.fitAll = function (ms) { fitToFolders(g, container, ms); };

        // Auto-fit on first stabilization. With boxes only the first time: opening or dragging
        // a box reheats the simulation, and refitting then would move the view under the mouse.
        // Files view: the warm-up already settled the boxes, so the picture is framed at once,
        // without animation, and never refitted (a refit would move the view under the mouse).
        let fitted = false;
        if (useBoxes) {
            settleBoxes();
            try { g.fitAll(0); } catch (e) {}
            fitted = true;
        }
        g.onEngineStop(function () {
            if (useBoxes) settleBoxes();
            if (fitted) return;
            fitted = true;
            try { g.fitAll(400); } catch (e) {}
        });
        bindHaloDrag(container, g);
        return g;
    }

    // ---- Cluster halos (2D, drawn under nodes) -------------------------------
    /**
     * Draws the folder circles from the layout (fixed centre and radius, parents before their
     * children so the children sit on top) and keeps them in _layout.halos (graph coordinates)
     * for the mouse (see startHaloDrag). A circle's members are every node inside it, children included.
     */
    function drawClusterHalos2D(ctx, globalScale) {
        if (!_data || !_layout) return;
        const halos = Object.create(null);
        _layout.halos = halos;
        const fillA   = _isDark ? 0.16 : 0.13;
        const strokeA = _isDark ? 0.38 : 0.30;
        function draw(b) {
            const f = _layout.folders[b];
            if (!f || !isFinite(f.cx) || !isFinite(f.cy)) return;
            halos[b] = { bucket: b, cx: f.cx, cy: f.cy, r: f.r, members: descendantsOf(_layout, b) };
            const color = PALETTE[hashStr(b) % PALETTE.length];
            ctx.save();
            ctx.fillStyle = hexToRgba(color, fillA);
            ctx.strokeStyle = hexToRgba(color, strokeA);
            ctx.lineWidth = 1.4 / globalScale;
            ctx.setLineDash([6 / globalScale, 5 / globalScale]);
            ctx.beginPath();
            ctx.arc(f.cx, f.cy, f.r, 0, Math.PI * 2);
            ctx.fill();
            ctx.stroke();
            ctx.setLineDash([]);
            const label = friendlyBucketLabel(b);
            if (label) {
                const fs = 13 / globalScale;
                ctx.font = '700 ' + fs + 'px -apple-system, "Segoe UI", Inter, system-ui, sans-serif';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'bottom';
                ctx.fillStyle = hexToRgba(color, _isDark ? 0.92 : 0.78);
                ctx.fillText(label, f.cx, f.cy - f.r - 4 / globalScale);
            }
            ctx.restore();
            f.children.forEach(draw);
        }
        _layout.top.forEach(draw);
    }

    // ---- Folder circles: drag one and its nodes follow -------------------------
    /** The circle under a graph point; the smallest one where circles overlap. */
    function haloAt(gx, gy) {
        if (!_layout || !_layout.halos) return null;
        const halos = _layout.halos;
        let best = null;
        for (const b in halos) {
            const h = halos[b];
            if (Math.hypot(gx - h.cx, gy - h.cy) > h.r) continue;
            if (!best || h.r < best.r) best = h;
        }
        return best;
    }

    /** A node drawn on the canvas (Concepts view) under a graph point: its own drag and click win over the circle. */
    function canvasNodeAt(gx, gy) {
        if (!_data) return null;
        for (let i = 0; i < _data.nodes.length; i++) {
            const n = _data.nodes[i];
            if (n._box || !isFinite(n.x) || !isFinite(n.y)) continue;
            if (Math.hypot(gx - n.x, gy - n.y) <= nodeRadius(n) + 2) return n;
        }
        return null;
    }

    /**
     * The mouse on the canvas: a hand over a circle, and a press on it starts the drag of
     * the circle. Listened in the capture phase because the canvas's own mousedown
     * (d3-zoom) would start a pan instead. The boxes of the Files view are HTML above the
     * canvas, so a press on a box never gets here.
     */
    function bindHaloDrag(container, g) {
        function graphPoint(ev) {
            const rect = container.getBoundingClientRect();
            return g.screen2GraphCoords(ev.clientX - rect.left, ev.clientY - rect.top);
        }
        container.addEventListener('mousemove', function (ev) {
            if (ev.buttons || !(ev.target instanceof HTMLCanvasElement)) return;
            const p = graphPoint(ev);
            if (canvasNodeAt(p.x, p.y)) { _handOverHalo = false; return; }   // force-graph shows its pointer
            const over = !!haloAt(p.x, p.y);
            if (over) { container.style.cursor = 'grab'; _handOverHalo = true; }
            else if (_handOverHalo) { container.style.cursor = ''; _handOverHalo = false; }
        });
        container.addEventListener('mousedown', function (ev) {
            if (ev.button !== 0 || !(ev.target instanceof HTMLCanvasElement)) return;
            const p = graphPoint(ev);
            if (canvasNodeAt(p.x, p.y)) return;
            const halo = haloAt(p.x, p.y);
            if (!halo) return;
            ev.preventDefault();
            ev.stopPropagation();
            startHaloDrag(ev, halo, container, g);
        }, true);
    }

    /**
     * Dragging a folder circle moves the circle, the circles inside it and every node in them
     * by the same offset, so the folder force keeps the nodes in the new place; a child circle
     * cannot leave its parent. Like a dragged box, the nodes stay where they are dropped (pinned).
     * The simulation is reheated on every move, like the box drag: the outer ForceGraph
     * exposes only d3ReheatSimulation (alpha back to 1), not resetCountdown/alphaTarget
     * (verified in force-graph 1.51, 2026-10-09). The other nodes make room; a higher
     * velocity decay while the mouse is down keeps them from being thrown around.
     */
    const DRAG_VELOCITY_DECAY = 0.75;   // force-graph default: 0.4
    function startHaloDrag(e, halo, container, g) {
        const rect = container.getBoundingClientRect();
        const start = g.screen2GraphCoords(e.clientX - rect.left, e.clientY - rect.top);
        const folders = _layout.folders;
        const folder = folders[halo.bucket];
        // The circle, the circles inside it and every node in them move together.
        const circles = [];
        (function walk(b) { const f = folders[b]; if (!f) return; circles.push({ f: f, cx: f.cx, cy: f.cy }); f.children.forEach(walk); })(halo.bucket);
        const members = halo.members.map(function (n) { return { node: n, x: n.x, y: n.y }; });
        const parent = folder && folder.parent ? folders[folder.parent] : null;
        container.style.cursor = 'grabbing';
        _handOverHalo = false;
        members.forEach(function (m) { if (m.node._box) m.node._box.classList.add('kgBoxDragging'); });
        const restingDecay = g.d3VelocityDecay();
        g.d3VelocityDecay(DRAG_VELOCITY_DECAY);
        function move(ev) {
            const p = g.screen2GraphCoords(ev.clientX - rect.left, ev.clientY - rect.top);
            let dx = p.x - start.x, dy = p.y - start.y;
            if (parent && folder) {
                // A child circle stays inside its parent: the drag is clamped to the room there.
                const room = Math.max(0, parent.r - folder.r - 6);
                const ox = circles[0].cx + dx - parent.cx, oy = circles[0].cy + dy - parent.cy;
                const od = Math.hypot(ox, oy);
                if (od > room) {
                    dx = parent.cx + ox * room / od - circles[0].cx;
                    dy = parent.cy + oy * room / od - circles[0].cy;
                }
            }
            circles.forEach(function (c) { c.f.cx = c.cx + dx; c.f.cy = c.cy + dy; });
            members.forEach(function (m) { m.node.fx = m.x + dx; m.node.fy = m.y + dy; });
            pushCirclesApart(halo.bucket);
            g.d3ReheatSimulation();
        }
        function up() {
            document.removeEventListener('mousemove', move);
            document.removeEventListener('mouseup', up);
            members.forEach(function (m) { if (m.node._box) m.node._box.classList.remove('kgBoxDragging'); });
            container.style.cursor = '';
            g.d3VelocityDecay(restingDecay);
        }
        document.addEventListener('mousemove', move);
        document.addEventListener('mouseup', up);
    }

    /** The folder circles in screen pixels of the graph's container (what the e2e proofs grab). */
    function foldersOnScreen() {
        if (!_graph || !_layout || !_layout.halos) return [];
        const out = [];
        const halos = _layout.halos;
        for (const b in halos) {
            const h = halos[b];
            const c = _graph.graph2ScreenCoords(h.cx, h.cy);
            out.push({ folder: b, label: friendlyBucketLabel(b), parent: (_layout.folders[b] || {}).parent || null, cx: c.x, cy: c.y, r: h.r * _graph.zoom(), nodes: h.members.length });
        }
        return out;
    }

    // ---- Overlay ------------------------------------------------------------
    function buildOverlay() {
        const o = document.createElement('div');
        o.className = 'kgOverlay';
        o.innerHTML =
            '<div class="kgHeader">' +
                '<div class="kgTitle"><span class="kgTitleIcon">◈</span>Knowledge Graph</div>' +
                '<div class="kgViewTabs" role="tablist" aria-label="Source">' +
                    '<button type="button" class="kgViewTab" data-source="files"    role="tab" title="Links between markdown files (legacy view)">Files</button>' +
                    '<button type="button" class="kgViewTab" data-source="concepts" role="tab" title="Concept graph from .kg.md (Neo4j)">Concepts</button>' +
                '</div>' +
                '<select class="kgNsPicker" style="margin-left:8px;display:none" title="Namespace filter (concepts only)">' +
                    '<option value="">All namespaces</option>' +
                '</select>' +
                '<input type="text" class="kgFocusCtl kgFocusInput" list="kgNodeList" placeholder="Vai al campo…" title="Isola un campo e le sue relazioni" style="display:none">' +
                '<datalist id="kgNodeList"></datalist>' +
                '<select class="kgFocusCtl kgFocusDir" title="Direzione del focus" style="display:none">' +
                    '<option value="up">◄ a monte</option>' +
                    '<option value="down">► a valle</option>' +
                    '<option value="both" selected>↔ entrambe</option>' +
                '</select>' +
                '<select class="kgFocusCtl kgFocusDepth" title="Profondità" style="display:none">' +
                    '<option value="1">1 salto</option>' +
                    '<option value="2">2 salti</option>' +
                    '<option value="3">3 salti</option>' +
                    '<option value="0" selected>tutto</option>' +
                '</select>' +
                '<button type="button" class="kgBtn kgBtnIcon kgFocusClear" data-act="focus-clear" title="Esci dal focus" style="display:none">✕</button>' +
                '<div class="kgHeaderSpacer"></div>' +
                '<div class="kgActions">' +
                    '<button type="button" class="kgBtn kgBtnIcon kgSanityBtn" data-act="sanity" title="Sanity report" style="display:none">⚕</button>' +
                    '<button type="button" class="kgBtn kgBtnIcon" data-act="refresh" title="Refresh">⟳</button>' +
                    '<button type="button" class="kgBtn kgBtnClose" data-act="close" title="Close (Esc)">✕</button>' +
                '</div>' +
            '</div>' +
            '<div class="kgBody"></div>' +
            '<aside class="kgSanityDrawer" style="display:none">' +
                '<div class="kgSanityHeader"><span>Sanity report</span>' +
                    '<button type="button" class="kgBtn kgBtnIcon" data-act="sanity-close" title="Close panel">✕</button>' +
                '</div>' +
                '<div class="kgSanityBody"><div class="kgSanityLoading">Loading…</div></div>' +
            '</aside>' +
            '<div class="kgFooter">' +
                '<div class="kgLegend">' +
                    '<span class="kgLegendGroup"><strong>Nodes</strong>' +
                        '<span><i class="kgDot kgDotCenter"></i>current</span>' +
                        '<span><i class="kgDot kgDotByFolder"></i>colored by folder</span>' +
                    '</span>' +
                    '<span class="kgLegendDivider"></span>' +
                    '<span class="kgLegendGroup"><strong>Edges</strong>' +
                        '<span><i class="kgLine kgLineLink"></i>link</span>' +
                        '<span><i class="kgLine kgLinePub"></i>publication</span>' +
                        '<span><i class="kgLine kgLineExc"></i>excerpt</span>' +
                        '<span><i class="kgLine kgLinePum"></i>plantuml</span>' +
                    '</span>' +
                '</div>' +
                '<div class="kgHint">drag to pan • scroll to zoom • click a node to open</div>' +
            '</div>' +
            '<div class="kgLoading"><div class="kgSpinner"></div><div>Building graph…</div></div>';
        document.body.appendChild(o);

        o.addEventListener('click', function (e) {
            const btn = e.target.closest('button');
            if (!btn) return;
            const act = btn.getAttribute('data-act');
            const source = btn.getAttribute('data-source');
            if (act === 'close') closeOverlay();
            else if (act === 'refresh') refresh();
            else if (act === 'sanity') toggleSanityPanel();
            else if (act === 'sanity-close') hideSanityPanel();
            else if (act === 'focus-clear') clearFocus();
            else if (source) setSource(source);
        });
        const nsPicker = o.querySelector('.kgNsPicker');
        if (nsPicker) {
            nsPicker.addEventListener('change', function () {
                _selectedNamespace = nsPicker.value || '';
                if (_source === 'concepts') refresh();
            });
        }
        const focusInput = o.querySelector('.kgFocusInput');
        if (focusInput) focusInput.addEventListener('change', onFocusInputChange);
        ['.kgFocusDir', '.kgFocusDepth'].forEach(function (sel) {
            const el = o.querySelector(sel);
            if (el) el.addEventListener('change', function () { if (_focusId) applyView(); });
        });
        document.addEventListener('keydown', escClose);
        window.addEventListener('resize', resize);
        return o;
    }

    function setActiveTab() {
        if (!_overlay) return;
        const tabs = _overlay.querySelectorAll('.kgViewTab');
        tabs.forEach(function (t) {
            const s = t.getAttribute('data-source');
            t.classList.toggle('kgViewTabActive', !!s && s === _source);
        });
        const hint = _overlay.querySelector('.kgHint');
        if (hint) {
            hint.textContent = _source === 'files'
                ? 'drag the background to pan • scroll to zoom • ▸ TL;DR • click a name to open • drag a box, or a folder circle with its files, to move it'
                : 'drag to pan • scroll to zoom • click a node • drag a circle to move it with its concepts';
        }
        const nsPicker = _overlay.querySelector('.kgNsPicker');
        if (nsPicker) nsPicker.style.display = _source === 'concepts' ? '' : 'none';
        const showFocus = _source === 'concepts';
        ['.kgFocusInput', '.kgFocusDir', '.kgFocusDepth'].forEach(function (sel) {
            const el = _overlay.querySelector(sel);
            if (el) el.style.display = showFocus ? '' : 'none';
        });
        const focusClear = _overlay.querySelector('.kgFocusClear');
        if (focusClear) focusClear.style.display = (showFocus && _focusId) ? '' : 'none';
        const sanityBtn = _overlay.querySelector('.kgSanityBtn');
        if (sanityBtn) sanityBtn.style.display = _source === 'concepts' ? '' : 'none';
        if (_source !== 'concepts') hideSanityPanel();
    }

    // ---- Sanity report panel ---------------------------------------------------

    function toggleSanityPanel() {
        if (!_overlay) return;
        const drawer = _overlay.querySelector('.kgSanityDrawer');
        if (!drawer) return;
        if (drawer.style.display === 'none' || drawer.style.display === '') {
            drawer.style.display = 'flex';
            loadSanity();
        } else {
            hideSanityPanel();
        }
    }

    function hideSanityPanel() {
        if (!_overlay) return;
        const drawer = _overlay.querySelector('.kgSanityDrawer');
        if (drawer) drawer.style.display = 'none';
    }

    function loadSanity() {
        const body = _overlay && _overlay.querySelector('.kgSanityBody');
        if (!body) return;
        body.innerHTML = '<div class="kgSanityLoading">Loading…</div>';
        resolveProjectIdAsync().then(function (projectId) {
            if (!projectId) { body.innerHTML = '<div class="kgSanityErr">No project resolved.</div>'; return; }
            let url = '/api/kg/sanity/' + encodeURIComponent(projectId);
            if (_selectedNamespace) url += '?ns=' + encodeURIComponent(_selectedNamespace);
            return $.get(url);
        }).then(function (resp) {
            if (!resp) return;
            body.innerHTML = renderSanityHtml(resp);
            // Wire click-to-focus on each finding
            body.querySelectorAll('[data-focus-id]').forEach(function (el) {
                el.addEventListener('click', function () {
                    const id = el.getAttribute('data-focus-id');
                    focusNodeById(id);
                });
            });
        }).fail(function (xhr) {
            body.innerHTML = '<div class="kgSanityErr">Failed: HTTP ' + (xhr ? xhr.status : '?') + '</div>';
        });
    }

    function renderSanityHtml(r) {
        const sb = [];
        const ratio = r.relatedToRatio || { totalEdges: 0, relatedToEdges: 0, ratio: 0 };
        const ratioPct = Math.round(ratio.ratio * 100);
        const ratioCls = ratio.ratio >= 0.5 ? 'kgSanityWarn' : '';
        sb.push('<div class="kgSanitySec ' + ratioCls + '">');
        sb.push('<h4>RELATED_TO ratio</h4>');
        sb.push('<p>' + ratio.relatedToEdges + ' of ' + ratio.totalEdges + ' edges (' + ratioPct + '%) use the fallback type.');
        if (ratio.ratio >= 0.5) sb.push(' &nbsp;<strong>⚠ over 50% — closed vocabulary not respected</strong>');
        sb.push('</p>');
        sb.push('</div>');

        const orphans = r.orphans || [];
        sb.push('<div class="kgSanitySec"><h4>Orphan concepts (no relationships) — ' + orphans.length + '</h4>');
        if (orphans.length === 0) sb.push('<p class="kgSanityOk">None ✓</p>');
        else {
            sb.push('<ul>');
            orphans.slice(0, 50).forEach(function (o) {
                const id = (o.graph || '') + '::' + (o.name || '');
                sb.push('<li><a href="#" data-focus-id="' + escapeHtml(id) + '"><strong>' + escapeHtml(o.name) + '</strong></a> <span class="kgMuted">(' + escapeHtml(o.graph) + ')</span></li>');
            });
            if (orphans.length > 50) sb.push('<li class="kgMuted">… +' + (orphans.length - 50) + ' more</li>');
            sb.push('</ul>');
        }
        sb.push('</div>');

        const hot = r.hot || [];
        sb.push('<div class="kgSanitySec"><h4>Hot concepts (degree > 10) — ' + hot.length + '</h4>');
        if (hot.length === 0) sb.push('<p class="kgSanityOk">None ✓</p>');
        else {
            sb.push('<ul>');
            hot.slice(0, 50).forEach(function (h) {
                const id = (h.graph || '') + '::' + (h.name || '');
                sb.push('<li><a href="#" data-focus-id="' + escapeHtml(id) + '"><strong>' + escapeHtml(h.name) + '</strong></a> <span class="kgMuted">(' + escapeHtml(h.graph) + ', deg ' + h.degree + ')</span></li>');
            });
            sb.push('</ul>');
        }
        sb.push('</div>');

        const cols = r.casingCollisions || [];
        sb.push('<div class="kgSanitySec ' + (cols.length > 0 ? 'kgSanityWarn' : '') + '"><h4>Casing collisions — ' + cols.length + '</h4>');
        if (cols.length === 0) sb.push('<p class="kgSanityOk">None ✓</p>');
        else {
            sb.push('<ul>');
            cols.forEach(function (c) {
                sb.push('<li><code>' + escapeHtml(c.key) + '</code> → ' + (c.names || []).map(function (n) { return '<strong>' + escapeHtml(n) + '</strong>'; }).join(', ') + '</li>');
            });
            sb.push('</ul>');
        }
        sb.push('</div>');
        return sb.join('');
    }

    function focusNodeById(nodeId) {
        if (!_graph || !_data) return;
        const node = _data.nodes.find(function (n) { return n.id === nodeId; });
        if (!node) return;
        try {
            if (_graph.centerAt) {
                if (node.x != null && node.y != null) _graph.centerAt(node.x, node.y, 800);
                if (_graph.zoom) _graph.zoom(3, 800);
            }
        } catch (e) { /* noop */ }
    }

    function setSource(source) {
        if (source !== 'files' && source !== 'concepts') return;
        if (source === _source) return;
        _source = source;
        setActiveTab();
        const body = _overlay && _overlay.querySelector('.kgBody');
        if (body) body.innerHTML = '';
        _graph = null;
        _data = null;
        _layout = null;
        _fullData = null;
        _focusId = null;
        loadAndRender();
    }

    function escClose(e) { if (e.key === 'Escape') closeOverlay(); }

    function showLoading(on) {
        if (!_overlay) return;
        const l = _overlay.querySelector('.kgLoading');
        if (l) l.style.display = on ? 'flex' : 'none';
    }

    function showError(msg) {
        if (!_overlay) return;
        const body = _overlay.querySelector('.kgBody');
        if (body) body.innerHTML = '<div class="kgEmpty"><div class="kgEmptyIcon">⚠</div><div>' + (msg || 'Failed to load Knowledge Graph.') + '</div></div>';
        showLoading(false);
    }

    function showEmpty() {
        if (!_overlay) return;
        const body = _overlay.querySelector('.kgBody');
        if (body) body.innerHTML = '<div class="kgEmpty"><div class="kgEmptyIcon">🕸️</div><div>No incoming or outgoing links indexed for this document.</div></div>';
        showLoading(false);
    }

    function renderActive() {
        if (!_overlay || !_data) return;
        const body = _overlay.querySelector('.kgBody');
        if (!body) return;
        body.innerHTML = '';
        // The picture stays hidden (kgSettling) until it is framed: no flash of an unfitted graph.
        body.classList.add('kgSettling');
        // Wait two animation frames so the container has its final layout
        // (the overlay just entered the DOM and is still transitioning).
        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                _graph = build2D(body, _data);
                // Belt-and-suspenders: re-measure once the canvas is settled
                // and re-fit, in case the initial container rect was stale.
                setTimeout(function () {
                    try {
                        const r = body.getBoundingClientRect();
                        if (_graph && r.width > 0 && r.height > 0) _graph.width(r.width).height(r.height);
                        if (_graph && _graph.fitAll) _graph.fitAll(0);
                        else if (_graph && _graph.zoomToFit) _graph.zoomToFit(400, 60);
                    } catch (e) { /* noop */ }
                    requestAnimationFrame(function () { body.classList.remove('kgSettling'); });
                }, 250);
            });
        });
    }

    function loadAndRender() {
        if (_source === 'concepts') {
            loadConceptGraphAndRender();
            return;
        }
        const pathFile = getDocumentPath();
        if (!pathFile) { showError('Could not determine current document path.'); return; }
        showLoading(true);
        const conn = $('#MdBody').attr('connectionid');
        let url = '/api/tabcontroller/GetKnowledgeGraph?fullPathFile=' + encodeURIComponent(pathFile) + '&depth=1';
        if (conn) url += '&connectionid=' + encodeURIComponent(conn);
        $.get(url)
            .done(function (raw) {
                const data = normalize(raw);
                _data = data;
                _layout = computeLayout(data);
                showLoading(false);
                if (!data.nodes.length || (data.nodes.length === 1 && data.links.length === 0)) {
                    showEmpty();
                    return;
                }
                renderActive();
            })
            .fail(function (xhr) {
                console.error('[KG] fetch failed', xhr && xhr.status, xhr && xhr.statusText);
                showError('Failed to load Knowledge Graph (HTTP ' + (xhr ? xhr.status : '?') + ').');
            });
    }

    // ---- Concept graph (Neo4j) source -----------------------------------------

    function normalizeConcepts(raw) {
        if (!raw || !raw.nodes) return { nodes: [], links: [] };
        return {
            nodes: raw.nodes.map(function (n) {
                return {
                    id: n.id,
                    label: n.name || n.id,
                    cluster: n.graph,       // cluster halo grouped by graph namespace
                    mdContext: n.graph,     // reused for legend / friendly label
                    sourceDocs: n.sourceDocs || [],
                    nodeType: n.type || '',     // secondary label (TargetField, Transformation, …)
                    kind: n.kind || '',         // transformation kind (LOOKUP, CONDITIONAL, …)
                    rule: n.rule || '',         // transformation logic
                    description: n.description || '',
                    docPath: n.docPath || '',
                    lineStart: n.lineStart,
                    lineEnd: n.lineEnd,
                    isCenter: false,
                    isExternal: false,
                    inDegree: 0,
                    outDegree: 0,
                    tldr: n.description || ''   // feed the shared node tooltip
                };
            }),
            links: (raw.links || []).map(function (l) {
                return {
                    source: l.source,
                    target: l.target,
                    linkType: (l.type || 'link').toLowerCase(),
                    relType: l.type || 'link',
                    role: l.role || '',
                    description: l.description || '',
                    sourceDocs: l.sourceDocs || []
                };
            })
        };
    }

    function resolveProjectIdAsync() {
        if (_currentProjectId) return $.Deferred().resolve(_currentProjectId).promise();
        const pathFile = getDocumentPath();
        return $.get('/api/MdProjects/GetProjects').then(function (projects) {
            if (!projects || !projects.length) return null;
            const norm = function (p) { return (p || '').replace(/\\/g, '/').replace(/\/$/, '').toLowerCase(); };
            const target = norm(pathFile);
            let best = null;
            for (let i = 0; i < projects.length; i++) {
                const p = projects[i];
                const pp = norm(p.path || '');
                if (!pp) continue;
                if (target === pp || target.indexOf(pp + '/') === 0) {
                    if (!best || pp.length > norm(best.path).length) best = p;
                }
            }
            _currentProjectId = best ? best.id : null;
            return _currentProjectId;
        });
    }

    function populateNamespacePicker(namespaces) {
        if (!_overlay) return;
        const picker = _overlay.querySelector('.kgNsPicker');
        if (!picker) return;
        const current = _selectedNamespace;
        picker.innerHTML = '<option value="">All namespaces</option>';
        (namespaces || []).forEach(function (ns) {
            const opt = document.createElement('option');
            opt.value = ns.graph;
            opt.textContent = ns.graph + ' (' + ns.conceptCount + ')';
            if (ns.graph === current) opt.selected = true;
            picker.appendChild(opt);
        });
    }

    function loadConceptGraphAndRender() {
        showLoading(true);
        resolveProjectIdAsync().then(function (projectId) {
            if (!projectId) { showError('Could not resolve current project for concept graph.'); return; }
            $.get('/api/kg/query/namespaces/' + encodeURIComponent(projectId))
                .done(function (nsResp) {
                    _availableNamespaces = (nsResp && nsResp.namespaces) || [];
                    populateNamespacePicker(_availableNamespaces);
                });
            let url = '/api/kg/graph/' + encodeURIComponent(projectId);
            if (_selectedNamespace) url += '?ns=' + encodeURIComponent(_selectedNamespace);
            return $.get(url);
        }).then(function (raw) {
            if (!raw) return;
            const data = normalizeConcepts(raw);
            showLoading(false);
            if (!data.nodes.length) {
                _fullData = null;
                const body = _overlay && _overlay.querySelector('.kgBody');
                if (body) body.innerHTML = '<div class="kgEmpty"><div class="kgEmptyIcon">🕸️</div><div>No concepts in Neo4j yet — sync from Project Settings → Knowledge Graph.</div></div>';
                return;
            }
            _fullData = data;
            _focusId = null;
            const fi = _overlay && _overlay.querySelector('.kgFocusInput');
            if (fi) fi.value = '';
            populateNodeDatalist(data.nodes);
            setActiveTab();
            applyView();
        }).fail(function (xhr) {
            console.error('[KG] concept fetch failed', xhr && xhr.status, xhr && xhr.statusText);
            const errBody = xhr && xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error : ('HTTP ' + (xhr ? xhr.status : '?'));
            showError('Failed to load concept graph: ' + errBody);
        });
    }

    // ---- Focus mode: isolate a node's neighborhood --------------------------
    // The concept graph is loaded whole into _fullData; _data is the rendered
    // subset. With _focusId set, only that node's directed neighborhood
    // (upstream / downstream / both, to a depth) survives — the rest is removed.

    function focusDir() {
        const s = _overlay && _overlay.querySelector('.kgFocusDir');
        return s ? s.value : 'up';
    }
    function focusDepth() {
        const s = _overlay && _overlay.querySelector('.kgFocusDepth');
        return s ? (parseInt(s.value, 10) || 0) : 0;   // 0 = unlimited
    }
    function linkEndId(e) { return (e && typeof e === 'object') ? e.id : e; }

    function populateNodeDatalist(nodes) {
        if (!_overlay) return;
        const dl = _overlay.querySelector('#kgNodeList');
        if (!dl) return;
        const seen = Object.create(null);
        const opts = [];
        (nodes || []).forEach(function (n) {
            const lbl = n.label || n.id;
            if (!lbl || seen[lbl]) return;
            seen[lbl] = true;
            opts.push('<option value="' + escapeHtml(lbl) + '"></option>');
        });
        dl.innerHTML = opts.join('');
    }

    function onFocusInputChange() {
        const input = _overlay && _overlay.querySelector('.kgFocusInput');
        if (!input || !_fullData) return;
        const val = (input.value || '').trim();
        if (!val) { clearFocus(); return; }
        const node = _fullData.nodes.find(function (n) {
            return n.label === val || n.id === val;
        });
        if (!node) return;            // typed text matches no node — leave as-is
        _focusId = node.id;
        setActiveTab();
        applyView();
    }

    function clearFocus() {
        _focusId = null;
        const input = _overlay && _overlay.querySelector('.kgFocusInput');
        if (input) input.value = '';
        setActiveTab();
        applyView();
    }

    // Click a node → focus on it. Direction/depth combos drive the chain.
    function focusOnNode(node) {
        if (!node || !node.id) return;
        _focusId = node.id;
        const input = _overlay && _overlay.querySelector('.kgFocusInput');
        if (input) input.value = node.label || node.id;
        setActiveTab();
        applyView();
    }

    function handleNodeClick(node) {
        if (_source === 'concepts') { focusOnNode(node); return; }
        openFileNode(node);
    }

    function computeFocusSet(focusId, dir, maxDepth) {
        const succ = Object.create(null), pred = Object.create(null);
        _fullData.links.forEach(function (l) {
            const s = linkEndId(l.source), t = linkEndId(l.target);
            (succ[s] = succ[s] || []).push(t);
            (pred[t] = pred[t] || []).push(s);
        });
        // A walk is MONOTONIC: once it follows a direction it never turns back.
        // 'down' follows successors only, 'up' follows predecessors only — so
        // the chain stops at its ends instead of re-spreading from every node.
        function walk(adj) {
            const seen = Object.create(null);
            seen[focusId] = true;
            let frontier = [focusId];
            let depth = 0;
            while (frontier.length && (maxDepth === 0 || depth < maxDepth)) {
                const next = [];
                frontier.forEach(function (id) {
                    (adj[id] || []).forEach(function (nid) {
                        if (!seen[nid]) { seen[nid] = true; next.push(nid); }
                    });
                });
                frontier = next;
                depth++;
            }
            return seen;
        }
        const keep = Object.create(null);
        keep[focusId] = true;
        // 'both' = pure-upstream walk UNION pure-downstream walk, never mixed.
        if (dir === 'down' || dir === 'both') {
            const d = walk(succ);
            for (var k1 in d) keep[k1] = true;
        }
        if (dir === 'up' || dir === 'both') {
            const u = walk(pred);
            for (var k2 in u) keep[k2] = true;
        }
        return keep;
    }

    // Recompute _data from _fullData applying the current focus, then render.
    // Fresh object copies go to the renderer so _fullData stays pristine
    // (ForceGraph mutates the node/link objects it is handed).
    function applyView() {
        if (!_fullData) return;
        let nodes = _fullData.nodes;
        let links = _fullData.links;
        if (_focusId && nodes.some(function (n) { return n.id === _focusId; })) {
            const keep = computeFocusSet(_focusId, focusDir(), focusDepth());
            nodes = nodes.filter(function (n) { return keep[n.id]; });
            links = links.filter(function (l) {
                return keep[linkEndId(l.source)] && keep[linkEndId(l.target)];
            });
        } else {
            _focusId = null;
        }
        _data = {
            nodes: nodes.map(function (n) {
                const c = Object.assign({}, n);
                if (_focusId && c.id === _focusId) c.isCenter = true;   // highlight the chain's origin
                return c;
            }),
            links: links.map(function (l) {
                return {
                    source: linkEndId(l.source),
                    target: linkEndId(l.target),
                    linkType: l.linkType,
                    relType: l.relType,
                    description: l.description
                };
            })
        };
        _layout = computeLayout(_data);
        renderActive();
    }

    function resize() {
        if (!_graph || !_overlay) return;
        const body = _overlay.querySelector('.kgBody');
        if (!body) return;
        const r = body.getBoundingClientRect();
        if (r.width > 0 && r.height > 0) _graph.width(r.width).height(r.height);
    }

    function openOverlay() {
        if (_overlay) return;
        _isDark = detectDark();
        _overlay = buildOverlay();
        if (_isDark) _overlay.classList.add('kgDark');
        setActiveTab();
        requestAnimationFrame(function () { _overlay.classList.add('kgOpen'); });
        loadAndRender();
    }

    function closeOverlay() {
        if (!_overlay) return;
        document.removeEventListener('keydown', escClose);
        window.removeEventListener('resize', resize);
        _overlay.classList.remove('kgOpen');
        const o = _overlay;
        setTimeout(function () { if (o && o.parentNode) o.parentNode.removeChild(o); }, 200);
        _overlay = null;
        _graph = null;
        _data = null;
        _layout = null;
        _currentProjectId = null;
        _availableNamespaces = [];
        _selectedNamespace = '';
        _fullData = null;
        _focusId = null;
        _source = 'files';
    }

    function refresh() {
        if (!_overlay) return;
        const body = _overlay.querySelector('.kgBody');
        if (body) body.innerHTML = '';
        _graph = null;
        _data = null;
        _layout = null;
        _fullData = null;
        _focusId = null;
        loadAndRender();
    }

    window.openKnowledgeGraph   = openOverlay;
    window.closeKnowledgeGraph  = closeOverlay;
    window.toggleKnowledgeGraph = function () { _overlay ? closeOverlay() : openOverlay(); };
    window.MdeKnowledgeGraph = {
        open: openOverlay,
        close: closeOverlay,
        toggle: window.toggleKnowledgeGraph,
        refresh: refresh,
        resize: resize,
        folders: foldersOnScreen
    };
})();
