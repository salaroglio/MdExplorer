using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.CodeGraph.Grammars;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// F3: la grammatica TypeScript si carica per percorso assoluto dallo store delle grammatiche (qui riempito dal
    /// NuGet in cache, come farebbe il download), l'estrattore produce moduli, classi Angular con kind proprio,
    /// metodi, parametri, import e chiamate con la confidenza giusta. Senza grammatica si dichiara non pronto.
    /// </summary>
    [TestClass]
    public class TreeSitterTypeScriptExtractorShould
    {
        private string _root, _project, _src;
        private GrammarStore _store;

        private static string NuGetGrammar(GrammarStore store)
        {
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            return Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", store.Rid, "native", GrammarCatalog.ById("typescript").FileName(store.Rid));
        }

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-ts-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "src", "app");
            Directory.CreateDirectory(Path.Combine(_src, "services"));
            _store = new GrammarStore(_root);
            File.WriteAllText(Path.Combine(_src, "services", "md-file.service.ts"),
                "import { Injectable } from '@angular/core';\nexport interface Hit { id: string; }\n@Injectable({ providedIn: 'root' })\nexport class MdFileService {\n  get(path: string): Promise<Hit> { return fetch(path) as any; }\n}\n");
            File.WriteAllText(Path.Combine(_src, "app.module.ts"),
                "import { NgModule } from '@angular/core';\nimport { CommonModule } from '@angular/common';\nimport { RouterModule } from '@angular/router';\n" +
                "import { XComponent } from './x.component';\nimport { MdFileService } from './services/md-file.service';\n" +
                "@NgModule({\n  declarations: [XComponent],\n  imports: [CommonModule, RouterModule.forRoot([])],\n  providers: [MdFileService],\n  bootstrap: [XComponent],\n})\nexport class AppModule {}\n");
            File.WriteAllText(Path.Combine(_src, "x.component.ts"),
                "import { Component, OnInit } from '@angular/core';\nimport { MdFileService, Hit } from './services/md-file.service';\n" +
                "export function helper(a: number, b?: string): boolean { return a > 0; }\n" +
                "@Component({ selector: 'app-x', templateUrl: './x.component.html' })\nexport class XComponent implements OnInit {\n  private items: Hit[] = [];\n" +
                "  constructor(private svc: MdFileService, public n: number = 3) {}\n" +
                "  ngOnInit(): void { this.load('a'); helper(1); new Date(); }\n" +
                "  async load(path: string, ...rest: string[]): Promise<void> { await this.svc.get(path); }\n" +
                "  get count(): number { return this.items.length; }\n}\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(_root, true); } catch { } }

        private CodeExtractionRequest Request()
        {
            var files = Directory.GetFiles(_src, "*.ts", SearchOption.AllDirectories).OrderBy(f => f)
                .Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            return new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files };
        }

        [TestMethod]
        public void Declare_itself_not_ready_without_the_grammar_and_refuse_to_extract()
        {
            var extractor = new TreeSitterTypeScriptExtractor(_store, NullLogger<TreeSitterTypeScriptExtractor>.Instance);
            StringAssert.Contains(extractor.Availability, "grammatica da scaricare");
            var registry = new CodeExtractorRegistry(new ICodeExtractor[] { extractor });
            Assert.IsNull(registry.Get("typescript"), "non pronto = non si usa");
            Assert.ThrowsException<InvalidOperationException>(() => extractor.Extract(Request()));
        }

        [TestMethod]
        public void Record_the_template_and_the_selector_of_a_component_for_the_pages()
        {
            var src = NuGetGrammar(_store);
            if (!File.Exists(src)) Assert.Inconclusive("Grammatica typescript non in cache NuGet: " + src);
            _store.Install(GrammarCatalog.ById("typescript"), src, "1.3.0", "test");
            var extractor = new TreeSitterTypeScriptExtractor(_store, NullLogger<TreeSitterTypeScriptExtractor>.Instance);
            Assert.AreEqual("6", extractor.ExtractorVersion);
            var x = extractor.Extract(Request()).Scripts.Single(s => s.File.LogicalPath == "src/app/x.component.ts").Script;
            var comp = x.Symbols.Single(s => s.Name == "XComponent").Id;
            var view = x.CallSites.Where(c => c.SourceId == comp).ToDictionary(c => c.Callee, c => c.Arguments.Single().Value);
            Assert.AreEqual("src/app/x.component.html", view["templateUrl"], "il percorso logico, risolto rispetto al .ts");
            Assert.AreEqual("app-x", view["selector"]);
        }

        [TestMethod]
        public void Keep_one_node_for_a_getter_and_setter_pair_so_the_script_loads()
        {
            var src = NuGetGrammar(_store);
            if (!File.Exists(src)) Assert.Inconclusive("Grammatica typescript non in cache NuGet: " + src);
            _store.Install(GrammarCatalog.ById("typescript"), src, "1.3.0", "test");
            File.WriteAllText(Path.Combine(_src, "source.ts"),
                "export class Source {\n  private _d: string[] = [];\n  get data(): string[] { return this._d; }\n  set data(v: string[]) { this._d = v; }\n}\n");
            var script = new TreeSitterTypeScriptExtractor(_store, NullLogger<TreeSitterTypeScriptExtractor>.Instance).Extract(Request())
                .Scripts.Single(s => s.File.LogicalPath == "src/app/source.ts").Script;
            var data = script.Symbols.Single(s => s.Name == "data");
            Assert.AreEqual((3, 4), (data.LineStart, data.LineEnd), "un nodo che copre get e set");
            Assert.AreEqual(1, script.Parameters.Count(p => p.SymbolId == data.Id));
            var back = CodeScriptTurtle.Read(CodeScriptTurtle.Write(script), "src/app/source.ts.ast.ttl");
            Assert.AreEqual(script.Symbols.Count, back.Symbols.Count, "lo script si rilegge: niente nodi doppi");
        }

        [TestMethod]
        public void Extract_angular_modules_classes_members_imports_and_calls_with_the_grammar_installed()
        {
            var src = NuGetGrammar(_store);
            if (!File.Exists(src)) Assert.Inconclusive("Grammatica typescript non in cache NuGet: " + src);
            _store.Install(GrammarCatalog.ById("typescript"), src, "1.3.0", "test");
            var extractor = new TreeSitterTypeScriptExtractor(_store, NullLogger<TreeSitterTypeScriptExtractor>.Instance);
            Assert.AreEqual("ready", extractor.Availability);

            var result = extractor.Extract(Request());
            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            Assert.AreEqual(3, result.Scripts.Count);

            var x = result.Scripts.Single(s => s.File.LogicalPath == "src/app/x.component.ts").Script;
            var module = x.Symbols.Single(s => s.Kind == "module");
            Assert.AreEqual("src/app/x.component", module.QualifiedName);
            var comp = x.Symbols.Single(s => s.Name == "XComponent");
            Assert.AreEqual("component", comp.Kind, "@Component → kind component");
            Assert.AreEqual("src/app/x.component:XComponent", comp.QualifiedName);
            Assert.AreEqual(5, comp.LineStart);
            var load = x.Symbols.Single(s => s.Name == "load");
            Assert.AreEqual("method", load.Kind); Assert.IsTrue(load.IsAsync); Assert.AreEqual("Promise<void>", load.ReturnType);
            StringAssert.StartsWith(load.Signature, "async load(path: string, ...rest: string[]): Promise<void>");
            var prms = x.Parameters.Where(p => p.SymbolId == load.Id).OrderBy(p => p.Position).ToList();
            Assert.AreEqual(2, prms.Count); Assert.AreEqual("string", prms[0].ParameterType); Assert.AreEqual("params", prms[1].Direction);
            var ctor = x.Symbols.Single(s => s.Kind == "constructor");
            var ctorPrms = x.Parameters.Where(p => p.SymbolId == ctor.Id).OrderBy(p => p.Position).ToList();
            Assert.AreEqual("MdFileService", ctorPrms[0].ParameterType); Assert.AreEqual("3", ctorPrms[1].DefaultValue);
            Assert.AreEqual("property", x.Symbols.Single(s => s.Name == "count").Kind, "get count → property");
            Assert.AreEqual("field", x.Symbols.Single(s => s.Name == "items").Kind);
            Assert.AreEqual("function", x.Symbols.Single(s => s.Name == "helper").Kind);
            Assert.IsTrue(x.Parameters.Single(p => p.Name == "b").IsNullable, "b?: string → nullable");

            // import relativo → modulo in perimetro; @angular/core → external
            var serviceModuleId = RoslynCSharpExtractor.StableId(CodeLanguages.TypeScript, "module", "src/app/services/md-file.service");
            Assert.IsTrue(x.Edges.Any(e => e.SourceId == module.Id && e.TargetId == serviceModuleId && e.Relation == "imports"));
            var angular = x.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "@angular/core");
            Assert.IsTrue(x.Edges.Any(e => e.SourceId == module.Id && e.TargetId == angular.Id && e.Relation == "imports"));

            // implements OnInit (external, extracted); references MdFileService (in perimetro, inferred); instantiates Date (external)
            Assert.IsTrue(x.Edges.Any(e => e.SourceId == comp.Id && e.Relation == "implements"));
            var serviceId = RoslynCSharpExtractor.StableId(CodeLanguages.TypeScript, "service", "src/app/services/md-file.service:MdFileService");
            Assert.IsTrue(x.Edges.Any(e => e.SourceId == ctor.Id && e.TargetId == serviceId && e.Relation == "references" && e.Confidence == "inferred"));
            Assert.IsTrue(x.Edges.Any(e => e.Relation == "instantiates"));

            // calls: this.load → extracted; helper → inferred; this.svc.get → inferred (unico 'get' nel perimetro)
            var ngOnInit = x.Symbols.Single(s => s.Name == "ngOnInit");
            Assert.IsTrue(x.Edges.Any(e => e.SourceId == ngOnInit.Id && e.TargetId == load.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var helper = x.Symbols.Single(s => s.Name == "helper");
            Assert.IsTrue(x.Edges.Any(e => e.SourceId == ngOnInit.Id && e.TargetId == helper.Id && e.Relation == "calls" && e.Confidence == "inferred"));
            var getId = RoslynCSharpExtractor.StableId(CodeLanguages.TypeScript, "method", "src/app/services/md-file.service:MdFileService.get");
            Assert.IsTrue(x.Edges.Any(e => e.SourceId == load.Id && e.TargetId == getId && e.Relation == "calls" && e.Confidence == "inferred"));

            // il servizio: @Injectable → service; il formato si rilegge e si carica
            var svc = result.Scripts.Single(s => s.File.LogicalPath == "src/app/services/md-file.service.ts").Script;
            Assert.AreEqual("service", svc.Symbols.Single(s => s.Name == "MdFileService").Kind);

            // Metadati Angular: il modulo dichiara il componente (contains), importa i moduli (imports), fornisce il service (references);
            // selettore e template nella firma del componente.
            var appModule = result.Scripts.Single(s => s.File.LogicalPath == "src/app/app.module.ts").Script;
            var ngModule = appModule.Symbols.Single(s => s.Kind == "ngmodule");
            Assert.AreEqual("src/app/app.module:AppModule", ngModule.QualifiedName);
            Assert.IsTrue(appModule.Edges.Any(e => e.SourceId == ngModule.Id && e.TargetId == comp.Id && e.Relation == "contains" && e.Confidence == "inferred"), "declarations: [XComponent]");
            Assert.IsTrue(appModule.Edges.Any(e => e.SourceId == ngModule.Id && e.TargetId == serviceId && e.Relation == "references"), "providers: [MdFileService]");
            Assert.IsTrue(appModule.Edges.Any(e => e.SourceId == ngModule.Id && e.TargetId == comp.Id && e.Relation == "references"), "bootstrap: [XComponent]");
            var imported = appModule.Edges.Where(e => e.SourceId == ngModule.Id && e.Relation == "imports").Select(e => appModule.Symbols.Single(s => s.Id == e.TargetId).Name).ToList();
            CollectionAssert.IsSubsetOf(new[] { "CommonModule", "RouterModule" }, imported, "imports: [CommonModule, RouterModule.forRoot([])] → external");
            StringAssert.Contains(comp.Signature, "selector: app-x");
            foreach (var (file, script) in result.Scripts)
                CodeScriptParser.Parse(CodeScriptWriter.Serialize(script), file.LogicalPath);
        }
    }
}
