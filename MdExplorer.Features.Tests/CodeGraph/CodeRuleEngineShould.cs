using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// D42: il motore delle regole di arricchimento, senza Fuseki. Una pagina chiama <c>inviaPagina('alias.metodo')</c>,
    /// l'alias diventa la classe con una mappa; un controller fa <c>crea("alias")</c> e poi <c>imposta("metodo")</c>.
    /// </summary>
    [TestClass]
    public class CodeRuleEngineShould
    {
        private static CodeCallSiteView Site(string caller, int line, string callee, int position, string value, string receiver = null, int ordinal = 0, string path = "web/page.js")
        {
            var s = new CodeCallSiteView { CallerId = caller, Callee = callee, Line = line, Ordinal = ordinal, Receiver = receiver, Path = path, ArgumentCount = position + 1 };
            if (value != null) s.Arguments[position] = value;
            return s;
        }

        private static readonly List<CodeOwnerMember> Owners = new()
        {
            new() { OwnerId = "c-ordini", OwnerQualifiedName = "com.acme.web.OrdiniController", OwnerName = "OrdiniController", MemberId = "m-elenco", MemberName = "elenco" },
            new() { OwnerId = "c-ordini", OwnerQualifiedName = "com.acme.web.OrdiniController", OwnerName = "OrdiniController", MemberId = "m-dettaglio", MemberName = "dettaglio" },
            new() { OwnerId = "c-ordini", OwnerQualifiedName = "com.acme.web.OrdiniController", OwnerName = "OrdiniController", MemberId = "m-dettaglio2", MemberName = "dettaglio" },
            new() { OwnerId = "c-be", OwnerQualifiedName = "com.acme.be.OrdiniBackend", OwnerName = "OrdiniBackend", MemberId = "m-leggi", MemberName = "leggiOrdini" },
            new() { OwnerId = "c-be-copia", OwnerQualifiedName = "com.acme.vecchio.OrdiniBackend", OwnerName = "OrdiniBackend", MemberId = "m-leggi-v", MemberName = "leggiOrdini" },
        };

        private static readonly Dictionary<string, string> Aliases = new() { ["ordini"] = "com.acme.web.OrdiniController", ["be"] = "com.acme.be.OrdiniBackend" };

        private static CodeEnrichmentRule PageRule() => CodeEnrichmentRule.Parse(@"{
            ""id"": ""azione-pagina"", ""description"": ""La pagina chiama il controller con 'alias.metodo'."",
            ""call"": { ""callee"": ""inviaPagina"", ""argument"": 0 },
            ""split"": { ""separator"": ""."" },
            ""owner"": { ""from"": ""string"", ""aliasMap"": ""conf/controller.properties"" },
            ""excludePrefixes"": [ ""redirect:"" ] }");

        [TestMethod]
        public void Link_alias_dot_method_through_the_alias_map_with_rule_certainty()
        {
            var sites = new List<CodeCallSiteView>
            {
                Site("f-a", 3, "inviaPagina", 0, "ordini.elenco"),
                Site("f-b", 7, "inviaPagina", 0, "ordini.dettaglio"),
                Site("f-c", 9, "inviaPagina", 0, "redirect:home"),
                Site("f-d", 11, "inviaPagina", 0, null),            // l'argomento è una variabile
                Site("f-e", 12, "inviaPagina", 0, "ignoto.elenco"),
                Site("f-f", 13, "inviaPagina", 0, "ordini.manca"),
                Site("f-g", 14, "inviaPagina", 0, "senzapunto"),
                Site("f-h", 15, "inviaPagina", 0, "ordini.elenco", path: "web/altra.js"),
            };
            var rule = PageRule();
            CollectionAssert.AreEquivalent(new[] { "com.acme.web.OrdiniController" }, CodeRuleEngine.OwnerKeys(rule, sites, null, Aliases).ToArray());

            var p = CodeRuleEngine.Evaluate(rule, sites, null, Aliases, Owners);

            Assert.AreEqual((8, 1, 1, 3, 3), (p.CallSites, p.NotLiteral, p.Excluded, p.Resolved, p.Unresolved));
            Assert.AreEqual(1, p.MissCounts["aliasNotInMap"]); Assert.AreEqual(1, p.MissCounts["methodNotFound"]); Assert.AreEqual(1, p.MissCounts["noSeparator"]);
            Assert.IsFalse(p.Accepted, "3 su 6 = 50%, sotto la soglia");
            StringAssert.Contains(p.RejectReasons.Single(), "3");
            // gli overload dello stesso nome sono tutti bersagli
            CollectionAssert.AreEquivalent(new[] { "m-dettaglio", "m-dettaglio2" }, p.Edges.Where(e => e.SourceId == "f-b").Select(e => e.TargetId).ToArray());
            var e = p.Edges.First(x => x.SourceId == "f-a");
            Assert.AreEqual(("m-elenco", "dispatches", "rule", 3, "web/page.js"), (e.TargetId, e.Relation, e.Confidence, e.Line, e.Path));
            Assert.AreEqual(3, p.Sample.Count);
            Assert.AreEqual("ignoto.elenco", p.MissExamples.Single(x => x.Miss == CodeRuleMiss.AliasNotInMap).Value);
        }

        [TestMethod]
        public void Accept_a_rule_that_links_most_calls()
        {
            var sites = new List<CodeCallSiteView> { Site("f-a", 3, "inviaPagina", 0, "ordini.elenco"), Site("f-b", 4, "inviaPagina", 0, "ordini.dettaglio"), Site("f-c", 5, "inviaPagina", 0, "ordini.manca") };
            var p = CodeRuleEngine.Evaluate(PageRule(), sites, null, Aliases, Owners);
            Assert.IsTrue(p.Accepted, string.Join(" ", p.RejectReasons));
            Assert.AreEqual(3, p.Edges.Count);
        }

        [TestMethod]
        public void List_contexts_by_size_and_keep_only_the_chosen_scope()
        {
            var sites = new List<CodeCallSiteView>
            {
                Site("f-a", 3, "inviaPagina", 0, "ordini.elenco", path: "prog/web/ordini/a.js"),
                Site("f-b", 4, "inviaPagina", 0, "ordini.elenco", path: "prog/web/ordini/b.js"),
                Site("f-c", 5, "inviaPagina", 0, "ordini.manca", path: "prog/web/clienti/c.js"),
                Site("f-d", 6, "inviaPagina", 0, "redirect:x", path: "prog/web/clienti/c.js"),   // esclusa: non conta nei contesti
            };
            var all = CodeRuleEngine.Evaluate(PageRule(), sites, null, Aliases, Owners);
            CollectionAssert.AreEqual(new[] { "prog/:3:2", "prog/web/:3:2", "prog/web/ordini/:2:2", "prog/web/clienti/:1:0" },
                all.Contexts.Select(c => $"{c.Prefix}:{c.CallSites}:{c.Resolved}").ToArray(), "dalla più grande alla più piccola");

            var rule = PageRule();
            rule.Scope.Add("prog/web/ordini");
            var scoped = CodeRuleEngine.Evaluate(rule, sites, null, Aliases, Owners);
            Assert.AreEqual((2, 0, 1, 2), (scoped.Resolved, scoped.Unresolved, scoped.OutOfScope, scoped.EdgeCount));
            Assert.IsTrue(scoped.Accepted);
            Assert.AreEqual(4, scoped.Contexts.Count, "i contesti restano quelli di tutto il grafo, per cambiare idea");
        }

        [TestMethod]
        public void Take_the_owner_from_the_paired_call_written_before_or_in_the_receiver()
        {
            var rule = CodeEnrichmentRule.Parse(@"{
                ""id"": ""verso-backend"", ""description"": ""Il controller sceglie il servizio e poi il metodo."",
                ""call"": { ""callee"": ""imposta"", ""argument"": 0 },
                ""owner"": { ""from"": ""pairedCall"", ""pairedCall"": { ""callee"": ""crea"", ""argument"": 0 }, ""aliasMap"": ""conf/backend.properties"" } }");
            var sites = new List<CodeCallSiteView>
            {
                Site("m-elenco", 20, "imposta", 0, "leggiOrdini", receiver: "svc", path: "web/OrdiniController.java"),
                Site("m-dettaglio", 31, "imposta", 0, "leggiOrdini", receiver: "crea(\"be\")", ordinal: 0, path: "web/OrdiniController.java"),
                Site("m-solo", 40, "imposta", 0, "leggiOrdini", receiver: "svc", path: "web/OrdiniController.java"),
            };
            var paired = new List<CodeCallSiteView>
            {
                Site("m-elenco", 12, "crea", 0, "altro"),
                Site("m-elenco", 18, "crea", 0, "be"),                 // l'ultima prima della riga 20
                Site("m-elenco", 25, "crea", 0, "altro"),              // dopo: non conta
                Site("m-dettaglio", 31, "crea", 0, "be", ordinal: 1),  // nel ricevente, stessa riga
            };
            var p = CodeRuleEngine.Evaluate(rule, sites, paired, Aliases, Owners);

            Assert.AreEqual((2, 1), (p.Resolved, p.Unresolved));
            Assert.AreEqual(1, p.MissCounts["noPairedCall"]);
            CollectionAssert.AreEquivalent(new[] { "m-elenco>m-leggi", "m-dettaglio>m-leggi" }, p.Edges.Select(x => x.SourceId + ">" + x.TargetId).ToArray(),
                "il nome qualificato della mappa sceglie com.acme.be, non la copia in com.acme.vecchio");
        }

        [TestMethod]
        public void Refuse_an_incomplete_or_unknown_rule_saying_what_is_wrong()
        {
            var ex = Assert.ThrowsException<CodeRuleException>(() => CodeEnrichmentRule.Parse(@"{ ""id"": ""x"", ""description"": ""d"", ""call"": { ""callee"": ""f"", ""argument"": 0 }, ""owner"": { ""from"": ""string"" } }"));
            StringAssert.Contains(ex.Message, "split");
            Assert.ThrowsException<CodeRuleException>(() => CodeEnrichmentRule.Parse(@"{ ""id"": ""x"", ""description"": ""d"", ""call"": { ""callee"": ""f"", ""argument"": 0 }, ""owner"": { ""from"": ""class"", ""class"": ""A"" }, ""campo"": 1 }"),
                "un campo che il formato non conosce");
            Assert.ThrowsException<CodeRuleException>(() => CodeEnrichmentRule.Parse(@"{ ""id"": ""X Y"", ""description"": ""d"", ""call"": { ""callee"": ""f"", ""argument"": 0 }, ""owner"": { ""from"": ""class"", ""class"": ""A"" } }"));
            Assert.ThrowsException<CodeRuleException>(() => CodeEnrichmentRule.Parse(@"{ ""id"": ""x"", ""description"": ""d"", ""call"": { ""callee"": ""f"", ""argument"": 0 }, ""owner"": { ""from"": ""class"", ""class"": ""A"", ""aliasMap"": ""../fuori.properties"" } }"));
        }

        [TestMethod]
        public void Read_a_properties_file_as_a_map()
        {
            var map = PropertiesFile.Parse("# commento\n! altro\nordini = com.acme.web.OrdiniController\nbe:com.acme.be.\\\n   OrdiniBackend\n\nvuoto=\n");
            Assert.AreEqual("com.acme.web.OrdiniController", map["ordini"]);
            Assert.AreEqual("com.acme.be.OrdiniBackend", map["be"]);
            Assert.AreEqual(string.Empty, map["vuoto"]);
            Assert.AreEqual(3, map.Count);
        }
    }
}
