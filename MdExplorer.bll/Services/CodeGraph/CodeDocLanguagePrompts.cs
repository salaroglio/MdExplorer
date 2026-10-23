using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il prompt di Mark per i sorgenti di un linguaggio con pacchetto (B6b): dopo <c>mde-code-doc</c> la sezione
    /// <c>---- LINGUAGGIO &lt;nome&gt; ----</c> con il suo <c>prompts/code-doc.md</c>, una per linguaggio della domanda (un ciclo
    /// può attraversare file di linguaggi diversi). Il prompt base resta uno: frammenti, salti, completezza e formato della
    /// risposta sono del motore; il pacchetto porta solo quello che è proprio del suo linguaggio.
    /// <para>
    /// La versione del prompt di una descrizione è quella di <c>mde-code-doc</c> più l'impronta del prompt del pacchetto
    /// (<c>11+toy.1a2b3c4d</c>): cambiare il prompt di un linguaggio rende «da rivedere» solo le descrizioni dei suoi sorgenti,
    /// anche se nessuno alza una versione.
    /// </para>
    /// </summary>
    public sealed class CodeDocLanguagePrompts
    {
        private readonly CodeLanguageCatalog _catalog;

        public CodeDocLanguagePrompts(CodeLanguageCatalog catalog) { _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); }

        /// <summary>Il pacchetto del linguaggio di uno script AST, se ne ha uno con un prompt; null altrimenti.</summary>
        public LanguagePackage PackageOf(CodeScript ast)
        {
            var package = _catalog.ById(ast?.Header?.Language ?? string.Empty)?.Package;
            return package?.CodeDocPrompt == null ? null : package;
        }

        public string VersionFor(string baseVersion, CodeScript ast) => Version(baseVersion, PackageOf(ast));

        /// <summary><paramref name="baseVersion"/> null = la versione non si guarda: resta null.</summary>
        public static string Version(string baseVersion, LanguagePackage package)
            => baseVersion == null || package?.CodeDocPromptHash == null ? baseVersion : $"{baseVersion}+{package.Id}.{package.CodeDocPromptHash}";

        public string BodyFor(string skillBody, IEnumerable<CodeScript> asts)
            => WithLanguages(skillBody, asts.Select(PackageOf).Where(p => p != null));

        public static string WithLanguages(string skillBody, IEnumerable<LanguagePackage> packages)
        {
            var distinct = packages.GroupBy(p => p.Id, StringComparer.Ordinal).Select(g => g.First()).OrderBy(p => p.Id, StringComparer.Ordinal).ToList();
            if (distinct.Count == 0) return skillBody;
            var sb = new StringBuilder(skillBody.TrimEnd());
            foreach (var p in distinct)
                sb.Append("\n\n---- LINGUAGGIO ").Append(p.Language.DisplayName).Append(" ----\n")
                  .Append("Quello che serve sapere del linguaggio dei sorgenti, oltre alle regole sopra (che valgono comunque):\n\n")
                  .Append(p.CodeDocPrompt);
            return sb.ToString();
        }
    }
}
