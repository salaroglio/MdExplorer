/**
 * MdExplorer - Mermaid Diagram Rendering
 * =======================================
 * Intelligent initialization for Mermaid diagrams in Electron/Web
 *
 * Features:
 * - Detects Electron vs Web browser environment
 * - Prevents double-rendering of already-rendered SVGs
 * - Handles pre-rendered diagrams (from backend SSR)
 * - Smooth fade-in animation after rendering
 * - Comprehensive logging for debugging
 *
 * Challenges Solved:
 * - Electron can double-render if not careful
 * - Web browser auto-initializes correctly
 * - Pre-rendered SVGs must not be re-processed
 *
 * Dependencies:
 * - mermaid.js library
 *
 * DOM:
 * - .mermaid: Elements needing rendering
 * - .mermaid-processed: Already rendered elements
 * - data-processed: Attribute marking processed elements
 */

/**
 * Initialize Mermaid diagram rendering
 * Different strategies for Electron vs Web browser
 */
$(function() {
    // Check if we're in Electron
    const isElectron = navigator.userAgent.includes('Electron');
    console.log(`=== MERMAID INIT (${isElectron ? 'ELECTRON' : 'WEB'}) ===`);

    // In web browser, let mermaid auto-initialize
    if (!isElectron) {
        console.log('Web browser detected - letting mermaid auto-initialize');
        return;
    }

    // In Electron, we need careful handling
    console.log('Electron detected - special mermaid handling');

    // FIRST: Immediately handle all mermaid elements
    const preProtect = document.querySelectorAll('.mermaid');
    console.log(`Pre-processing ${preProtect.length} mermaid elements...`);
    preProtect.forEach(el => {
        if (el.querySelector('svg') || el.children.length > 0) {
            // Already has SVG - just remove mermaid class to prevent reprocessing
            el.classList.remove('mermaid');
            el.classList.add('mermaid-already-processed');
            el.setAttribute('data-pre-rendered', 'true');
        } else {
            // Needs rendering - hide it until ready to prevent flash
            el.style.visibility = 'hidden';
            el.setAttribute('data-needs-rendering', 'true');
        }
    });

    // Process as soon as mermaid is available
    const checkMermaid = () => {
        if (typeof mermaid === 'undefined') {
            // Retry quickly if mermaid not loaded yet
            setTimeout(checkMermaid, 10);
            return;
        }

        console.log('Mermaid loaded, processing immediately...');
        const elements = document.querySelectorAll('.mermaid');
        console.log(`Found ${elements.length} .mermaid elements`);

        // Check what type of content we have
        let needsRendering = [];
        let alreadyRendered = [];

        elements.forEach((el, i) => {
            const content = el.textContent.trim();
            const hasChildren = el.children.length > 0;
            const hasSVG = el.querySelector('svg') !== null;
            const innerHTML = el.innerHTML;

            console.log(`Element ${i}:`);
            console.log('  - ID:', el.id);
            console.log('  - Has children:', hasChildren);
            console.log('  - Has SVG:', hasSVG);
            console.log('  - Has data-processed:', el.hasAttribute('data-processed'));
            console.log('  - Content starts with:', content.substring(0, 50));
            console.log('  - InnerHTML (first 300 chars):', innerHTML.substring(0, 300));
            console.log('  - OuterHTML (first 300 chars):', el.outerHTML.substring(0, 300));

            // Check for potential HTML issues
            const hasStyleTag = innerHTML.includes('<style');
            const hasScriptTag = innerHTML.includes('<script');
            const looksBroken = innerHTML.includes('&lt;') || innerHTML.includes('&gt;');

            console.log('  - Has <style> tag:', hasStyleTag);
            console.log('  - Has <script> tag:', hasScriptTag);
            console.log('  - Has escaped HTML entities:', looksBroken);

            // IMPORTANT: Mark already processed elements to prevent mermaid from touching them
            if (hasSVG || hasChildren) {
                alreadyRendered.push(el);
                console.log('  -> Already rendered, marking as processed');
                // Mark it so mermaid won't try to process it
                el.setAttribute('data-processed', 'true');
                // Also remove the mermaid class to be extra safe
                el.classList.remove('mermaid');
                el.classList.add('mermaid-processed');
            } else if (content && !content.startsWith('<')) {
                // Has mermaid syntax that needs rendering
                needsRendering.push(el);
                console.log('  -> Needs rendering');
                // Ensure it has an ID
                if (!el.id) {
                    el.id = `mermaid-${Date.now()}-${i}`;
                }
            } else {
                console.log('  -> Unknown state, skipping and hiding');
                el.setAttribute('data-processed', 'true');
                el.classList.remove('mermaid');
            }
        });

        console.log(`Summary: ${alreadyRendered.length} already rendered, ${needsRendering.length} need rendering`);

        // Only initialize and render if we have elements that need it
        if (needsRendering.length > 0) {
            try {
                console.log('Initializing mermaid...');
                mermaid.initialize({
                    startOnLoad: false,
                    theme: 'default',
                    securityLevel: 'loose'
                });

                console.log('Running mermaid.run()...');
                mermaid.run({
                    nodes: needsRendering,
                    querySelector: '.mermaid:not([data-processed="true"])'
                }).then(() => {
                    console.log('✅ Mermaid rendering complete');
                    // Show all rendered elements with smooth transition
                    needsRendering.forEach(el => {
                        el.style.visibility = 'visible';
                        el.style.opacity = '0';
                        el.style.transition = 'opacity 0.3s ease-in';
                        setTimeout(() => {
                            el.style.opacity = '1';
                        }, 10);
                    });
                }).catch(err => {
                    console.error('❌ Mermaid rendering error:', err);
                    // Show elements even if error
                    needsRendering.forEach(el => {
                        el.style.visibility = 'visible';
                    });
                });
            } catch (error) {
                console.error('❌ Fatal mermaid error:', error);
            }
        }
    };

    // Start checking immediately
    checkMermaid();
});
