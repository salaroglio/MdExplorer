using System.Collections.Generic;
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
        public void Report_files_read_and_scripts_made_for_the_background_panel()
        {
            var steps = new List<(string Step, int Done, int Total)>();
            var files = Request().AllFiles;

            Extractor().Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files, Progress = (s, d, t) => steps.Add((s, d, t)) });

            CollectionAssert.AreEqual(Enumerable.Range(1, files.Count).ToArray(), steps.Where(x => x.Step == CodeExtractionRequest.ReadStep).Select(x => x.Done).ToArray());
            CollectionAssert.AreEqual(Enumerable.Range(1, files.Count).ToArray(), steps.Where(x => x.Step == CodeExtractionRequest.EmitStep).Select(x => x.Done).ToArray());
            Assert.IsTrue(steps.All(x => x.Total == files.Count), string.Join(", ", steps));
            Assert.IsTrue(steps.FindLastIndex(x => x.Step == CodeExtractionRequest.ReadStep) < steps.FindIndex(x => x.Step == CodeExtractionRequest.EmitStep), "prima si legge, poi si emette");
        }

        [TestMethod]
        public void Start_the_event_to_handler_edge_from_the_declared_event()
        {
            // 13/11/2026, dal cliente: Event X(sender, e) dichiarato e Handles ctl.X avevano due id (con e senza parametri)
            File.WriteAllText(Path.Combine(_src, "Pulsante.vb"),
                "Namespace Demo\n  Public Class Pulsante\n    Public Event Premuto(sender As Object, e As EventArgs)\n  End Class\n" +
                "  Public Class Maschera\n    Private WithEvents _p As Pulsante\n    Private Sub Su_Premuto(sender As Object, e As EventArgs) Handles _p.Premuto\n    End Sub\n  End Class\nEnd Namespace\n");

            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Pulsante.vb").Script;

            var ev = s.Symbols.Single(x => x.Name == "Premuto" && x.Kind == "field");
            Assert.AreEqual(RoslynCSharpExtractor.StableId("visualbasic", "field", "Demo.Pulsante.Premuto"), ev.Id, ev.QualifiedName);
            var handler = s.Symbols.Single(x => x.Name == "Su_Premuto").Id;
            Assert.IsTrue(s.Edges.Any(e => e.SourceId == ev.Id && e.TargetId == handler && e.Relation == "calls"), string.Join(", ", s.Edges.Select(e => e.SourceId + ">" + e.TargetId)));
        }

        [TestMethod]
        public void Declare_delegate_types_that_the_code_uses()
        {
            // 13/11/2026, dal cliente: chi usava un Delegate puntava a un tipo mai dichiarato (arco verso un nodo che non c'è)
            File.WriteAllText(Path.Combine(_src, "Avvisi.vb"),
                "Namespace Demo\n  Public Delegate Sub Avviso(testo As String, foo As Foo)\n" +
                "  Public Class Avvisi\n    Private _a As Avviso\n    Public Sub Go()\n      _a = New Avviso(AddressOf Scrivi)\n    End Sub\n" +
                "    Private Sub Scrivi(testo As String, foo As Foo)\n    End Sub\n  End Class\nEnd Namespace\n");

            var result = Extractor().Extract(Request());

            var s = result.Scripts.Single(x => x.File.LogicalPath == "src/Demo/Avvisi.vb").Script;
            var avviso = s.Symbols.Single(x => x.Name == "Avviso" && x.Kind != "external");
            Assert.AreEqual(RoslynCSharpExtractor.StableId("visualbasic", "class", "Demo.Avviso"), avviso.Id);
            var fooId = RoslynCSharpExtractor.StableId("visualbasic", "class", "Demo.Foo");
            Assert.IsTrue(s.Edges.Any(e => e.SourceId == avviso.Id && e.TargetId == fooId && e.Relation == "references"), "la firma del delegate usa Foo");
            Assert.IsFalse(s.Symbols.Any(x => x.Name is "Invoke" or "BeginInvoke" or "EndInvoke"), "i metodi del delegate li crea il compilatore");
            Assert.IsTrue(s.Edges.Any(e => e.TargetId == avviso.Id && e.Relation != "contains"), "chi lo usa arriva al tipo dichiarato");
        }

        [TestMethod]
        public void Give_shared_and_instance_constructors_different_ids()
        {
            File.WriteAllText(Path.Combine(_src, "Manager.vb"),
                "Namespace Demo\n  Public Class Manager\n    Shared Sub New()\n    End Sub\n    Public Sub New()\n    End Sub\n  End Class\nEnd Namespace\n");

            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Manager.vb").Script;

            var ctors = s.Symbols.Where(x => x.Kind == "constructor").ToList();
            Assert.AreEqual(2, ctors.Select(c => c.Id).Distinct().Count(), string.Join(", ", ctors.Select(c => c.Name + "=" + c.QualifiedName)));
            Assert.AreEqual(s.Symbols.Count, s.Symbols.Select(x => x.Id).Distinct().Count(), "nessun id ripetuto nello script");
        }

        [TestMethod]
        public void Make_one_external_node_for_a_name_written_with_different_case()
        {
            // il VB non distingue le maiuscole: lo stesso metodo che manca, scritto in due modi, è un nodo solo (13/11/2026)
            File.WriteAllText(Path.Combine(_src, "Other.vb"),
                "Namespace Demo\n  Public Class Other\n    Public Sub Go()\n      MISSING.doit()\n      missing.DoIt()\n    End Sub\n  End Class\nEnd Namespace\n");

            var result = Extractor().Extract(Request());

            var client = result.Scripts.Single(s => s.File.LogicalPath == "src/Demo/Client.vb").Script;
            var other = result.Scripts.Single(s => s.File.LogicalPath == "src/Demo/Other.vb").Script;
            var first = client.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "Missing.DoIt");
            var inOther = other.Symbols.Where(s => s.Kind == "external" && s.Name.Equals("DoIt", StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.AreEqual(1, inOther.Count, string.Join(", ", inOther.Select(s => s.QualifiedName)));
            Assert.AreEqual(first.Id, inOther[0].Id, "stesso nodo in tutti i file, comunque sia scritto");
            Assert.AreEqual("MISSING.doit", inOther[0].QualifiedName, "il nome mostrato è la prima grafia del file");
            var go = other.Symbols.Single(s => s.Name == "Go" && s.Kind != "external");
            CollectionAssert.AreEqual(new[] { 4, 5 }, other.Edges.Where(e => e.SourceId == go.Id && e.TargetId == first.Id && e.Relation == "calls").Select(e => e.Line).OrderBy(l => l).ToArray(),
                "le due chiamate, ognuna alla sua riga, verso lo stesso nodo");
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

        [TestMethod]
        public void Link_a_method_to_the_procedures_it_calls_by_name_with_CommandType_StoredProcedure()
        {
            // 12/11/2026, misurato da MarkAgent su un applicativo vero: 123 chiamate così, nessuna arrivava nel grafo
            File.WriteAllText(Path.Combine(_src, "Procedure.vb"),
                "Imports System.Data\n" +
                "Namespace Demo\n" +
                "  Public Class Procedure\n" +
                "    Public Sub ConHelper(id As Integer, nome As String)\n" +
                "      Esegui(\" PKG_ORDINI.CHIUDI \", CommandType.StoredProcedure, \"p_id\")\n" +      // riga 5: helper, spazi, un parametro senza punto
                "      Esegui(\"GEST.PKG_ORDINI.APRI\", CommandType.StoredProcedure)\n" +            // riga 6: con lo schema
                "      ' Esegui(\"PKG_VECCHIO.X\", CommandType.StoredProcedure)\n" +                 // riga 7: commentata
                "      Esegui(nome, CommandType.StoredProcedure)\n" +                                  // riga 8: nome in una variabile
                "      Esegui(\"RICALCOLA\", CommandType.StoredProcedure)\n" +                        // riga 9: procedura senza package
                "      Esegui(\"PKG_ALTRO.NON_PROC\", CommandType.Text)\n" +                          // riga 10: non è una procedura
                "    End Sub\n" +
                "    Public Sub ConComando()\n" +
                "      Dim cmd As New Comando()\n" +
                "      cmd.CommandType = CommandType.StoredProcedure\n" +
                "      cmd.CommandText = \"PKG_RAPPORTI.SETTIMANA\"\n" +                              // riga 15
                "    End Sub\n" +
                "    Public Sub ConCostruttore()\n" +
                "      Dim cmd As New OleCommand(\"PKG_RAPPORTI.MESE\")\n" +                          // riga 18
                "      With cmd\n" +
                "        .CommandType = CommandType.StoredProcedure\n" +
                "      End With\n" +
                "    End Sub\n" +
                "    Public Sub SenzaProcedure()\n" +
                "      Dim cmd As New Comando()\n" +
                "      cmd.CommandText = \"PKG_RAPPORTI.ANNO\"\n" +                                  // senza StoredProcedure: niente
                "    End Sub\n" +
                "    Private Sub Esegui(testo As String, tipo As CommandType, ParamArray altri() As String)\n" +
                "    End Sub\n" +
                "  End Class\n" +
                "  Public Class Comando\n    Public Property CommandText As String\n    Public Property CommandType As CommandType\n  End Class\n" +
                "  Public Class OleCommand\n    Inherits Comando\n    Public Sub New(testo As String)\n      Me.CommandText = testo\n    End Sub\n  End Class\n" +
                "End Namespace\n");
            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Procedure.vb").Script;
            var names = s.Symbols.ToDictionary(x => x.Id, x => x.QualifiedName);
            string Calls(string member) => string.Join(" ", s.Edges.Where(e => e.SourceId == s.Symbols.Single(x => x.Name == member && x.Kind != "external").Id && e.Relation == "calls"
                    && s.Symbols.Single(x => x.Id == e.TargetId).Language == RoslynExtractorBase.SqlLanguage)
                .OrderBy(e => e.Line).ThenBy(e => names[e.TargetId]).Select(e => $"{e.Line}:{names[e.TargetId]}:{e.Confidence}"));

            Assert.AreEqual("5:PKG_ORDINI.CHIUDI:inferred 6:GEST.PKG_ORDINI.APRI:inferred 8:?:ambiguous 9:RICALCOLA:inferred", Calls("ConHelper"), "il nome nel parametro: il nodo «?», lo dicono i chiamanti");
            Assert.AreEqual("15:PKG_RAPPORTI.SETTIMANA:inferred", Calls("ConComando"));
            Assert.AreEqual("18:PKG_RAPPORTI.MESE:inferred", Calls("ConCostruttore"));
            Assert.AreEqual("", Calls("SenzaProcedure"));
            Assert.AreEqual(CodeSqlLinker.ExternalId("symbol", "PKG_ORDINI.CHIUDI"), s.Symbols.Single(x => x.QualifiedName == "PKG_ORDINI.CHIUDI").Id, "lo stesso nodo dell'SQL scritto, che il ricollegamento sposta sulla procedura vera");
            Assert.AreEqual(s.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Procedure.vb").RecordCount);
        }

        [TestMethod]
        public void Follow_the_name_of_a_procedure_through_variables_branches_and_parameters()
        {
            // 12/11/2026, misurato da MarkAgent dal cliente: 11 nomi in una variabile locale (anche in rami), 3 in un parametro
            File.WriteAllText(Path.Combine(_src, "Flusso.vb"),
                "Imports System.Data\n" +
                "Namespace Demo\n" +
                "  Public Class Comando\n    Public Property CommandText As String\n    Public Property CommandType As CommandType\n  End Class\n" +
                "  Public Class Flusso\n" +                                                              // riga 7
                "    Private Const PKG As String = \"PKG_STAMPE\"\n" +
                "    Public Sub Una()\n" +
                "      Dim nome As String = PKG & \".SETTIMANA\"\n" +                                    // riga 10: costante e pezzi
                "      Esegui(nome, CommandType.StoredProcedure)\n" +                                    // riga 11
                "    End Sub\n" +
                "    Public Sub Rami(tipo As Integer)\n" +
                "      Dim nome As String\n" +
                "      Select Case tipo\n" +
                "        Case 1 : nome = \"PKG_A.UNO\"\n" +
                "        Case Else : nome = \"PKG_A.DUE\"\n" +
                "      End Select\n" +
                "      Esegui(nome, CommandType.StoredProcedure)\n" +                                    // riga 19
                "    End Sub\n" +
                "    Public Sub DaFuori(lettore As IDataReader)\n" +
                "      Dim nome As String = lettore.GetString(0)\n" +
                "      Esegui(nome, CommandType.StoredProcedure)\n" +                                    // riga 23: non si sa
                "    End Sub\n" +
                "    Private Sub Lancia(procedura As String)\n" +                                         // l'helper: il nome è il parametro
                "      Dim cmd As New Comando()\n" +
                "      cmd.CommandType = CommandType.StoredProcedure\n" +
                "      cmd.CommandText = procedura\n" +                                                   // riga 28
                "    End Sub\n" +
                "    Private Sub Apri(nome As String)\n" +
                "      Lancia(nome.Trim())\n" +                                                           // un passaggio in più
                "    End Sub\n" +
                "    Public Sub Chiamante()\n" +
                "      Lancia(\"PKG_B.TRE\")\n" +                                                        // riga 34
                "      Apri(\" PKG_B.QUATTRO \")\n" +                                                     // riga 35: attraverso Apri
                "    End Sub\n" +
                "    Private Sub Esegui(testo As String, tipo As CommandType)\n" +
                "    End Sub\n" +
                "  End Class\n" +
                "End Namespace\n");
            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Flusso.vb").Script;
            var names = s.Symbols.ToDictionary(x => x.Id, x => x.QualifiedName);
            string Calls(string member) => string.Join(" | ", s.Edges.Where(e => e.SourceId == s.Symbols.Single(x => x.Name == member && x.Kind != "external").Id && e.Relation == "calls"
                    && s.Symbols.Single(x => x.Id == e.TargetId).Language == RoslynExtractorBase.SqlLanguage)
                .OrderBy(e => e.Line).ThenBy(e => names[e.TargetId]).Select(e => $"{e.Line}:{names[e.TargetId]}:{e.Confidence}:{e.Guard}"));

            Assert.AreEqual("11:PKG_STAMPE.SETTIMANA:inferred:nome = \"PKG_STAMPE.SETTIMANA\"", Calls("Una"));
            Assert.AreEqual("19:PKG_A.DUE:inferred:nome = \"PKG_A.DUE\" (1 of 2) | 19:PKG_A.UNO:inferred:nome = \"PKG_A.UNO\" (1 of 2)", Calls("Rami"));
            Assert.AreEqual("23:?:ambiguous:nome: the name is decided at run time (returned by GetString())", Calls("DaFuori"));
            Assert.AreEqual("28:?:ambiguous:procedura: the name comes from the callers", Calls("Lancia"));
            Assert.AreEqual("", Calls("Apri"), "il nome lo dà a sua volta chi chiama Apri");
            Assert.AreEqual("34:PKG_B.TRE:inferred:Lancia(procedura = \"PKG_B.TRE\") | 35:PKG_B.QUATTRO:inferred:Apri(nome = \"PKG_B.QUATTRO\")", Calls("Chiamante"));
            Assert.AreEqual(s.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Flusso.vb").RecordCount);
        }

        [TestMethod]
        public void Give_each_use_only_the_values_that_reach_it()
        {
            // 12/11/2026, utente: «basta un errore, ed è tutto sbagliato» — un uso vede solo le assegnazioni che gli arrivano
            File.WriteAllText(Path.Combine(_src, "Esatto.vb"),
                "Imports System.Data\n" +
                "Namespace Demo\n" +
                "  Public Class Esatto\n" +
                "    Private _scelta As String = \"PKG_C.UNO\"\n" +
                "    Public Sub Ordine()\n" +
                "      Dim nome As String = \"PKG_O.PRIMA\"\n" +
                "      Esegui(nome, CommandType.StoredProcedure)\n" +                                    // riga 7: solo PRIMA
                "      nome = \"PKG_O.DOPO\"\n" +
                "      Esegui(nome, CommandType.StoredProcedure)\n" +                                    // riga 9: solo DOPO
                "    End Sub\n" +
                "    Public Sub Riassegnato(nome As String)\n" +
                "      nome = \"PKG_R.FISSO\"\n" +
                "      Esegui(nome, CommandType.StoredProcedure)\n" +                                    // riga 13: il parametro è riscritto
                "    End Sub\n" +
                "    Public Sub Campo()\n" +
                "      Esegui(_scelta, CommandType.StoredProcedure)\n" +                                 // riga 16: inizializzatore e Cambia
                "    End Sub\n" +
                "    Public Sub Cambia()\n" +
                "      _scelta = \"PKG_C.DUE\"\n" +
                "    End Sub\n" +
                "    Public Sub InParte(l As IDataReader, annuale As Boolean)\n" +
                "      Dim nome As String = \"PKG_P.NOTO\"\n" +
                "      If annuale Then nome = l.GetString(0)\n" +
                "      Esegui(nome, CommandType.StoredProcedure)\n" +                                    // riga 24: uno noto, uno no
                "    End Sub\n" +
                "    Private Sub Lancia(testo As String, tipo As CommandType)\n" +                        // l'helper: nome e tipo dai chiamanti
                "      Dim cmd As New Comando()\n" +
                "      cmd.CommandType = tipo\n" +
                "      cmd.CommandText = testo\n" +
                "    End Sub\n" +
                "    Public Sub Chiamanti(t As CommandType)\n" +
                "      Lancia(\"PKG_T.SI\", CommandType.StoredProcedure)\n" +                            // riga 32: sì
                "      Lancia(\"SELECT 1 FROM DUAL\", CommandType.Text)\n" +                            // riga 33: no, è SQL
                "      Lancia(\"PKG_T.FORSE\", t)\n" +                                                   // riga 34: il tipo da un parametro
                "    End Sub\n" +
                "    Public Sub DiSopra()\n" +
                "      Chiamanti(CommandType.StoredProcedure)\n" +
                "    End Sub\n" +
                "    Private Sub Esegui(testo As String, tipo As CommandType)\n" +
                "    End Sub\n" +
                "  End Class\n" +
                "  Public Class Comando\n    Public Property CommandText As String\n    Public Property CommandType As CommandType\n  End Class\n" +
                "End Namespace\n");
            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Esatto.vb").Script;
            var names = s.Symbols.ToDictionary(x => x.Id, x => x.QualifiedName);
            string Calls(string member) => string.Join(" | ", s.Edges.Where(e => e.SourceId == s.Symbols.Single(x => x.Name == member && x.Kind != "external").Id && e.Relation == "calls"
                    && s.Symbols.Single(x => x.Id == e.TargetId).Language == RoslynExtractorBase.SqlLanguage)
                .OrderBy(e => e.Line).ThenBy(e => names[e.TargetId]).Select(e => $"{e.Line}:{names[e.TargetId]}:{e.Confidence}:{e.Guard}"));

            Assert.AreEqual("7:PKG_O.PRIMA:inferred:nome = \"PKG_O.PRIMA\" | 9:PKG_O.DOPO:inferred:nome = \"PKG_O.DOPO\"", Calls("Ordine"));
            Assert.AreEqual("13:PKG_R.FISSO:inferred:nome = \"PKG_R.FISSO\"", Calls("Riassegnato"));
            Assert.AreEqual("16:PKG_C.DUE:inferred:_scelta = \"PKG_C.DUE\" (1 of 2) | 16:PKG_C.UNO:inferred:_scelta = \"PKG_C.UNO\" (1 of 2)", Calls("Campo"));
            Assert.AreEqual("24:?:ambiguous:nome: the name is decided at run time (returned by GetString()) | 24:PKG_P.NOTO:inferred:nome = \"PKG_P.NOTO\" (one of 1 known values; others: returned by GetString())", Calls("InParte"));
            Assert.AreEqual("32:PKG_T.SI:inferred:Lancia(testo = \"PKG_T.SI\") | 34:PKG_T.FORSE:inferred:Lancia(testo = \"PKG_T.FORSE\"), when the caller passes CommandType.StoredProcedure", Calls("Chiamanti"),
                "riga 33 passa CommandType.Text: nessuna procedura; riga 34 dipende da chi chiama Chiamanti");
            Assert.AreEqual(s.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Esatto.vb").RecordCount);
        }

        [TestMethod]
        public void Pair_command_type_and_command_text_of_the_same_command()
        {
            File.WriteAllText(Path.Combine(_src, "Comandi.vb"),
                "Imports System.Data\n" +
                "Namespace Demo\n" +
                "  Public Class Comando\n    Public Property CommandText As String\n    Public Property CommandType As CommandType\n    Public Sub New(Optional cmdText As String = Nothing)\n      Me.CommandText = cmdText\n    End Sub\n  End Class\n" +
                "  Public Class Comandi\n" +                                                              // riga 10
                "    Public Sub Due()\n" +
                "      Dim proc As New Comando()\n" +
                "      proc.CommandType = CommandType.StoredProcedure\n" +
                "      proc.CommandText = \"PKG_M.PROC\"\n" +                                          // riga 14: procedura
                "      Dim testo As New Comando()\n" +
                "      testo.CommandText = \"SELECT X FROM T_MISTO\"\n" +                               // riga 16: testo SQL, nessun CommandType
                "    End Sub\n" +
                "    Public Sub ConWith()\n" +
                "      Dim cmd As New Comando(\"PKG_W.UNO\")\n" +                                      // riga 19
                "      With cmd\n" +
                "        .CommandType = CommandType.StoredProcedure\n" +
                "      End With\n" +
                "      With New Comando(\"PKG_W.DUE\")\n" +                                            // riga 23
                "        .CommandType = CommandType.StoredProcedure\n" +
                "      End With\n" +
                "    End Sub\n" +
                "  End Class\n" +
                "End Namespace\n");
            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Comandi.vb").Script;
            var names = s.Symbols.ToDictionary(x => x.Id, x => x.QualifiedName);
            string Calls(string member) => string.Join(" | ", s.Edges.Where(e => e.SourceId == s.Symbols.Single(x => x.Name == member && x.Kind != "external").Id && e.Relation == "calls"
                    && s.Symbols.Single(x => x.Id == e.TargetId).Language == RoslynExtractorBase.SqlLanguage)
                .OrderBy(e => e.Line).Select(e => $"{e.Line}:{names[e.TargetId]}"));
            Assert.AreEqual("14:PKG_M.PROC", Calls("Due"), "il comando di testo SQL non è una procedura");
            Assert.IsTrue(s.Edges.Any(e => e.Relation == "reads" && names[e.TargetId] == "T_MISTO"), "il suo SQL resta letto come SQL");
            Assert.AreEqual("19:PKG_W.UNO | 23:PKG_W.DUE", Calls("ConWith"));
        }

        [TestMethod]
        public void Load_the_handler_of_an_event_declared_in_another_file()
        {
            // 12/11/2026, visto nel demo: lo script del gestore ha l'arco «evento → gestore» con l'evento di un altro file,
            // e il lettore Turtle lo rifiutava («nodo senza classe»): il file del gestore restava fuori dal grafo
            File.WriteAllText(Path.Combine(_src, "Fonte.vb"),
                "Namespace Demo\n  Public Class Fonte\n    Public Event Pronto(n As Integer)\n    Public Sub Vai()\n      RaiseEvent Pronto(1)\n    End Sub\n  End Class\nEnd Namespace\n");
            File.WriteAllText(Path.Combine(_src, "Ascolto.vb"),
                "Namespace Demo\n  Public Class Ascolto\n    Private WithEvents _f As Fonte\n    Private Sub Quando(n As Integer) Handles _f.Pronto\n    End Sub\n  End Class\nEnd Namespace\n");
            var s = Extractor().Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Ascolto.vb").Script;
            var turtle = CodeScriptTurtle.Write(s);
            var back = CodeScriptTurtle.Read(turtle, ".mde-code/src/Demo/Ascolto.vb.ast.ttl");
            Assert.AreEqual(s.Edges.Count, back.Edges.Count);
            var handler = s.Symbols.Single(x => x.Name == "Quando").Id;
            Assert.AreEqual(2, back.Edges.Count(e => e.TargetId == handler && e.Relation == "calls"), "dall'evento dell'altro file e dal campo WithEvents");
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

        // ---- Visual Studio .NET 2003 (12/11/2026, rilevazione dal cliente: schermate → BL → DL con «Reference Project="{GUID}"»,
        //      un blocco applicativo di libreria con la connessione prima del nome della procedura)

        private const string BlocksSource =
            "namespace Acme.Blocks { public static class DbHelper {\n" +
            "  public static System.Data.DataSet ExecuteDataset(string connectionString, System.Data.CommandType commandType, string commandText, params object[] commandParameters) => null;\n" +
            "  public static int ExecuteNonQuery(string connectionString, System.Data.CommandType commandType, string commandText, params object[] commandParameters) => 0;\n" +
            "} }\n";

        private static string Vs2003Project(string guid, string rootNamespace, string references)
            => "<VisualStudioProject>\n    <VisualBasic\n        ProjectType = \"Local\"\n        ProductVersion = \"7.10.3077\"\n        SchemaVersion = \"2.0\"\n" +
               $"        ProjectGuid = \"{{{guid}}}\"\n    >\n        <Build>\n            <Settings\n                AssemblyName = \"{rootNamespace}\"\n" +
               $"                OptionExplicit = \"On\"\n                OptionStrict = \"Off\"\n                RootNamespace = \"{rootNamespace}\"\n            >\n            </Settings>\n" +
               "            <References>\n" +
               "                <Reference Name = \"System\" AssemblyName = \"System\" HintPath = \"..\\..\\WINDOWS\\Microsoft.NET\\Framework\\v1.1.4322\\System.dll\" />\n" +
               references +
               "            </References>\n            <Imports>\n                <Import Namespace = \"Microsoft.VisualBasic\" />\n                <Import Namespace = \"System\" />\n" +
               "                <Import Namespace = \"System.Data\" />\n            </Imports>\n        </Build>\n    </VisualBasic>\n</VisualStudioProject>\n";

        /// <summary>Un DL e un BL di Visual Studio 2003; con <paramref name="withLibrary"/> la libreria del blocco applicativo c'è, compilata.</summary>
        /// <param name="libraryAsMissingProject">Il DL riferisce la libreria come un progetto che non c'è (per GUID); la DLL la riferisce il BL.</param>
        /// <param name="nobodyReferencesTheLibrary">Con <paramref name="libraryAsMissingProject"/>: nemmeno il BL riferisce la DLL, che sta solo nella cartella.</param>
        private string WriteVs2003Solution(bool withLibrary, bool libraryAsMissingProject = false, bool nobodyReferencesTheLibrary = false)
        {
            var root = Path.Combine(_project, "vs2003");
            foreach (var d in new[] { "Lib", "Dl", "Bl" }) Directory.CreateDirectory(Path.Combine(root, d));
            if (withLibrary)
            {
                var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator).Select(p => Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(p));
                var lib = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("Acme.Blocks", new[] { Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(BlocksSource) }, refs,
                    new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
                Microsoft.CodeAnalysis.Emit.EmitResult emitted;
                using (var dll = File.Create(Path.Combine(root, "Lib", "Acme.Blocks.dll"))) emitted = lib.Emit(dll);
                Assert.IsTrue(emitted.Success, string.Join("\n", emitted.Diagnostics));
            }
            var blocksByPath = "                <Reference Name = \"Acme.Blocks\" AssemblyName = \"Acme.Blocks\" HintPath = \"..\\Lib\\Acme.Blocks.dll\" />\n";
            File.WriteAllText(Path.Combine(root, "Dl", "Dl.vbproj"), Vs2003Project("11111111-AAAA-BBBB-CCCC-000000000001", "Acme.Dl", libraryAsMissingProject
                ? "                <Reference Name = \"Acme.Blocks\" Project = \"{77777777-AAAA-BBBB-CCCC-000000000007}\" Package = \"{F184B08F-C81C-45F6-A57F-5ABD9991F28F}\" />\n"
                : blocksByPath));
            File.WriteAllText(Path.Combine(root, "Dl", "Dati.vb"),
                "Imports Acme.Blocks\n" +
                "Public Class Accesso\n" +
                "    Public Shared Function Leggi(ByVal procedura As String, ByVal connessione As String)\n" +
                "        Return DbHelper.ExecuteDataset(connessione, CommandType.StoredProcedure, procedura)\n" +          // riga 4
                "    End Function\n" +
                "    Public Shared Function Esegui(ByVal connessione As String, ByVal procedura As String)\n" +
                "        Return DbHelper.ExecuteNonQuery(connessione, CommandType.StoredProcedure, procedura)\n" +         // riga 7
                "    End Function\n" +
                "End Class\n" +
                "Public Class Ordini\n" +
                "    Private Shared conn As String = System.Configuration.ConfigurationSettings.AppSettings(\"db\").ToString()\n" +
                "    Public Function Elenco(ByVal filtro As String) As DataSet\n" +
                "        Dim nome As String = \"SCHEMA_1.PKG_2.PROC_1\"\n" +
                "        Return Accesso.Leggi(nome, conn)\n" +                                                          // riga 14
                "    End Function\n" +
                "    Public Function Segna() As Boolean\n" +
                "        Accesso.Esegui(conn, \"PKG_1.PROC_1\")\n" +                                                    // riga 17
                "        DbHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, \"PROC_2\")\n" +                   // riga 18
                "        Return True\n" +
                "    End Function\n" +
                "End Class\n");
            File.WriteAllText(Path.Combine(root, "Bl", "Bl.vbproj"), Vs2003Project("22222222-AAAA-BBBB-CCCC-000000000002", "Acme.Bl",
                "                <Reference Name = \"Acme.Dl\" Project = \"{11111111-aaaa-bbbb-cccc-000000000001}\" Package = \"{F184B08F-C81C-45F6-A57F-5ABD9991F28F}\" />\n" +
                "                <Reference Name = \"Mancante\" Project = \"{99999999-AAAA-BBBB-CCCC-000000000009}\" Package = \"{F184B08F-C81C-45F6-A57F-5ABD9991F28F}\" />\n" +
                (libraryAsMissingProject && !nobodyReferencesTheLibrary ? blocksByPath : "")));
            File.WriteAllText(Path.Combine(root, "Bl", "Logica.vb"),
                "Imports Acme.Dl\n" +
                "Public Class Servizio\n" +
                "    Private _ordini As Ordini\n" +
                "    Private ReadOnly Property Dati() As Ordini\n" +
                "        Get\n" +
                "            If _ordini Is Nothing Then\n" +
                "                _ordini = New Ordini\n" +
                "            End If\n" +
                "            Return _ordini\n" +
                "        End Get\n" +
                "    End Property\n" +
                "    Public Function Elenco(ByVal filtro As String) As System.Data.DataSet\n" +
                "        Return Dati.Elenco(filtro)\n" +
                "    End Function\n" +
                "End Class\n");
            return root;
        }

        private CodeExtractionResult ExtractWithoutNetFramework(string folder)
        {
            var before = Environment.GetEnvironmentVariable(NetFrameworkLibraries.OverrideVariable);
            try
            {
                // le librerie di .NET Framework 1.1 non ci sono mai: quelle di .NET bastano per CommandType e DataSet
                Environment.SetEnvironmentVariable(NetFrameworkLibraries.OverrideVariable, Path.Combine(_root, "non-c-e"));
                return ExtractAll(folder);
            }
            finally { Environment.SetEnvironmentVariable(NetFrameworkLibraries.OverrideVariable, before); }
        }

        private static string ProcedureCalls(CodeScript s, string member)
        {
            var names = s.Symbols.ToDictionary(x => x.Id, x => x.QualifiedName);
            return string.Join(" | ", s.Edges.Where(e => e.SourceId == s.Symbols.Single(x => x.Name == member && x.Kind != "external").Id && e.Relation == "calls"
                    && s.Symbols.Single(x => x.Id == e.TargetId).Language == RoslynExtractorBase.SqlLanguage)
                .OrderBy(e => e.Line).ThenBy(e => names[e.TargetId]).Select(e => $"{e.Line}:{names[e.TargetId]}:{e.Confidence}:{e.Guard}"));
        }

        [TestMethod]
        public void Read_visual_studio_2003_projects_with_references_by_guid_and_libraries_by_hint_path()
        {
            var result = ExtractWithoutNetFramework(WriteVs2003Solution(withLibrary: true));
            var warnings = string.Join(" | ", result.Warnings);
            StringAssert.Contains(warnings, "Mancante", "un progetto riferito che non è fra i sorgenti si dice");
            Assert.IsFalse(warnings.Contains("System.dll"), "una libreria del framework stesso non è una libreria mancante: " + warnings);
            Assert.IsFalse(warnings.Contains("Acme.Blocks"), warnings);

            var bl = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("Logica.vb")).Script;
            var dl = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("Dati.vb")).Script;
            Assert.IsNotNull(dl.Symbols.SingleOrDefault(x => x.QualifiedName == "Acme.Dl.Ordini" && x.Kind == "class"), "RootNamespace dagli attributi di Settings");
            var elenco = bl.Symbols.Single(x => x.QualifiedName == "Acme.Bl.Servizio.Elenco(String)");
            var target = dl.Symbols.Single(x => x.QualifiedName == "Acme.Dl.Ordini.Elenco(String)");
            Assert.IsTrue(bl.Edges.Any(e => e.SourceId == elenco.Id && e.TargetId == target.Id && e.Relation == "calls" && e.Confidence == "extracted"),
                "BL → DL per GUID: la proprietà tipizzata porta al metodo del DL, non a un esterno");
            Assert.IsFalse(bl.Symbols.Any(x => x.Kind == "external" && x.Name.Contains("Elenco")));

            // il nome è commandText della libreria, non la connessione: i chiamanti scrivono la procedura
            Assert.AreEqual("14:SCHEMA_1.PKG_2.PROC_1:inferred:Leggi(procedura = \"SCHEMA_1.PKG_2.PROC_1\")", ProcedureCalls(dl, "Elenco"));
            Assert.AreEqual("17:PKG_1.PROC_1:inferred:Esegui(procedura = \"PKG_1.PROC_1\") | 18:PROC_2:inferred:", ProcedureCalls(dl, "Segna"));
            StringAssert.Contains(ProcedureCalls(dl, "Leggi"), "procedura: the name comes from the callers");
            StringAssert.Contains(ProcedureCalls(dl, "Esegui"), "procedura: the name comes from the callers");
            Assert.IsFalse(dl.Edges.Any(e => e.Guard?.Contains("ToString") == true || e.Guard?.Contains("connessione") == true), "la connessione non è mai il nome");
        }

        [TestMethod]
        public void Use_the_library_another_project_references_when_the_referenced_project_is_missing_and_say_so()
        {
            // dal cliente: il DL riferisce il blocco applicativo come progetto (che non c'è), la DLL la riferiscono le schermate
            var result = ExtractWithoutNetFramework(WriteVs2003Solution(withLibrary: true, libraryAsMissingProject: true));
            var warnings = string.Join(" | ", result.Warnings);
            StringAssert.Contains(warnings, "Dl.vbproj", warnings);
            StringAssert.Contains(warnings, "Acme.Blocks.dll", "si dice quale libreria si usa e chi la riferisce");
            StringAssert.Contains(warnings, "Bl.vbproj", warnings);
            var dl = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("Dati.vb")).Script;
            Assert.AreEqual("14:SCHEMA_1.PKG_2.PROC_1:inferred:Leggi(procedura = \"SCHEMA_1.PKG_2.PROC_1\")", ProcedureCalls(dl, "Elenco"));
            Assert.AreEqual("17:PKG_1.PROC_1:inferred:Esegui(procedura = \"PKG_1.PROC_1\") | 18:PROC_2:inferred:", ProcedureCalls(dl, "Segna"));
        }

        [TestMethod]
        public void Draw_no_edge_to_a_symbol_the_compiler_makes_by_itself()
        {
            // 13/11/2026, rilevazione dal cliente: 194 archi dalla dichiarazione di un Event verso il suo delegato nascosto e 3 da
            // MyBase.New() verso il costruttore di default, nodi che nel grafo non ci sono
            var root = Path.Combine(_project, "impliciti");
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "Eventi.vb"),
                "Public Class Valore\nEnd Class\n" +
                "Public Class Sorgente\n    Public Event Cambiato(ByVal nuovo As Valore)\nEnd Class\n" +
                "Public Class Figlia\n    Inherits Sorgente\n    Public Sub New()\n        MyBase.New()\n    End Sub\nEnd Class\n");
            var s = ExtractAll(root).Scripts.Single().Script;
            var ids = s.Symbols.Select(x => x.Id).ToHashSet();
            var dangling = s.Edges.Where(e => !ids.Contains(e.TargetId)).Select(e => e.Relation + ":" + e.TargetId).ToList();
            Assert.AreEqual(0, dangling.Count, string.Join(", ", dangling));
            var evento = s.Symbols.Single(x => x.Name == "Cambiato");
            Assert.IsTrue(s.Edges.Any(e => e.SourceId == evento.Id && e.Relation == "references" && e.TargetId == s.Symbols.Single(x => x.Name == "Valore" && x.Kind == "class").Id),
                "dell'evento conta il tipo dei suoi parametri");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Leave_without_symbols_a_file_the_project_does_not_compile_so_its_class_is_not_a_duplicate(bool visualStudio2003)
        {
            // 13/11/2026, rilevazione dal cliente: una copia della pagina in un'altra cartella, con la stessa classe, che il
            // progetto non elenca; prima la classe risultava dichiarata in due file e le frecce verso di lei ambigue
            var root = Path.Combine(_project, visualStudio2003 ? "elenco2003" : "elenco");
            Directory.CreateDirectory(Path.Combine(root, "Vecchia"));
            File.WriteAllText(Path.Combine(root, "Web.vbproj"), visualStudio2003
                ? Vs2003Project("33333333-AAAA-BBBB-CCCC-000000000003", "Acme.Web", "").Replace("    </VisualBasic>",
                    "        <Files>\n            <Include>\n                <File RelPath = \"Ordini.vb\" SubType = \"Code\" BuildAction = \"Compile\" />\n" +
                    "                <File RelPath = \"Ordini.aspx\" SubType = \"Form\" BuildAction = \"Content\" />\n            </Include>\n        </Files>\n    </VisualBasic>")
                : LegacyProject + "  <PropertyGroup><RootNamespace>Acme.Web</RootNamespace></PropertyGroup>\n  <ItemGroup><Compile Include=\"Ordini.vb\" /></ItemGroup>\n</Project>\n");
            const string Ordini = "Public Class Ordini\n    Public Sub Salva()\n    End Sub\nEnd Class\n";
            File.WriteAllText(Path.Combine(root, "Ordini.vb"), Ordini);
            File.WriteAllText(Path.Combine(root, "Vecchia", "Ordini.vb"), Ordini + "' copia di una volta\n");
            var result = ExtractWithoutNetFramework(root);

            var copy = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("Vecchia/Ordini.vb")).Script;
            Assert.AreEqual(0, copy.Symbols.Count + copy.Edges.Count, "la copia che il progetto non compila resta senza simboli");
            Assert.AreEqual(1, result.Scripts.SelectMany(x => x.Script.Symbols).Count(x => x.QualifiedName == "Acme.Web.Ordini" && x.Kind == "class"), "una classe sola");
            var warning = result.Warnings.Single(w => w.Contains("Web.vbproj") && w.Contains("Vecchia/Ordini.vb"));
            StringAssert.Contains(warning, "1 files");
        }

        [TestMethod]
        public void Use_a_library_lying_in_the_analysed_folder_when_no_project_references_it_and_say_where()
        {
            // dal cliente (13/11/2026): il DL riferisce il blocco applicativo come progetto, nessun .vbproj riferisce la DLL, che però c'è
            var result = ExtractWithoutNetFramework(WriteVs2003Solution(withLibrary: true, libraryAsMissingProject: true, nobodyReferencesTheLibrary: true));
            var warnings = string.Join(" | ", result.Warnings);
            StringAssert.Contains(warnings, "Acme.Blocks.dll", warnings);
            StringAssert.Contains(warnings, "which no project references but lies in", warnings);
            StringAssert.Contains(warnings, Path.Combine("vs2003", "Lib"), "si dice in quale cartella sta: " + warnings);
            var dl = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("Dati.vb")).Script;
            Assert.AreEqual("14:SCHEMA_1.PKG_2.PROC_1:inferred:Leggi(procedura = \"SCHEMA_1.PKG_2.PROC_1\")", ProcedureCalls(dl, "Elenco"));
            Assert.AreEqual("17:PKG_1.PROC_1:inferred:Esegui(procedura = \"PKG_1.PROC_1\") | 18:PROC_2:inferred:", ProcedureCalls(dl, "Segna"));
        }

        [TestMethod]
        public void Say_which_argument_is_the_procedure_only_when_the_library_is_not_resolved_and_one_has_its_shape()
        {
            var result = ExtractWithoutNetFramework(WriteVs2003Solution(withLibrary: false));
            StringAssert.Contains(string.Join(" | ", result.Warnings), "Acme.Blocks.dll", "la libreria che manca si dice");
            var dl = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("Dati.vb")).Script;
            Assert.AreEqual("4:?:ambiguous:DbHelper.ExecuteDataset is not resolved (its library is not among the references): which argument is the procedure name is not known", ProcedureCalls(dl, "Leggi"),
                "due parametri, nessuno ha la forma di un nome: non si tira a indovinare");
            Assert.AreEqual("18:PROC_2:ambiguous:library not resolved: the only argument shaped like a procedure name", ProcedureCalls(dl, "Segna"),
                "la connessione non si sa (ToString), «PROC_2» è il solo a forma di nome; Esegui non si sa più come passa il nome");
            Assert.AreEqual("", ProcedureCalls(dl, "Elenco"));
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
