/**
 * MdExplorer - Clipboard Paste (Screenshot Annotation Wizard)
 * ============================================================
 * Opens the Screenshot Annotation Wizard with the image in the system clipboard, and says
 * WHERE in the markdown file the annotated image will go.
 *
 * Two gestures:
 * - Ctrl+V: the image goes at the block under the mouse pointer — before it if the pointer
 *   is on its upper half, after it on the lower half. Pointer outside any mapped block: at
 *   the end of the document, as it always was. The wizard says which.
 * - Right-click on a block: a line shows the exact point, and a menu offers
 *   "Incolla immagine qui". Shift + right-click gives the browser's menu, as before.
 *
 * The point comes from the source map: every rendered block carries data-mde-line-start/end,
 * the lines of the FILE on disk (MarkdownSourceMapService). The anchor is the OUTERMOST mapped
 * block inside the document content, so a list, a table or a quote is never split: the image
 * goes before or after the whole of it. Where no block is mapped (diagrams, table of contents,
 * generated content) nothing is offered — the right-click stays the browser's. Never guess.
 *
 * The page also carries data-mde-source-hash, the fingerprint of the file it was rendered
 * from: the backend refuses the anchor (409) if the file has changed since, because the page's
 * line numbers would then point at the wrong place.
 *
 * Flow:
 * 1. Gesture → anchor (optional) → POST /api/mdfiles/TriggerPasteWizard
 * 2. The backend checks the anchor, reads the clipboard (CrossPlatformClipboard), and sends
 *    the image + anchor to Angular via SignalR; Angular opens the wizard.
 *
 * Note: the backend reads the system clipboard, not the JavaScript clipboard API. This works
 * because the backend runs on the same machine as the browser.
 */

(function() {
    'use strict';

    // Prevent multiple initializations
    if (window.clipboardPasteInitialized) {
        console.log('[clipboard-paste.js] Already initialized, skipping');
        return;
    }
    window.clipboardPasteInitialized = true;

    console.log('[clipboard-paste.js] Initializing Ctrl+V and right-click paste for Screenshot Annotation Wizard');

    // The document content: TOC, toolbars and side panels are outside it.
    var CONTENT_SELECTOR = '.mdeItemMainPageCenter';
    var MENU_ID = 'mde-paste-menu';
    var LINE_ID = 'mde-paste-line';
    var TOAST_ID = 'mde-paste-toast';

    /**
     * Get the SignalR connection ID from the body attribute
     * @returns {string|null} Connection ID or null if not found
     */
    /**
     * The page shows a markdown document. A text file shown as source (body data-mde-view="text",
     * a click on a .json in the md-tree) is not one: pasting there would append markdown to it.
     */
    function isMarkdownPage() {
        return !document.body.hasAttribute('data-mde-view');
    }

    function getConnectionId() {
        const connectionId = document.body.getAttribute('ConnectionId');
        if (!connectionId) {
            console.warn('[clipboard-paste.js] ConnectionId not found on document.body');
        }
        return connectionId;
    }

    /**
     * Get the current document path from the body attribute
     * @returns {string|null} Document path or null if not found
     */
    function getDocumentPath() {
        const documentPath = document.body.getAttribute('DocumentPath');
        return documentPath;
    }

    /**
     * Fingerprint of the file this page was rendered from. Without it an anchor cannot be
     * checked, so none is offered.
     */
    function getSourceHash() {
        return document.body.getAttribute('data-mde-source-hash') || null;
    }

    /**
     * Check if the active element is an editable field
     * @returns {boolean} True if focus is on an editable element
     */
    function isInEditableElement() {
        const activeElement = document.activeElement;
        if (!activeElement) return false;
        if (activeElement.tagName === 'INPUT' || activeElement.tagName === 'TEXTAREA') {
            return true;
        }
        if (activeElement.getAttribute('contenteditable') === 'true') {
            return true;
        }
        if (activeElement.closest('[contenteditable="true"]')) {
            return true;
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  The anchor: which block, and before or after it
    // ─────────────────────────────────────────────────────────────────────

    /**
     * The outermost element carrying data-mde-line-start between el and the document content.
     * Outermost on purpose: right-clicking a list item anchors to the whole list.
     */
    function outermostMappedBlock(el) {
        var content = document.querySelector(CONTENT_SELECTOR);
        if (!content || !el || !content.contains(el)) return null;
        var found = null;
        for (var node = el; node && node !== content; node = node.parentElement) {
            if (node.hasAttribute && node.hasAttribute('data-mde-line-start')) {
                found = node;
            }
        }
        return found;
    }

    /**
     * What the user sees as the block. For code the source map marks the <code>, but on screen
     * the block is the whole box around it, toolbar of the Run button included.
     */
    function visualBox(block) {
        if (block.tagName === 'CODE') {
            return block.closest('.mde-exec-block') || block.closest('pre') || block;
        }
        return block;
    }

    /**
     * The anchor under a point of the page, or null when there is nothing safe to point at.
     * @returns {{startLine:number,endLine:number,position:string,box:Element}|null}
     */
    function anchorAt(target, clientY) {
        var el = target && target.nodeType === Node.ELEMENT_NODE ? target : (target && target.parentElement);
        if (!el || el.closest('svg')) return null;   // diagrams: their own menu, or the browser's
        if (!getSourceHash()) return null;

        var block = outermostMappedBlock(el);
        if (!block) {
            // A code block's box is bigger than its mapped <code>: the Run toolbar above it and the
            // padding of the <pre> are the block too, for whoever looks at the page — but there
            // the <code> is a sibling or a child, not an ancestor. Found by right-clicking the
            // toolbar in a real browser: no menu.
            var codeBox = el.closest('.mde-exec-block') || el.closest('pre');
            var mappedCode = codeBox && codeBox.querySelector('code[data-mde-line-start]');
            if (mappedCode) block = outermostMappedBlock(mappedCode);
        }
        if (!block) return null;

        var startLine = parseInt(block.getAttribute('data-mde-line-start'), 10);
        var endLine = parseInt(block.getAttribute('data-mde-line-end'), 10);
        if (isNaN(startLine) || isNaN(endLine) || endLine < startLine) return null;

        var box = visualBox(block);
        var rect = box.getBoundingClientRect();
        return {
            startLine: startLine,
            endLine: endLine,
            position: clientY < rect.top + rect.height / 2 ? 'before' : 'after',
            box: box
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    //  The request
    // ─────────────────────────────────────────────────────────────────────

    /**
     * Call backend API to trigger paste wizard
     * @param {string} connectionId - SignalR connection ID
     * @param {string|null} documentPath - Current document path
     * @param {object|null} anchor - Where the image goes; null = at the end of the document
     */
    async function triggerPasteWizard(connectionId, documentPath, anchor) {
        console.log('[clipboard-paste.js] Triggering paste wizard via backend API');
        console.log('[clipboard-paste.js] ConnectionId:', connectionId);
        console.log('[clipboard-paste.js] DocumentPath:', documentPath);
        console.log('[clipboard-paste.js] Anchor:', anchor
            ? anchor.position + ' lines ' + anchor.startLine + '-' + anchor.endLine
            : 'none (end of document)');

        const body = {
            connectionId: connectionId,
            documentPath: documentPath
        };
        if (anchor) {
            body.anchorStartLine = anchor.startLine;
            body.anchorEndLine = anchor.endLine;
            body.anchorPosition = anchor.position;
            body.sourceHash = getSourceHash();
        }

        try {
            const response = await fetch('/api/mdfiles/TriggerPasteWizard', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(body)
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('[clipboard-paste.js] API call failed:', response.status, errorText);
                // Say it on the page: before, a refusal only reached the console, and the user
                // pressed Ctrl+V and saw nothing happen.
                showToast(readableError(errorText, response.status));
            } else {
                const result = await response.json();
                console.log('[clipboard-paste.js] API call successful:', result);
            }
        } catch (error) {
            console.error('[clipboard-paste.js] Error calling API:', error);
            showToast('Incolla non riuscito: ' + error.message);
        }
    }

    function readableError(errorText, status) {
        try {
            const parsed = JSON.parse(errorText);
            if (parsed.message) return parsed.message;
            if (parsed.error) return 'Incolla non riuscito: ' + parsed.error;
        } catch (e) { /* not JSON */ }
        return 'Incolla non riuscito (HTTP ' + status + ')';
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Guide line, menu, notice
    // ─────────────────────────────────────────────────────────────────────

    /**
     * The line that shows where the image will go. Placed in document coordinates, so it stays
     * with the content while the page scrolls.
     */
    function showGuideLine(anchor) {
        removeGuideLine();
        var rect = anchor.box.getBoundingClientRect();
        var line = document.createElement('div');
        line.id = LINE_ID;
        line.className = 'mde-paste-line';
        var y = anchor.position === 'before' ? rect.top - 5 : rect.bottom + 5;
        line.style.top = (y + window.scrollY) + 'px';
        line.style.left = (rect.left + window.scrollX) + 'px';
        line.style.width = rect.width + 'px';
        line.innerHTML = '<span class="mde-paste-line-label">l\'immagine andrà qui</span>';
        document.body.appendChild(line);
    }

    function removeGuideLine() {
        var line = document.getElementById(LINE_ID);
        if (line) line.remove();
    }

    function closeMenu() {
        var menu = document.getElementById(MENU_ID);
        if (menu) menu.remove();
        removeGuideLine();
    }

    function menuItem(icon, label, onClick) {
        var item = document.createElement('button');
        item.type = 'button';
        item.className = 'mde-paste-menu-item';
        item.innerHTML = '<span class="mde-paste-menu-icon">' + icon + '</span>' + label;
        item.addEventListener('click', function () {
            closeMenu();
            onClick();
        });
        return item;
    }

    /**
     * onPaste and onEdit are optional, not both absent: pasting needs an anchor between blocks,
     * correcting the text needs a block that inline-edit.js can address.
     */
    function openMenu(x, y, anchor, onPaste, onEdit) {
        closeMenu();
        if (anchor) showGuideLine(anchor);

        var menu = document.createElement('div');
        menu.id = MENU_ID;
        menu.className = 'mde-paste-menu';

        // The browser's menu is one key away: say so, since this one replaced it.
        var hint = document.createElement('div');
        hint.className = 'mde-paste-menu-hint';
        hint.textContent = 'Shift + tasto destro: menu del browser';

        if (onPaste) menu.appendChild(menuItem('📋', 'Incolla immagine qui', onPaste));
        if (onEdit) menu.appendChild(menuItem('✏️', 'Modifica testo', onEdit));
        menu.appendChild(hint);
        document.body.appendChild(menu);

        // Keep the menu inside the viewport.
        var rect = menu.getBoundingClientRect();
        var left = Math.min(x, window.innerWidth - rect.width - 8);
        var top = Math.min(y, window.innerHeight - rect.height - 8);
        menu.style.left = Math.max(8, left) + 'px';
        menu.style.top = Math.max(8, top) + 'px';
    }

    function showToast(message) {
        var existing = document.getElementById(TOAST_ID);
        if (existing) existing.remove();
        var toast = document.createElement('div');
        toast.id = TOAST_ID;
        toast.className = 'mde-paste-toast';
        toast.textContent = message;
        toast.addEventListener('click', function () { toast.remove(); });
        document.body.appendChild(toast);
        setTimeout(function () { if (toast.isConnected) toast.remove(); }, 7000);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Gestures
    // ─────────────────────────────────────────────────────────────────────

    // Where the pointer is, for Ctrl+V. Forgotten when it leaves the page: pressing Ctrl+V with
    // the pointer on the side panel must not paste at wherever it last crossed the document.
    var lastPointer = null;
    document.addEventListener('mousemove', function (e) {
        lastPointer = { x: e.clientX, y: e.clientY };
    }, { passive: true });
    document.documentElement.addEventListener('mouseleave', function () {
        lastPointer = null;
    });

    /**
     * Handle Ctrl+V keydown event
     * @param {KeyboardEvent} event - Keyboard event
     */
    function handleCtrlV(event) {
        // Check for Ctrl+V (or Cmd+V on Mac)
        if (!((event.ctrlKey || event.metaKey) && event.key === 'v')) {
            return;
        }

        console.log('[clipboard-paste.js] Ctrl+V detected');

        if (!isMarkdownPage()) {
            return;
        }

        // Skip if user is in an editable element (allow normal paste)
        if (isInEditableElement()) {
            console.log('[clipboard-paste.js] Skipping - focus is on editable element');
            return;
        }

        // Get connection ID
        const connectionId = getConnectionId();
        if (!connectionId) {
            console.warn('[clipboard-paste.js] Cannot trigger paste wizard - no connectionId');
            return;
        }

        // Get document path
        const documentPath = getDocumentPath();
        if (!documentPath) {
            console.warn('[clipboard-paste.js] No document path found - will be handled by Angular');
        }

        // The block under the pointer, if any. None → the end of the document, and the wizard
        // says so: it is the only confirmation of the point this gesture gets.
        var anchor = null;
        if (lastPointer) {
            var under = document.elementFromPoint(lastPointer.x, lastPointer.y);
            anchor = anchorAt(under, lastPointer.y);
        }

        // Prevent default paste behavior
        event.preventDefault();
        event.stopPropagation();

        // Trigger the paste wizard via backend
        triggerPasteWizard(connectionId, documentPath, anchor);
    }

    /**
     * Right-click on a mapped block: our menu, with the line showing the point. Everywhere else
     * — and always with Shift — the browser's menu, untouched.
     */
    function handleContextMenu(event) {
        if (event.shiftKey || !isMarkdownPage()) return;
        closeMenu();

        var target = event.target;
        if (target && target.closest && target.closest('input, textarea, [contenteditable="true"]')) return;

        const connectionId = getConnectionId();
        if (!connectionId) return;

        var anchor = anchorAt(target, event.clientY);
        // The other gesture of this menu (inline-edit.js): correct the text of the block under the
        // pointer — the cell, the list item, not the whole table or list the anchor points at.
        var inlineEdit = window.mdeInlineEdit;
        var editable = inlineEdit ? inlineEdit.blockAt(target) : null;
        if (!anchor && !editable) return;

        event.preventDefault();
        var x = event.clientX;
        var y = event.clientY;
        openMenu(x, y, anchor,
            anchor ? function () { triggerPasteWizard(connectionId, getDocumentPath(), anchor); } : null,
            editable ? function () { inlineEdit.start(editable, x, y); } : null);
    }

    // Register the listeners
    document.addEventListener('keydown', handleCtrlV);
    // Bubble phase: the diagram menu (mark-diagram-context.js) stops propagation on its boxes,
    // so a right-click on a diagram box never reaches here.
    document.addEventListener('contextmenu', handleContextMenu);

    // Closing: click elsewhere, Esc, leaving the window, scrolling (the menu is fixed, the line
    // moves with the content — they would drift apart).
    document.addEventListener('click', function (e) {
        if (!e.target.closest('#' + MENU_ID)) closeMenu();
    });
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeMenu();
    });
    window.addEventListener('blur', closeMenu);
    window.addEventListener('scroll', closeMenu, { passive: true });

    console.log('[clipboard-paste.js] Ctrl+V and right-click listeners registered');
})();
