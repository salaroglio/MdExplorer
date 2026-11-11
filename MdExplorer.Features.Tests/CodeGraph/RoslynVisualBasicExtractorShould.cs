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
