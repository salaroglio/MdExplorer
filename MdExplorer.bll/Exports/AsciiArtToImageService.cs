using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
                    var codeContent = match.Groups[2].Value;

                    if (!ContainsBoxDrawingChars(codeContent))
                        continue;

                    _logger.LogInformation($"[AsciiArt] Found ASCII art block in {filePath}, converting to image...");

                    try
                    {
                        // Generate unique filename based on content hash
                        var hash = ComputeHash(codeContent);
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
                            imageWidth = await RenderTextToImageAsync(codeContent, imagePath);
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
        /// Renders text to a PNG image and returns the image width in pixels.
        /// </summary>
        private Task<int> RenderTextToImageAsync(string text, string outputPath)
        {
            return Task.Run(() =>
            {
                int resultWidth = 0;
                // Normalize line endings and split into lines
                var lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

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

                        // Draw each line of text
                        var y = Padding - fontMetrics.Ascent;
                        foreach (var line in lines)
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                canvas.DrawText(line, Padding, y, paint);
                            }
                            y += lineHeightPx;
                        }

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
        /// Adjusts the DPI metadata of a PNG image so that it renders at the target width.
        /// This preserves all pixels but changes how Word interprets the physical size.
        /// Formula: DPI = pixels / inches, where inches = TargetWidthCm / 2.54
        /// </summary>
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
