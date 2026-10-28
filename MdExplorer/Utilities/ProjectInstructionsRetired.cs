using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Le frasi che MDE scriveva nel file d'istruzioni del progetto (<c>CLAUDE.md</c>, <c>AGENTS.md</c>,
    /// <c>.github/copilot-instructions.md</c>) e che oggi dicono il falso. Il file si crea una volta sola: senza questa
    /// correzione un progetto nato prima continuerebbe a portarle. Si sostituisce solo il testo esatto scritto da MDE,
    /// mai il resto del file, che è del gruppo.
    /// </summary>
    public static class ProjectInstructionsRetired
    {
        /// <summary>
        /// Fino al 21/10/2026 la frase su <c>mde-doc</c> chiedeva un «<c>.mde-doc/&lt;docname&gt;.kg.md</c> co-locato»: un formato
        /// che MDE non legge più, e che la skill non descrive, quindi l'AI lo inventava a ogni documento.
        /// </summary>
        private static readonly Regex KgMdSentence = new(
            @"Le regole di documentazione \(TL;DR, niente codice nei doc di analisi, fence ```plantuml, generazione del file `\.mde-doc/<docname>\.kg\.md` co-locato\) sono descritte nella skill `mde-doc` installata in `(?<path>[^`]+)`\. Consultala ogni volta che scrivi o aggiorni un documento `\.md` di analisi/design/sprint/how-to\. Le regole per i README runnable sono nella skill `mde-readme`\.",
            RegexOptions.Compiled);

        /// <summary>La frase di oggi, con il percorso della skill del motore del progetto.</summary>
        public static string MdeDocSentence(string skillPath)
            => $"La regola dei documenti (in testa un TL;DR che basta da solo) è nella skill `mde-doc` installata in `{skillPath}`: consultala ogni volta che scrivi o aggiorni un documento `.md` di analisi/design/sprint/how-to. I diagrammi seguono la skill `mde-plantuml`, i README runnable la skill `mde-readme`.";

        /// <summary>Il testo con le frasi ritirate sostituite; lo stesso testo se non ne ha.</summary>
        public static string Apply(string text)
            => text == null ? null : KgMdSentence.Replace(text, m => MdeDocSentence(m.Groups["path"].Value));

        /// <summary>
        /// Corregge il file, se ha una frase ritirata; true se l'ha riscritto. Il file si riscrive con la codifica che aveva:
        /// un BOM UTF-8 in testa (lo mettono alcuni editor di Windows) resta, altrimenti git mostrerebbe cambiata anche la
        /// prima riga di un testo che per il resto è identico.
        /// </summary>
        public static bool ApplyToFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            var before = Encoding.UTF8.GetString(bytes, bom ? 3 : 0, bytes.Length - (bom ? 3 : 0));
            var after = Apply(before);
            if (after == before) return false;
            File.WriteAllText(path, after, new UTF8Encoding(encoderShouldEmitUTF8Identifier: bom));
            return true;
        }
    }
}
