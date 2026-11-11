using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Visual Basic .NET (11/11/2026): Roslyn senza SDK legge i .vb di una cartella e scrive gli stessi script AST del C#,
    /// con ciò che in VB è scritto diversamente: Module, parentesi facoltative, AddressOf, a(1) che è un indice.
    /// </summary>
    [TestClass]
    public class RoslynVisualBasicExtractorShould
    {
        private string _root, _project, _src;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-roslyn-vb-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "src", "Demo");
            Directory.CreateDirectory(_src);
            File.WriteAllText(Path.Combine(_src, "IFoo.vb"),
                "Namespace Demo\n  Public Interface IFoo\n    Function Bar(path As String, Optional retries As Integer = 3) As Integer\n  End Interface\nEnd Namespace\n");
            File.WriteAllText(Path.Combine(_src, "Foo.vb"),
                "Namespace Demo\n" +
                "  Public Class Foo\n" +
                "    Implements IFoo\n" +
                "    Private ReadOnly _baz As New Baz\n" +
                "    Public Property Name As String\n" +
                "    Public Function Bar(path As String, Optional retries As Integer = 3) As Integer Implements IFoo.Bar\n" +
                "      Console.WriteLine(path)\n" +
                "      Dim n As Integer\n" +
                "      _baz.Run(n)\n" +
                "      Return n\n" +
                "    End Function\n" +
                "  End Class\n" +
                "  Public Class Baz\n" +
                "    Public Sub Run(ByRef n As Integer)\n" +
                "      n = 1\n" +
                "    End Sub\n" +
                "  End Class\n" +
                "End Namespace\n");
            File.WriteAllText(Path.Combine(_src, "Client.vb"),
                "Namespace Demo\n" +
                "  Public Class Client\n" +
                "    Private ReadOnly _f As IFoo\n" +
                "    Public Sub New(f As IFoo)\n" +
                "      _f = f\n" +
                "    End Sub\n" +
                "    Public Sub Go()\n" +
                "      _f.Bar(\"x\")\n" +
                "      Missing.DoIt()\n" +
                "    End Sub\n" +
                "  End Class\n" +
                "End Namespace\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private CodeExtractionRequest Request()
        {
            var files = Directory.GetFiles(_src, "*.vb").OrderBy(f => f).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            return new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files };
        }

        private static RoslynVisualBasicExtractor Extractor() => new(NullLogger<RoslynVisualBasicExtractor>.Instance);

        [TestMethod]
        public void Be_in_the_catalog_for_vb_files()
        {
            Assert.AreEqual(CodeLanguages.VisualBasic, CodeLanguages.ForFile("src/Demo/Foo.vb")?.Id);
            Assert.AreEqual("ready", new CodeExtractorRegistry(new ICodeExtractor[] { Extractor() }).Availability(CodeLanguages.VisualBasic));
        }

        [TestMethod]
        public void Extract_symbols_parameters_and_resolved_edges_without_an_sdk()
        {
            var result = Extractor().Extract(Request());

            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            Assert.AreEqual(3, result.Scripts.Count);

            var foo = result.Scripts.Single(s => s.File.LogicalPath == "src/Demo/Foo.vb").Script;
            Assert.AreEqual(CodeLanguages.VisualBasic, foo.Header.Language);
            var bar = foo.Symbols.Single(s => s.Kind == "method" && s.Name == "Bar");
            Assert.AreEqual("Demo.Foo.Bar(String, Integer)", bar.QualifiedName);
            Assert.AreEqual("Integer", bar.ReturnType);
            Assert.AreEqual((6, 11), (bar.LineStart, bar.LineEnd), "le righe sono quelle del blocco, non della sola riga Function");
            var prms = foo.Parameters.Where(p => p.SymbolId == bar.Id).OrderBy(p => p.Position).ToList();
            Assert.AreEqual(2, prms.Count);
            Assert.AreEqual(("path", "String"), (prms[0].Name, prms[0].ParameterType));
            Assert.AreEqual(("retries", "3"), (prms[1].Name, prms[1].DefaultValue));
            var run = foo.Symbols.Single(s => s.Name == "Run");
            Assert.AreEqual("ref", foo.Parameters.Single(p => p.SymbolId == run.Id).Direction, "ByRef");

            var fooClass = foo.Symbols.Single(s => s.Kind == "class" && s.Name == "Foo");
            var ifooId = RoslynCSharpExtractor.StableId("visualbasic", "interface", "Demo.IFoo");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooClass.Id && e.TargetId == ifooId && e.Relation == "implements" && e.Confidence == "extracted"));
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooClass.Id && e.TargetId == bar.Id && e.Relation == "contains"));
            Assert.IsNotNull(foo.Symbols.SingleOrDefault(s => s.Kind == "property" && s.Name == "Name"));
            var field = foo.Symbols.Single(s => s.Kind == "field" && s.Name == "_baz");
            var bazClass = foo.Symbols.Single(s => s.Kind == "class" && s.Name == "Baz");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == field.Id && e.TargetId == bazClass.Id && e.Relation == "instantiates"), "As New Baz");
            var console = foo.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "System.Console.WriteLine(String)");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == console.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == run.Id && e.Relation == "calls" && e.Confidence == "extracted"));

            // dispatch per interfaccia e nome sconosciuto
            var client = result.Scripts.Single(s => s.File.LogicalPath == "src/Demo/Client.vb").Script;
            var go = client.Symbols.Single(s => s.Name == "Go");
            var ifaceBarId = RoslynCSharpExtractor.StableId("visualbasic", "method", "Demo.IFoo.Bar(String, Integer)");
            Assert.IsTrue(client.Edges.Any(e => e.SourceId == go.Id && e.TargetId == ifaceBarId && e.Relation == "calls" && e.Confidence == "extracted"));
            Assert.IsTrue(client.Edges.Any(e => e.SourceId == go.Id && e.TargetId == bar.Id && e.Relation == "calls" && e.Confidence == "inferred"));
            var missing = client.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "Missing.DoIt");
            Assert.IsTrue(client.Edges.Any(e => e.TargetId == missing.Id && e.Confidence == "ambiguous"));
            var ctor = client.Symbols.Single(s => s.Kind == "constructor");
            Assert.IsTrue(client.Edges.Any(e => e.SourceId == ctor.Id && e.TargetId == ifooId && e.Relation == "references"));
            Assert.IsFalse(foo.Edges.Concat(client.Edges).Any(e => e.TargetId == ifaceBarId && e.SourceId == bar.Id), "Implements IFoo.Bar non è una chiamata");
        }

        [TestMethod]
        public void Read_calls_the_way_vb_writes_them()
        {
            File.WriteAllText(Path.Combine(_src, "Util.vb"),
                "Namespace Demo\n" +
                "  Public Module Util\n" +
                "    Public Function Conta() As Integer\n" +
                "      Return 1\n" +
                "    End Function\n" +
                "    Public Sub Gestisci(sender As Object, e As EventArgs)\n" +
                "    End Sub\n" +
                "    Public Sub Lavora()\n" +
                "      Dim valori(3) As Integer\n" +
                "      Dim elenco As New List(Of String)\n" +
                "      Dim x = Conta\n" +
                "      x = valori(0) + elenco(0).Length\n" +
                "      Dim h As EventHandler = AddressOf Gestisci\n" +
                "      Dim nome = NameOf(Conta)\n" +
                "      Stampa\n" +
                "      MsgBox(\"fatto\")\n" +
                "    End Sub\n" +
                "    Private Sub Stampa()\n" +
                "    End Sub\n" +
                "  End Module\n" +
                "End Namespace\n");
            var result = Extractor().Extract(Request());
            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            var s = result.Scripts.Single(x => x.File.LogicalPath == "src/Demo/Util.vb").Script;

            var module = s.Symbols.Single(x => x.Name == "Util");
            Assert.AreEqual(("class", true), (module.Kind, module.IsStatic), "un Module è una classe statica");
            var lavora = s.Symbols.Single(x => x.Name == "Lavora").Id;
            string[] Called(string confidence) => s.Edges.Where(e => e.SourceId == lavora && e.Relation == "calls" && e.Confidence == confidence)
                .Select(e => s.Symbols.Single(x => x.Id == e.TargetId).Name).OrderBy(n => n, StringComparer.Ordinal).ToArray();

            CollectionAssert.AreEqual(new[] { "Conta", "Gestisci", "MsgBox", "Stampa" }, Called("extracted"),
                "senza parentesi, AddressOf, istruzione nuda e libreria VB; NameOf non chiama");
            CollectionAssert.AreEqual(new string[0], Called("ambiguous"), "valori(0) ed elenco(0) sono indici, non chiamate");
            Assert.AreEqual("Microsoft.VisualBasic.Interaction.MsgBox(Object, Microsoft.VisualBasic.MsgBoxStyle, Object)",
                s.Symbols.Single(x => x.Name == "MsgBox").QualifiedName);
        }

        [TestMethod]
        public void Keep_declare_functions_and_withevents_fields_as_what_they_are()
        {
            File.WriteAllText(Path.Combine(_src, "Nativo.vb"),
                "Namespace Demo\n" +
                "  Public Class Nativo\n" +
                "    Private Declare Function GetTickCount Lib \"kernel32\" () As Integer\n" +
                "    Private WithEvents _baz As New Baz\n" +
                "    Public Function Ora() As Integer\n" +
                "      Return GetTickCount()\n" +
                "    End Function\n" +
                "  End Class\n" +
                "End Namespace\n");
            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Nativo.vb").Script;

            var tick = s.Symbols.Single(x => x.Name == "GetTickCount");
            Assert.AreEqual(("method", "Integer"), (tick.Kind, tick.ReturnType), "Declare … Lib è un metodo, con il suo simbolo");
            var ora = s.Symbols.Single(x => x.Name == "Ora");
            Assert.IsTrue(s.Edges.Any(e => e.SourceId == ora.Id && e.TargetId == tick.Id && e.Relation == "calls" && e.Confidence == "extracted"), "la chiamata arriva a un simbolo che esiste");
            Assert.AreEqual("field", s.Symbols.Single(x => x.Name == "_baz").Kind, "WithEvents resta un campo");
        }

        [TestMethod]
        public void Record_the_types_of_a_registration_gettype_generic_and_chained()
        {
            File.WriteAllText(Path.Combine(_src, "Avvio.vb"),
                "Namespace Demo\n" +
                "  Public Class Avvio\n" +
                "    Public Sub Configura(services As Object, builder As Object)\n" +
                "      services.AddScoped(Of IFoo, Foo)()\n" +
                "      services.AddScoped(GetType(IFoo), GetType(Foo))\n" +
                "      builder.RegisterType(Of Foo)().SingleInstance().As(Of IFoo)()\n" +
                "    End Sub\n" +
                "  End Class\n" +
                "End Namespace\n");
            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Avvio.vb").Script;
            string Types(ScriptCallSite c) => string.Join(" ", c.TypeArguments.OrderBy(t => t.Position).Select(t => $"{t.Position}={t.TypeName}"));

            Assert.AreEqual("1000=Demo.IFoo 1001=Demo.Foo", Types(s.CallSites.Single(c => c.Line == 4)), "(Of IFoo, Foo)");
            Assert.AreEqual("0=Demo.IFoo 1=Demo.Foo", Types(s.CallSites.Single(c => c.Line == 5)), "GetType(X) come typeof(X)");
            var register = s.CallSites.Single(c => c.Callee == "RegisterType");
            var asCall = s.CallSites.Single(c => c.Callee == "As");
            Assert.AreEqual(("1000=Demo.Foo", "1000=Demo.IFoo"), (Types(register), Types(asCall)));
            Assert.AreEqual((register.Line, register.Ordinal), (asCall.ReceiverLine, asCall.ReceiverOrdinal), "la catena scende fino a RegisterType");
            Assert.AreEqual(s.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Avvio.vb").RecordCount);
        }

        [TestMethod]
        public void Record_attributes_with_their_type_and_string_arguments_and_the_return_type()
        {
            File.WriteAllText(Path.Combine(_src, "Parti.vb"),
                "Namespace Demo\n" +
                "  <Serializable>\n" +
                "  <Export(GetType(IFoo)), ExportMetadata(\"Nome\", \"pdf\")>\n" +
                "  Public Class Parte\n" +
                "    <Import(ContractType:=GetType(IFoo))> Private _altro As IFoo\n" +
                "    <Provides>\n" +
                "    Public Function Crea() As Baz\n" +
                "      Return New Baz()\n" +
                "    End Function\n" +
                "    <Tipi({GetType(Foo), GetType(Baz)})>\n" +
                "    Public Property Nome As String\n" +
                "  End Class\n" +
                "End Namespace\n");
            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Parti.vb").Script;
            string Of(string symbol) => string.Join(" | ", s.Annotations.Where(a => a.SymbolId == s.Symbols.Single(x => x.Name == symbol && x.Kind != "external").Id).OrderBy(a => a.Ordinal)
                .Select(a => a.Name + "(" + string.Join(", ", a.Arguments.OrderBy(x => x.Index).Select(x => $"{x.Element}={x.TypeName ?? "'" + x.Value + "'"}")) + ")"));

            Assert.AreEqual("Export(value=Demo.IFoo) | ExportMetadata(value='Nome', arg1='pdf')", Of("Parte"));
            Assert.AreEqual("Import(ContractType=Demo.IFoo)", Of("_altro"));
            Assert.AreEqual("Provides()", Of("Crea"));
            Assert.AreEqual("Tipi(value=Demo.Foo, value=Demo.Baz)", Of("Nome"));
            Assert.AreEqual("Demo.Baz", s.Symbols.Single(x => x.Name == "Crea").ReturnTypeName);
            Assert.AreEqual(s.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Parti.vb").RecordCount);
        }

        [TestMethod]
        public void Link_a_method_to_the_tables_of_the_sql_it_writes_wherever_the_string_goes()
        {
            File.WriteAllText(Path.Combine(_src, "Dati.vb"),
                "Namespace Demo\n" +
                "  Public Class Dati\n" +
                "    Private Const SQL_CLIENTI As String = \"SELECT * FROM CLIENTI\"\n" +
                "    Public Function Elenco(cliente As String, tabella As String) As String\n" +
                "      Dim sql As String = \"SELECT O.* FROM ORDINI O\" &\n" +                          // riga 5: concatenazione su più righe
                "          \" JOIN RIGHE R ON R.ORD = O.ID WHERE O.CLI = '\" & cliente & \"'\"\n" +
                "      sql &= \" ORDER BY O.ID\"\n" +
                "      Dim sb As New System.Text.StringBuilder()\n" +
                "      sb.Append(\" FROM STORICO S \")\n" +                                            // riga 9: un pezzo in uno StringBuilder
                "      Dim altro = $\"DELETE FROM {tabella} WHERE ID = 1\"\n" +                          // riga 10: la tabella arriva da una variabile
                "      Esegui(\"UPDATE ORDINI SET STATO = 1\")\n" +
                "      Esegui(\"BEGIN PKG_ORDINI.CHIUDI(:id); END;\")\n" +
                "      Console.WriteLine(\"Select a file from the list.\")\n" +
                "      Return sql & SQL_CLIENTI\n" +
                "    End Function\n" +
                "    Private Sub Esegui(testo As String)\n" +
                "    End Sub\n" +
                "  End Class\n" +
                "End Namespace\n");
            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Dati.vb").Script;
            var names = s.Symbols.ToDictionary(x => x.Id, x => x.QualifiedName);
            string Sql(string member) => string.Join(" ", s.Edges.Where(e => e.SourceId == s.Symbols.Single(x => x.Name == member && x.Kind != "external").Id && (e.Relation is "reads" or "writes" || names[e.TargetId].StartsWith("PKG_")))
                .OrderBy(e => e.Line).ThenBy(e => e.Relation).ThenBy(e => names[e.TargetId]).Select(e => $"{e.Line}:{e.Relation}:{names[e.TargetId]}:{e.Confidence}"));

            Assert.AreEqual("5:reads:ORDINI:inferred 5:reads:RIGHE:inferred 9:reads:STORICO:inferred 10:writes:?:ambiguous 11:writes:ORDINI:inferred 12:calls:PKG_ORDINI.CHIUDI:inferred", Sql("Elenco"));
            Assert.AreEqual("3:reads:CLIENTI:inferred", Sql("SQL_CLIENTI"), "la costante porta la sua tabella");
            Assert.AreEqual(s.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Dati.vb").RecordCount);
        }

        private CodeExtractionResult ExtractAll(string folder)
        {
            var files = Directory.GetFiles(folder, "*.vb", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            return Extractor().Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files });
        }

        private const string LegacyProject = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Project ToolsVersion=\"4.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n";

        /// <summary>Il VB di una volta: i file non hanno Namespace, i tipi prendono il RootNamespace del .vbproj; gli Imports sono del progetto.</summary>
        [TestMethod]
        public void Read_root_namespace_imports_and_project_references_from_the_vbproj()
        {
            var legacy = Path.Combine(_project, "legacy");
            Directory.CreateDirectory(Path.Combine(legacy, "Core")); Directory.CreateDirectory(Path.Combine(legacy, "Web", "Pagine"));
            File.WriteAllText(Path.Combine(legacy, "Core", "Core.vbproj"), LegacyProject +
                "  <PropertyGroup><RootNamespace>Acme.Core</RootNamespace></PropertyGroup>\n" +
                "  <ItemGroup><Reference Include=\"Terzi\"><HintPath>..\\lib\\Terzi.dll</HintPath></Reference></ItemGroup>\n</Project>\n");
            File.WriteAllText(Path.Combine(legacy, "Core", "Ordini.vb"),
                "Public Interface IOrdini\n  Function Conta() As Integer\nEnd Interface\n" +
                "Public Class Strumenti\n  Public Shared Function Doppio(n As Integer) As Integer\n    Return n * 2\n  End Function\nEnd Class\n");
            File.WriteAllText(Path.Combine(legacy, "Web", "Web.vbproj"), LegacyProject +
                "  <PropertyGroup><RootNamespace>Acme.Web</RootNamespace></PropertyGroup>\n" +
                "  <ItemGroup><Import Include=\"System\" /><Import Include=\"Acme.Core\" /></ItemGroup>\n" +
                "  <ItemGroup><ProjectReference Include=\"..\\Core\\Core.vbproj\" /></ItemGroup>\n</Project>\n");
            File.WriteAllText(Path.Combine(legacy, "Web", "Pagine", "Elenco.vb"),
                "Public Class OrdiniWeb\n  Implements IOrdini\n  Public Function Conta() As Integer Implements IOrdini.Conta\n    Return Strumenti.Doppio(1)\n  End Function\nEnd Class\n" +
                "Public Class Elenco\n  Public Function Quanti(o As IOrdini) As Integer\n    Return o.Conta()\n  End Function\nEnd Class\n");

            var result = ExtractAll(legacy);
            StringAssert.Contains(string.Join(" | ", result.Warnings), "Terzi.dll", "una libreria riferita che qui non c'è si dice");
            var web = result.Scripts.Single(s => s.File.LogicalPath.EndsWith("Elenco.vb")).Script;
            var core = result.Scripts.Single(s => s.File.LogicalPath.EndsWith("Ordini.vb")).Script;

            Assert.IsNotNull(core.Symbols.SingleOrDefault(s => s.QualifiedName == "Acme.Core.IOrdini" && s.Kind == "interface"), "il RootNamespace sta davanti ai tipi");
            var conta = web.Symbols.Single(s => s.QualifiedName == "Acme.Web.OrdiniWeb.Conta()");
            var doppio = core.Symbols.Single(s => s.Name == "Doppio");
            Assert.IsTrue(web.Edges.Any(e => e.SourceId == conta.Id && e.TargetId == doppio.Id && e.Relation == "calls" && e.Confidence == "extracted"), "Imports di progetto + ProjectReference: la chiamata arriva nell'altro progetto");
            var quanti = web.Symbols.Single(s => s.Name == "Quanti");
            var ifaceConta = core.Symbols.Single(s => s.QualifiedName == "Acme.Core.IOrdini.Conta()");
            Assert.IsTrue(web.Edges.Any(e => e.SourceId == quanti.Id && e.TargetId == ifaceConta.Id && e.Confidence == "extracted"));
            Assert.IsTrue(web.Edges.Any(e => e.SourceId == quanti.Id && e.TargetId == conta.Id && e.Confidence == "inferred"), "l'implementazione sta in un altro progetto di chi dichiara l'interfaccia");
            Assert.IsFalse(web.Symbols.Any(s => s.Kind == "external" && s.QualifiedName.Contains("IOrdini")), "niente doppioni esterni dei tipi dell'altro progetto");
        }

        [TestMethod]
        public void Resolve_dotnet_framework_classes_or_say_that_its_libraries_are_missing()
        {
            var web = Path.Combine(_project, "webforms");
            Directory.CreateDirectory(web);
            File.WriteAllText(Path.Combine(web, "Web.vbproj"), LegacyProject + "  <PropertyGroup><RootNamespace>Acme.Web</RootNamespace><TargetFrameworkVersion>v4.8</TargetFrameworkVersion></PropertyGroup>\n</Project>\n");
            File.WriteAllText(Path.Combine(web, "Elenco.aspx.vb"),
                "Partial Class Elenco\n  Inherits System.Web.UI.Page\n" +
                "  Protected WithEvents btnCerca As Global.System.Web.UI.WebControls.Button\n" +
                "  Protected Sub btnCerca_Click(sender As Object, e As System.EventArgs) Handles btnCerca.Click\n" +
                "    Response.Redirect(\"Dettaglio.aspx\")\n" +
                "  End Sub\nEnd Class\n");

            var before = Environment.GetEnvironmentVariable(NetFrameworkLibraries.OverrideVariable);
            try
            {
                Environment.SetEnvironmentVariable(NetFrameworkLibraries.OverrideVariable, Path.Combine(_root, "non-c-e"));
                var missing = ExtractAll(web);
                StringAssert.Contains(string.Join(" | ", missing.Warnings), ".NET Framework 4.8", "senza le librerie del framework lo dice, non tace");
                var s0 = missing.Scripts.Single().Script;
                var click0 = s0.Symbols.Single(x => x.Name == "btnCerca_Click");
                Assert.IsTrue(s0.Edges.Any(e => e.TargetId == click0.Id && e.Relation == "calls" && e.Confidence == "ambiguous"), "Handles resta, con l'evento come nome scritto");
            }
            finally { Environment.SetEnvironmentVariable(NetFrameworkLibraries.OverrideVariable, before); }

            if (NetFrameworkLibraries.FindDirectory("4.8") == null) Assert.Inconclusive("le librerie di riferimento di .NET Framework non sono su questa macchina");
            var result = ExtractAll(web);
            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            var s = result.Scripts.Single().Script;
            var click = s.Symbols.Single(x => x.Name == "btnCerca_Click");
            Assert.IsNotNull(s.Symbols.SingleOrDefault(x => x.Kind == "external" && x.QualifiedName == "System.Web.HttpResponse.Redirect(String)"), "Response è di Page: si risolve");
            var evento = s.Symbols.Single(x => x.Kind == "external" && x.QualifiedName == "System.Web.UI.WebControls.Button.Click");
            Assert.IsTrue(s.Edges.Any(e => e.SourceId == evento.Id && e.TargetId == click.Id && e.Relation == "calls" && e.Confidence == "inferred"));
            var campo = s.Symbols.Single(x => x.Kind == "field" && x.Name == "btnCerca");
            Assert.IsTrue(s.Edges.Any(e => e.SourceId == campo.Id && e.TargetId == click.Id && e.Relation == "calls" && e.Confidence == "inferred"), "il controllo della pagina porta al suo gestore");
            Assert.IsFalse(s.Edges.Any(e => e.Confidence == "ambiguous"), "niente resta un nome scritto");
        }

        [TestMethod]
        public void Link_events_to_who_raises_them_and_to_their_handlers_and_record_assigned_names()
        {
            File.WriteAllText(Path.Combine(_src, "Eventi.vb"),
                "Namespace Demo\n" +
                "  Public Class Sorgente\n" +
                "    Public Event Fatto(sender As Object, e As EventArgs)\n" +
                "    Public Property Comando As String\n" +
                "    Public Sub New(Optional nome As String = Nothing)\n    End Sub\n" +
                "    Public Sub Lavora()\n" +
                "      RaiseEvent Fatto(Me, EventArgs.Empty)\n" +
                "    End Sub\n" +
                "  End Class\n" +
                "  Public Class Ascolto\n" +
                "    Private Const SP_TOTALE As String = \"PKG_ORDINI.TOTALE\"\n" +
                "    Private WithEvents _s As New Sorgente\n" +
                "    Private Sub Quando(sender As Object, e As EventArgs) Handles _s.Fatto\n" +
                "    End Sub\n" +
                "    Private Sub Anche(sender As Object, e As EventArgs)\n" +
                "    End Sub\n" +
                "    Public Sub Aggancia(altro As Sorgente)\n" +
                "      AddHandler altro.Fatto, AddressOf Anche\n" +
                "      altro.Comando = \"PKG_ORDINI.ELENCO\"\n" +
                "      Dim t As New Sorgente(SP_TOTALE) With {.Comando = \"PKG_ORDINI.CHIUDI\"}\n" +
                "      Dim frase = \"due parole\"\n" +
                "    End Sub\n" +
                "  End Class\n" +
                "End Namespace\n");
            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Eventi.vb").Script;
            string Id(string name) => s.Symbols.Single(x => x.Name == name && x.Kind != "external").Id;
            bool Calls(string from, string to, string confidence) => s.Edges.Any(e => e.SourceId == Id(from) && e.TargetId == Id(to) && e.Relation == "calls" && e.Confidence == confidence);

            Assert.AreEqual("field", s.Symbols.Single(x => x.Name == "Fatto").Kind, "l'evento è un nodo");
            Assert.IsTrue(Calls("Lavora", "Fatto", "extracted"), "RaiseEvent");
            Assert.IsTrue(Calls("Fatto", "Quando", "inferred"), "Handles");
            Assert.IsTrue(Calls("_s", "Quando", "inferred"), "il campo WithEvents porta al gestore");
            Assert.IsTrue(Calls("Fatto", "Anche", "inferred"), "AddHandler");

            CollectionAssert.AreEquivalent(new[] { "altro.Comando(PKG_ORDINI.ELENCO)", ".Sorgente(PKG_ORDINI.TOTALE)", ".Comando(PKG_ORDINI.CHIUDI)" },
                s.CallSites.Select(c => $"{c.Receiver}.{c.Callee}({string.Join(",", c.Arguments.Select(a => a.Value))})").ToArray(),
                "un nome assegnato a una proprietà, passato a un costruttore (anche da una costante) o in un inizializzatore; le variabili locali e le frasi no");
            Assert.AreEqual(s.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Eventi.vb").RecordCount);
        }

        [TestMethod]
        public void Record_call_sites_with_string_literals_and_nameof()
        {
            File.WriteAllText(Path.Combine(_src, "OrdiniController.vb"),
                "Namespace Demo\n  Public Class OrdiniController\n" +
                "    Function View(n As String) As Object\n      Return n\n    End Function\n" +
                "    Function RedirectToAction(a As String, Optional c As String = Nothing) As Object\n      Return a\n    End Function\n" +
                "    Public Function Elenco() As Object\n      Return View(\"Ordini\")\n    End Function\n" +
                "    Public Function Cancella(id As Integer) As Object\n      Return Me.RedirectToAction(NameOf(Elenco), \"Ordini\")\n    End Function\n" +
                "  End Class\nEnd Namespace\n");
            var result = Extractor().Extract(Request());
            var s = result.Scripts.Single(x => x.File.LogicalPath == "src/Demo/OrdiniController.vb").Script;

            var view = s.CallSites.Single(c => c.Callee == "View");
            Assert.AreEqual((s.Symbols.Single(x => x.Name == "Elenco").Id, 10, (string)null, "Ordini"), (view.SourceId, view.Line, view.Receiver, view.Arguments.Single().Value));
            var redirect = s.CallSites.Single(c => c.Callee == "RedirectToAction");
            Assert.AreEqual("Me", redirect.Receiver);
            CollectionAssert.AreEqual(new[] { "Elenco", "Ordini" }, redirect.Arguments.Select(a => a.Value).ToArray());
        }

        [TestMethod]
        public void Write_scripts_the_parser_reads_back()
        {
            foreach (var (file, script) in Extractor().Extract(Request()).Scripts)
            {
                var reparsed = CodeScriptParser.Parse(CodeScriptWriter.Serialize(script), file.LogicalPath);
                Assert.AreEqual(script.RecordCount, reparsed.RecordCount);
            }
        }
    }
}
