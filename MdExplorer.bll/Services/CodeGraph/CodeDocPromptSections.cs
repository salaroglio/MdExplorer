using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Le parti di <c>mde-code-doc</c> che valgono solo per certi sorgenti (28/10/2026). La v11 aggiungeva soltanto i
    /// frammenti dei blocchi lunghi del BAL, eppure faceva «prompt più vecchio» tutte le descrizioni Java scritte con la v10:
    /// 7.851 da rivedere su un progetto, e la tentazione di spendere i token di tutto il progetto per niente.
    /// <para>
    /// Una sezione sta fra <c>&lt;!-- mde-section: nome --&gt;</c> e <c>&lt;!-- /mde-section --&gt;</c> e va all'LLM solo se la
    /// domanda la riguarda; l'intestazione del prompt dice in che versione è cambiata la base e in quale ogni sezione
    /// (<c>promptVersions: base: 10, fragments: 11</c>). La versione di una descrizione è la più alta fra la base e le
    /// sezioni che il suo file usa. Un prompt senza <c>promptVersions</c> (personalizzato, o di prima) resta com'era: una
    /// versione sola, quella di <c>mde: version</c>.
    /// </para>
    /// </summary>
    public sealed class CodeDocPromptSections
    {
        public const string Base = "base";
        public const string Fragments = "fragments";

        private static readonly Regex Block = new(@"^promptVersions:[ \t]*\r?\n((?:[ \t]+[\w-]+:[ \t]*\S+[ \t]*\r?\n)+)", RegexOptions.Multiline);
        private static readonly Regex Entry = new(@"([\w-]+):[ \t]*(\S+)");
        private static readonly Regex Section = new(@"<!--\s*mde-section:\s*(?<name>[\w-]+)\s*-->(?<body>.*?)<!--\s*/mde-section\s*-->", RegexOptions.Singleline);

        public static readonly CodeDocPromptSections None = new(new Dictionary<string, int>());

        /// <summary>Le versioni dichiarate, per nome; vuoto = il prompt non ne dichiara.</summary>
        public IReadOnlyDictionary<string, int> Versions { get; }

        private CodeDocPromptSections(IReadOnlyDictionary<string, int> versions) { Versions = versions; }

        /// <summary>Dal testo intero del prompt (intestazione compresa). Una versione che non è un numero è un errore del prompt.</summary>
        public static CodeDocPromptSections FromPromptText(string text)
        {
            var m = Block.Match(text ?? string.Empty);
            if (!m.Success) return None;
            var versions = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (Match e in Entry.Matches(m.Groups[1].Value))
                versions[e.Groups[1].Value] = int.TryParse(e.Groups[2].Value, out var v) ? v
                    : throw new FormatException($"mde-code-doc: promptVersions.{e.Groups[1].Value} non è un numero: «{e.Groups[2].Value}»");
            if (!versions.ContainsKey(Base)) throw new FormatException("mde-code-doc: promptVersions senza «base»");
            return new CodeDocPromptSections(versions);
        }

        /// <summary>Le sezioni che un sorgente usa: i frammenti solo se l'AST ne ha.</summary>
        public static IEnumerable<string> UsedBy(CodeScript ast)
        {
            if (ast?.Symbols.Any(s => s.Kind == "fragment") == true) yield return Fragments;
        }

        /// <summary>La versione del prompt per le descrizioni di <paramref name="ast"/>; senza versioni dichiarate, <paramref name="fileVersion"/>.</summary>
        public string VersionFor(string fileVersion, CodeScript ast)
        {
            if (fileVersion == null || !Versions.TryGetValue(Base, out var version)) return fileVersion;
            foreach (var name in UsedBy(ast))
                if (Versions.TryGetValue(name, out var v) && v > version) version = v;
            return version.ToString();
        }

        /// <summary>Il testo per una domanda su <paramref name="asts"/>: le sezioni che la riguardano senza i marcatori, le altre tolte.</summary>
        public static string Keep(string body, IEnumerable<CodeScript> asts)
        {
            if (!Section.IsMatch(body)) return body;
            var used = new HashSet<string>(asts.SelectMany(UsedBy), StringComparer.Ordinal);
            var kept = Section.Replace(body, m => used.Contains(m.Groups["name"].Value) ? m.Groups["body"].Value.Trim('\r', '\n') : string.Empty);
            return Regex.Replace(kept, @"(\r?\n){3,}", "\n\n");
        }
    }
}
