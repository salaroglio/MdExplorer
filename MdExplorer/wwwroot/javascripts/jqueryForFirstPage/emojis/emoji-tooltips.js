/**
 * MdExplorer - Emoji Tooltips (Tippy.js Integration)
 * ==================================================
 * Manages tooltips for priority and process emojis
 *
 * Features:
 * - Initializes Tippy.js tooltips for emoji elements
 * - Applies themed styles based on tooltip content
 * - Two categories: Priority and Process
 *
 * Global dependencies:
 * - window.tippyDictPriority (from globals.js)
 * - window.tippyDictProcess (from globals.js)
 * - tippy (Tippy.js library)
 *
 * DOM Selectors:
 * - [id*="Priority"][data-tippy-content]: Priority emojis
 * - [id*="Process"][data-tippy-content]: Process emojis
 *
 * Theme Mappings:
 * Priority: urgente, annullata, fermata, conclusa, dubbio urgente, da valutare, obbligatorio
 * Process: approvato, work in progress, completato, Info, attenzione
 */

/**
 * Initialize Tippy tooltips for all emoji elements
 * Sets up tooltips and assigns custom themes based on content
 */
window.initializeEmojiTooltips = function() {
    console.log('🔍 [EMOJI-TOOLTIPS] Initializing tooltips...');
    console.log('🔍 [EMOJI-TOOLTIPS] typeof tippy:', typeof tippy);
    console.log('🔍 [EMOJI-TOOLTIPS] typeof $:', typeof $);

    const priorityElements = document.querySelectorAll('[id*="Priority"][data-tippy-content]');
    const processElements = document.querySelectorAll('[id*="Process"][data-tippy-content]');
    console.log('🔍 [EMOJI-TOOLTIPS] Found priority elements:', priorityElements.length);
    console.log('🔍 [EMOJI-TOOLTIPS] Found process elements:', processElements.length);

    if (typeof tippy === 'undefined') {
        console.error('❌ [EMOJI-TOOLTIPS] CRITICAL: tippy is not defined! Tooltips will not work.');
        return;
    }

    window.tippyDictPriority = tippy('[id*="Priority"][data-tippy-content]', {
        placement: 'left',
    });
    window.tippyDictProcess = tippy('[id*="Process"][data-tippy-content]', {
        placement: 'left'
    });

    console.log('🔍 [EMOJI-TOOLTIPS] tippyDictPriority instances:', window.tippyDictPriority ? window.tippyDictPriority.length : 0);
    console.log('🔍 [EMOJI-TOOLTIPS] tippyDictProcess instances:', window.tippyDictProcess ? window.tippyDictProcess.length : 0);

    window.tippyDictProcess.forEach(_ => {
        let tippyReferenceProcess = _.reference;
        tippyReferenceProcess.setAttribute('data-tippy-process-id', window.tippyDictProcess.indexOf(_));
        setTippyTypeProcess(tippyReferenceProcess, _);
    });

    window.tippyDictPriority.forEach(_ => {
        let tippyReferencePriority = _.reference;
        tippyReferencePriority.setAttribute('data-tippy-priority-id', window.tippyDictPriority.indexOf(_));
        setTippyTypePriority(tippyReferencePriority, _);
    });

    console.log('✅ [EMOJI-TOOLTIPS] Initialization complete');
};

// Call initialization multiple times to catch dynamic content loading
$(function () {
    console.log('🔍 [EMOJI-TOOLTIPS] Document ready - attempting initialization');
    window.initializeEmojiTooltips();

    // Retry after a delay to catch dynamically loaded content
    setTimeout(function() {
        console.log('🔍 [EMOJI-TOOLTIPS] Delayed initialization (500ms) - attempting');
        window.initializeEmojiTooltips();
    }, 500);

    setTimeout(function() {
        console.log('🔍 [EMOJI-TOOLTIPS] Delayed initialization (1000ms) - attempting');
        window.initializeEmojiTooltips();
    }, 1000);

    setTimeout(function() {
        console.log('🔍 [EMOJI-TOOLTIPS] Delayed initialization (2000ms) - final attempt');
        window.initializeEmojiTooltips();
    }, 2000);
});

/**
 * Set theme for priority emoji tooltips
 * Maps Italian priority labels to CSS theme classes
 *
 * @param {HTMLElement} tippyReference - Reference element with data-tippy-content
 * @param {TippyInstance} _ - Tippy instance to configure
 */
window.setTippyTypePriority = function(tippyReference, _) {
    if (tippyReference.getAttribute('data-tippy-content') == 'urgente') {
        _.setProps({ theme: 'priorityUrgente' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'annullata') {
        _.setProps({ theme: 'priorityAnnullato' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'fermata') {
        _.setProps({ theme: 'priorityFermata' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'conclusa') {
        _.setProps({ theme: 'priorityConclusa' });
    }

    if (tippyReference.getAttribute('data-tippy-content') == 'dubbio urgente') {
        _.setProps({ theme: 'priorityDaCapireUrgentemente' });
    }

    if (tippyReference.getAttribute('data-tippy-content') == 'da valutare') {
        _.setProps({ theme: 'priorityDaValutare' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'obbligatorio') {
        _.setProps({ theme: 'priorityObbligatorio' });
    }



}

/**
 * Set theme for process emoji tooltips
 * Maps Italian process labels to CSS theme classes
 *
 * @param {HTMLElement} tippyReference - Reference element with data-tippy-content
 * @param {TippyInstance} _ - Tippy instance to configure
 */
window.setTippyTypeProcess = function(tippyReference, _) {
    if (tippyReference.getAttribute('data-tippy-content') == 'approvato') {
        _.setProps({ theme: 'processok' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'work in progress') {
        _.setProps({ theme: 'processWIP' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'completato') {
        _.setProps({ theme: 'processCompletato' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'Info') {
        _.setProps({ theme: 'processInfo' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'attenzione') {
        _.setProps({ theme: 'processAttenzione' });
    }


}
