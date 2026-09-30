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
 *    [Costi](vendite.md?pages=2,6-9) opens that deck showing only those pages: the server leaves
 *    the others out (SlidePages.cs) and reveal.js never knows them. The pages travel with the jump
 *    and with the breadcrumb, so coming back shows the deck as it was left.
 *
 *    A link to an HTML page of the project (.html/.htm) is opened by Angular the same way, so it
 *    shows in MdExplorer's view and enters the navigation history; the page shows the breadcrumb
 *    of the way that led to it (html-page-trail.js, added to the page by the server).
 *
 *    A link to another site (http/https) opens in the system browser, as in a document
 *    (navigation-history.js: POST /api/MdFiles/OpenUrlInBrowser): followed in the iframe, a site
 *    that forbids being framed left a black page in MdExplorer's view.
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
    var HTML_JUMP = 'mde.slideHtmlJump';
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

    /** The project-relative path of an HTML page of the project a link points to, or null. */
    function htmlPagePathOf(url) {
        if (url.origin !== window.location.origin || !PAGE_PREFIX.test(url.pathname)) return null;
        var path = decodeURIComponent(url.pathname.replace(PAGE_PREFIX, ''));
        return /\.html?$/i.test(path) ? path : null;
    }

    var here = markdownPathOf(new URL(window.location.href));
    /** The pages of this deck the link that opened it asked for (2,6-9), or null: the whole deck. */
    var herePages = new URL(window.location.href).searchParams.get('pages');

    function position() {
        var i = Reveal.getIndices();
        return '#/' + i.h + (i.v ? '/' + i.v : '');
    }

    function slideTitle(slide) {
        var heading = slide && slide.querySelector('h1, h2, h3');
        return heading ? heading.textContent.trim() : '';
    }

    /**
     * Opens a deck, on a slide when a position is given, showing the pages a link asked for
     * (?pages=2,6-9) when given, the way this page was opened.
     */
    function open(path, hash, pages) {
        if (inMdExplorer) {
            window.parent.postMessage({
                type: 'md-navigate',
                relativePath: path,
                name: path.split('/').pop(),
                slideHash: hash || undefined,
                pages: pages || undefined
            }, '*');
        } else {
            var connectionId = document.body.getAttribute('ConnectionId') || '';
            window.location.href = '/api/mdexplorer/' + path.split('/').map(encodeURIComponent).join('/') +
                '?connectionId=' + encodeURIComponent(connectionId) + (pages ? '&pages=' + encodeURIComponent(pages) : '') + (hash || '');
        }
    }

    /** A link to another site: the system browser, never MdExplorer's view. */
    function openOutside(href) {
        if (!inMdExplorer) {
            // Detached window (Electron sends it to the system browser) or a browser tab.
            window.open(href, '_blank', 'noopener');
            return;
        }
        fetch('/api/MdFiles/OpenUrlInBrowser', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ url: href, connectionId: document.body.getAttribute('ConnectionId') || '' })
        }).then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
        }).catch(function (error) {
            console.error('[slide-navigation] The link could not be opened in the browser:', href, error);
            showNotice(_toolbarText('nav.linkFailed', { href: href }));
        });
    }

    function showNotice(message) {
        var notice = document.createElement('div');
        notice.className = 'mde-inline-edit-toast';
        notice.textContent = message;
        notice.addEventListener('click', function () { notice.remove(); });
        document.body.appendChild(notice);
        setTimeout(function () { if (notice.isConnected) notice.remove(); }, 8000);
    }

    function followLinks() {
        document.addEventListener('click', function (event) {
            var link = event.target.closest && event.target.closest('a[href]');
            if (!link || event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey) return;
            var url = new URL(link.href, window.location.href);
            if (/^https?:$/.test(url.protocol) && url.origin !== window.location.origin) {
                event.preventDefault();
                openOutside(url.href);
                return;
            }
            // A page of the project (.html): opened by Angular, like a click in the tree, so it
            // shows in MdExplorer's view and enters the history (the arrows bring the deck back on
            // its slide). Followed by the iframe itself it left a black page.
            var page = htmlPagePathOf(url);
            if (page && inMdExplorer) {
                event.preventDefault();
                // The page shows the way that led to it (html-page-trail.js): the decks up to here.
                write(HTML_JUMP, {
                    to: page,
                    trail: (read(TRAIL) || []).concat([{
                        path: here, deck: document.title, slide: slideTitle(Reveal.getCurrentSlide()), hash: position(), pages: herePages
                    }])
                });
                open(page, null);
                return;
            }
            var target = markdownPathOf(url);
            // Another slide of this deck (#/3) is reveal.js's business.
            if (!target || target === here) return;

            write(JUMP, {
                to: target,
                from: { path: here, deck: document.title, slide: slideTitle(Reveal.getCurrentSlide()), hash: position(), pages: herePages }
            });
            if (inMdExplorer) {
                event.preventDefault();
                // [Costi](vendite.md#/3) opens that deck on that slide.
                open(target, /^#\/\d+(\/\d+)?$/.test(url.hash) ? url.hash : null, url.searchParams.get('pages'));
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
        write(HTML_JUMP, null);
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
                open(step.path, step.hash, step.pages);
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
