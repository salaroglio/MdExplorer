/**
 * MdExplorer - Image Transform (Move & Resize)
 * ==============================================
 * Enables drag-to-move and resize functionality for images
 *
 * Features:
 * - Click-and-drag to reposition images
 * - Toggle resize mode to make images resizable
 * - Saves position and size to markdown file via API
 * - Toolbar show/hide on hover
 *
 * Global dependencies:
 * - window.arrayLinksMoveToggle (from globals.js)
 * - window.arrayLinksResizeToggle (from globals.js)
 * - window.moving (from globals.js)
 * - window.image (from globals.js)
 *
 * Backend API:
 * - POST /api/WriteMD/SaveImgPositionAndSize
 */

/**
 * Show image toolbar on hover
 * Positions toolbar absolutely relative to image
 *
 * @param {string} referenceId - ID of toolbar element
 */
function showImageToolbar(referenceId) {
    var $element = $('#' + referenceId);
    var divStyle = getComputedStyle($element[0]);
    var rect = $element[0].getBoundingClientRect();
    var test = rect.top;
    $element.attr("style", "display:block; position:absolute;");
}

/**
 * Hide image toolbar
 *
 * @param {string} referenceId - ID of toolbar element
 */
function hideImageToolbar(referenceId) {
    var $element = $('#' + referenceId);
    $element.attr("style", "display:none;");
}

/**
 * Toggle move mode for image
 * First click: Enable drag-to-move (class='movable')
 * Second click: Fix position and save to backend (class='movedAndFixed')
 *
 * @param {HTMLElement} currentObject - Button element that triggered the action
 * @param {string} linkHash - Unique hash identifier for the image link
 * @param {string} referenceId - ID of the image reference
 */
function activateMove(currentObject, linkHash, referenceId) {
    var toSend = currentObject.parentElement.parentElement;
    $movable = $(toSend);
    var buttonPressed = window.arrayLinksMoveToggle.find(data => data == linkHash);
    if (buttonPressed == undefined) {
        var newClass = $movable.attr('class', 'movable');
        window.arrayLinksMoveToggle.push(linkHash);
    } else {

        var currentIndex = window.arrayLinksMoveToggle.findIndex(data => data == linkHash);
        window.arrayLinksMoveToggle.splice(currentIndex, 1);
        var possibleMatch = currentObject.parentElement.nextSibling;
        resizeImage(possibleMatch);
        $movable.attr('class', 'movedAndFixed');

    }

    initialClick(toSend, referenceId);
}

/**
 * Initialize click handler for move functionality
 * Toggles between starting and stopping move mode
 *
 * @param {HTMLElement} currentObject - Element to make movable
 * @param {string} referenceId - ID of the image reference
 */
function initialClick(currentObject, referenceId) {

    if (window.moving) {
        document.removeEventListener("mousemove", move);
        window.moving = !window.moving;
        return;
    }

    window.moving = !window.moving;
    window.image = currentObject;

    document.addEventListener("mousemove", move, false);

}

/**
 * Handle mouse movement during drag
 * Updates element position to follow cursor
 * Applies offset correction: -76px horizontal, -18px vertical
 *
 * @param {MouseEvent} e - Mouse event with clientX/clientY
 */
function move(e) {

    var newX = e.clientX - 76;
    var newY = e.clientY - 18;

    window.image.style.left = newX + "px";
    window.image.style.top = newY + "px";


}

/**
 * Toggle resize mode for image
 * First click: Add 'resizable' class to enable resizing
 * Second click: Remove 'resizable' class to fix size
 *
 * @param {string} linkHash - Unique hash identifier for the image link
 */
function activateResize(linkHash) {
    // Find nodes
    var buttonPressed = window.arrayLinksResizeToggle.find(data => data == linkHash);
    var $links = $('div[md-link-hash=' + linkHash + ']'); // shold exist only one link. I'm using each() because i'm lazy :-)
    $links.each(function (index) {
        if (buttonPressed == undefined) {
            var oldValue = $links[index].attributes['class'].value;
            $links[index].attributes['class'].value = oldValue + ' resizable';
            window.arrayLinksResizeToggle.push(linkHash);
        }
        else {


            var oldValue = $links[index].attributes['class'].value;
            $links[index].attributes['class'].value = oldValue.replace(' resizable', '');
            var currentIndex = window.arrayLinksResizeToggle.findIndex(data => data == linkHash);
            window.arrayLinksResizeToggle.splice(currentIndex, 1);

        }

    });
}

/**
 * Calculate cumulative offset of element relative to document
 * Recursively sums offsetTop and offsetLeft up the parent chain
 *
 * @param {HTMLElement} element - Element to calculate offset for
 * @returns {{top: number, left: number}} Cumulative offset
 */
var cumulativeOffset = function (element) {
    var top = 0, left = 0;
    do {
        top += element.offsetTop || 0;
        left += element.offsetLeft || 0;
        element = element.offsetParent;
    } while (element);

    return {
        top: top,
        left: left
    };
};

/**
 * Save image position and size to markdown file
 * Called on mouseUp event after move or resize
 * Extracts image dimensions and position, sends to backend API
 *
 * @param {HTMLElement} currentDiv - Div containing the image
 */
function resizeImage(currentDiv) {

    // going inside the div
    var img = currentDiv.childNodes[0].childNodes[0];
    var divStyle = getComputedStyle(img.parentElement.parentElement.parentElement);
    var position = divStyle.position;// == "" ? "none" : img.style.position;
    // getting infos from attributes

    var currentHash = currentDiv.attributes['md-css-hash'].value;
    var pathFile = currentDiv.attributes['md-path-file'].value;
    var linkHash = currentDiv.attributes['md-link-hash'].value;
    var CurrentQueryRequest = currentDiv.attributes['md-CurrentQueryRequest'].value;

    var currentImageData = {
        pathFile: pathFile,
        linkHash: linkHash,
        cssHash: currentHash,
        Width: currentDiv.clientWidth,
        Height: currentDiv.scrollHeight,
        ClientX: cumulativeOffset(currentDiv).left,
        ClientY: cumulativeOffset(currentDiv).top,
        Position: position,
        CurrentQueryRequest: CurrentQueryRequest
    };
    $.ajax({
        url: "/api/WriteMD/SaveImgPositionAndSize",
        type: "POST",
        data: JSON.stringify(currentImageData),//'{"linkHash": "1234", "cssHash": "5678", "Width": "100px", "Height": "50px","ClientX":"","ClientY":"" }', //
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            currentDiv.attributes['md-css-hash'].value = data.cssHash;
            var $divs = $("div[md-css-hash='" + currentHash + "']");
            $divs.each(function (index) {
                $divs[index].attributes['md-css-hash'].value = data.cssHash;
            });
            //.attributes['md-css-hash'].value = data.cssHash
        }
    });
}
