using MdExplorer.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// The cleaning of the agent's answer into a commit message. Since 01/10/2026 the message is a
    /// Conventional Commit: the old cleaning capitalised the first letter of the header, which
    /// turned <c>fix(git): …</c> into <c>Fix(git): …</c>.
    /// </summary>
    [TestClass]
    public class GitCommitAiMessage_Should
    {
        private static string Clean(string raw) =>
            new GitCommitAiService(NullLogger<GitCommitAiService>.Instance, null).CleanCommitMessage(raw);

        [TestMethod]
        public void Keep_a_conventional_header_lowercase()
        {
            Assert.AreEqual("fix(git): correggi il conteggio dei file", Clean("fix(git): correggi il conteggio dei file"));
        }

        [TestMethod]
        public void Put_a_conventional_header_in_its_canonical_spelling()
        {
            Assert.AreEqual("feat(toolbar): aggiungi il pannello", Clean("Feat (Toolbar) :aggiungi il pannello."));
            Assert.AreEqual("refactor!: rimuovi il vecchio endpoint", Clean("Refactor!: rimuovi il vecchio endpoint"));
            Assert.AreEqual("docs: aggiorna il piano", Clean("docs:aggiorna il piano"));
        }

        [TestMethod]
        public void Leave_a_header_that_is_not_conventional_as_written()
        {
            Assert.AreEqual("nota: qualcosa da ricordare", Clean("nota: qualcosa da ricordare"));
            Assert.AreEqual("aggiorna il piano", Clean("aggiorna il piano"));
        }

        [TestMethod]
        public void Separate_header_and_body_and_drop_wrappers_and_trailers()
        {
            var raw = "```\nEcco il messaggio: fix(git): togli il tooltip\nIl tooltip copriva il pannello. Impediva di arrivare al pulsante.\n\nCo-authored-by: Qualcuno <q@example.com>\n```";
            Assert.AreEqual(
                "fix(git): togli il tooltip\n\nIl tooltip copriva il pannello. Impediva di arrivare al pulsante.",
                Clean(raw));
        }

        [TestMethod]
        public void Wrap_the_body_at_72_characters()
        {
            var body = "La prima frase dice che cosa cambia nel pannello della toolbar quando ci si passa sopra. La seconda dice perché.";
            var cleaned = Clean("fix: correggi il pannello\n\n" + body);
            foreach (var line in cleaned.Split('\n'))
                Assert.IsTrue(line.Length <= 72, $"line longer than 72: {line}");
            Assert.AreEqual(body, cleaned.Substring(cleaned.IndexOf("\n\n") + 2).Replace("\n", " "));
        }
    }
}
