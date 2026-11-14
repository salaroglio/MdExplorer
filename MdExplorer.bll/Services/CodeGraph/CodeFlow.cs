using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Un passo del flusso: da chi, per quale relazione, verso chi, a quale profondità.</summary>
    public sealed class CodeFlowStep
    {
        public int Depth { get; init; }
        public string From { get; init; }
        public CodeEdgeView Edge { get; init; }
    }

    /// <summary>
    /// Il flusso da un simbolo (D43 F2, <c>code_query flow-from</c>): livello per livello seguendo solo le relazioni che
    /// eseguono — chiamate, smistamenti per stringa, implementazioni registrate, istanze, pagine — mai i riferimenti ai tipi.
    /// Ogni simbolo si espande una volta; gli esterni sono foglie.
    /// </summary>
    public static class CodeFlow
    {
        public static readonly IReadOnlyCollection<string> Relations = new HashSet<string>(StringComparer.Ordinal) { "calls", "dispatches", "resolvesTo", "instantiates", "renders", "includes" };

        public static async Task<(List<CodeFlowStep> Steps, bool Truncated)> FromAsync(ICodeGraphStore store, string projectPath, CodeSymbolView start, int depth, int limit, CancellationToken ct = default)
        {
            var steps = new List<CodeFlowStep>();
            var seen = new HashSet<string>(StringComparer.Ordinal) { start.Id };
            var names = new Dictionary<string, string>(StringComparer.Ordinal) { [start.Id] = start.QualifiedName };
            var frontier = new List<string> { start.Id };
            for (var d = 1; d <= depth && frontier.Count > 0; d++)
            {
                var next = new List<string>();
                foreach (var from in frontier)
                    foreach (var e in (await store.NeighborsAsync(projectPath, from, null, "out", 1, 200, ct)).Where(e => Relations.Contains(e.Relation)))
                    {
                        if (steps.Count >= limit) return (steps, true);
                        steps.Add(new CodeFlowStep { Depth = d, From = names[from], Edge = e });
                        names[e.Other.Id] = e.Other.QualifiedName;
                        if (e.Other.Kind != "external" && seen.Add(e.Other.Id)) next.Add(e.Other.Id);
                    }
                frontier = next;
            }
            return (steps, false);
        }
    }
}
