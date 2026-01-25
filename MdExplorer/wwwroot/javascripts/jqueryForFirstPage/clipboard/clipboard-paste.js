/**
 * MdExplorer - Clipboard Paste Interception
 * ==========================================
 * Intercepts Ctrl+V in iframe and triggers Screenshot Annotation Wizard
 * via backend SignalR notification.
 *
 * Flow:
 * 1. User presses Ctrl+V in iframe (document content area)
 * 2. This script intercepts keydown event
 * 3. Calls backend API with connectionId
 * 4. Backend reads clipboard using CrossPlatformClipboard
 * 5. Backend sends SignalR event with image data to Angular
 * 6. Angular opens Screenshot Annotation Wizard
 *
 * Note: The backend reads the system clipboard, not JavaScript clipboard API.
 * This works because the backend runs on the same machine as the browser.
 */

(function() {
    'use strict';

    // Prevent multiple initializations
    if (window.clipboardPasteInitialized) {
        console.log('[clipboard-paste.js] Already initialized, skipping');
        return;
    }
    window.clipboardPasteInitialized = true;

    console.log('[clipboard-paste.js] Initializing Ctrl+V interception for Screenshot Annotation Wizard');

    /**
     * Get the SignalR connection ID from the body attribute
     * @returns {string|null} Connection ID or null if not found
     */
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
     * Check if the active element is an editable field
     * @returns {boolean} True if focus is on an editable element
     */
    function isInEditableElement() {
        const activeElement = document.activeElement;
        if (!activeElement) return false;

        // Check for input/textarea
        if (activeElement.tagName === 'INPUT' || activeElement.tagName === 'TEXTAREA') {
            return true;
        }

        // Check for contenteditable
        if (activeElement.getAttribute('contenteditable') === 'true') {
            return true;
        }

        // Check if inside a contenteditable parent
        if (activeElement.closest('[contenteditable="true"]')) {
            return true;
        }

        return false;
    }

    /**
     * Call backend API to trigger paste wizard
     * @param {string} connectionId - SignalR connection ID
     * @param {string|null} documentPath - Current document path
     */
    async function triggerPasteWizard(connectionId, documentPath) {
        console.log('[clipboard-paste.js] Triggering paste wizard via backend API');
        console.log('[clipboard-paste.js] ConnectionId:', connectionId);
        console.log('[clipboard-paste.js] DocumentPath:', documentPath);

        try {
            const response = await fetch('/api/mdfiles/TriggerPasteWizard', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    connectionId: connectionId,
                    documentPath: documentPath
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('[clipboard-paste.js] API call failed:', response.status, errorText);
            } else {
                const result = await response.json();
                console.log('[clipboard-paste.js] API call successful:', result);
            }
        } catch (error) {
            console.error('[clipboard-paste.js] Error calling API:', error);
        }
    }

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

        // Prevent default paste behavior
        event.preventDefault();
        event.stopPropagation();

        // Trigger the paste wizard via backend
        triggerPasteWizard(connectionId, documentPath);
    }

    // Register the keydown listener
    document.addEventListener('keydown', handleCtrlV);

    console.log('[clipboard-paste.js] Ctrl+V listener registered');

})();
