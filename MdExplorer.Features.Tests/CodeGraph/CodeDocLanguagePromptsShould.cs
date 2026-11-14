using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Il prompt di un pacchetto di linguaggio in coda a mde-code-doc (B6b), e la versione del prompt composta: cambiare il
    /// prompt di un linguaggio rende «da rivedere» solo le descrizioni dei suoi sorgenti.
    /// </summary>
    [TestClass]
    public class CodeDocLanguagePromptsShould
    {
        private string _project;

        [TestInitialize]
        public void Setup()
        {
            _project = Path.Combine(Path.GetTempPath(), "mde-prompt-tests", Guid.NewGuid().ToString("N"));
            Write("toy/language.yml", DeclarativeLanguageExtractorShould.ToyDefinition);
            Write("toy/prompts/code-doc.md", "Un PART n è un blocco chiamato con RUN PART n.\n");
            Write("zeta/language.yml", DeclarativeLanguageExtractorShould.ToyDefinition.Replace("id: toy", "id: zeta").Replace("name: Toy", "name: Zeta")
                .Replace("extensions: [.toy, .toh, .tor, .tos]", "extensions: [.zt]"));
            Write("zeta/prompts/code-doc.md", "In Zeta le etichette cominciano con @.\n");
            Write("muto/language.yml", DeclarativeLanguageExtractorShould.ToyDefinition.Replace("id: toy", "id: muto").Replace("extensions: [.toy, .toh, .tor, .tos]", "extensions: [.mu]"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(Path.GetDirectoryName(_project), true); } catch (IOException) { }
        }

        private void Write(string rel, string text)
        {
            var path = Path.Combine(DeclarativeLanguage.FolderOf(_project), rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
        }

        private CodeDocLanguagePrompts Prompts()
        {
            var catalog = CodeLanguageCatalog.ForProject(_project);
            Assert.AreEqual(0, catalog.Errors.Count, string.Join("\n", catalog.Errors.Values.Select(e => e.Message)));
            return new CodeDocLanguagePrompts(catalog);
        }

        private static CodeScript Ast(string language, string source)
        {
            var ast = new CodeScript { Header = new CodeScriptHeader { Kind = CodeScriptKind.Ast, Source = source, Language = language } };
            ast.Symbols.Add(new ScriptSymbol { Id = source + "#f", Kind = "function", Name = "F", QualifiedName = source + ":F", LineStart = 1, LineEnd = 3 });
            return ast;
        }

        [TestMethod]
        public void Add_the_language_prompt_after_the_base_one_once_per_language()
        {
            var prompts = Prompts();
            var body = prompts.BodyFor("BASE", new[] { Ast("toy", "a.toy"), Ast("toy", "b.toy") });
            StringAssert.StartsWith(body, "BASE\n\n---- LINGUAGGIO Toy ----\n");
            StringAssert.EndsWith(body, "Un PART n è un blocco chiamato con RUN PART n.");
            Assert.AreEqual(1, body.Split("---- LINGUAGGIO").Length - 1, "due file dello stesso linguaggio: una sezione");

            var cycle = prompts.BodyFor("BASE", new[] { Ast("zeta", "c.zt"), Ast("toy", "a.toy") });
            Assert.IsTrue(cycle.IndexOf("---- LINGUAGGIO Toy") < cycle.IndexOf("---- LINGUAGGIO Zeta"), "un ciclo fra linguaggi: una sezione per linguaggio, sempre nello stesso ordine");

            Assert.AreEqual("BASE", prompts.BodyFor("BASE", new[] { Ast("csharp", "x.cs") }), "un linguaggio di MdExplorer non ha pacchetto");
            Assert.AreEqual("BASE", prompts.BodyFor("BASE", new[] { Ast("muto", "x.mu") }), "un pacchetto senza prompt non aggiunge niente");
        }

        [TestMethod]
        public void Compose_the_prompt_version_with_the_language_prompt_fingerprint()
        {
            var prompts = Prompts();
            var toy = prompts.VersionFor("11", Ast("toy", "a.toy"));
            StringAssert.Matches(toy, new System.Text.RegularExpressions.Regex(@"^11\+toy\.[0-9a-f]{8}$"));
            Assert.AreEqual("11", prompts.VersionFor("11", Ast("csharp", "x.cs")));
            Assert.AreEqual("11", prompts.VersionFor("11", Ast("muto", "x.mu")));
            Assert.IsNull(prompts.VersionFor(null, Ast("toy", "a.toy")), "versione che non si guarda: resta null");

            Write("toy/prompts/code-doc.md", "Un PART n è un blocco numerato.\n");
            Assert.AreNotEqual(toy, Prompts().VersionFor("11", Ast("toy", "a.toy")), "il prompt cambiato si vede anche senza alzare una versione");
        }

        [TestMethod]
        public void Review_only_the_descriptions_of_the_language_whose_prompt_changed()
        {
            CodeDocVerdict VerdictOf(CodeScript ast, string writtenWith, string now)
            {
                var s = ast.Symbols.Single();
                var doc = new CodeScript { Header = new CodeScriptHeader { Kind = CodeScriptKind.Doc, Source = ast.Header.Source } };
                doc.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = s.Id, Summary = "fa F", SignatureHash = CodeDocPlanner.SignatureHash(s, Array.Empty<ScriptParameter>()), ContextHash = "c", PromptVersion = writtenWith });
                var a = CodeDocPlanner.Assess(ast, doc, force: false, review: true, contextHashOf: _ => "c", promptVersion: now);
                return a.Counts.Single().Key;
            }

            var before = Prompts();
            var toyAst = Ast("toy", "a.toy");
            var csAst = Ast("csharp", "x.cs");
            var toyThen = before.VersionFor("11", toyAst);
            Write("toy/prompts/code-doc.md", "Un PART n è un blocco numerato.\n");
            var after = Prompts();

            Assert.AreEqual(CodeDocVerdict.ReviewPrompt, VerdictOf(toyAst, toyThen, after.VersionFor("11", toyAst)), "Toy: il suo prompt è cambiato");
            Assert.AreEqual(CodeDocVerdict.Fresh, VerdictOf(csAst, before.VersionFor("11", csAst), after.VersionFor("11", csAst)), "C#: niente da rivedere");
            Assert.AreEqual(CodeDocVerdict.Fresh, VerdictOf(toyAst, toyThen, before.VersionFor("11", toyAst)));
        }

        private static string RealPrompt()
        {
            // il prompt distribuito con MdExplorer, dal sorgente del repository
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var path = Path.Combine(dir.FullName, "MdExplorer", "skills", "mde-code-doc", "prompt.md");
                if (File.Exists(path)) return File.ReadAllText(path);
            }
            throw new FileNotFoundException("MdExplorer/skills/mde-code-doc/prompt.md non trovato risalendo da " + AppContext.BaseDirectory);
        }

        private static CodeScript WithFragment(CodeScript ast)
        {
            ast.Symbols.Add(new ScriptSymbol { Id = ast.Header.Source + "#p1", Kind = "fragment", Name = "P1", QualifiedName = ast.Header.Source + ":P1", LineStart = 2, LineEnd = 3 });
            return ast;
        }

        [TestMethod]
        public void Judge_and_ask_each_source_only_with_the_prompt_sections_it_uses()
        {
            var text = RealPrompt();
            var sections = CodeDocPromptSections.FromPromptText(text);
            Assert.AreEqual(10, sections.Versions[CodeDocPromptSections.Base]);
            var prompts = new CodeDocLanguagePrompts(CodeLanguageCatalog.ForProject(_project), sections);

            var java = Ast("java", "a.java");
            var toy = WithFragment(Ast("toy", "b.toy"));
            Assert.AreEqual("10", prompts.VersionFor("12", java), "i frammenti non cambiano il prompt di un sorgente che non ne ha");
            StringAssert.StartsWith(prompts.VersionFor("12", toy), "11+toy.", "con i frammenti: la versione della loro sezione, più il pacchetto");
            Assert.IsNull(prompts.VersionFor(null, java), "versione che non si guarda: resta null");

            var body = "prima\n\n<!-- mde-section: fragments -->\n**I frammenti** — kind `fragment`\n<!-- /mde-section -->\n\ndopo";
            Assert.AreEqual("prima\n\ndopo", prompts.BodyFor(body, new[] { java }), "senza frammenti la sezione non va all'LLM");
            StringAssert.StartsWith(prompts.BodyFor(body, new[] { java, toy }), "prima\n\n**I frammenti** — kind `fragment`\n\ndopo", "basta un file con frammenti nella domanda");
            Assert.IsFalse(prompts.BodyFor(body, new[] { toy }).Contains("mde-section"), "i marcatori non arrivano mai all'LLM");

            Assert.AreEqual("11", CodeDocPromptSections.None.VersionFor("11", toy), "prompt senza promptVersions (personalizzato): la versione sola, come prima");
            Assert.ThrowsException<FormatException>(() => CodeDocPromptSections.FromPromptText("---\npromptVersions:\n  fragments: 11\n---\n"), "senza base non si indovina");
        }
    }
}
