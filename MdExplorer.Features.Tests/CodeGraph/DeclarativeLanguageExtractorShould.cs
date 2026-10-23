using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// I linguaggi definiti nel progetto: la definizione in <c>.mde-languages/&lt;id&gt;.lang.yml</c> letta in modo rigido, il
    /// catalogo del progetto, e l'estrattore generico su «Toy», un linguaggio a righe inventato per i test — blocchi,
    /// etichette locali al blocco, parti chiamate per numero, include testuali, callback per nome in una stringa, commenti
    /// e stringhe che non aprono niente, continuazione di riga.
    /// </summary>
    [TestClass]
    public class DeclarativeLanguageExtractorShould
    {
        internal const string ToyDefinition = @"schema: 1
id: toy
name: Toy
version: 1
extensions: [.toy, .toh, .tor, .tos]
ignoreCase: true
lexical:
  lineComment: '--'
  string: '""'
  continuation: '\s_\s*$'
module:
  name: '^\s*MODULE\s+""(?<name>[^""]+)""'
blocks:
  - id: proc
    kind: function
    begin: '^\s*PROC\s+(?<name>\w+)(?<type>[$%#&])?(?:\s*=\s*(?<length>\w+))?\s*\((?<params>.*)\)'
    end: '^\s*ENDPROC\b'
  - id: part
    kind: function
    begin: '^\s*PART\s+(?<name>\d+)'
    end: '^\s*ENDPART\b'
    name: 'PART{name}'
    signature: 'PART {name}'
labels:
  - id: label
    kind: function
    at: '^\s*@(?<name>\w+)\b'
    in: [part, proc]
    name: '@{name}'
    onlyIfReferenced: true
parameters:
  item: '(?:(?<direction>OUT|IN)\s+)?(?<name>[A-Za-z_]\w*)(?<type>[$%#&])?(?:\s*=\s*(?<length>\w+))?'
  direction: { OUT: ref, IN: in }
  default: in
types: { '$': string, '%': integer, '#': decimal, '&': long }
references:
  - relation: references
    pattern: '^\s*USE\s+""(?<target>[^"";]+)'
    text: raw
    after: '^\s*MAP\b'
    strip: '^\[[^\]]*\]'
    resolve: file
    unresolved: external
  - relation: imports
    pattern: '^\s*(?:USE|LIB)\s+""(?<target>[^"";]+)'
    text: raw
    notAfter: '^\s*MAP\b'
    strip: '^\[[^\]]*\]'
    resolve: file
    unresolved: external
  - relation: calls
    pattern: '\bCALL\s+@(?<target>\w+)'
    resolve: symbol
    rule: label
    scope: block
  - relation: calls
    pattern: '\bRUN\s+PART\s+(?<target>\d+)'
    resolve: symbol
    rule: part
    scope: module
  - relation: calls
    pattern: '\b(?<target>[A-Za-z_]\w*)[$%#&]?\s*\('
    resolve: symbol
    rule: proc
  - relation: calls
    pattern: '""(?<target>[A-Za-z_]\w*)[$%#&]?""'
    text: raw
    resolve: symbol
    rule: proc
    confidence: inferred
";

        /// <summary>Toy con tutto: macro, costanti e variabili come indice, librerie per nome di modulo, parole del linguaggio ignorate.</summary>
        internal const string ToyFullDefinition = @"schema: 1
id: toy
name: Toy
version: 1
extensions: [.toy, .toh, .tor, .tos]
ignoreCase: true
lexical:
  lineComment: '--'
  string: '""'
module:
  name: '^\s*MODULE\s+""(?<name>[^""]+)""'
blocks:
  - id: proc
    kind: function
    begin: '^\s*PROC\s+(?<name>\w+)(?<type>[$%#&])?(?:\s*=\s*(?<length>\w+))?\s*\((?<params>.*)\)'
    end: '^\s*ENDPROC\b'
  - id: part
    kind: function
    begin: '^\s*PART\s+(?<name>\d+)'
    end: '^\s*ENDPART\b'
    name: 'PART{name}'
declarations:
  - id: macro
    kind: function
    at: '^\s*#macro\s+(?<name>\w+)\((?<params>[^)]*)\)'
  - id: constant
    at: '^\s*#const\s+(?<name>\w+)\b(?!\()'
    emit: false
  - id: variable
    at: '^\s*VAR\s+(?<list>.*)'
    each: '(?:^|,)\s*(?<name>[A-Za-z_]\w*)'
    in: [module, proc, part]
    emit: false
parameters:
  item: '(?:(?<direction>OUT|IN)\s+)?(?<name>[A-Za-z_]\w*)(?<type>[$%#&])?(?:\s*=\s*(?<length>\w+))?'
  direction: { OUT: ref, IN: in }
  default: in
  sticky: true
types: { '$': string, '%': integer, '#': decimal, '&': long }
references:
  - relation: imports
    pattern: '^\s*USE\s+""(?<target>[^""]+)""'
    text: raw
    resolve: file
    unresolved: external
  - relation: imports
    pattern: '^\s*LIB\s+""(?<target>[^""]+)""'
    text: raw
    resolve: module
    unresolved: external
  - relation: calls
    pattern: '\b(?<target>[A-Za-z_]\w*)[$%#&]?\s*\('
    resolve: symbol
    rules: [proc, macro]
    unless: [variable, constant]
    unresolved: external
    ignore: [IF, AND, OR, NOT, PRINT, LEFT]
";

        private const string Demo = @"MODULE ""DEMO""
USE ""lib.toh""
LIB ""lexact""
VAR G%
MAP
USE ""[C_]REC.TOR""   -- tracciato

PROC SOMMA%(OUT A$=8, IN B#)
   SOMMA = HELPER(A) -- commento con CALL @FALSO e FINTA(
ENDPROC

PROC HELPER$=2(X$)
ENDPROC

PART 10
   CALL @DONE
   RUN PART 20
   PRINT ""CALL @NOPE"", SOMMA(1,2) + _
         LIBF(3)
   VOID(REPORT(X,""HELPER""))
@DONE
   RETURN
@SOLOJUMP
   JUMP @SOLOJUMP
ENDPART 10

PART 20
@DONE  PRINT ""x""
   CALL @DONE
ENDPART 20
END
";

        private string _root, _project, _src;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-decl-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "src");
            Directory.CreateDirectory(_src);
            Directory.CreateDirectory(DeclarativeLanguage.FolderOf(_project));
            File.WriteAllText(Path.Combine(DeclarativeLanguage.FolderOf(_project), "toy.lang.yml"), ToyDefinition);
            File.WriteAllText(Path.Combine(_src, "demo.toy"), Demo);
            File.WriteAllText(Path.Combine(_src, "lib.toh"), "PROC LIBF%(IN X%)\nENDPROC\n");
            File.WriteAllText(Path.Combine(_src, "REC.TOR"), "VAR R_KEY$=10\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_root, true); } catch (IOException) { }
        }

        private CodeScript Extract(string file = "demo.toy")
        {
            var catalog = CodeLanguageCatalog.ForProject(_project);
            Assert.AreEqual(0, catalog.Errors.Count, string.Join("\n", catalog.Errors.Values.Select(e => e.Message)));
            var lang = catalog.ForFile("x.TOY");
            Assert.IsNotNull(lang, "l'estensione si riconosce senza badare alle maiuscole");
            Assert.AreEqual(CodeExtractorKind.Declarative, lang.ExtractorKind);
            var all = Directory.GetFiles(_src).OrderBy(f => f).Select(f => new CodeSourceFile(f, "src/" + Path.GetFileName(f))).ToList();
            var extractor = new DeclarativeLanguageExtractor(lang.Definition);
            var result = extractor.Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = all, FilesToEmit = all.Where(f => f.LogicalPath == "src/" + file).ToList() });
            Assert.AreEqual(0, result.Warnings.Count, string.Join("\n", result.Warnings));
            return result.Scripts.Single().Script;
        }

        private static ScriptSymbol Sym(CodeScript s, string qualifiedName) => s.Symbols.SingleOrDefault(x => x.QualifiedName == qualifiedName)
            ?? throw new AssertFailedException($"manca {qualifiedName}; ci sono: {string.Join(", ", s.Symbols.Select(x => x.QualifiedName))}");

        private static ScriptEdge EdgeOf(CodeScript s, string from, string to, string relation)
            => s.Edges.SingleOrDefault(e => e.SourceId == from && e.TargetId == to && e.Relation == relation);

        [TestMethod]
        public void Declare_module_functions_with_types_and_parameters()
        {
            var s = Extract();
            var module = Sym(s, "src/demo.toy");
            Assert.AreEqual("module", module.Kind);
            Assert.AreEqual("DEMO", module.Name, "il nome viene da MODULE");
            Assert.AreEqual("toy", s.Header.Language);
            Assert.AreEqual("declarative", s.Header.Extractor);

            var somma = Sym(s, "src/demo.toy:SOMMA");
            Assert.AreEqual("function", somma.Kind);
            Assert.AreEqual("integer", somma.ReturnType);
            Assert.AreEqual(module.Id, somma.ParentId);
            Assert.AreEqual(8, somma.LineStart);
            Assert.AreEqual(10, somma.LineEnd);
            var ps = s.Parameters.Where(p => p.SymbolId == somma.Id).OrderBy(p => p.Position).ToList();
            Assert.AreEqual(2, ps.Count);
            Assert.AreEqual("A", ps[0].Name); Assert.AreEqual("string(8)", ps[0].ParameterType); Assert.AreEqual("ref", ps[0].Direction);
            Assert.AreEqual("B", ps[1].Name); Assert.AreEqual("decimal", ps[1].ParameterType); Assert.AreEqual("in", ps[1].Direction);

            Assert.AreEqual("string(2)", Sym(s, "src/demo.toy:HELPER").ReturnType);
            Assert.IsNotNull(EdgeOf(s, module.Id, somma.Id, "contains"));
        }

        [TestMethod]
        public void Keep_labels_local_to_their_block_and_only_when_called()
        {
            var s = Extract();
            var part10 = Sym(s, "src/demo.toy:PART10");
            var part20 = Sym(s, "src/demo.toy:PART20");
            Assert.AreEqual("PART 10", part10.Signature);
            var done10 = Sym(s, "src/demo.toy:PART10.@DONE");
            var done20 = Sym(s, "src/demo.toy:PART20.@DONE");
            Assert.AreNotEqual(done10.Id, done20.Id, "lo stesso nome in due blocchi sono due etichette");
            Assert.AreEqual(part10.Id, done10.ParentId);
            Assert.AreEqual(21, done10.LineStart);
            Assert.AreEqual(22, done10.LineEnd, "l'etichetta finisce dove comincia la successiva");
            Assert.IsFalse(s.Symbols.Any(x => x.Name == "@SOLOJUMP"), "un'etichetta raggiunta solo da JUMP non è un simbolo");

            Assert.AreEqual("extracted", EdgeOf(s, part10.Id, done10.Id, "calls")?.Confidence);
            Assert.IsNull(EdgeOf(s, part10.Id, done20.Id, "calls"), "CALL resta nel suo blocco");
            Assert.AreEqual("extracted", EdgeOf(s, part10.Id, part20.Id, "calls")?.Confidence, "RUN PART 20");
        }

        [TestMethod]
        public void Resolve_calls_across_files_strings_and_continued_lines()
        {
            var s = Extract();
            var part10 = Sym(s, "src/demo.toy:PART10");
            var somma = Sym(s, "src/demo.toy:SOMMA");
            var helper = Sym(s, "src/demo.toy:HELPER");
            var libf = RoslynCSharpExtractor.StableId("toy", "function", "src/lib.toh:LIBF");

            Assert.AreEqual("extracted", EdgeOf(s, part10.Id, somma.Id, "calls")?.Confidence);
            Assert.AreEqual("inferred", EdgeOf(s, part10.Id, libf, "calls")?.Confidence, "LIBF( sulla riga continuata, dichiarata in lib.toh");
            Assert.AreEqual(18, EdgeOf(s, part10.Id, libf, "calls").Line, "la riga logica comincia alla 18");
            Assert.AreEqual("inferred", EdgeOf(s, part10.Id, helper.Id, "calls")?.Confidence, "\"HELPER\" passato per nome = callback");
            Assert.AreEqual("extracted", EdgeOf(s, somma.Id, helper.Id, "calls")?.Confidence);
            Assert.IsFalse(s.Symbols.Any(x => x.Name.Contains("NOPE") || x.Name.Contains("FALSO") || x.Name.Contains("FINTA")),
                "niente dai commenti né dalle stringhe");
            Assert.IsFalse(s.Edges.Any(e => e.SourceId == somma.Id && e.TargetId == somma.Id), "nessun arco verso sé stessi");
        }

        [TestMethod]
        public void Tell_includes_from_record_layouts()
        {
            var s = Extract();
            var module = Sym(s, "src/demo.toy");
            var lib = RoslynCSharpExtractor.StableId("toy", "module", "src/lib.toh");
            var rec = RoslynCSharpExtractor.StableId("toy", "module", "src/REC.TOR");
            Assert.AreEqual("extracted", EdgeOf(s, module.Id, lib, "imports")?.Confidence);
            Assert.AreEqual("extracted", EdgeOf(s, module.Id, rec, "references")?.Confidence, "dopo MAP, col prefisso [C_] tolto");
            Assert.IsNull(EdgeOf(s, module.Id, rec, "imports"));
            var lexact = s.Symbols.Single(x => x.Kind == "external" && x.Name == "lexact");
            Assert.IsNotNull(EdgeOf(s, module.Id, lexact.Id, "imports"), "LIB di una libreria fuori dal perimetro");
        }

        [TestMethod]
        public void Change_version_when_a_rule_changes()
        {
            var before = new DeclarativeLanguageExtractor(DeclarativeLanguage.Parse(ToyDefinition, "toy.lang.yml")).ExtractorVersion;
            var after = new DeclarativeLanguageExtractor(DeclarativeLanguage.Parse(ToyDefinition.Replace("ENDPROC", "ENDPROCEDURE"), "toy.lang.yml")).ExtractorVersion;
            Assert.AreNotEqual(before, after, "una regola cambiata riscrive gli script anche senza alzare version");
            StringAssert.StartsWith(before, DeclarativeLanguageExtractor.EngineVersion + "-1.");
        }

        [DataTestMethod]
        [DataRow("blocchi: []\n", "blocchi")]
        [DataRow("blocks:\n  - id: f\n    kind: function\n    begin: '(unclosed'\n    end: 'E'\n", "blocks[f].begin")]
        [DataRow("blocks:\n  - id: f\n    kind: function\n    begin: 'PROC'\n    end: 'E'\n", "(?<name>")]
        [DataRow("blocks:\n  - id: f\n    kind: widget\n    begin: '(?<name>x)'\n    end: 'E'\n", "blocks[f].kind")]
        [DataRow("blocks:\n  - id: f\n    kind: fragment\n    begin: '(?<name>x)'\n    end: 'E'\n", "blocks[f].kind")]
        [DataRow("references:\n  - relation: calls\n    pattern: '(?<target>x)'\n    resolve: symbol\n    rule: nothing\n", "references[0].rule")]
        [DataRow("references:\n  - relation: contains\n    pattern: '(?<target>x)'\n    resolve: file\n", "references[0].relation")]
        public void Refuse_a_broken_definition_saying_where(string body, string expected)
        {
            var yaml = "schema: 1\nid: toy\nversion: 1\nextensions: [.toy]\n" + body;
            var ex = Assert.ThrowsException<LanguageDefinitionException>(() => DeclarativeLanguage.Parse(yaml, "/p/.mde-languages/toy.lang.yml"));
            StringAssert.Contains(ex.Message, expected);
            StringAssert.Contains(ex.Message, "toy.lang.yml");
        }

        [TestMethod]
        public void Refuse_an_id_different_from_the_file_name()
        {
            var ex = Assert.ThrowsException<LanguageDefinitionException>(() => DeclarativeLanguage.Parse(ToyDefinition, "/p/.mde-languages/altro.lang.yml"));
            StringAssert.Contains(ex.Message, "altro.lang.yml");
        }

        [TestMethod]
        public void Keep_a_broken_or_clashing_definition_out_of_the_catalog_with_its_reason()
        {
            var folder = DeclarativeLanguage.FolderOf(_project);
            File.WriteAllText(Path.Combine(folder, "rotto.lang.yml"), "schema: 1\nid: rotto\nversion: 1\nextensions: [.rt]\nnonEsiste: 1\n");
            File.WriteAllText(Path.Combine(folder, "doppio.lang.yml"), "schema: 1\nid: doppio\nversion: 1\nextensions: [.cs]\n");
            var catalog = CodeLanguageCatalog.ForProject(_project);

            Assert.IsNotNull(catalog.ById("toy"), "le definizioni buone restano");
            Assert.IsNull(catalog.ById("rotto"));
            Assert.IsNull(catalog.ForFile("a.rt"));
            Assert.AreEqual("csharp", catalog.ForFile("a.cs").Id, "un'estensione già presa non cambia padrone");
            StringAssert.Contains(catalog.Errors["doppio"].Message, ".cs");
            var ex = Assert.ThrowsException<InvalidOperationException>(() => catalog.Require("rotto"));
            StringAssert.Contains(ex.Message, "rotto.lang.yml");

            var registry = new CodeExtractorRegistry(Array.Empty<ICodeExtractor>()).ForProject(catalog);
            Assert.IsNotNull(registry.Get("toy"));
            StringAssert.Contains(registry.Availability("rotto"), "rotto.lang.yml");
            var detected = CodeLanguageDetector.Detect(new[] { "a.toy", "b.TOH", "c.cs" }, registry, catalog).Single(d => d.Id == "toy");
            Assert.AreEqual(2, detected.Files);
            Assert.AreEqual("ready", detected.Availability);
            Assert.AreEqual(".mde-languages/toy.lang.yml", detected.DefinitionPath);
        }

        /// <summary>Libreria per nome di modulo, macro, costanti e variabili come indice, parole del linguaggio ignorate, direzioni che valgono fino alla parola dopo.</summary>
        [TestMethod]
        public void Read_a_library_its_header_and_a_program()
        {
            File.WriteAllText(Path.Combine(DeclarativeLanguage.FolderOf(_project), "toy.lang.yml"), ToyFullDefinition);
            foreach (var f in Directory.GetFiles(_src)) File.Delete(f);
            File.WriteAllText(Path.Combine(_src, "lib.tos"), "MODULE \"LIB\"\nPROC LISTLEN$=3(X%, Y% OUT DEST, Z IN N%)\nENDPROC\n");
            File.WriteAllText(Path.Combine(_src, "lib.toh"), "LIB \"LIB\"\n#const MAXR 10\n#macro LIST(X,DEST) LISTLEN(X,1,DEST)\n");
            File.WriteAllText(Path.Combine(_src, "prog.toy"),
                "MODULE \"PROG\"\nUSE \"lib.toh\"\nVAR TAB_X =8(16), ALTRA$\nPART 0\n" +
                "  A = LIST(1,B)\n  C = TAB_X(3) + MAXR(2) + LEFT(B,2) + strleft(B) + STRLEFT(B)\n  IF (A) PRINT A\nENDPART 0\nEND\n");

            var catalog = CodeLanguageCatalog.ForProject(_project);
            var all = Directory.GetFiles(_src).OrderBy(f => f).Select(f => new CodeSourceFile(f, "src/" + Path.GetFileName(f))).ToList();
            var result = new DeclarativeLanguageExtractor(catalog.ById("toy").Definition)
                .Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = all, FilesToEmit = all });
            Assert.AreEqual(0, result.Warnings.Count, string.Join("\n", result.Warnings));
            var lib = result.Scripts.Single(x => x.File.LogicalPath == "src/lib.tos").Script;
            var header = result.Scripts.Single(x => x.File.LogicalPath == "src/lib.toh").Script;
            var prog = result.Scripts.Single(x => x.File.LogicalPath == "src/prog.toy").Script;

            var listlen = Sym(lib, "src/lib.tos:LISTLEN");
            Assert.AreEqual("string(3)", listlen.ReturnType);
            Assert.AreEqual("X:in Y:in DEST:ref Z:ref N:in",
                string.Join(" ", lib.Parameters.Where(p => p.SymbolId == listlen.Id).OrderBy(p => p.Position).Select(p => p.Name + ":" + p.Direction)),
                "OUT e IN valgono fino alla parola successiva");

            Assert.AreEqual("extracted", EdgeOf(header, Sym(header, "src/lib.toh").Id, Sym(lib, "src/lib.tos").Id, "imports")?.Confidence,
                "LIB \"LIB\" è il modulo LIB, non il file lib.toh che ha lo stesso nome");
            var list = Sym(header, "src/lib.toh:LIST");
            Assert.AreEqual("function", list.Kind);
            Assert.AreEqual(2, header.Parameters.Count(p => p.SymbolId == list.Id));
            Assert.AreEqual("inferred", EdgeOf(header, list.Id, listlen.Id, "calls")?.Confidence, "il corpo della macro chiama la funzione");
            Assert.IsFalse(header.Symbols.Any(x => x.Name == "MAXR"), "una costante è solo indice");

            var part0 = Sym(prog, "src/prog.toy:PART0");
            Assert.AreEqual("inferred", EdgeOf(prog, part0.Id, list.Id, "calls")?.Confidence);
            var externals = prog.Symbols.Where(x => x.Kind == "external").Select(x => x.Name).ToList();
            CollectionAssert.AreEquivalent(new[] { "strleft" }, externals,
                "TAB_X è una variabile, MAXR una costante, LEFT e IF del linguaggio; strleft e STRLEFT sono lo stesso esterno");
        }

        /// <summary>Molti sorgenti aziendali sono in CP-850: letti in UTF-8 ogni accentata diventerebbe U+FFFD e l'hash non vedrebbe «è» → «à».</summary>
        [TestMethod]
        public void Read_a_cp850_source_and_see_an_accent_change_in_the_hash()
        {
            var path = Path.Combine(_src, "acc.toy");
            void Write(string accent) => File.WriteAllBytes(path, MdExplorer.RemoteProtocol.TextDecoding.Encode(
                $"MODULE \"ACC\"\n-- perch{accent} così, città\nPROC F%(IN X$)\n   PRINT \"già fatto\"\nENDPROC\n", MdExplorer.RemoteProtocol.TextDecoding.Ibm850, bom: false));
            Write("é");
            var first = CodeSourceText.Read(path);
            StringAssert.Contains(first, "perché così, città", "riconosciuto CP-850 dal contenuto, senza configurazioni");
            Write("è");
            Assert.AreNotEqual(CodeSourceText.Hash(first), CodeSourceText.Hash(CodeSourceText.Read(path)));
            Assert.AreEqual("ibm850", MdExplorer.RemoteProtocol.TextDecoding.ForExtension(path, File.ReadAllBytes(path), new System.Collections.Generic.Dictionary<string, string> { [".toy"] = "ibm850" }),
                "una regola per estensione della radice vale come per i file remoti");

            var s = Extract("acc.toy");
            Assert.AreEqual("ACC", Sym(s, "src/acc.toy").Name);
            Assert.AreEqual(CodeSourceText.Hash(CodeSourceText.Read(path)), s.Header.SourceHash);
        }

        [TestMethod]
        public void Leave_the_built_in_catalog_alone_without_definitions()
        {
            Assert.AreSame(CodeLanguageCatalog.BuiltIn, CodeLanguageCatalog.ForProject(Path.Combine(_root, "altro")));
        }
    }
}
