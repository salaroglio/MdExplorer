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

// Scroll listeners keyed by referenceId, to clean up on hide
var _toolbarScrollListeners = {};

/**
 * Toggle SVG light mode: removes/restores the CSS invert filter on the SVG
 * sibling of the toolbar, so the user can see original colors in dark mode.
 */
function toggleSvgLightMode(btnElement) {
    var $toolbar = $(btnElement).closest('[id]');
    var $container = $toolbar.next();
    var $svg = $container.find('svg').first();
    if (!$svg.length) $svg = $container.filter('svg');
    if (!$svg.length) return;

    var $icon = $(btnElement).find('.svg-light-toggle-icon');
    var current = $svg.css('filter');
    if (current && current !== 'none') {
        // Turn ON the light: remove filter, light mode (yellow bulb)
        $svg.data('original-filter', current);
        $svg.css('filter', 'none');
        $icon.css({
            'filter': 'none',
            'opacity': '1'
        });
        $(btnElement).attr('title', 'Turn off the light (back to dark mode)');
    } else {
        // Turn OFF the light: restore filter, dark mode (faded/grayscale bulb)
        var original = $svg.data('original-filter') || 'invert(0.88) hue-rotate(180deg)';
        $svg.css('filter', original);
        $icon.css({
            'filter': 'grayscale(1) brightness(0.6)',
            'opacity': '0.5'
        });
        $(btnElement).attr('title', 'Turn on the light (view in light mode)');
    }
}

// Pending hide timers keyed by referenceId (allows mouseenter on search box to cancel)
var _hideToolbarTimers = {};

// Idle auto-hide: timers, mousemove listeners, and hidden-state flags keyed by referenceId
var _idleHideTimers = {};
var _idleMouseMoveListeners = {};
var _toolbarIdleHidden = {};

/**
 * Recalculate and apply position:fixed coordinates for the toolbar,
 * clamping to the container's visible top-left corner in the viewport.
 *
 * @param {string} referenceId - ID of toolbar element
 */
function _updateToolbarPosition(referenceId) {
    var $toolbar = $('#' + referenceId);
    if (!$toolbar.length || $toolbar.css('display') === 'none') return;

    var rect = $toolbar.parent()[0].getBoundingClientRect();
    var top = Math.max(0, rect.top) + 20;
    var left = Math.max(0, rect.left);

    $toolbar.css({ top: top + 'px', left: left + 'px' });
}

/**
 * Start (or restart) the idle auto-hide timer.
 * After 2 seconds with no mouse movement the toolbar hides automatically.
 * Skipped when the SVG search box is open.
 *
 * @param {string} referenceId - ID of toolbar element
 */
function _startIdleHideTimer(referenceId) {
    if (_idleHideTimers[referenceId]) {
        clearTimeout(_idleHideTimers[referenceId]);
    }

    _idleHideTimers[referenceId] = setTimeout(function () {
        // Don't idle-hide if search box is open for this toolbar's image
        if (typeof _toolbarToHashMap !== 'undefined' && _toolbarToHashMap[referenceId]) {
            var hash = _toolbarToHashMap[referenceId];
            if (window.svgSearchActive && window.svgSearchActive[hash]) {
                return;
            }
        }

        var $toolbar = $('#' + referenceId);
        $toolbar.attr("style", "display:none;");
        _toolbarIdleHidden[referenceId] = true;
    }, 2000);
}

/**
 * Attach a mousemove listener to the toolbar's parent container.
 * On movement: re-show the toolbar if idle-hidden, or reset the idle timer.
 *
 * @param {string} referenceId - ID of toolbar element
 */
function _attachIdleMouseMoveListener(referenceId) {
    if (_idleMouseMoveListeners[referenceId]) return;

    var container = $('#' + referenceId).parent()[0];
    if (!container) return;

    var handler = function () {
        if (_toolbarIdleHidden[referenceId]) {
            _toolbarIdleHidden[referenceId] = false;
            showImageToolbar(referenceId);
        } else {
            _startIdleHideTimer(referenceId);
        }
    };

    _idleMouseMoveListeners[referenceId] = { container: container, handler: handler };
    container.addEventListener('mousemove', handler, false);
}

/**
 * Show image toolbar on hover.
 * Uses position:fixed so the buttons stay visible at the top-left of the
 * container's visible area even when the image is larger than the viewport
 * and the user scrolls or pans horizontally.
 *
 * @param {string} referenceId - ID of toolbar element
 */
function showImageToolbar(referenceId) {
    // Cancel any pending hide so moving to the search box doesn't cause a flicker
    if (_hideToolbarTimers[referenceId]) {
        clearTimeout(_hideToolbarTimers[referenceId]);
        delete _hideToolbarTimers[referenceId];
    }

    // Clear idle-hidden state if re-showing
    _toolbarIdleHidden[referenceId] = false;

    var $element = $('#' + referenceId);
    var rect = $element.parent()[0].getBoundingClientRect();
    var top = Math.max(0, rect.top) + 20;
    var left = Math.max(0, rect.left);

    $element.attr("style",
        "display:block; position:fixed; top:" + top + "px; left:" + left + "px; z-index:100;");

    // Dark mode: inject light-mode toggle button for SVG diagrams
    if (document.body.classList.contains('dark-theme') && !$element.data('light-toggle-added')) {
        var $sibling = $element.next();
        var hasSvg = $sibling.find('svg').length > 0 || $sibling.find('.svg-zoom-viewport').length > 0;
        if (hasSvg) {
            // Reflect current filter state so the button starts consistent with the
            // project setting PlantUmlKeepOriginalColorsInDarkMode (body.plantuml-keep-original).
            var $svg = $sibling.find('svg').first();
            if (!$svg.length) $svg = $sibling.filter('svg');
            var filterActive = false;
            if ($svg.length) {
                var computed = window.getComputedStyle($svg[0]).filter;
                filterActive = !!computed && computed !== 'none';
            }
            var initialTitle = filterActive
                ? 'Turn on the light (view in light mode)'
                : 'Turn off the light (back to dark mode)';
            var iconStyle = filterActive
                ? 'font-size:18px;line-height:1;display:inline-block;filter:grayscale(1) brightness(0.6);opacity:0.5;'
                : 'font-size:18px;line-height:1;display:inline-block;';
            var $btn = $('<button alt="light mode" title="' + initialTitle + '" onclick="toggleSvgLightMode(this)">' +
                '<span class="svg-light-toggle-icon" style="' + iconStyle + '">💡</span>' +
                '</button>');
            $element.append($btn);
            $element.data('light-toggle-added', true);
        }
    }

    // Guard against double-adding the scroll listener
    if (!_toolbarScrollListeners[referenceId]) {
        var listener = function () { _updateToolbarPosition(referenceId); };
        _toolbarScrollListeners[referenceId] = listener;
        window.addEventListener('scroll', listener, true);
    }

    // Re-show the search box if it was open for this toolbar's image.
    // _showSearchBox only changes opacity — no repositioning — to avoid
    // the element moving under the cursor and triggering an immediate mouseleave.
    if (typeof _toolbarToHashMap !== 'undefined' && _toolbarToHashMap[referenceId]) {
        var hash = _toolbarToHashMap[referenceId];
        if (typeof _showSearchBox === 'function') {
            _showSearchBox(hash);
        }
    }

    // Start idle auto-hide timer and attach mousemove listener
    _startIdleHideTimer(referenceId);
    _attachIdleMouseMoveListener(referenceId);
}

/**
 * Hide image toolbar (with a short delay so cursor can move to the search box
 * without triggering a hide) and remove the associated scroll listener.
 *
 * @param {string} referenceId - ID of toolbar element
 */
function hideImageToolbar(referenceId) {
    // Immediately cancel idle timer to prevent it firing during the 150ms delay
    if (_idleHideTimers[referenceId]) {
        clearTimeout(_idleHideTimers[referenceId]);
        delete _idleHideTimers[referenceId];
    }

    _hideToolbarTimers[referenceId] = setTimeout(function () {
        delete _hideToolbarTimers[referenceId];

        var $element = $('#' + referenceId);
        $element.attr("style", "display:none;");

        var listener = _toolbarScrollListeners[referenceId];
        if (listener) {
            window.removeEventListener('scroll', listener, true);
            delete _toolbarScrollListeners[referenceId];
        }

        // Hide the search box together with the toolbar
        if (typeof _toolbarToHashMap !== 'undefined' && _toolbarToHashMap[referenceId]) {
            var hash = _toolbarToHashMap[referenceId];
            if (typeof _hideSearchBox === 'function') {
                _hideSearchBox(hash);
            }
        }

        // Clean up idle auto-hide state
        delete _toolbarIdleHidden[referenceId];
        var idleListener = _idleMouseMoveListeners[referenceId];
        if (idleListener) {
            idleListener.container.removeEventListener('mousemove', idleListener.handler, false);
            delete _idleMouseMoveListeners[referenceId];
        }
    }, 150);
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

    // Skip saving for SVG/PlantUML images - they don't need position/size persistence
    if (currentDiv && currentDiv.querySelector && currentDiv.querySelector('svg')) {
        console.log('Skipping resizeImage for SVG/PlantUML element');
        return;
    }

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
