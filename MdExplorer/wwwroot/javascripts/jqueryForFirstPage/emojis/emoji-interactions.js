/**
 * MdExplorer - Emoji Interactions
 * =================================
 * Handles click interactions for dynamic process and priority emojis
 *
 * Features:
 * - Process emoji cycle: ℹ️ → 🆗 → ⚠️ → 🚧 → ✔️ → ℹ️
 * - Priority emoji cycle: ❓ → ❔ → ❕ → ❗ → ❌ → ⛔ → ❎ → ❓
 * - Saves emoji state to markdown file via backend API
 * - Updates Tippy tooltips after state change
 *
 * Global dependencies:
 * - window.tippyDictProcess (from emoji-tooltips.js)
 * - window.tippyDictPriority (from emoji-tooltips.js)
 * - setTippyTypeProcess() (from emoji-tooltips.js)
 * - setTippyTypePriority() (from emoji-tooltips.js)
 * - tocbot.refresh() (from TocBot library)
 *
 * Backend API:
 * - GET /api/WriteMD/SetEmojiProcess
 * - GET /api/WriteMD/SetEmojiPriority
 *
 * IMPORTANT: These functions MUST be global (not wrapped in document.ready)
 * because they are called directly from onclick attributes in backend-generated HTML
 */

/**
 * Handle click on process emoji - cycles through process states
 * Called from onclick attribute: onclick="dynamicEmojiForProcess(this, 0, '/path/file.md')"
 *
 * @param {HTMLElement} el - The emoji span element that was clicked
 * @param {number} index - The index of this emoji in the document
 * @param {string} pathfile - The absolute path to the markdown file
 */
window.dynamicEmojiForProcess = function(el, index, pathfile) {
    console.log('🔍 [EMOJI-INTERACTIONS] dynamicEmojiForProcess called', { el, index, pathfile });
    console.log('🔍 [EMOJI-INTERACTIONS] Current emoji text:', el.innerText.trim());

    let dataToSet;
    el.removeAttribute('data-tippy-content');

    if (el.innerText.trim() == 'ℹ️') {
        el.innerText = '🆗';
        el.setAttribute('data-tippy-content', 'approvato');
        dataToSet = 'approvato';
    } else if (el.innerText.trim() == '🆗') {
        el.innerText = '⚠️';
        el.setAttribute('data-tippy-content', 'attenzione');
        dataToSet = 'attenzione';
    } else if (el.innerText.trim() == '⚠️') {
        el.innerText = '🚧';
        el.setAttribute('data-tippy-content', 'work in progress');
        dataToSet = 'work in progress';
    } else if (el.innerText.trim() == '🚧') {
        el.innerText = '✔️';
        el.setAttribute('data-tippy-content', 'completato');
        dataToSet = 'completato';
    } else if (el.innerText.trim() == '✔️') {
        el.innerText = 'ℹ️';
        el.setAttribute('data-tippy-content', 'in valutazione');
        dataToSet = 'Info';
    }

    var currentIndex = el.attributes['data-md-process-index'].value;
    var connectionId = $('body').attr('connectionid') || '';
    $.get("/api/WriteMD/SetEmojiProcess?index=" + currentIndex + "&pathFile=" + pathfile + "&toReplace=" + el.innerText.trim() + "&ConnectionId=" + connectionId, function (data) {
        $(".result").html(data);
        var oldData$ = $('div.hiddendataforeditorh1');
        for (i = 0; i < oldData$.length; i++) {
            let myData$ = $(oldData$.get(i));
            let check = myData$.attr("md-itemmatchindex");
            myData$.attr("md-itemmatchindex", data[i].itemMatchIndex);
        }
    });

    setTooltipProcess(dataToSet, el);
    tocbot.refresh();
}

/**
 * Update Tippy tooltip for process emoji after state change
 *
 * @param {string} dataToSet - The new tooltip text
 * @param {HTMLElement} el - The emoji element
 */
window.setTooltipProcess = function(dataToSet, el) {
    // Try to get tippy instance directly from element (Tippy.js stores it on _tippy property)
    let current = el._tippy;

    // Fallback: try to find by index in array
    if (!current) {
        let $el = $(el);
        let attributeValue = $el.attr("data-tippy-process-id");
        current = window.tippyDictProcess && window.tippyDictProcess[attributeValue];
    }

    if (current) {
        current.setContent(dataToSet);
        current.reference.setAttribute('data-tippy-content', dataToSet);
        setTippyTypeProcess(current.reference, current);
        current.show();
    } else {
        // Last resort: update attribute and try to reinitialize tippy on this element
        el.setAttribute('data-tippy-content', dataToSet);
        if (typeof tippy !== 'undefined') {
            let newTippy = tippy(el, { placement: 'left' });
            newTippy.setContent(dataToSet);
            setTippyTypeProcess(el, newTippy);
            newTippy.show();
        }
    }
}

/**
 * Handle click on priority emoji - cycles through priority states
 * Called from onclick attribute: onclick="dynamicEmojiForPriority(this, 0, '/path/file.md')"
 *
 * @param {HTMLElement} el - The emoji span element that was clicked
 * @param {number} index - The index of this emoji in the document
 * @param {string} pathfile - The absolute path to the markdown file
 */
window.dynamicEmojiForPriority = function(el, index, pathfile) {
    let dataToSet;

    if (el.innerText.trim() == '❓') {
        el.innerText = '❔';
        dataToSet = 'da valutare';
    } else if (el.innerText.trim() == '❔') {
        el.innerText = '❕';
        dataToSet = 'obbligatorio';
    } else if (el.innerText.trim() == '❕') {
        el.innerText = '❗';
        dataToSet = 'urgente';
    } else if (el.innerText.trim() == '❗') {
        el.innerText = '❌';
        dataToSet = 'annullata';
    } else if (el.innerText.trim() == '❌') {
        el.innerText = '⛔';
        dataToSet = 'fermata';
        var element = $('#' + el.id).parent();
    } else if (el.innerText.trim() == '⛔') {
        el.innerText = '❎';
        dataToSet = 'conclusa';
        var element = $('#' + el.id).parent();
        var check = element.parent().is('li');
        if (check) {
            element = element.parent();
        }
    } else if (el.innerText.trim() == '❎') {
        el.innerText = '❓';
        dataToSet = 'dubbio urgente';
        var element = $('#' + el.id).parent();
        var check = element.is('li');
        if (!check) {
            element = element.parent();
        }
        element.stop();
        element.fadeIn();
    }

    var currentIndex = el.attributes['data-md-priority-index'].value;
    var connectionId = $('body').attr('connectionid') || '';
    $.get("/api/WriteMD/SetEmojiPriority?index=" + currentIndex + "&pathFile=" + pathfile + "&toReplace=" + el.innerText.trim() + "&ConnectionId=" + connectionId, function (data) {
        $(".result").html(data);

        var oldData$ = $('div.hiddendataforeditorh1');
        for (i = 0; i < oldData$.length; i++) {
            let myData$ = $(oldData$.get(i));
            let check = myData$.attr("md-itemmatchindex");
            myData$.attr("md-itemmatchindex", data[i].itemMatchIndex);
        }
    });

    setTooltipPriority(dataToSet, el);
    tocbot.refresh();
}

/**
 * Update Tippy tooltip for priority emoji after state change
 *
 * @param {string} dataToSet - The new tooltip text
 * @param {HTMLElement} el - The emoji element
 */
window.setTooltipPriority = function(dataToSet, el) {
    // Try to get tippy instance directly from element (Tippy.js stores it on _tippy property)
    let currentPriority = el._tippy;

    // Fallback: try to find by index in array
    if (!currentPriority) {
        let $el = $(el);
        let attributeValue = $el.attr("data-tippy-priority-id");
        currentPriority = window.tippyDictPriority && window.tippyDictPriority[attributeValue];
    }

    if (currentPriority) {
        currentPriority.setContent(dataToSet);
        currentPriority.reference.setAttribute('data-tippy-content', dataToSet);
        setTippyTypePriority(currentPriority.reference, currentPriority);
        currentPriority.show();
    } else {
        // Last resort: update attribute and try to reinitialize tippy on this element
        el.setAttribute('data-tippy-content', dataToSet);
        if (typeof tippy !== 'undefined') {
            let newTippy = tippy(el, { placement: 'left' });
            newTippy.setContent(dataToSet);
            setTippyTypePriority(el, newTippy);
            newTippy.show();
        }
    }
}

// Verification logs - MUST be at end of file after all function definitions
console.log('🔍 [EMOJI-INTERACTIONS] Script loaded');
console.log('🔍 [EMOJI-INTERACTIONS] typeof window.dynamicEmojiForProcess:', typeof window.dynamicEmojiForProcess);
console.log('🔍 [EMOJI-INTERACTIONS] typeof window.dynamicEmojiForPriority:', typeof window.dynamicEmojiForPriority);
console.log('🔍 [EMOJI-INTERACTIONS] typeof window.setTooltipProcess:', typeof window.setTooltipProcess);
console.log('🔍 [EMOJI-INTERACTIONS] typeof window.setTooltipPriority:', typeof window.setTooltipPriority);
