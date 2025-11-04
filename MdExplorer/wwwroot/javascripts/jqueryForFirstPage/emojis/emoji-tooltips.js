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
$(function () {

    window.tippyDictPriority = tippy('[id*="Priority"][data-tippy-content]', {
        placement: 'left',
    });
    window.tippyDictProcess = tippy('[id*="Process"][data-tippy-content]', {
        placement: 'left'
    });

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
});

/**
 * Set theme for priority emoji tooltips
 * Maps Italian priority labels to CSS theme classes
 *
 * @param {HTMLElement} tippyReference - Reference element with data-tippy-content
 * @param {TippyInstance} _ - Tippy instance to configure
 */
function setTippyTypePriority(tippyReference, _) {
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
function setTippyTypeProcess(tippyReference, _) {
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
