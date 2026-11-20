using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il pezzo di grafo che una domanda da un punto percorre (pacchetto 3, 20/11/2026). data-reach, who-reaches e
    /// check-claims leggevano TUTTI gli archi della navigazione (milioni, minuti dal cliente) per seguirne poche centinaia.
    /// Qui si parte dal simbolo e si scaricano, livello per livello, solo gli archi dei nodi raggiunti: gli algoritmi di
    /// <see cref="CodeNavigation"/> restano gli stessi e su questo pezzo vedono quello che vedrebbero sul grafo intero,
    /// perché ogni nodo raggiunto ha TUTTI i suoi archi nella direzione della visita (anche quelli dell'ultimo livello: servono
    /// a contare chi ha ancora strada e a riconoscere foglie e ingressi). Il costo segue la risposta, non il grafo.
    /// </summary>
    public static class CodeNavNeighborhood
    {
        /// <summary>
        /// In avanti da <paramref name="start"/>: gli archi in uscita di ogni nodo raggiunto entro <paramref name="maxDepth"/>
        /// passi seguendo le relazioni di flusso (come <see cref="CodeNavigation.From"/> e <see cref="CodeNavigation.PathTo"/>).
        /// <c>int.MaxValue</c> = fin dove si arriva.
        /// </summary>
        public static Task<CodeNavigation.Graph> ForwardAsync(Func<IReadOnlyCollection<string>, Task<IReadOnlyList<CodeNavEdge>>> outEdges, string start, int maxDepth)
            => LoadAsync(outEdges, start, maxDepth, e => CodeNavigation.Flow.Contains(e.Relation), e => e.Target);

        /// <summary>All'indietro verso <paramref name="target"/>: gli archi in entrata di ogni nodo raggiunto (come <see cref="CodeNavigation.To"/>).</summary>
        public static Task<CodeNavigation.Graph> BackwardAsync(Func<IReadOnlyCollection<string>, Task<IReadOnlyList<CodeNavEdge>>> inEdges, string target, int maxDepth)
            => LoadAsync(inEdges, target, maxDepth, _ => true, e => e.Source);

        private static async Task<CodeNavigation.Graph> LoadAsync(Func<IReadOnlyCollection<string>, Task<IReadOnlyList<CodeNavEdge>>> edgesOf, string from, int maxDepth,
            Func<CodeNavEdge, bool> follow, Func<CodeNavEdge, string> other)
        {
            var edges = new List<CodeNavEdge>();
            var seen = new HashSet<string>(StringComparer.Ordinal) { from };
            var frontier = new List<string> { from };
            for (var depth = 0; frontier.Count > 0 && depth <= maxDepth; depth++)
            {
                var got = await edgesOf(frontier);
                edges.AddRange(got);
                // si va avanti sugli stessi archi che la visita conta (contains solo verso un controllo)
                frontier = got.Where(e => CodeNavigation.Counts(e) && follow(e)).Select(other).Where(seen.Add).ToList();
            }
            return new CodeNavigation.Graph(edges);
        }
    }
}
