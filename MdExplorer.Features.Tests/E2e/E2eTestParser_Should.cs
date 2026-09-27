using MdExplorer.Features.E2e;
using MdExplorer.Features.Tests.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2eTestParser_Should
    {
        private const string FrontMatter =
            "---\ne2e:\n  baseUrl: https://example.org\n  siteMap: mappa.md\n  credentials: credenziali-x.txt\n  artifacts: t.e2e/\n---\n";

        private const string Sections = "\n## Artefatti\n\n## Esiti\n";

        private static string SkillTestExample()
        {
            var skill = File.ReadAllText(Path.Combine(SlidePage.RepositoryRoot(), "MdExplorer", "skills", "mde-e2e", "SKILL.md"));
            return Regex.Match(skill, @"^````markdown esempio=test\n(.*?)^````$", RegexOptions.Singleline | RegexOptions.Multiline).Groups[1].Value;
        }

        [TestMethod]
        public void Read_the_test_the_skill_teaches_without_problems()
        {
            var document = E2eTestParser.Parse(SkillTestExample(), "login.e2e.md");

            CollectionAssert.AreEqual(new string[0], document.Problems.ToArray());
            Assert.AreEqual("https://the-internet.herokuapp.com", document.BaseUrl);
            Assert.AreEqual("credenziali-the-internet.txt", document.Credentials);
            CollectionAssert.AreEqual(new[] { 1, 2 }, document.Tests.Select(t => t.Number).ToArray());
            Assert.AreEqual("Login riuscito", document.Tests[0].Title);
            Assert.AreEqual(6, document.Tests[0].Steps.Count);
            CollectionAssert.AreEquivalent(new[] { "the-internet.utente", "the-internet.password" }, document.CredentialKeys.ToArray());

            var check = document.Tests[1].Steps[4];
            Assert.AreEqual(E2eCheckKind.TextVisible, check.Check);
            Assert.AreEqual("Your password is invalid!", check.Expected);
            Assert.IsNull(document.Tests[0].Steps[0].Check, "an action is not a check");
        }

        [TestMethod]
        public void Recognise_every_form_of_check_the_skill_lists()
        {
            void Is(string line, E2eCheckKind kind, string expected = null, string target = null, int? count = null)
            {
                var step = E2eTestParser.ReadStep(1, line);
                Assert.AreEqual(kind, step.Check, line);
                Assert.AreEqual(expected, step.Expected, line);
                Assert.AreEqual(target, step.Target, line);
                Assert.AreEqual(count, step.Count, line);
            }

            Is("✔ Compare il testo \"Benvenuto\"", E2eCheckKind.TextVisible, "Benvenuto");
            Is("✔ Non compare il testo \"Errore\"", E2eCheckKind.TextHidden, "Errore");
            Is("✔ L'URL contiene \"/secure\"", E2eCheckKind.UrlContains, "/secure");
            Is("✔ L’URL contiene \"/secure\"", E2eCheckKind.UrlContains, "/secure");
            Is("✔ Il titolo della pagina è \"Home\"", E2eCheckKind.TitleIs, "Home");
            Is("✔ Il campo \"Nome\" vale \"Mario\"", E2eCheckKind.FieldValue, "Mario", "Nome");
            Is("✔ Il campo \"Nome\" vale \"\"", E2eCheckKind.FieldValue, "", "Nome");
            Is("✔ Il bottone \"Invia\" è disabilitato", E2eCheckKind.ButtonDisabled, target: "Invia");
            Is("✔ Ci sono esattamente 2 bottoni \"Delete\"", E2eCheckKind.ButtonCount, target: "Delete", count: 2);
            Is("✔ Ci sono esattamente 3 link \"Dettagli\"", E2eCheckKind.LinkCount, target: "Dettagli", count: 3);
            Is("✔ Nella pagina ci sono esattamente due bottoni \"Delete\"", E2eCheckKind.Judgment);
        }

        [TestMethod]
        public void Report_every_problem_of_the_text_at_once()
        {
            var markdown = FrontMatter.Replace("https://example.org", "example.org") +
                "## T1 — Uno\n1. Apri `/a`\n3. Premi \"B\"\n\n## T1 — Doppione\n1. Apri `/b`\n\n## T2 — Vuoto\n";

            var problems = E2eTestParser.Parse(markdown, "t.e2e.md").Problems;

            Assert.IsTrue(problems.Any(p => p.Contains("'e2e.baseUrl' vale 'example.org'")), string.Join("\n", problems));
            Assert.IsTrue(problems.Any(p => p.Contains("il passo 2 è numerato 3")));
            Assert.IsTrue(problems.Any(p => p.Contains("il numero T1 è usato da più test")));
            Assert.IsTrue(problems.Any(p => p.Contains("il test T2") && p.Contains("non ha passi")));
            Assert.IsTrue(problems.Any(p => p.Contains("manca la sezione '## Artefatti'")));
            Assert.IsTrue(problems.Any(p => p.Contains("manca la sezione '## Esiti'")));
        }

        [TestMethod]
        public void Refuse_a_file_without_e2e_block_saying_what_it_needs()
        {
            var ex = Assert.ThrowsException<E2eFormatException>(() => E2eTestParser.Parse("---\ntitle: x\n---\n## T1 — a\n1. b\n", "t.e2e.md"));
            StringAssert.Contains(ex.Message, "manca il blocco 'e2e:'");
        }

        [TestMethod]
        public void Not_read_steps_inside_code_fences()
        {
            var markdown = FrontMatter + "## T1 — Uno\n1. Apri `/a`\n```text\n2. non è un passo\n## T9 — non è un test\n```\n2. ✔ Compare il testo \"ok\"\n" + Sections;
            var document = E2eTestParser.Parse(markdown, "t.e2e.md");

            CollectionAssert.AreEqual(new string[0], document.Problems.ToArray());
            Assert.AreEqual(1, document.Tests.Count);
            Assert.AreEqual(2, document.Tests[0].Steps.Count);
        }

        [TestMethod]
        public void Change_the_fingerprint_with_the_text_of_the_test_and_nothing_else()
        {
            string Fingerprint(string test) => E2eTestParser.Parse(FrontMatter + test + Sections, "t.e2e.md").Tests[0].Fingerprint;

            var original = Fingerprint("## T1 — Uno\n1. Apri `/a`\n2. ✔ Compare il testo \"ok\"\n");
            StringAssert.Matches(original, new Regex("^sha256:[0-9a-f]{16}$"));
            Assert.AreEqual(original, Fingerprint("## T1 — Uno\n1. Apri `/a`   \n\n\n2. ✔ Compare il testo \"ok\"\n"), "trailing blanks and empty lines");
            Assert.AreNotEqual(original, Fingerprint("## T1 — Uno\n1. Apri `/a`\n2. ✔ Compare il testo \"OK\"\n"), "an expected text");
            Assert.AreNotEqual(original, Fingerprint("## T1 — Due\n1. Apri `/a`\n2. ✔ Compare il testo \"ok\"\n"), "the title");
        }
    }
}
