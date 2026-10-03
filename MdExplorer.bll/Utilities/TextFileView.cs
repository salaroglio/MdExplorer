using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// A text file shown in the page as colored source: what <c>```text(path)</c> embeds in a
    /// document, and what the document panel shows when a text file is clicked in the md-tree.
    /// One place, so the two look and color the same.
    /// </summary>
    public static class TextFileView
    {
        /// <summary>Above this the file is not shown: the page would be too slow to color.</summary>
        public const int MaxBytes = 512_000; // 500 KB — same as the HTML preview

        // How much of a file is looked at to decide whether it is text.
        private const int SniffBytes = 8192;

        // File extension → Prism language identifier. Unknown extensions fall through to plain
        // <pre><code> without a language class (Prism leaves it untouched).
        private static readonly Dictionary<string, string> ExtensionToLanguage =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".ttl"] = "turtle",     [".nt"] = "turtle",      [".n3"] = "turtle",     [".nq"] = "turtle",
                [".rdf"] = "markup",     [".owl"] = "markup",
                [".json"] = "json",      [".jsonld"] = "json",
                [".yaml"] = "yaml",      [".yml"] = "yaml",
                [".xml"] = "markup",     [".xsd"] = "markup",     [".xslt"] = "markup",
                [".sql"] = "sql",        [".cypher"] = "cypher",  [".sparql"] = "sparql",
                [".cs"] = "csharp",      [".ts"] = "typescript",  [".js"] = "javascript",
                [".java"] = "java",      [".kt"] = "kotlin",
                [".py"] = "python",
                [".sh"] = "bash",        [".bash"] = "bash",      [".ps1"] = "powershell",
                [".css"] = "css",        [".scss"] = "scss",
                [".md"] = "markdown",
                [".cob"] = "cobol",      [".cbl"] = "cobol",      [".cpy"] = "cobol",
            };

        public static string LanguageFor(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return ExtensionToLanguage.TryGetValue(Path.GetExtension(path), out var language) ? language : string.Empty;
        }

        /// <summary>
        /// Whether the file is text, by its content and not by its name: a <c>.log</c>, a
        /// <c>.gitignore</c>, a <c>Dockerfile</c> are text too. Text means UTF-8 (with or without
        /// BOM) without NUL bytes, or UTF-16 with its BOM — what Windows PowerShell writes.
        /// Only the first 8 KB are read.
        /// </summary>
        public static bool IsText(string path)
        {
            byte[] head;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                head = new byte[Math.Min(SniffBytes, stream.Length)];
                var read = 0;
                while (read < head.Length)
                {
                    var n = stream.Read(head, read, head.Length - read);
                    if (n == 0) break;
                    read += n;
                }
                if (read < head.Length) Array.Resize(ref head, read);
            }
            return IsText(head);
        }

        internal static bool IsText(byte[] head)
        {
            if (HasUtf16Bom(head))
            {
                return true;
            }
            if (Array.IndexOf(head, (byte)0) >= 0)
            {
                return false;
            }
            try
            {
                // flush: false — the 8 KB may cut a multi-byte character in two: that is not an error.
                var decoder = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetDecoder();
                decoder.GetCharCount(head, 0, head.Length, flush: false);
                return true;
            }
            catch (DecoderFallbackException)
            {
                return false;
            }
        }

        /// <summary>The file's text, decoded as <see cref="IsText(string)"/> recognised it.</summary>
        public static string ReadText(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // detectEncodingFromByteOrderMarks: UTF-16 and UTF-8 BOMs; without BOM, UTF-8.
            using var reader = new StreamReader(stream, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);
            return reader.ReadToEnd();
        }

        /// <summary>The colored source box, with the file name, "copy path" and "fullscreen".</summary>
        public static string ContainerHtml(string id, string filePath, string language, string content)
        {
            var encodedContent = HttpUtility.HtmlEncode(content);
            var encodedPath    = HttpUtility.HtmlAttributeEncode(filePath);
            var encodedFileNameForHeader = HttpUtility.HtmlEncode(Path.GetFileName(filePath));

            var languageClass = string.IsNullOrEmpty(language) ? "" : $" class=\"language-{language}\"";

            // SVG icons (kept inline to avoid extra HTTP requests and to match html-preview style)
            var copyIcon       = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""14"" height=""14"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><rect x=""9"" y=""9"" width=""13"" height=""13"" rx=""2"" ry=""2""></rect><path d=""M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1""></path></svg>";
            var fullscreenIcon = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""14"" height=""14"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><polyline points=""15 3 21 3 21 9""></polyline><polyline points=""9 21 3 21 3 15""></polyline><line x1=""21"" y1=""3"" x2=""14"" y2=""10""></line><line x1=""3"" y1=""21"" x2=""10"" y2=""14""></line></svg>";

            return $@"<div class=""mde-text-include-container"" data-id=""{id}"">
  <div class=""mde-text-include-header"">
    <span class=""mde-text-include-filename"">{encodedFileNameForHeader}</span>
    <span class=""mde-text-include-toolbar"">
      <a class=""mde-text-include-btn mde-copy-path-btn"" href=""#"" title=""{encodedPath}"" data-filepath=""{encodedPath}"">{copyIcon}</a>
      <a class=""mde-text-include-btn mde-text-include-fullscreen-btn"" href=""#"" title=""Fullscreen"">{fullscreenIcon}</a>
    </span>
  </div>
  <pre class=""mde-text-include-pre""><code{languageClass}>{encodedContent}</code></pre>
</div>";
        }

        private static bool HasUtf16Bom(byte[] head)
            => head.Length >= 2 && ((head[0] == 0xFF && head[1] == 0xFE) || (head[0] == 0xFE && head[1] == 0xFF));
    }
}
