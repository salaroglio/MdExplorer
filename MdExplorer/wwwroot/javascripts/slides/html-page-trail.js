/**
 * MdExplorer - Breadcrumb on an HTML page opened from a slide deck
 * =================================================================
 * The server adds this script to an HTML page of the project when MdExplorer's view shows it
 * (HtmlPageTrail.cs). The deck that was left (slide-navigation.js) wrote the way that led here in
 * sessionStorage, shared by the deck, Angular and this page: the decks up to it, each one back to
 * the very slide it was left from. A page opened some other way (the file tree) finds no way for
 * itself and shows nothing.
 *
 * In a static export (data-mde-export-root on <html>, a zip opened without MdExplorer) the export adds it
 * to its copy of the page: the way is the same, with the zip's paths, and going back follows the file.
 */
(function () {
    'use strict';

    var KEY = 'mde.slideHtmlJump';
    var PAGE_PREFIX = /^\/api\/mdexplorer\//i;
    var exportRoot = document.documentElement.getAttribute('data-mde-export-root');
    var inExport = exportRoot !== null;
    if (!inExport && (window.parent === window || !PAGE_PREFIX.test(window.location.pathname))) return;

    var here = inExport
        ? document.documentElement.getAttribute('data-mde-export-path')
        : decodeURIComponent(window.location.pathname.replace(PAGE_PREFIX, ''));
    var jump;
    try { jump = JSON.parse(window.sessionStorage.getItem(KEY)); } catch (e) { jump = null; }
    if (!jump || !jump.to || jump.to.toLowerCase() !== here.toLowerCase() || !jump.trail || !jump.trail.length) return;

    function open(step) {
        if (inExport) {
            window.location.href = new URL(exportRoot || './', window.location.href).href +
                step.path.split('/').map(encodeURIComponent).join('/') +
                (step.pages ? '?pages=' + encodeURIComponent(step.pages) : '') + (step.hash || '');
            return;
        }
        window.parent.postMessage({
            type: 'md-navigate',
            relativePath: step.path,
            name: step.path.split('/').pop(),
            slideHash: step.hash || undefined,
            pages: step.pages || undefined
        }, '*');
    }

    function show() {
        var bar = document.createElement('nav');
        bar.className = 'mde-slide-trail';
        jump.trail.forEach(function (step) {
            var back = document.createElement('a');
            back.href = '#';
            var sameAsDeck = !step.slide || step.slide.toLowerCase() === (step.deck || '').toLowerCase();
            back.textContent = sameAsDeck ? step.deck : step.deck + ': ' + step.slide;
            back.title = step.deck + (step.slide ? ' — ' + step.slide : '');
            back.addEventListener('click', function (event) {
                event.preventDefault();
                open(step);
            });
            bar.appendChild(back);
            var separator = document.createElement('span');
            separator.className = 'mde-slide-trail-separator';
            separator.textContent = '›';
            bar.appendChild(separator);
        });
        var current = document.createElement('span');
        current.className = 'mde-slide-trail-current';
        current.textContent = document.title || here.split('/').pop();
        bar.appendChild(current);
        document.body.appendChild(bar);
    }

    if (document.body) show();
    else document.addEventListener('DOMContentLoaded', show);
})();
