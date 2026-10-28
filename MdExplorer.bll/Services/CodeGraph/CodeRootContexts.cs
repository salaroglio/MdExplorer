using MdExplorer.Features.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Le regole dei contesti di una radice esterna (P2 di 2026-10-27-Grafo-Di-Progetti-Enormi, G3/G5): sottocartelle marcate
    /// <c>program</c> per analizzare a pezzi una radice enorme. Decise dall'utente il 28/10/2026:
    /// <list type="bullet">
    /// <item>A — o la radice intera è un programma (com'era, e resta così per chi l'ha già) o ha dei contesti, non tutti e due;</item>
    /// <item>B — un contesto non sta dentro un altro: lo stato della scansione e la ricarica in Fuseki sono per cartella.</item>
    /// </list>
    /// </summary>
    public static class CodeRootContexts
    {
        /// <summary>La sottocartella relativa alla radice: «/», niente «/» in testa e in coda; «..» rifiutato.</summary>
        public static string Normalize(string path)
        {
            var p = (path ?? string.Empty).Replace('\\', '/').Trim().Trim('/');
            if (p.Split('/').Any(s => s == ".." || s == "."))
                throw new ArgumentException(UiText.Say($"'{path}': un contesto è una sottocartella della radice, senza «.» né «..».", $"'{path}': a context is a subfolder of the root, without «.» or «..»."));
            return p;
        }

        /// <summary>
        /// Perché <paramref name="path"/> (<c>""</c> = la radice intera) non si può marcare, o null se si può. Marcare di nuovo
        /// ciò che è già marcato non è un errore: null.
        /// </summary>
        public static string MarkProblem(string alias, bool rootIsProgram, IEnumerable<string> contexts, string path)
        {
            var existing = contexts.Select(Normalize).ToList();
            path = Normalize(path);
            if (path.Length == 0)
                return existing.Count == 0 || rootIsProgram ? null
                    : UiText.Say($"@{alias} ha già dei contesti marcati ({string.Join(", ", existing)}): smarcali prima di marcare tutta la radice.",
                        $"@{alias} already has marked contexts ({string.Join(", ", existing)}): unmark them before marking the whole root.");
            if (rootIsProgram)
                return UiText.Say($"Tutta la radice @{alias} è già un programma: per analizzarla a pezzi smarca prima la radice.",
                    $"The whole root @{alias} is already a program: to analyse it in parts, unmark the root first.");
            if (existing.Contains(path, StringComparer.Ordinal)) return null;
            var outer = existing.FirstOrDefault(c => path.StartsWith(c + "/", StringComparison.Ordinal));
            if (outer != null)
                return UiText.Say($"@{alias}/{path} sta già dentro il contesto @{alias}/{outer}.", $"@{alias}/{path} is already inside the context @{alias}/{outer}.");
            var inner = existing.FirstOrDefault(c => c.StartsWith(path + "/", StringComparison.Ordinal));
            if (inner != null)
                return UiText.Say($"@{alias}/{path} contiene già il contesto @{alias}/{inner}: smarcalo prima.", $"@{alias}/{path} already contains the context @{alias}/{inner}: unmark it first.");
            return null;
        }
    }
}
