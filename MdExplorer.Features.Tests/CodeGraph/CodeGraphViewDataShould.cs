using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// D24: la vista a cascata. Un file aperto mostra i punti d'ingresso (lo script, ciò che nessuno chiama, i tipi), un
    /// simbolo aperto mostra chi chiama anche in altri file; gli esterni non sono nodi ma stanno nell'elenco del simbolo,
    /// e chi non è chiamato da nessuno è segnato.
    /// </summary>
    [TestClass]
    public class CodeGraphViewDataShould
    {
        private const string Main = "s/main.ps1", Lib = "s/lib.ps1";

        private static CodeSymbolView Sym(string id, string kind, string path, string parent = null)
            => new() { Id = id, Kind = kind, Name = id, QualifiedName = id, Path = path, ParentId = parent, LineStart = 1 };

        private static CodeGraphViewInput.EdgeInput Edge(string a, string b, string rel, string targetKind = "function", string conf = "inferred")
            => new() { Source = a, Target = b, Relation = rel, Confidence = conf, TargetKind = targetKind, TargetName = b, TargetQualifiedName = b };

        /// <summary>
        /// main.ps1: lo script (M) chiama Run; Run chiama Helper e Load (in lib.ps1) e Get-ChildItem (esterno); Dead non la
        /// chiama nessuno; la classe Store ha il metodo Save. lib.ps1: Load chiama Parse.
        /// </summary>
        private static CodeGraphViewInput Input(IEnumerable<string> expanded, IEnumerable<string> opened)
        {
            var input = new CodeGraphViewInput { FolderPrefix = "s" };
            input.Files.Add((Main, 6)); input.Files.Add((Lib, 3));
            var main = new List<CodeSymbolView>
            {
                Sym("M", "module", Main), Sym("Run", "function", Main, "M"), Sym("Helper", "function", Main, "M"), Sym("Dead", "function", Main, "M"),
                Sym("Store", "class", Main, "M"), Sym("Save", "method", Main, "Store"), Sym("Cfg", "field", Main, "M"),
            };
            var lib = new List<CodeSymbolView> { Sym("L", "module", Lib), Sym("Load", "function", Lib, "L"), Sym("Parse", "function", Lib, "L") };
            foreach (var s in main) input.SymbolFiles[s.Id] = Main;
            foreach (var s in lib) input.SymbolFiles[s.Id] = Lib;
            input.Expanded.UnionWith(expanded);
            input.Opened.UnionWith(opened);
            // come lo store: i file aperti, quelli dei simboli aperti e dei loro bersagli
            var toRead = new HashSet<string>(input.Expanded);
            foreach (var id in input.Opened) toRead.Add(input.SymbolFiles[id]);
            input.Edges.AddRange(new[]
            {
                Edge("M", "Run", "calls"), Edge("Run", "Helper", "calls"), Edge("Run", "Load", "calls"),
                Edge("Run", "ext:gci", "calls", "external", "extracted"), Edge("Load", "Parse", "calls"), Edge("Main", "Store", "references", "class"),
            });
            input.Edges.Single(e => e.Target == "ext:gci").TargetName = "Get-ChildItem";
            foreach (var e in input.Edges.Where(e => input.Opened.Contains(e.Source)))
                if (input.SymbolFiles.TryGetValue(e.Target, out var p)) toRead.Add(p);
            if (toRead.Contains(Main)) input.Outlines[Main] = main;
            if (toRead.Contains(Lib)) input.Outlines[Lib] = lib;
            return input;
        }

        private static CodeGraphViewModel.ViewNode Node(CodeGraphViewModel m, string id) => m.Nodes.SingleOrDefault(n => n.Id == id);

        [TestMethod]
        public void Show_only_entry_points_when_a_file_is_opened()
        {
            var m = CodeGraphViewData.Build(Input(new[] { Main }, new string[0]));
            var shown = m.Nodes.Where(n => n.Kind != "file").Select(n => n.Id).ToList();
            // lo script (esegue qualcosa), ciò che nessuno chiama nel file, il tipo; non Run/Helper (li chiama qualcuno), non Save (sta in Store), non il campo
            CollectionAssert.AreEquivalent(new[] { "M", "Dead", "Store" }, shown);
            Assert.AreEqual(1, Node(m, "M").Hidden, "il «+» dello script apre Run");
            Assert.IsTrue(Node(m, "Dead").Uncalled);
            Assert.IsFalse(Node(m, "Store").Uncalled, "un tipo non è «non chiamato»");
            Assert.IsTrue(m.Links.Any(l => l.Source == "file:" + Main && l.Target == "M" && l.Relation == "contains"), "il punto d'ingresso pende dal suo file");
            Assert.IsFalse(m.Nodes.Any(n => n.External), "gli esterni non sono nodi");
        }

        [TestMethod]
        public void Cascade_through_calls_across_files_and_list_externals()
        {
            var m = CodeGraphViewData.Build(Input(new[] { Main }, new[] { "M", "Run" }));
            Assert.AreEqual("M", Node(m, "Run").ParentId);
            Assert.IsTrue(m.Links.Any(l => l.Source == "M" && l.Target == "Run" && l.Relation == "calls"));
            Assert.AreEqual(1, m.Links.Count(l => l.Source == "M" && l.Target == "Run"), "chiamata dal suo contenitore: un arco solo");
            Assert.IsNotNull(Node(m, "Helper")); Assert.IsFalse(Node(m, "Helper").Uncalled);
            var load = Node(m, "Load");
            Assert.IsNotNull(load, "la cascata attraversa i file, anche se lib.ps1 non è aperto");
            Assert.AreEqual(1, load.Hidden, "Load ha Parse da aprire");
            Assert.IsNull(Node(m, "Parse"));
            Assert.IsTrue(m.Links.Any(l => l.Source == "Run" && l.Target == "Load" && l.Relation == "calls"));
            var run = Node(m, "Run");
            Assert.AreEqual(0, run.Hidden); Assert.IsTrue(run.Expanded);
            Assert.AreEqual("Get-ChildItem", run.Externals.Single().Name);
            Assert.IsFalse(m.Nodes.Any(n => n.Id == "ext:gci"));
        }

        [TestMethod]
        public void Open_a_type_to_see_its_members()
        {
            var m = CodeGraphViewData.Build(Input(new[] { Main }, new[] { "Store" }));
            Assert.IsNotNull(Node(m, "Save"));
            Assert.IsTrue(Node(m, "Save").Uncalled);
            Assert.IsTrue(m.Links.Any(l => l.Source == "Store" && l.Target == "Save" && l.Relation == "contains"));
            Assert.IsNull(Node(m, "Cfg"), "i campi non entrano nella cascata");
        }

        [TestMethod]
        public void Put_each_level_one_row_below_the_caller()
        {
            var m = CodeGraphViewData.Build(Input(new[] { Main }, new[] { "M", "Run", "Load" }));
            Assert.AreEqual(0, Node(m, "file:" + Main).Depth);
            Assert.AreEqual(1, Node(m, "M").Depth);
            Assert.AreEqual(2, Node(m, "Run").Depth);
            Assert.AreEqual(3, Node(m, "Load").Depth);
            Assert.AreEqual(4, Node(m, "Parse").Depth, "anche nell'altro file, un livello sotto chi lo chiama");
        }

        [TestMethod]
        public void Hide_a_symbol_but_say_it_where_it_is_called()
        {
            var input = Input(new[] { Main }, new[] { "M", "Run" });
            input.HiddenSymbols.Add("Helper");
            var m = CodeGraphViewData.Build(input);
            Assert.IsNull(Node(m, "Helper"), "nascosto: non si disegna");
            var run = Node(m, "Run");
            CollectionAssert.AreEqual(new[] { "Helper" }, run.HiddenCalls, "la scheda di chi lo chiama lo nomina");
            Assert.AreEqual(0, run.Hidden, "il «+» non conta i nascosti");
            Assert.IsFalse(m.Links.Any(l => l.Source == "Helper" || l.Target == "Helper"));
        }

        [TestMethod]
        public void Drop_hidden_symbols_from_file_to_file_links()
        {
            var input = Input(new string[0], new string[0]);
            input.HiddenSymbols.Add("Load");
            var m = CodeGraphViewData.Build(input);
            Assert.IsFalse(m.Links.Any(l => l.Source == "file:" + Main && l.Target == "file:" + Lib), "l'unico arco fra i file passava per il nascosto");
        }

        [TestMethod]
        public void Keep_file_to_file_overview_when_no_file_is_opened()
        {
            var m = CodeGraphViewData.Build(Input(new string[0], new string[0]));
            Assert.IsTrue(m.Nodes.All(n => n.Kind == "file"));
            var l = m.Links.Single(x => x.Source == "file:" + Main && x.Target == "file:" + Lib);
            Assert.AreEqual("calls", l.Relation);
        }
    }
}
