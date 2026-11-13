using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// F2: Roslyn senza SDK legge i .cs di una cartella, scrive gli script AST (formato che il parser rilegge) e il
    /// caricatore li porta nel DB con chiamate risolte, dispatch per interfaccia e nodi external.
    /// </summary>
    [TestClass]
    public class RoslynCSharpExtractorShould
    {
        private string _root, _project, _src;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-roslyn-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "src", "Demo");
            Directory.CreateDirectory(_src);
            File.WriteAllText(Path.Combine(_src, "IFoo.cs"), "namespace Demo { public interface IFoo { System.Threading.Tasks.Task<int> Bar(string path, int retries = 3); } }");
            File.WriteAllText(Path.Combine(_src, "Foo.cs"),
                "using System;\nnamespace Demo {\n  public class Foo : IFoo {\n    private readonly Baz _baz = new Baz();\n    public string Name { get; set; }\n" +
                "    public async System.Threading.Tasks.Task<int> Bar(string path, int retries = 3) { Console.WriteLine(path); _baz.Run(out var n); return await System.Threading.Tasks.Task.FromResult(n); }\n  }\n" +
                "  public class Baz { public void Run(out int n) { n = 1; } }\n}\n");
            File.WriteAllText(Path.Combine(_src, "Client.cs"),
                "namespace Demo { public class Client { private readonly IFoo _f; public Client(IFoo f) { _f = f; } public void Go() { _f.Bar(\"x\"); Missing.Do(); } } }");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private CodeExtractionRequest Request()
        {
            var files = Directory.GetFiles(_src, "*.cs").OrderBy(f => f).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            return new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files };
        }

        [TestMethod]
        public void Extract_symbols_parameters_and_resolved_edges_without_an_sdk()
        {
            var extractor = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance);
            var result = extractor.Extract(Request());

            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            Assert.AreEqual(3, result.Scripts.Count);

            var foo = result.Scripts.Single(s => s.File.LogicalPath == "src/Demo/Foo.cs").Script;
            var bar = foo.Symbols.Single(s => s.Kind == "method" && s.Name == "Bar");
            Assert.AreEqual("Demo.Foo.Bar(string, int)", bar.QualifiedName);
            Assert.AreEqual("public Task<int> Bar(string path, int retries = 3)", bar.Signature); // async non è parte della firma: sta in isAsync
            Assert.AreEqual("Task<int>", bar.ReturnType);
            Assert.IsTrue(bar.IsAsync);
            Assert.AreEqual(6, bar.LineStart);
            var prms = foo.Parameters.Where(p => p.SymbolId == bar.Id).OrderBy(p => p.Position).ToList();
            Assert.AreEqual(2, prms.Count);
            Assert.AreEqual("path", prms[0].Name); Assert.AreEqual("string", prms[0].ParameterType);
            Assert.AreEqual("retries", prms[1].Name); Assert.AreEqual("3", prms[1].DefaultValue);
            var run = foo.Symbols.Single(s => s.Name == "Run");
            Assert.AreEqual("out", foo.Parameters.Single(p => p.SymbolId == run.Id).Direction);

            // archi: implements, contains, instantiates, calls risolta verso external (Console.WriteLine) e interna (Baz.Run)
            var fooClass = foo.Symbols.Single(s => s.Kind == "class" && s.Name == "Foo");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooClass.Id && e.Relation == "implements" && e.Confidence == "extracted"));
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooClass.Id && e.TargetId == bar.Id && e.Relation == "contains"));
            Assert.IsTrue(foo.Edges.Any(e => e.Relation == "instantiates"));
            var console = foo.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "System.Console.WriteLine(string)");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == console.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == run.Id && e.Relation == "calls" && e.Confidence == "extracted"));

            // dispatch per interfaccia: Client.Go chiama IFoo.Bar (extracted) e Foo.Bar (inferred); Missing.Do → external ambiguous
            var client = result.Scripts.Single(s => s.File.LogicalPath == "src/Demo/Client.cs").Script;
            var go = client.Symbols.Single(s => s.Name == "Go");
            var ifaceBarId = RoslynCSharpExtractor.StableId("csharp", "method", "Demo.IFoo.Bar(string, int)");
            Assert.IsTrue(client.Edges.Any(e => e.SourceId == go.Id && e.TargetId == ifaceBarId && e.Relation == "calls" && e.Confidence == "extracted"));
            Assert.IsTrue(client.Edges.Any(e => e.SourceId == go.Id && e.TargetId == bar.Id && e.Relation == "calls" && e.Confidence == "inferred"), "l'implementazione in perimetro è inferred");
            var missing = client.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "Missing.Do");
            Assert.IsTrue(client.Edges.Any(e => e.TargetId == missing.Id && e.Confidence == "ambiguous"));
            // il parametro del costruttore punta all'interfaccia in perimetro
            var ctor = client.Symbols.Single(s => s.Kind == "constructor");
            var ifooId = RoslynCSharpExtractor.StableId("csharp", "interface", "Demo.IFoo");
            Assert.IsTrue(client.Edges.Any(e => e.SourceId == ctor.Id && e.TargetId == ifooId && e.Relation == "references"));
        }

        [TestMethod]
        public void Link_a_method_to_the_tables_of_the_sql_written_in_its_strings()
        {
            File.WriteAllText(Path.Combine(_src, "Dati.cs"),
                "namespace Demo {\npublic class Dati {\n" +
                "  const string SqlClienti = \"SELECT * FROM CLIENTI\";\n" +
                "  public string Elenco(string cliente, string tabella) {\n" +
                "    var sql = \"SELECT O.* FROM ORDINI O\" +\n" +                       // riga 5
                "        \" JOIN RIGHE R ON R.ORD = O.ID WHERE O.CLI = '\" + cliente + \"'\";\n" +
                "    sql += \" ORDER BY O.ID\";\n" +
                "    var sb = new System.Text.StringBuilder().Append(\" FROM STORICO S \");\n" +
                "    var altro = $\"INSERT INTO {tabella} (ID) VALUES (1)\";\n" +          // riga 9: la tabella arriva da una variabile
                "    var testo = @\"UPDATE ORDINI\n       SET STATO = 1\";\n" +             // riga 10: su più righe
                "    System.Console.WriteLine(\"Update available from server\");\n" +
                "    return sql + SqlClienti;\n  }\n} }");
            var s = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Dati.cs").Script;
            var names = s.Symbols.ToDictionary(x => x.Id, x => x.QualifiedName);
            string Sql(string member) => string.Join(" ", s.Edges.Where(e => e.SourceId == s.Symbols.Single(x => x.Name == member && x.Kind != "external").Id && e.Relation is "reads" or "writes")
                .OrderBy(e => e.Line).ThenBy(e => names[e.TargetId]).Select(e => $"{e.Line}:{e.Relation}:{names[e.TargetId]}:{e.Confidence}"));
            Assert.AreEqual("5:reads:ORDINI:inferred 5:reads:RIGHE:inferred 8:reads:STORICO:inferred 9:writes:?:ambiguous 10:writes:ORDINI:inferred", Sql("Elenco"));
            Assert.AreEqual("3:reads:CLIENTI:inferred", Sql("SqlClienti"));
            Assert.AreEqual(s.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Dati.cs").RecordCount);
        }

        [TestMethod]
        public void Link_a_method_to_the_procedures_it_calls_by_name_with_CommandType_StoredProcedure()
        {
            File.WriteAllText(Path.Combine(_src, "Procedure.cs"),
                "using System.Data;\nnamespace Demo {\npublic class Comando { public string CommandText { get; set; } public CommandType CommandType { get; set; } }\n" +
                "public class Procedure {\n" +
                "  public void Chiudi(string nome) {\n" +
                "    var cmd = new Comando { CommandType = CommandType.StoredProcedure };\n" +
                "    cmd.CommandText = \"PKG_ORDINI.CHIUDI\";\n" +                                 // riga 7
                "    Esegui(\"PKG_ORDINI.APRI\", System.Data.CommandType.StoredProcedure);\n" +    // riga 8: nome qualificato del tipo
                "    Esegui(nome, CommandType.StoredProcedure);\n" +
                "  }\n" +
                "  void Esegui(string testo, CommandType tipo) { }\n} }");
            var s = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Procedure.cs").Script;
            var names = s.Symbols.ToDictionary(x => x.Id, x => x.QualifiedName);
            var calls = string.Join(" ", s.Edges.Where(e => e.SourceId == s.Symbols.Single(x => x.Name == "Chiudi").Id && e.Relation == "calls"
                    && s.Symbols.Single(x => x.Id == e.TargetId).Language == RoslynExtractorBase.SqlLanguage)
                .OrderBy(e => e.Line).Select(e => $"{e.Line}:{names[e.TargetId]}:{e.Confidence}"));
            Assert.AreEqual("7:PKG_ORDINI.CHIUDI:inferred 8:PKG_ORDINI.APRI:inferred 9:?:ambiguous", calls, "il nome nel parametro lo dicono i chiamanti: qui il nodo «?»");
        }

        [TestMethod]
        public void Give_rules_and_sql_the_values_of_strings_held_in_variables()
        {
            // 12/11/2026: lo stesso flusso dei valori per tutti — i punti di chiamata letti dalle regole e l'SQL a pezzi
            File.WriteAllText(Path.Combine(_src, "Valori.cs"),
                "namespace Demo { public class Valori {\n" +
                "  static readonly string Fisso = \"R_FISSO\";\n" +
                "  void Render(string report) { } void Exec(string sql) { }\n" +
                "  public void Stampa(bool annuale) {\n" +
                "    var report = annuale ? \"R_ANNO\" : \"R_MESE\";\n" +
                "    Render(report);\n" +                                            // riga 6: due punti, uno per valore
                "    Render(Fisso);\n" +                                             // riga 7: campo di sola lettura
                "    var tabella = annuale ? \"STORICO_ANNO\" : \"STORICO_MESE\";\n" +
                "    Exec(\"DELETE FROM \" + tabella);\n" +                         // riga 9
                "  }\n} }");
            var s = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Valori.cs").Script;
            var names = s.Symbols.ToDictionary(x => x.Id, x => x.QualifiedName);
            CollectionAssert.AreEqual(new[] { "6:R_ANNO", "6:R_MESE", "7:R_FISSO" },
                s.CallSites.Where(c => c.Callee == "Render").Select(c => $"{c.Line}:{c.Arguments.Single().Value}").OrderBy(x => x).ToArray());
            var writes = s.Edges.Where(e => e.Relation == "writes").OrderBy(e => names[e.TargetId]).Select(e => $"{e.Line}:{names[e.TargetId]}:{e.Guard}").ToArray();
            CollectionAssert.AreEqual(new[] { "9:STORICO_ANNO:tabella = \"STORICO_ANNO\" (1 of 2)", "9:STORICO_MESE:tabella = \"STORICO_MESE\" (1 of 2)" }, writes);
        }

        [TestMethod]
        public void Mark_as_ambiguous_a_name_built_from_two_pieces_with_several_values()
        {
            File.WriteAllText(Path.Combine(_src, "Pezzi.cs"),
                "namespace Demo { public class Pezzi {\n" +
                "  void Exec(string sql) { }\n" +
                "  public void Leggi(bool annuale) {\n" +
                "    string t, c;\n" +
                "    if (annuale) { t = \"T_ANNO\"; c = \"C_ANNO\"; } else { t = \"T_MESE\"; c = \"C_MESE\"; }\n" +
                "    Exec(\"SELECT * FROM \" + t + \" JOIN \" + c + \" ON 1 = 1\");\n" +          // riga 6: due oggetti, ciascuno vero
                "    Exec(\"SELECT * FROM STORICO_\" + t + c);\n" +                                // riga 7: un nome fatto da due pezzi
                "  }\n} }");
            var s = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Pezzi.cs").Script;
            var names = s.Symbols.ToDictionary(x => x.Id, x => x.QualifiedName);
            string Reads(int line) => string.Join(" ", s.Edges.Where(e => e.Relation == "reads" && e.Line == line).OrderBy(e => names[e.TargetId]).Select(e => $"{names[e.TargetId]}:{e.Confidence}"));
            Assert.AreEqual("C_ANNO:inferred C_MESE:inferred T_ANNO:inferred T_MESE:inferred", Reads(6), "ogni pezzo nomina un oggetto vero");
            Assert.AreEqual("STORICO_T_ANNOC_ANNO:ambiguous STORICO_T_ANNOC_MESE:ambiguous STORICO_T_MESEC_ANNO:ambiguous STORICO_T_MESEC_MESE:ambiguous", Reads(7));
            StringAssert.Contains(s.Edges.First(e => e.Line == 7 && e.Relation == "reads").Guard, "combination of pieces not verified");
        }

        [TestMethod]
        public void Record_call_sites_with_string_literals_and_nameof()
        {
            File.WriteAllText(Path.Combine(_src, "OrdiniController.cs"),
                "namespace Demo { public class OrdiniController {\n" +
                "  object View(string n) => n; object RedirectToAction(string a, string c = null) => a; void Log(string m) { }\n" +
                "  public object Elenco() { Log(\"frase con spazi\"); return View(\"Ordini\"); }\n" +
                "  public object Cancella(int id) { var v = \"x\" + id; Log(v); return this.RedirectToAction(nameof(Elenco), \"Ordini\"); }\n} }");
            var result = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(Request());
            var s = result.Scripts.Single(x => x.File.LogicalPath == "src/Demo/OrdiniController.cs").Script;
            Assert.AreEqual("14", new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).ExtractorVersion);

            var elenco = s.Symbols.Single(x => x.Name == "Elenco").Id;
            var view = s.CallSites.Single(c => c.Callee == "View");
            Assert.AreEqual((elenco, 3, (string)null, "Ordini"), (view.SourceId, view.Line, view.Receiver, view.Arguments.Single().Value));
            var redirect = s.CallSites.Single(c => c.Callee == "RedirectToAction");
            Assert.AreEqual(("this", 2), (redirect.Receiver, redirect.ArgumentCount));
            CollectionAssert.AreEqual(new[] { "Elenco", "Ordini" }, redirect.Arguments.Select(a => a.Value).ToArray(), "nameof(Elenco) è la costante 'Elenco'");
            Assert.IsFalse(s.CallSites.Any(c => c.Callee == "Log"), "una frase o una variabile non bastano a registrare la chiamata");
        }

        [TestMethod]
        public void Record_the_types_of_a_registration_typeof_generic_and_chained()
        {
            // i contenitori non sono fra le reference (pacchetti NuGet): i fatti si leggono lo stesso, da come è scritta la chiamata
            File.WriteAllText(Path.Combine(_src, "Startup.cs"),
                "namespace Demo { public class Startup {\n" +
                "  public void Configure(dynamic services, dynamic builder) {\n" +
                "    services.AddScoped<IFoo, Foo>();\n" +
                "    services.AddScoped(typeof(IFoo), typeof(Foo));\n" +
                "    builder.RegisterType<Foo>().SingleInstance().As<IFoo>();\n" +
                "    services.AddSingleton<string>();\n" +
                "  }\n} }");
            var s = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Startup.cs").Script;
            string Types(ScriptCallSite c) => string.Join(" ", c.TypeArguments.OrderBy(t => t.Position).Select(t => $"{t.Position}={t.TypeName}/{t.Confidence}"));

            Assert.AreEqual("1000=Demo.IFoo/extracted 1001=Demo.Foo/extracted", Types(s.CallSites.Single(c => c.Line == 3)), "gli argomenti generici, da 1000");
            Assert.AreEqual("0=Demo.IFoo/extracted 1=Demo.Foo/extracted", Types(s.CallSites.Single(c => c.Line == 4)), "typeof per posizione, come X.class");
            var register = s.CallSites.Single(c => c.Callee == "RegisterType");
            var asCall = s.CallSites.Single(c => c.Callee == "As");
            Assert.AreEqual("1000=Demo.Foo/extracted", Types(register));
            Assert.AreEqual("1000=Demo.IFoo/extracted", Types(asCall));
            Assert.AreEqual((register.Line, register.Ordinal), (asCall.ReceiverLine, asCall.ReceiverOrdinal), "As → RegisterType, saltando SingleInstance() che non porta niente");
            Assert.AreEqual("1000=string/platform", Types(s.CallSites.Single(c => c.Callee == "AddSingleton")));
            Assert.IsTrue(s.CallSites.All(c => c.Ordinal >= CodeCallSites.TypeOnlyOrdinalBase), "punti con soli tipi: i numeri dei punti con stringhe non si spostano");

            var reparsed = CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Startup.cs");
            Assert.AreEqual(s.RecordCount, reparsed.RecordCount, "gli script con i fatti nuovi si rileggono");
        }

        [TestMethod]
        public void Record_attributes_with_their_type_and_string_arguments_and_the_return_type()
        {
            // MEF non è fra le reference: la classe dell'attributo non si risolve, il fatto si legge lo stesso
            File.WriteAllText(Path.Combine(_src, "Parti.cs"),
                "using System;\nnamespace Demo {\n" +
                "  [Serializable]\n" +
                "  [Export(typeof(IFoo)), ExportMetadata(\"Nome\", \"pdf\")]\n" +
                "  public class Parte : IFoo {\n" +
                "    [Import(ContractType = typeof(IFoo), AllowDefault = true)] private IFoo _altro;\n" +
                "    [Provides] public Baz Crea() => new Baz();\n" +
                "    [Tipi(new[] { typeof(Foo), typeof(Baz) })] public System.Threading.Tasks.Task<int> Bar(string path, int retries = 3) => null;\n" +
                "  }\n}\n");
            var s = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Parti.cs").Script;
            string Of(string symbol) => string.Join(" | ", s.Annotations.Where(a => a.SymbolId == s.Symbols.Single(x => x.Name == symbol && x.Kind != "external").Id).OrderBy(a => a.Ordinal)
                .Select(a => a.Name + "(" + string.Join(", ", a.Arguments.OrderBy(x => x.Index).Select(x => $"{x.Element}={x.TypeName ?? "'" + x.Value + "'"}")) + ")"));

            Assert.AreEqual("Export(value=Demo.IFoo) | ExportMetadata(value='Nome', arg1='pdf')", Of("Parte"), "Serializable non dice niente al grafo");
            Assert.AreEqual("Import(ContractType=Demo.IFoo)", Of("_altro"), "l'attributo di un campo; AllowDefault non è né un tipo né un nome");
            Assert.AreEqual("Provides()", Of("Crea"));
            Assert.AreEqual("Tipi(value=Demo.Foo, value=Demo.Baz)", Of("Bar"), "un vettore: un argomento per elemento");
            Assert.AreEqual("Demo.Baz", s.Symbols.Single(x => x.Name == "Crea").ReturnTypeName, "per le regole «il metodo annotato fornisce ciò che restituisce»");
            Assert.AreEqual(s.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Parti.cs").RecordCount);
        }

        [TestMethod]
        public void Link_events_to_who_raises_them_and_to_their_handlers_and_record_assigned_names()
        {
            File.WriteAllText(Path.Combine(_src, "Eventi.cs"),
                "using System;\nnamespace Demo {\n" +
                "  public class Sorgente {\n" +
                "    public event EventHandler Fatto;\n" +
                "    public string Comando { get; set; }\n" +
                "    public Sorgente(string nome = null) { }\n" +
                "    public void Lavora() { Fatto?.Invoke(this, EventArgs.Empty); }\n" +
                "  }\n" +
                "  public class Ascolto {\n" +
                "    const string SpTotale = \"PKG_ORDINI.TOTALE\";\n" +
                "    void Quando(object sender, EventArgs e) { }\n" +
                "    void Anche(object sender, EventArgs e) { }\n" +
                "    public void Aggancia(Sorgente altro) {\n" +
                "      altro.Fatto += Quando;\n" +
                "      altro.Fatto += new EventHandler(Anche);\n" +
                "      altro.Comando = \"PKG_ORDINI.ELENCO\";\n" +
                "      var t = new Sorgente(SpTotale) { Comando = \"PKG_ORDINI.CHIUDI\" };\n" +
                "      var n = 1; n += 2; var frase = \"due parole\";\n" +
                "    }\n" +
                "  }\n}\n");
            var s = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Eventi.cs").Script;
            string Id(string name) => s.Symbols.Single(x => x.Name == name && x.Kind != "external").Id;
            bool Calls(string from, string to, string confidence) => s.Edges.Any(e => e.SourceId == Id(from) && e.TargetId == Id(to) && e.Relation == "calls" && e.Confidence == confidence);

            Assert.AreEqual("field", s.Symbols.Single(x => x.Name == "Fatto").Kind, "l'evento è un nodo");
            Assert.IsTrue(Calls("Lavora", "Fatto", "extracted"), "Fatto?.Invoke(…)");
            Assert.IsTrue(Calls("Fatto", "Quando", "inferred"), "+= Quando");
            Assert.IsTrue(Calls("Fatto", "Anche", "inferred"), "+= new EventHandler(Anche)");
            CollectionAssert.AreEquivalent(new[] { "altro.Comando(PKG_ORDINI.ELENCO)", ".Sorgente(PKG_ORDINI.TOTALE)", ".Comando(PKG_ORDINI.CHIUDI)" },
                s.CallSites.Select(c => $"{c.Receiver}.{c.Callee}({string.Join(",", c.Arguments.Select(a => a.Value))})").ToArray());
            Assert.AreEqual(s.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Eventi.cs").RecordCount);
        }

        [TestMethod]
        public void Compile_each_csproj_on_its_own_and_follow_project_references()
        {
            var sol = Path.Combine(_project, "soluzione");
            Directory.CreateDirectory(Path.Combine(sol, "Core")); Directory.CreateDirectory(Path.Combine(sol, "App"));
            File.WriteAllText(Path.Combine(sol, "Core", "Core.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>");
            File.WriteAllText(Path.Combine(sol, "Core", "IOrdini.cs"), "namespace Acme.Core { public interface IOrdini { int Conta(); } }");
            File.WriteAllText(Path.Combine(sol, "App", "App.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup><ProjectReference Include=\"..\\Core\\Core.csproj\" /></ItemGroup></Project>");
            File.WriteAllText(Path.Combine(sol, "App", "Ordini.cs"),
                "using Acme.Core;\nnamespace Acme.App { public class Ordini : IOrdini { public int Conta() => 1; }\n public class Uso { public int Quanti(IOrdini o) => o.Conta(); } }");
            var files = Directory.GetFiles(sol, "*.cs", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            var result = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files });

            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            var app = result.Scripts.Single(x => x.File.LogicalPath.EndsWith("/Ordini.cs")).Script;
            var quanti = app.Symbols.Single(x => x.Name == "Quanti");
            var iface = RoslynCSharpExtractor.StableId("csharp", "method", "Acme.Core.IOrdini.Conta()");
            Assert.IsTrue(app.Edges.Any(e => e.SourceId == quanti.Id && e.TargetId == iface && e.Confidence == "extracted"), "il tipo dell'altro progetto è del perimetro, non un esterno");
            Assert.IsTrue(app.Edges.Any(e => e.SourceId == quanti.Id && e.TargetId == app.Symbols.Single(x => x.QualifiedName == "Acme.App.Ordini.Conta()").Id && e.Confidence == "inferred"));
            Assert.AreEqual("net8.0 non è .NET Framework", DotNetProject.Load(Path.Combine(sol, "Core", "Core.csproj")).IsNetFramework ? "framework" : "net8.0 non è .NET Framework");
            Assert.AreEqual("4.7.2", DotNetProject.NetFrameworkVersion(null, "net472"));
        }

        [TestMethod]
        public async System.Threading.Tasks.Task Write_scripts_the_parser_reads_back_and_the_loader_puts_in_fuseki()
        {
            var extractor = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance);
            var result = extractor.Extract(Request());
            foreach (var (file, script) in result.Scripts)
            {
                var text = CodeScriptWriter.Serialize(script);
                var reparsed = CodeScriptParser.Parse(text, file.LogicalPath);           // il formato è rigido: deve rileggersi
                Assert.AreEqual(script.RecordCount, reparsed.RecordCount);
                Assert.IsTrue(CodeScriptWriter.WriteIfChanged(CodeScriptWriter.ScriptPathFor(_project, file.LogicalPath, CodeScriptKind.Ast), text));
                Assert.IsFalse(CodeScriptWriter.WriteIfChanged(CodeScriptWriter.ScriptPathFor(_project, file.LogicalPath, CodeScriptKind.Ast), text), "stesso contenuto = nessuna scrittura");
            }
            Assert.IsTrue(File.Exists(Path.Combine(_project, ".mde-code", "src", "Demo", "Foo.cs.ast.ttl")));

            var store = FusekiFixture.NewStore();   // Inconclusive se Java 21 o il jar mancano
            var outcome = await store.LoadProjectAsync(_project);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            Assert.AreEqual(3, outcome.Loaded);

            var status = await store.GetStatusAsync(_project);
            Assert.AreEqual(3, status.Files);
            Assert.IsTrue(status.Symbols >= 9, $"simboli: {status.Symbols}");
            Assert.IsTrue(status.ExternalSymbols >= 2, $"external: {status.ExternalSymbols}");
            var hits = await store.SearchSymbolsAsync(_project, "Bar");
            Assert.IsTrue(hits.Any(h => h.QualifiedName == "Demo.Foo.Bar(string, int)" && h.Path == "src/Demo/Foo.cs" && h.LineStart == 6));
        }
    }
}
