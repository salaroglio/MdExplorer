using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public record KgPlantUmlEdge(string From, string To, string Label);
    public record KgConceptRow(string Name);
    public record KgRelationshipRow(string From, string Type, string To);

    public class ParsedKgFile
    {
        public List<KgPlantUmlEdge> PlantUmlEdges { get; } = new();
        public List<KgConceptRow> Concepts { get; } = new();
        public List<KgRelationshipRow> Relationships { get; } = new();
    }

    /// <summary>
    /// Parses a <c>.kg.md</c> file produced under the mde-doc skill, extracting:
    /// <list type="bullet">
    ///   <item><description>PlantUML component-diagram edges (<c>[A] --&gt; [B] : label</c>) from the <c>```plantuml</c> fence;</description></item>
    ///   <item><description>Concepts table rows under <c>### Concepts</c> / <c>### Concetti</c>;</description></item>
    ///   <item><description>Relationships table rows under <c>### Relationships</c> / <c>### Relazioni</c>.</description></item>
    /// </list>
    /// Shared by <c>TocGenerationService</c> (which deduplicates and writes the per-folder
    /// aggregate <c>_aggregate.kg.md</c>) and by the Neo4j ingest service (M1.8+).
    /// </summary>
    public static class KgFileParser
    {
        private static readonly Regex PlantUmlArrowRegex = new(
            @"^\s*\[([^\]]+)\]\s*-->\s*\[([^\]]+)\]\s*(?::\s*(.+?))?\s*$",
            RegexOptions.Compiled);

        public static ParsedKgFile Parse(string content)
        {
            var result = new ParsedKgFile();
            if (string.IsNullOrEmpty(content)) return result;

            var lines = content.Split('\n');
            bool inPlantumlFence = false;
            string currentTable = null;       // "concepts" | "relationships" | null
            bool tableHeaderConsumed = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                var trimmed = line.Trim();

                if (trimmed.StartsWith("```plantuml", System.StringComparison.OrdinalIgnoreCase))
                {
                    inPlantumlFence = true;
                    continue;
                }
                if (inPlantumlFence && trimmed.StartsWith("```"))
                {
                    inPlantumlFence = false;
                    continue;
                }
                if (inPlantumlFence)
                {
                    var m = PlantUmlArrowRegex.Match(line);
                    if (m.Success)
                    {
                        var from = m.Groups[1].Value.Trim();
                        var to = m.Groups[2].Value.Trim();
                        var label = m.Groups[3].Success ? m.Groups[3].Value.Trim() : string.Empty;
                        result.PlantUmlEdges.Add(new KgPlantUmlEdge(from, to, label));
                    }
                    continue;
                }

                if (trimmed.StartsWith("## ") || trimmed == "##")
                {
                    currentTable = null;
                    tableHeaderConsumed = false;
                    continue;
                }
                if (trimmed.StartsWith("### "))
                {
                    var headingText = trimmed.TrimStart('#').Trim();
                    if (headingText.Equals("Concepts", System.StringComparison.OrdinalIgnoreCase) ||
                        headingText.Equals("Concetti", System.StringComparison.OrdinalIgnoreCase))
                    {
                        currentTable = "concepts";
                        tableHeaderConsumed = false;
                    }
                    else if (headingText.Equals("Relationships", System.StringComparison.OrdinalIgnoreCase) ||
                             headingText.Equals("Relazioni", System.StringComparison.OrdinalIgnoreCase))
                    {
                        currentTable = "relationships";
                        tableHeaderConsumed = false;
                    }
                    else
                    {
                        currentTable = null;
                    }
                    continue;
                }

                if (currentTable != null && trimmed.StartsWith("|") && trimmed.EndsWith("|"))
                {
                    bool isSeparator = trimmed.All(c => c == '-' || c == '|' || c == ' ' || c == ':');
                    if (isSeparator) continue;

                    if (!tableHeaderConsumed)
                    {
                        tableHeaderConsumed = true;
                        continue;
                    }

                    var cells = trimmed
                        .Trim('|')
                        .Split('|')
                        .Select(c => c.Trim().Replace("\\|", "|"))
                        .ToList();

                    if (currentTable == "concepts" && cells.Count >= 1)
                    {
                        var name = cells[0];
                        if (!string.IsNullOrEmpty(name))
                            result.Concepts.Add(new KgConceptRow(name));
                    }
                    else if (currentTable == "relationships" && cells.Count >= 3)
                    {
                        var from = cells[0];
                        var type = cells[1];
                        var to = cells[2];
                        if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(to))
                            result.Relationships.Add(new KgRelationshipRow(from, type, to));
                    }
                }
                else if (currentTable != null && !string.IsNullOrEmpty(trimmed))
                {
                    currentTable = null;
                    tableHeaderConsumed = false;
                }
            }

            return result;
        }
    }
}
