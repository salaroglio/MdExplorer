using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Markdig;
using Markdig.Syntax;
using MdExplorer.Features.Commands;

namespace MdExplorer.Features.Diagrams
{
    /// <summary>A <c>```plantuml</c> block of a markdown text, with the file line of each source line.</summary>
    public sealed class PlantumlBlock
    {
        public PlantumlBlock(string source, int[] fileLines, int fenceLine)
        {
            Source = source;
            FileLines = fileLines;
            FenceLine = fenceLine;
        }

        /// <summary>The diagram as the jar gets it, from <c>@startuml</c> to <c>@enduml</c>.</summary>
        public string Source { get; }

        /// <summary>1-based file line of each source line: <c>FileLines[0]</c> is the first line of the diagram.</summary>
        public int[] FileLines { get; }

        /// <summary>1-based file line of the opening fence, for problems that have no line of their own.</summary>
        public int FenceLine { get; }

        public int FileLineOf(int sourceLine)
            => sourceLine >= 1 && sourceLine <= FileLines.Length ? FileLines[sourceLine - 1] : FenceLine;
    }

    /// <summary>One thing to fix in a diagram, placed in the file it was written in.</summary>
    public sealed class PlantumlBlockProblem
    {
        /// <summary>Where the diagram is: a path relative to the project, or "la tua risposta".</summary>
        public string Where { get; set; }
        public int FileLine { get; set; }
        public string Severity { get; set; }
        public string Source { get; set; }
        public string Meaning { get; set; }
        public string Fix { get; set; }
    }

    /// <summary>The outcome of checking every diagram of one or more texts.</summary>
    public sealed class PlantumlVerification
    {
        public int Diagrams { get; set; }
        public List<PlantumlBlockProblem> Problems { get; } = new List<PlantumlBlockProblem>();

        /// <summary>
        /// Non-null when the check could not run (java or the jar missing): the diagrams were NOT
        /// judged. Said to the user; never turned into "all fine" nor into a correction request.
        /// </summary>
        public string ToolUnavailable { get; set; }

        public bool HasProblems => Problems.Count > 0;

        /// <summary>
        /// What the model reads: every problem with file, line, the line itself, what it means
        /// and one fix — the words of <see cref="PlantumlCheckAnalyzer"/>, placed in the file.
        /// </summary>
        public string ForModel()
        {
            var sb = new StringBuilder();
            sb.Append("MdExplorer ha verificato i diagrammi PlantUML che hai scritto: ")
              .Append(Problems.Count == 1 ? "c'è 1 problema da correggere." : $"ci sono {Problems.Count} problemi da correggere.")
              .Append('\n');
            foreach (var p in Problems)
            {
                sb.Append("\n- ").Append(p.Where);
                if (p.FileLine > 0) sb.Append(", riga ").Append(p.FileLine);
                sb.Append(p.Severity == PlantumlCheckAnalyzer.Error
                    ? " (errore: il diagramma non viene mostrato)"
                    : " (avviso: in MdExplorer si vede sbagliato)");
                if (!string.IsNullOrWhiteSpace(p.Source)) sb.Append(": ").Append(p.Source);
                sb.Append("\n  Significa: ").Append(p.Meaning);
                sb.Append("\n  Correzione: ").Append(p.Fix);
            }
            sb.Append("\n\nCorreggi dove hai scritto il diagramma. MdExplorer lo ricontrolla da solo.");
            return sb.ToString();
        }

        /// <summary>What the user reads when problems are left: where, and what they mean.</summary>
        public string ForUser()
        {
            var sb = new StringBuilder();
            foreach (var p in Problems)
            {
                sb.Append("\n- ").Append(p.Where);
                if (p.FileLine > 0) sb.Append(", riga ").Append(p.FileLine);
                sb.Append(": ").Append(p.Meaning);
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Checks every PlantUML diagram of a markdown text — with the same code as
    /// <c>POST /api/Plantuml/Check</c> — and places each problem at its FILE line.
    ///
    /// <para>
    /// For MarkAgent: the agent writes a document, MdExplorer checks its diagrams and hands the
    /// problems back to the model, instead of hoping the model remembers to call the check.
    /// Sprint: docs-internal/Sprints/2026-09-11-MarkAgent-Verifica-PlantUML.md.
    /// </para>
    ///
    /// <para>
    /// Blocks are found by Markdig, as the page does: an example written inside a longer fence is
    /// not a block of its own, so it is not checked. <c>```plantuml(@json, …)</c> is left out: its
    /// source is a data file, not something the model wrote. Hints are left out too: a round of
    /// corrections for style is not worth it.
    /// </para>
    ///
    /// <para>
    /// Starting java costs about a second per diagram, so a source already checked is not
    /// checked again: one instance per chat session keeps the outcomes.
    /// </para>
    /// </summary>
    public sealed class PlantumlBlockVerifier
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .Build();

        private readonly Func<string, Task<PlantumlCheckOutcome>> _check;
        private readonly ConcurrentDictionary<string, PlantumlCheckOutcome> _outcomes = new();

        /// <param name="check">The jar check: <c>PlantumlServer.CheckAsync</c> in the app.</param>
        public PlantumlBlockVerifier(Func<string, Task<PlantumlCheckOutcome>> check)
        {
            _check = check ?? throw new ArgumentNullException(nameof(check));
        }

        public static IReadOnlyList<PlantumlBlock> FindBlocks(string markdown)
        {
            var blocks = new List<PlantumlBlock>();
            if (string.IsNullOrEmpty(markdown)) return blocks;

            var document = Markdown.Parse(markdown, Pipeline);
            foreach (var fence in document.Descendants<FencedCodeBlock>())
            {
                // MdExplorer draws ```plantuml only (its regex wants backticks); ~~~plantuml stays code.
                if (fence.FencedChar != '`' || !string.Equals(fence.Info?.Trim(), "plantuml", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var lines = fence.Lines.Lines.Take(fence.Lines.Count).ToArray();
                if (lines.Length == 0) continue;

                var source = string.Join("\n", lines.Select(l => l.Slice.ToString()));
                blocks.Add(new PlantumlBlock(source, lines.Select(l => l.Line + 1).ToArray(), fence.Line + 1));
            }
            return blocks;
        }

        /// <param name="texts">Each text with the name the model knows it by (a file path, "la tua risposta").</param>
        public async Task<PlantumlVerification> VerifyAsync(IEnumerable<(string Where, string Markdown)> texts, CancellationToken ct = default)
        {
            var result = new PlantumlVerification();
            foreach (var (where, markdown) in texts)
            {
                foreach (var block in FindBlocks(markdown))
                {
                    ct.ThrowIfCancellationRequested();
                    result.Diagrams++;

                    var outcome = await CheckOnceAsync(block.Source).ConfigureAwait(false);
                    var report = PlantumlCheckAnalyzer.Analyze(block.Source, outcome);
                    if (report.ToolUnavailable != null)
                    {
                        result.ToolUnavailable = report.ToolUnavailable;
                        return result;
                    }

                    foreach (var p in report.Problems.Where(p => p.Severity != PlantumlCheckAnalyzer.Hint))
                    {
                        result.Problems.Add(new PlantumlBlockProblem
                        {
                            Where = where,
                            FileLine = block.FileLineOf(p.Line),
                            Severity = p.Severity,
                            Source = p.Source,
                            Meaning = p.Meaning,
                            Fix = p.Fix
                        });
                    }
                }
            }
            return result;
        }

        private async Task<PlantumlCheckOutcome> CheckOnceAsync(string source)
        {
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
            if (_outcomes.TryGetValue(key, out var known)) return known;

            var outcome = await _check(source).ConfigureAwait(false);
            // "Could not run" is not an answer about the diagram: next time, try again.
            if (outcome.ToolUnavailable == null) _outcomes[key] = outcome;
            return outcome;
        }
    }
}
