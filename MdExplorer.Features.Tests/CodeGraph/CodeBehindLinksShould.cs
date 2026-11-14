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

        [TestMethod]
        public async Task Link_server_controls_and_the_page_to_the_handlers_wired_in_the_code_behind()
        {
            // 13/11/2026, rilevazione dal cliente (Web Forms 1.1): AutoEventWireup="false", nessun evento nel markup, i gestori
            // agganciati con Handles e AddHandler; la pagina non arrivava a nessuno dei 1494 gestori
            var store = FusekiFixture.NewStore();
            var root = Path.Combine(Path.GetTempPath(), "mde-codebehind-tests", Guid.NewGuid().ToString("N"));
            var project = Path.Combine(root, "project");
            try
            {
                var web = Path.Combine(project, "web");
                Directory.CreateDirectory(web);
                var package = Path.Combine(DeclarativeLanguage.FolderOf(project), "pagine");
                Directory.CreateDirectory(package);
                File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Dialect + "  controls:\n    runat: '^server$'\n");
                File.WriteAllText(Path.Combine(web, "Ordini.pag"),
                    "<%@ Page Language=\"vb\" AutoEventWireup=\"false\" Codebehind=\"Ordini.pag.vb\" Inherits=\"Demo.Ordini\" %>\n" +
                    "<asp:Button id=\"btnSalva\" runat=\"server\" Text=\"Salva\" />\n" +
                    "<asp:Button id=\"btnAltro\" runat=\"server\" />\n" +
                    "<asp:Label id=\"lblNota\" runat=\"server\" />\n" +
                    "<asp:Button id=\"btnTerzo\" runat=\"server\" />\n" +
                    "<div id=\"fuori\">testo</div>\n");
                File.WriteAllText(Path.Combine(web, "Ordini.pag.vb"),
                    "Namespace Demo\n" +
                    "  Public Class Bottone\n    Public Event Click As EventHandler\n  End Class\n" +
                    "  Public Class Pagina\n    Public Event Load As EventHandler\n  End Class\n" +
                    "  Public Class Servizio\n    Public Shared Sub Esegui(ByVal sender As Object, ByVal e As EventArgs)\n    End Sub\n  End Class\n" +
                    "  Public Class Ordini\n    Inherits Pagina\n" +
                    "    Protected WithEvents btnSalva As Bottone\n    Protected WithEvents btnAltro As Bottone\n    Protected lblNota As Object\n    Protected btnTerzo As Bottone\n" +
                    "    Private Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load\n" +
                    "      AddHandler btnAltro.Click, AddressOf btnAltro_Click\n" +
                    "      AddHandler btnTerzo.Click, AddressOf Servizio.Esegui\n    End Sub\n" +
                    "    Private Sub btnSalva_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSalva.Click\n    End Sub\n" +
                    "    Private Sub btnAltro_Click(ByVal sender As Object, ByVal e As EventArgs)\n    End Sub\n" +
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
                await new CodeRuleService(store, NullLogger<CodeRuleService>.Instance).ApplyPageScriptsAsync(project, new[] { "pagine" });

                async Task<string> Callers(string method)
                {
                    var id = (await store.ResolveAsync(project, method)).Single(x => x.Kind == "method").Id;
                    return string.Join(" ", (await store.NeighborsAsync(project, id, "calls", "in", 1, 10))
                        .Where(e => e.Other.Kind is "control" or "module" or "method").Select(e => e.Other.Kind + ":" + e.Other.Name).OrderBy(x => x, StringComparer.Ordinal));
                }
                Assert.AreEqual("control:btnSalva", await Callers("Ordini.btnSalva_Click"), "Handles btnSalva.Click: il controllo della pagina con quell'id");
                Assert.AreEqual("control:btnAltro", await Callers("Ordini.btnAltro_Click"), "AddHandler: il controllo sì, Page_Load che lo registra no (AddressOf non è una chiamata)");
                Assert.AreEqual("module:Ordini.pag", await Callers("Ordini.Page_Load"), "Handles MyBase.Load: la pagina, che solleva il suo Load");
                Assert.AreEqual("control:btnTerzo", await Callers("Servizio.Esegui"), "AddHandler verso un gestore di un'altra classe: il controllo ci arriva lo stesso");

                var page = (await store.OutlineAsync(project, "web/Ordini.pag")).Where(x => x.Kind == "control").Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal);
                CollectionAssert.AreEqual(new[] { "btnAltro", "btnSalva", "btnTerzo", "lblNota" }, page.ToArray(), "i controlli runat=server con un id; non il div");
            }
            finally { try { Directory.Delete(root, true); } catch { } }
        }
    }
}
