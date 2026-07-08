/**
 * MdExplorer - "Find in table" floating widget
 * ============================================
 * Hovering a markdown table shows a small magnifier button at its top-left,
 * floating with the (sticky) header. Clicking it opens a floating search box
 * that highlights matching cells (yellow, current = orange) and lets you jump
 * between them (↑ ↓ / Enter / Shift+Enter, circular). Esc or × closes.
 *
 * A "Colonne (n/tot) ▾" button opens a dropdown panel with one checkbox per
 * column (all ticked by default) plus a master "Tutte" checkbox to select /
 * deselect them all. The search only matches cells of the ticked columns.
 *
 * Same UX as the "search inside SVG diagram" feature, but entirely front-end
 * (no backend button generation): one shared button + one shared box, both
 * position:fixed and re-anchored to the table currently in play.
 *
 * Runs once per document (the viewer iframe reloads fully per file).
 */
(function () {
    if (window.__mdeTableFindLoaded) return;
    window.__mdeTableFindLoaded = true;

    var btn, box, input, counter, colBtn, colPanel, colMaster;
    var colCheckboxes = [];   // index = column index
    var activeTable = null;   // table the widget currently targets
    var searchOpen = false;
    var matches = [];         // matching cells
    var current = -1;
    var hideTimer = null, debounceTimer = null, rafPending = false;

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

        box = document.createElement('div');
        box.className = 'mde-tfind-box';
        input = document.createElement('input');
        input.type = 'text';
        input.placeholder = 'Cerca nella tabella…';
        colBtn = document.createElement('button');
        colBtn.className = 'mde-tfind-colbtn';
        colBtn.title = 'Colonne in cui cercare';
        colBtn.textContent = 'Colonne ▾';
        counter = document.createElement('span');
        counter.className = 'mde-tfind-count';
        var prev = mkBtn('↑', 'Precedente (Shift+Invio)');
        var next = mkBtn('↓', 'Successivo (Invio)');
        var close = mkBtn('×', 'Chiudi (Esc)');
        close.className = 'mde-tfind-close';
        box.appendChild(input);
        box.appendChild(colBtn);
        box.appendChild(counter);
        box.appendChild(prev);
        box.appendChild(next);
        box.appendChild(close);
        document.body.appendChild(box);

        colPanel = document.createElement('div');
        colPanel.className = 'mde-tfind-cols-panel';
        document.body.appendChild(colPanel);

        btn.addEventListener('click', toggleSearch);
        btn.addEventListener('mouseenter', cancelHide);
        btn.addEventListener('mouseleave', scheduleHide);
        box.addEventListener('mouseenter', cancelHide);
        colPanel.addEventListener('mouseenter', cancelHide);
        colBtn.addEventListener('click', toggleColPanel);
        input.addEventListener('input', onInput);
        input.addEventListener('keydown', onKey);
        prev.addEventListener('click', function () { navigate(-1); });
        next.addEventListener('click', function () { navigate(1); });
        close.addEventListener('click', closeSearch);
        // click outside the panel closes it
        document.addEventListener('click', function (e) {
            if (colPanel.style.display === 'block' &&
                !colPanel.contains(e.target) && e.target !== colBtn) {
                colPanel.style.display = 'none';
            }
        });
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
        box.style.top = top + 'px';
        box.style.left = (left + 32) + 'px';
        if (colPanel.style.display === 'block') positionColPanel();
    }

    function positionColPanel() {
        var r = colBtn.getBoundingClientRect();
        colPanel.style.top = Math.round(r.bottom + 2) + 'px';
        colPanel.style.left = Math.round(r.left) + 'px';
    }

    function showBtn() { btn.style.display = 'flex'; }
    function hideBtn() { if (!searchOpen) btn.style.display = 'none'; }
    function cancelHide() { if (hideTimer) { clearTimeout(hideTimer); hideTimer = null; } }
    function scheduleHide() { cancelHide(); hideTimer = setTimeout(hideBtn, 250); }

    // ---- hover ----------------------------------------------------------
    function onEnterTable(t) {
        if (searchOpen) return; // don't hijack the widget mid-search
        activeTable = t;
        cancelHide();
        positionUI();
        showBtn();
    }
    function onLeaveTable() { if (!searchOpen) scheduleHide(); }

    // ---- open / close ---------------------------------------------------
    function toggleSearch() { searchOpen ? closeSearch() : openSearch(); }

    function openSearch() {
        if (!activeTable) return;
        searchOpen = true;
        cancelHide();
        showBtn();
        positionUI();
        box.style.display = 'flex';
        input.value = '';
        counter.textContent = '';
        populateColumns();
        colPanel.style.display = 'none';
        clearHighlights();
        matches = []; current = -1;
        input.focus();
    }

    function closeSearch() {
        searchOpen = false;
        box.style.display = 'none';
        colPanel.style.display = 'none';
        clearHighlights();
        matches = []; current = -1;
        scheduleHide();
    }

    // ---- column checkboxes ----------------------------------------------
    function toggleColPanel(e) {
        e.stopPropagation();
        if (colPanel.style.display === 'block') {
            colPanel.style.display = 'none';
        } else {
            positionColPanel();
            colPanel.style.display = 'block';
        }
    }

    function populateColumns() {
        colPanel.innerHTML = '';
        colCheckboxes = [];

        var masterLabel = document.createElement('label');
        masterLabel.className = 'mde-tfind-col-all';
        colMaster = document.createElement('input');
        colMaster.type = 'checkbox';
        colMaster.checked = true;
        masterLabel.appendChild(colMaster);
        masterLabel.appendChild(document.createTextNode(' Tutte'));
        colPanel.appendChild(masterLabel);
        colMaster.addEventListener('change', function () {
            colCheckboxes.forEach(function (cb) { cb.checked = colMaster.checked; });
            colMaster.indeterminate = false;
            updateColBtnLabel();
            rerun();
        });

        var headRow = activeTable && activeTable.tHead && activeTable.tHead.rows[0];
        if (headRow) {
            for (var i = 0; i < headRow.cells.length; i++) {
                var lbl = document.createElement('label');
                var cb = document.createElement('input');
                cb.type = 'checkbox';
                cb.checked = true;
                cb.value = String(i);
                lbl.appendChild(cb);
                var name = (headRow.cells[i].textContent || '').trim() || ('Colonna ' + (i + 1));
                lbl.appendChild(document.createTextNode(' ' + name));
                colPanel.appendChild(lbl);
                colCheckboxes.push(cb);
                cb.addEventListener('change', function () {
                    syncMaster();
                    updateColBtnLabel();
                    rerun();
                });
            }
        }
        updateColBtnLabel();
    }

    function syncMaster() {
        var all = colCheckboxes.every(function (cb) { return cb.checked; });
        var none = colCheckboxes.every(function (cb) { return !cb.checked; });
        colMaster.checked = all;
        colMaster.indeterminate = !all && !none;
    }

    function updateColBtnLabel() {
        var n = colCheckboxes.filter(function (cb) { return cb.checked; }).length;
        colBtn.textContent = 'Colonne (' + n + '/' + colCheckboxes.length + ') ▾';
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
            var cb = colCheckboxes[cell.cellIndex];
            if (cb && !cb.checked) return; // column not selected
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

    // ---- reposition on scroll/resize ------------------------------------
    function scheduleReposition() {
        if (rafPending) return;
        rafPending = true;
        window.requestAnimationFrame(function () {
            rafPending = false;
            if (activeTable && (searchOpen || btn.style.display !== 'none')) positionUI();
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
