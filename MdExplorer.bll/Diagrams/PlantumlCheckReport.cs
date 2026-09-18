using System.Collections.Generic;

namespace MdExplorer.Features.Diagrams
{
    /// <summary>
    /// What <c>POST /api/Plantuml/Check</c> answers. The shape is designed for a SMALL model:
    /// the scarce resource is not context (we target 120k and up) but attention — every field
    /// exists so the reader never has to deduce anything.
    /// <para>
    /// Contract: docs-internal/Sprints/2026-09-01-Plantuml-Check-Api-Mcp.md §3.
    /// </para>
    /// </summary>
    public sealed class PlantumlCheckReport
    {
        /// <summary>
        /// The single boolean the caller branches on: the diagram is valid AND MdExplorer will
        /// display it. False whenever there is at least one <c>error</c>.
        /// </summary>
        public bool Ok { get; set; }

        /// <summary>Diagram type PlantUML recognised (activity, sequence, class, …), when known.</summary>
        public string DiagramType { get; set; }

        /// <summary>
        /// Set only when the check could not run (java or jar missing). When this is non-null the
        /// diagram was NOT judged: it is neither valid nor invalid, and the caller must not
        /// "fix" it.
        /// </summary>
        public string ToolUnavailable { get; set; }

        /// <summary>Most severe first, capped — see <see cref="PlantumlCheckAnalyzer.MaxProblems"/>.</summary>
        public List<PlantumlProblem> Problems { get; set; } = new List<PlantumlProblem>();
    }

    /// <summary>
    /// One thing to fix. <see cref="Fix"/> is deliberately a SINGLE action: given two, a small
    /// model performs one and considers the job done.
    /// </summary>
    public sealed class PlantumlProblem
    {
        /// <summary>
        /// <c>error</c> — the diagram will not be shown at all.
        /// <c>warning</c> — it will be shown, but wrong inside MdExplorer.
        /// <c>hint</c> — correct, but it reads badly.
        /// </summary>
        public string Severity { get; set; }

        /// <summary>1-based line inside the diagram source, or 0 when the problem has no line.</summary>
        public int Line { get; set; }

        /// <summary>
        /// The offending line, verbatim. The number alone forces the reader to count lines, and
        /// counting is exactly where a small model slips.
        /// </summary>
        public string Source { get; set; }

        /// <summary>What PlantUML said, untouched. Kept for traceability.</summary>
        public string Message { get; set; }

        /// <summary>What it actually means. This is the value MdExplorer adds over the raw jar.</summary>
        public string Meaning { get; set; }

        /// <summary>One concrete action.</summary>
        public string Fix { get; set; }
    }
}
