/**
 * MdExplorer - Knowledge Graph (fullscreen, 2D + 3D)
 * ===================================================
 * Fullscreen interactive force-directed graph of links between markdown files.
 * Two view modes:
 *   - 2D (default): canvas, top-down map — best for "big picture" overview
 *   - 3D: WebGL, orbital — best for exploring clusters in depth
 *
 * Backend:
 * - GET /api/tabcontroller/GetKnowledgeGraph?fullPathFile=...&depth=1&connectionid=...
 *
 * UMD globals required:
 * - THREE          (three@0.147)              — only for 3D
 * - ForceGraph3D   (3d-force-graph@1.73)      — only for 3D
 * - ForceGraph     (force-graph@1.51)         — for 2D
 *
 * Public API:
 * - window.openKnowledgeGraph()
 * - window.closeKnowledgeGraph()
 * - window.toggleKnowledgeGraph()
 * - window.MdeKnowledgeGraph.{open,close,toggle,refresh,resize,setMode}
 */
(function () {
    'use strict';

    let _overlay = null;
    let _graph = null;          // active graph instance (2D or 3D)
    let _mode = '2d';           // default
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
        if (b === '__root__') return 'root';
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

    // ---- Cluster layout: assign each folder a slot on a ring ---------------
    function computeLayout(data) {
        const buckets = [];
        const seen = Object.create(null);
        for (let i = 0; i < data.nodes.length; i++) {
            const n = data.nodes[i];
            if (n.isCenter) continue;
            const b = bucketFor(n);
            if (!seen[b]) { seen[b] = true; buckets.push(b); }
        }
        const centers = Object.create(null);
        const count = buckets.length;
        const ringR = Math.max(260, 90 + count * 38);
        for (let i = 0; i < count; i++) {
            const angle = (i / count) * Math.PI * 2 - Math.PI / 2;
            centers[buckets[i]] = { x: Math.cos(angle) * ringR, y: Math.sin(angle) * ringR };
        }
        return { buckets: buckets, centers: centers, ringR: ringR };
    }

    function clusterForce2D(alpha) {
        if (!_data || !_layout) return;
        const k = 0.09 * alpha;
        const kC = 0.2 * alpha;
        const nodes = _data.nodes;
        for (let i = 0; i < nodes.length; i++) {
            const n = nodes[i];
            if (n.isCenter) {
                n.vx = (n.vx || 0) + (0 - (n.x || 0)) * kC;
                n.vy = (n.vy || 0) + (0 - (n.y || 0)) * kC;
                continue;
            }
            const c = _layout.centers[bucketFor(n)];
            if (!c) continue;
            n.vx = (n.vx || 0) + (c.x - (n.x || 0)) * k;
            n.vy = (n.vy || 0) + (c.y - (n.y || 0)) * k;
        }
    }

    function clusterForce3D(alpha) {
        if (!_data || !_layout) return;
        const k = 0.06 * alpha;
        const kC = 0.15 * alpha;
        const nodes = _data.nodes;
        for (let i = 0; i < nodes.length; i++) {
            const n = nodes[i];
            if (n.isCenter) {
                n.vx = (n.vx || 0) + (0 - (n.x || 0)) * kC;
                n.vy = (n.vy || 0) + (0 - (n.y || 0)) * kC;
                continue;
            }
            const c = _layout.centers[bucketFor(n)];
            if (!c) continue;
            n.vx = (n.vx || 0) + (c.x - (n.x || 0)) * k;
            n.vy = (n.vy || 0) + (c.y - (n.y || 0)) * k;
            // leave z free; gentle pull toward z=0
            n.vz = (n.vz || 0) + (0 - (n.z || 0)) * (k * 0.4);
        }
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

    function buildBoxLayer(container, data, g) {
        const layer = document.createElement('div');
        layer.className = 'kgBoxLayer';
        container.appendChild(layer);
        data.nodes.forEach(function (node) {
            const el = buildBox(node, container, g);
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
        positionBoxes(g);
    }

    function buildBox(node, container, g) {
        const el = document.createElement('div');
        const missing = node.exists === false;
        el.className = 'kgBox' + (node.isCenter ? ' kgBoxCenter' : '') + (missing ? ' kgBoxMissing' : '');
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
        toggle.addEventListener('click', function (e) {
            e.stopPropagation();
            if (toggle.disabled) return;
            const open = el.classList.toggle('kgBoxOpen');
            toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
            measureBox(node);
            positionBoxes(g);
            // The box changed size: let the collision force make room for it.
            try { g.d3ReheatSimulation(); } catch (err) { /* noop */ }
        });
        el.querySelectorAll('.kgFileIcon, .kgBoxName').forEach(function (target) {
            target.addEventListener('click', function (e) {
                e.stopPropagation();
                handleNodeClick(node);
            });
        });
        el.querySelector('.kgBoxHead').addEventListener('mousedown', function (e) {
            if (e.button !== 0 || e.target.closest('.kgBoxToggle, .kgFileIcon, .kgBoxName')) return;
            startBoxDrag(e, node, container, g);
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
     * When the simulation stops the forces may still leave boxes overlapping (the cluster pull
     * wins over the collision): move the boxes themselves until none overlaps. A pinned box
     * (dragged by hand) stays where it is and the other one moves.
     */
    function settleBoxes() {
        if (!_data) return;
        const nodes = _data.nodes.filter(function (n) { return n._boxW && isFinite(n.x) && isFinite(n.y); });
        const pad = 16;
        for (let iter = 0; iter < 80; iter++) {
            let moved = false;
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

    // ---- 3D label sprite ----------------------------------------------------
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

    function buildLabelSprite3D(node) {
        if (typeof THREE === 'undefined') return null;
        const rawLabel = node.label || node.id || '';
        const text = node.isExternal ? '🌐 ' + rawLabel : rawLabel;
        const isCenter = !!node.isCenter;
        const fontSize = isCenter ? 44 : 30;
        const font = (isCenter ? 'bold ' : '600 ') + fontSize + 'px -apple-system, "Segoe UI", Inter, system-ui, sans-serif';
        const padX = 18, padY = 10;
        const m = document.createElement('canvas').getContext('2d');
        m.font = font;
        const textW = Math.ceil(m.measureText(text).width);
        const ratio = (window.devicePixelRatio || 1);
        const canvas = document.createElement('canvas');
        canvas.width = (textW + padX * 2) * ratio;
        canvas.height = (fontSize + padY * 2) * ratio;
        const ctx = canvas.getContext('2d');
        ctx.scale(ratio, ratio);
        ctx.font = font;
        ctx.textBaseline = 'middle';
        const w = textW + padX * 2;
        const h = fontSize + padY * 2;
        const r = h / 2;
        // Dark-aware pill
        const pillBg     = isCenter ? CENTER_COLOR : (_isDark ? 'rgba(15,23,42,0.92)' : '#ffffff');
        const pillStroke = isCenter ? CENTER_RING  : (_isDark ? 'rgba(255,255,255,0.22)' : 'rgba(15,23,42,0.18)');
        const textColor  = isCenter ? '#451a03'    : (_isDark ? '#e6edf3' : '#0f172a');
        ctx.fillStyle = pillBg;
        ctx.strokeStyle = pillStroke;
        ctx.lineWidth = isCenter ? 3 : 1.5;
        drawRoundedRect(ctx, 0, 0, w, h, r);
        ctx.fill();
        ctx.stroke();
        ctx.fillStyle = textColor;
        ctx.fillText(text, padX, h / 2 + 1);
        const tex = new THREE.CanvasTexture(canvas);
        tex.needsUpdate = true;
        tex.minFilter = THREE.LinearFilter;
        const mat = new THREE.SpriteMaterial({ map: tex, transparent: true, depthWrite: false });
        const sprite = new THREE.Sprite(mat);
        const baseScale = isCenter ? 0.45 : 0.36;
        sprite.scale.set(w * baseScale / 6, h * baseScale / 6, 1);
        sprite.position.y = isCenter ? 16 : 11;
        return sprite;
    }

    // ---- 3D builder ---------------------------------------------------------
    function build3D(container, data) {
        if (typeof ForceGraph3D !== 'function') {
            console.error('[KG] ForceGraph3D not loaded');
            return null;
        }
        const rect = container.getBoundingClientRect();
        const w = Math.max(rect.width, 400);
        const h = Math.max(rect.height, 400);
        const g = ForceGraph3D()(container)
            .width(w).height(h)
            .backgroundColor(_isDark ? '#0b1220' : '#f8fafc')
            .showNavInfo(false)
            .nodeLabel(buildNodeTooltip)
            .nodeColor(nodeColor)
            .nodeVal(function (n) { return n.isCenter ? 14 : 3 + Math.min(10, (n.inDegree || 0) + (n.outDegree || 0)); })
            .nodeOpacity(1)
            .nodeResolution(24)
            .nodeThreeObjectExtend(true)
            .nodeThreeObject(buildLabelSprite3D)
            .linkLabel(buildLinkTooltip)
            .linkColor(linkColor)
            .linkOpacity(0.55)
            .linkWidth(1.1)
            .linkCurvature(0.08)
            .linkDirectionalArrowLength(4)
            .linkDirectionalArrowRelPos(1)
            .linkDirectionalArrowColor(linkColor)
            .linkDirectionalParticles(2)
            .linkDirectionalParticleSpeed(0.006)
            .linkDirectionalParticleWidth(2)
            .linkDirectionalParticleColor(linkColor)
            .onNodeClick(handleNodeClick)
            .onNodeHover(function (node) { container.style.cursor = node ? 'pointer' : null; })
            .graphData(data);
        try {
            if (g.d3Force) {
                const charge = g.d3Force('charge'); if (charge) charge.strength(-160);
                const linkF  = g.d3Force('link');   if (linkF)  linkF.distance(70);
                g.d3Force('cluster', clusterForce3D);
            }
        } catch (e) { /* noop */ }
        setTimeout(function () { try { g.zoomToFit(500, 60); } catch (e) {} }, 700);
        return g;
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
            .onNodeHover(function (node) { container.style.cursor = node ? 'pointer' : null; })
            .cooldownTicks(120)
            .onRenderFramePre(function (ctx, globalScale) {
                drawClusterHalos2D(ctx, globalScale);
            })
            .onRenderFramePost(function () {
                if (useBoxes) positionBoxes(g);
            })
            .graphData(data);

        try {
            if (g.d3Force) {
                const charge = g.d3Force('charge'); if (charge) charge.strength(-280);
                const linkF  = g.d3Force('link');   if (linkF)  linkF.distance(useBoxes ? 170 : 80);
                g.d3Force('cluster', clusterForce2D);
                if (useBoxes) g.d3Force('boxCollide', boxCollideForce2D);
            }
        } catch (e) { /* noop */ }

        // Auto-fit on first stabilization. With boxes only the first time: opening or dragging
        // a box reheats the simulation, and refitting then would move the view under the mouse.
        let fitted = false;
        g.onEngineStop(function () {
            if (useBoxes) settleBoxes();
            if (useBoxes && fitted) return;
            fitted = true;
            try { g.zoomToFit(400, useBoxes ? 90 : 50); } catch (e) {}
        });
        if (useBoxes) buildBoxLayer(container, data, g);
        return g;
    }

    // ---- Cluster halos (2D, drawn under nodes) -------------------------------
    function drawClusterHalos2D(ctx, globalScale) {
        if (!_data || !_layout) return;
        const buckets = _layout.buckets;
        for (let i = 0; i < buckets.length; i++) {
            const b = buckets[i];
            let cx = 0, cy = 0, count = 0;
            const members = [];
            for (let j = 0; j < _data.nodes.length; j++) {
                const n = _data.nodes[j];
                if (n.isCenter) continue;
                if (bucketFor(n) !== b) continue;
                if (!isFinite(n.x) || !isFinite(n.y)) continue;   // not placed yet; typeof let NaN through
                cx += n.x; cy += n.y; count++;
                members.push(n);
            }
            if (!count) continue;
            cx /= count; cy /= count;
            let maxR = 0;
            for (let j = 0; j < members.length; j++) {
                const n = members[j];
                const dx = n.x - cx, dy = n.y - cy;
                const extent = n._boxW ? Math.hypot(n._boxW / 2, n._boxH / 2) : nodeRadius(n);
                const d = Math.sqrt(dx * dx + dy * dy) + extent + 16;
                if (d > maxR) maxR = d;
            }
            if (maxR < 36) maxR = 36;

            const color = PALETTE[hashStr(b) % PALETTE.length];
            const fillA   = _isDark ? 0.16 : 0.13;
            const strokeA = _isDark ? 0.38 : 0.30;

            ctx.save();
            ctx.fillStyle = hexToRgba(color, fillA);
            ctx.strokeStyle = hexToRgba(color, strokeA);
            ctx.lineWidth = 1.4 / globalScale;
            ctx.setLineDash([6 / globalScale, 5 / globalScale]);
            ctx.beginPath();
            ctx.arc(cx, cy, maxR, 0, Math.PI * 2);
            ctx.fill();
            ctx.stroke();
            ctx.setLineDash([]);

            // Folder label centered on top of the cluster
            const label = friendlyBucketLabel(b);
            if (label) {
                const fs = 13 / globalScale;
                ctx.font = '700 ' + fs + 'px -apple-system, "Segoe UI", Inter, system-ui, sans-serif';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'bottom';
                ctx.fillStyle = hexToRgba(color, _isDark ? 0.92 : 0.78);
                ctx.fillText(label, cx, cy - maxR - 4 / globalScale);
            }
            ctx.restore();
        }
    }

    function buildForMode(container, data, mode) {
        return mode === '3d' ? build3D(container, data) : build2D(container, data);
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
                '<div class="kgViewTabs" role="tablist" aria-label="Render mode" style="margin-left:8px">' +
                    '<button type="button" class="kgViewTab" data-mode="2d" role="tab">2D</button>' +
                    '<button type="button" class="kgViewTab" data-mode="3d" role="tab">3D</button>' +
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
                '<div class="kgHint" data-mode-hint="2d">drag to pan • scroll to zoom • click a node to open</div>' +
            '</div>' +
            '<div class="kgLoading"><div class="kgSpinner"></div><div>Building graph…</div></div>';
        document.body.appendChild(o);

        o.addEventListener('click', function (e) {
            const btn = e.target.closest('button');
            if (!btn) return;
            const act = btn.getAttribute('data-act');
            const mode = btn.getAttribute('data-mode');
            const source = btn.getAttribute('data-source');
            if (act === 'close') closeOverlay();
            else if (act === 'refresh') refresh();
            else if (act === 'sanity') toggleSanityPanel();
            else if (act === 'sanity-close') hideSanityPanel();
            else if (act === 'focus-clear') clearFocus();
            else if (mode) setMode(mode);
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
            const m = t.getAttribute('data-mode');
            const s = t.getAttribute('data-source');
            let active = false;
            if (m && m === _mode) active = true;
            else if (s && s === _source) active = true;
            t.classList.toggle('kgViewTabActive', active);
        });
        const hint = _overlay.querySelector('.kgHint');
        if (hint) {
            const sourceLbl = _source === 'concepts' ? 'concepts' : 'files';
            hint.textContent = _mode === '3d'
                ? 'drag to orbit • scroll to zoom • click a node — viewing ' + sourceLbl
                : (_source === 'files'
                    ? 'drag the background to pan • scroll to zoom • ▸ TL;DR • click a name to open • drag a box to move it'
                    : 'drag to pan • scroll to zoom • click a node — viewing ' + sourceLbl);
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
            if (_mode === '2d' && _graph.centerAt) {
                if (node.x != null && node.y != null) _graph.centerAt(node.x, node.y, 800);
                if (_graph.zoom) _graph.zoom(3, 800);
            } else if (_mode === '3d' && _graph.cameraPosition) {
                const distance = 200;
                const distRatio = 1 + distance / Math.hypot(node.x || 1, node.y || 1, node.z || 1);
                _graph.cameraPosition({ x: (node.x || 0) * distRatio, y: (node.y || 0) * distRatio, z: (node.z || 0) * distRatio }, node, 1000);
            }
        } catch (e) { /* noop */ }
    }

    function setMode(mode) {
        if (mode !== '2d' && mode !== '3d') return;
        if (mode === _mode && _graph) return;
        _mode = mode;
        setActiveTab();
        if (!_data || !_data.nodes || _data.nodes.length === 0) return;
        renderActive();
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
        // Wait two animation frames so the container has its final layout
        // (the overlay just entered the DOM and is still transitioning).
        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                _graph = buildForMode(body, _data, _mode);
                // Belt-and-suspenders: re-measure once the canvas is settled
                // and re-fit, in case the initial container rect was stale.
                setTimeout(function () {
                    try {
                        const r = body.getBoundingClientRect();
                        if (_graph && r.width > 0 && r.height > 0) _graph.width(r.width).height(r.height);
                        if (_graph && _graph.zoomToFit) _graph.zoomToFit(400, 60);
                    } catch (e) { /* noop */ }
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
        _mode = '2d'; // always start with 2D for the big-picture view
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
        setMode: setMode
    };
})();
