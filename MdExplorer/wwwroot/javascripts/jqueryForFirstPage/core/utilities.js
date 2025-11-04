/**
 * MdExplorer - Utility Functions
 * ===============================
 * Helper functions used across multiple modules
 */

/**
 * Hash function (MurmurHash3 variant) for generating URL keys
 * Used primarily for localStorage keys in scroll position management
 *
 * @param {string} str - String to hash
 * @param {number} seed - Optional seed value (default: 0)
 * @returns {number} 53-bit hash
 */
const cyrb53 = function (str, seed = 0) {
    let h1 = 0xdeadbeef ^ seed, h2 = 0x41c6ce57 ^ seed;
    for (let i = 0, ch; i < str.length; i++) {
        ch = str.charCodeAt(i);
        h1 = Math.imul(h1 ^ ch, 2654435761);
        h2 = Math.imul(h2 ^ ch, 1597334677);
    }
    h1 = Math.imul(h1 ^ (h1 >>> 16), 2246822507) ^ Math.imul(h2 ^ (h2 >>> 13), 3266489909);
    h2 = Math.imul(h2 ^ (h2 >>> 16), 2246822507) ^ Math.imul(h1 ^ (h1 >>> 13), 3266489909);
    return 4294967296 * (2097151 & h2) + (h1 >>> 0);
};

/**
 * Create a snapshot of the current file with a custom name
 * Opens a dialog to input the snapshot version name
 *
 * @param {number} index - Index of the camera flash icon clicked
 */
function createSnapshot(index) {
    let $camera_flash_Id = $('span[md-camera_flash_id=' + index + ']');
    let fullpath = $camera_flash_Id.attr('md-fullpath');

    let $dialog = $('<div></div>')
        .html('<p>Snapshot name:</p><input type="text" id="myTextbox" style="width: 100%;" />')
        .dialog({
            title: "Take a picture!",
            autoOpen: false,
            modal: true,
            buttons: {
                Ok: function () {
                    var textboxValue = $('#myTextbox').val();
                    $.post("/api/MdFiles/CreateSnapshot", { fullPath: fullpath, versioningdesc: textboxValue },
                        function (data) {
                        });
                    $(this).dialog("close");
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });

    // Open the dialog
    $dialog.dialog('open');
}

/**
 * Open a file in an external application
 *
 * @param {string} fullpath - Full path of the file to open
 */
function openApplication(fullpath) {
    let $body = $("#MdBody");
    let toStringify = { fullPath: fullpath, connectionId: $body.attr("connectionid") };
    $.ajax({
        url: "/api/MdFiles/OpenFileInApplication",
        type: "POST",
        data: JSON.stringify(toStringify),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            console.log(data);
        }
    });
}

/**
 * Activate save copy mode (functionality to be implemented)
 *
 * @param {HTMLElement} el - Element triggering the save
 * @param {string} path - Path to save to
 */
function activateSaveCopy(el, path) {
    // Implementation placeholder
    console.log('Save copy activated for:', path);
}
