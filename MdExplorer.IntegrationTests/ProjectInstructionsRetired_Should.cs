using System;
using System.IO;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Le frasi ritirate del file d'istruzioni del progetto (21/10/2026): fino ad allora la frase su <c>mde-doc</c> chiedeva un
    /// <c>.mde-doc/&lt;docname&gt;.kg.md</c> che MDE non legge più, e l'AI lo inventava a ogni documento.
    /// </summary>
    [TestClass]
    public class ProjectInstructionsRetired_Should
    {
        private const string Old = "Le regole di documentazione (TL;DR, niente codice nei doc di analisi, fence ```plantuml, generazione del file `.mde-doc/<docname>.kg.md` co-locato) sono descritte nella skill `mde-doc` installata in `.claude/skills/mde-doc/SKILL.md`. Consultala ogni volta che scrivi o aggiorni un documento `.md` di analisi/design/sprint/how-to. Le regole per i README runnable sono nella skill `mde-readme`.";

        [TestMethod]
        public void Replace_only_the_sentence_mde_wrote_keeping_the_engine_path_and_the_rest_of_the_file()
        {
            var text = "# Istruzioni di progetto\n\nRegole del gruppo.\n\n## Skill MdExplorer\n\n" + Old + "\n\n<!-- mde-template:documenti -->\nIl wiki.\n<!-- /mde-template:documenti -->\n";

            var fixedText = ProjectInstructionsRetired.Apply(text);

            Assert.IsFalse(fixedText.Contains(".kg.md"), fixedText);
            StringAssert.Contains(fixedText, ProjectInstructionsRetired.MdeDocSentence(".claude/skills/mde-doc/SKILL.md"));
            StringAssert.StartsWith(fixedText, "# Istruzioni di progetto\n\nRegole del gruppo.\n\n## Skill MdExplorer\n\n");
            StringAssert.EndsWith(fixedText, "\n\n<!-- mde-template:documenti -->\nIl wiki.\n<!-- /mde-template:documenti -->\n");
            Assert.AreEqual(fixedText, ProjectInstructionsRetired.Apply(fixedText), "una seconda volta non cambia niente");
        }

        [TestMethod]
        public void Leave_alone_a_sentence_the_group_has_changed()
        {
            var mine = Old.Replace("Consultala ogni volta", "Consultala quando serve");
            Assert.AreEqual(mine, ProjectInstructionsRetired.Apply(mine), "il testo è del gruppo: non è più quello scritto da MDE");
        }

        [TestMethod]
        public void Ship_instructions_that_no_longer_ask_for_kg_md()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MdExplorer", "claude-instructions.md"))) dir = dir.Parent;
            Assert.IsNotNull(dir);
            foreach (var file in new[] { "claude-instructions.md", "AGENTS.md", "copilot-instructions.md" })
            {
                var text = File.ReadAllText(Path.Combine(dir.FullName, "MdExplorer", file));
                Assert.IsFalse(text.Contains(".kg.md"), file);
                StringAssert.Contains(text, "skill `mde-doc`", file);
            }
        }
    }
}
