/**
 * MdExplorer - The pages a link asks for, in a static export
 * ==========================================================
 * [Costi](vendite.md?pages=2,6-9) opens that deck showing only those pages. Inside MdExplorer the
 * service leaves the others out (SlidePages.cs); in a static export (a zip, opened without
 * MdExplorer) nothing reads the query, so this page does it, before reveal.js starts: the pages
 * not asked for are taken out of the deck, and reveal.js never knows them.
 *
 * Same rules as SlidePages.cs: a page is one horizontal slide (a vertical stack counts as one),
 * numbered from 1; written N, N-M or N- (to the end), separated by commas; shown in the deck's
 * order. A page the deck does not have is an error shown on the page, never an empty deck.
 *
 * Loaded only by an exported deck (SlideDeckRenderer, StaticExport), right before Reveal.initialize.
 * Sprint: docs-internal/Sprints/2026-09-30-Slide-Export-HTML.md
 */
(function () {
    'use strict';

    var text = new URLSearchParams(window.location.search).get('pages');
    if (text == null || text.trim() === '') return;

    var container = document.querySelector('.reveal .slides');
    var pages = Array.prototype.filter.call(container.children, function (node) {
        return node.tagName === 'SECTION';
    });

    function number(item) {
        if (!/^\d+$/.test(item) || parseInt(item, 10) < 1) {
            throw new Error('The pages "' + text + '" are not valid: "' + item + '" is not a page number. ' +
                'Pages are numbered from 1; write them as 2,6-9 (from 2 and from 6 to 9) or 2- (from 2 to the end).');
        }
        return parseInt(item, 10);
    }

    function parse() {
        return text.split(',').map(function (raw) {
            var item = raw.trim();
            var dash = item.indexOf('-');
            if (dash < 0) {
                var page = number(item);
                return { from: page, to: page };
            }
            var from = number(item.substring(0, dash).trim());
            var toText = item.substring(dash + 1).trim();
            var to = toText.length === 0 ? null : number(toText);
            if (to !== null && to < from) {
                throw new Error('The pages "' + text + '" are not valid: "' + item + '" goes backwards. ' +
                    'Write them as 2,6-9 (from 2 and from 6 to 9) or 2- (from 2 to the end).');
            }
            return { from: from, to: to };
        });
    }

    function check(ranges) {
        var count = pages.length;
        ranges.forEach(function (range) {
            var last = range.to === null ? range.from : range.to;
            if (range.from > count || last > count) {
                throw new Error('The deck has ' + count + ' page' + (count === 1 ? '' : 's') + ', and the link asks for page ' +
                    (range.from > count ? range.from : last) + '. Change the pages of the link.');
            }
        });
    }

    function includes(ranges, page) {
        return ranges.some(function (range) {
            return page >= range.from && (range.to === null || page <= range.to);
        });
    }

    try {
        var ranges = parse();
        check(ranges);
        pages.forEach(function (section, index) {
            if (!includes(ranges, index + 1)) section.remove();
        });
    } catch (error) {
        // What to change, on the page itself: as the service's error page does.
        console.error('[slide-export-pages]', error.message);
        container.innerHTML = '';
        var section = document.createElement('section');
        var heading = document.createElement('h3');
        heading.textContent = document.title;
        var message = document.createElement('p');
        message.textContent = error.message;
        section.appendChild(heading);
        section.appendChild(message);
        container.appendChild(section);
    }
})();
