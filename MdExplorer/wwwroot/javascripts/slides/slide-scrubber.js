/**
 * MdExplorer - Scroll quickly through a deck
 * ===========================================
 * A slider on the deck's bar (slide-toolbar.js): with many slides, the arrows one at a time are slow. Dragging it,
 * the deck goes to that slide while the slider moves, and a small label under the bar says which one: its number
 * and its title (the first heading on it). Next to the slider, «7 / 18», that follows the deck however one moves
 * (arrows, links, the slider).
 *
 * The slider counts the horizontal slides, as reveal.js's own numbers do: a vertical stack is one stop, and the
 * slider goes to its first slide. With fewer than three slides there is nothing to scroll: no slider.
 *
 * The keys go back to the deck when the slider is let go: a focused slider would keep the arrows for itself.
 */
(function () {
    'use strict';

    if (!window.mdeSlideToolbar || typeof window.Reveal === 'undefined') return;
    if (typeof _toolbarText !== 'function') {
        console.error('[slide-scrubber] toolbar-shared.js is not loaded: no texts, no slider.');
        return;
    }

    var MIN_SLIDES = 3;

    function start() {
        var slides = Reveal.getHorizontalSlides();
        if (!slides || slides.length < MIN_SLIDES) return;

        var box = document.createElement('div');
        box.className = 'mde-tb-scrub';
        box.title = _toolbarText('slide.scrub');

        var range = document.createElement('input');
        range.type = 'range';
        range.min = '1';
        range.max = String(slides.length);
        range.step = '1';
        range.setAttribute('aria-label', _toolbarText('slide.scrub'));

        var count = document.createElement('span');
        count.className = 'mde-tb-scrub-count';

        box.appendChild(range);
        box.appendChild(count);
        window.mdeSlideToolbar.addElement(box);

        // Which slide the slider is on, while it is dragged: under the bar, not on it, so the bar does not change width.
        var label = document.createElement('div');
        label.className = 'mde-tb-scrub-label';
        label.hidden = true;
        document.body.appendChild(label);

        function titleOf(index) {
            var slide = Reveal.getHorizontalSlides()[index];
            // A vertical stack: its first slide is the one the slider goes to.
            if (slide && slide.querySelector(':scope > section')) slide = slide.querySelector(':scope > section');
            var heading = slide && slide.querySelector('h1, h2, h3');
            var text = heading ? heading.textContent.replace(/\s+/g, ' ').trim() : '';
            return text || _toolbarText('slide.scrubUntitled');
        }

        function show(position) {
            var total = Reveal.getHorizontalSlides().length;
            count.textContent = position + ' / ' + total;
            range.max = String(total);
            range.value = String(position);
        }

        function current() {
            return (Reveal.getIndices().h || 0) + 1;
        }

        function placeLabel() {
            var rect = box.getBoundingClientRect();
            label.style.left = Math.max(8, Math.min(window.innerWidth - label.offsetWidth - 8, rect.left + rect.width / 2 - label.offsetWidth / 2)) + 'px';
            label.style.top = (rect.bottom + 6) + 'px';
        }

        range.addEventListener('input', function () {
            var position = parseInt(range.value, 10);
            Reveal.slide(position - 1, 0);
            count.textContent = position + ' / ' + range.max;
            label.textContent = position + ' · ' + titleOf(position - 1);
            label.hidden = false;
            placeLabel();
        });

        function letGo() {
            label.hidden = true;
            // The keys go back to the deck.
            range.blur();
        }

        range.addEventListener('change', letGo);
        range.addEventListener('pointerup', letGo);
        range.addEventListener('pointercancel', letGo);
        // Dragging the slider is not dragging the bar, and not a click on the slide behind it.
        range.addEventListener('pointerdown', function (event) { event.stopPropagation(); });

        Reveal.on('slidechanged', function () { show(current()); });
        show(current());
    }

    if (Reveal.isReady && Reveal.isReady()) start();
    else Reveal.on('ready', start);
})();
