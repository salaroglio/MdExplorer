using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// D42 passo 2: le pagine a direttive. Un dialetto inventato per i test (<c>{%parola(argomento)%}</c>) in un pacchetto
    /// <c>.mde-languages/pagine/</c>: include, condizioni con ramo «altrimenti» e casi, cicli; l'HTML (controlli, script
    /// incorporati, <c>&lt;script src&gt;</c>) lo legge MdExplorer.
    /// </summary>
    [TestClass]
    public class TemplatePageExtractorShould
    {
        internal const string Dialect = @"schema: 1
id: pagine
name: Pagine
version: 1
extensions: [.page]
template:
  directive: '\{%\s*(?<word>\w+)(?:\((?<arg>[^)]*)\))?\s*%\}'
  include: [include]
  blocks:
    - { open: if, close: endif, else: [else], guard: true }
    - { open: switch, close: endswitch, cases: [case], guard: true }
    - { open: loop, close: endloop }
";

        private const string Orders = @"<html>
<head>
<script src=""../js/comune.js?v=3""></script>
<script>
function vai(azione) {
  invia('ordini.' + azione);
  invia('ordini.elenco');
}
var apri = function(x) { mostra(""popup.dettaglio""); };
</script>
</head>
<body>
{%include(parti/testata)%}
{%if(RUOLO==ADMIN)%}
<button id=""cancella"" onclick=""vai('cancella'); conferma('ordini.cancella')"">Cancella</button>
{%include(parti/admin)%}
{%else%}
<a href=""javascript:apri('{%codice%}')"">Apri</a>
{%endif%}
<!-- <button id=""vecchio"" onclick=""vecchio()"">x</button> -->
{%switch(STATO)%}{%case(A)%}<input type=""button"" name=""salva"" onclick=""this.form.invia('ordini.salva')"">{%endswitch%}
{%loop(righe)%}<span onclick=""dettaglio()"">{%nome%}</span>{%endloop%}
{%include(manca)%}
{%include({%x%})%}
{%sconosciuta(1)%}
</body>
</html>
";

        private string _root, _project;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-page-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            var package = Path.Combine(DeclarativeLanguage.FolderOf(_project), "pagine");
            Directory.CreateDirectory(package);
            File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Dialect);
            Write("web/pagine/ordini.page", Orders);
            Write("web/pagine/parti/testata.page", "<h1>{%titolo%}</h1>");
            Write("web/altro/parti/admin.page", "<p>admin</p>");
            Write("web/js/comune.js", "function invia(a) {}");
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_root, true); } catch (IOException) { }
        }

        private void Write(string logical, string text)
        {
            var path = Path.Combine(_project, logical);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
        }

        private TemplatePage Read(string logical = "web/pagine/ordini.page")
        {
            var catalog = CodeLanguageCatalog.ForProject(_project);
            Assert.AreEqual(0, catalog.Errors.Count, string.Join("\n", catalog.Errors.Values.Select(e => e.Message)));
            var extractor = new CodeExtractorRegistry(Array.Empty<ICodeExtractor>()).ForProject(catalog).Get("pagine");
            Assert.IsInstanceOfType(extractor, typeof(TemplatePageExtractor), "un pacchetto con template è un dialetto di pagine");
            var all = Directory.GetFiles(_project, "*.page", SearchOption.AllDirectories).OrderBy(f => f)
                .Select(f => new CodeSourceFile(f, Path.GetRelativePath(_project, f).Replace('\\', '/'))).ToList();
            return ((TemplatePageExtractor)extractor).Read(new CodeExtractionRequest { ProjectPath = _project, AllFiles = all, FilesToEmit = all.Where(f => f.LogicalPath == logical).ToList() }).Single();
        }

        private static ScriptSymbol Sym(CodeScript s, string qualifiedName) => s.Symbols.SingleOrDefault(x => x.QualifiedName == qualifiedName)
            ?? throw new AssertFailedException($"manca {qualifiedName}; ci sono: {string.Join(", ", s.Symbols.Select(x => x.QualifiedName))}");

        private static string Id(string kind, string qualifiedName) => RoslynCSharpExtractor.StableId("pagine", kind, qualifiedName);

        [TestMethod]
        public void Read_controls_with_their_guard_and_skip_the_commented_ones()
        {
            var page = Read();
            Assert.AreEqual(0, page.Warnings.Count, string.Join("\n", page.Warnings));
            var s = page.Script;
            Assert.AreEqual(("template", "pagine"), (s.Header.Extractor, s.Header.Language));
            var module = Sym(s, "web/pagine/ordini.page");
            Assert.AreEqual(("module", "ordini.page"), (module.Kind, module.Name));

            var cancella = Sym(s, "web/pagine/ordini.page:cancella");
            Assert.AreEqual(("control", "RUOLO==ADMIN", module.Id, 15), (cancella.Kind, cancella.Guard, cancella.ParentId, cancella.LineStart));
            StringAssert.StartsWith(cancella.Signature, "<button id=\"cancella\"");
            var link = s.Symbols.Single(x => x.Kind == "control" && x.Name == "a@18");
            Assert.AreEqual("!(RUOLO==ADMIN)", link.Guard, "il ramo «altrimenti» nega la condizione");
            Assert.AreEqual("STATO=A", Sym(s, "web/pagine/ordini.page:salva").Guard, "un caso è argomento=caso");
            Assert.IsNull(s.Symbols.Single(x => x.Name == "span@22").Guard, "un ciclo non è una condizione");
            Assert.IsFalse(s.Symbols.Any(x => x.Name == "vecchio"), "un controllo in un commento HTML non arriva al browser");
            Assert.AreEqual((4, 3), (page.Controls, page.GuardedControls));
            Assert.IsTrue(s.Edges.Any(e => e.SourceId == module.Id && e.TargetId == cancella.Id && e.Relation == "contains"));
        }

        [TestMethod]
        public void Record_the_calls_of_controls_and_scripts_and_link_the_functions_of_the_page()
        {
            var s = Read().Script;
            var vai = Sym(s, "web/pagine/ordini.page:vai");
            var apri = Sym(s, "web/pagine/ordini.page:apri");
            Assert.AreEqual(("function", 5, 8), (vai.Kind, vai.LineStart, vai.LineEnd));
            var cancella = Sym(s, "web/pagine/ordini.page:cancella").Id;

            var fromVai = s.CallSites.Where(c => c.SourceId == vai.Id).ToList();
            Assert.AreEqual("ordini.elenco", fromVai.Single().Arguments.Single().Value, "'ordini.' + azione non è una stringa letterale");
            Assert.AreEqual("popup.dettaglio", s.CallSites.Single(c => c.SourceId == apri.Id && c.Callee == "mostra").Arguments.Single().Value);

            var fromButton = s.CallSites.Where(c => c.SourceId == cancella).OrderBy(c => c.Ordinal).ToList();
            CollectionAssert.AreEqual(new[] { "vai:cancella", "conferma:ordini.cancella" }, fromButton.Select(c => c.Callee + ":" + c.Arguments.Single().Value).ToArray());
            var salva = s.CallSites.Single(c => c.Callee == "invia" && c.Receiver == "this.form");
            Assert.AreEqual("ordini.salva", salva.Arguments.Single().Value);

            var fromLink = s.CallSites.Single(c => c.Callee == "apri");
            Assert.AreEqual((1, 0), (fromLink.ArgumentCount, fromLink.Arguments.Count), "una stringa che scrive il server non è letterale, ma la chiamata del controllo resta");
            Assert.AreEqual(0, s.CallSites.Single(c => c.Callee == "dettaglio").ArgumentCount);

            Assert.IsTrue(s.Edges.Any(e => e.SourceId == cancella && e.TargetId == vai.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var link = s.Symbols.Single(x => x.Name == "a@18").Id;
            Assert.IsTrue(s.Edges.Any(e => e.SourceId == link && e.TargetId == apri.Id && e.Relation == "calls"), "la funzione è scritta prima nella pagina");
            Assert.IsFalse(s.Edges.Any(e => e.Relation == "calls" && e.TargetId != vai.Id && e.TargetId != apri.Id), "le funzioni di altri file le collegano le regole");
        }

        [TestMethod]
        public void Link_included_pages_and_imported_scripts()
        {
            var page = Read();
            var s = page.Script;
            var module = Id("module", "web/pagine/ordini.page");
            var includes = s.Edges.Where(e => e.Relation == "includes").ToList();
            var testata = includes.Single(e => e.TargetId == Id("module", "web/pagine/parti/testata.page"));
            Assert.AreEqual(("extracted", (string)null, 13), (testata.Confidence, testata.Guard, testata.Line), "relativo alla pagina, senza estensione");
            var admin = includes.Single(e => e.TargetId == Id("module", "web/altro/parti/admin.page"));
            Assert.AreEqual("RUOLO==ADMIN", admin.Guard, "trovata dal percorso che finisce così");
            var manca = s.Symbols.Single(x => x.Kind == "external" && x.Name == "manca");
            Assert.IsTrue(includes.Any(e => e.TargetId == manca.Id));
            Assert.AreEqual((4, 2, 1), (page.Includes, page.IncludesResolved, page.DynamicIncludes));
            CollectionAssert.AreEqual(new[] { "manca" }, page.UnresolvedIncludes);

            var js = s.Edges.Single(e => e.Relation == "imports");
            Assert.AreEqual((module, RoslynCSharpExtractor.StableId(CodeLanguages.JavaScript, "module", "web/js/comune")), (js.SourceId, js.TargetId),
                "lo stesso id che gli dà l'estrattore JS");
        }

        [TestMethod]
        public void Count_the_words_the_dialect_does_not_declare()
        {
            var page = Read();
            Assert.AreEqual(1, page.UnknownWords["sconosciuta"]);
            Assert.AreEqual(2, page.Placeholders, "{%codice%} e {%nome%}: valori che scrive il server");
            Assert.AreEqual(4, page.Words["include"]);
            Assert.AreEqual(1, page.Words["else"]);
        }

        [TestMethod]
        public void Warn_about_blocks_that_do_not_close_and_keep_reading()
        {
            Write("web/pagine/rotta.page", "{%if(A)%}\n<b onclick=\"f()\">x</b>\n{%endloop%}\n");
            var page = Read("web/pagine/rotta.page");
            Assert.AreEqual(2, page.Warnings.Count, string.Join("\n", page.Warnings));
            StringAssert.Contains(page.Warnings[0], "endloop");
            StringAssert.Contains(page.Warnings[1], ":1:");
            Assert.AreEqual("A", page.Script.Symbols.Single(x => x.Kind == "control").Guard);
        }

        [TestMethod]
        public void Keep_guards_through_the_turtle_script()
        {
            var s = Read().Script;
            var back = CodeScriptTurtle.Read(CodeScriptTurtle.Write(s), "web/pagine/ordini.page.ast.ttl");
            var original = s.Symbols.Single(x => x.Name == "cancella");
            Assert.AreEqual("RUOLO==ADMIN", back.Symbols.Single(x => x.Id == original.Id).Guard);
            Assert.AreEqual("RUOLO==ADMIN", back.Edges.Single(e => e.Relation == "includes" && e.Guard != null).Guard);
            Assert.AreEqual(s.CallSites.Count, back.CallSites.Count, "anche le chiamate senza stringhe");
        }

        [TestMethod]
        public void Refuse_a_dialect_that_mixes_line_rules_or_repeats_a_word()
        {
            static string Message(string yaml) => Assert.ThrowsException<LanguageDefinitionException>(() => DeclarativeLanguage.Parse(yaml, "/p/.mde-languages/pagine/language.yml")).Message;
            StringAssert.Contains(Message(Dialect + "blocks: []\n"), "blocks");
            StringAssert.Contains(Message(Dialect.Replace("include: [include]", "include: [if]")), "'if'");
            StringAssert.Contains(Message(Dialect.Replace("{ open: loop, close: endloop }", "{ open: loop, close: endloop, else: [altro] }")), "guard");
            StringAssert.Contains(Message(Dialect.Replace("(?<word>\\w+)", "(?<parola>\\w+)")), "word");
            StringAssert.Contains(Message(Dialect.Replace("  include: [include]\n", "  incluso: [x]\n")), "incluso");
        }
    }
}
