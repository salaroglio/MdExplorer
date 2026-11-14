using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// La navigazione dei metodi (14/11/2026): pagina → controllo → gestore → business layer → data layer → query del catalogo →
    /// tabelle, e all'indietro. Un grafo piccolo con tutti i casi: il percorso fino ai dati, un esterno, una foglia, un arco
    /// ambiguo, un metodo del data layer che nessuno chiama e uno chiamato solo da codice non raggiunto.
    /// </summary>
    [TestClass]
    public class CodeNavigationShould
    {
        private static readonly Dictionary<string, (string Kind, string Name, string Path)> N = new()
        {
            ["page"] = ("module", "web/Ordini.aspx", "web/Ordini.aspx"),
            ["btn"] = ("control", "web/Ordini.aspx#btnSalva", "web/Ordini.aspx"),
            ["h"] = ("method", "Web.Ordini.btnSalva_Click(Object, EventArgs)", "web/Ordini.aspx.vb"),
            ["bl"] = ("method", "Bl.Ordini.Salva()", "bl/Ordini.vb"),
            ["dl"] = ("method", "Dl.Ordini.Inserisci()", "dl/Ordini.vb"),
            ["dl2"] = ("method", "Dl.Ordini.Storico()", "dl/Ordini.vb"),
            ["orphan"] = ("method", "Dl.Ordini.Mai()", "dl/Ordini.vb"),
            ["dead"] = ("method", "Dl.Ordini.SoloDaMorto()", "dl/Ordini.vb"),
            ["deadCaller"] = ("method", "Bl.Vecchio.Usa()", "bl/Vecchio.vb"),
            ["q"] = ("query", "dl/sql/ordini.xml:ORDINI.INSERISCI", "dl/sql/ordini.xml"),
            ["qa"] = ("query", "dl/sql/a.xml:ORDINI.STORICO", "dl/sql/a.xml"),
            ["qb"] = ("query", "dl/sql/b.xml:ORDINI.STORICO", "dl/sql/b.xml"),
            ["log"] = ("external", "log4net.ILog.Info", null),
            ["fmt"] = ("method", "Bl.Ordini.Formatta()", "bl/Ordini.vb"),
            ["load"] = ("method", "Web.Ordini.Page_Load(Object, EventArgs)", "web/Ordini.aspx.vb"),
            ["init"] = ("method", "Dl.Ordini.Inizializza()", "dl/Ordini.vb"),
            ["T_ORD"] = ("external", "ORDINI", null),
            ["T_RIG"] = ("external", "RIGHE", null),
            ["T_STO"] = ("external", "STORICO", null),
        };

        private static CodeNavEdge E(string s, string t, string rel, string conf = "extracted", int line = 1)
            => new(s, N[s].Kind, N[s].Name, N[s].Path, t, N[t].Kind, N[t].Name, N[t].Path, rel, conf, N[s].Path, line);

        private static CodeNavigation.Graph Graph() => new(new[]
        {
            E("page", "btn", "contains"),
            E("btn", "h", "calls"),
            E("h", "bl", "calls", line: 12),
            E("bl", "fmt", "calls", line: 20),
            E("bl", "log", "calls", line: 21),
            E("bl", "dl", "calls", "inferred", 22),
            E("bl", "dl2", "calls", line: 23),
            E("dl", "q", "dispatches", "inferred", 30),
            E("dl2", "qa", "dispatches", "ambiguous", 40),
            E("dl2", "qb", "dispatches", "ambiguous", 40),
            E("q", "T_ORD", "writes"), E("q", "T_RIG", "writes"),
            E("qa", "T_STO", "reads"), E("qb", "T_STO", "reads"),
            E("deadCaller", "dead", "calls"),
            // il ciclo di vita: la pagina chiama Page_Load (AutoEventWireup), e solo da lì si arriva a Inizializza
            E("page", "load", "calls"), E("load", "init", "calls"),
            // contains verso un metodo è struttura, non esecuzione: non conta
            E("page", "h", "contains"),
        });

        [TestMethod]
        public void Follow_a_page_down_to_its_tables_with_a_shortest_chain()
        {
            var r = CodeNavigation.From(Graph(), "page", 50, 50);

            CollectionAssert.AreEqual(new[] { "ORDINI", "RIGHE", "STORICO" }, r.Tables.Select(t => t.Table).ToArray());
            var ordini = r.Tables.Single(t => t.Table == "ORDINI");
            Assert.IsTrue(ordini.Writes && !ordini.Reads);
            CollectionAssert.AreEqual(new[] { "web/Ordini.aspx#btnSalva", "Web.Ordini.btnSalva_Click(Object, EventArgs)", "Bl.Ordini.Salva()", "Dl.Ordini.Inserisci()", "dl/sql/ordini.xml:ORDINI.INSERISCI", "ORDINI" },
                ordini.Chain.Select(s => s.Symbol).ToArray(), "pagina → controllo → gestore → BL → DL → query → tabella");
            Assert.AreEqual("bl/Ordini.vb:22", ordini.Chain[3].At, "ogni passo dice dove è scritto");
            Assert.AreEqual("inferred", ordini.Weakest);

            var storico = r.Tables.Single(t => t.Table == "STORICO");
            Assert.AreEqual(2, storico.SqlIn, "due query dello stesso nome in due file: tutte e due leggono");
            Assert.AreEqual("ambiguous", storico.Weakest);
        }

        [TestMethod]
        public void Count_every_stop_before_the_data_with_its_cause()
        {
            var r = CodeNavigation.From(Graph(), "page", 50, 50);

            Assert.AreEqual(1, r.ExternalStops);
            Assert.AreEqual("log4net.ILog.Info", r.Externals.Single().Symbol);
            Assert.AreEqual("bl/Ordini.vb:21", r.Externals.Single().At);
            CollectionAssert.AreEquivalent(new[] { "Bl.Ordini.Formatta()", "Dl.Ordini.Inizializza()" }, r.Leaves.Select(l => l.Symbol).ToArray(),
                "non chiamano niente e non hanno SQL; le query non sono foglie, arrivano ai dati");
            Assert.AreEqual(2, r.LeafStops);
            Assert.AreEqual(2, r.UncertainEdges["ambiguous"]);
            Assert.AreEqual(2, r.UncertainEdges["inferred"]);
            Assert.AreEqual(0, r.Unexplored);
        }

        [TestMethod]
        public void Say_when_the_depth_limit_cut_the_walk()
        {
            var r = CodeNavigation.From(Graph(), "page", 3, 50);
            Assert.AreEqual(3, r.MaxDepth);
            Assert.IsTrue(r.Unexplored > 0, "il BL al terzo passo ha ancora archi");
            Assert.AreEqual(0, r.Tables.Count);
        }

        [TestMethod]
        public void Go_back_from_a_table_to_the_page_controls_that_reach_it()
        {
            var r = CodeNavigation.To(Graph(), "T_ORD", 50, 50);

            Assert.AreEqual(1, r.Entries);
            Assert.AreEqual("module", r.List.Single().Kind, "la pagina è l'ingresso: nessuno la esegue");
            CollectionAssert.AreEqual(new[] { "web/Ordini.aspx#btnSalva", "Web.Ordini.btnSalva_Click(Object, EventArgs)", "Bl.Ordini.Salva()", "Dl.Ordini.Inserisci()", "dl/sql/ordini.xml:ORDINI.INSERISCI", "ORDINI" },
                r.List.Single().Chain.Select(s => s.Symbol).ToArray(), "dall'ingresso alla tabella");
        }

        [TestMethod]
        public void Measure_which_data_layer_methods_the_pages_reach_and_why_not()
        {
            var symbols = N.Where(kv => kv.Value.Kind != "external")
                .Select(kv => (kv.Key, kv.Value.Kind, kv.Value.Name, kv.Value.Path)).ToList();
            var r = CodeNavigation.Over(Graph(), symbols, "web/", "dl/", 50, 50);

            Assert.AreEqual("page+control", r.EntriesKind);
            Assert.AreEqual(2, r.EntryCount, "la pagina e il suo controllo");
            Assert.AreEqual(8, r.Targets, "5 metodi del data layer (Inserisci, Storico, Inizializza, Mai, SoloDaMorto) e le sue 3 query");
            Assert.AreEqual(6, r.Reached, "anche Inizializza, che si raggiunge solo dal ciclo di vita della pagina");
            Assert.AreEqual(1, r.UnreachedByWhy["no-callers"]);
            Assert.AreEqual(1, r.UnreachedByWhy["callers-not-reached"]);
            Assert.AreEqual("Bl.Vecchio.Usa()", r.Unreached.Single(u => u.Why == "callers-not-reached").ExampleCaller);
            Assert.AreEqual("Dl.Ordini.Mai()", r.Unreached.Single(u => u.Why == "no-callers").Symbol);
        }
    }
}
