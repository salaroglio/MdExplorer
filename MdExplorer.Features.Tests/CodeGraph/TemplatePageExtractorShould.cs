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

        internal const string Razor = @"schema: 1
id: razor
name: Viste Razor
version: 1
extensions: [.cshtml]
template:
  dynamic: '@[A-Za-z(]'
  events: ['^on.+$', '^\(.+\)$']
  recognizers:
    - { role: include, match: '@await\s+Html\.PartialAsync\(""(?<arg>[^""]+)""\)' }
    - { role: open, match: '@if\s*\((?<arg>.+?)\)\s*\{', close: brace, guard: true }
    - { role: else, match: '\belse\s*\{', close: brace }
    - { id: tag-helper, role: call, match: 'asp-controller=""(?<a0>[^""]+)""\s+asp-action=""(?<a1>[^""]+)""', callee: asp-action }
    - { role: call, match: 'Html\.ActionLink\(""[^""]*"",\s*""(?<a1>[^""]+)"",\s*""(?<a2>[^""]+)""\)', callee: ActionLink }
";

        private const string OrdersView = @"@model Ordini
@await Html.PartialAsync(""_Testata"")
@if (User.IsInRole(""Admin"")) {
    <a asp-controller=""Ordini"" asp-action=""Cancella"" id=""cancella"">Cancella</a>
    <button id=""vai"" onclick=""vai('@Model.Id')"">x</button>
} else {
    <span (click)=""apri()"">y</span>
}
@Html.ActionLink(""Elenco"", ""Elenco"", ""Ordini"")
";

        private TemplatePage ReadRazor(string view)
        {
            var package = Path.Combine(DeclarativeLanguage.FolderOf(_project), "razor");
            Directory.CreateDirectory(package);
            File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Razor);
            Write("Views/Ordini/Index.cshtml", view);
            Write("Views/Shared/_Testata.cshtml", "<header>@User.Identity.Name</header>");
            var catalog = CodeLanguageCatalog.ForProject(_project);
            Assert.AreEqual(0, catalog.Errors.Count, string.Join("\n", catalog.Errors.Values.Select(e => e.Message)));
            var extractor = (TemplatePageExtractor)new CodeExtractorRegistry(Array.Empty<ICodeExtractor>()).ForProject(catalog).Get("razor");
            var all = Directory.GetFiles(_project, "*.cshtml", SearchOption.AllDirectories).OrderBy(f => f)
                .Select(f => new CodeSourceFile(f, Path.GetRelativePath(_project, f).Replace('\\', '/'))).ToList();
            return extractor.Read(new CodeExtractionRequest { ProjectPath = _project, AllFiles = all, FilesToEmit = all.Where(f => f.LogicalPath == "Views/Ordini/Index.cshtml").ToList() }).Single();
        }

        [TestMethod]
        public void Read_a_razor_view_with_recognizers_closing_blocks_at_the_brace()
        {
            var page = ReadRazor(OrdersView);
            Assert.AreEqual(0, page.Warnings.Count, string.Join("\n", page.Warnings));
            var s = page.Script;
            var admin = "User.IsInRole(\"Admin\")";

            var cancella = s.Symbols.Single(x => x.Name == "cancella");
            Assert.AreEqual(("control", admin), (cancella.Kind, cancella.Guard), "una chiamata riconosciuta dentro un tag fa dell'elemento un controllo");
            var helper = s.CallSites.Single(c => c.Callee == "asp-action");
            Assert.AreEqual((cancella.Id, 2), (helper.SourceId, helper.ArgumentCount));
            CollectionAssert.AreEqual(new[] { "Ordini", "Cancella" }, helper.Arguments.Select(a => a.Value).ToArray());

            Assert.AreEqual(admin, s.Symbols.Single(x => x.Name == "vai").Guard);
            Assert.AreEqual(0, s.CallSites.Single(c => c.Callee == "vai").Arguments.Count, "'@Model.Id' la scrive il server");
            Assert.AreEqual("!(" + admin + ")", s.Symbols.Single(x => x.Name == "span@7").Guard, "else a graffa = la negazione del blocco appena chiuso; (click) è un evento");
            Assert.IsTrue(s.CallSites.Any(c => c.Callee == "apri"));

            var link = s.CallSites.Single(c => c.Callee == "ActionLink");
            Assert.AreEqual((s.Symbols.Single(x => x.Kind == "module").Id, 3), (link.SourceId, link.ArgumentCount), "fuori da un tag la chiama la pagina");
            CollectionAssert.AreEqual(new[] { 1, 2 }, link.Arguments.Select(a => a.Position).ToArray());

            var include = s.Edges.Single(e => e.Relation == "includes");
            Assert.AreEqual((RoslynCSharpExtractor.StableId("razor", "module", "Views/Shared/_Testata.cshtml"), (string)null), (include.TargetId, include.Guard));
            Assert.AreEqual(1, page.Words["tag-helper"], "quante volte ogni riconoscitore ha trovato qualcosa");
        }

        internal const string Angular = @"schema: 1
id: angular
name: Template Angular
version: 1
extensions: [.html]
template:
  dynamic: '\{\{'
  events: ['^\(.+\)$']
  recognizers:
    - { role: open, match: '\*ngIf=""(?<arg>[^"";]+)(?:;\s*then\s+(?<then>\w+))?(?:;?\s*else\s+(?<else>\w+))?\s*""', close: element, guard: true }
    - { role: named, match: '<ng-template\b[^>]*?\#(?<name>\w+)', close: element }
    - { role: open, match: '\*ngFor=""[^""]*""', close: element }
    - { role: open, match: '@if\s*\((?<arg>.+?)\)\s*\{', close: brace, guard: true }
    - { role: else, match: '@else\s*\{', close: brace }
    - { role: call, match: '<(?<a0>app-[\w-]+)', callee: selector }
  inline: { extensions: [.ts], match: 'template\s*:\s*`(?<body>[^`]*)`' }
";

        [TestMethod]
        public void Read_an_angular_template_closing_ngif_at_the_end_of_the_element()
        {
            var package = Path.Combine(DeclarativeLanguage.FolderOf(_project), "angular");
            Directory.CreateDirectory(package);
            File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Angular);
            Write("src/app/x.component.html",
                "<div *ngIf=\"isAdmin\" class=\"bar\">\n  <button (click)=\"salva()\">ok</button>\n  <div><span>annidato</span></div>\n</div>\n" +
                "<button id=\"fuori\" (click)=\"annulla()\">no</button>\n<input *ngIf=\"aperto\" (keyup)=\"cerca()\">\n" +
                "<ul><li *ngFor=\"let r of righe\" (click)=\"apri(r)\">{{r.nome}}</li></ul>\n@if (pronto) {\n<app-dettaglio [id]=\"x\"></app-dettaglio>\n} @else {\n<p>attendi</p>\n}\n");
            var catalog = CodeLanguageCatalog.ForProject(_project);
            var extractor = (TemplatePageExtractor)new CodeExtractorRegistry(Array.Empty<ICodeExtractor>()).ForProject(catalog).Get("angular");
            var file = new CodeSourceFile(Path.Combine(_project, "src", "app", "x.component.html"), "src/app/x.component.html");
            var page = extractor.Read(new CodeExtractionRequest { ProjectPath = _project, AllFiles = new[] { file }, FilesToEmit = new[] { file } }).Single();
            Assert.AreEqual(0, page.Warnings.Count, string.Join("\n", page.Warnings));
            var s = page.Script;

            Assert.AreEqual("isAdmin", s.Symbols.Single(x => x.Name == "button@2").Guard, "dentro l'elemento con *ngIf");
            Assert.IsNull(s.Symbols.Single(x => x.Name == "fuori").Guard, "dopo la chiusura dell'elemento (con un div annidato dentro)");
            Assert.AreEqual("aperto", s.Symbols.Single(x => x.Name == "input@6").Guard, "un elemento vuoto: la condizione vale per lui");
            Assert.IsNull(s.Symbols.Single(x => x.Name == "li@7").Guard, "*ngFor non è una condizione");
            var child = s.Symbols.Single(x => x.Name == "app-dettaglio@9");
            Assert.AreEqual(("control", "pronto"), (child.Kind, child.Guard));
            Assert.AreEqual("app-dettaglio", s.CallSites.Single(c => c.Callee == "selector").Arguments.Single().Value);
            CollectionAssert.IsSubsetOf(new[] { "salva", "annulla", "cerca", "apri" }, s.CallSites.Select(c => c.Callee).ToArray());
        }

        [TestMethod]
        public void Give_an_ng_template_the_condition_of_the_ngif_that_names_it()
        {
            var package = Path.Combine(DeclarativeLanguage.FolderOf(_project), "angular");
            Directory.CreateDirectory(package);
            File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Angular);
            Write("src/app/y.component.html",
                "<div *ngIf=\"caricato; else attesa\">\n  <button id=\"salva\" (click)=\"salva()\">ok</button>\n</div>\n" +
                "<section *ngIf=\"admin\">\n<ng-template #attesa>\n  <button id=\"ricarica\" (click)=\"ricarica()\">r</button>\n</ng-template>\n</section>\n" +
                "<p *ngIf=\"x; then si else no\"></p>\n" +
                "<ng-template #si><button id=\"b1\" (click)=\"a()\"></button></ng-template>\n" +
                "<ng-template #no><button id=\"b2\" (click)=\"b()\"></button></ng-template>\n" +
                "<ng-template #mai><button id=\"b3\" (click)=\"c()\"></button></ng-template>\n" +
                "<ng-template #dopo><button id=\"b4\" (click)=\"d()\"></button></ng-template>\n" +
                "<div *ngIf=\"utente\"><span *ngIf=\"pronto; else dopo\"></span></div>\n");
            var catalog = CodeLanguageCatalog.ForProject(_project);
            var extractor = (TemplatePageExtractor)new CodeExtractorRegistry(Array.Empty<ICodeExtractor>()).ForProject(catalog).Get("angular");
            var file = new CodeSourceFile(Path.Combine(_project, "src", "app", "y.component.html"), "src/app/y.component.html");
            var page = extractor.Read(new CodeExtractionRequest { ProjectPath = _project, AllFiles = new[] { file }, FilesToEmit = new[] { file } }).Single();
            Assert.AreEqual(0, page.Warnings.Count, string.Join("\n", page.Warnings));
            string Guard(string name) => page.Script.Symbols.Single(x => x.Name == name).Guard;

            Assert.AreEqual("caricato", Guard("salva"));
            Assert.AreEqual("admin && !(caricato)", Guard("ricarica"), "il ramo else nominato più in alto, dentro un'altra condizione");
            Assert.AreEqual("x", Guard("b1"), "then");
            Assert.AreEqual("!(x)", Guard("b2"), "else dopo then");
            Assert.IsNull(Guard("b3"), "un ng-template che nessuno nomina");
            Assert.AreEqual("utente && !(pronto)", Guard("b4"), "nominato più in basso, con la condizione che il nome ha intorno");
        }

        [TestMethod]
        public void Read_an_inline_template_as_a_page_inside_the_ts_file_with_its_lines()
        {
            var package = Path.Combine(DeclarativeLanguage.FolderOf(_project), "angular");
            Directory.CreateDirectory(package);
            File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Angular);
            Write("src/app/z.component.ts",
                "import { Component } from '@angular/core';\n" +
                "const finto = '<b id=\"fuori\" (click)=\"no()\">';\n" +
                "@Component({ selector: 'app-z', template: `\n<div *ngIf=\"admin\">\n  <button id=\"ok\" (click)=\"salva()\">ok</button>\n</div>\n<app-dettaglio></app-dettaglio>\n` })\n" +
                "export class ZComponent { salva(): void { } }\n");
            Write("src/app/w.component.ts", "@Component({ selector: 'app-w', templateUrl: './w.component.html' })\nexport class WComponent { }\n");
            var catalog = CodeLanguageCatalog.ForProject(_project);
            var ts = new[] { "z", "w" }.Select(n => new CodeSourceFile(Path.Combine(_project, "src", "app", n + ".component.ts"), $"src/app/{n}.component.ts")).ToList();

            var files = CodeInlinePages.Expand(catalog, ts);
            var inline = files.Single(f => CodeInlinePages.IsInline(f.LogicalPath));
            Assert.AreEqual(("src/app/z.component.ts#template", "angular", "typescript"), (inline.LogicalPath, catalog.LanguageOf(inline), catalog.LanguageOf(ts[0])), "solo il .ts che ha un template inline");

            var extractor = (TemplatePageExtractor)new CodeExtractorRegistry(Array.Empty<ICodeExtractor>()).ForProject(catalog).Get("angular");
            var page = extractor.Read(new CodeExtractionRequest { ProjectPath = _project, AllFiles = new[] { inline }, FilesToEmit = new[] { inline } }).Single();
            Assert.AreEqual(0, page.Warnings.Count, string.Join("\n", page.Warnings));
            var s = page.Script;
            Assert.AreEqual("src/app/z.component.ts#template", s.Header.Source);
            var ok = s.Symbols.Single(x => x.Name == "ok");
            Assert.AreEqual(("admin", 5), (ok.Guard, ok.LineStart), "la riga è quella del .ts");
            Assert.IsFalse(s.Symbols.Any(x => x.Name == "fuori"), "il codice TypeScript fuori dal template non è pagina");
            Assert.AreEqual("app-dettaglio", s.CallSites.Single(c => c.Callee == "selector").Arguments.Single().Value);
            Assert.AreEqual(Path.Combine(_project, "src", "app", "z.component.ts"), new CodeRootResolver(_project, null).Resolve(inline.LogicalPath), "il sorgente di una pagina inline è il .ts");
        }

        [TestMethod]
        public void Refuse_an_inline_extension_already_read_by_another_dialect()
        {
            foreach (var id in new[] { "angular", "angular2" })
            {
                var package = Path.Combine(DeclarativeLanguage.FolderOf(_project), id);
                Directory.CreateDirectory(package);
                File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Angular.Replace("id: angular", "id: " + id).Replace("[.html]", id == "angular" ? "[.html]" : "[.htm]"));
            }
            var catalog = CodeLanguageCatalog.ForProject(_project);
            Assert.IsNotNull(catalog.ById("angular"));
            StringAssert.Contains(catalog.Errors["angular2"].Message, ".ts");

            var own = Path.Combine(DeclarativeLanguage.FolderOf(_project), "angular2");
            File.WriteAllText(Path.Combine(own, LanguagePackage.DefinitionFile), Angular.Replace("id: angular", "id: angular2").Replace("[.html]", "[.htm]").Replace("extensions: [.ts]", "extensions: [.htm]"));
            StringAssert.Contains(Assert.ThrowsException<LanguageDefinitionException>(() => LanguagePackage.Load(own)).Message, "template.inline");
        }

        [TestMethod]
        public void Refuse_then_and_else_groups_without_a_named_block()
        {
            var package = Path.Combine(DeclarativeLanguage.FolderOf(_project), "angular");
            Directory.CreateDirectory(package);
            File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), Angular.Replace("    - { role: named,", "    #"));
            var ex = Assert.ThrowsException<LanguageDefinitionException>(() => LanguagePackage.Load(package));
            StringAssert.Contains(ex.Message, "named");
        }

        [TestMethod]
        public void Warn_when_a_brace_never_closes()
        {
            var page = ReadRazor("@if (A) {\n<b onclick=\"f()\">x</b>\n");
            StringAssert.Contains(page.Warnings.Single(), "open#1");
            Assert.IsNull(page.Script.Symbols.Single(x => x.Kind == "control").Guard, "senza la graffa che chiude il blocco non si apre");
        }

        [TestMethod]
        public void Refuse_recognizers_without_what_their_role_needs()
        {
            static string Message(string yaml)
            {
                try { DeclarativeLanguage.Parse(yaml, "/p/.mde-languages/razor/language.yml"); }
                catch (LanguageDefinitionException ex) { return ex.Message; }
                throw new AssertFailedException("accettato:\n" + yaml);
            }
            StringAssert.Contains(Message(Razor.Replace(", close: brace, guard: true }", ", guard: true }")), "close");
            StringAssert.Contains(Message(Razor.Replace("close: brace, guard: true", "close: fine, guard: true")), "fine");
            StringAssert.Contains(Message(Razor.Replace(", callee: ActionLink }", " }")), "callee");
            StringAssert.Contains(Message(Razor.Replace("(?<arg>[^\"]+)\"\\)' }", "[^\"]+\"\\)' }")), "arg");
            StringAssert.Contains(Message(Razor.Replace("role: include", "role: inclusione")), "inclusione");
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
