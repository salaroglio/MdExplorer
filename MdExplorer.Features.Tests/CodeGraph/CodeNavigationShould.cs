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
            // Mai è dichiarato anche in una copia: lo stesso nodo con due percorsi, contato una volta
            symbols.Add(("orphan", "method", "Dl.Ordini.Mai()", "dl/copia/Ordini.vb"));
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

        // ---- i controlli di qualità: conti per causa e campioni che si ripetono (14/11/2026)

        /// <summary>I nodi in più dei test di qualità: una pagina per esito, un metodo per causa.</summary>
        private static readonly Dictionary<string, (string Kind, string Name, string Path)> Q = new()
        {
            ["p2"] = ("module", "web/Storico.aspx", "web/Storico.aspx"), ["b2"] = ("control", "web/Storico.aspx#btnCerca", "web/Storico.aspx"),
            ["h2"] = ("method", "Web.Storico.btnCerca_Click()", "web/Storico.aspx.vb"),
            ["p3"] = ("module", "web/Login.aspx", "web/Login.aspx"), ["h3"] = ("method", "Web.Login.Page_Load()", "web/Login.aspx.vb"),
            ["unres"] = ("external", "Sicurezza.Verifica", null),
            ["p4"] = ("module", "web/Stampa.aspx", "web/Stampa.aspx"), ["h4"] = ("method", "Web.Stampa.Page_Load()", "web/Stampa.aspx.vb"),
            ["p5"] = ("module", "web/Info.aspx", "web/Info.aspx"), ["h5"] = ("method", "Web.Info.Page_Load()", "web/Info.aspx.vb"),
            ["evt"] = ("external", "System.Web.UI.Control.Unload", null), ["hEvt"] = ("method", "Web.Ordini.Chiudi()", "web/Ordini.aspx.vb"),
            ["dlEvt"] = ("method", "Dl.Ordini.Rilascia()", "dl/Ordini.vb"),
            ["pa"] = ("module", "admin/Gestione.aspx", "admin/Gestione.aspx"), ["ha"] = ("method", "Admin.Gestione.Page_Load()", "admin/Gestione.aspx.vb"),
            ["dlAdmin"] = ("method", "Dl.Ordini.Azzera()", "dl/Ordini.vb"),
            ["c1"] = ("method", "Dl.Giro.Uno()", "dl/Giro.vb"), ["c2"] = ("method", "Dl.Giro.Due()", "dl/Giro.vb"),
        };

        private static (string Kind, string Name, string Path) Node(string id) => N.TryGetValue(id, out var n) ? n : Q[id];

        private static CodeNavEdge E2(string s, string t, string rel, string conf = "extracted", int line = 1)
            => new(s, Node(s).Kind, Node(s).Name, Node(s).Path, t, Node(t).Kind, Node(t).Name, Node(t).Path, rel, conf, Node(s).Path, line);

        private static CodeNavigation.Graph QualityGraph() => new(new[]
        {
            E("page", "btn", "contains"), E("btn", "h", "calls"), E("h", "bl", "calls", line: 12), E("bl", "fmt", "calls", line: 20), E("bl", "log", "calls", line: 21),
            E("bl", "dl", "calls", "inferred", 22), E("bl", "dl2", "calls", line: 23), E("dl", "q", "dispatches", "inferred", 30),
            E("dl2", "qa", "dispatches", "ambiguous", 40), E("dl2", "qb", "dispatches", "ambiguous", 40),
            E("q", "T_ORD", "writes"), E("q", "T_RIG", "writes"), E("qa", "T_STO", "reads"), E("qb", "T_STO", "reads"),
            E("deadCaller", "dead", "calls"),
            // Storico: ai dati solo per l'arco ambiguo del lettore
            E2("p2", "b2", "contains"), E2("b2", "h2", "calls"), E2("h2", "dl2", "calls"),
            // Login: si ferma su un nome che il compilatore non ha risolto
            E2("p3", "h3", "calls"), E2("h3", "unres", "calls", "ambiguous", 7),
            // Stampa: si ferma su una libreria risolta
            E2("p4", "h4", "calls"), E2("h4", "log", "calls", line: 9),
            // Info: tutto nel grafo, niente dati
            E2("p5", "h5", "calls"), E2("h5", "fmt", "calls"),
            // un evento del framework che nessuno solleva nel grafo → Chiudi → Rilascia
            E2("evt", "hEvt", "calls", "inferred"), E2("hEvt", "dlEvt", "calls"),
            // una pagina fuori da web/ arriva ad Azzera
            E2("pa", "ha", "calls"), E2("ha", "dlAdmin", "calls"),
            // due metodi che si chiamano solo fra loro
            E2("c1", "c2", "calls"), E2("c2", "c1", "calls"),
        });

        private static List<(string Id, string Kind, string Name, string Path)> QualitySymbols()
            => N.Concat(Q).Where(kv => kv.Value.Kind != "external").Select(kv => (kv.Key, kv.Value.Kind, kv.Value.Name, kv.Value.Path)).ToList();

        [TestMethod]
        public void Count_the_pages_by_how_they_reach_the_data_and_sample_only_on_request()
        {
            var r = CodeNavigation.EntriesToTables(QualityGraph(), QualitySymbols(), "web/", "page", null, 0, "1", 10);

            Assert.AreEqual(("page", 5), (r.Unit, r.Total), "le cinque pagine di web/; quella di admin/ è fuori");
            Assert.AreEqual("data=1, data-only-through-ambiguous=1, stops-at-unresolved-name=1, stops-at-library=1, no-data-in-graph=1",
                string.Join(", ", CodeNavigation.Outcomes.Select(o => $"{o}={r.ByOutcome[o]}")));
            Assert.AreEqual(0, r.Samples.Count, "senza sample: solo conti");
            Assert.AreEqual("Sicurezza.Verifica:True:1 | log4net.ILog.Info:False:1", string.Join(" | ", r.TopExternalStops.Select(x => $"{x.Symbol}:{x.Unresolved}:{x.Entries}")),
                "gli esterni che fermano le pagine senza dati, prima i nomi non risolti; Ordini arriva ai dati e il suo log non conta");

            var sampled = CodeNavigation.EntriesToTables(QualityGraph(), QualitySymbols(), "web/", "page", CodeNavigation.OutcomeUnresolved, 3, "1", 10);
            Assert.AreEqual(CodeNavigation.OutcomeUnresolved, sampled.Samples.Keys.Single(), "solo l'esito chiesto");
            var login = sampled.Samples[CodeNavigation.OutcomeUnresolved].Single();
            Assert.AreEqual(("web/Login.aspx", 0, 1), (login.Symbol, login.Tables, login.ExternalStops));
            CollectionAssert.AreEqual(new[] { "Web.Login.Page_Load()", "Sicurezza.Verifica" }, login.Chain.Select(c => c.Symbol).ToArray(), "la catena fino all'esterno");
            Assert.AreEqual("web/Login.aspx.vb:7", login.Chain[1].At);
            Assert.AreEqual(5, sampled.Total, "i conti restano di tutti, non del campione");
        }

        [TestMethod]
        public void Count_single_controls_when_the_unit_is_the_control()
        {
            var r = CodeNavigation.EntriesToTables(QualityGraph(), QualitySymbols(), "web/", "control", null, 0, "1", 10);
            Assert.AreEqual(("control", 2), (r.Unit, r.Total));
            Assert.AreEqual((1, 1), (r.ByOutcome[CodeNavigation.OutcomeData], r.ByOutcome[CodeNavigation.OutcomeDataAmbiguous]), "btnSalva sicuro, btnCerca solo per l'arco ambiguo");
        }

        [TestMethod]
        public void Count_the_unreached_code_by_where_its_road_starts()
        {
            var r = CodeNavigation.UnreachedByCause(QualityGraph(), QualitySymbols(), "web/", "dl/", null, 0, "1", 30);

            Assert.AreEqual("page+control", r.EntriesKind);
            Assert.AreEqual((12, 5, 7), (r.Targets, r.Reached, r.Unreached), "raggiunti: Inserisci, Storico e le 3 query");
            Assert.AreEqual("no-callers=2, started-by-other-pages=1, started-by-external=1, started-by-uncalled-code=1, only-in-a-cycle=2",
                string.Join(", ", CodeNavigation.Causes.Select(c => $"{c}={r.ByCause[c]}")),
                "Mai e Inizializza (qui senza Page_Load) senza chiamanti; Azzera dalla pagina di admin; Rilascia dall'evento; SoloDaMorto da codice che nessuno chiama; Uno e Due in giro");
            Assert.AreEqual(0, r.Samples.Count);
            Assert.AreEqual("dl/:9:2 | dl/sql:3:3", string.Join(" | ", r.Folders.OrderBy(f => f.Folder).Select(f => $"{f.Folder}:{f.Targets}:{f.Reached}")));

            var sampled = CodeNavigation.UnreachedByCause(QualityGraph(), QualitySymbols(), "web/", "dl/", CodeNavigation.CauseExternalRoot, 5, "1", 30);
            var rilascia = sampled.Samples[CodeNavigation.CauseExternalRoot].Single();
            Assert.AreEqual(("Dl.Ordini.Rilascia()", "Web.Ordini.Chiudi()", "System.Web.UI.Control.Unload", "external"),
                (rilascia.Symbol, rilascia.ExampleCaller, rilascia.ExampleRoot, rilascia.ExampleRootKind), "chi lo chiama e dove comincia la strada");
        }

        [TestMethod]
        public void Give_the_same_sample_for_the_same_seed()
        {
            var ids = Enumerable.Range(1, 200).Select(i => "id" + i).ToList();
            var a = CodeNavigation.Sample(ids, x => x, 10, "verifica-1");
            CollectionAssert.AreEqual(a, CodeNavigation.Sample(ids.AsEnumerable().Reverse(), x => x, 10, "verifica-1"), "stesso seme, stessi casi, in qualunque ordine arrivino");
            CollectionAssert.AreNotEqual(a, CodeNavigation.Sample(ids, x => x, 10, "verifica-2"), "un altro seme, un altro campione");
            Assert.AreEqual(10, a.Distinct().Count());
            Assert.AreEqual(0, CodeNavigation.Sample(ids, x => x, 0, "x").Count);
            Assert.AreEqual(200, CodeNavigation.Sample(ids, x => x, 500, "x").Count, "più del totale: tutti");
        }
    }
}
