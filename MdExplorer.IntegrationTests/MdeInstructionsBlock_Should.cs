using System;
using System.IO;
using System.Text;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// S4 dello sprint «File di sistema di MDE»: nel file d'istruzioni di MDE è solo il blocco fra <c>&lt;!-- mde:skill vN --&gt;</c> e
    /// <c>&lt;!-- /mde:skill --&gt;</c>; il paragrafo delle versioni precedenti diventa il blocco. Prende il posto di
    /// <c>ProjectInstructionsRetired</c> (la frase ritirata il 21/10 sul <c>.kg.md</c> si migra qui).
    /// </summary>
    [TestClass]
    public class MdeInstructionsBlock_Should
    {
        private const string Claude = ".claude/skills/mde-doc/SKILL.md";
        private const string Retired = "Le regole di documentazione (TL;DR, niente codice nei doc di analisi, fence ```plantuml, generazione del file `.mde-doc/<docname>.kg.md` co-locato) sono descritte nella skill `mde-doc` installata in `.claude/skills/mde-doc/SKILL.md`. Consultala ogni volta che scrivi o aggiorni un documento `.md` di analisi/design/sprint/how-to. Le regole per i README runnable sono nella skill `mde-readme`.";
        private static readonly string Today = MdeInstructionsBlock.MdeDocSentence(Claude);
        private const string Head = "# Istruzioni di progetto\n\nRegole del gruppo.\n\n";
        private const string Tail = "\n\n<!-- mde-template:documenti -->\nIl wiki.\n<!-- /mde-template:documenti -->\n";

        [TestMethod]
        public void Turn_the_paragraph_of_earlier_versions_into_the_block_and_keep_the_rest()
        {
            foreach (var sentence in new[] { Today, Retired })
            {
                var text = Head + "## Skill MdExplorer\n\n" + sentence + Tail;
                var after = MdeInstructionsBlock.Apply(text, Claude);
                Assert.AreEqual(Head + MdeInstructionsBlock.Block(Claude) + Tail, after);
                Assert.IsFalse(after.Contains(".kg.md"));
                Assert.AreEqual(after, MdeInstructionsBlock.Apply(after, Claude), "una seconda volta non cambia niente");
            }
        }

        [TestMethod]
        public void Leave_alone_a_paragraph_the_group_has_changed()
        {
            var mine = Head + "## Skill MdExplorer\n\n" + Today.Replace("ogni volta", "quando serve") + Tail;
            Assert.AreEqual(mine, MdeInstructionsBlock.Apply(mine, Claude), "il testo è del gruppo: non è più quello scritto da MDE");
            var none = Head + "Niente skill qui." + Tail;
            Assert.AreEqual(none, MdeInstructionsBlock.Apply(none, Claude), "senza paragrafo di MDE non si aggiunge niente");
        }

        [TestMethod]
        public void Rewrite_the_inside_of_the_block_and_leave_the_outside()
        {
            var text = Head + MdeInstructionsBlock.StartMarker(1) + "\nvecchio testo\n" + MdeInstructionsBlock.EndMarker + Tail + "riga del gruppo\n";
            Assert.AreEqual(Head + MdeInstructionsBlock.Block(Claude) + Tail + "riga del gruppo\n", MdeInstructionsBlock.Apply(text, Claude));
        }

        [TestMethod]
        public void Leave_a_block_written_by_a_newer_mde()
        {
            var newer = Head + MdeInstructionsBlock.StartMarker(MdeInstructionsBlock.Version + 1) + "\ntesto di domani\n" + MdeInstructionsBlock.EndMarker + Tail;
            Assert.AreEqual(newer, MdeInstructionsBlock.Apply(newer, Claude), "un MDE più vecchio non riporta indietro il blocco di un collega");
        }

        [TestMethod]
        public void Refuse_a_block_that_is_not_whole()
        {
            var broken = Head + MdeInstructionsBlock.StartMarker(1) + "\nsenza fine" + Tail;
            var ex = Assert.ThrowsException<InvalidOperationException>(() => MdeInstructionsBlock.Apply(broken, Claude));
            StringAssert.Contains(ex.Message, MdeInstructionsBlock.EndMarker);
        }

        [TestMethod]
        public void Ship_templates_that_already_have_the_block()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MdExplorer", "claude-instructions.md"))) dir = dir.Parent;
            Assert.IsNotNull(dir);
            foreach (var (file, skills) in new[] { ("claude-instructions.md", ".claude/skills"), ("AGENTS.md", ".opencode/skills"), ("copilot-instructions.md", ".github/skills") })
            {
                var text = File.ReadAllText(Path.Combine(dir.FullName, "MdExplorer", file)).Replace("\r\n", "\n");
                var skill = skills + "/mde-doc/SKILL.md";
                StringAssert.Contains(text, MdeInstructionsBlock.Block(skill), file);
                Assert.AreEqual(text, MdeInstructionsBlock.Apply(text, skill), file + ": appena creato non c'è niente da aggiornare");
            }
        }

        [TestMethod]
        public void Produce_the_same_file_on_two_machines()
        {
            // Il caso di progetto B: due MDE uguali migrano lo stesso paragrafo; il risultato è identico byte per byte.
            var text = Head + "## Skill MdExplorer\n\n" + Retired + Tail;
            Assert.AreEqual(MdeInstructionsBlock.Apply(text, Claude), MdeInstructionsBlock.Apply(text, Claude));
        }

        [TestMethod]
        public void Rewrite_the_file_with_the_line_endings_and_bom_it_had()
        {
            var dir = Path.Combine(Path.GetTempPath(), "istruzioni-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var text = "# Istruzioni\r\n\r\nRegole del gruppo, à è ì.\r\n\r\n## Skill MdExplorer\r\n\r\n" + Retired + "\r\n";
                foreach (var bom in new[] { true, false })
                {
                    var file = Path.Combine(dir, $"CLAUDE-{bom}.md");
                    File.WriteAllText(file, text, new UTF8Encoding(bom));

                    Assert.IsTrue(MdeInstructionsBlock.ApplyToFile(file, Claude));

                    var bytes = File.ReadAllBytes(file);
                    Assert.AreEqual(bom, bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF, $"BOM {bom}: come prima");
                    var after = File.ReadAllText(file);
                    StringAssert.StartsWith(after, "# Istruzioni\r\n\r\nRegole del gruppo, à è ì.\r\n\r\n<!-- mde:skill v1 -->\r\n## Skill MdExplorer\r\n", "a capo di Windows compresi");
                    Assert.IsFalse(after.Contains(".kg.md"));
                    Assert.IsFalse(MdeInstructionsBlock.ApplyToFile(file, Claude), "migrato una volta, poi niente da fare");
                }
            }
            finally { Directory.Delete(dir, recursive: true); }
        }
    }
}
