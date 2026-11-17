using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// La mappa fra unità (17/11/2026): di quale progetto è un file, e quante chiamate vanno da un'unità all'altra. Un grafo
    /// piccolo a tre strati con una chiamata che salta lo strato di mezzo, un ciclo, un arco ambiguo e gli esterni.
    /// </summary>
    [TestClass]
    public class CodeUnitMapShould
    {
        private static readonly Dictionary<string, (string Kind, string Name, string Path)> N = new()
        {
            ["page"] = ("module", "src/Web/Ordini.aspx", "src/Web/Ordini.aspx"),
            ["h"] = ("method", "Web.Ordini.Salva()", "src/Web/Ordini.aspx.vb"),
            ["h2"] = ("method", "Web.Ordini.Carica()", "src/Web/Ordini.aspx.vb"),
            ["bl"] = ("method", "Bl.Ordini.Salva()", "src/Bl/Ordini.cs"),
            ["bl2"] = ("method", "Bl.Ordini.Avvisa()", "src/Bl/Ordini.cs"),
            ["dl"] = ("method", "Dl.Ordini.Inserisci()", "src/Dl/Ordini.vb"),
            ["dl2"] = ("method", "Dl.Ordini.Leggi()", "src/Dl/Ordini.vb"),
            ["old"] = ("method", "Vecchio.Usa()", "src/Vecchio/Usa.vb"),
            ["T"] = ("external", "ORDINI", null),
            ["lib"] = ("external", "log4net.ILog.Info", null),
            ["boh"] = ("external", "Sicurezza.Verifica", null),
            ["evt"] = ("external", "System.Timers.Timer.Elapsed", null),
        };

        private static readonly List<(string Path, string Project)> Files = new()
        {
            ("src/Web/Ordini.aspx", "src/Web/Web.vbproj"), ("src/Web/Ordini.aspx.vb", "src/Web/Web.vbproj"),
            ("src/Bl/Ordini.cs", "src/Bl/Bl.csproj"), ("src/Dl/Ordini.vb", "src/Dl/Dl.vbproj"),
            ("src/Vecchio/Usa.vb", null), ("sql/ordini.sql", CodeProjectFiles.None),
        };

        private static CodeNavEdge E(string s, string t, string rel, string conf = "extracted", int line = 1)
            => new(s, N[s].Kind, N[s].Name, N[s].Path, t, N[t].Kind, N[t].Name, N[t].Path, rel, conf, N[s].Path, line);

        private static CodeNavigation.Graph Graph() => new(new[]
        {
            E("page", "h", "calls"),                    // dentro Web
            E("h", "bl", "calls"), E("h2", "bl", "calls", line: 2), E("h", "bl2", "calls", "ambiguous", 3),   // Web → Bl: 3, 1 ambiguo
            E("h2", "dl2", "calls"),                    // Web → Dl: salta lo strato di mezzo
            E("bl", "dl", "calls"),                     // Bl → Dl
            E("dl", "bl2", "calls"),                    // Dl → Bl: un ciclo fra Bl e Dl
            E("dl", "T", "writes"), E("dl2", "T", "reads"),   // Dl → la tabella, due archi, una tabella
            E("bl2", "lib", "calls"),                   // Bl → libreria
            E("h", "boh", "calls", "ambiguous"),        // Web → nome non risolto
            E("evt", "bl2", "calls"),                   // un esterno fa partire Bl
            E("old", "dl", "calls"),                    // un file senza progetto registrato
        });

        private static CodeUnitMap.Map ByProject(string pairFrom = null, string pairTo = null, int sample = 0)
            => CodeUnitMap.Build(Graph(), Files, CodeUnitMap.ByProject, null, 1, pairFrom, pairTo, sample, "s", 50);

        [TestMethod]
        public void Add_up_the_edges_between_projects_keeping_the_ambiguous_apart()
        {
            var m = ByProject();
            var webBl = m.Rows.Single(r => r.From == "src/Web/Web.vbproj" && r.To == "src/Bl/Bl.csproj");
            Assert.AreEqual((3, 1, 2, 2), (webBl.Edges, webBl.Ambiguous, webBl.Callers, webBl.Called));
            Assert.AreEqual(1, m.Rows.Single(r => r.From == "src/Web/Web.vbproj" && r.To == "src/Dl/Dl.vbproj").Edges, "la chiamata che salta lo strato di mezzo si vede");
            Assert.AreEqual("src/Web/Web.vbproj", m.Rows[0].From, "prima la riga con più archi");
        }

        [TestMethod]
        public void Say_for_each_unit_what_stays_inside_and_what_leaves_the_graph()
        {
            var m = ByProject();
            var web = m.Units.Single(u => u.Name == "src/Web/Web.vbproj");
            Assert.AreEqual((2, 1, 4, 0), (web.Files, web.Inside, web.Out, web.In));
            Assert.AreEqual((1, 1), (web.UnresolvedCalls, web.UnresolvedNames));
            var dl = m.Units.Single(u => u.Name == "src/Dl/Dl.vbproj");
            Assert.AreEqual((1, 2), (dl.Tables, dl.DataEdges), "due archi verso la stessa tabella: una tabella");
            var bl = m.Units.Single(u => u.Name == "src/Bl/Bl.csproj");
            Assert.AreEqual((1, 1), (bl.LibraryCalls, bl.StartedByExternal));
        }

        [TestMethod]
        public void Put_every_edge_in_exactly_one_total()
        {
            var t = ByProject().Totals;
            Assert.AreEqual(13, t.Edges);
            Assert.AreEqual(t.Edges, t.BetweenUnits + t.InsideUnits + t.ToExternalData + t.ToLibrary + t.ToUnresolvedNames + t.FromExternal + t.BetweenExternals);
            Assert.AreEqual((7, 1, 2, 1, 1, 1), (t.BetweenUnits, t.InsideUnits, t.ToExternalData, t.ToLibrary, t.ToUnresolvedNames, t.FromExternal));
        }

        [TestMethod]
        public void Find_the_cycle_between_units_and_leave_out_the_units_that_are_not_projects()
        {
            var m = ByProject();
            Assert.AreEqual(1, m.Cycles.Count);
            CollectionAssert.AreEqual(new[] { "src/Bl/Bl.csproj", "src/Dl/Dl.vbproj" }, m.Cycles[0]);
            // i file senza progetto registrato e quelli senza progetto sopra sono unità a parte, dette per nome
            Assert.AreEqual(1, m.FilesWithoutRecord);
            Assert.AreEqual(1, m.Units.Single(u => u.Name == CodeUnitMap.NotRecorded).Out);
            Assert.AreEqual(1, m.Units.Single(u => u.Name == CodeUnitMap.NoProject).Files);
        }

        [TestMethod]
        public void Sample_the_edges_of_one_row_the_same_ones_for_the_same_seed()
        {
            var first = ByProject("src/Web/Web.vbproj", "src/Bl/Bl.csproj", 2);
            var again = ByProject("src/Web/Web.vbproj", "src/Bl/Bl.csproj", 2);
            Assert.AreEqual(3, first.PairEdges, "il conto è di tutti gli archi della riga, non del campione");
            Assert.AreEqual(2, first.Samples.Count);
            CollectionAssert.AreEqual(first.Samples.Select(s => s.At).ToList(), again.Samples.Select(s => s.At).ToList());
            StringAssert.StartsWith(first.Samples[0].At, "src/Web/Ordini.aspx.vb:");
        }

        [TestMethod]
        public void Group_by_folder_at_the_asked_depth()
        {
            var m = CodeUnitMap.Build(Graph(), Files, CodeUnitMap.ByFolder, "src/", 1, null, null, 0, "s", 50);
            Assert.AreEqual(3, m.Rows.Single(r => r.From == "src/Web/" && r.To == "src/Bl/").Edges);
            Assert.AreEqual(1, m.Units.Single(u => u.Name == CodeUnitMap.Elsewhere).Files, "ciò che sta fuori da from è un'unità sola");
            var whole = CodeUnitMap.Build(Graph(), Files, CodeUnitMap.ByFolder, null, 1, null, null, 0, "s", 50);
            Assert.AreEqual(0, whole.RowCount, "a un livello tutto il codice sta sotto src/");
            Assert.AreEqual(8, whole.Units.Single(u => u.Name == "src/").Inside);
        }

        // ------------------------------------------------------------------ di quale progetto è un file

        private static Func<string, IReadOnlyList<string>> Disk(params (string Dir, string[] Names)[] folders)
        {
            var map = folders.ToDictionary(f => Path.GetFullPath(f.Dir), f => (IReadOnlyList<string>)f.Names, StringComparer.Ordinal);
            return dir => map.TryGetValue(dir, out var names) ? names : Array.Empty<string>();
        }

        private static CodeSourceFile Source(string logical) => new(Path.GetFullPath("/repo/" + logical), logical);

        [TestMethod]
        public void Take_the_nearest_project_file_going_up_the_folders()
        {
            var disk = Disk(("/repo/src/Web", new[] { "Web.vbproj", "Web.vbproj.user", "packages.config" }), ("/repo/src/Web/Pagine", new[] { "Ordini.aspx" }));
            Assert.AreEqual("src/Web/Web.vbproj", CodeProjectFiles.Of(Source("src/Web/Pagine/Ordini.aspx.vb"), "visualbasic", "/repo", disk));
            Assert.AreEqual("src/Web/Web.vbproj", CodeProjectFiles.Of(Source("src/Web/Pagine/Ordini.aspx"), "pagine", "/repo", disk), "una pagina è del progetto che la contiene");
            Assert.AreEqual(CodeProjectFiles.None, CodeProjectFiles.Of(Source("sql/ordini.sql"), "plsql", "/repo", disk));
        }

        [TestMethod]
        public void Give_a_dotnet_source_only_the_project_of_its_own_language()
        {
            // come l'estrattore: un file C# non si compila con un .vbproj, anche se è il più vicino
            var disk = Disk(("/repo/src", new[] { "Tutto.csproj" }), ("/repo/src/Web", new[] { "Web.vbproj", "B.vbproj" }));
            Assert.AreEqual("src/Tutto.csproj", CodeProjectFiles.Of(Source("src/Web/Util.cs"), "csharp", "/repo", disk));
            Assert.AreEqual("src/Web/B.vbproj", CodeProjectFiles.Of(Source("src/Web/Util.vb"), "visualbasic", "/repo", disk), "il primo per nome, come l'estrattore");
            Assert.AreEqual(CodeProjectFiles.None, CodeProjectFiles.Of(Source("src/Web/Pagina.aspx"), "pagine", "/repo", disk), "due progetti .NET per una pagina: non si sceglie");
        }

        [TestMethod]
        public void Prefer_the_project_file_of_the_languages_own_family()
        {
            var disk = Disk(("/repo/app", new[] { "App.csproj", "package.json" }), ("/repo/svc", new[] { "pom.xml", "build.gradle" }));
            Assert.AreEqual("app/package.json", CodeProjectFiles.Of(Source("app/src/main.ts"), "typescript", "/repo", disk));
            Assert.AreEqual("app/App.csproj", CodeProjectFiles.Of(Source("app/Views/Home.cshtml"), "pagine", "/repo", disk));
            Assert.AreEqual("svc/pom.xml", CodeProjectFiles.Of(Source("svc/src/main/java/A.java"), "java", "/repo", disk));
        }

        [TestMethod]
        public void Name_a_project_above_an_external_root_without_leaving_the_alias()
        {
            var disk = Disk(("/altrove", new[] { "Api.csproj" }), ("/altrove/Api", new[] { "Foo.cs" }));
            var file = new CodeSourceFile(Path.GetFullPath("/altrove/Api/Foo.cs"), "@backend/Foo.cs");
            Assert.AreEqual("@backend/../Api.csproj", CodeProjectFiles.Of(file, "csharp", null, disk));
            // la cartella non è su questo disco (una radice remota): non si è potuto cercare, ed è diverso da «non c'è»
            Assert.IsNull(CodeProjectFiles.Of(Source("x/y.cs"), "csharp", "/repo", _ => null));
        }

        [TestMethod]
        public void Keep_the_project_through_the_script_on_disk()
        {
            var script = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = "src/A.cs", Language = "csharp",
                SourceHash = "h", Extractor = "roslyn", ExtractorVersion = "1", GeneratedAt = "2026-11-17T00:00:00Z", Project = "src/A.csproj" } };
            var text = CodeScriptTurtle.Write(script);
            StringAssert.Contains(text, "code:project \"src/A.csproj\"");
            Assert.AreEqual("src/A.csproj", CodeScriptTurtle.Read(text, "a.ast.ttl").Header.Project);
            script.Header.Project = null;
            Assert.IsFalse(CodeScriptTurtle.Write(script).Contains("code:project"), "non registrato: la proprietà non c'è");
            script.Header.Project = CodeProjectFiles.None;
            Assert.AreEqual(CodeProjectFiles.None, CodeScriptTurtle.Read(CodeScriptTurtle.Write(script), "a.ast.ttl").Header.Project, "cercato e non trovato resta diverso da non registrato");
        }
    }
}
