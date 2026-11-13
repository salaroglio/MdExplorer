using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// I cataloghi di SQL in XML (13/11/2026, rilevazione dal cliente: il livello dati legge le frasi da file XML per gruppo e
    /// nome e le esegue; il grafo si fermava al lettore). I tag e i metodi qui sono inventati, nelle due forme che MarkAgent
    /// ha misurato: query con nome e frammenti dentro un gruppo; query che hanno per nome il loro elemento, dentro una sezione.
    /// </summary>
    [TestClass]
    public class XmlCatalogShould
    {
        internal const string Definition = @"schema: 1
id: catalogo
name: Cataloghi SQL
version: 1
extensions: [.xml]
xml:
  queries:
    - match: '//Gruppo/Query'
      name: 'Nome'
      group: '../@name'
      fragments: '*[not(self::Nome)]'
    - match: '//Sezione/*'
      name: 'name()'
      group: '../@ID'
  loaders:
    - method: Leggi
      receiver: Lettore
      file: 0
      group: 1
      name: 2
    - method: LeggiFrammento
      group: 1
      name: 2
      fragment: 3
    - method: Componi
      group: 1
      name: 2
      fragments: [3, 4]
";

        private const string Main = @"<?xml version=""1.0""?>
<Radice>
  <Gruppo name=""ORDINI"">
    <Query>
      <Nome>ELENCO</Nome>
      <Testo><![CDATA[SELECT o.ID FROM ORDINI o, CLIENTI c
        WHERE o.CLI = c.ID AND o.STATO IN ({0})]]></Testo>
      <Ordine>ORDER BY {1} {2}</Ordine>
    </Query>
    <Query>
      <Nome>STORICO</Nome>
      <Testo>SELECT * FROM {0} WHERE DATA &gt; {1}</Testo>
      <Altro>SELECT * FROM ARCHIVIO_{0}</Altro>
    </Query>
    <Query>
      <Nome>ELENCO</Nome>
      <Testo>UPDATE ORDINI SET STATO = {0}</Testo>
    </Query>
  </Gruppo>
</Radice>
";

        private const string Alternative = @"<Radice>
  <Sezione ID=""CLIENTI"">
    <CERCA>SELECT NOME FROM CLIENTI WHERE ID = {0}</CERCA>
    <SALVA><![CDATA[INSERT INTO CLIENTI (ID, NOME) VALUES ({0}, '{1}')]]></SALVA>
  </Sezione>
</Radice>
";

        private static DeclarativeLanguage Language(string yaml = Definition) => DeclarativeLanguage.Parse(yaml, "/p/.mde-languages/catalogo/language.yml");

        private static List<CodeScript> Extract(out CodeExtractionResult result, params (string Name, string Text)[] files)
        {
            var root = Path.Combine(Path.GetTempPath(), "mde-xmlcatalog-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var all = files.Select(f => { var p = Path.Combine(root, f.Name); File.WriteAllText(p, f.Text); return new CodeSourceFile(p, "web/sql/" + f.Name); }).ToList();
                result = new XmlCatalogExtractor(Language()).Extract(new CodeExtractionRequest { ProjectPath = root, AllFiles = all, FilesToEmit = all });
                return result.Scripts.Select(s => s.Script).ToList();
            }
            finally { try { Directory.Delete(root, true); } catch (IOException) { } }
        }

        private static string Out(List<CodeScript> s, string from)
        {
            var names = s.SelectMany(x => x.Symbols).GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().QualifiedName);
            var id = s.SelectMany(x => x.Symbols).Single(x => x.QualifiedName == from).Id;
            return string.Join(" | ", s.SelectMany(x => x.Edges).Where(e => e.SourceId == id && e.Relation != "contains")
                .Select(e => $"{e.Relation}:{names[e.TargetId]}:{e.Confidence}").OrderBy(x => x, StringComparer.Ordinal));
        }

        [TestMethod]
        public void Read_queries_groups_and_fragments_with_the_tables_of_their_text()
        {
            var s = Extract(out var result, ("ordini.xml", Main), ("clienti.xml", Alternative));
            var symbols = s.SelectMany(x => x.Symbols).Where(x => x.Kind is "query" or "namespace").Select(x => $"{x.Kind}:{x.QualifiedName}:{x.LineStart}").ToList();
            CollectionAssert.IsSubsetOf(new[]
            {
                "namespace:web/sql/ordini.xml:ORDINI:3", "query:web/sql/ordini.xml:ORDINI.ELENCO:4", "query:web/sql/ordini.xml:ORDINI.ELENCO.Testo:6",
                "query:web/sql/ordini.xml:ORDINI.ELENCO.Ordine:8", "query:web/sql/ordini.xml:ORDINI.STORICO:10",
                "namespace:web/sql/clienti.xml:CLIENTI:2", "query:web/sql/clienti.xml:CLIENTI.CERCA:3", "query:web/sql/clienti.xml:CLIENTI.SALVA:4",
            }, symbols, string.Join("\n", symbols));
            Assert.AreEqual("reads:CLIENTI:inferred | reads:ORDINI:inferred", Out(s, "web/sql/ordini.xml:ORDINI.ELENCO.Testo"), "FROM con l'elenco su due righe, dentro CDATA");
            Assert.AreEqual("", Out(s, "web/sql/ordini.xml:ORDINI.ELENCO.Ordine"), "un pezzo di frase senza tabelle");
            Assert.AreEqual("reads:?:ambiguous", Out(s, "web/sql/ordini.xml:ORDINI.STORICO.Testo"), "FROM {0}: la tabella la sceglie il codice");
            Assert.AreEqual("reads:?:ambiguous", Out(s, "web/sql/ordini.xml:ORDINI.STORICO.Altro"), "ARCHIVIO_{0}: nemmeno un pezzo del nome è la tabella");
            Assert.AreEqual("reads:CLIENTI:inferred", Out(s, "web/sql/clienti.xml:CLIENTI.CERCA"));
            Assert.AreEqual("writes:CLIENTI:inferred", Out(s, "web/sql/clienti.xml:CLIENTI.SALVA"));
            StringAssert.Contains(result.Warnings.Single(), "web/sql/ordini.xml:15: 'ORDINI.ELENCO' is already declared", "la stessa query due volte nello stesso gruppo si dice");
            Assert.AreEqual("writes:ORDINI:inferred", Out(s, "web/sql/ordini.xml:ORDINI.ELENCO@15.Testo"));
        }

        [TestMethod]
        public void Keep_a_file_that_is_not_valid_xml_without_queries_and_say_so()
        {
            var s = Extract(out var result, ("rotto.xml", "<Radice><Gruppo name=\"A\"></Radice>"));
            Assert.AreEqual("module", s.Single().Symbols.Single().Kind);
            StringAssert.Contains(result.Warnings.Single(), "invalid XML");
        }

        private static CodeCallSiteView Site(string caller, string method, int line, params string[] args)
        {
            var site = new CodeCallSiteView { CallerId = caller, Callee = method, Path = "web/Dl.vb", Line = line, Receiver = "Lettore" };
            for (var i = 0; i < args.Length; i++) if (args[i] != null) site.Arguments[i] = args[i];
            return site;
        }

        [TestMethod]
        public void Link_each_read_to_the_query_or_the_fragments_it_names()
        {
            var entries = new List<XmlCatalogLinker.Entry>
            {
                new("q1", "web/sql/ordini.xml", "ORDINI", "ELENCO", null), new("f1", "web/sql/ordini.xml", "ORDINI", "ELENCO", "Testo"), new("f2", "web/sql/ordini.xml", "ORDINI", "ELENCO", "Ordine"),
                new("q2", "web/sql/clienti.xml", "CLIENTI", "CERCA", null),
                new("q3", "web/sql/vecchio.xml", "CLIENTI", "CERCA", null),
            };
            var sites = new Dictionary<string, List<CodeCallSiteView>>
            {
                ["Leggi"] = new() { Site("m1", "Leggi", 10, null, "CLIENTI", "CERCA"), Site("m2", "Leggi", 20, "C:\\x\\clienti.xml", "CLIENTI", "CERCA"), Site("m3", "Leggi", 30, null, "ORDINI", "ELENCO"),
                                    Site("m4", "Leggi", 40, null, "ORDINI", "MANCA"), Site("m5", "Leggi", 50, null, "ORDINI", null) },
                ["LeggiFrammento"] = new() { Site("m6", "LeggiFrammento", 60, null, "ORDINI", "ELENCO", "Ordine") },
                ["Componi"] = new() { Site("m7", "Componi", 70, null, "ORDINI", "ELENCO", "Testo", "Ordine") },
            };
            var misses = new List<XmlCatalogLinker.Miss>();
            var edges = XmlCatalogLinker.Link(Language().Xml.Loaders, m => sites.TryGetValue(m, out var s) ? s : new List<CodeCallSiteView>(), entries, misses);
            var text = string.Join(" | ", edges.Select(e => $"{e.SourceId}>{e.TargetId}:{e.Confidence}"));
            Assert.AreEqual(
                "m1>q2:ambiguous | m1>q3:ambiguous | m2>q2:inferred | m3>f1:ambiguous | m3>f2:ambiguous | m6>f2:inferred | m7>f1:inferred | m7>f2:inferred", text,
                "m1: due cataloghi con CLIENTI.CERCA e il file non si sa; m2: il file lo dice; m3: senza frammento, uno dei due; m6: il frammento scritto; m7: tutti e due, eseguiti insieme");
            Assert.IsTrue(edges.All(e => e.Relation == "dispatches"));
            Assert.AreEqual("ORDINI.MANCA", misses.Single().Text, "un nome che nessun catalogo ha si dice; un nome che non si sa (m5) no: non c'è niente da dire");
            StringAssert.Contains(edges.First(e => e.SourceId == "m2").Guard, "name = \"CERCA\"");
        }

        [TestMethod]
        public void Link_by_the_reader_class_with_fixed_names_and_the_file_name_only()
        {
            // 13/11/2026, dal cliente: un lettore C# omonimo per un altro XML; un metodo di paginazione che legge sempre la stessa
            // query; il file passato come MapPath("~/sql/x.xml") o configurazione + "\x.xml"
            var yaml = Definition[..Definition.IndexOf("  loaders:", StringComparison.Ordinal)] +
                "  loaders:\n" +
                "    - method: Leggi\n      class: Dati.Lettore\n      file: 0\n      group: 1\n      name: 2\n" +
                "    - method: Pagina\n      class: Lettore\n      file: 0\n      groupValue: ORDINI\n      nameValue: ELENCO\n      defaultFragment: Testo\n";
            var entries = new List<XmlCatalogLinker.Entry>
            {
                new("q1", "web/sql/ordini.xml", "ORDINI", "ELENCO", null), new("f1", "web/sql/ordini.xml", "ORDINI", "ELENCO", "Testo"),
                new("q9", "web/sql/copia.xml", "ORDINI", "ELENCO", null), new("f9", "web/sql/copia.xml", "ORDINI", "ELENCO", "Testo"),
                new("q2", "web/sql/clienti.xml", "CLIENTI", "CERCA", null), new("q3", "web/sql/vecchio.xml", "CLIENTI", "CERCA", null),
            };
            CodeCallSiteView With(CodeCallSiteView s, string type, string file) { s.CalleeType = type; if (file != null) s.FileNames[0] = file; return s; }
            var sites = new Dictionary<string, List<CodeCallSiteView>>
            {
                ["Leggi"] = new()
                {
                    With(Site("m1", "Leggi", 10, null, "CLIENTI", "CERCA"), "Demo.Dati.Lettore", "clienti.xml"),
                    With(Site("m2", "Leggi", 20, null, "CLIENTI", "CERCA"), "Demo.Altro.Lettore", "clienti.xml"),
                    With(Site("m3", "Leggi", 30, null, "CLIENTI", "CERCA"), null, "clienti.xml"),
                },
                ["Pagina"] = new() { With(Site("m4", "Pagina", 40), "Demo.Dati.Lettore", "ordini.xml"), With(Site("m5", "Pagina", 50), "Demo.Dati.Lettore", null) },
            };
            var misses = new List<XmlCatalogLinker.Miss>();
            var edges = XmlCatalogLinker.Link(Language(yaml).Xml.Loaders, m => sites.TryGetValue(m, out var s) ? s : new List<CodeCallSiteView>(), entries, misses);

            Assert.AreEqual("m1>q2:inferred | m4>f1:inferred | m5>f1:ambiguous | m5>f9:ambiguous", string.Join(" | ", edges.Select(e => $"{e.SourceId}>{e.TargetId}:{e.Confidence}")),
                "m1: la classe e il nome del file; m2: un'altra classe, non è il lettore; m3: non risolto, non si dice; m4: gruppo e nome fissi, il file dal nome; m5: il file non si sa");
            Assert.AreEqual(0, misses.Count);
            StringAssert.Contains(edges.First(e => e.SourceId == "m4").Guard, "file = \"ordini.xml\"");
        }

        [DataTestMethod]
        [DataRow("xml:\n  queries:\n    - match: '//Q'\n", "xml.queries[0].name")]
        [DataRow("xml:\n  queries:\n    - match: '//Q['\n      name: 'N'\n", "XPath")]
        [DataRow("xml:\n  queries:\n    - match: '//Q'\n      name: 'N'\n  loaders:\n    - method: Leggi\n", "xml.loaders[0].name")]
        [DataRow("blocks:\n  - id: b\n    kind: class\n    begin: '(?<name>x)'\n    end: 'y'\nxml:\n  queries:\n    - match: '//Q'\n      name: 'N'\n", "blocks")]
        public void Refuse_a_broken_xml_section_saying_where(string body, string expected)
        {
            var yaml = "schema: 1\nid: catalogo\nversion: 1\nextensions: [.xml]\n" + body;
            var ex = Assert.ThrowsException<LanguageDefinitionException>(() => Language(yaml));
            StringAssert.Contains(ex.Message, expected);
        }

        [TestMethod]
        public async Task Go_from_the_data_layer_through_the_query_to_its_tables()
        {
            var store = FusekiFixture.NewStore();
            var root = Path.Combine(Path.GetTempPath(), "mde-xmlcatalog-tests", Guid.NewGuid().ToString("N"));
            var project = Path.Combine(root, "project");
            try
            {
                var web = Path.Combine(project, "web");
                Directory.CreateDirectory(Path.Combine(web, "sql"));
                var package = Path.Combine(DeclarativeLanguage.FolderOf(project), "catalogo");
                Directory.CreateDirectory(package);
                File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Definition);
                File.WriteAllText(Path.Combine(web, "sql", "clienti.xml"), Alternative);
                // il lettore sta fuori dal perimetro (come dal cliente): la chiamata resta un nome, con il suo argomento
                File.WriteAllText(Path.Combine(web, "Dl.vb"),
                    "Public Class Dati\n" +
                    "  Public Function Cerca(ByVal id As Integer) As String\n" +
                    "    Dim nome As String = \"CERCA\"\n" +
                    "    Dim testo As String = Lettore.Leggi(percorso, \"CLIENTI\", nome)\n" +
                    "    Return testo\n" +
                    "  End Function\n" +
                    "End Class\n");
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
                var catalogs = CodeLanguageCatalog.ForProject(project).ById("catalogo").Definition;
                Emit(new XmlCatalogExtractor(catalogs), "*.xml");
                var load = await store.LoadProjectAsync(project);
                Assert.AreEqual(0, load.Failed, load.FirstError);
                var (edges, misses) = await new CodeRuleService(store, NullLogger<CodeRuleService>.Instance).ApplyXmlCatalogsAsync(project, new[] { catalogs });
                Assert.AreEqual((1, 0), (edges, misses.Count));

                var cerca = (await store.ResolveAsync(project, "Dati.Cerca")).Single(x => x.Kind == "method").Id;
                var query = (await store.NeighborsAsync(project, cerca, "dispatches", "out", 1, 5)).Single().Other;
                Assert.AreEqual("web/sql/clienti.xml:CLIENTI.CERCA", query.QualifiedName, "il nome passato per una variabile");
                Assert.AreEqual("CLIENTI", (await store.NeighborsAsync(project, query.Id, "reads", "out", 1, 5)).Single().Other.Name, "e dalla query la tabella");
            }
            finally { try { Directory.Delete(root, true); } catch { } }
        }
    }
}
