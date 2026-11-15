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
        public async Task Take_the_class_from_the_pages_own_code_behind_file_never_a_namesake()
        {
            // 14/11/2026, dal cliente: una pagina col code-behind vuoto (o escluso dalla compilazione) si agganciava alla classe
            // omonima di un'altra pagina o di un controllo. Con il suo file accanto, la classe è quella dichiarata lì, o nessuna
            var store = FusekiFixture.NewStore();
            var root = Path.Combine(Path.GetTempPath(), "mde-codebehind-tests", Guid.NewGuid().ToString("N"));
            var project = Path.Combine(root, "project");
            try
            {
                var web = Path.Combine(project, "web");
                Directory.CreateDirectory(Path.Combine(web, "altro"));
                var package = Path.Combine(DeclarativeLanguage.FolderOf(project), "pagine");
                Directory.CreateDirectory(package);
                File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Dialect);
                // Elenco: il suo code-behind c'è ma è vuoto; un'altra pagina, altrove, ha una classe con lo stesso nome
                File.WriteAllText(Path.Combine(web, "Elenco.pag"), "<%@ Page Language=\"VB\" AutoEventWireup=\"true\" Inherits=\"Elenco\" %>\n");
                File.WriteAllText(Path.Combine(web, "Elenco.pag.vb"), "' vuoto\n");
                File.WriteAllText(Path.Combine(web, "altro", "Elenco.pag"), "<%@ Page Language=\"VB\" AutoEventWireup=\"true\" Inherits=\"elenco\" %>\n");
                File.WriteAllText(Path.Combine(web, "altro", "Elenco.pag.vb"),
                    "Namespace Demo\n  Public Class Elenco\n    Protected Sub Page_Load(sender As Object, e As EventArgs)\n    End Sub\n  End Class\nEnd Namespace\n");
                // Comune: nessun file suo, eredita una classe comune dichiarata altrove
                File.WriteAllText(Path.Combine(web, "Comune.pag"), "<%@ Page Language=\"VB\" Inherits=\"Demo.Base\" %>\n");
                File.WriteAllText(Path.Combine(web, "Base.vb"), "Namespace Demo\n  Public Class Base\n  End Class\nEnd Namespace\n");
                void Emit(ICodeExtractor extractor, string pattern)
                {
                    var files = Directory.GetFiles(web, pattern, SearchOption.AllDirectories).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(project, f))).ToList();
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

                var elenco = (await store.ResolveAsync(project, "Demo.Elenco")).Single(x => x.Kind == "class").Id;
                Assert.AreEqual("web/altro/Elenco.pag", string.Join(" | ", (await store.NeighborsAsync(project, elenco, "renders", "out", 1, 5)).Select(e => e.Other.QualifiedName)),
                    "solo la pagina che ha la classe nel suo file (il nome nella direttiva non distingue le maiuscole); quella col code-behind vuoto resta senza classe");
                var load2 = (await store.ResolveAsync(project, "Elenco.Page_Load")).Single(x => x.Kind == "method").Id;
                Assert.AreEqual("web/altro/Elenco.pag", string.Join(" | ", (await store.NeighborsAsync(project, load2, "calls", "in", 1, 5)).Select(e => e.Other.QualifiedName)),
                    "e Page_Load lo chiama solo la sua pagina");
                var common = (await store.ResolveAsync(project, "Demo.Base")).Single(x => x.Kind == "class").Id;
                Assert.AreEqual("web/Comune.pag", (await store.NeighborsAsync(project, common, "renders", "out", 1, 5)).Single().Other.QualifiedName,
                    "una pagina senza un file suo eredita la classe comune, cercata per nome");
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
        [TestMethod]
        public async Task Start_from_the_page_the_methods_a_library_calls_by_itself_the_nearest_redefinition_first()
        {
            // 15/11/2026, dal cliente: «Overrides Sub OnLoad» di una pagina non aveva chi lo fa partire, e il codice che
            // chiama risultava non raggiunto. Lo chiama la libreria della classe base, quindi parte dalla pagina; se lo
            // ridefinisce anche una classe base del perimetro, gira la ridefinizione più vicina alla pagina
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
                File.WriteAllText(Path.Combine(web, "Listino.pag"), "<%@ Page Language=\"vb\" AutoEventWireup=\"false\" Inherits=\"Demo.Listino\" %>\n");
                File.WriteAllText(Path.Combine(web, "Orfana.pag"), "<%@ Page Language=\"vb\" AutoEventWireup=\"false\" Inherits=\"Demo.NonCompilata\" %>\n");
                File.WriteAllText(Path.Combine(web, "Listino.pag.vb"),
                    "Imports System.Collections.ObjectModel\n" +
                    "Namespace Demo\n" +
                    "  Public Class Radice\n    Inherits Collection(Of String)\n    Public Event Load As EventHandler\n  End Class\n" +
                    "  Public Class PaginaBase\n    Inherits Radice\n" +
                    "    Private Sub Base_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load\n    End Sub\n" +
                    "    Protected Overrides Sub ClearItems()\n    End Sub\n" +
                    "    Protected Overrides Sub InsertItem(ByVal index As Integer, ByVal item As String)\n    End Sub\n  End Class\n" +
                    "  Public Class Listino\n    Inherits PaginaBase\n    Implements IDisposable\n" +
                    "    Protected Overrides Sub InsertItem(ByVal index As Integer, ByVal item As String)\n    End Sub\n" +
                    "    Public Overrides Function ToString() As String\n      Return \"\"\n    End Function\n" +
                    "    Public Sub Dispose() Implements IDisposable.Dispose\n    End Sub\n" +
                    "    Public Sub Altro()\n    End Sub\n  End Class\n" +
                    "End Namespace\n");
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

                var page = (await store.ResolveAsync(project, "web/Listino.pag")).Single(x => x.Kind == "module").Id;
                var started = (await store.NeighborsAsync(project, page, "calls", "out", 1, 20))
                    .Select(e => e.Other.QualifiedName + ":" + e.Confidence).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                CollectionAssert.AreEqual(new[]
                {
                    "Demo.Listino.Dispose():extracted",
                    "Demo.Listino.InsertItem(Integer, String):extracted",
                    "Demo.PaginaBase.Base_Load(Object, System.EventArgs):extracted",
                    "Demo.PaginaBase.ClearItems():extracted",
                }, started, "non PaginaBase.InsertItem (ridefinito più sotto), non ToString (di System.Object), non Altro; il gestore " +
                    "«Handles MyBase.Load» della classe base gira anche per la pagina che eredita");

                // la pagina che dichiara una classe che nel grafo non c'è (code-behind escluso dalla compilazione): si sa quale è
                var orphan = (await store.ResolveAsync(project, "web/Orfana.pag")).Single(x => x.Kind == "module").Id;
                CollectionAssert.AreEqual(new[] { orphan }, await store.PagesWithoutClassAsync(project, "web/"));
                Assert.AreEqual(0, (await store.PagesWithoutClassAsync(project, "altro/")).Count);
            }
            finally { try { Directory.Delete(root, true); } catch { } }
        }
    }
}
