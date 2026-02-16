using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services;

namespace MdExplorer.Features.Services.AI
{
    public class MarkdownChunkingService : IMarkdownChunkingService
    {
        private readonly IEmbeddingConfigService _configService;
        private static readonly Regex HeadingRegex = new Regex(@"^(#{1,6})\s+(.+)$", RegexOptions.Compiled);
        private static readonly Regex YamlFrontMatterRegex = new Regex(@"^---\s*\n(.*?)\n---\s*\n", RegexOptions.Singleline | RegexOptions.Compiled);

        private int MaxChunkChars
        {
            get
            {
                try
                {
                    return _configService?.GetConfig()?.MaxChunkChars ?? 2000;
                }
                catch
                {
                    return 2000;
                }
            }
        }

        public MarkdownChunkingService(IEmbeddingConfigService configService)
        {
            _configService = configService;
        }

        public List<MarkdownChunk> ChunkFile(string filePath, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<MarkdownChunk>();

            var lines = content.Split('\n');

            // Extract YAML front matter as global context
            string yamlContext = null;
            var yamlMatch = YamlFrontMatterRegex.Match(content);
            if (yamlMatch.Success)
                yamlContext = yamlMatch.Groups[1].Value.Trim();

            var contextPrefix = yamlContext != null
                ? $"[File: {filePath}]\n[Metadata: {TruncateYaml(yamlContext)}]\n\n"
                : $"[File: {filePath}]\n\n";

            // Effective chunk budget = MaxChunkChars minus contextPrefix overhead
            var effectiveMaxChunkChars = Math.Max(200, MaxChunkChars - contextPrefix.Length);

            // Phase 1: Split into segments (text vs code blocks)
            var segments = ParseSegments(lines);

            // Phase 2: Convert segments to typed chunks
            var chunks = new List<MarkdownChunk>();

            foreach (var segment in segments)
            {
                if (segment.IsCodeBlock)
                {
                    var blockContent = string.Join("\n", segment.Lines).Trim();
                    if (string.IsNullOrWhiteSpace(blockContent))
                        continue;

                    var chunkType = ClassifyCodeBlock(segment.Language);
                    var title = segment.EnclosingHeading ?? $"[{chunkType}]";

                    if (blockContent.Length <= effectiveMaxChunkChars)
                    {
                        chunks.Add(new MarkdownChunk
                        {
                            FilePath = filePath,
                            SectionTitle = title,
                            Content = contextPrefix + blockContent,
                            StartLine = segment.StartLine,
                            EndLine = segment.EndLine,
                            ChunkType = chunkType
                        });
                    }
                    else
                    {
                        // Split oversized code blocks by lines
                        var subChunks = HardSplitByLines(blockContent, effectiveMaxChunkChars);
                        int lineOffset = segment.StartLine;

                        foreach (var subChunk in subChunks)
                        {
                            var subLineCount = subChunk.Count(c => c == '\n');
                            chunks.Add(new MarkdownChunk
                            {
                                FilePath = filePath,
                                SectionTitle = title,
                                Content = contextPrefix + subChunk,
                                StartLine = lineOffset,
                                EndLine = lineOffset + subLineCount,
                                ChunkType = chunkType
                            });
                            lineOffset += subLineCount + 1;
                        }
                    }
                }
                else
                {
                    // Text segment → split by headings, then by paragraphs
                    var textChunks = ChunkTextSegment(segment, filePath, contextPrefix, effectiveMaxChunkChars);
                    chunks.AddRange(textChunks);
                }
            }

            // Post-processing: assign GroupId by SectionTitle
            // All chunks (text + code) sharing the same SectionTitle get the same GroupId
            var groupIdMap = new Dictionary<string, string>();
            foreach (var chunk in chunks)
            {
                var key = chunk.SectionTitle ?? "__no_heading__";
                if (!groupIdMap.ContainsKey(key))
                    groupIdMap[key] = Guid.NewGuid().ToString("N").Substring(0, 12);
                chunk.GroupId = groupIdMap[key];
            }

            return chunks;
        }

        #region Segment Parsing

        /// <summary>
        /// Parses lines into segments: alternating text and code blocks.
        /// Code blocks are delimited by ``` markers.
        /// </summary>
        private static List<ContentSegment> ParseSegments(string[] lines)
        {
            var segments = new List<ContentSegment>();
            var currentLines = new List<string>();
            int segmentStartLine = 0;
            bool inCodeBlock = false;
            string codeBlockLanguage = null;
            string lastHeading = null;

            for (int i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].TrimEnd('\r').TrimStart();

                if (trimmed.StartsWith("```"))
                {
                    if (!inCodeBlock)
                    {
                        // Flush preceding text segment
                        if (currentLines.Count > 0)
                        {
                            segments.Add(new ContentSegment
                            {
                                IsCodeBlock = false,
                                Lines = new List<string>(currentLines),
                                StartLine = segmentStartLine,
                                EndLine = i - 1,
                                EnclosingHeading = lastHeading
                            });
                            currentLines.Clear();
                        }

                        // Start code block
                        inCodeBlock = true;
                        codeBlockLanguage = trimmed.Length > 3 ? trimmed.Substring(3).Trim().ToLower() : null;
                        segmentStartLine = i;
                        currentLines.Add(lines[i]);
                    }
                    else
                    {
                        // End code block
                        currentLines.Add(lines[i]);
                        segments.Add(new ContentSegment
                        {
                            IsCodeBlock = true,
                            Language = codeBlockLanguage,
                            Lines = new List<string>(currentLines),
                            StartLine = segmentStartLine,
                            EndLine = i,
                            EnclosingHeading = lastHeading
                        });
                        currentLines.Clear();
                        inCodeBlock = false;
                        segmentStartLine = i + 1;
                    }
                }
                else
                {
                    // Track headings for context
                    if (!inCodeBlock)
                    {
                        var headingMatch = HeadingRegex.Match(lines[i].TrimEnd('\r'));
                        if (headingMatch.Success)
                            lastHeading = headingMatch.Groups[2].Value.Trim();
                    }
                    currentLines.Add(lines[i]);
                }
            }

            // Flush remaining
            if (currentLines.Count > 0)
            {
                segments.Add(new ContentSegment
                {
                    IsCodeBlock = inCodeBlock,
                    Language = inCodeBlock ? codeBlockLanguage : null,
                    Lines = new List<string>(currentLines),
                    StartLine = segmentStartLine,
                    EndLine = lines.Length - 1,
                    EnclosingHeading = lastHeading
                });
            }

            return segments;
        }

        /// <summary>
        /// Classifies a code block language into a chunk type.
        /// </summary>
        private static string ClassifyCodeBlock(string language)
        {
            if (string.IsNullOrEmpty(language))
                return "codeblock";

            // PlantUML variants
            if (language == "plantuml" || language == "puml" || language == "uml"
                || language.StartsWith("plantuml") || language.StartsWith("puml"))
                return "plantuml";

            return "codeblock";
        }

        #endregion

        #region Text Chunking

        /// <summary>
        /// Chunks a text segment by headings and paragraphs. Same logic as before,
        /// but only applies to prose (no code blocks to worry about).
        /// </summary>
        private List<MarkdownChunk> ChunkTextSegment(ContentSegment segment, string filePath, string contextPrefix, int maxChars)
        {
            var chunks = new List<MarkdownChunk>();
            var lines = segment.Lines;

            // Split into sub-sections by headings
            var sections = new List<SectionInfo>();
            var currentSection = new SectionInfo
            {
                Title = segment.EnclosingHeading,
                StartLine = segment.StartLine,
                Lines = new List<string>()
            };

            for (int i = 0; i < lines.Count; i++)
            {
                var globalLine = segment.StartLine + i;
                var headingMatch = HeadingRegex.Match(lines[i].TrimEnd('\r'));

                if (headingMatch.Success)
                {
                    if (currentSection.Lines.Count > 0)
                    {
                        currentSection.EndLine = globalLine - 1;
                        sections.Add(currentSection);
                    }

                    currentSection = new SectionInfo
                    {
                        Title = headingMatch.Groups[2].Value.Trim(),
                        StartLine = globalLine,
                        Lines = new List<string> { lines[i] }
                    };
                }
                else
                {
                    currentSection.Lines.Add(lines[i]);
                }
            }

            if (currentSection.Lines.Count > 0)
            {
                currentSection.EndLine = segment.EndLine;
                sections.Add(currentSection);
            }

            // Convert sections to chunks
            foreach (var section in sections)
            {
                var sectionText = string.Join("\n", section.Lines).Trim();

                if (string.IsNullOrWhiteSpace(sectionText))
                    continue;

                // Skip pure YAML front matter
                if (sectionText.StartsWith("---") && sectionText.TrimEnd().EndsWith("---")
                    && sectionText.IndexOf("---", 3) > 0)
                    continue;

                if (sectionText.Length <= maxChars)
                {
                    chunks.Add(new MarkdownChunk
                    {
                        FilePath = filePath,
                        SectionTitle = section.Title,
                        Content = contextPrefix + sectionText,
                        StartLine = section.StartLine,
                        EndLine = section.EndLine,
                        ChunkType = "document"
                    });
                }
                else
                {
                    var subChunks = SplitByParagraphs(sectionText, maxChars);
                    int lineOffset = section.StartLine;

                    foreach (var subChunk in subChunks)
                    {
                        var subLineCount = subChunk.Count(c => c == '\n');
                        chunks.Add(new MarkdownChunk
                        {
                            FilePath = filePath,
                            SectionTitle = section.Title,
                            Content = contextPrefix + subChunk,
                            StartLine = lineOffset,
                            EndLine = lineOffset + subLineCount,
                            ChunkType = "document"
                        });
                        lineOffset += subLineCount + 1;
                    }
                }
            }

            return chunks;
        }

        #endregion

        #region Splitting Helpers

        private static List<string> SplitByParagraphs(string text, int maxChars)
        {
            var result = new List<string>();
            var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.None);
            var currentChunk = new StringBuilder();

            foreach (var para in paragraphs)
            {
                if (para.Length > maxChars)
                {
                    if (currentChunk.Length > 0)
                    {
                        result.Add(currentChunk.ToString().Trim());
                        currentChunk.Clear();
                    }
                    foreach (var piece in HardSplitByLines(para, maxChars))
                        result.Add(piece.Trim());
                    continue;
                }

                if (currentChunk.Length + para.Length + 2 > maxChars && currentChunk.Length > 0)
                {
                    result.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                }
                if (currentChunk.Length > 0)
                    currentChunk.Append("\n\n");
                currentChunk.Append(para);
            }

            if (currentChunk.Length > 0)
                result.Add(currentChunk.ToString().Trim());

            return result;
        }

        private static List<string> HardSplitByLines(string text, int maxChars)
        {
            var result = new List<string>();
            var lines = text.Split('\n');
            var current = new StringBuilder();

            foreach (var line in lines)
            {
                if (current.Length + line.Length + 1 > maxChars && current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                if (current.Length > 0)
                    current.Append('\n');
                current.Append(line);
            }

            if (current.Length > 0)
                result.Add(current.ToString());

            return result;
        }

        private static string TruncateYaml(string yaml)
        {
            if (yaml.Length <= 200) return yaml;
            return yaml.Substring(0, 200) + "...";
        }

        #endregion

        #region Internal Types

        private class ContentSegment
        {
            public bool IsCodeBlock { get; set; }
            public string Language { get; set; }
            public List<string> Lines { get; set; }
            public int StartLine { get; set; }
            public int EndLine { get; set; }
            public string EnclosingHeading { get; set; }
        }

        private class SectionInfo
        {
            public string Title { get; set; }
            public int StartLine { get; set; }
            public int EndLine { get; set; }
            public List<string> Lines { get; set; }
        }

        #endregion
    }
}
