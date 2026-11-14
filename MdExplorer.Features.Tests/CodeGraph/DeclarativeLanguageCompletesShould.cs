using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// <c>completes</c> (13/11/2026, rilevazione dal cliente: specifica e corpo del package nello stesso file, 1192 «già
    /// dichiarato» e chi chiamava PKG.PROC arrivava alla firma, che «chiamava» il corpo). «Blk» ha la forma del PL/SQL: la
    /// specifica dichiara le firme, il corpo le implementa; una funzione sovraccarica.
    /// </summary>
    [TestClass]
    public class DeclarativeLanguageCompletesShould
    {
        private const string Definition = @"schema: 1
id: blk
version: 1
extensions: [.blk, .spc, .bdy]
ignoreCase: true
lexical:
  lineComment: '--'
  string: ""'""
blocks:
  - id: spec
    kind: interface
    begin: '^\s*PACKAGE\s+(?!BODY\b)(?<name>\w+)'
    nesting:
      open: '\bBEGIN\b'
      close: '\bEND\b'
  - id: body
    kind: class
    completes: spec
    begin: '^\s*PACKAGE\s+BODY\s+(?<name>\w+)'
    nesting:
      open: '(?<!\bEND\s+)\bBEGIN\b'
      close: '\bEND\b'
      body: '\bBEGIN\b'
  - id: proc
    kind: function
    in: [module, body]
    begin: '^\s*(?:PROCEDURE|FUNCTION)\s+(?<name>\w+)\s*(?:\((?<params>[^)]*)\)?)?'
    nesting:
      header: '\b(?:IS|AS)\b'
      declaration: ';'
      open: '\bBEGIN\b'
      close: '\bEND\b'
      body: '\bBEGIN\b'
declarations:
  - id: decl
    kind: function
    in: [spec]
    at: '^\s*(?:PROCEDURE|FUNCTION)\s+(?<name>\w+)\s*(?:\((?<params>[^)]*)\)?)?'
parameters:
  item: '(?<name>\w+)\s+(?:(?<direction>IN|OUT)\s+)?(?<type>\w+)'
  direction: { IN: in, OUT: out }
  default: in
references:
  - relation: calls
    pattern: '(?<![\w.])(?:\w+\.(?=\w+\.\w+\s*\())?(?<owner>\w+)\.(?<target>\w+)\s*\('
    text: code
    ignoreUnresolved: [EXISTS, COUNT]
    resolve: symbol
    rules: [proc, decl]
    unresolved: external
  - relation: calls
    pattern: '(?<![\w.])(?<target>\w+)\s*\('
    text: code
    resolve: symbol
    rules: [proc]
    scope: module
  - relation: writes
    pattern: '\bINSERT\s+INTO\s+(?<target>\w+)'
    text: code
    resolve: symbol
    rules: [proc]
    unresolved: external
";

        private const string Spec = @"PACKAGE PKG AS
PROCEDURE SAVE(a IN NUMBER);
FUNCTION F(a IN NUMBER) RETURN NUMBER;
FUNCTION F(a IN NUMBER, b IN NUMBER) RETURN NUMBER;
END PKG;
";

        private const string Body = @"PACKAGE BODY PKG AS
PROCEDURE SAVE(a IN NUMBER) IS
BEGIN
  INSERT INTO T_ONE VALUES (F(a));
END;
FUNCTION F(a IN NUMBER) RETURN NUMBER IS
BEGIN
  RETURN a;
END;
FUNCTION F(a IN NUMBER, b IN NUMBER) RETURN NUMBER IS
BEGIN
  RETURN a + b;
END;
END PKG;
";

        private const string Caller = @"PACKAGE BODY OTHER AS
PROCEDURE RUN(a IN NUMBER) IS
BEGIN
  PKG.SAVE(a);
END;
END OTHER;
";

        private static List<CodeScript> Extract(out CodeExtractionResult result, params (string Name, string Text)[] files)
        {
            var root = Path.Combine(Path.GetTempPath(), "mde-completes-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var all = files.Select(f =>
                {
                    var path = Path.Combine(root, f.Name);
                    File.WriteAllText(path, f.Text);
                    return new CodeSourceFile(path, "db/" + f.Name);
                }).ToList();
                var lang = DeclarativeLanguage.Parse(Definition, "/p/.mde-languages/blk/language.yml");
                result = new DeclarativeLanguageExtractor(lang).Extract(new CodeExtractionRequest { ProjectPath = root, AllFiles = all, FilesToEmit = all });
                return result.Scripts.Select(s => s.Script).ToList();
            }
            finally { try { Directory.Delete(root, true); } catch (IOException) { } }
        }

        private static ScriptSymbol Sym(IEnumerable<CodeScript> scripts, string qualified)
            => scripts.SelectMany(s => s.Symbols).SingleOrDefault(x => x.QualifiedName == qualified)
               ?? throw new AssertFailedException($"manca {qualified}; ci sono: {string.Join(", ", scripts.SelectMany(s => s.Symbols).Select(x => x.QualifiedName))}");

        private static List<ScriptEdge> Edges(IEnumerable<CodeScript> scripts, ScriptSymbol from, string relation)
            => scripts.SelectMany(s => s.Edges).Where(e => e.SourceId == from.Id && e.Relation == relation).ToList();

        [TestMethod]
        public void Give_the_name_to_the_body_when_specification_and_body_are_in_the_same_file()
        {
            var s = Extract(out var result, ("pkg.blk", Spec + Body), ("other.blk", Caller));
            Assert.IsFalse(result.Warnings.Any(w => w.Contains("'PKG'") || w.Contains("'SAVE'")), "specifica e corpo non sono doppioni (restano solo i sovraccarichi di F): " + string.Join("\n", result.Warnings));

            var body = Sym(s, "db/pkg.blk:PKG");
            var save = Sym(s, "db/pkg.blk:PKG.SAVE");
            var spec = Sym(s, "db/pkg.blk:PKG@spec");
            var saveDecl = Sym(s, "db/pkg.blk:PKG@spec.SAVE");
            Assert.AreEqual("class", body.Kind);
            Assert.AreEqual((6, 19), (body.LineStart, body.LineEnd));
            Assert.AreEqual((7, 10), (save.LineStart, save.LineEnd), "il nome PKG.SAVE è della procedura con il codice");
            Assert.AreEqual("interface", spec.Kind);
            Assert.AreEqual((2, 2), (saveDecl.LineStart, saveDecl.LineEnd));

            Assert.AreEqual(0, Edges(s, saveDecl, "calls").Count, "la firma nella specifica non chiama niente");
            var impl = Edges(s, save, "implements").Single();
            Assert.AreEqual((saveDecl.Id, "extracted"), (impl.TargetId, impl.Confidence));
            Assert.AreEqual((spec.Id, "extracted"), (Edges(s, body, "implements").Single().TargetId, Edges(s, body, "implements").Single().Confidence));

            var call = Edges(s, Sym(s, "db/other.blk:OTHER.RUN"), "calls").Single();
            Assert.AreEqual((save.Id, "inferred"), (call.TargetId, call.Confidence), "PKG.SAVE da un altro file arriva al corpo, non alla firma");
        }

        [TestMethod]
        public void Pair_overloads_by_number_of_parameters_and_never_read_a_header_as_a_call()
        {
            var s = Extract(out var result, ("pkg.blk", Spec + Body));
            var f1 = Sym(s, "db/pkg.blk:PKG.F");
            var f2 = Sym(s, "db/pkg.blk:PKG.F@15");
            var d1 = Sym(s, "db/pkg.blk:PKG@spec.F");
            var d2 = Sym(s, "db/pkg.blk:PKG@spec.F@4");
            Assert.AreEqual(d1.Id, Edges(s, f1, "implements").Single().TargetId);
            Assert.AreEqual(d2.Id, Edges(s, f2, "implements").Single().TargetId);
            Assert.AreEqual(0, Edges(s, f1, "calls").Count + Edges(s, f2, "calls").Count, "l'intestazione di un sovraccarico non chiama l'altro");
            Assert.AreEqual(2, result.Warnings.Count, "restano i due sovraccarichi, uno nella specifica e uno nel corpo: " + string.Join("\n", result.Warnings));
            Assert.IsTrue(result.Warnings.All(w => w.Contains("'F'")));
        }

        [TestMethod]
        public void Link_specification_and_body_in_separate_files_as_inferred_and_send_calls_to_the_body()
        {
            var s = Extract(out var result, ("pkg.spc", Spec), ("pkg.bdy", Body), ("other.blk", Caller));
            var save = Sym(s, "db/pkg.bdy:PKG.SAVE");
            var saveDecl = Sym(s, "db/pkg.spc:PKG.SAVE");
            var impl = Edges(s, save, "implements").Single();
            Assert.AreEqual((saveDecl.Id, "inferred"), (impl.TargetId, impl.Confidence));
            var call = Edges(s, Sym(s, "db/other.blk:OTHER.RUN"), "calls").Single();
            Assert.AreEqual((save.Id, "inferred"), (call.TargetId, call.Confidence), "fra la firma e il corpo, il corpo: non ambiguous");
        }

        [TestMethod]
        public void Keep_a_specification_without_body_as_the_target()
        {
            var s = Extract(out _, ("pkg.spc", Spec), ("other.blk", Caller));
            var call = Edges(s, Sym(s, "db/other.blk:OTHER.RUN"), "calls").Single();
            Assert.AreEqual(Sym(s, "db/pkg.spc:PKG.SAVE").Id, call.TargetId, "senza corpo il grafo arriva alla firma");
        }

        private const string Others = @"PACKAGE BODY PKG_A AS
PROCEDURE SAVE(a IN NUMBER) IS
BEGIN
  PKG_B.SAVE(a);
  SCH.PKG_B.SAVE(a);
  PKG_C.SAVE(a);
  SCH.RUN(a);
  PKG_B.MISSING(a);
END;
END PKG_A;
PACKAGE BODY PKG_B AS
PROCEDURE SAVE(a IN NUMBER) IS
BEGIN
  INSERT INTO T_B VALUES (a);
END;
END PKG_B;
";

        private const string Standalone = @"PROCEDURE RUN(a IN NUMBER) IS
BEGIN
  INSERT INTO T_RUN VALUES (a);
END;
";

        [TestMethod]
        public void Send_a_call_written_with_its_package_to_that_package_and_never_to_the_namesake_of_another()
        {
            // 13/11/2026: PKG_B.SAVE dentro PKG_A arrivava al SAVE di PKG_A (lo stesso file vince), «extracted»
            var s = Extract(out _, ("a.blk", Others), ("run.blk", Standalone));
            var names = s.SelectMany(x => x.Symbols).GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().QualifiedName);
            string Calls(List<CodeScript> scripts) => string.Join(" | ", Edges(scripts, Sym(scripts, "db/a.blk:PKG_A.SAVE"), "calls")
                .Select(e => $"{names[e.TargetId]}:{e.Confidence}").OrderBy(x => x, StringComparer.Ordinal));
            Assert.AreEqual("PKG_B.MISSING:inferred | PKG_C.SAVE:inferred | db/a.blk:PKG_B.SAVE:extracted | db/run.blk:RUN:inferred", Calls(s),
                "PKG_B.SAVE (anche con lo schema) al SAVE di PKG_B; PKG_C non c'è: esterno PKG_C.SAVE, non il SAVE del chiamante; " +
                "SCH.RUN: SCH non è un package, è lo schema della procedura a sé; PKG_B c'è ma non ha MISSING: esterno");

            // il ricollegamento fra contesti legge lo stesso nome con il package e non cambia niente
            var scripts = s.Select((x, i) => (Path: i == 0 ? "db/a.blk" : "db/run.blk", Script: x)).ToList();
            CodeCrossLinker.LinkAll("blk", true, scripts, Array.Empty<(string, CodeScript)>(), allSymbols: true);
            names = s.SelectMany(x => x.Symbols).GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().QualifiedName);
            Assert.AreEqual("PKG_B.MISSING:inferred | PKG_C.SAVE:inferred | db/a.blk:PKG_B.SAVE:extracted | db/run.blk:RUN:inferred", Calls(s));
        }

        [TestMethod]
        public void Relink_a_call_written_with_its_package_to_that_package_from_another_context()
        {
            // il chiamante analizzato da solo (un contesto), il package dopo (un altro): il ricollegamento sceglie per package
            var caller = Extract(out _, ("a.blk", Others.Substring(0, Others.IndexOf("PACKAGE BODY PKG_B", StringComparison.Ordinal))));
            var other = Extract(out _, ("b.blk", Others[Others.IndexOf("PACKAGE BODY PKG_B", StringComparison.Ordinal)..]),
                ("c.blk", "PACKAGE BODY PKG_C AS\nPROCEDURE SAVE(a IN NUMBER) IS\nBEGIN\n  NULL;\nEND;\nEND PKG_C;\n"));
            CodeCrossLinker.LinkAll("blk", true, new[] { ("db/a.blk", caller[0]) }, new[] { ("db/b.blk", other[0]), ("db/c.blk", other[1]) }, allSymbols: true);
            var save = Sym(caller, "db/a.blk:PKG_A.SAVE");
            var targets = caller[0].Edges.Where(e => e.SourceId == save.Id && e.Relation == "calls").Select(e => e.TargetId).ToList();
            CollectionAssert.Contains(targets, Sym(other, "db/b.blk:PKG_B.SAVE").Id);
            CollectionAssert.Contains(targets, Sym(other, "db/c.blk:PKG_C.SAVE").Id);
            Assert.AreEqual(4, targets.Count, "PKG_B.SAVE due volte (con e senza schema) è un arco; PKG_C.SAVE; RUN e PKG_B.MISSING esterni: " + string.Join(", ", targets));
        }

        [TestMethod]
        public void Ignore_a_listed_name_only_when_it_does_not_resolve()
        {
            // 13/11/2026: v_tab.EXISTS(i) è un metodo di collezione, non un nodo esterno; PKG_B.COUNT(…) una procedura vera
            var s = Extract(out _, ("a.blk", @"PACKAGE BODY PKG_A AS
PROCEDURE SAVE(a IN NUMBER) IS
BEGIN
  v_ok := v_tab.EXISTS(a);
  PKG_B.COUNT(a);
END;
END PKG_A;
PACKAGE BODY PKG_B AS
PROCEDURE COUNT(a IN NUMBER) IS
BEGIN
  NULL;
END;
END PKG_B;
"));
            var calls = Edges(s, Sym(s, "db/a.blk:PKG_A.SAVE"), "calls");
            Assert.AreEqual(Sym(s, "db/a.blk:PKG_B.COUNT").Id, calls.Single().TargetId, "COUNT si risolve: la freccia c'è");
            Assert.IsFalse(s.SelectMany(x => x.Symbols).Any(x => x.Kind == "external" && x.Name.Contains("EXISTS")), "EXISTS non si risolve: niente nodo esterno");
        }

        [TestMethod]
        public void Refuse_completes_that_names_no_other_block()
        {
            var yaml = Definition.Replace("completes: spec", "completes: nothing");
            var ex = Assert.ThrowsException<LanguageDefinitionException>(() => DeclarativeLanguage.Parse(yaml, "/p/.mde-languages/blk/language.yml"));
            StringAssert.Contains(ex.Message, "blocks[body].completes");
        }
    }
}
