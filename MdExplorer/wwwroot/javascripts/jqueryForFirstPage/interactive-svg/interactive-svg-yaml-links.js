/**
 * MdExplorer - Interactive SVG YAML Links
 * ========================================
 * Makes url/link/href values clickable in PlantUML YAML diagrams (@startyaml/@endyaml).
 *
 * Features:
 * - Scans SVG <text> elements for YAML keys: url, link, href (case-insensitive)
 * - Makes the corresponding value text clickable
 * - Supports internal relative links (./path, ../path), root links (/path), and external URLs
 * - External links open in a new window (Electron uses setWindowOpenHandler)
 * - Internal links navigate via postMessage to Angular (prevents double load)
 *
 * Usage:
 *   InteractiveSvgYamlLinks.init(svgElement);
 *   InteractiveSvgYamlLinks.initAll();
 *
 * CSS Required:
 *   Include interactive-svg-yaml-links.css for visual effects
 */

var InteractiveSvgYamlLinks = (function() {
    'use strict';

    // Track initialized SVGs to avoid double-initialization
    var initializedSvgs = new WeakSet();

    // YAML key names that should trigger link detection
    var LINK_KEY_PATTERN = /^(url|link|href)$/i;

    // Pattern for combined "key : value" text
    var COMBINED_KEY_VALUE_PATTERN = /^(url|link|href)\s*:\s*(.+)$/i;

    /**
     * Check if a URL is external (starts with http:// or https://)
     * @param {string} url - The URL to check
     * @returns {boolean}
     */
    function isExternalUrl(url) {
        return /^https?:\/\//i.test(url);
    }

    /**
     * Resolve a relative path against the current document directory
     * @param {string} relativePath - The relative path (./foo.md, ../bar.md)
     * @param {string} documentPath - The current document path from body attribute
     * @returns {string} - Resolved path
     */
    function resolveRelativePath(relativePath, documentPath) {
        // Get directory of current document
        var lastSlash = documentPath.lastIndexOf('/');
        var currentDir = lastSlash >= 0 ? documentPath.substring(0, lastSlash) : '';

        var parts = currentDir.split('/').filter(function(p) { return p !== ''; });
        var relParts = relativePath.split('/');

        for (var i = 0; i < relParts.length; i++) {
            var segment = relParts[i];
            if (segment === '.' || segment === '') {
                continue;
            } else if (segment === '..') {
                if (parts.length > 0) {
                    parts.pop();
                }
            } else {
                parts.push(segment);
            }
        }

        return '/' + parts.join('/');
    }

    /**
     * Handle click on a link text element.
     * Internal links use postMessage to let Angular handle navigation (single load, no flickering).
     * @param {string} url - The URL to navigate to
     */
    function handleLinkClick(url) {
        if (!url) return;

        url = url.trim();

        if (isExternalUrl(url)) {
            // External link: open in new window
            // In Electron, setWindowOpenHandler in index.js will handle this
            window.open(url, '_blank');
        } else {
            // Internal link: communicate with Angular via postMessage (avoids double load)
            var documentPath = $('body').attr('documentpath') || '';
            var resolvedPath;

            if (url.charAt(0) === '/') {
                resolvedPath = url;
            } else {
                resolvedPath = resolveRelativePath(url, documentPath);
            }

            window.parent.postMessage({
                type: 'md-navigate',
                relativePath: resolvedPath,
                name: resolvedPath.split('/').pop()
            }, '*');
        }
    }

    /**
     * Make a <text> element clickable as a link
     * @param {SVGTextElement} textEl - The SVG text element
     * @param {string} url - The URL this text links to
     */
    function makeClickable(textEl, url) {
        textEl.classList.add('yaml-link-text');
        textEl.setAttribute('data-yaml-link-url', url);
        textEl.style.cursor = 'pointer';

        textEl.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            handleLinkClick(url);
        });
    }

    /**
     * Get the Y coordinate of a <text> element
     * @param {SVGTextElement} textEl
     * @returns {number}
     */
    function getTextY(textEl) {
        return parseFloat(textEl.getAttribute('y')) || 0;
    }

    /**
     * Get the X coordinate of a <text> element
     * @param {SVGTextElement} textEl
     * @returns {number}
     */
    function getTextX(textEl) {
        return parseFloat(textEl.getAttribute('x')) || 0;
    }

    /**
     * Initialize clickable YAML links on a single SVG
     * @param {SVGElement} svg - The SVG element to process
     */
    function init(svg) {
        if (!svg || svg.tagName !== 'svg') return;

        if (initializedSvgs.has(svg)) return;

        var textElements = svg.querySelectorAll('text');
        if (textElements.length === 0) return;

        var linksFound = 0;

        // Build a list of all text elements with their positions
        var textList = Array.prototype.slice.call(textElements);

        for (var i = 0; i < textList.length; i++) {
            var textEl = textList[i];
            var content = (textEl.textContent || '').trim();

            // Case B: Combined "key : value" in a single text element
            var combinedMatch = content.match(COMBINED_KEY_VALUE_PATTERN);
            if (combinedMatch) {
                var value = combinedMatch[2].trim();
                if (value) {
                    makeClickable(textEl, value);
                    linksFound++;
                }
                continue;
            }

            // Case A: Key and value are separate <text> elements
            if (LINK_KEY_PATTERN.test(content)) {
                var keyY = getTextY(textEl);
                var keyX = getTextX(textEl);
                var tolerance = 2; // Y coordinate tolerance in SVG units

                // Find the value text: same Y, greater X
                var bestCandidate = null;
                var bestX = Infinity;

                for (var j = 0; j < textList.length; j++) {
                    if (j === i) continue;

                    var candidate = textList[j];
                    var candY = getTextY(candidate);
                    var candX = getTextX(candidate);
                    var candText = (candidate.textContent || '').trim();

                    // Same row (within tolerance) and to the right of the key
                    if (Math.abs(candY - keyY) < tolerance && candX > keyX && candText !== '') {
                        // Skip if the candidate is itself a YAML key
                        if (LINK_KEY_PATTERN.test(candText)) continue;

                        // Pick the closest text to the right
                        if (candX < bestX) {
                            bestX = candX;
                            bestCandidate = candidate;
                        }
                    }
                }

                if (bestCandidate) {
                    var linkUrl = (bestCandidate.textContent || '').trim();
                    if (linkUrl) {
                        makeClickable(bestCandidate, linkUrl);
                        linksFound++;
                    }
                }
            }
        }

        if (linksFound > 0) {
            initializedSvgs.add(svg);
            console.log('[InteractiveSvgYamlLinks] Initialized SVG with', linksFound, 'clickable YAML links');
        }
    }

    /**
     * Initialize clickable YAML links on all SVGs in the page
     */
    function initAll() {
        document.querySelectorAll('svg').forEach(function(svg) {
            init(svg);
        });
    }

    // Public API
    return {
        init: init,
        initAll: initAll
    };

})();
