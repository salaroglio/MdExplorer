/**
 * MdExplorer - Narrow column sizing for markdown tables
 * =====================================================
 * The base rule gives every column `min-width: 40ch` so wide tables keep a
 * horizontal scrollbar inside their box. But columns whose data cells are
 * short — pure numbers ("Progressivo", "Modalità") OR short codes/sigle
 * ("Web Services" = "YBT3", "Si/No", "Causale", "Schema"...) — don't need 40ch;
 * at that width they'd be mostly empty.
 *
 * This module tags such columns:
 *   - `.mde-narrowcol` — any column that is mostly short (numbers or sigle):
 *     CSS drops the 40ch floor and wraps the (usually long) header onto spaces.
 *   - `.mde-numcol` — the subset that is numeric: CSS also centres the values.
 * Pure classification; all sizing lives in CSS.
 *
 * Runs once per document (the viewer iframe reloads fully per file).
 */
(function () {
    if (window.__mdeNarrowColsLoaded) return;
    window.__mdeNarrowColsLoaded = true;

    // Numeric = whole text is digits plus common separators (comma, dot, %, /,
    // :, +, -, whitespace). Any letter makes it non-numeric.
    var NUMERIC = /^[\s\d.,%/:+\-]+$/;
    var SIGLA_MAX = 10;  // a "sigla" is at most this many characters
    var THRESHOLD = 0.8; // share of non-empty body cells that must qualify

    function markTable(table) {
        var head = table.tHead;
        var body = table.tBodies && table.tBodies[0];
        if (!head || !head.rows.length || !body || !body.rows.length) return;

        var headRows = head.rows, bodyRows = body.rows;
        var colCount = headRows[0].cells.length;

        for (var c = 0; c < colCount; c++) {
            var total = 0, numeric = 0, sigla = 0;
            for (var r = 0; r < bodyRows.length; r++) {
                var cell = bodyRows[r].cells[c];
                if (!cell) continue;
                var text = (cell.textContent || '').trim();
                if (text === '') continue; // ignore empty cells
                total++;
                if (NUMERIC.test(text)) numeric++;
                if (text.length <= SIGLA_MAX) sigla++;
            }
            if (total === 0) continue;

            var isNumeric = numeric / total >= THRESHOLD;
            var isNarrow = isNumeric || (sigla / total >= THRESHOLD);
            if (!isNarrow) continue;

            for (var hr = 0; hr < headRows.length; hr++) {
                var hc = headRows[hr].cells[c];
                if (!hc) continue;
                hc.classList.add('mde-narrowcol');
                if (isNumeric) hc.classList.add('mde-numcol');
            }
            for (var br = 0; br < bodyRows.length; br++) {
                var bc = bodyRows[br].cells[c];
                if (!bc) continue;
                bc.classList.add('mde-narrowcol');
                if (isNumeric) bc.classList.add('mde-numcol');
            }
        }
    }

    function run() {
        document.querySelectorAll('.mdeItemMainPageCenter table.table').forEach(markTable);
    }

    // Public hook (e.g. if content is injected dynamically).
    window.MdeNarrowCols = { run: run };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', run);
    } else {
        run();
    }
})();
