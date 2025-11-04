/**
 * MdExplorer - Emoji Sortable (Drag & Drop Priority)
 * ===================================================
 * Enables drag-and-drop reordering of priority emojis
 *
 * Features:
 * - jQuery UI sortable for .sortable containers
 * - Saves new order to backend via API
 * - Reconstructs data-md-*-index attributes after sort
 * - Supports table-game grouping (multiple priority lists)
 * - Refreshes TOC after reordering
 *
 * Dependencies:
 * - jQuery UI sortable
 * - tocbot (for TOC refresh)
 *
 * Backend API:
 * - GET /api/WriteMD/SetEmojiOrderPriority
 *
 * Data Attributes Used:
 * - data-md-card-index: Position within table-game
 * - data-md-table-game-index: Which table-game group
 * - data-md-pathfile: Path to markdown file
 * - data-md-process-index: Global process emoji index
 * - data-md-priority-index: Global priority emoji index
 */

/**
 * Initialize sortable functionality for priority emojis
 * Sets up drag-and-drop with backend persistence
 */
$(function () {
    $(".sortable").sortable();
    $(".sortable").disableSelection();

    $(".sortable").on("sortstop", function (event, ui) {
        // controllo che l'item selezionato abbia il simbolo di priorità
        var previousNodeIndex = null;
        var nextNodeIndex = null;
        var currentNodeIndex = null;

        // nodo corrente
        currentNode = ui.item[0].childNodes[0];
        currentNodeIndex = currentNode.attributes['data-md-card-index'].value;
        currentTableGameIndex = currentNode.attributes['data-md-table-game-index'].value;

        // nodo precedente
        if (ui.item[0].previousElementSibling != null) {
            previousNode = ui.item[0].previousElementSibling.childNodes[0];
            previousNodeIndex = previousNode.attributes['data-md-card-index'].value;
        }
        // nodo successivo
        if (ui.item[0].nextElementSibling != null) {
            nextNode = ui.item[0].nextSibling.childNodes[0];
            nextNodeIndex = nextNode.attributes['data-md-card-index'].value;
        }

        var currentPathFile = ui.item[0].childNodes[0].attributes['data-md-pathfile'].value;
        var queryPreviousNodeIndex = previousNodeIndex == null ? '' : previousNodeIndex;
        var queryNextNodeIndex = nextNodeIndex == null ? '' : nextNodeIndex;
        $.get("/api/WriteMD/SetEmojiOrderPriority?currentNodeIndex=" + currentNodeIndex +
            "&previousNodeIndex=" + queryPreviousNodeIndex +
            "&nextNodeIndex=" + queryNextNodeIndex +
            "&pathFile=" + currentPathFile +
            "&tableGameIndex=" + currentTableGameIndex,
            function (data) {
                currentNode.attributes['data-md-card-index'].value = data.currentNodeIndex;
                if (data.previousNodeIndex != null) {
                    previousNode.attributes['data-md-card-index'].value = data.previousNodeIndex;
                }
                if (data.nextNodeIndex != null) {
                    nextNode.attributes['data-md-card-index'].value = data.nextNodeIndex;
                }
                // reconstruct process IDs
                var listProcess = $("span[id*='emojiProcess']");
                listProcess.each(function (index, element) {
                    element.attributes['data-md-process-index'].value = index;// listProcess.index(element);
                });
                var listPriority = $("span[id*='mojiPriority']");
                listPriority.each(function (index, element) {
                    element.attributes['data-md-priority-index'].value = index;// listProcess.index(element);
                });
                // Reconstruct index of table-card into table-game, foreach table-game
                var iTable = 0;
                var listPriorityItableGame = $("span[id*='mojiPriority'][data-md-table-game-index=" + iTable.toString() + "]");
                while (listPriorityItableGame.length > 0) {
                    listPriorityItableGame.each(function (index, element) {
                        element.attributes['data-md-card-index'].value = index;// listProcess.index(element);
                    });
                    iTable++;
                    var listPriorityItableGame = $("span[id*='mojiPriority'][data-md-table-game-index=" + iTable.toString() + "]");
                }




                console.log(data);
            });
        console.log('sortstop parents Event = ', event, '  ui = ', ui);
        console.log(ui.item);
        tocbot.refresh();
        //do sort of parents
    });
});
