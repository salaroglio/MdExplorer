/**
 * MdExplorer - Dragging the items of a list in a slide
 * =====================================================
 * With "Modifica veloce" on (slide-toolbar.js), a handle (⋮⋮) appears at the left of the list item
 * under the pointer, and the item can also be dragged from its own text (slide-edit-mode.js, through
 * window.mdeListDrag.begin). Dragged up or down, the
 * item goes to another place among its siblings — the items of the same list, with the sub-items
 * it holds — and is dropped where the line shows. The server moves the lines in the .md
 * (POST api/mdfiles/MoveListItem, ListItemReorderer) and writes only if the file, read again, is
 * the same with the items in another order; the view then reloads as after any change of the file.
 *
 * Offered only where the server can do it: an item that carries its file line (data-mde-line-start:
 * not one a command generated), in a list of two or more such items, in MdExplorer's view — not in
 * the speaker view, a detached window or print (as slide-edit.js).
 *
 * The drag is done with the pointer's events, not with HTML5's drag and drop: reveal.js's own
 * mouse and touch handling and the browser's text selection do not enter, and it can be tried with
 * real mouse events. The handle lives on <body>, outside reveal.js's element; while a drag is on,
 * the drop line holds data-mde-holds-keys, so that reveal.js leaves the keys to it (slide-diagrams.js:
 * Esc cancels the drag and does not open the overview).
 *
 * The page's fingerprint (data-mde-source-hash) is NOT updated after a move, on purpose: the lines
 * of the items below have moved with the text, and a page with the new fingerprint and the old
 * lines would move the wrong item. Until the view reloads, a second move gets a 409 and is refused.
 *
 * Sprint: docs-internal/Sprints/2026-09-30-Slide-Drag-Elenchi.md
 */
(function () {
    'use strict';

    var ITEM = 'li[data-mde-line-start]';
    var DRAGGING = 'mde-list-dragging';
    /** How far above or below the list the pointer may go and still mean "here". Farther: cancel. */
    var REACH = 40;

    function inMdExplorerView() {
        if (window.parent === window) return false;
        try {
            return /\/client2\//.test(window.parent.location.pathname);
        } catch (e) {
            return false;
        }
    }

    function isPrint() {
        return /print-pdf|view=print/.test(window.location.search);
    }

    function enabled() {
        return inMdExplorerView() && !isPrint()
            && !!document.body.getAttribute('data-mde-source-hash')
            && !!document.body.getAttribute('DocumentPath')
            && !!document.body.getAttribute('ConnectionId');
    }
    if (!enabled()) return;

    var content = document.querySelector('[data-mde-content]');
    if (!content) return;

    var handle = document.createElement('div');
    handle.className = 'mde-list-handle';
    handle.textContent = '\u22EE\u22EE';
    handle.title = 'Trascina per spostare la voce';
    handle.hidden = true;
    document.body.appendChild(handle);

    var dropLine = document.createElement('div');
    dropLine.className = 'mde-list-drop';
    dropLine.hidden = true;
    document.body.appendChild(dropLine);

    /** The mode of slide-toolbar.js: with it off there is no handle and nothing to drag. */
    function inMode() {
        return document.body.classList.contains('mde-edit-mode');
    }

    var current = null;   // the item the handle is shown for
    var drag = null;      // {item, siblings, from, to}

    // ---- what can be moved ----

    /** The items of the list this item is in that carry a file line: the ones the server can place. */
    function siblingsOf(item) {
        var list = item.parentElement;
        if (!list || !/^(UL|OL)$/.test(list.tagName)) return [];
        return Array.prototype.filter.call(list.children, function (child) {
            return child.matches(ITEM);
        });
    }

    function movable(item) {
        return item && content.contains(item) && !item.closest('aside.notes') && siblingsOf(item).length > 1;
    }

    function busy() {
        return !!(document.activeElement && document.activeElement.isContentEditable)
            || !!document.querySelector('[data-mde-holds-keys]:not(.mde-list-drop)');
    }

    // ---- the handle ----

    function showHandle(item) {
        current = item;
        var rect = item.getBoundingClientRect();
        handle.hidden = false;
        // A fixed size on the page, not scaled with the slide: it sits left of the item's bullet or
        // number, which are drawn outside the item's box (about an em of the item's text).
        var em = parseFloat(window.getComputedStyle(item).fontSize) || 16;
        var marker = em * (item.parentElement.tagName === 'OL' ? 1.6 : 1.15);
        handle.style.left = Math.max(2, rect.left - marker - 28) + 'px';
        handle.style.top = Math.max(2, rect.top + Math.min(rect.height, 40) / 2 - 12) + 'px';
    }

    function hideHandle() {
        cancelHide();
        current = null;
        handle.hidden = true;
    }

    // Going from the item to the handle crosses a few pixels of slide (a list is as wide as its
    // text in reveal.js): the handle waits a moment before it goes away, and stays if it is reached.
    var hideTimer = null;
    var overHandle = false;

    function cancelHide() {
        clearTimeout(hideTimer);
        hideTimer = null;
    }

    function scheduleHide() {
        if (hideTimer) return;
        hideTimer = setTimeout(function () {
            hideTimer = null;
            if (!drag && !overHandle) hideHandle();
        }, 350);
    }

    handle.addEventListener('mouseenter', function () { overHandle = true; cancelHide(); });
    handle.addEventListener('mouseleave', function () { overHandle = false; if (!drag) scheduleHide(); });

    document.addEventListener('mouseover', function (event) {
        if (drag || busy()) return;
        if (!inMode()) {
            if (current) hideHandle();
            return;
        }
        if (handle.contains(event.target)) { cancelHide(); return; }
        var item = event.target.closest && event.target.closest(ITEM);
        if (item && movable(item)) {
            cancelHide();
            if (item !== current) showHandle(item);
        } else if (current) {
            scheduleHide();
        }
    });

    document.documentElement.addEventListener('mouseleave', function () {
        if (!drag) hideHandle();
    });

    // The slide changes or the view scrolls: the handle would float where its item no longer is.
    if (window.Reveal) {
        Reveal.on('slidechanged', hideHandle);
        Reveal.on('overviewshown', hideHandle);
        Reveal.on('fragmentshown', hideHandle);
        Reveal.on('fragmenthidden', hideHandle);
    }
    window.addEventListener('resize', hideHandle);

    // ---- the drag ----

    /** Where the item would land: the number of the other items whose middle is above the pointer. */
    function targetIndex(y) {
        var others = drag.siblings.filter(function (s) { return s !== drag.item; });
        var index = 0;
        others.forEach(function (s) {
            var rect = s.getBoundingClientRect();
            if (rect.top + rect.height / 2 < y) index++;
        });
        return index;
    }

    function withinReach(y) {
        var first = drag.siblings[0].getBoundingClientRect();
        var last = drag.siblings[drag.siblings.length - 1].getBoundingClientRect();
        return y >= first.top - REACH && y <= last.bottom + REACH;
    }

    function showDropLine(index) {
        var others = drag.siblings.filter(function (s) { return s !== drag.item; });
        var before = others[index];
        var ref = (before || others[others.length - 1]).getBoundingClientRect();
        var list = drag.item.parentElement.getBoundingClientRect();
        dropLine.hidden = false;
        dropLine.style.left = Math.max(2, list.left - 6) + 'px';
        dropLine.style.width = Math.max(40, list.width + 12) + 'px';
        dropLine.style.top = ((before ? ref.top : ref.bottom) - 1.5) + 'px';
    }

    function endDrag() {
        if (!drag) return;
        document.removeEventListener('pointermove', onMove, true);
        document.removeEventListener('pointerup', onUp, true);
        document.removeEventListener('pointercancel', endDrag, true);
        drag.item.classList.remove(DRAGGING);
        document.body.classList.remove(DRAGGING);
        dropLine.hidden = true;
        dropLine.removeAttribute('data-mde-holds-keys');
        drag = null;
        hideHandle();
    }

    /**
     * Starts dragging the item, the pointer being already down: from the handle, or from the item's own
     * text (slide-edit-mode.js). Returns false when it cannot be moved.
     */
    function begin(item, event) {
        if (drag || !movable(item)) return false;
        current = item;
        // Cancelling pointerdown also cancels the mousedown that would give this page the keyboard: without
        // this, if the last click was in the app around the slide, Esc would go there and not cancel the drag.
        try { window.focus(); } catch (e) { /* the keys stay where they were */ }
        var siblings = siblingsOf(item);
        drag = { item: item, siblings: siblings, from: siblings.indexOf(item), to: null };
        drag.item.classList.add(DRAGGING);
        document.body.classList.add(DRAGGING);
        // reveal.js leaves the keys alone while this is here (slide-diagrams.js): Esc is ours.
        dropLine.setAttribute('data-mde-holds-keys', '');
        // On the document, not on the handle: pointer capture is asked for too, but it is not what
        // this relies on (measured with synthetic mouse events: the moves went to the element below).
        document.addEventListener('pointermove', onMove, true);
        document.addEventListener('pointerup', onUp, true);
        document.addEventListener('pointercancel', endDrag, true);
        try { handle.setPointerCapture(event.pointerId); } catch (e) { /* the document's listeners carry on */ }
        onMove(event);
        return true;
    }

    handle.addEventListener('pointerdown', function (event) {
        if (event.button !== 0 || !current || !movable(current)) return;
        event.preventDefault();
        event.stopPropagation();
        begin(current, event);
    });

    // The mode went off (the button, or Esc): no handle, and a drag in progress is dropped.
    window.addEventListener('mde-edit-mode', function (event) {
        if (event.detail && event.detail.on) return;
        if (drag) endDrag();
        hideHandle();
    });

    window.mdeListDrag = {
        /** The item that can be moved at this element (a list item with its file line, in a list of two or more), or null. */
        itemAt: function (target) {
            var el = target && target.nodeType === Node.ELEMENT_NODE ? target : (target && target.parentElement);
            var item = el && el.closest(ITEM);
            return item && movable(item) ? item : null;
        },
        begin: begin
    };

    function onMove(event) {
        if (!drag) return;
        // The button was let go where this page could not see it (outside the window): cancel, do not stay stuck.
        if (event.type === 'pointermove' && event.buttons === 0) {
            endDrag();
            return;
        }
        if (!withinReach(event.clientY)) {
            drag.to = null;
            dropLine.hidden = true;
            return;
        }
        drag.to = targetIndex(event.clientY);
        showDropLine(drag.to);
    }

    function onUp() {
        if (!drag) return;
        var item = drag.item, siblings = drag.siblings, from = drag.from, to = drag.to;
        endDrag();
        if (to === null || to === from) return;
        move(item, siblings, to);
    }

    // Esc cancels the drag: in capture, before reveal.js sees it (it is told to leave the keys).
    window.addEventListener('keydown', function (event) {
        if (drag && event.key === 'Escape') {
            event.preventDefault();
            event.stopPropagation();
            endDrag();
        }
    }, true);

    // ---- the server ----

    function showNotice(message) {
        var notice = document.createElement('div');
        notice.className = 'mde-inline-edit-toast';
        notice.textContent = message;
        notice.addEventListener('click', function () { notice.remove(); });
        document.body.appendChild(notice);
        setTimeout(function () { if (notice.isConnected) notice.remove(); }, 8000);
    }

    function move(item, siblings, to) {
        var body = document.body;
        fetch('/api/mdfiles/MoveListItem', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                connectionId: body.getAttribute('ConnectionId'),
                documentPath: body.getAttribute('DocumentPath'),
                sourceHash: body.getAttribute('data-mde-source-hash'),
                line: parseInt(item.getAttribute('data-mde-line-start'), 10),
                toIndex: to
            })
        }).then(function (response) {
            return response.json().catch(function () { return {}; }).then(function (result) {
                if (response.ok) {
                    // The file is written; the view reloads by itself. Until then the item is shown
                    // where it went, so the page does not flicker back to the old order.
                    place(item, siblings, to);
                    return;
                }
                showNotice(result.message || result.error || ('Non sono riuscito a spostare la voce (HTTP ' + response.status + ').'));
            });
        }).catch(function (error) {
            console.error('[slide-list-drag] The item could not be moved:', error);
            showNotice('Non sono riuscito a spostare la voce: ' + error.message);
        });
    }

    /** The item where the server put it: at position <to> among the others. */
    function place(item, siblings, to) {
        var others = siblings.filter(function (s) { return s !== item; });
        var before = others[to];
        if (before) before.parentElement.insertBefore(item, before);
        else others[others.length - 1].parentElement.insertBefore(item, others[others.length - 1].nextSibling);
    }
})();
