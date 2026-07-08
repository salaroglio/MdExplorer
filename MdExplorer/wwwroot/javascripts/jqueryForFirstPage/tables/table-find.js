/**
 * MdExplorer - "Find in table" floating widget
 * ============================================
 * Hovering a markdown table shows a small magnifier button at its top-left,
 * floating with the (sticky) header. Clicking it opens a floating search box
 * that highlights matching cells (yellow, current = orange) and lets you jump
 * between them (↑ ↓ / Enter / Shift+Enter, circular). Esc or × closes.
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

    var btn, box, input, counter, colSelect;
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
        colSelect = document.createElement('select');
        colSelect.className = 'mde-tfind-col';
        colSelect.title = 'Colonna in cui cercare';
        counter = document.createElement('span');
        counter.className = 'mde-tfind-count';
        var prev = mkBtn('↑', 'Precedente (Shift+Invio)');
        var next = mkBtn('↓', 'Successivo (Invio)');
        var close = mkBtn('×', 'Chiudi (Esc)');
        close.className = 'mde-tfind-close';
        box.appendChild(input);
        box.appendChild(colSelect);
        box.appendChild(counter);
        box.appendChild(prev);
        box.appendChild(next);
        box.appendChild(close);
        document.body.appendChild(box);

        btn.addEventListener('click', toggleSearch);
        btn.addEventListener('mouseenter', cancelHide);
        btn.addEventListener('mouseleave', scheduleHide);
        box.addEventListener('mouseenter', cancelHide);
        colSelect.addEventListener('change', rerun);
        input.addEventListener('input', onInput);
        input.addEventListener('keydown', onKey);
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
        box.style.top = top + 'px';
        box.style.left = (left + 32) + 'px';
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
        clearHighlights();
        matches = []; current = -1;
        input.focus();
    }

    // Fill the column combo from the active table's header cells.
    function populateColumns() {
        colSelect.innerHTML = '';
        var all = document.createElement('option');
        all.value = '';
        all.textContent = 'Tutte le colonne';
        colSelect.appendChild(all);
        var headRow = activeTable && activeTable.tHead && activeTable.tHead.rows[0];
        if (!headRow) return;
        for (var i = 0; i < headRow.cells.length; i++) {
            var o = document.createElement('option');
            o.value = String(i);
            o.textContent = (headRow.cells[i].textContent || '').trim() || ('Colonna ' + (i + 1));
            colSelect.appendChild(o);
        }
        colSelect.value = '';
    }

    // Re-run the current search (e.g. when the column filter changes).
    function rerun() {
        var term = input.value.trim();
        if (term.length < 2) { clearHighlights(); matches = []; current = -1; counter.textContent = ''; return; }
        execute(term);
    }

    function closeSearch() {
        searchOpen = false;
        box.style.display = 'none';
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

    function execute(term) {
        clearHighlights();
        matches = []; current = -1;
        var tl = term.toLowerCase();
        var col = colSelect.value === '' ? -1 : parseInt(colSelect.value, 10); // -1 = all columns
        var cells = activeTable.querySelectorAll('thead th, tbody th, tbody td');
        cells.forEach(function (cell) {
            if (col >= 0 && cell.cellIndex !== col) return; // restrict to the chosen column
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
