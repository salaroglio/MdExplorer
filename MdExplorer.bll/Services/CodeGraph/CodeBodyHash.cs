using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'impronta del corpo di un simbolo (F12b): prima una descrizione si rifaceva solo se cambiava la FIRMA, e un corpo
    /// riscritto lasciava la descrizione vecchia senza dirlo. Calcolata in un posto solo, dopo l'estrazione, dalle righe
    /// del simbolo nel sorgente: vale per tutti i linguaggi senza toccare gli estrattori. Gli spazi si compattano (un
    /// rientro rifatto non cambia il significato); i commenti contano (possono cambiarlo).
    /// Solo per chi ha un corpo che si esegue: un tipo non ne ha una sua, la sua freschezza dipende dai membri
    /// (impronta del contesto, <see cref="CodeDocGraph.ContextHash"/>).
    /// </summary>
    public static class CodeBodyHash
    {
        private static readonly HashSet<string> WithBody = new(StringComparer.Ordinal) { "method", "constructor", "function", "property" };
        private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);

        public static bool HasBody(string kind) => WithBody.Contains(kind);

        /// <summary>
        /// La versione scritta negli script AST: quella dell'estrattore più quella di questo passaggio. L'hash del sorgente
        /// non dice se uno script ha l'impronta (regola: serve la versione di chi lo emette): con questo suffisso gli script
        /// di prima del 21/10/2026 risultano «di un altro estrattore» e «Salva e analizza» li rifà.
        /// </summary>
        public static string EmitterVersion(string extractorVersion) => extractorVersion + "+body1";

        public static void Apply(CodeScript script, string sourceText)
        {
            var lines = sourceText.Replace("\r\n", "\n").Split('\n');
            foreach (var s in script.Symbols)
            {
                if (!WithBody.Contains(s.Kind) || s.LineStart == null) continue;
                var from = Math.Max(1, s.LineStart.Value);
                var to = Math.Min(lines.Length, Math.Max(from, s.LineEnd ?? from));
                var text = Spaces.Replace(string.Join("\n", lines.Skip(from - 1).Take(to - from + 1)), " ").Trim();
                s.BodyHash = Hash(text);
            }
        }

        public static string Hash(string text)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)), 0, 16).ToLowerInvariant();
        }
    }
}
