using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Il blocco di MDE nel file d'istruzioni del progetto (<c>CLAUDE.md</c>, <c>AGENTS.md</c>, <c>.github/copilot-instructions.md</c>):
    /// S4 dello sprint «File di sistema di MDE» (docs-internal/Sprints/2026-11-07-File-Di-Sistema-MDE.md). Il file è del gruppo; di MDE
    /// è solo il paragrafo sulle sue skill, fra <c>&lt;!-- mde:skill vN --&gt;</c> e <c>&lt;!-- /mde:skill --&gt;</c>, come i blocchi dei
    /// metodi (<c>TemplateInstructions</c>).
    /// <para>
    /// <b>La versione è del blocco, non di MDE</b>: si alza solo quando cambia il testo. Con la versione di MDE due colleghi con release
    /// diverse riscriverebbero il blocco a vicenda a ogni apertura; così un MDE più vecchio lascia stare il blocco di uno più nuovo, e due
    /// MDE uguali scrivono lo stesso testo (il caso di progetto B non crea più modifiche).
    /// </para>
    /// <para>
    /// Migrazione: il paragrafo «## Skill MdExplorer» scritto dalle versioni precedenti (la frase di oggi o quella ritirata il 21/10 sul
    /// <c>.kg.md</c>) diventa il blocco. Un paragrafo che il gruppo ha cambiato non è più di MDE: si lascia com'è e non si aggiunge niente.
    /// </para>
    /// </summary>
    public static class MdeInstructionsBlock
    {
        /// <summary>Alzala quando cambia il testo del blocco (<see cref="Body"/>).</summary>
        public const int Version = 1;

        public const string EndMarker = "<!-- /mde:skill -->";
        public static string StartMarker(int version) => $"<!-- mde:skill v{version} -->";

        private static readonly Regex Start = new(@"<!-- mde:skill v(?<v>\d+) -->", RegexOptions.Compiled);
        private static readonly Regex WholeBlock = new(@"<!-- mde:skill v(?<v>\d+) -->[\s\S]*?<!-- /mde:skill -->", RegexOptions.Compiled);

        /// <summary>Il blocco di MDE nel testo (marcatori compresi) e la sua versione; Success=false se non c'è. Per l'unione nel pull (S5).</summary>
        public static Match FindBlock(string text) => WholeBlock.Match(text ?? string.Empty);

        /// <summary>La versione di un blocco trovato con <see cref="FindBlock"/>.</summary>
        public static int VersionOf(Match block) => int.Parse(block.Groups["v"].Value);

        /// <summary>La frase di oggi, con il percorso della skill del motore del progetto.</summary>
        public static string MdeDocSentence(string skillPath)
            => $"La regola dei documenti (in testa un TL;DR che basta da solo) è nella skill `mde-doc` installata in `{skillPath}`: consultala ogni volta che scrivi o aggiorni un documento `.md` di analisi/design/sprint/how-to. I diagrammi seguono la skill `mde-plantuml`, i README runnable la skill `mde-readme`.";

        /// <summary>Il contenuto del blocco, marcatori esclusi (fine riga LF).</summary>
        public static string Body(string skillPath) => "## Skill MdExplorer\n\n" + MdeDocSentence(skillPath);

        /// <summary>Il blocco intero, marcatori compresi (fine riga LF).</summary>
        public static string Block(string skillPath) => StartMarker(Version) + "\n" + Body(skillPath) + "\n" + EndMarker;

        private const string PathHole = "\u0001";

        /// <summary>
        /// Le frasi scritte da MDE prima dei marcatori, come espressioni regolari col percorso della skill libero: la frase di oggi e quella
        /// ritirata il 21/10/2026 (chiedeva un <c>.mde-doc/&lt;docname&gt;.kg.md</c> che MDE non legge più).
        /// </summary>
        private static readonly Regex OldParagraph = BuildOldParagraph();

        private static Regex BuildOldParagraph()
        {
            static string Sentence(string s) => Regex.Escape(s).Replace(Regex.Escape(PathHole), "[^`\\n]+");
            var today = Sentence(MdeDocSentence(PathHole));
            var retired = Sentence("Le regole di documentazione (TL;DR, niente codice nei doc di analisi, fence ```plantuml, generazione del file `.mde-doc/<docname>.kg.md` co-locato) sono descritte nella skill `mde-doc` installata in `" + PathHole + "`. Consultala ogni volta che scrivi o aggiorni un documento `.md` di analisi/design/sprint/how-to. Le regole per i README runnable sono nella skill `mde-readme`.");
            return new Regex(@"(?m)(?:^## Skill MdExplorer[ \t]*\n(?:[ \t]*\n)*)?(?:" + today + "|" + retired + @")[ \t]*$", RegexOptions.Compiled);
        }

        /// <summary>
        /// Il testo (fine riga LF) con il blocco di MDE alla versione di oggi; lo stesso testo se non c'è niente da fare. Un blocco con
        /// versione più nuova resta com'è (lo ha scritto un MDE più recente). Un blocco aperto e non chiuso è un errore che dice cosa fare.
        /// </summary>
        public static string Apply(string text, string skillPath)
        {
            if (text == null) return null;
            var starts = Start.Matches(text);
            var ends = Regex.Matches(text, Regex.Escape(EndMarker)).Count;
            if (starts.Count == 0 && ends == 0)
            {
                var old = OldParagraph.Match(text);
                return old.Success ? text.Substring(0, old.Index) + Block(skillPath) + text.Substring(old.Index + old.Length) : text;
            }

            var from = starts.Count == 1 ? starts[0].Index : -1;
            var to = text.IndexOf(EndMarker, StringComparison.Ordinal);
            if (starts.Count != 1 || ends != 1 || to < from)
                // Le due lingue insieme, senza UiText: si arriva qui all'apertura del progetto, e leggere lì la lingua dell'interfaccia
                // ha già bloccato il DB (03/11). L'errore finisce nel log, non in un dialogo.
                throw new InvalidOperationException(
                    $"Il blocco di MDE nelle istruzioni non è intero: serve una riga {StartMarker(Version)} e, dopo, una riga {EndMarker}. Correggilo a mano: finché non è intero MDE non lo aggiorna. / " +
                    $"The MDE block in the instructions is not whole: it needs a line {StartMarker(Version)} and, after it, a line {EndMarker}. Fix it by hand: until it is whole MDE does not update it.");

            if (int.Parse(starts[0].Groups["v"].Value) > Version) return text;
            return text.Substring(0, from) + Block(skillPath) + text.Substring(to + EndMarker.Length);
        }

        /// <summary>
        /// Porta il file al blocco di oggi; true se l'ha riscritto. La scrittura passa da <see cref="MdeRepoWriter.WriteIfChanged"/>: fine
        /// riga e BOM restano quelli del file, e la modifica arriva al commit automatico e alla notifica.
        /// </summary>
        public static bool ApplyToFile(string path, string skillPath)
        {
            var bytes = File.ReadAllBytes(path);
            var bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            var before = new UTF8Encoding(false).GetString(bytes, bom ? 3 : 0, bytes.Length - (bom ? 3 : 0)).Replace("\r\n", "\n");
            var after = Apply(before, skillPath);
            if (after == before) return false;
            var migrated = !Start.IsMatch(before);
            return MdeRepoWriter.WriteIfChanged(path, after, migrated
                ? new MdeWhy($"il paragrafo sulle skill di MDE ora è delimitato (marcatori {StartMarker(Version)} … {EndMarker}): MDE aggiorna solo quello, il resto del file è vostro",
                             $"the paragraph about the MDE skills is now delimited (markers {StartMarker(Version)} … {EndMarker}): MDE updates only that, the rest of the file is yours")
                : new MdeWhy($"aggiornato il blocco di MDE sulle skill alla versione {Version}",
                             $"MDE block about the skills updated to version {Version}"));
        }
    }
}
