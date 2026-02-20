/**
 * MdExplorer - SVG Text Search
 * ============================
 * Search and highlight text within SVG diagrams (PlantUML, etc.)
 *
 * Features:
 * - Case-insensitive text search within SVG <text> elements
 * - Yellow highlight rects for matches, orange for current match
 * - Enter/Shift+Enter navigation between results (circular)
 * - Debounced input (300ms), minimum 3 characters
 * - Independent search state per SVG (keyed by hash)
 * - Graceful degradation for non-SVG images
 *
 * Global dependencies:
 * - window.svgSearchActive (from globals.js)
 *
 * DOM:
 * - Creates search box div above each SVG container
 * - Search box ID: svgSearch_{hash}
 */

/**
 * Toggle SVG text search on/off for specified element.
 * First click opens search box, second click closes it.
 *
 * @param {string} stringMatchedHash - ID of the image container element
 */
function toggleMagnifier(stringMatchedHash) {
    var $box = $('#' + stringMatchedHash);
    if ($box.length === 0) return;

    if (window.svgSearchActive[stringMatchedHash]) {
        _svgSearchClose(stringMatchedHash);
    } else {
        _svgSearchOpen(stringMatchedHash, $box);
    }
}

/**
 * Open search box for the given SVG container.
 *
 * @param {string} hash - Element hash/ID
 * @param {jQuery} $box - Container element
 */
function _svgSearchOpen(hash, $box) {
    window.svgSearchActive[hash] = true;

    var searchBoxId = 'svgSearch_' + hash;

    // Don't create duplicate
    if ($('#' + searchBoxId).length > 0) return;

    // Build search box HTML
    var $searchBox = $('<div>', {
        id: searchBoxId,
        css: {
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            padding: '4px 8px',
            background: '#f5f5f5',
            border: '1px solid #ccc',
            borderRadius: '4px',
            marginBottom: '4px',
            fontFamily: 'Arial, sans-serif',
            fontSize: '13px'
        }
    });

    var $input = $('<input>', {
        type: 'text',
        placeholder: 'Search text in SVG...',
        css: {
            flex: '1',
            border: '1px solid #aaa',
            borderRadius: '3px',
            padding: '3px 6px',
            fontSize: '13px',
            outline: 'none',
            minWidth: '120px'
        }
    });

    var $counter = $('<span>', {
        css: {
            color: '#666',
            whiteSpace: 'nowrap',
            minWidth: '60px',
            textAlign: 'center'
        },
        text: ''
    });

    var $closeBtn = $('<button>', {
        html: '&times;',
        css: {
            border: 'none',
            background: 'transparent',
            cursor: 'pointer',
            fontSize: '18px',
            lineHeight: '1',
            padding: '0 4px',
            color: '#666'
        }
    });

    $searchBox.append($input, $counter, $closeBtn);

    // Store state on the search box DOM element
    $searchBox.data('_svgState', {
        matches: [],
        currentIndex: -1,
        highlightRects: [],
        debounceTimer: null
    });

    // Insert as first child of parent container (above the toolbar icons)
    $box.parent().prepend($searchBox);

    // Check if this container has an SVG
    var $svg = $box.find('svg').first();
    if ($svg.length === 0) {
        $counter.text('SVG only');
        $input.prop('disabled', true);
        $input.attr('placeholder', 'No SVG found');
        return;
    }

    // Debounced input handler
    $input.on('input', function () {
        var state = $searchBox.data('_svgState');
        if (state.debounceTimer) clearTimeout(state.debounceTimer);

        var term = $input.val().trim();
        if (term.length < 3) {
            _svgSearchClearHighlights(state);
            state.matches = [];
            state.currentIndex = -1;
            $searchBox.data('_svgState', state);
            $counter.text(term.length > 0 ? 'min 3 chars' : '');
            return;
        }

        state.debounceTimer = setTimeout(function () {
            _svgSearchExecute(hash, term, $box, $searchBox);
        }, 300);
        $searchBox.data('_svgState', state);
    });

    // Keyboard handler
    $input.on('keydown', function (e) {
        if (e.key === 'Escape') {
            _svgSearchClose(hash);
            e.preventDefault();
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (e.shiftKey) {
                _svgSearchNavigate(hash, -1);
            } else {
                _svgSearchNavigate(hash, 1);
            }
        }
    });

    // Close button
    $closeBtn.on('click', function () {
        _svgSearchClose(hash);
    });

    // Focus input
    $input.focus();
}

/**
 * Close search box and clean up highlights.
 *
 * @param {string} hash - Element hash/ID
 */
function _svgSearchClose(hash) {
    window.svgSearchActive[hash] = false;

    var searchBoxId = 'svgSearch_' + hash;
    var $searchBox = $('#' + searchBoxId);
    if ($searchBox.length > 0) {
        var state = $searchBox.data('_svgState');
        if (state) {
            if (state.debounceTimer) clearTimeout(state.debounceTimer);
            _svgSearchClearHighlights(state);
        }
        $searchBox.remove();
    }
}

/**
 * Execute text search within SVG <text> elements.
 *
 * @param {string} hash - Element hash/ID
 * @param {string} searchTerm - Text to search for
 * @param {jQuery} $box - Container element
 * @param {jQuery} $searchBox - Search box element
 */
function _svgSearchExecute(hash, searchTerm, $box, $searchBox) {
    var state = $searchBox.data('_svgState');
    var $counter = $searchBox.find('span');

    // Clear previous highlights
    _svgSearchClearHighlights(state);
    state.matches = [];
    state.currentIndex = -1;

    var $svg = $box.find('svg').first();
    if ($svg.length === 0) {
        $counter.text('SVG only');
        $searchBox.data('_svgState', state);
        return;
    }

    var svgElement = $svg[0];
    var textElements = svgElement.querySelectorAll('text');
    var termLower = searchTerm.toLowerCase();

    // Find all matching <text> elements
    textElements.forEach(function (textEl) {
        var content = textEl.textContent || '';
        if (content.toLowerCase().indexOf(termLower) !== -1) {
            state.matches.push(textEl);
        }
    });

    if (state.matches.length === 0) {
        $counter.text('0 results');
        $searchBox.data('_svgState', state);
        return;
    }

    // Create highlight rects for all matches
    state.matches.forEach(function (textEl, idx) {
        var rect = _svgSearchCreateHighlightRect(svgElement, textEl, false);
        if (rect) state.highlightRects.push(rect);
    });

    // Set current to first match
    state.currentIndex = 0;
    _svgSearchUpdateCurrent(state, svgElement);

    $counter.text('1 / ' + state.matches.length);
    $searchBox.data('_svgState', state);

    // Scroll first match into view
    _svgSearchScrollToMatch(state.matches[0]);
}

/**
 * Create an SVG <rect> highlight behind a <text> element.
 *
 * @param {SVGElement} svgElement - Parent SVG
 * @param {SVGTextElement} textEl - Text element to highlight
 * @param {boolean} isCurrent - Whether this is the current/active match
 * @returns {SVGRectElement|null} The created rect, or null on failure
 */
function _svgSearchCreateHighlightRect(svgElement, textEl, isCurrent) {
    try {
        var bbox = textEl.getBBox();
        var padding = 2;

        var rect = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
        rect.setAttribute('x', bbox.x - padding);
        rect.setAttribute('y', bbox.y - padding);
        rect.setAttribute('width', bbox.width + padding * 2);
        rect.setAttribute('height', bbox.height + padding * 2);
        rect.setAttribute('fill', isCurrent ? '#FF9800' : '#FFFF00');
        rect.setAttribute('fill-opacity', '0.5');
        rect.setAttribute('stroke', 'none');
        rect.setAttribute('class', 'svg-search-highlight');
        rect.setAttribute('pointer-events', 'none');

        // Copy transform from text element to rect if present
        var transform = textEl.getAttribute('transform');
        if (transform) {
            rect.setAttribute('transform', transform);
        }

        // Insert rect just before the text element (renders behind it)
        textEl.parentNode.insertBefore(rect, textEl);

        return rect;
    } catch (e) {
        return null;
    }
}

/**
 * Remove all highlight rects from the SVG.
 *
 * @param {object} state - Search state object
 */
function _svgSearchClearHighlights(state) {
    if (!state || !state.highlightRects) return;

    state.highlightRects.forEach(function (rect) {
        if (rect && rect.parentNode) {
            rect.parentNode.removeChild(rect);
        }
    });
    state.highlightRects = [];
}

/**
 * Update highlight colors: orange for current, yellow for others.
 *
 * @param {object} state - Search state object
 * @param {SVGElement} svgElement - Parent SVG
 */
function _svgSearchUpdateCurrent(state, svgElement) {
    state.highlightRects.forEach(function (rect, idx) {
        if (rect) {
            rect.setAttribute('fill', idx === state.currentIndex ? '#FF9800' : '#FFFF00');
        }
    });
}

/**
 * Navigate between search results (circular).
 *
 * @param {string} hash - Element hash/ID
 * @param {number} direction - 1 for next, -1 for previous
 */
function _svgSearchNavigate(hash, direction) {
    var searchBoxId = 'svgSearch_' + hash;
    var $searchBox = $('#' + searchBoxId);
    if ($searchBox.length === 0) return;

    var state = $searchBox.data('_svgState');
    if (!state || state.matches.length === 0) return;

    var $box = $('#' + hash);
    var $svg = $box.find('svg').first();
    if ($svg.length === 0) return;

    // Calculate new index (circular)
    var newIndex = state.currentIndex + direction;
    if (newIndex >= state.matches.length) newIndex = 0;
    if (newIndex < 0) newIndex = state.matches.length - 1;

    state.currentIndex = newIndex;
    _svgSearchUpdateCurrent(state, $svg[0]);

    // Update counter
    var $counter = $searchBox.find('span');
    $counter.text((newIndex + 1) + ' / ' + state.matches.length);

    $searchBox.data('_svgState', state);

    // Scroll to current match
    _svgSearchScrollToMatch(state.matches[newIndex]);
}

/**
 * Scroll the matched text element into the visible viewport.
 *
 * @param {SVGTextElement} textEl - Text element to scroll to
 */
function _svgSearchScrollToMatch(textEl) {
    if (!textEl) return;

    try {
        var rect = textEl.getBoundingClientRect();
        var viewportHeight = window.innerHeight;
        var viewportWidth = window.innerWidth;

        // Check if element is outside the visible viewport
        var isOutOfView = rect.top < 0 || rect.bottom > viewportHeight ||
                          rect.left < 0 || rect.right > viewportWidth;

        if (isOutOfView) {
            textEl.scrollIntoView({ behavior: 'smooth', block: 'center', inline: 'nearest' });
        }
    } catch (e) {
        // Ignore scroll errors
    }
}
