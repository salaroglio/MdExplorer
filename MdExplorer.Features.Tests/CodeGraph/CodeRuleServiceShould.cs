using System;
using System.Collections.Generic;
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

        /// <summary>
        /// 30/10/2026: ServiceLoader. Il grafo sapeva chi implementa l'interfaccia, non chi è registrato e quindi gira: ora
        /// interfaccia → implementazioni registrate, metodo per metodo, e l'impatto di un'implementazione risale a chi
        /// chiama l'interfaccia.
        /// </summary>
        [TestMethod]
        public async Task Resolve_an_interface_into_the_implementations_registered_in_meta_inf_services()
        {
            var src = Path.Combine(_project, "program", "src", "com", "acme");
            File.WriteAllText(Path.Combine(src, "Exporter.java"), "package com.acme;\n\npublic interface Exporter {\n    void export(String doc);\n}\n");
            File.WriteAllText(Path.Combine(src, "PdfExporter.java"), "package com.acme;\n\npublic class PdfExporter implements Exporter {\n    public void export(String doc) { }\n}\n");
            File.WriteAllText(Path.Combine(src, "CsvExporter.java"), "package com.acme;\n\npublic class CsvExporter implements Exporter {\n    public void export(String doc) { }\n}\n");
            File.WriteAllText(Path.Combine(src, "OldExporter.java"), "package com.acme;\n\npublic class OldExporter implements Exporter {\n    public void export(String doc) { }\n}\n");
            File.WriteAllText(Path.Combine(src, "Runner.java"), "package com.acme;\n\npublic class Runner {\n    public void run(Exporter e) {\n        e.export(\"x\");\n    }\n}\n");
            Scan();
            Assert.AreEqual(0, (await _store.LoadProjectAsync(_project)).Failed);
            const string services = "program/resources/META-INF/services/com.acme.Exporter";
            var file = (services, "# registrati\ncom.acme.PdfExporter\n\ncom.acme.CsvExporter  # il secondo\ncom.lib.Missing\n");

            var (edges, misses) = await _rules.ApplyRegistrationsAsync(_project, new[] { file });
            Assert.AreEqual(1, misses.Count, "la classe di una libreria non è nel grafo: si dice");
            StringAssert.Contains(misses[0], "com.lib.Missing");
            var exporter = (await _store.ResolveAsync(_project, "com.acme.Exporter")).Single(x => x.Kind == "interface").Id;
            var resolved = await _store.NeighborsAsync(_project, exporter, CodeRegistrations.Relation, "out", 1, 10);
            CollectionAssert.AreEquivalent(new[] { "com.acme.CsvExporter", "com.acme.PdfExporter" }, resolved.Select(e => e.Other.QualifiedName).ToArray(), "OldExporter implementa ma non è registrata");
            Assert.AreEqual(("extracted", services, 2), (resolved.Single(e => e.Other.QualifiedName == "com.acme.PdfExporter").Confidence, resolved[0].Path,
                resolved.Single(e => e.Other.QualifiedName == "com.acme.PdfExporter").Line));
            Assert.AreEqual(4, edges, "2 classi + 2 metodi");

            // l'impatto del metodo registrato risale al metodo dell'interfaccia e a chi lo chiama
            var pdfExport = (await _store.ResolveAsync(_project, "com.acme.PdfExporter.export")).Single(x => x.Kind == "method").Id;
            var impact = await _store.ImpactAsync(_project, pdfExport, 2, 20);
            CollectionAssert.IsSubsetOf(new[] { "com.acme.Exporter.export(String)", "com.acme.Runner.run(Exporter)" }, impact.Select(e => e.Other.QualifiedName).ToArray(),
                string.Join(", ", impact.Select(e => e.Relation + " " + e.Other.QualifiedName)));

            // tolto il file, via gli archi
            await _rules.ApplyRegistrationsAsync(_project, Array.Empty<(string, string)>());
            Assert.IsFalse(File.Exists(CodeRuleService.ScriptPath(_project, CodeRuleService.RegistrationsId)));
            Assert.AreEqual(0, (await _store.NeighborsAsync(_project, exporter, CodeRegistrations.Relation, "out", 1, 10)).Count);
        }

        /// <summary>
        /// D43: i fatti nuovi (argomenti X.class, catene, annotazioni) entrano in Fuseki, e le regole sulle stringhe (D42) non
        /// li vedono: un punto con soli tipi non è un punto con stringhe.
        /// </summary>
        [TestMethod]
        public async Task Load_class_arguments_and_annotations_without_showing_them_to_string_rules()
        {
            var src = Path.Combine(_project, "program", "src", "com", "acme");
            File.WriteAllText(Path.Combine(src, "Exporter.java"), "package com.acme;\n\npublic interface Exporter { }\n");
            File.WriteAllText(Path.Combine(src, "PdfExporter.java"), "package com.acme;\n\npublic class PdfExporter implements Exporter { }\n");
            File.WriteAllText(Path.Combine(src, "AppModule.java"),
                "package com.acme;\n\nimport com.google.inject.Provides;\n\npublic class AppModule {\n" +
                "    void configure() {\n        bind(Exporter.class).to(PdfExporter.class);\n        named(\"main\");\n    }\n" +
                "    @Provides\n    Exporter make() { return null; }\n}\n");
            Scan();
            var loaded = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, loaded.Failed, loaded.FirstError);
            Assert.AreEqual(0, (await _store.CallSitesAsync(_project, "bind")).Count, "solo tipi: invisibile alle regole sulle stringhe");
            Assert.AreEqual("main", (await _store.CallSitesAsync(_project, "named")).Single().Arguments[0]);
        }

        [TestMethod]
        public async Task Link_page_controls_to_the_functions_of_the_scripts_the_page_loads()
        {
            var pages = Path.Combine(_project, "program", "web", "pagine");
            Directory.CreateDirectory(Path.Combine(pages, "parti"));
            File.WriteAllText(Path.Combine(pages, "ordini.page"),
                "<script src=\"../page.js\"></script>\n{%include(parti/menu)%}\n<button id=\"scegli\" onclick=\"scegli(1)\">x</button>\n<a id=\"menu\" href=\"javascript:apriMenu()\">m</a>\n<i id=\"altro\" onclick=\"nessuna()\">y</i>\n");
            File.WriteAllText(Path.Combine(pages, "parti", "menu.page"), "<script>\nfunction apriMenu() { }\n</script>\n");
            Scan();
            var dialect = new TemplatePageExtractor(DeclarativeLanguage.Parse(TemplatePageExtractorShould.Dialect, "/p/.mde-languages/pagine/language.yml"));
            var all = Directory.GetFiles(pages, "*.page", SearchOption.AllDirectories).OrderBy(f => f).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            foreach (var (file, ast) in dialect.Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = all, FilesToEmit = all }).Scripts)
            {
                var path = CodeScriptWriter.ScriptPathFor(_project, file.LogicalPath, CodeScriptKind.Ast);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, CodeScriptWriter.Serialize(ast));
            }
            var loaded = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, loaded.Failed, loaded.FirstError);

            Assert.AreEqual(0, await _rules.ApplyPageScriptsAsync(_project, new[] { "altro-dialetto" }), "solo le pagine dei dialetti dati");
            var linked = await _rules.ApplyPageScriptsAsync(_project, new[] { "pagine" });
            var script = CodeRuleService.ScriptPath(_project, CodeRuleService.PageScriptsId);
            Assert.AreEqual(2, linked, File.ReadAllText(script));
            Assert.IsTrue(File.Exists(script));
            Assert.AreEqual(0, _rules.List(_project).Count, "non è una regola dell'utente");

            var scegli = (await _store.ResolveAsync(_project, "program/web/pagine/ordini.page:scegli")).Single(s => s.Kind == "control").Id;
            var toJs = (await _store.NeighborsAsync(_project, scegli, "calls", "out", 1, 10)).Single();
            Assert.AreEqual(("extracted", "function", "program/web/page.js"), (toJs.Confidence, toJs.Other.Kind, toJs.Other.Path), "la funzione del file che la pagina carica");
            var menu = (await _store.ResolveAsync(_project, "program/web/pagine/ordini.page:menu")).Single(s => s.Kind == "control").Id;
            Assert.AreEqual("program/web/pagine/parti/menu.page", (await _store.NeighborsAsync(_project, menu, "calls", "out", 1, 10)).Single().Other.Path, "la funzione della pagina inclusa");

            var before = File.GetLastWriteTimeUtc(script);
            System.Threading.Thread.Sleep(20);
            Assert.AreEqual(2, await _rules.ApplyPageScriptsAsync(_project, new[] { "pagine" }));
            Assert.AreEqual(before, File.GetLastWriteTimeUtc(script), "lo stesso risultato non riscrive il file");
            Assert.AreEqual(0, await _rules.ApplyPageScriptsAsync(_project, Array.Empty<string>()));
            Assert.IsFalse(File.Exists(script), "senza dialetti di pagine il file se ne va");
            Assert.AreEqual(0, (await _store.NeighborsAsync(_project, scegli, "calls", "out", 1, 10)).Count);
        }

        [TestMethod]
        public async Task Link_an_angular_template_to_its_component_methods_and_child_components()
        {
            var app = Path.Combine(_project, "program", "app");
            Directory.CreateDirectory(app);
            File.WriteAllText(Path.Combine(app, "x.component.ts"),
                "import { Component } from '@angular/core';\n@Component({ selector: 'app-x', templateUrl: './x.component.html' })\n" +
                "export class XComponent {\n  salva(): void { }\n  annulla(): void { }\n}\n");
            File.WriteAllText(Path.Combine(app, "dettaglio.component.ts"),
                "import { Component } from '@angular/core';\n@Component({ selector: 'app-dettaglio', template: '<p></p>' })\nexport class DettaglioComponent { }\n");
            File.WriteAllText(Path.Combine(app, "x.component.html"),
                "<div *ngIf=\"admin\">\n<button id=\"ok\" (click)=\"salva()\">ok</button>\n</div>\n<app-dettaglio></app-dettaglio>\n<i id=\"x\" (click)=\"nessuno()\">x</i>\n");
            var grammars = new GrammarStore(_root);
            Directory.CreateDirectory(grammars.Folder);
            var ts = NuGetGrammar(grammars, "typescript");
            if (!File.Exists(ts)) Assert.Inconclusive("grammatica typescript non trovata nel NuGet in cache: " + ts);
            grammars.Install(GrammarCatalog.ById("typescript"), ts, "1.3.0", "nuget");
            void Emit(ICodeExtractor extractor, string pattern)
            {
                var files = Directory.GetFiles(app, pattern).OrderBy(f => f).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
                foreach (var (file, ast) in extractor.Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files }).Scripts)
                {
                    var path = CodeScriptWriter.ScriptPathFor(_project, file.LogicalPath, CodeScriptKind.Ast);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, CodeScriptWriter.Serialize(ast));
                }
            }
            Emit(new TreeSitterTypeScriptExtractor(grammars, NullLogger<TreeSitterTypeScriptExtractor>.Instance), "*.ts");
            Emit(new TemplatePageExtractor(DeclarativeLanguage.Parse(TemplatePageExtractorShould.Angular, "/p/.mde-languages/angular/language.yml")), "*.html");
            var loaded = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, loaded.Failed, loaded.FirstError);

            Assert.AreEqual(3, await _rules.ApplyPageScriptsAsync(_project, new[] { "angular" }), File.Exists(CodeRuleService.ScriptPath(_project, CodeRuleService.PageScriptsId)) ? File.ReadAllText(CodeRuleService.ScriptPath(_project, CodeRuleService.PageScriptsId)) : "");
            var comp = (await _store.ResolveAsync(_project, "program/app/x.component:XComponent")).Single(x => x.Kind == "component").Id;
            var renders = (await _store.NeighborsAsync(_project, comp, "renders", "out", 1, 10)).Single();
            Assert.AreEqual(("module", "program/app/x.component.html"), (renders.Other.Kind, renders.Other.Path));
            var ok = (await _store.ResolveAsync(_project, "program/app/x.component.html:ok")).Single(x => x.Kind == "control").Id;
            var salva = (await _store.NeighborsAsync(_project, ok, "calls", "out", 1, 10)).Single();
            Assert.AreEqual(("method", "salva", "extracted"), (salva.Other.Kind, salva.Other.Name, salva.Confidence), "(click)=salva() → il metodo del componente");
            var child = (await _store.ResolveAsync(_project, "program/app/x.component.html:app-dettaglio@4")).Single(x => x.Kind == "control").Id;
            Assert.AreEqual("DettaglioComponent", (await _store.NeighborsAsync(_project, child, "instantiates", "out", 1, 10)).Single().Other.Name);
        }

        [TestMethod]
        public async Task Link_an_inline_angular_template_to_its_component()
        {
            var app = Path.Combine(_project, "program", "app");
            Directory.CreateDirectory(app);
            File.WriteAllText(Path.Combine(app, "x.component.ts"),
                "import { Component } from '@angular/core';\n@Component({ selector: 'app-x', template: `\n<div *ngIf=\"admin\">\n<button id=\"ok\" (click)=\"salva()\">ok</button>\n</div>\n<app-dettaglio></app-dettaglio>\n` })\n" +
                "export class XComponent {\n  salva(): void { }\n}\n");
            File.WriteAllText(Path.Combine(app, "dettaglio.component.ts"),
                "import { Component } from '@angular/core';\n@Component({ selector: 'app-dettaglio', templateUrl: './dettaglio.component.html' })\nexport class DettaglioComponent { }\n");
            var package = Path.Combine(DeclarativeLanguage.FolderOf(_project), "angular");
            Directory.CreateDirectory(package);
            File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), TemplatePageExtractorShould.Angular);
            var catalog = CodeLanguageCatalog.ForProject(_project);
            var grammars = new GrammarStore(_root);
            Directory.CreateDirectory(grammars.Folder);
            var ts = NuGetGrammar(grammars, "typescript");
            if (!File.Exists(ts)) Assert.Inconclusive("grammatica typescript non trovata nel NuGet in cache: " + ts);
            grammars.Install(GrammarCatalog.ById("typescript"), ts, "1.3.0", "nuget");
            var sources = Directory.GetFiles(app, "*.ts").OrderBy(f => f).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            void Emit(ICodeExtractor extractor, List<CodeSourceFile> files)
            {
                foreach (var (file, ast) in extractor.Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files }).Scripts)
                {
                    var path = CodeScriptWriter.ScriptPathFor(_project, file.LogicalPath, CodeScriptKind.Ast);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, CodeScriptWriter.Serialize(ast));
                }
            }
            Emit(new TreeSitterTypeScriptExtractor(grammars, NullLogger<TreeSitterTypeScriptExtractor>.Instance), sources);
            Emit(new CodeExtractorRegistry(Array.Empty<ICodeExtractor>()).ForProject(catalog).Get("angular"),
                CodeInlinePages.Expand(catalog, sources).Where(f => CodeInlinePages.IsInline(f.LogicalPath)).ToList());
            var loaded = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, loaded.Failed, loaded.FirstError);

            Assert.AreEqual(3, await _rules.ApplyPageScriptsAsync(_project, new[] { "angular" }), File.Exists(CodeRuleService.ScriptPath(_project, CodeRuleService.PageScriptsId)) ? File.ReadAllText(CodeRuleService.ScriptPath(_project, CodeRuleService.PageScriptsId)) : "");
            var comp = (await _store.ResolveAsync(_project, "program/app/x.component:XComponent")).Single(x => x.Kind == "component").Id;
            var renders = (await _store.NeighborsAsync(_project, comp, "renders", "out", 1, 10)).Single();
            Assert.AreEqual(("module", "program/app/x.component.ts#template"), (renders.Other.Kind, renders.Other.Path));
            var ok = (await _store.ResolveAsync(_project, "program/app/x.component.ts#template:ok")).Single(x => x.Kind == "control");
            Assert.AreEqual(("admin", 4), (ok.Guard, ok.LineStart), "condizione e riga del .ts");
            var salva = (await _store.NeighborsAsync(_project, ok.Id, "calls", "out", 1, 10)).Single();
            Assert.AreEqual(("method", "salva", "extracted"), (salva.Other.Kind, salva.Other.Name, salva.Confidence));
            var child = (await _store.ResolveAsync(_project, "program/app/x.component.ts#template:app-dettaglio@6")).Single(x => x.Kind == "control").Id;
            Assert.AreEqual("DettaglioComponent", (await _store.NeighborsAsync(_project, child, "instantiates", "out", 1, 10)).Single().Other.Name);
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
