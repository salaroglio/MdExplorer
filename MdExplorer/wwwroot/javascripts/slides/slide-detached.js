/**
 * MdExplorer - a slide deck in the detached window ("open the document in a separate window")
 * ===========================================================================================
 * The detached window has no native frame (frame:false): a document draws its own app-bar
 * (mde-exec-blocks.js), a deck did not, and was left with no way to move, minimize, maximize or
 * close its window. This is the same bar for a deck: a drag region with the file's name, and
 * Refresh / Minimize / Maximize / Close, which ask Electron's main process through
 * preload-detached.js (window.mdeDetachedWindow). In a plain browser tab only Refresh and
 * window.close() apply.
 *
 * The bar takes its strip at the top and the deck the rest: reveal.js lays the slides out again
 * in what is left, so nothing of a slide ends up under the bar.
 *
 * Only with ?detached=true, never in print.
 */
(function () {
    'use strict';

    var BAR_ID = 'mde-detached-appbar';
    var BAR_HEIGHT = 38;

    function query(name) {
        try { return new URLSearchParams(window.location.search).get(name); } catch (e) { return null; }
    }

    if (query('detached') !== 'true') return;
    if (/print-pdf|view=print/.test(window.location.search)) return;

    function button(glyph, tip, colors, onClick, danger) {
        var b = document.createElement('button');
        b.type = 'button';
        b.title = tip;
        b.innerHTML = glyph;
        b.style.cssText = [
            '-webkit-app-region:no-drag', 'border:none', 'background:transparent',
            'color:' + colors.fg, 'cursor:pointer', 'width:34px', 'height:30px',
            'border-radius:4px', 'font-size:16px', 'line-height:1',
            'display:flex', 'align-items:center', 'justify-content:center'
        ].join(';');
        b.addEventListener('mouseenter', function () {
            b.style.background = danger ? '#e81123' : colors.hover;
            if (danger) b.style.color = '#ffffff';
        });
        b.addEventListener('mouseleave', function () { b.style.background = 'transparent'; b.style.color = colors.fg; });
        b.addEventListener('click', onClick);
        return b;
    }

    function inject() {
        if (!document.body || document.getElementById(BAR_ID)) return;

        var isDark = (query('theme') || '').toLowerCase().indexOf('dark') !== -1;
        var colors = isDark
            ? { bg: '#1e1e1e', fg: '#e0e0e0', border: '#333333', hover: '#333333' }
            : { bg: '#f3f3f3', fg: '#333333', border: '#dddddd', hover: '#e0e0e0' };

        var bar = document.createElement('div');
        bar.id = BAR_ID;
        bar.style.cssText = [
            'position:fixed', 'top:0', 'left:0', 'right:0', 'height:' + BAR_HEIGHT + 'px',
            'display:flex', 'align-items:center', 'gap:2px', 'padding:0 6px',
            'box-sizing:border-box', 'background:' + colors.bg, 'color:' + colors.fg,
            'border-bottom:1px solid ' + colors.border,
            'font-family:system-ui,Segoe UI,sans-serif', 'font-size:13px',
            'z-index:2147483647', 'user-select:none', '-webkit-app-region:drag'
        ].join(';');

        var title = document.createElement('div');
        title.style.cssText = 'flex:1;padding-left:6px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;opacity:0.8;';
        try { title.textContent = decodeURIComponent(window.location.pathname.split('/').pop() || 'MdExplorer'); }
        catch (e) { title.textContent = 'MdExplorer'; }
        bar.appendChild(title);

        var win = window.mdeDetachedWindow;
        bar.appendChild(button('&#x21bb;', 'Refresh', colors, function () { window.location.reload(); }, false));
        if (win) {
            bar.appendChild(button('&#x2500;', 'Minimize', colors, function () { win.minimize(); }, false));
            var maximize = button('&#x2610;', 'Maximize', colors, function () { win.maximizeToggle(); }, false);
            bar.appendChild(maximize);
            win.onMaximizedChange(function (isMaximized) {
                maximize.innerHTML = isMaximized ? '&#x2750;' : '&#x2610;';
                maximize.title = isMaximized ? 'Restore' : 'Maximize';
            });
        }
        bar.appendChild(button('&#x2715;', 'Close', colors, function () {
            if (win) { win.close(); } else { window.close(); }
        }, true));
        document.body.appendChild(bar);

        // The deck below the bar. reveal.js measures its own element: laid out again, the slides
        // (and their backgrounds, which live inside it) fit what is left of the window.
        var deck = document.querySelector('.reveal');
        if (deck) {
            deck.style.position = 'absolute';
            deck.style.top = BAR_HEIGHT + 'px';
            deck.style.left = '0';
            deck.style.width = '100%';
            deck.style.height = 'calc(100% - ' + BAR_HEIGHT + 'px)';
            if (window.Reveal && Reveal.isReady && Reveal.isReady()) Reveal.layout();
            else if (window.Reveal && Reveal.on) Reveal.on('ready', function () { Reveal.layout(); });
        }
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', inject);
    else inject();
})();
