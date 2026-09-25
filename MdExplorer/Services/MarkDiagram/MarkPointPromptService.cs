using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services.MarkPoint;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.MarkDiagram
{
    /// <summary>The prompt of the second phase, with what the search found.</summary>
    public sealed record MarkPointAnswerPrompt(string Prompt, IReadOnlyList<string> Keywords, IReadOnlyList<string> Sources);

    /// <summary>
    /// The deterministic half of "Chiedi a MarkAgent" on a point of a slide: the prompts and the
    /// search. The conversation is the client's, as in Mark Search: it sends both prompts on a
    /// channel of the AI chat (<c>AiChatService.sendMessageToChannel</c>), so they go into the
    /// same CLI session as the MarkAgent tab — the tab knows what was asked about the slides, and
    /// the explanation knows what was done in the tab (user's decision, 25/09/2026).
    ///
    /// Between the two phases the client hands back MarkAgent's first answer: the keywords are
    /// read here, the search runs here (<see cref="ISearchService.SearchAsync"/>, the function
    /// behind <c>api/search/quick</c> that Mark Search calls), the passages are read here.
    ///
    /// Sprint: docs-internal/Sprints/2026-09-25-Slide-Chiedi-A-MarkAgent.md
    /// </summary>
    public class MarkPointPromptService
    {
        /// <summary>Most results per keyword: the number Mark Search asks <c>quickSearch</c> for.</summary>
        private const int SearchResultsPerKeyword = 24;

        private readonly ISearchService _search;
        private readonly ILogger<MarkPointPromptService> _logger;

        public MarkPointPromptService(ISearchService search, ILogger<MarkPointPromptService> logger)
        {
            _search = search;
            _logger = logger;
        }

        public string KeywordsPrompt(MarkDiagramContextDto context, string? question)
            => MarkPointPromptBuilder.BuildKeywordsPrompt(context, question);

        /// <exception cref="FormatException">MarkAgent's first answer does not hold the keywords as asked.</exception>
        public async Task<MarkPointAnswerPrompt> AnswerPromptAsync(
            MarkDiagramContextDto context, string keywordsAnswer, string? question, string projectPath)
        {
            var keywords = MarkSearchKeywords.Parse(keywordsAnswer);
            var linked = ReadLinkedDocuments(context, projectPath);
            var deck = MarkDiagramExplainService.ResolveDocumentPath(context, projectPath);

            var sources = (IReadOnlyList<ProjectSource>)Array.Empty<ProjectSource>();
            if (keywords.Count > 0)
            {
                var searches = new List<(string Keyword, SearchResult Result)>();
                foreach (var keyword in keywords)
                {
                    searches.Add((keyword, await _search.SearchAsync(keyword, SearchType.All, SearchResultsPerKeyword, projectPath)));
                }
                // The deck and the linked documents are given whole: not again as passages.
                var given = linked.Select(d => d.Path).Append(deck).Where(p => p != null).ToList();
                sources = ProjectSearchDigest.Build(searches, projectPath, given, relative => ReadProjectFile(projectPath, relative),
                    // What the agentic environment installs (skills, agents, commands) is MdExplorer's
                    // documentation, not the project's.
                    HarnessLayout.All.SelectMany(l => new[] { l.SkillsFolder, l.AgentsFolder, l.PromptsFolder }).ToList());
            }

            var deckText = ReadDeck(context, projectPath, out var truncated);
            var prompt = MarkPointPromptBuilder.BuildSystemPrompt() + "\n\n---\n\n"
                       + MarkPointPromptBuilder.BuildAnswerPrompt(context, keywords, sources, deckText, truncated, question, linked);

            return new MarkPointAnswerPrompt(prompt, keywords,
                linked.Where(d => d.Text != null).Select(d => d.Path).Concat(sources.Select(s => s.Path)).ToList());
        }

        /// <summary>
        /// The documents the point links to (<see cref="MarkPoint.Links"/>), read whole. A path outside the
        /// project or a missing file stays in the list without text: the prompt says it could not be read.
        /// </summary>
        private List<LinkedDocument> ReadLinkedDocuments(MarkDiagramContextDto context, string projectPath)
        {
            var result = new List<LinkedDocument>();
            foreach (var relative in (context.Point?.Links ?? new List<string>())
                         .Where(l => !string.IsNullOrWhiteSpace(l))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .Take(MarkPointPromptBuilder.MaxLinkedDocuments))
            {
                var path = relative.Replace('\\', '/').TrimStart('/');
                var text = ReadProjectFile(projectPath, path);
                var truncated = text != null && text.Length > MarkPointPromptBuilder.MaxDeckChars;
                if (truncated) text = text!.Substring(0, MarkPointPromptBuilder.MaxDeckChars);
                if (text == null) _logger.LogWarning("[MarkPoint] Documento collegato non leggibile: {Path}", path);
                result.Add(new LinkedDocument(path, text, truncated));
            }
            return result;
        }

        private string? ReadProjectFile(string projectPath, string relative)
        {
            var full = Path.GetFullPath(Path.Combine(projectPath, relative));
            if (!MarkDiagramExplainService.IsInsideProject(full, projectPath) || !File.Exists(full)) return null;
            try
            {
                return File.ReadAllText(full);
            }
            catch (Exception ex)
            {
                // The file stays among the found ones, without passages: the prompt tells the model.
                _logger.LogWarning(ex, "[MarkPoint] Non riesco a leggere {Path}", full);
                return null;
            }
        }

        /// <summary>The deck, given whole up to <see cref="MarkPointPromptBuilder.MaxDeckChars"/>. Empty when it cannot be read: the prompt says so.</summary>
        private string ReadDeck(MarkDiagramContextDto context, string projectPath, out bool truncated)
        {
            truncated = false;
            var full = MarkDiagramExplainService.ResolveDocumentPath(context, projectPath);
            if (full == null || !File.Exists(full))
            {
                _logger.LogWarning("[MarkPoint] Presentazione non leggibile: {Path}", context.DocumentPath);
                return string.Empty;
            }
            var text = File.ReadAllText(full);
            if (text.Length > MarkPointPromptBuilder.MaxDeckChars)
            {
                truncated = true;
                text = text.Substring(0, MarkPointPromptBuilder.MaxDeckChars);
            }
            return text;
        }
    }
}
