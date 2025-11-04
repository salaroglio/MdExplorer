/**
 * MdExplorer - Calendar Picker Configuration
 * ===========================================
 * Resolves conflicts between Bootstrap and jQuery UI datepicker
 *
 * Purpose:
 * Allows Bootstrap datepicker to work without interference from jQuery UI's datepicker
 * by implementing a no-conflict mode
 *
 * Dependencies:
 * - jQuery
 * - Bootstrap datepicker
 * - jQuery UI (conflicting datepicker)
 */

/**
 * Enable no-conflict mode for Bootstrap datepicker
 * Prevents jQuery UI datepicker from interfering
 * Returns Bootstrap datepicker for chaining
 */
$.fn.datepicker.noConflict = function () {
    $.fn.datepicker = old;
    return this;
};
