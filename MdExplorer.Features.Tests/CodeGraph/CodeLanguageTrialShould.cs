using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// D42 passo 2: la prova a secco di un linguaggio scritto da MarkAgent, senza scrivere niente. Il dialetto finto di
    /// <see cref="TemplatePageExtractorShould"/> su poche pagine: accettato con i numeri e il campione, respinto quando non
    /// trova le pagine incluse o quando i blocchi non si chiudono.
    /// </summary>
    [TestClass]
    public class CodeLanguageTrialShould
    {
        private string _root, _project;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-trial-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            Directory.CreateDirectory(_project);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_root, true); } catch (IOException) { }
        }

        private CodeSourceFile Write(string logical, string text)
        {
            var path = Path.Combine(_project, logical);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
            return new CodeSourceFile(path, logical);
        }

        private CodeLanguagePreview Evaluate(string definition, params CodeSourceFile[] files)
            => CodeLanguageTrial.Evaluate(_project, CodeLanguageTrial.PackageOf(_project, definition), new[] { new CodeLanguageFiles("web", files, null) }, replacesVersion: null);

        [TestMethod]
        public void Accept_a_dialect_that_reads_the_pages_and_give_a_sample_to_check()
        {
            var a = Write("web/a.page", "{%include(b)%}\n{%if(X)%}<button id=\"ok\" onclick=\"invia('c.m')\">ok</button>{%endif%}\n{%strano(1)%}");
            var b = Write("web/b.page", "<a href=\"javascript:vai()\">x</a>");
            var p = Evaluate(TemplatePageExtractorShould.Dialect, a, b);

            Assert.IsTrue(p.Accepted, string.Join(" ", p.RejectReasons));
            Assert.AreEqual(("pages", "pagine", 2, 2), (p.Kind, p.LanguageId, p.Files, p.FilesRead));
            Assert.AreEqual(2, p.SymbolsByKind["control"]);
            Assert.AreEqual(1, p.EdgesByRelation["includes"]);
            Assert.AreEqual((1, 1, 1), (p.Pages.Includes, p.Pages.IncludesResolved, p.Pages.GuardedControls));
            Assert.AreEqual("web/a.page:3", p.Pages.UnknownWordExamples["strano"]);
            StringAssert.Contains(p.Notes.Single(), "strano");
            CollectionAssert.AreEqual(new[] { "web/a.page", "web/b.page" }, p.Sample.Select(s => s.Path).ToArray());
            var facts = string.Join("\n", p.Sample[0].Facts);
            StringAssert.Contains(facts, "control ok");
            StringAssert.Contains(facts, "X");
            StringAssert.Contains(facts, "web/a.page includes → web/b.page");
            StringAssert.Contains(facts, "ok: invia('c.m')");

            var checks = p.Sample.Select(s => new CodeLanguageSampleCheck { Path = s.Path, Ok = true }).ToList();
            Assert.AreEqual(0, CodeLanguageProposals.SampleProblems(p, checks).Count);
            checks[1].Ok = false; checks[1].Note = "manca un controllo";
            StringAssert.Contains(CodeLanguageProposals.SampleProblems(p, checks).Single(), "manca un controllo");
            Assert.AreEqual(2, CodeLanguageProposals.SampleProblems(p, null).Count, "senza verifiche non si propone");
        }

        [TestMethod]
        public void Reject_a_dialect_whose_includes_do_not_lead_to_pages_or_whose_blocks_do_not_close()
        {
            var a = Write("web/a.page", "{%include(x1)%}{%include(x2)%}{%include(b)%}<b onclick=\"f()\">x</b>");
            var b = Write("web/b.page", "{%if(Y)%}<i onclick=\"g()\">y</i>");
            var p = Evaluate(TemplatePageExtractorShould.Dialect, a, b);
            Assert.IsFalse(p.Accepted);
            Assert.AreEqual(2, p.RejectReasons.Count, string.Join("\n", p.RejectReasons));
            StringAssert.Contains(p.RejectReasons[0], "1");
            Assert.AreEqual(1, p.FilesWithWarnings);
            CollectionAssert.AreEquivalent(new[] { "x1", "x2" }, p.Pages.UnresolvedIncludes.Keys.ToArray());
        }

        [TestMethod]
        public void Note_the_recognizers_that_never_match()
        {
            var v = Write("Views/a.cshtml", "@if (X) {\n<a asp-controller=\"O\" asp-action=\"L\">x</a>\n}");
            var p = CodeLanguageTrial.Evaluate(_project, CodeLanguageTrial.PackageOf(_project, TemplatePageExtractorShould.Razor), new[] { new CodeLanguageFiles("web", new[] { v }, null) }, null);
            Assert.IsTrue(p.Accepted, string.Join(" ", p.RejectReasons));
            Assert.AreEqual(3, p.Notes.Count, string.Join("\n", p.Notes));
            Assert.IsTrue(p.Notes.Any(n => n.Contains("'include#0'")) && p.Notes.Any(n => n.Contains("'else#2'")) && p.Notes.Any(n => n.Contains("'call#4'")));
        }

        [TestMethod]
        public void Reject_when_there_are_no_files_and_refuse_a_broken_definition()
        {
            var p = CodeLanguageTrial.Evaluate(_project, CodeLanguageTrial.PackageOf(_project, TemplatePageExtractorShould.Dialect), new[] { new CodeLanguageFiles("web", Array.Empty<CodeSourceFile>(), null) }, null);
            Assert.IsFalse(p.Accepted);
            StringAssert.Contains(p.RejectReasons.Single(), ".page");

            Assert.ThrowsException<LanguageDefinitionException>(() => CodeLanguageTrial.PackageOf(_project, "schema: 1\nname: x\n"));
            var ex = Assert.ThrowsException<LanguageDefinitionException>(() => CodeLanguageTrial.PackageOf(_project, TemplatePageExtractorShould.Dialect.Replace("include: [include]", "include: [include, if]")));
            StringAssert.Contains(ex.Reason, "'if'");
            var clash = Assert.ThrowsException<LanguageDefinitionException>(() => CodeLanguageCatalog.ForProject(_project).With(CodeLanguageTrial.PackageOf(_project, TemplatePageExtractorShould.Dialect.Replace("[.page]", "[.js]"))));
            StringAssert.Contains(clash.Reason, "JavaScript");
        }
    }
}
