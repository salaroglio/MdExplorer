/**
 * MdExplorer - TEMPORARY diagnostics of a slide deck's loading
 * =============================================================
 * Writes to the console, with the time since the page started (seconds), what happens while the deck
 * loads: the page's milestones, reveal.js's, the fonts (and where they come from), the diagrams (size,
 * font, first text) every time something about them changes, the resources that took long or failed,
 * and the long tasks that held the page. To understand a diagram whose boxes appear at once and whose
 * text comes right only 15–20 s later (30/09/2026). Filter the console on "MDE-DIAG".
 *
 * To be removed once the cause is found.
 */
(function () {
    'use strict';

    var T0 = performance.timeOrigin || (Date.now() - performance.now());
    function t() { return ((Date.now() - T0) / 1000).toFixed(3) + 's'; }
    function log() {
        var args = Array.prototype.slice.call(arguments);
        args.unshift('[MDE-DIAG ' + t() + ']');
        console.log.apply(console, args);
    }

    log('script start', 'readyState=' + document.readyState, 'inMdExplorer=' + (window.parent !== window),
        'theme=' + (document.querySelector('link[href*="/reveal/dist/theme/"]') || { getAttribute: function () { return '?'; } }).getAttribute('href'),
        'url=' + location.pathname + location.search.replace(/connectionId=[^&]*/, 'connectionId=…') + location.hash);

    document.addEventListener('DOMContentLoaded', function () { log('DOMContentLoaded'); });
    window.addEventListener('load', function () { log('window load'); });

    // ---- fonts ----
    if (document.fonts) {
        var faces = [];
        try { document.fonts.forEach(function (f) { faces.push(f.family + '/' + f.weight + '/' + f.style + ':' + f.status); }); } catch (e) { /* no forEach */ }
        log('fonts at start: status=' + document.fonts.status, faces.join(' | ') || '(none)');
        document.fonts.ready.then(function () { log('fonts.ready resolved, status=' + document.fonts.status); });
        document.fonts.addEventListener('loadingdone', function (e) { log('fonts loadingdone', (e.fontfaces || []).map(function (f) { return f.family + '/' + f.weight; }).join(' | ')); });
        document.fonts.addEventListener('loadingerror', function (e) { log('fonts loadingERROR', (e.fontfaces || []).map(function (f) { return f.family + '/' + f.weight; }).join(' | ')); });
    }

    // ---- reveal (its events can be listened to only once it is initialized: waited for) ----
    var revealWait = setInterval(function () {
        if (!(window.Reveal && Reveal.isReady && Reveal.isReady())) return;
        clearInterval(revealWait);
        log('Reveal ready', 'scale=' + Reveal.getScale(), 'indices=' + JSON.stringify(Reveal.getIndices()));
        snapshot('ready');
        Reveal.on('resize', function (e) { log('Reveal resize (layout)', 'scale=' + (e && e.scale)); snapshot('resize'); });
        Reveal.on('slidechanged', function () { log('Reveal slidechanged', JSON.stringify(Reveal.getIndices())); snapshot('slidechanged'); });
        if (window.ResizeObserver) {
            document.querySelectorAll('svg[data-diagram-type]').forEach(function (svg, i) {
                new ResizeObserver(function () { snapshot('svg#' + i + ' resized'); }).observe(svg);
            });
        }
    }, 50);

    // ---- the diagrams of the slide on screen: what changes, when ----
    var last = {};
    function describe(svg, i) {
        var rect = svg.getBoundingClientRect();
        var text = svg.querySelector('text');
        var d = {
            type: svg.getAttribute('data-diagram-type'),
            svgW: Math.round(rect.width), svgH: Math.round(rect.height),
            styleW: svg.style.width || '-', styleH: svg.style.height || '-',
            viewBox: svg.getAttribute('viewBox') || '-',
            attrW: svg.getAttribute('width') || '-',
            fontAttr: text ? (text.getAttribute('font-family') || '-') + '/' + (text.getAttribute('font-size') || '-') : '(no text)',
            fontUsed: text ? window.getComputedStyle(text).fontFamily + '/' + window.getComputedStyle(text).fontSize : '-',
            textLen: text ? (text.getAttribute('textLength') || '-') : '-',
            textW: 0, textX: 0
        };
        try { if (text) { var b = text.getBBox(); d.textW = Math.round(b.width * 10) / 10; d.textX = Math.round(text.getBoundingClientRect().left - rect.left); } } catch (e) { d.textW = 'err'; }
        if (document.fonts && text) {
            var fam = text.getAttribute('font-family');
            try { d.fontCheck = fam ? document.fonts.check('12px ' + fam) : '-'; } catch (e) { d.fontCheck = 'err'; }
        }
        return d;
    }

    function snapshot(why) {
        var slide = window.Reveal && Reveal.getCurrentSlide ? Reveal.getCurrentSlide() : document;
        if (!slide) return;
        var svgs = slide.querySelectorAll('svg[data-diagram-type]');
        Array.prototype.forEach.call(svgs, function (svg, i) {
            var d = describe(svg, i);
            var key = JSON.stringify(d);
            if (last[i] === key) return;
            last[i] = key;
            log('diagram#' + i + ' (' + why + ')', key);
        });
    }

    var polls = 0;
    var poll = setInterval(function () {
        polls++;
        snapshot('poll');
        if (polls === 10 || polls === 30 || polls === 60) resources(polls / 2 + 's');
        if (polls >= 80) clearInterval(poll);   // 40 s
    }, 500);

    // ---- resources: what was slow or failed ----
    function resources(when) {
        var slow = [];
        performance.getEntriesByType('resource').forEach(function (r) {
            var took = Math.round(r.duration);
            var failed = r.responseEnd === 0 && r.duration > 0;
            if (took > 700 || failed || /font|katex/i.test(r.name)) {
                slow.push((failed ? 'PENDING/FAILED ' : '') + took + 'ms ' + r.initiatorType + ' ' + r.name.replace(/^https?:\/\/[^/]+/, '').slice(0, 90));
            }
        });
        log('resources at ' + when + ' (slow > 700 ms, pending, fonts):', slow.length ? slow.join(' || ') : 'none');
        var nav = performance.getEntriesByType('navigation')[0];
        if (nav) log('navigation: request→response ' + Math.round(nav.responseEnd - nav.requestStart) + 'ms, DOMContentLoaded ' + Math.round(nav.domContentLoadedEventEnd) + 'ms, load ' + Math.round(nav.loadEventEnd) + 'ms');
    }

    // ---- long tasks: the page held for more than 50 ms at a time ----
    if (window.PerformanceObserver) {
        try {
            var longTotal = 0, longCount = 0;
            new PerformanceObserver(function (list) {
                list.getEntries().forEach(function (e) {
                    longCount++; longTotal += e.duration;
                    if (e.duration > 200) log('long task ' + Math.round(e.duration) + 'ms');
                });
            }).observe({ entryTypes: ['longtask'] });
            setTimeout(function () { log('long tasks in the first 30 s: ' + longCount + ', total ' + Math.round(longTotal) + 'ms'); }, 30000);
        } catch (e) { /* not supported */ }
    }

    window.addEventListener('resize', function () { log('window resize ' + window.innerWidth + 'x' + window.innerHeight); });
})();
