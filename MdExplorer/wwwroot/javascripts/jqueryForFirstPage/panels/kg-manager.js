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
        const meta = isExt
            ? (node.cluster ? escapeHtml(node.cluster) : 'external')
            : (node.mdContext ? escapeHtml(node.mdContext) : 'root');
        const icon = isExt ? '🌐 ' : (node.isCenter ? '◈ ' : '📄 ');
        const tldrHtml = renderTldrHtml(node.tldr);
        const tldrBlock = tldrHtml
            ? '<div class="kgTipTldr">' + tldrHtml + '</div>'
            : (isExt ? '' : '<div class="kgTipNote">No TLDR; available</div>');
        return '<div class="kgTooltip">' +
            '<div class="kgTipHead">' +
                '<span class="kgTipIcon">' + icon + '</span>' +
                '<span class="kgTipLabel">' + label + '</span>' +
            '</div>' +
            '<div class="kgTipMeta">' + meta + '</div>' +
            tldrBlock +
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

    function navigateToNode(node) {
        if (!node || node.isCenter) return;

        // External link: open in default browser / new window
        if (node.isExternal && node.externalUrl) {
            try {
                const w = window.open(node.externalUrl, '_blank', 'noopener,noreferrer');
                if (!w) {
                    // popup blocked: fallback to anchor click
                    const a = document.createElement('a');
                    a.href = node.externalUrl;
                    a.target = '_blank';
                    a.rel = 'noopener noreferrer';
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                }
            } catch (e) {
                console.error('[KG] failed to open external URL', e);
            }
            return;
        }

        let rel = node.relativePath || '';
        if (!rel) return;
        if (isExternalUrl(rel) || isExternalUrl(node.fullPath)) {
            console.warn('[KG] node points to an external URL but is not flagged as external:', rel);
            return;
        }
        if (/^[a-zA-Z]:[\\/]/.test(rel) || rel.startsWith('/') || rel.startsWith('\\')) {
            console.warn('[KG] refusing to navigate to non-relative path:', rel);
            return;
        }
        rel = rel.replace(/^\.\/+/, '').replace(/\\/g, '/');
        const conn = $('#MdBody').attr('connectionid');
        let url = '/api/mdexplorer/' + rel;
        if (conn) url += '?connectionid=' + encodeURIComponent(conn);
        closeOverlay();
        window.location.href = url;
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
            .onNodeClick(navigateToNode)
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

        const g = ForceGraph()(container)
            .width(w).height(h)
            .backgroundColor('rgba(0,0,0,0)') // transparent over our CSS gradient
            .nodeRelSize(6)
            .nodeLabel(buildNodeTooltip)
            .nodeColor(nodeColor)
            .nodeVal(function (n) { return n.isCenter ? 14 : 3 + Math.min(10, (n.inDegree || 0) + (n.outDegree || 0)); })
            .nodeCanvasObjectMode(function () { return 'replace'; })
            .nodeCanvasObject(function (node, ctx, globalScale) {
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
                const r = nodeRadius(node);
                ctx.beginPath();
                ctx.arc(node.x, node.y, r + 2, 0, 2 * Math.PI);
                ctx.fillStyle = color;
                ctx.fill();
            })
            .linkColor(linkColor)
            .linkWidth(function (l) { return 1.4; })
            .linkCurvature(0.08)
            .linkDirectionalArrowLength(5)
            .linkDirectionalArrowRelPos(0.92)
            .linkDirectionalArrowColor(linkColor)
            .linkDirectionalParticles(2)
            .linkDirectionalParticleSpeed(0.006)
            .linkDirectionalParticleWidth(2)
            .linkDirectionalParticleColor(linkColor)
            .onNodeClick(navigateToNode)
            .onNodeHover(function (node) { container.style.cursor = node ? 'pointer' : null; })
            .cooldownTicks(120)
            .onRenderFramePre(function (ctx, globalScale) {
                drawClusterHalos2D(ctx, globalScale);
            })
            .graphData(data);

        try {
            if (g.d3Force) {
                const charge = g.d3Force('charge'); if (charge) charge.strength(-280);
                const linkF  = g.d3Force('link');   if (linkF)  linkF.distance(80);
                g.d3Force('cluster', clusterForce2D);
            }
        } catch (e) { /* noop */ }

        // Auto-fit on first stabilization
        g.onEngineStop(function () {
            try { g.zoomToFit(400, 50); } catch (e) {}
        });
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
                if (typeof n.x !== 'number' || typeof n.y !== 'number') continue;
                cx += n.x; cy += n.y; count++;
                members.push(n);
            }
            if (!count) continue;
            cx /= count; cy /= count;
            let maxR = 0;
            for (let j = 0; j < members.length; j++) {
                const n = members[j];
                const dx = n.x - cx, dy = n.y - cy;
                const d = Math.sqrt(dx * dx + dy * dy) + nodeRadius(n) + 16;
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
                : 'drag to pan • scroll to zoom • click a node — viewing ' + sourceLbl;
        }
        const nsPicker = _overlay.querySelector('.kgNsPicker');
        if (nsPicker) nsPicker.style.display = _source === 'concepts' ? '' : 'none';
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
                    isCenter: false,
                    isExternal: false,
                    inDegree: 0,
                    outDegree: 0,
                    tldr: ''
                };
            }),
            links: (raw.links || []).map(function (l) {
                return {
                    source: l.source,
                    target: l.target,
                    linkType: (l.type || 'link').toLowerCase(),
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
            _data = data;
            _layout = computeLayout(data);
            showLoading(false);
            if (!data.nodes.length) {
                const body = _overlay && _overlay.querySelector('.kgBody');
                if (body) body.innerHTML = '<div class="kgEmpty"><div class="kgEmptyIcon">🕸️</div><div>No concepts in Neo4j yet — sync from Project Settings → Knowledge Graph.</div></div>';
                return;
            }
            renderActive();
        }).fail(function (xhr) {
            console.error('[KG] concept fetch failed', xhr && xhr.status, xhr && xhr.statusText);
            const errBody = xhr && xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error : ('HTTP ' + (xhr ? xhr.status : '?'));
            showError('Failed to load concept graph: ' + errBody);
        });
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
        _source = 'files';
    }

    function refresh() {
        if (!_overlay) return;
        const body = _overlay.querySelector('.kgBody');
        if (body) body.innerHTML = '';
        _graph = null;
        _data = null;
        _layout = null;
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
