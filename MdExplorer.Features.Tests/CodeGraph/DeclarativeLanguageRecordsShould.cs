using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Motore 11 dei linguaggi dichiarativi (18/11/2026, rilevazione dal campo: una cartella di migliaia di file che sono
    /// tracciati di archivi, e nel grafo di ognuno c'era solo «il file esiste»). «Rec» è un linguaggio inventato con quella
    /// forma: un tracciato comincia con una riga e finisce dove comincia altro o con il file (<c>until</c>), ogni campo ha la
    /// sua spiegazione nel commento a fine riga, e il file dice in testa a che cosa serve (<c>comments</c>).
    /// </summary>
    [TestClass]
    public class DeclarativeLanguageRecordsShould
    {
        private const string Definition = @"schema: 1
id: rec
version: 1
extensions: [.rec, .prg]
ignoreCase: true
lexical:
  lineComment: '--'
  string: '""'
blocks:
  - id: proc
    kind: function
    begin: '^\s*PROC\s+(?<name>\w+)'
    end: '^\s*ENDPROC\b'
  - id: layout
    kind: record
    begin: '^\s*OVERLAY\s+(?<name>\w+)'
    until: '^\s*(?:OVERLAY|PROC)\b'
    in: [module, proc]
declarations:
  - id: column
    kind: field
    at: '^\s*VAR\s+(?<name>\w+)(?<type>[$%#])?(?:=(?<length>\d+))?'
    in: [layout]
  - id: variable
    at: '^\s*VAR\s+(?<name>\w+)'
    in: [module, proc]
    emit: false
types: { '$': string, '%': integer, '#': decimal }
comments:
  trailing: [column]
  leading: [layout]
  module: '^\[(?<tag>TABLE|DESC)\]\s*(?<text>.*)$'
references:
  - relation: references
    pattern: '^\s*USE\s+""(?<target>[^""]+)""'
    text: raw
    resolve: file
    unresolved: external
  - relation: calls
    pattern: '\b(?<target>[A-Za-z_]\w*)\s*\('
    resolve: symbol
    rule: proc
    unless: [variable, column]
";

        private const string Archive = @"-- [TABLE] CLIENTS
-- [DESC] Registry of the customers of the branch
-- written long ago
VAR BUFFER$=120

-- the record of a customer
-- one row per customer
OVERLAY BUFFER
VAR CODE$=8        -- key: customer code
VAR NAME$=40       -- name ""as printed""
VAR BALANCE#
VAR ROWS%=4

-- the short form, over the same buffer
OVERLAY SHORT
VAR KEY$=8

";

        private const string Program = @"USE ""clients.rec""
PROC MAIN
  VAR TOTAL#
  OVERLAY SCRATCH
  VAR PIECE$=4     -- a piece of the buffer
  X = ROWS(3) + PIECE(1)
ENDPROC
PROC OTHER
  MAIN()
ENDPROC
";

        private static Dictionary<string, CodeScript> Extract(out CodeExtractionResult result, string definition = Definition)
        {
            var root = Path.Combine(Path.GetTempPath(), "mde-records-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                File.WriteAllText(Path.Combine(root, "clients.rec"), Archive);
                File.WriteAllText(Path.Combine(root, "main.prg"), Program);
                var lang = DeclarativeLanguage.Parse(definition, "/p/.mde-languages/rec/language.yml");
                var all = new[] { new CodeSourceFile(Path.Combine(root, "clients.rec"), "data/clients.rec"), new CodeSourceFile(Path.Combine(root, "main.prg"), "src/main.prg") }.ToList();
                result = new DeclarativeLanguageExtractor(lang).Extract(new CodeExtractionRequest { ProjectPath = root, AllFiles = all, FilesToEmit = all });
                return result.Scripts.ToDictionary(s => s.Script.Header.Source, s => s.Script);
            }
            finally { try { Directory.Delete(root, true); } catch (IOException) { } }
        }

        private static ScriptSymbol Sym(CodeScript s, string qualified) => s.Symbols.SingleOrDefault(x => x.QualifiedName == qualified)
            ?? throw new AssertFailedException($"manca {qualified}; ci sono: {string.Join(", ", s.Symbols.Select(x => x.QualifiedName))}");

        [TestMethod]
        public void Make_a_record_with_its_fields_out_of_a_layout_that_ends_where_the_next_begins_or_with_the_file()
        {
            var archive = Extract(out var result)["data/clients.rec"];
            Assert.AreEqual(0, result.Warnings.Count, string.Join("\n", result.Warnings));

            var buffer = Sym(archive, "data/clients.rec:BUFFER");
            Assert.AreEqual("record", buffer.Kind);
            Assert.AreEqual(8, buffer.LineStart);
            Assert.AreEqual(12, buffer.LineEnd, "finisce all'ultimo campo, non alle righe vuote e di commento prima del tracciato dopo");
            var module = archive.Symbols.Single(x => x.Kind == "module");
            Assert.AreEqual(module.Id, buffer.ParentId);

            var fields = archive.Symbols.Where(x => x.ParentId == buffer.Id).OrderBy(x => x.LineStart).ToList();
            CollectionAssert.AreEqual(new[] { "CODE", "NAME", "BALANCE", "ROWS" }, fields.Select(f => f.Name).ToArray());
            Assert.IsTrue(fields.All(f => f.Kind == "field"));
            CollectionAssert.AreEqual(new[] { "string(8)", "string(40)", "decimal", "integer(4)" }, fields.Select(f => f.ReturnType).ToArray(), "il tipo porta la lunghezza");
            Assert.AreEqual("data/clients.rec:BUFFER.CODE", fields[0].QualifiedName);
            Assert.AreEqual(4, archive.Edges.Count(e => e.SourceId == buffer.Id && e.Relation == "contains"));

            // il secondo tracciato comincia dove finisce il primo e finisce con il file, senza avvisi
            var shortForm = Sym(archive, "data/clients.rec:SHORT");
            Assert.AreEqual(15, shortForm.LineStart);
            Assert.AreEqual(16, shortForm.LineEnd);
            Assert.AreEqual("KEY", archive.Symbols.Single(x => x.ParentId == shortForm.Id).Name);

            // la variabile dichiarata fuori da un tracciato resta un indice, non un simbolo
            Assert.IsFalse(archive.Symbols.Any(x => x.QualifiedName == "data/clients.rec:BUFFER$" || (x.Name == "BUFFER" && x.Kind == "field")));
        }

        [TestMethod]
        public void Carry_the_comments_of_the_source_as_written_file_header_lines_above_and_end_of_line()
        {
            var archive = Extract(out _)["data/clients.rec"];

            var module = archive.Symbols.Single(x => x.Kind == "module");
            Assert.AreEqual("TABLE: CLIENTS\nDESC: Registry of the customers of the branch", module.Comment, "solo le righe che la regola riconosce, con il loro tag");

            Assert.AreEqual("the record of a customer\none row per customer", Sym(archive, "data/clients.rec:BUFFER").Comment);
            Assert.AreEqual("the short form, over the same buffer", Sym(archive, "data/clients.rec:SHORT").Comment);

            Assert.AreEqual("key: customer code", Sym(archive, "data/clients.rec:BUFFER.CODE").Comment);
            Assert.AreEqual("name \"as printed\"", Sym(archive, "data/clients.rec:BUFFER.NAME").Comment, "le virgolette dentro un commento non aprono una stringa");
            Assert.IsNull(Sym(archive, "data/clients.rec:BUFFER.BALANCE").Comment);
            Assert.IsNull(Sym(archive, "data/clients.rec:SHORT.KEY").Comment);
        }

        [TestMethod]
        public void Take_the_header_of_the_file_as_its_comment_when_the_language_says_so()
        {
            var withHeader = Definition.Replace("  leading: [layout]", "  leading: [layout]\n  header: true");
            var scripts = Extract(out _, withHeader);

            // tutta l'intestazione, com'è scritta; le righe riconosciute tengono il loro tag e non si ripetono
            Assert.AreEqual("TABLE: CLIENTS\nDESC: Registry of the customers of the branch\nwritten long ago", scripts["data/clients.rec"].Symbols.Single(x => x.Kind == "module").Comment);
            // un file che comincia con il codice non ha intestazione
            Assert.IsNull(scripts["src/main.prg"].Symbols.Single(x => x.Kind == "module").Comment);
        }

        [TestMethod]
        public void Close_a_layout_left_open_when_the_block_around_it_ends()
        {
            var program = Extract(out var result)["src/main.prg"];
            Assert.AreEqual(0, result.Warnings.Count, string.Join("\n", result.Warnings));

            var main = Sym(program, "src/main.prg:MAIN");
            Assert.AreEqual(7, main.LineEnd, "ENDPROC chiude MAIN anche con il tracciato ancora aperto");
            var scratch = Sym(program, "src/main.prg:MAIN.SCRATCH");
            Assert.AreEqual(4, scratch.LineStart);
            Assert.AreEqual(6, scratch.LineEnd);
            Assert.AreEqual("a piece of the buffer", Sym(program, "src/main.prg:MAIN.SCRATCH.PIECE").Comment);

            // PROC dopo ENDPROC si apre nel file, non dentro il tracciato
            var other = Sym(program, "src/main.prg:OTHER");
            Assert.AreEqual(program.Symbols.Single(x => x.Kind == "module").Id, other.ParentId);
            Assert.IsTrue(program.Edges.Any(e => e.SourceId == other.Id && e.TargetId == main.Id && e.Relation == "calls"));
        }

        [TestMethod]
        public void Link_the_program_that_uses_an_archive_to_the_archive_and_not_take_a_field_for_a_call()
        {
            var scripts = Extract(out _);
            var program = scripts["src/main.prg"];
            var archiveModule = scripts["data/clients.rec"].Symbols.Single(x => x.Kind == "module");

            var use = program.Edges.Single(e => e.Relation == "references");
            Assert.AreEqual(archiveModule.Id, use.TargetId, "il programma arriva all'archivio, che sta in un'altra cartella");
            Assert.AreEqual(program.Symbols.Single(x => x.Kind == "module").Id, use.SourceId);

            // ROWS(3) e PIECE(1) sono campi con un indice, non chiamate
            Assert.IsFalse(program.Symbols.Any(x => x.Kind == "external"), string.Join(", ", program.Symbols.Where(x => x.Kind == "external").Select(x => x.QualifiedName)));
        }

        /// <summary>Una cartella analizzata da sola: i file che non sono suoi non li vede.</summary>
        private static List<(string Path, CodeScript Script)> AnalyseAlone(string name, string logical, string text)
        {
            var root = Path.Combine(Path.GetTempPath(), "mde-records-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                File.WriteAllText(Path.Combine(root, name), text);
                var lang = DeclarativeLanguage.Parse(Definition, "/p/.mde-languages/rec/language.yml");
                var all = new[] { new CodeSourceFile(Path.Combine(root, name), logical) }.ToList();
                var result = new DeclarativeLanguageExtractor(lang).Extract(new CodeExtractionRequest { ProjectPath = root, AllFiles = all, FilesToEmit = all });
                return result.Scripts.Select(x => (x.File.LogicalPath, x.Script)).ToList();
            }
            finally { try { Directory.Delete(root, true); } catch (IOException) { } }
        }

        [TestMethod]
        public void Link_a_program_to_the_archive_analysed_in_another_folder_whichever_comes_first()
        {
            // il programma prima, da solo: l'archivio è un nodo esterno
            var programs = AnalyseAlone("main.prg", "@programs/src/main.prg", Program);
            CodeCrossLinker.LinkAll("rec", true, programs, Array.Empty<(string, CodeScript)>());
            var program = programs.Single().Script;
            var use = program.Edges.Single(e => e.Relation == "references");
            Assert.AreEqual("external", program.Symbols.Single(x => x.Id == use.TargetId).Kind);

            // poi la cartella degli archivi: il programma già analizzato si ricollega
            var archives = AnalyseAlone("clients.rec", "@archives/data/clients.rec", Archive);
            var changed = CodeCrossLinker.LinkAll("rec", true, archives, programs);
            CollectionAssert.AreEqual(new[] { "@programs/src/main.prg" }, changed);
            var archiveModule = archives.Single().Script.Symbols.Single(x => x.Kind == "module");
            Assert.AreEqual(archiveModule.Id, program.Edges.Single(e => e.Relation == "references").TargetId, "il programma arriva all'archivio dell'altra cartella");
            Assert.IsFalse(program.Symbols.Any(x => x.Kind == "external" && x.Name.Contains("clients", StringComparison.OrdinalIgnoreCase)), "l'esterno ricollegato si toglie");

            // nell'ordine opposto: gli archivi ci sono già quando arriva il programma
            var later = AnalyseAlone("main.prg", "@programs/src/main.prg", Program);
            CodeCrossLinker.LinkAll("rec", true, later, archives);
            Assert.AreEqual(archiveModule.Id, later.Single().Script.Edges.Single(e => e.Relation == "references").TargetId);
        }

        [TestMethod]
        public void Keep_the_comment_through_the_script_on_disk()
        {
            var archive = Extract(out _)["data/clients.rec"];
            var back = CodeScriptParser.Parse(CodeScriptWriter.Serialize(archive), "clients.rec.ast.ttl");

            Assert.AreEqual("key: customer code", back.Symbols.Single(x => x.QualifiedName == "data/clients.rec:BUFFER.CODE").Comment);
            Assert.AreEqual("TABLE: CLIENTS\nDESC: Registry of the customers of the branch", back.Symbols.Single(x => x.Kind == "module").Comment);
            Assert.IsNull(back.Symbols.Single(x => x.QualifiedName == "data/clients.rec:BUFFER.BALANCE").Comment);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task Bring_the_comment_into_the_graph_where_whoever_describes_the_symbol_finds_it()
        {
            var store = FusekiFixture.NewStore();   // Inconclusive se Java 21 o il jar mancano
            var archive = Extract(out _)["data/clients.rec"];
            var project = Path.Combine(Path.GetTempPath(), "mde-records-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(project, ".mde-code", "data"));
            try
            {
                File.WriteAllText(Path.Combine(project, ".mde-code", "data", "clients.rec.ast.ttl"), CodeScriptWriter.Serialize(archive));
                var outcome = await store.LoadProjectAsync(project, compact: false);
                Assert.AreEqual(0, outcome.Failed, outcome.FirstError);

                var field = await store.DescribeAsync(project, archive.Symbols.Single(x => x.QualifiedName == "data/clients.rec:BUFFER.CODE").Id);
                Assert.AreEqual("field", field.Kind);
                Assert.AreEqual("key: customer code", field.Comment);
                Assert.AreEqual("string(8)", field.ReturnType);
                var file = await store.DescribeAsync(project, archive.Symbols.Single(x => x.Kind == "module").Id);
                Assert.AreEqual("TABLE: CLIENTS\nDESC: Registry of the customers of the branch", file.Comment);
                Assert.IsNull((await store.DescribeAsync(project, archive.Symbols.Single(x => x.QualifiedName == "data/clients.rec:BUFFER.BALANCE").Id)).Comment);
            }
            finally { try { Directory.Delete(project, true); } catch (IOException) { } }
        }

        [TestMethod]
        public void Refuse_a_definition_that_gets_until_or_comments_wrong()
        {
            static string Why(string yaml)
            {
                try { DeclarativeLanguage.Parse(yaml, "/p/.mde-languages/rec/language.yml"); return null; }
                catch (LanguageDefinitionException ex) { return ex.Message; }
            }

            StringAssert.Contains(Why(Definition.Replace("    until: '^\\s*(?:OVERLAY|PROC)\\b'", "    until: '^\\s*(?:OVERLAY|PROC)\\b'\n    end: '^\\s*END\\b'")), "blocks[layout]");
            StringAssert.Contains(Why(Definition.Replace("    until: '^\\s*(?:OVERLAY|PROC)\\b'\n", "")), "blocks[layout]");
            StringAssert.Contains(Why(Definition.Replace("  trailing: [column]", "  trailing: [variable]")), "comments.trailing");
            StringAssert.Contains(Why(Definition.Replace("  leading: [layout]", "  leading: [nothing]")), "comments.leading");
            StringAssert.Contains(Why(Definition.Replace("(?<tag>TABLE|DESC)\\]\\s*(?<text>.*)$", "(?<tag>TABLE|DESC)\\]")), "comments.module");
            StringAssert.Contains(Why(Definition.Replace("  lineComment: '--'\n", "")), "comments");
            Assert.IsNull(Why(Definition));
        }
    }
}
