/**
 * MdExplorer - Numeric column sizing for markdown tables
 * ======================================================
 * The base rule gives every column `min-width: 40ch` so wide tables keep a
 * horizontal scrollbar inside their box. But columns whose data cells are just
 * numbers (e.g. "Progressivo", "Modalità", "Versione", "Data") don't need 40ch
 * — at that width they'd be mostly empty.
 *
 * This module detects numeric columns and tags their cells with `.mde-numcol`,
 * which the CSS then renders narrow (min-width:0) with the long header text
 * wrapped onto multiple lines. Pure classification: all sizing lives in CSS.
 *
 * Runs once per document (the viewer iframe reloads fully per file).
 */
(function () {
    if (window.__mdeNumColLoaded) return;
    window.__mdeNumColLoaded = true;

    // A cell is "numeric" when its whole text is digits plus common numeric
    // separators (comma, dot, %, slash, colon, +, -, whitespace). Any letter
    // (descriptions, identifiers) makes it non-numeric.
    var NUMERIC = /^[\s\d.,%/:+\-]+$/;
    var THRESHOLD = 0.8; // share of non-empty body cells that must be numeric

    function markTable(table) {
        var head = table.tHead;
        var body = table.tBodies && table.tBodies[0];
        if (!head || !head.rows.length || !body || !body.rows.length) return;

        var headRows = head.rows, bodyRows = body.rows;
        var colCount = headRows[0].cells.length;

        for (var c = 0; c < colCount; c++) {
            var total = 0, numeric = 0;
            for (var r = 0; r < bodyRows.length; r++) {
                var cell = bodyRows[r].cells[c];
                if (!cell) continue;
                var text = (cell.textContent || '').trim();
                if (text === '') continue; // ignore empty cells
                total++;
                if (NUMERIC.test(text)) numeric++;
            }
            if (total === 0 || numeric / total < THRESHOLD) continue;

            for (var hr = 0; hr < headRows.length; hr++) {
                if (headRows[hr].cells[c]) headRows[hr].cells[c].classList.add('mde-numcol');
            }
            for (var br = 0; br < bodyRows.length; br++) {
                if (bodyRows[br].cells[c]) bodyRows[br].cells[c].classList.add('mde-numcol');
            }
        }
    }

    function run() {
        document.querySelectorAll('.mdeItemMainPageCenter table.table').forEach(markTable);
    }

    // Public hook (e.g. if content is injected dynamically).
    window.MdeNumericCols = { run: run };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', run);
    } else {
        run();
    }
})();
