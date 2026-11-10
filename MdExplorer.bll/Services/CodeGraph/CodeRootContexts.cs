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
    /// <item><s>B — un contesto non sta dentro un altro</s>: tolta il 10/11/2026 (F1b). I contesti si annidano e si sovrappongono:
    /// uno script si toglie solo se nessun contesto lo vuole più (<see cref="ClaimedByOther"/>), e grazie al ricollegamento
    /// fra contesti (F1a) lo script di un file è lo stesso qualunque contesto lo analizzi.</item>
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
            return null;
        }

        /// <summary>
        /// F1b (10/11/2026): uno script del file <paramref name="source"/> (percorso logico), del linguaggio
        /// <paramref name="language"/>, lo vuole un contesto diverso da <paramref name="current"/>? Sì se un altro contesto
        /// segnato contiene il file e ha scelto quel linguaggio. Allora l'analisi di <paramref name="current"/> non lo toglie:
        /// se il file sparisce, lo toglie l'analisi di chi lo vuole.
        /// </summary>
        public static bool ClaimedByOther(string source, string language, string current, IEnumerable<(string Folder, IReadOnlyCollection<string> Languages)> contexts)
            => source != null && language != null && contexts.Any(c => !string.Equals(c.Folder, current, StringComparison.Ordinal)
                && source.StartsWith(c.Folder.TrimEnd('/') + "/", StringComparison.Ordinal)
                && c.Languages.Contains(language, StringComparer.OrdinalIgnoreCase));
    }
}
