using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#if WINDOWS
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace MdExplorer.Features.Exports
{
    /// <summary>
    /// Converts ASCII art code blocks to PNG images for Word export.
    /// Uses SkiaSharp for cross-platform text rendering with monospace fonts.
    /// </summary>
    public class AsciiArtToImageService : IAsciiArtToImageService
    {
        private readonly ILogger<AsciiArtToImageService> _logger;

        // Box-drawing and related Unicode characters that indicate ASCII art
        private static readonly char[] BoxDrawingChars = new[]
        {
            '┌', '┐', '└', '┘', '│', '─', '├', '┤', '┬', '┴', '┼',
            '╔', '╗', '╚', '╝', '║', '═', '╠', '╣', '╦', '╩', '╬',
            '▼', '▲', '►', '◄', '▶', '◀', '●', '○', '■', '□',
            '╭', '╮', '╯', '╰', '╱', '╲', '╳'
        };

        // Languages that should always be converted to images for better Word rendering
        // Note: bash/shell excluded to allow copy/paste in Word
        private static readonly string[] ImageConversionLanguages = new[]
        {
            "txt", "text"
        };

        // Regex to find fenced code blocks (``` or ~~~)
        private static readonly Regex CodeBlockRegex = new Regex(
            @"```([^\n]*)\n([\s\S]*?)```",
            RegexOptions.Compiled | RegexOptions.Multiline);

        // Font settings
        private const float FontSize = 14f;
        private const float LineHeight = 1.2f;
        private const float Padding = 20f;
        private const string PreferredFont = "Consolas";
        private static readonly string[] FallbackFonts = { "Courier New", "DejaVu Sans Mono", "Liberation Mono", "monospace" };

        // Syntax highlighting color palette (VS Code-like colors for light background)
        private static readonly SKColor ColorComment = new SKColor(0, 128, 0);       // Green
        private static readonly SKColor ColorString = new SKColor(163, 21, 21);      // Dark red
        private static readonly SKColor ColorKeyword = new SKColor(0, 0, 255);       // Blue
        private static readonly SKColor ColorType = new SKColor(43, 145, 175);       // Teal
        private static readonly SKColor ColorNumber = new SKColor(9, 134, 88);       // Dark green
        private static readonly SKColor ColorOperator = new SKColor(128, 0, 128);    // Purple
        private static readonly SKColor ColorDefault = SKColors.Black;

        // Language categories for syntax highlighting
        private enum LanguageCategory { None, CStyle, Python, Shell, Sql, Xml }

        private static readonly Dictionary<string, LanguageCategory> LanguageCategories = new Dictionary<string, LanguageCategory>(StringComparer.OrdinalIgnoreCase)
        {
            { "csharp", LanguageCategory.CStyle }, { "cs", LanguageCategory.CStyle }, { "c#", LanguageCategory.CStyle },
            { "java", LanguageCategory.CStyle }, { "javascript", LanguageCategory.CStyle }, { "js", LanguageCategory.CStyle },
            { "typescript", LanguageCategory.CStyle }, { "ts", LanguageCategory.CStyle },
            { "cpp", LanguageCategory.CStyle }, { "c++", LanguageCategory.CStyle }, { "c", LanguageCategory.CStyle },
            { "php", LanguageCategory.CStyle }, { "go", LanguageCategory.CStyle }, { "rust", LanguageCategory.CStyle },
            { "json", LanguageCategory.CStyle },
            { "python", LanguageCategory.Python }, { "py", LanguageCategory.Python },
            { "bash", LanguageCategory.Shell }, { "sh", LanguageCategory.Shell }, { "shell", LanguageCategory.Shell },
            { "powershell", LanguageCategory.Shell }, { "ps1", LanguageCategory.Shell },
            { "sql", LanguageCategory.Sql },
            { "xml", LanguageCategory.Xml }, { "html", LanguageCategory.Xml }, { "yaml", LanguageCategory.Xml }, { "yml", LanguageCategory.Xml },
        };

        // Keywords for different language categories
        private static readonly HashSet<string> CStyleKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "abstract", "as", "async", "await", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event",
            "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
            "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try",
            "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var", "virtual", "void", "volatile",
            "while", "yield", "function", "let", "export", "import", "from", "extends", "implements", "constructor",
            "get", "set", "async", "package", "throws", "final", "synchronized", "transient", "native", "boolean"
        };

        private static readonly HashSet<string> PythonKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "False", "None", "True", "and", "as", "assert", "async", "await", "break", "class", "continue", "def",
            "del", "elif", "else", "except", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda",
            "nonlocal", "not", "or", "pass", "raise", "return", "try", "while", "with", "yield"
        };

        private static readonly HashSet<string> SqlKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "FROM", "WHERE", "INSERT", "UPDATE", "DELETE", "CREATE", "DROP", "ALTER", "TABLE", "INDEX",
            "VIEW", "TRIGGER", "PROCEDURE", "FUNCTION", "INTO", "VALUES", "SET", "AND", "OR", "NOT", "NULL", "IS",
            "IN", "BETWEEN", "LIKE", "ORDER", "BY", "GROUP", "HAVING", "JOIN", "LEFT", "RIGHT", "INNER", "OUTER",
            "ON", "AS", "DISTINCT", "TOP", "LIMIT", "OFFSET", "UNION", "ALL", "EXISTS", "CASE", "WHEN", "THEN",
            "ELSE", "END", "BEGIN", "COMMIT", "ROLLBACK", "TRANSACTION", "PRIMARY", "KEY", "FOREIGN", "REFERENCES"
        };

        private static readonly HashSet<string> ShellKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "if", "then", "else", "elif", "fi", "case", "esac", "for", "while", "until", "do", "done", "in",
            "function", "return", "exit", "break", "continue", "local", "export", "readonly", "declare", "typeset",
            "source", "alias", "unalias", "set", "unset", "shift", "trap", "exec", "eval", "echo", "printf", "read"
        };

        // Max width before auto-scaling (set low to always apply DPI adjustment for testing)
        private const int MaxWidthBeforeScaling = 100;
        // Target width in cm for oversized images (reduced for testing)
        private const float TargetWidthCm = 10f;

        public AsciiArtToImageService(ILogger<AsciiArtToImageService> logger)
        {
            _logger = logger;
        }

        public async Task<string> ConvertAsciiArtToImagesAsync(string markdown, string projectPath, string filePath)
        {
            if (string.IsNullOrEmpty(markdown))
                return markdown;

            try
            {
                var matches = CodeBlockRegex.Matches(markdown);
                if (matches.Count == 0)
                    return markdown;

                var result = markdown;
                var imageIndex = 0;

                // Process matches in reverse order to preserve string positions
                var matchList = new List<Match>();
                foreach (Match match in matches)
                {
                    matchList.Add(match);
                }
                matchList.Reverse();

                foreach (var match in matchList)
                {
                    var language = match.Groups[1].Value.Trim().ToLowerInvariant();
                    var codeContent = match.Groups[2].Value;

                    // Convert to image if:
                    // 1. Contains box-drawing characters (ASCII art), OR
                    // 2. Language is in the list of languages to convert (txt, bash, etc.)
                    var hasBoxDrawing = ContainsBoxDrawingChars(codeContent);
                    var isConvertibleLanguage = ShouldConvertLanguage(language);

                    if (!hasBoxDrawing && !isConvertibleLanguage)
                        continue;

                    var reason = hasBoxDrawing
                        ? "ASCII art detected"
                        : string.IsNullOrEmpty(language)
                            ? "no language specified"
                            : $"language '{language}'";
                    _logger.LogInformation($"[CodeBlockToImage] Converting code block ({reason}) in {filePath}");

                    try
                    {
                        // Generate unique filename based on content hash (include language for syntax highlighting)
                        var hash = ComputeHash(codeContent + "|" + language);
                        var imageFileName = $"ascii-art-{hash}.png";
                        var imageDir = Path.Combine(projectPath, ".md", "exports", "ascii-art");
                        var imagePath = Path.Combine(imageDir, imageFileName);

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(imageDir))
                        {
                            Directory.CreateDirectory(imageDir);
                            _logger.LogInformation($"[AsciiArt] Created directory: {imageDir}");
                        }

                        // Render image and get its width
                        int imageWidth;
                        if (!File.Exists(imagePath))
                        {
                            imageWidth = await RenderTextToImageAsync(codeContent, imagePath, language);
                            _logger.LogInformation($"[AsciiArt] Generated image: {imagePath} (width: {imageWidth}px)");
                        }
                        else
                        {
                            imageWidth = GetImageWidth(imagePath);
                            _logger.LogInformation($"[AsciiArt] Using cached image: {imagePath} (width: {imageWidth}px)");
                        }

                        // Create relative path for markdown (from project root)
                        // Pandoc runs from project root, so use relative path without leading slash
                        var relativeImagePath = Path.Combine(".md", "exports", "ascii-art", imageFileName)
                            .Replace("\\", "/");

                        // Replace code block with image reference (no Pandoc attributes - DPI is set in the image)
                        var imageMarkdown = $"![ASCII Art Diagram]({relativeImagePath})";
                        result = result.Remove(match.Index, match.Length);
                        result = result.Insert(match.Index, imageMarkdown);

                        imageIndex++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"[AsciiArt] Failed to convert code block, keeping original");
                    }
                }

                if (imageIndex > 0)
                {
                    _logger.LogInformation($"[AsciiArt] Converted {imageIndex} ASCII art blocks to images");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AsciiArt] Error processing markdown for ASCII art");
                return markdown; // Return original on error
            }
        }

        private bool ContainsBoxDrawingChars(string text)
        {
            foreach (var ch in BoxDrawingChars)
            {
                if (text.Contains(ch))
                    return true;
            }
            return false;
        }

        private bool ShouldConvertLanguage(string language)
        {
            // Convert code blocks without language (empty string)
            if (string.IsNullOrEmpty(language))
                return true;

            foreach (var lang in ImageConversionLanguages)
            {
                if (language.Equals(lang, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private string ComputeHash(string content)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                var hashBytes = sha256.ComputeHash(bytes);
                // Take first 8 bytes for shorter filename
                return BitConverter.ToString(hashBytes, 0, 8).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Renders text to a PNG image with syntax highlighting and returns the image width in pixels.
        /// </summary>
        private Task<int> RenderTextToImageAsync(string text, string outputPath, string language)
        {
            return Task.Run(() =>
            {
                int resultWidth = 0;
                // Normalize line endings
                var normalizedText = text.Replace("\r\n", "\n").Replace("\r", "\n");
                var lines = normalizedText.Split('\n');

                // Remove trailing empty lines
                var lineList = new List<string>(lines);
                while (lineList.Count > 0 && string.IsNullOrWhiteSpace(lineList[lineList.Count - 1]))
                {
                    lineList.RemoveAt(lineList.Count - 1);
                }
                lines = lineList.ToArray();

                if (lines.Length == 0)
                {
                    lines = new[] { " " }; // At least one line
                }

                // Parse code with ColorCode for syntax highlighting
                var coloredSegments = ParseCodeWithColorCode(normalizedText, language);

                // Find a suitable monospace font
                using (var typeface = FindMonospaceTypeface())
                using (var paint = new SKPaint())
                {
                    paint.Typeface = typeface;
                    paint.TextSize = FontSize;
                    paint.IsAntialias = true;
                    paint.Color = SKColors.Black;

                    // Calculate dimensions
                    float maxWidth = 0;
                    foreach (var line in lines)
                    {
                        var lineWidth = paint.MeasureText(line.Length > 0 ? line : " ");
                        maxWidth = Math.Max(maxWidth, lineWidth);
                    }

                    var fontMetrics = paint.FontMetrics;
                    var lineHeightPx = FontSize * LineHeight;
                    var textHeight = lines.Length * lineHeightPx;

                    // Add padding
                    var imageWidth = (int)Math.Ceiling(maxWidth + Padding * 2);
                    var imageHeight = (int)Math.Ceiling(textHeight + Padding * 2);

                    // Ensure minimum dimensions
                    imageWidth = Math.Max(imageWidth, 100);
                    imageHeight = Math.Max(imageHeight, 50);
                    resultWidth = imageWidth;

                    // Create bitmap and canvas
                    using (var bitmap = new SKBitmap(imageWidth, imageHeight))
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        // Fill background with light gray (like code blocks)
                        canvas.Clear(new SKColor(245, 245, 245)); // #f5f5f5

                        // Draw border
                        using (var borderPaint = new SKPaint())
                        {
                            borderPaint.Style = SKPaintStyle.Stroke;
                            borderPaint.Color = new SKColor(200, 200, 200);
                            borderPaint.StrokeWidth = 1;
                            canvas.DrawRect(0.5f, 0.5f, imageWidth - 1, imageHeight - 1, borderPaint);
                        }

                        // Draw colored text segments
                        DrawColoredText(canvas, coloredSegments, paint, fontMetrics, lineHeightPx);

                        // Save to file
                        using (var image = SKImage.FromBitmap(bitmap))
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                        using (var stream = File.OpenWrite(outputPath))
                        {
                            data.SaveTo(stream);
                        }
                    }
                }

                // If image is wider than threshold, adjust DPI so it fits in target width
                // This preserves all pixels but tells Word to render it smaller
                if (resultWidth > MaxWidthBeforeScaling)
                {
                    AdjustImageDpiForTargetWidth(outputPath, resultWidth);
                }

                return resultWidth;
            });
        }

        /// <summary>
        /// Represents a colored segment of code for rendering.
        /// </summary>
        private class ColoredSegment
        {
            public string Text { get; set; }
            public SKColor Color { get; set; }
            public int Line { get; set; }
            public int Column { get; set; }
        }

        /// <summary>
        /// Parses code and returns a list of colored segments for syntax highlighting.
        /// Uses regex-based tokenization for common programming patterns.
        /// </summary>
        private List<ColoredSegment> ParseCodeWithColorCode(string code, string language)
        {
            var segments = new List<ColoredSegment>();

            // Get language category
            LanguageCategory category = LanguageCategory.None;
            if (!string.IsNullOrEmpty(language))
            {
                LanguageCategories.TryGetValue(language, out category);
            }

            if (category != LanguageCategory.None)
            {
                _logger.LogDebug($"[AsciiArt] Using syntax highlighting for language: {language} (category: {category})");
            }

            // Parse each line
            var lines = code.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                if (string.IsNullOrEmpty(line))
                {
                    segments.Add(new ColoredSegment { Text = "", Color = ColorDefault, Line = lineIndex, Column = 0 });
                    continue;
                }

                if (category == LanguageCategory.None)
                {
                    // No highlighting - add entire line as default color
                    segments.Add(new ColoredSegment { Text = line, Color = ColorDefault, Line = lineIndex, Column = 0 });
                }
                else
                {
                    // Tokenize and colorize the line
                    var lineSegments = TokenizeLine(line, category);
                    foreach (var seg in lineSegments)
                    {
                        seg.Line = lineIndex;
                        segments.Add(seg);
                    }
                }
            }

            return segments;
        }

        /// <summary>
        /// Tokenizes a single line of code and returns colored segments.
        /// </summary>
        private List<ColoredSegment> TokenizeLine(string line, LanguageCategory category)
        {
            var segments = new List<ColoredSegment>();
            int pos = 0;

            while (pos < line.Length)
            {
                // Skip whitespace - keep track of it
                if (char.IsWhiteSpace(line[pos]))
                {
                    int start = pos;
                    while (pos < line.Length && char.IsWhiteSpace(line[pos]))
                        pos++;
                    segments.Add(new ColoredSegment { Text = line.Substring(start, pos - start), Color = ColorDefault, Column = start });
                    continue;
                }

                // Check for comments
                var commentResult = TryParseComment(line, pos, category);
                if (commentResult.HasValue)
                {
                    segments.Add(new ColoredSegment { Text = commentResult.Value.Text, Color = ColorComment, Column = pos });
                    pos += commentResult.Value.Text.Length;
                    continue;
                }

                // Check for strings
                var stringResult = TryParseString(line, pos, category);
                if (stringResult.HasValue)
                {
                    segments.Add(new ColoredSegment { Text = stringResult.Value.Text, Color = ColorString, Column = pos });
                    pos += stringResult.Value.Text.Length;
                    continue;
                }

                // Check for numbers
                var numberResult = TryParseNumber(line, pos);
                if (numberResult.HasValue)
                {
                    segments.Add(new ColoredSegment { Text = numberResult.Value.Text, Color = ColorNumber, Column = pos });
                    pos += numberResult.Value.Text.Length;
                    continue;
                }

                // Check for words (keywords, identifiers)
                var wordResult = TryParseWord(line, pos, category);
                if (wordResult.HasValue)
                {
                    segments.Add(new ColoredSegment { Text = wordResult.Value.Text, Color = wordResult.Value.Color, Column = pos });
                    pos += wordResult.Value.Text.Length;
                    continue;
                }

                // Check for XML/HTML tags
                if (category == LanguageCategory.Xml)
                {
                    var tagResult = TryParseXmlTag(line, pos);
                    if (tagResult.HasValue)
                    {
                        segments.Add(new ColoredSegment { Text = tagResult.Value.Text, Color = tagResult.Value.Color, Column = pos });
                        pos += tagResult.Value.Text.Length;
                        continue;
                    }
                }

                // Default: single character as operator or punctuation
                segments.Add(new ColoredSegment { Text = line[pos].ToString(), Color = ColorDefault, Column = pos });
                pos++;
            }

            return segments;
        }

        private (string Text, SKColor Color)? TryParseComment(string line, int pos, LanguageCategory category)
        {
            // C-style single line comment
            if ((category == LanguageCategory.CStyle || category == LanguageCategory.Sql) &&
                pos + 1 < line.Length && line[pos] == '/' && line[pos + 1] == '/')
            {
                return (line.Substring(pos), ColorComment);
            }

            // SQL single line comment
            if (category == LanguageCategory.Sql && pos + 1 < line.Length && line[pos] == '-' && line[pos + 1] == '-')
            {
                return (line.Substring(pos), ColorComment);
            }

            // Python/Shell comment
            if ((category == LanguageCategory.Python || category == LanguageCategory.Shell) && line[pos] == '#')
            {
                return (line.Substring(pos), ColorComment);
            }

            // XML comment start
            if (category == LanguageCategory.Xml && pos + 3 < line.Length && line.Substring(pos, 4) == "<!--")
            {
                var endPos = line.IndexOf("-->", pos + 4);
                if (endPos >= 0)
                    return (line.Substring(pos, endPos + 3 - pos), ColorComment);
                return (line.Substring(pos), ColorComment);
            }

            return null;
        }

        private (string Text, SKColor Color)? TryParseString(string line, int pos, LanguageCategory category)
        {
            char quote = line[pos];
            if (quote != '"' && quote != '\'' && quote != '`')
                return null;

            // For XML, single quotes are often attribute values
            if (category == LanguageCategory.Xml && quote == '\'')
            {
                // Parse until closing quote
            }

            int endPos = pos + 1;
            while (endPos < line.Length)
            {
                if (line[endPos] == '\\' && endPos + 1 < line.Length)
                {
                    endPos += 2; // Skip escaped character
                    continue;
                }
                if (line[endPos] == quote)
                {
                    return (line.Substring(pos, endPos - pos + 1), ColorString);
                }
                endPos++;
            }

            // Unclosed string - return rest of line
            return (line.Substring(pos), ColorString);
        }

        private (string Text, SKColor Color)? TryParseNumber(string line, int pos)
        {
            if (!char.IsDigit(line[pos]) && line[pos] != '.')
                return null;

            // Don't match dot alone
            if (line[pos] == '.' && (pos + 1 >= line.Length || !char.IsDigit(line[pos + 1])))
                return null;

            int endPos = pos;

            // Handle hex numbers
            if (pos + 1 < line.Length && line[pos] == '0' && (line[pos + 1] == 'x' || line[pos + 1] == 'X'))
            {
                endPos = pos + 2;
                while (endPos < line.Length && (char.IsDigit(line[endPos]) ||
                       (line[endPos] >= 'a' && line[endPos] <= 'f') ||
                       (line[endPos] >= 'A' && line[endPos] <= 'F')))
                    endPos++;
            }
            else
            {
                // Regular number (including floats)
                while (endPos < line.Length && (char.IsDigit(line[endPos]) || line[endPos] == '.' ||
                       line[endPos] == 'e' || line[endPos] == 'E' || line[endPos] == '-' || line[endPos] == '+'))
                {
                    // Handle scientific notation
                    if ((line[endPos] == '-' || line[endPos] == '+') && endPos > pos &&
                        line[endPos - 1] != 'e' && line[endPos - 1] != 'E')
                        break;
                    endPos++;
                }
            }

            // Handle suffixes like 'f', 'L', 'UL', etc.
            while (endPos < line.Length && (line[endPos] == 'f' || line[endPos] == 'F' ||
                   line[endPos] == 'l' || line[endPos] == 'L' || line[endPos] == 'u' || line[endPos] == 'U' ||
                   line[endPos] == 'd' || line[endPos] == 'D' || line[endPos] == 'm' || line[endPos] == 'M'))
                endPos++;

            if (endPos > pos)
                return (line.Substring(pos, endPos - pos), ColorNumber);

            return null;
        }

        private (string Text, SKColor Color)? TryParseWord(string line, int pos, LanguageCategory category)
        {
            if (!char.IsLetter(line[pos]) && line[pos] != '_' && line[pos] != '@')
                return null;

            int endPos = pos;
            while (endPos < line.Length && (char.IsLetterOrDigit(line[endPos]) || line[endPos] == '_'))
                endPos++;

            var word = line.Substring(pos, endPos - pos);
            var color = GetWordColor(word, category);

            return (word, color);
        }

        private SKColor GetWordColor(string word, LanguageCategory category)
        {
            switch (category)
            {
                case LanguageCategory.CStyle:
                    if (CStyleKeywords.Contains(word))
                        return ColorKeyword;
                    // Check if it looks like a type (starts with uppercase)
                    if (char.IsUpper(word[0]) && word.Length > 1)
                        return ColorType;
                    break;

                case LanguageCategory.Python:
                    if (PythonKeywords.Contains(word))
                        return ColorKeyword;
                    // Python built-in types/functions
                    if (word == "self" || word == "cls")
                        return ColorKeyword;
                    break;

                case LanguageCategory.Shell:
                    if (ShellKeywords.Contains(word))
                        return ColorKeyword;
                    break;

                case LanguageCategory.Sql:
                    if (SqlKeywords.Contains(word))
                        return ColorKeyword;
                    break;

                case LanguageCategory.Xml:
                    // XML doesn't have keywords in the traditional sense
                    break;
            }

            return ColorDefault;
        }

        private (string Text, SKColor Color)? TryParseXmlTag(string line, int pos)
        {
            if (line[pos] != '<')
                return null;

            int endPos = line.IndexOf('>', pos);
            if (endPos < 0)
                return (line.Substring(pos), ColorType);

            return (line.Substring(pos, endPos - pos + 1), ColorType);
        }

        /// <summary>
        /// Draws colored text segments on the canvas.
        /// </summary>
        private void DrawColoredText(SKCanvas canvas, List<ColoredSegment> segments, SKPaint paint, SKFontMetrics fontMetrics, float lineHeightPx)
        {
            // Group segments by line
            var lineGroups = segments.GroupBy(s => s.Line).OrderBy(g => g.Key);

            foreach (var lineGroup in lineGroups)
            {
                var y = Padding - fontMetrics.Ascent + (lineGroup.Key * lineHeightPx);

                // Sort segments by column within each line
                var lineSegments = lineGroup.OrderBy(s => s.Column).ToList();

                foreach (var segment in lineSegments)
                {
                    if (string.IsNullOrEmpty(segment.Text))
                        continue;

                    // Calculate X position based on column
                    float x = Padding;
                    if (segment.Column > 0)
                    {
                        // Measure the text before this segment to get the X position
                        var textBefore = string.Join("", lineSegments
                            .Where(s => s.Column < segment.Column)
                            .Select(s => s.Text));
                        x = Padding + paint.MeasureText(textBefore);
                    }

                    paint.Color = segment.Color;
                    canvas.DrawText(segment.Text, x, y, paint);
                }
            }
        }

        /// <summary>
        /// Adjusts the DPI metadata of a PNG image so that it renders at the target width.
        /// This preserves all pixels but changes how Word interprets the physical size.
        /// Formula: DPI = pixels / inches, where inches = TargetWidthCm / 2.54
        /// </summary>
        [SupportedOSPlatform("windows")]
        private void AdjustImageDpiForTargetWidth(string imagePath, int pixelWidth)
        {
#if WINDOWS
            try
            {
                // Calculate required DPI to fit in target width
                // DPI = pixels / inches = pixels / (cm / 2.54)
                float targetWidthInches = TargetWidthCm / 2.54f;
                float requiredDpi = pixelWidth / targetWidthInches;

                // Load image, set DPI, and save
                using (var original = System.Drawing.Image.FromFile(imagePath))
                {
                    // Create a new bitmap to avoid file locking issues
                    using (var bmp = new Bitmap(original))
                    {
                        bmp.SetResolution(requiredDpi, requiredDpi);

                        // Save to a temp file first, then replace
                        var tempPath = imagePath + ".tmp";
                        bmp.Save(tempPath, ImageFormat.Png);

                        // Close original before replacing
                        original.Dispose();

                        // Replace original with temp
                        File.Delete(imagePath);
                        File.Move(tempPath, imagePath);

                        _logger.LogInformation($"[AsciiArt] Adjusted DPI to {requiredDpi:F0} for {pixelWidth}px -> {TargetWidthCm}cm");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AsciiArt] Failed to adjust image DPI, image may appear oversized in Word");
            }
#endif
        }

        /// <summary>
        /// Gets the width of an existing image file.
        /// </summary>
        private int GetImageWidth(string imagePath)
        {
            try
            {
                using (var stream = File.OpenRead(imagePath))
                using (var codec = SKCodec.Create(stream))
                {
                    return codec?.Info.Width ?? 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private SKTypeface FindMonospaceTypeface()
        {
            // Try preferred font first
            var typeface = SKTypeface.FromFamilyName(PreferredFont, SKFontStyle.Normal);
            if (typeface != null && typeface.FamilyName.Equals(PreferredFont, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug($"[AsciiArt] Using font: {PreferredFont}");
                return typeface;
            }

            // Try fallback fonts
            foreach (var fontName in FallbackFonts)
            {
                typeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.Normal);
                if (typeface != null)
                {
                    _logger.LogDebug($"[AsciiArt] Using fallback font: {fontName}");
                    return typeface;
                }
            }

            // Last resort: default typeface
            _logger.LogWarning("[AsciiArt] No monospace font found, using default");
            return SKTypeface.Default;
        }
    }
}
