/**
 * MdExplorer - The changes of a document, shown on the rendered page
 * ===================================================================
 * The differences tab opens a markdown file as a rendered document
 * (GET /api/MdExplorerChanges/render/...). The page that arrives is the
 * document as it is now, plus:
 *   - #mdeChangesData   the changed parts, as JSON (data-changes);
 *   - #mdeChangesOld    a <template> with the document as it was, rendered whole.
 * In both documents a changed part made of whole blocks sits between two marker
 * elements (.mde-chg-s / .mde-chg-e with the same data-r); a part made of list
 * items is found by the items' data-mde-line-start.
 *
 * This module turns each part into a box with two tabs, «Before» and «After»,
 * and puts a bar at the top of the document to move from one part to the next.
 * Only the changed part is boxed: the rest of the document stays as it is.
 *
 * It must run before the other modules look at the page (table of contents,
 * diagrams, images, tables): it is loaded right after the core ones and listens
 * to DOMContentLoaded natively, so it is the first to run.
 */
(function () {
    if (window.__mdeChangesReviewLoaded) return;
    window.__mdeChangesReviewLoaded = true;

    function el(tag, className, text) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined && text !== null) node.textContent = text;
        return node;
    }

    /** What sits between the two markers of a part, taken out of its document. Null when a marker is missing. */
    function takeBetweenMarkers(root, ownerDocument, id) {
        var start = root.querySelector('.mde-chg-s[data-r="' + id + '"]');
        var end = root.querySelector('.mde-chg-e[data-r="' + id + '"]');
        if (!start || !end) return null;
        var range = ownerDocument.createRange();
        range.setStartAfter(start);
        range.setEndBefore(end);
        // A part can start in one section and end in the next (each section is a div.editorH1):
        // the range takes it across, cloning the section boxes it only partly covers.
        var content = range.extractContents();
        return { content: content, start: start, end: end };
    }

    function hasSomethingToShow(fragment) {
        if (!fragment) return false;
        if (fragment.querySelector('img, svg, table, hr, pre, li, canvas, iframe')) return true;
        return (fragment.textContent || '').trim().length > 0;
    }

    /** The old version lives in the same page as the new one: its ids must not collide, its headings must stay out of the TOC. */
    function neutralise(node) {
        if (!node.querySelectorAll) return;
        node.querySelectorAll('.mde-img-toolbar').forEach(function (toolbar) { toolbar.remove(); });
        node.querySelectorAll('[id]').forEach(function (child) { child.id = 'mde-old-' + child.id; });
        node.querySelectorAll('[onmouseenter], [onmouseleave]').forEach(function (child) {
            child.removeAttribute('onmouseenter');
            child.removeAttribute('onmouseleave');
        });
        node.querySelectorAll('h1, h2, h3, h4, h5, h6').forEach(function (heading) { heading.classList.add('js-toc-ignore'); });
    }

    function buildBox(region, labels, oldContent, newContent) {
        var box = el('div', 'mde-chg-box mde-chg-' + region.kind);
        box.setAttribute('data-r', region.id);
        var tabs = el('div', 'mde-chg-tabs');
        var oldTab = el('button', 'mde-chg-tab', labels.before);
        var newTab = el('button', 'mde-chg-tab', labels.after);
        oldTab.type = 'button';
        newTab.type = 'button';
        tabs.appendChild(oldTab);
        tabs.appendChild(newTab);

        var oldPane = el('div', 'mde-chg-pane mde-chg-pane-old');
        var newPane = el('div', 'mde-chg-pane mde-chg-pane-new');
        if (hasSomethingToShow(oldContent)) oldPane.appendChild(oldContent);
        else oldPane.appendChild(el('p', 'mde-chg-empty', labels.notThere));
        if (hasSomethingToShow(newContent)) newPane.appendChild(newContent);
        else newPane.appendChild(el('p', 'mde-chg-empty', labels.removed));
        neutralise(oldPane);

        function show(which) {
            var old = which === 'old';
            // Not display:none: the page modules measure tables and images, and what is not laid out measures zero.
            oldPane.classList.toggle('is-off', !old);
            newPane.classList.toggle('is-off', old);
            oldTab.classList.toggle('is-on', old);
            newTab.classList.toggle('is-on', !old);
        }
        oldTab.addEventListener('click', function () { show('old'); });
        newTab.addEventListener('click', function () { show('new'); });

        box.appendChild(tabs);
        box.appendChild(oldPane);
        box.appendChild(newPane);
        // A removed part opens on what was there: «After» would only say «Removed».
        show(region.kind === 'removed' ? 'old' : 'new');
        return box;
    }

    function blockPart(region, labels, center, oldRoot) {
        var taken = takeBetweenMarkers(center, document, region.id);
        if (!taken) return null;
        var old = oldRoot ? takeBetweenMarkers(oldRoot, oldRoot.ownerDocument, region.id) : null;
        var box = buildBox(region, labels, old ? old.content : null, taken.content);
        taken.start.parentNode.insertBefore(box, taken.start.nextSibling);
        return box;
    }

    function itemAt(root, line) {
        return line ? root.querySelector('li[data-mde-line-start="' + line + '"]') : null;
    }

    /** A list like the one the items come from, empty: same tag, same classes, numbered from where they were. */
    function emptyListLike(items) {
        if (!items.length) return null;
        var list = items[0].parentNode.cloneNode(false);
        list.removeAttribute('id');
        list.removeAttribute('data-mde-line-start');
        list.removeAttribute('data-mde-line-end');
        if (list.tagName === 'OL') {
            var position = 0;
            for (var sibling = items[0]; sibling; sibling = sibling.previousElementSibling) {
                if (sibling.tagName === 'LI' && !sibling.classList.contains('mde-chg-li')) position++;
            }
            var first = parseInt(items[0].parentNode.getAttribute('start') || '1', 10) || 1;
            list.setAttribute('start', first + position - 1);
        }
        return list;
    }

    function listPart(region, labels, center, oldRoot) {
        var newItems = (region.newItems || []).map(function (line) { return itemAt(center, line); });
        var oldItems = oldRoot ? (region.oldItems || []).map(function (line) { return itemAt(oldRoot, line); }) : [];
        if (newItems.indexOf(null) >= 0 || oldItems.indexOf(null) >= 0) return null;

        // Where the box goes in the list as it is now: where the changed items are, or where the removed ones were.
        var holder = el('li', 'mde-chg-li');
        if (newItems.length) {
            newItems[0].parentNode.insertBefore(holder, newItems[0]);
        } else {
            var before = itemAt(center, region.newBeforeItem);
            var after = itemAt(center, region.newAfterItem);
            if (before) before.parentNode.insertBefore(holder, before);
            else if (after) after.parentNode.insertBefore(holder, after.nextSibling);
            else return null;
        }

        // In a numbered list the items after the box keep their number: the box is not an item.
        if (holder.parentNode.tagName === 'OL') {
            var position = 0;
            var firstNumber = parseInt(holder.parentNode.getAttribute('start') || '1', 10) || 1;
            Array.prototype.forEach.call(holder.parentNode.children, function (child) {
                if (child.tagName !== 'LI' || child === holder) return;
                if (!child.hasAttribute('value')) child.setAttribute('value', firstNumber + position);
                position++;
            });
        }

        function gather(items) {
            var list = emptyListLike(items);
            if (!list) return null;
            items.forEach(function (item) { list.appendChild(item); });
            var fragment = document.createDocumentFragment();
            fragment.appendChild(list);
            return fragment;
        }
        var newContent = gather(newItems);
        var oldContent = gather(oldItems);
        holder.appendChild(buildBox(region, labels, oldContent, newContent));
        return holder.firstChild;
    }

    function buildBar(data, boxes, failed) {
        var labels = data.labels;
        var bar = el('div', 'mde-chg-bar');
        var text;
        if (data.kind === 'added') text = labels.added;
        else if (data.kind === 'deleted') text = labels.deleted;
        else if (!boxes.length) text = labels.none;
        else text = boxes.length + ' ' + (boxes.length === 1 ? labels.one : labels.count);
        var summary = el('span', 'mde-chg-bar-text', text);
        bar.appendChild(summary);
        if (data.baseRef && data.kind !== 'added') bar.appendChild(el('span', 'mde-chg-bar-ref', labels.comparedWith + ' ' + data.baseRef));

        var spacer = el('span', 'mde-chg-bar-space');
        bar.appendChild(spacer);

        if (boxes.length) {
            var current = -1;
            var show = function () {
                boxes.forEach(function (b) { b.classList.remove('is-current'); });
                boxes[current].classList.add('is-current');
                boxes[current].scrollIntoView({ behavior: 'smooth', block: 'center' });
            };
            var previous = el('button', 'mde-chg-bar-button', '↑ ' + labels.previous);
            var next = el('button', 'mde-chg-bar-button', '↓ ' + labels.next);
            previous.type = 'button';
            next.type = 'button';
            // The first press goes to the first part (or to the last, going backwards).
            previous.addEventListener('click', function () {
                current = current < 0 ? boxes.length - 1 : (current - 1 + boxes.length) % boxes.length;
                show();
            });
            next.addEventListener('click', function () {
                current = current < 0 ? 0 : (current + 1) % boxes.length;
                show();
            });
            bar.appendChild(previous);
            bar.appendChild(next);
        }

        var asText = el('button', 'mde-chg-bar-button mde-chg-bar-text-view', labels.asText);
        asText.type = 'button';
        asText.addEventListener('click', function () {
            window.parent.postMessage({ type: 'mde-changes.showAsText' }, '*');
        });
        bar.appendChild(asText);

        var wrapper = el('div', 'mde-chg-head');
        wrapper.appendChild(bar);
        if (data.hiddenChanges) wrapper.appendChild(el('div', 'mde-chg-note', labels.hidden));
        if (failed) wrapper.appendChild(el('div', 'mde-chg-note mde-chg-note-error',
            'MdExplorer: ' + failed + ' / ' + data.regions.length + ' — ' + labels.asText));
        return wrapper;
    }

    function compose() {
        var dataElement = document.getElementById('mdeChangesData');
        var center = document.querySelector('.mdeItemMainPageCenter');
        if (!dataElement || !center) return;

        var data;
        try { data = JSON.parse(dataElement.getAttribute('data-changes')); }
        catch (error) { console.error('[changes-review] unreadable data', error); return; }

        var template = document.getElementById('mdeChangesOld');
        var oldRoot = template && template.content ? template.content : null;

        var boxes = [];
        var failed = 0;
        (data.regions || []).forEach(function (region) {
            var box = region.listItems ? listPart(region, data.labels, center, oldRoot) : blockPart(region, data.labels, center, oldRoot);
            if (box) boxes.push(box);
            else { failed++; console.error('[changes-review] part not found on the page', region); }
        });

        // Whatever is left of the scaffolding goes: markers, the old document, the data.
        center.querySelectorAll('.mde-chg-s, .mde-chg-e').forEach(function (marker) { marker.remove(); });
        if (template) template.remove();
        dataElement.remove();

        document.body.classList.add('mde-chg-view', 'mde-chg-doc-' + data.kind);
        center.insertBefore(buildBar(data, boxes, failed), center.firstChild);
    }

    // A native listener, not jQuery's ready: jQuery 3 calls its ready handlers later than the native ones, and the
    // modules that listen natively (the tables') would look at the page before the old version is in it.
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', compose);
    else compose();
})();
