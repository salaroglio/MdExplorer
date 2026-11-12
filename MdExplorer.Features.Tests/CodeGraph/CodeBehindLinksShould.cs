using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// 12/11/2026: una pagina con il suo code-behind (Web Forms). Un dialetto di pagine riconosce tre chiamate — codebehind,
    /// handler, autowire — e MdExplorer collega la classe alla pagina, il controllo al suo gestore, la pagina ai Page_….
    /// Il dialetto qui è inventato: il pacchetto vero sta fuori da questo repository.
    /// </summary>
    [TestClass]
    public class CodeBehindLinksShould
    {
        private const string Dialect = @"schema: 1
id: pagine
name: Pagine con code-behind
version: 1
extensions: [.pag]
ignoreCase: true
template:
  events: ['^on[a-z]+$']
  recognizers:
    - role: call
      match: '<%@\s*Page\b[^%]*?\bInherits\s*=\s*""(?<a0>[\w.]+)""'
      callee: codebehind
    - role: call
      match: '<%@\s*Page\b[^%]*?\bAutoEventWireup\s*=\s*""(?<a0>true)""'
      callee: autowire
    - role: call
      match: '\bOn(?!Client)[A-Z]\w*\s*=\s*""(?<a0>[A-Za-z_]\w*)""'
      callee: handler
";

        [TestMethod]
        public async Task Link_the_class_to_its_page_the_control_to_its_handler_and_the_page_to_page_load()
        {
            var store = FusekiFixture.NewStore();
            var root = Path.Combine(Path.GetTempPath(), "mde-codebehind-tests", Guid.NewGuid().ToString("N"));
            var project = Path.Combine(root, "project");
            try
            {
                var web = Path.Combine(project, "web");
                Directory.CreateDirectory(web);
                var package = Path.Combine(DeclarativeLanguage.FolderOf(project), "pagine");
                Directory.CreateDirectory(package);
                File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Dialect);
                File.WriteAllText(Path.Combine(web, "Ricerca.pag"),
                    "<%@ Page Language=\"VB\" AutoEventWireup=\"true\" Inherits=\"Demo.Ricerca\" %>\n" +
                    "<asp:Button ID=\"btnCerca\" runat=\"server\" OnClick=\"btnCerca_Click\" />\n" +
                    "<asp:Button ID=\"btnNulla\" runat=\"server\" OnClick=\"NonEsiste\" />\n");
                File.WriteAllText(Path.Combine(web, "Ricerca.pag.vb"),
                    "Namespace Demo\n  Public Class Ricerca\n" +
                    "    Protected Sub Page_Load(sender As Object, e As EventArgs)\n    End Sub\n" +
                    "    Protected Sub btnCerca_Click(sender As Object, e As EventArgs)\n    End Sub\n" +
                    "    Private Sub Aiuto()\n    End Sub\n" +
                    "  End Class\nEnd Namespace\n");
                void Emit(ICodeExtractor extractor, string pattern)
                {
                    var files = Directory.GetFiles(web, pattern).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(project, f))).ToList();
                    foreach (var (file, script) in extractor.Extract(new CodeExtractionRequest { ProjectPath = project, AllFiles = files, FilesToEmit = files }).Scripts)
                    {
                        var path = CodeScriptWriter.ScriptPathFor(project, file.LogicalPath, CodeScriptKind.Ast);
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        File.WriteAllText(path, CodeScriptWriter.Serialize(script));
                    }
                }
                Emit(new RoslynVisualBasicExtractor(NullLogger<RoslynVisualBasicExtractor>.Instance), "*.vb");
                Emit(new TemplatePageExtractor(CodeLanguageCatalog.ForProject(project).ById("pagine").Definition), "*.pag");
                var load = await store.LoadProjectAsync(project);
                Assert.AreEqual(0, load.Failed, load.FirstError);
                var rules = new CodeRuleService(store, NullLogger<CodeRuleService>.Instance);
                await rules.ApplyPageScriptsAsync(project, new[] { "pagine" });

                var cls = (await store.ResolveAsync(project, "Demo.Ricerca")).Single(x => x.Kind == "class").Id;
                Assert.AreEqual("web/Ricerca.pag", (await store.NeighborsAsync(project, cls, "renders", "out", 1, 5)).Single().Other.QualifiedName, "la classe di Inherits «renders» la pagina");
                async Task<string> Callers(string method)
                {
                    var id = (await store.ResolveAsync(project, method)).Single(x => x.Kind == "method").Id;
                    return string.Join(" ", (await store.NeighborsAsync(project, id, "calls", "in", 1, 5)).Select(e => e.Other.Name).OrderBy(x => x));
                }
                Assert.AreEqual("btnCerca", await Callers("Ricerca.btnCerca_Click"), "OnClick nel markup: il controllo chiama il metodo del code-behind");
                Assert.AreEqual("Ricerca.pag", await Callers("Ricerca.Page_Load"), "AutoEventWireup: la pagina chiama Page_Load");
                Assert.AreEqual("", await Callers("Ricerca.Aiuto"), "un metodo che nessuno nomina resta senza chiamante");
            }
            finally { try { Directory.Delete(root, true); } catch { } }
        }
    }
}
