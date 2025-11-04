/**
 * MdExplorer - Syntax Highlighting (Prism.js)
 * ============================================
 * Auto-detects programming language and applies Prism.js highlighting
 *
 * Features:
 * - Detects language from code content (heuristics)
 * - Skips already-processed code blocks
 * - Supports: Java, C#, JavaScript, Python, SQL
 * - Default fallback: Java
 * - 500ms delay to ensure DOM is ready
 *
 * Language Detection Heuristics:
 * - Java: "public class", "private ", "import java"
 * - C#: "using System", "namespace ", "public void"
 * - JavaScript: "function(", "const ", "var "
 * - Python: "def ", "import ", "print("
 * - SQL: "SELECT ", "INSERT " (case-insensitive)
 *
 * Dependencies:
 * - Prism.js library
 *
 * DOM:
 * - <code>: Inline code or code blocks
 * - <pre><code>: Code blocks for highlighting
 * - .language-*: CSS class applied after detection
 */

/**
 * Initialize Prism.js syntax highlighting
 * Auto-detects language for code blocks without language class
 * Applies highlighting with 500ms delay to ensure DOM is ready
 */
$(function() {
    // Wait for DOM to be fully loaded
    setTimeout(function() {
        // Find code blocks and add language classes based on content
        $('code').each(function() {
            // Skip if already processed
            if ($(this).hasClass('language-none') || $(this).attr('class')?.includes('language-')) {
                return;
            }

            // Check parent pre tag for language hint
            var $pre = $(this).parent('pre');
            if ($pre.length && $pre.attr('class')?.includes('language-')) {
                return; // Already has language class
            }

            // Add default language class for inline code
            if (!$(this).parent('pre').length) {
                // This is inline code, skip Prism for now
                return;
            }

            // For code blocks, try to detect language
            var codeText = $(this).text().trim();
            var lines = codeText.split('\n');

            // Simple heuristics for language detection
            if (codeText.includes('public class') || codeText.includes('private ') || codeText.includes('import java')) {
                $(this).addClass('language-java');
            } else if (codeText.includes('using System') || codeText.includes('namespace ') || codeText.includes('public void')) {
                $(this).addClass('language-csharp');
            } else if (codeText.includes('function(') || codeText.includes('const ') || codeText.includes('var ')) {
                $(this).addClass('language-javascript');
            } else if (codeText.includes('def ') || codeText.includes('import ') || codeText.includes('print(')) {
                $(this).addClass('language-python');
            } else if (codeText.toUpperCase().includes('SELECT ') || codeText.toUpperCase().includes('INSERT ')) {
                $(this).addClass('language-sql');
            } else {
                // Default to Java if can't detect
                $(this).addClass('language-java');
            }
        });

        // Apply Prism highlighting only once
        if (typeof Prism !== 'undefined') {
            Prism.highlightAll();
        }
    }, 500);
});
