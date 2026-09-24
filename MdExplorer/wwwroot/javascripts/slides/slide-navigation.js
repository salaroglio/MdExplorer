/**
 * MdExplorer - Moving between slide decks
 * ========================================
 * A deck can link to other decks (a master deck with a link per section). Two things here:
 *
 * 1. A link to another markdown file, inside MdExplorer's view, goes through Angular the way a
 *    document's links do (postMessage 'md-navigate'): the file is loaded once. Followed by the
 *    iframe itself it was loaded twice — once by the link, once more by Angular, told by the
 *    server (measured, sprint F0). Outside MdExplorer (detached window, browser) a link stays a link.
 *
 * 2. A breadcrumb in the top left corner: the decks that led here, each one back to the very
 *    slide it was left from. The way is kept in sessionStorage, which the decks and Angular share
 *    in a window. The deck being left writes where the jump starts (deck, slide, #/h/v); the deck
 *    arriving reads it. A deck already on the way shortens it; the same deck shown again (a save,
 *    a theme change reload it) keeps it; a deck opened some other way (the file tree, the arrows)
 *    starts a new one.
 *
 * Sprint: docs-internal/Sprints/2026-09-24-Slide-Breadcrumb-Tra-Presentazioni.md
 */
(function () {
    'use strict';

    var TRAIL = 'mde.slideTrail';
    var JUMP = 'mde.slideJump';
    var SHOWN = 'mde.slideShown';
    var PAGE_PREFIX = /^\/api\/mdexplorer\//i;
    var inMdExplorer = window.parent !== window;

    function read(key) {
        try { return JSON.parse(window.sessionStorage.getItem(key)); } catch (e) { return null; }
    }

    function write(key, value) {
        try {
            if (value == null) window.sessionStorage.removeItem(key);
            else window.sessionStorage.setItem(key, JSON.stringify(value));
        } catch (e) { /* no storage: no breadcrumb, links still work */ }
    }

    /** The project-relative path of the markdown file a page URL shows, or null if it is not one. */
    function markdownPathOf(url) {
        if (url.origin !== window.location.origin || !PAGE_PREFIX.test(url.pathname)) return null;
        var path = decodeURIComponent(url.pathname.replace(PAGE_PREFIX, ''));
        return /\.md$/i.test(path) ? path : null;
    }

    var here = markdownPathOf(new URL(window.location.href));

    function position() {
        var i = Reveal.getIndices();
        return '#/' + i.h + (i.v ? '/' + i.v : '');
    }

    function slideTitle(slide) {
        var heading = slide && slide.querySelector('h1, h2, h3');
        return heading ? heading.textContent.trim() : '';
    }

    /** Opens a deck, on a slide when a position is given, the way this page was opened. */
    function open(path, hash) {
        if (inMdExplorer) {
            window.parent.postMessage({
                type: 'md-navigate',
                relativePath: path,
                name: path.split('/').pop(),
                slideHash: hash || undefined
            }, '*');
        } else {
            var connectionId = document.body.getAttribute('ConnectionId') || '';
            window.location.href = '/api/mdexplorer/' + path.split('/').map(encodeURIComponent).join('/') +
                '?connectionId=' + encodeURIComponent(connectionId) + (hash || '');
        }
    }

    function followLinks() {
        document.addEventListener('click', function (event) {
            var link = event.target.closest && event.target.closest('a[href]');
            if (!link || event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey) return;
            var url = new URL(link.href, window.location.href);
            var target = markdownPathOf(url);
            // Another slide of this deck (#/3) is reveal.js's business.
            if (!target || target === here) return;

            write(JUMP, {
                to: target,
                from: { path: here, deck: document.title, slide: slideTitle(Reveal.getCurrentSlide()), hash: position() }
            });
            if (inMdExplorer) {
                event.preventDefault();
                // [Costi](vendite.md#/3) opens that deck on that slide.
                open(target, /^#\/\d+(\/\d+)?$/.test(url.hash) ? url.hash : null);
            }
        }, true);
    }

    /** The way that led to this deck, updated for this arrival. */
    function trailForThisDeck() {
        var trail = read(TRAIL) || [];
        var jump = read(JUMP);
        var shownBefore = read(SHOWN);
        write(JUMP, null);
        write(SHOWN, here);
        if (!jump && shownBefore === here) return trail;
        var onTheWay = trail.map(function (step) { return step.path; }).indexOf(here);
        if (onTheWay >= 0) return trail.slice(0, onTheWay);
        if (jump && jump.to === here && jump.from && jump.from.path) return trail.concat([jump.from]);
        return [];
    }

    function showBreadcrumb(trail) {
        if (!trail.length) return;
        var bar = document.createElement('nav');
        bar.className = 'mde-slide-trail';
        trail.forEach(function (step) {
            var back = document.createElement('a');
            back.href = '#';
            // The first slide often repeats the deck's title: said once.
            var sameAsDeck = !step.slide || step.slide.toLowerCase() === (step.deck || '').toLowerCase();
            back.textContent = sameAsDeck ? step.deck : step.deck + ': ' + step.slide;
            back.title = step.deck + (step.slide ? ' — ' + step.slide : '');
            back.addEventListener('click', function (event) {
                event.preventDefault();
                event.stopPropagation();
                open(step.path, step.hash);
            });
            bar.appendChild(back);
            var separator = document.createElement('span');
            separator.className = 'mde-slide-trail-separator';
            separator.textContent = '›';
            bar.appendChild(separator);
        });
        var current = document.createElement('span');
        current.className = 'mde-slide-trail-current';
        current.textContent = document.title;
        bar.appendChild(current);
        document.body.appendChild(bar);
    }

    if (!here) return;
    followLinks();
    Reveal.on('ready', function () {
        var trail = trailForThisDeck();
        write(TRAIL, trail);
        showBreadcrumb(trail);
    });
})();
