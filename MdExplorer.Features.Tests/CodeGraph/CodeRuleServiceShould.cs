using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.CodeGraph.Grammars;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// D42 dal sorgente al grafo: estrattori veri (JavaScript e Java), Fuseki vero, la regola applicata e tolta. La pagina
    /// chiama <c>inviaPagina('ordini.elenco')</c>; l'alias sta in un <c>.properties</c>; il controller Java ha il metodo.
    /// </summary>
    [TestClass]
    public class CodeRuleServiceShould
    {
        private string _root, _project;
        private CodeGraphStore _store;
        private CodeRuleService _rules;

        private const string Rule = @"{
            ""id"": ""azione-pagina"", ""description"": ""La pagina chiama il controller con 'alias.metodo'."",
            ""call"": { ""callee"": ""inviaPagina"", ""argument"": 0 },
            ""split"": { ""separator"": ""."" },
            ""owner"": { ""from"": ""string"", ""aliasMap"": ""program/conf/controller.properties"" },
            ""excludePrefixes"": [ ""redirect:"" ] }";

        [TestInitialize]
        public void Setup()
        {
            _store = FusekiFixture.NewStore();   // Inconclusive se Java 21 o il jar mancano
            _rules = new CodeRuleService(_store, NullLogger<CodeRuleService>.Instance);
            _root = Path.Combine(Path.GetTempPath(), "mde-coderules-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            var program = Path.Combine(_project, "program");
            Directory.CreateDirectory(Path.Combine(program, "web"));
            Directory.CreateDirectory(Path.Combine(program, "src", "com", "acme"));
            Directory.CreateDirectory(Path.Combine(program, "conf"));
            File.WriteAllText(Path.Combine(program, "web", "page.js"),
                "function scegli(id) {\n  inviaPagina('ordini.elenco');\n}\nfunction apri() {\n  inviaPagina('ordini.dettaglio');\n  inviaPagina('redirect:home');\n}\n");
            File.WriteAllText(Path.Combine(program, "src", "com", "acme", "OrdiniController.java"),
                "package com.acme;\n\npublic class OrdiniController {\n    public void elenco() { }\n    public void dettaglio() { }\n}\n");
            File.WriteAllText(Path.Combine(program, "conf", "controller.properties"), "ordini=com.acme.OrdiniController\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private static string NuGetGrammar(GrammarStore store, string id)
        {
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            return Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", store.Rid, "native", GrammarCatalog.ById(id).FileName(store.Rid));
        }

        private void Scan()
        {
            var grammars = new GrammarStore(_root);
            Directory.CreateDirectory(grammars.Folder);
            foreach (var id in new[] { "java", "javascript" })
            {
                var source = NuGetGrammar(grammars, id);
                if (!File.Exists(source)) Assert.Inconclusive($"grammatica {id} non trovata nel NuGet in cache: " + source);
                grammars.Install(GrammarCatalog.ById(id), source, "1.3.0", "nuget");
            }
            void Emit(ICodeExtractor extractor, string pattern)
            {
                var files = Directory.GetFiles(Path.Combine(_project, "program"), pattern, SearchOption.AllDirectories).OrderBy(f => f)
                    .Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
                foreach (var (file, script) in extractor.Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files }).Scripts)
                {
                    var path = CodeScriptWriter.ScriptPathFor(_project, file.LogicalPath, CodeScriptKind.Ast);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, CodeScriptWriter.Serialize(script));
                }
            }
            Emit(new TreeSitterJavaScriptExtractor(grammars, NullLogger<TreeSitterJavaScriptExtractor>.Instance), "*.js");
            Emit(new TreeSitterJavaExtractor(grammars, NullLogger<TreeSitterJavaExtractor>.Instance), "*.java");
        }

        private async Task<string> IdOf(string qualifiedNameStart)
            => (await _store.ResolveAsync(_project, qualifiedNameStart)).Single(s => s.Kind is "method" or "function").Id;

        [TestMethod]
        public async Task Link_the_page_to_the_controller_method_and_remove_the_links_with_the_rule()
        {
            Scan();
            var loaded = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, loaded.Failed, loaded.FirstError);

            var preview = await _rules.PreviewAsync(_project, CodeEnrichmentRule.Parse(Rule));
            Assert.IsTrue(preview.Accepted, string.Join(" ", preview.RejectReasons));
            Assert.AreEqual((3, 1, 2, 0), (preview.CallSites, preview.Excluded, preview.Resolved, preview.Unresolved));
            Assert.IsFalse(File.Exists(CodeRuleService.ScriptPath(_project, "azione-pagina")), "la prova a secco non scrive niente");

            await _rules.ApplyAsync(_project, CodeEnrichmentRule.Parse(Rule), "MarkAgent / test");
            Assert.IsTrue(File.Exists(CodeRuleService.DefinitionPath(_project, "azione-pagina")));
            var scegli = await IdOf("scegli");
            var edge = (await _store.NeighborsAsync(_project, scegli, "dispatches", "out", 1, 10)).Single();
            Assert.AreEqual(("rule", "program/web/page.js", 2), (edge.Confidence, edge.Path, edge.Line));
            StringAssert.EndsWith(edge.Other.QualifiedName, "OrdiniController.elenco()");
            Assert.AreEqual("azione-pagina", _rules.List(_project).Single().Id);
            Assert.AreEqual("MarkAgent / test", _rules.List(_project).Single().WrittenBy);

            // Una nuova apertura del progetto ricarica la regola dal disco come gli altri script (Git porta i due file).
            Assert.AreEqual(0, (await _store.LoadProjectAsync(_project)).Failed);

            Assert.IsTrue(await _rules.RemoveAsync(_project, "azione-pagina"));
            Assert.AreEqual(0, (await _store.NeighborsAsync(_project, scegli, "dispatches", "out", 1, 10)).Count, "via con la regola");
            Assert.IsTrue((await _store.NeighborsAsync(_project, await IdOf("OrdiniController.elenco"), null, "in", 1, 10)).Any(), "il resto del grafo resta");
            Assert.IsFalse(await _rules.RemoveAsync(_project, "azione-pagina"));
        }

        [TestMethod]
        public async Task Refuse_to_apply_a_rule_that_links_too_little_and_write_nothing()
        {
            Scan();
            await _store.LoadProjectAsync(_project);
            var wrong = CodeEnrichmentRule.Parse(Rule.Replace("\"separator\": \".\"", "\"separator\": \":\""));   // la stringa non ha i due punti

            var ex = await Assert.ThrowsExceptionAsync<CodeRuleException>(() => _rules.ApplyAsync(_project, wrong, "test"));
            StringAssert.Contains(ex.Message, "0");
            Assert.IsFalse(Directory.Exists(CodeGraphPaths.GetRulesFolder(_project)) && Directory.EnumerateFiles(CodeGraphPaths.GetRulesFolder(_project)).Any());
        }
    }
}
