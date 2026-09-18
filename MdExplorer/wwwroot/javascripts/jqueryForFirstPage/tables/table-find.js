/**
 * MdExplorer - "Find in table" and "Copy table" floating widget
 * =============================================================
 * Hovering a markdown table shows two small buttons at its top-left, floating
 * with the (sticky) header:
 *
 * - magnifier: opens a floating search box that highlights matching cells
 *   (yellow, current = orange) and lets you jump between them (↑ ↓ / Enter /
 *   Shift+Enter, circular). Esc or × closes.
 * - copy [⧉▾]: ⧉ copies the whole table at once; ▾ opens a box to choose the
 *   columns to copy, then "Copia". The clipboard gets the table twice — as HTML
 *   (Word, Outlook: a real table; Excel: one value per cell) and as
 *   tab-separated text (plain editors) — and a short message says what was
 *   copied: "Copiata: 12 righe × 4 colonne".
 *
 * Both boxes have a "Colonne (n/tot) ▾" button that opens a dropdown panel with
 * one checkbox per column (all ticked by default) plus a master "Tutte". Search
 * and copy have a panel each: choosing what to copy does not change where the
 * search looks. The copy choice is remembered per table while the page is open.
 *
 * Same UX as the "search inside SVG diagram" feature, but entirely front-end
 * (no backend button generation): one shared set of buttons and boxes, all
 * position:fixed and re-anchored to the table currently in play. One box open
 * at a time.
 *
 * Runs once per document (the viewer iframe reloads fully per file).
 */
(function () {
    if (window.__mdeTableFindLoaded) return;
    window.__mdeTableFindLoaded = true;

    var COPY_ICON = '<svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path></svg>';

    var btn, copyGroup, copyBtn, copyMenuBtn;
    var box, input, counter, searchCols;
    var copyBox, copyCols, copyDoBtn;
    var toast;
    var activeTable = null;   // table the widget currently targets
    var searchOpen = false;
    var copyOpen = false;
    var matches = [];         // matching cells
    var current = -1;
    var hideTimer = null, debounceTimer = null, rafPending = false, toastTimer = null;

    function widgetOpen() { return searchOpen || copyOpen; }

    // ---- column picker: "Colonne (n/tot) ▾" + checkbox panel -------------
    // Used twice (search, copy): same look, independent choices.
    function createColumnPicker(title, onChange) {
        var button = document.createElement('button');
        button.className = 'mde-tfind-colbtn';
        button.title = title;
        var panel = document.createElement('div');
        panel.className = 'mde-tfind-cols-panel';
        document.body.appendChild(panel);
        var boxes = [];           // index = column index
        var master = null;

        function isOpen() { return panel.style.display === 'block'; }
        function close() { panel.style.display = 'none'; }
        function position() {
            var r = button.getBoundingClientRect();
            panel.style.top = Math.round(r.bottom + 2) + 'px';
            panel.style.left = Math.round(r.left) + 'px';
        }

        button.addEventListener('click', function (e) {
            e.stopPropagation();
            if (isOpen()) { close(); } else { position(); panel.style.display = 'block'; }
        });
        panel.addEventListener('mouseenter', cancelHide);
        // click outside the panel closes it
        document.addEventListener('click', function (e) {
            if (isOpen() && !panel.contains(e.target) && e.target !== button) close();
        });

        function syncMaster() {
            var all = boxes.every(function (cb) { return cb.checked; });
            var none = boxes.every(function (cb) { return !cb.checked; });
            master.checked = all;
            master.indeterminate = !all && !none;
        }

        function updateLabel() {
            button.textContent = 'Colonne (' + selected().length + '/' + boxes.length + ') ▾';
        }

        function changed() { updateLabel(); if (onChange) onChange(); }

        /** @param {number[]=} ticked columns to tick; all when omitted */
        function populate(table, ticked) {
            panel.innerHTML = '';
            boxes = [];

            var masterLabel = document.createElement('label');
            masterLabel.className = 'mde-tfind-col-all';
            master = document.createElement('input');
            master.type = 'checkbox';
            masterLabel.appendChild(master);
            masterLabel.appendChild(document.createTextNode(' Tutte'));
            panel.appendChild(masterLabel);
            master.addEventListener('change', function () {
                boxes.forEach(function (cb) { cb.checked = master.checked; });
                master.indeterminate = false;
                changed();
            });

            var headRow = headerRow(table);
            if (headRow) {
                for (var i = 0; i < headRow.cells.length; i++) {
                    var lbl = document.createElement('label');
                    var cb = document.createElement('input');
                    cb.type = 'checkbox';
                    cb.checked = !ticked || ticked.indexOf(i) !== -1;
                    cb.value = String(i);
                    lbl.appendChild(cb);
                    lbl.appendChild(document.createTextNode(' ' + columnName(headRow, i)));
                    panel.appendChild(lbl);
                    boxes.push(cb);
                    cb.addEventListener('change', function () { syncMaster(); changed(); });
                }
            }
            syncMaster();
            updateLabel();
        }

        function selected() {
            return boxes.filter(function (cb) { return cb.checked; }).map(function (cb) { return +cb.value; });
        }

        // A column without a checkbox (a row longer than the header) counts as selected.
        function isSelected(i) { var cb = boxes[i]; return !cb || cb.checked; }

        return {
            button: button, populate: populate, selected: selected, isSelected: isSelected,
            close: close, isOpen: isOpen, position: position,
            count: function () { return boxes.length; }
        };
    }

    function headerRow(table) { return table && table.tHead && table.tHead.rows[0]; }

    function columnName(headRow, i) {
        return (headRow.cells[i].textContent || '').trim() || ('Colonna ' + (i + 1));
    }

    // ---- UI construction -------------------------------------------------
    function buildUI() {
        btn = document.createElement('button');
        btn.className = 'mde-tfind-btn';
        btn.title = 'Cerca nella tabella';
        var img = document.createElement('img');
        img.src = '/assets/magnifier.svg';
        img.alt = 'cerca';
        btn.appendChild(img);
        document.body.appendChild(btn);

        // copy: ⧉ copies everything, ▾ opens the column choice
        copyGroup = document.createElement('div');
        copyGroup.className = 'mde-tcopy-group';
        copyBtn = document.createElement('button');
        copyBtn.className = 'mde-tcopy-btn';
        copyBtn.title = 'Copia la tabella (Excel, Word…)';
        copyBtn.innerHTML = COPY_ICON;
        copyMenuBtn = document.createElement('button');
        copyMenuBtn.className = 'mde-tcopy-menu';
        copyMenuBtn.title = 'Scegli le colonne da copiare';
        copyMenuBtn.textContent = '▾';
        copyGroup.appendChild(copyBtn);
        copyGroup.appendChild(copyMenuBtn);
        document.body.appendChild(copyGroup);

        // search box
        box = document.createElement('div');
        box.className = 'mde-tfind-box';
        input = document.createElement('input');
        input.type = 'text';
        input.placeholder = 'Cerca nella tabella…';
        searchCols = createColumnPicker('Colonne in cui cercare', rerun);
        counter = document.createElement('span');
        counter.className = 'mde-tfind-count';
        var prev = mkBtn('↑', 'Precedente (Shift+Invio)');
        var next = mkBtn('↓', 'Successivo (Invio)');
        var close = mkBtn('×', 'Chiudi (Esc)');
        close.className = 'mde-tfind-close';
        box.appendChild(input);
        box.appendChild(searchCols.button);
        box.appendChild(counter);
        box.appendChild(prev);
        box.appendChild(next);
        box.appendChild(close);
        document.body.appendChild(box);

        // copy box
        copyBox = document.createElement('div');
        copyBox.className = 'mde-tfind-box mde-tcopy-box';
        copyCols = createColumnPicker('Colonne da copiare', onCopyColumnsChanged);
        copyDoBtn = mkBtn('Copia', 'Copia le colonne scelte');
        copyDoBtn.className = 'mde-tcopy-do';
        var copyClose = mkBtn('×', 'Chiudi (Esc)');
        copyClose.className = 'mde-tfind-close';
        copyBox.appendChild(copyCols.button);
        copyBox.appendChild(copyDoBtn);
        copyBox.appendChild(copyClose);
        document.body.appendChild(copyBox);

        toast = document.createElement('div');
        toast.className = 'mde-tcopy-toast';
        document.body.appendChild(toast);

        btn.addEventListener('click', toggleSearch);
        copyBtn.addEventListener('click', function () { copyTable(null); });
        // stopPropagation: the column panel opens in this click, and the "click outside closes it"
        // listener on document would otherwise close it as the same click bubbles up.
        copyMenuBtn.addEventListener('click', function (e) { e.stopPropagation(); toggleCopyBox(); });
        copyDoBtn.addEventListener('click', function () { copyTable(copyCols.selected()); });
        copyClose.addEventListener('click', closeCopyBox);
        [btn, copyGroup].forEach(function (el) {
            el.addEventListener('mouseenter', cancelHide);
            el.addEventListener('mouseleave', scheduleHide);
        });
        box.addEventListener('mouseenter', cancelHide);
        copyBox.addEventListener('mouseenter', cancelHide);
        input.addEventListener('input', onInput);
        input.addEventListener('keydown', onKey);
        // On document, not on the box: after a click on a column checkbox the focus is in the
        // column panel, which is not inside the box (found in the browser: Esc did nothing).
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && copyOpen) { e.preventDefault(); closeCopyBox(); }
        });
        prev.addEventListener('click', function () { navigate(-1); });
        next.addEventListener('click', function () { navigate(1); });
        close.addEventListener('click', closeSearch);
    }

    function mkBtn(html, title) {
        var b = document.createElement('button');
        b.innerHTML = html;
        b.title = title;
        return b;
    }

    // ---- positioning (anchor to the table's top-left corner) -------------
    function positionUI() {
        if (!activeTable) return;
        var r = activeTable.getBoundingClientRect();
        var top = Math.round(r.top + 4), left = Math.round(r.left + 4);
        btn.style.top = top + 'px';
        btn.style.left = left + 'px';
        copyGroup.style.top = top + 'px';
        copyGroup.style.left = (left + 32) + 'px';
        // the boxes open to the right of both buttons
        var boxLeft = left + 32 + (copyGroup.offsetWidth || 44) + 6;
        box.style.top = top + 'px';
        box.style.left = boxLeft + 'px';
        copyBox.style.top = top + 'px';
        copyBox.style.left = boxLeft + 'px';
        if (searchCols.isOpen()) searchCols.position();
        if (copyCols.isOpen()) copyCols.position();
    }

    function showBtn() { btn.style.display = 'flex'; copyGroup.style.display = 'flex'; }
    function hideBtn() {
        if (widgetOpen()) return;
        btn.style.display = 'none';
        copyGroup.style.display = 'none';
    }
    function cancelHide() { if (hideTimer) { clearTimeout(hideTimer); hideTimer = null; } }
    function scheduleHide() { cancelHide(); hideTimer = setTimeout(hideBtn, 250); }

    // ---- hover ----------------------------------------------------------
    function onEnterTable(t) {
        if (widgetOpen()) return; // don't hijack the widget mid-search / mid-choice
        activeTable = t;
        cancelHide();
        positionUI();
        showBtn();
    }
    function onLeaveTable() { if (!widgetOpen()) scheduleHide(); }

    // ---- search: open / close -------------------------------------------
    function toggleSearch() { searchOpen ? closeSearch() : openSearch(); }

    function openSearch() {
        if (!activeTable) return;
        if (copyOpen) closeCopyBox();
        hideToast();
        searchOpen = true;
        cancelHide();
        showBtn();
        positionUI();
        box.style.display = 'flex';
        input.value = '';
        counter.textContent = '';
        searchCols.populate(activeTable);
        searchCols.close();
        clearHighlights();
        matches = []; current = -1;
        input.focus();
    }

    function closeSearch() {
        searchOpen = false;
        box.style.display = 'none';
        searchCols.close();
        clearHighlights();
        matches = []; current = -1;
        scheduleHide();
    }

    // ---- search ---------------------------------------------------------
    function clearHighlights() {
        if (!activeTable) return;
        activeTable.querySelectorAll('.mde-tfind-hit, .mde-tfind-current').forEach(function (c) {
            c.classList.remove('mde-tfind-hit', 'mde-tfind-current');
        });
    }

    function onInput() {
        if (debounceTimer) clearTimeout(debounceTimer);
        var term = input.value.trim();
        if (term.length < 2) {
            clearHighlights(); matches = []; current = -1;
            counter.textContent = term.length ? 'min 2' : '';
            return;
        }
        debounceTimer = setTimeout(function () { execute(term); }, 200);
    }

    // Re-run the current search (e.g. when the column selection changes).
    function rerun() {
        if (!searchOpen) return;
        var term = input.value.trim();
        if (term.length < 2) { clearHighlights(); matches = []; current = -1; counter.textContent = ''; return; }
        execute(term);
    }

    function execute(term) {
        clearHighlights();
        matches = []; current = -1;
        var tl = term.toLowerCase();
        var cells = activeTable.querySelectorAll('thead th, tbody th, tbody td');
        cells.forEach(function (cell) {
            if (!searchCols.isSelected(cell.cellIndex)) return; // column not selected
            if ((cell.textContent || '').toLowerCase().indexOf(tl) !== -1) {
                cell.classList.add('mde-tfind-hit');
                matches.push(cell);
            }
        });
        if (!matches.length) { counter.textContent = '0'; return; }
        current = 0;
        setCurrent();
    }

    function setCurrent() {
        if (!activeTable) return;
        activeTable.querySelectorAll('.mde-tfind-current').forEach(function (c) {
            c.classList.remove('mde-tfind-current');
        });
        var cell = matches[current];
        cell.classList.add('mde-tfind-current');
        counter.textContent = (current + 1) + ' / ' + matches.length;
        cell.scrollIntoView({ block: 'nearest', inline: 'nearest' });
        positionUI(); // internal/page scroll may have moved the table
    }

    function navigate(dir) {
        if (!matches.length) return;
        current = (current + dir + matches.length) % matches.length;
        setCurrent();
    }

    function onKey(e) {
        if (e.key === 'Escape') { e.preventDefault(); closeSearch(); }
        else if (e.key === 'Enter') { e.preventDefault(); navigate(e.shiftKey ? -1 : 1); }
    }

    // ---- copy: box ------------------------------------------------------
    function toggleCopyBox() { copyOpen ? closeCopyBox() : openCopyBox(); }

    function openCopyBox() {
        if (!activeTable) return;
        if (searchOpen) closeSearch();
        hideToast(); // it sits where the column panel opens
        copyOpen = true;
        cancelHide();
        showBtn();
        positionUI();
        copyBox.style.display = 'flex';
        copyCols.populate(activeTable, activeTable.__mdeCopyColumns);
        onCopyColumnsChanged();
        // The choice is the reason this box exists: open it straight away.
        copyCols.position();
        copyCols.button.click();
        copyDoBtn.focus();
    }

    function closeCopyBox() {
        copyOpen = false;
        copyBox.style.display = 'none';
        copyCols.close();
        scheduleHide();
    }

    function onCopyColumnsChanged() {
        var chosen = copyCols.selected();
        copyDoBtn.disabled = chosen.length === 0;
        if (activeTable) activeTable.__mdeCopyColumns = chosen;
    }

    // ---- copy: content --------------------------------------------------
    /** The table as rows of cell texts, keeping only the given columns (all when null). */
    function tableRows(table, columns) {
        var headRow = headerRow(table);
        var width = headRow ? headRow.cells.length : 0;
        var keep = columns || Array.from({ length: width }, function (_, i) { return i; });
        var rows = [];
        Array.from(table.rows).forEach(function (row) {
            rows.push(keep.map(function (i) {
                var cell = row.cells[i];
                return cell ? cellText(cell) : '';
            }));
        });
        return { rows: rows, columns: keep.length, dataRows: rows.length - (headRow ? 1 : 0) };
    }

    // What the cell shows: rendered text, line breaks kept (innerText, not textContent).
    function cellText(cell) { return (cell.innerText || cell.textContent || '').replace(/ /g, ' ').trim(); }

    function escapeHtml(s) {
        return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    function toHtml(rows) {
        var cellStyle = 'border:1px solid #999;padding:2px 6px;vertical-align:top';
        var html = '<table style="border-collapse:collapse">';
        rows.forEach(function (cells, r) {
            var tag = r === 0 ? 'th' : 'td';
            html += '<tr>' + cells.map(function (c) {
                return '<' + tag + ' style="' + cellStyle + '">' + escapeHtml(c).replace(/\n/g, '<br>') + '</' + tag + '>';
            }).join('') + '</tr>';
        });
        return html + '</table>';
    }

    // Tab-separated: a tab or a line break inside a cell would start a new cell or row.
    function toTsv(rows) {
        return rows.map(function (cells) {
            return cells.map(function (c) { return c.replace(/[\t\r\n]+/g, ' '); }).join('\t');
        }).join('\r\n') + '\r\n';
    }

    function copyTable(columns) {
        if (!activeTable) return;
        if (columns && !columns.length) return;
        var t = tableRows(activeTable, columns);
        var html = toHtml(t.rows), text = toTsv(t.rows);
        writeClipboard(html, text).then(function () {
            showToast('Copiata: ' + t.dataRows + (t.dataRows === 1 ? ' riga' : ' righe') +
                ' × ' + t.columns + (t.columns === 1 ? ' colonna' : ' colonne'), false);
            if (copyOpen) closeCopyBox();
        }, function (err) {
            console.error('[table-copy] copy failed', err);
            showToast('Copia non riuscita: ' + ((err && err.message) || err), true);
        });
    }

    /**
     * HTML and plain text together. The Clipboard API needs a secure context
     * (localhost, the app); a page opened from a network address has none, and
     * there the classic copy event writes the same two formats.
     */
    function writeClipboard(html, text) {
        if (navigator.clipboard && window.ClipboardItem && window.isSecureContext) {
            return navigator.clipboard.write([new ClipboardItem({
                'text/html': new Blob([html], { type: 'text/html' }),
                'text/plain': new Blob([text], { type: 'text/plain' })
            })]);
        }
        return new Promise(function (resolve, reject) {
            var written = false;
            function onCopy(e) {
                e.clipboardData.setData('text/html', html);
                e.clipboardData.setData('text/plain', text);
                e.preventDefault();
                written = true;
            }
            document.addEventListener('copy', onCopy);
            try {
                document.execCommand('copy');
            } finally {
                document.removeEventListener('copy', onCopy);
            }
            written ? resolve() : reject(new Error('il browser non ha permesso di scrivere negli appunti'));
        });
    }

    function showToast(message, isError) {
        if (toastTimer) clearTimeout(toastTimer);
        toast.textContent = message;
        toast.classList.toggle('mde-tcopy-toast-error', !!isError);
        var r = copyGroup.getBoundingClientRect();
        toast.style.top = Math.round(r.bottom + 6) + 'px';
        toast.style.left = Math.round(r.left) + 'px';
        toast.style.display = 'block';
        toastTimer = setTimeout(hideToast, isError ? 5000 : 2500);
    }

    function hideToast() {
        if (toastTimer) { clearTimeout(toastTimer); toastTimer = null; }
        toast.style.display = 'none';
    }

    // ---- reposition on scroll/resize ------------------------------------
    function scheduleReposition() {
        if (rafPending) return;
        rafPending = true;
        window.requestAnimationFrame(function () {
            rafPending = false;
            if (activeTable && (widgetOpen() || btn.style.display !== 'none')) positionUI();
        });
    }

    function scan() {
        document.querySelectorAll('.mdeItemMainPageCenter table.table').forEach(function (t) {
            if (t.__mdeTFindBound) return;
            t.__mdeTFindBound = true;
            t.addEventListener('mouseenter', function () { onEnterTable(t); });
            t.addEventListener('mouseleave', onLeaveTable);
            t.addEventListener('scroll', scheduleReposition, { passive: true });
        });
    }

    function init() {
        if (!document.querySelector('.mdeItemMainPageCenter')) return;
        buildUI();
        scan();
        window.addEventListener('scroll', scheduleReposition, { passive: true });
        window.addEventListener('resize', scheduleReposition);
        window.addEventListener('load', scan);
    }

    window.MdeTableFind = { init: init, rescan: scan };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
