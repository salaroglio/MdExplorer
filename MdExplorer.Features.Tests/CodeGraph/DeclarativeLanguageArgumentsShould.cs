using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Motore 8 dei linguaggi dichiarativi (13/11/2026, rilevazioni dal cliente su PL/SQL): firme lette tutte, anche su più
    /// righe, con i parametri facoltativi; argomenti delle chiamate contati seguendo le parentesi (anche su più righe e per
    /// nome) per scegliere fra i sovraccarichi, anche nel ricollegamento fra contesti; la parentesi sulla riga dopo; gli
    /// elenchi su più righe dopo un modello (<c>FROM T1 a, T2 b</c>). «Blk» è un linguaggio inventato con la forma del PL/SQL.
    /// </summary>
    [TestClass]
    public class DeclarativeLanguageArgumentsShould
    {
        private const string Definition = @"schema: 1
id: blk
version: 1
extensions: [.blk]
ignoreCase: true
lexical:
  lineComment: '--'
  string: ""'""
  multilineString: true
blocks:
  - id: body
    kind: class
    begin: '^\s*PACKAGE\s+BODY\s+(?<name>\w+)'
    nesting:
      open: '(?<!\bEND\s+)\bBEGIN\b'
      close: '\bEND\b'
      body: '\bBEGIN\b'
  - id: proc
    kind: function
    in: [body]
    begin: '^\s*(?:PROCEDURE|FUNCTION)\s+(?<name>\w+)\s*(?:\((?<params>[^)]*)\)?)?'
    nesting:
      header: '\b(?:IS|AS)\b'
      declaration: ';'
      open: '\bBEGIN\b'
      close: '\bEND\b'
      body: '\bBEGIN\b'
  - id: table
    kind: record
    begin: '^\s*TABLE\s+(?<name>\w+)'
    end: ';'
parameters:
  item: '(?<name>\w+)\s+(?:(?<direction>IN\s+OUT|IN|OUT)\s+)?(?<type>\w+)'
  direction: { IN: in, OUT: out, 'IN OUT': ref }
  default: in
  separator: ','
  optional: '\bDEFAULT\b'
references:
  - relation: calls
    pattern: '(?<![\w.])(?<owner>\w+)\.(?<target>\w+)(?:\s*\(|\s*$)'
    text: code
    resolve: symbol
    rules: [proc]
    unresolved: external
    arguments: { separator: ',', named: '^\s*(?<name>\w+)\s*=>', nextLine: true }
  - relation: calls
    pattern: '(?<![\w.])(?<target>\w+)\s*\('
    text: code
    resolve: symbol
    rules: [proc]
    scope: module
    arguments: { separator: ',', named: '^\s*(?<name>\w+)\s*=>' }
  - relation: reads
    pattern: '\bFROM\b'
    text: code
    list:
      until: '(?:WHERE|GROUP|ORDER)\b|;'
      separator: ','
      item: '\s*(?<target>\w+)(?![\w(.])'
    resolve: symbol
    rules: [table]
    unresolved: external
";

        private const string Package = @"PACKAGE BODY PKG AS
FUNCTION F(a IN NUMBER) RETURN NUMBER IS
BEGIN
  RETURN a;
END;
FUNCTION F(a IN NUMBER,
           b IN VARCHAR2 DEFAULT 'x,y',
           c IN OUT NUMBER) RETURN NUMBER IS
BEGIN
  RETURN 1;
END;
PROCEDURE RUN(a IN NUMBER) IS
BEGIN
  v := F(1);
  w := F(1, c => v);
  z := F(G(a, 1),
         'q,r',
         v);
  u := F(1, 2);
  s := t.COL
       + 1;
  r := PKG.F
       (1);
  SELECT x INTO v FROM T_ONE o, T_TWO p,
         (SELECT 1 FROM T_THREE WHERE 1 = 1) q,
         T_FOUR
   WHERE o.c = p.c;
END;
END PKG;
";

        private static List<CodeScript> Extract(out CodeExtractionResult result, params (string Name, string Text)[] files)
        {
            var root = Path.Combine(Path.GetTempPath(), "mde-arguments-tests", Guid.NewGuid().ToString("N"));
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

        /// <summary>Gli archi di un simbolo, riga per riga: «riga:bersaglio:certezza».</summary>
        private static string Out(List<CodeScript> s, string from, string relation)
        {
            var names = s.SelectMany(x => x.Symbols).GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().QualifiedName);
            var id = Sym(s, from).Id;
            return string.Join(" | ", s.SelectMany(x => x.Edges).Where(e => e.SourceId == id && e.Relation == relation)
                .OrderBy(e => e.Line).ThenBy(e => names[e.TargetId], StringComparer.Ordinal).Select(e => $"{e.Line}:{names[e.TargetId]}:{e.Confidence}"));
        }

        [TestMethod]
        public void Read_a_signature_on_several_lines_with_its_optional_parameters()
        {
            var s = Extract(out _, ("pkg.blk", Package));
            var f2 = Sym(s, "db/pkg.blk:PKG.F@6");
            var parameters = s.SelectMany(x => x.Parameters).Where(p => p.SymbolId == f2.Id).OrderBy(p => p.Position)
                .Select(p => $"{p.Name}:{p.ParameterType}:{p.Direction}:{p.DefaultValue}");
            CollectionAssert.AreEqual(new[] { "a:NUMBER:in:", "b:VARCHAR2:in:'x,y'", "c:NUMBER:ref:" }, parameters.ToArray(),
                "le righe dopo l'intestazione e la virgola dentro la stringa del default");
        }

        [TestMethod]
        public void Choose_the_overload_that_accepts_the_written_arguments()
        {
            var s = Extract(out var result, ("pkg.blk", Package));
            // F(1) → F; F(1, c => v), e F(G(a,1), 'q,r', v) su tre righe → F@6 (b ha un default); F(1, 2) non va a nessuno:
            // c non ha default (ambiguous, con un avviso); PKG.F con la parentesi a capo → F
            Assert.AreEqual("14:db/pkg.blk:PKG.F:extracted | 15:db/pkg.blk:PKG.F@6:extracted", Out(s, "db/pkg.blk:PKG.RUN", "calls"),
                "un arco per bersaglio: F alle righe 14 e 22, F@6 alle 15 e 16 (certe); la 19 li tocca tutti e due come ambiguous, e vince la certa");
            var warning = result.Warnings.Single(w => w.Contains(":19:"));
            StringAssert.Contains(warning, "'F'");
            Assert.IsFalse(s.SelectMany(x => x.Symbols).Any(x => x.Kind == "external" && x.Name.Contains("COL")), "t.COL a capo seguito da + 1 non è una chiamata");
        }

        [TestMethod]
        public void Read_every_item_of_a_list_on_several_lines_and_stop_at_the_end_of_a_subquery()
        {
            var s = Extract(out _, ("pkg.blk", Package));
            Assert.AreEqual("24:T_ONE:inferred | 24:T_TWO:inferred | 25:T_THREE:inferred | 26:T_FOUR:inferred", Out(s, "db/pkg.blk:PKG.RUN", "reads"),
                "FROM T_ONE o, T_TWO p, (sottoquery) q, T_FOUR fino a WHERE; la sottoquery ha il suo FROM");
        }

        [TestMethod]
        public void Choose_the_overload_when_relinking_a_call_from_another_context()
        {
            // il chiamante analizzato da solo: PKG non c'è, le chiamate sono esterne con gli argomenti scritti
            var caller = Extract(out _, ("run.blk", "PACKAGE BODY OTHER AS\nPROCEDURE GO(a IN NUMBER) IS\nBEGIN\n  v := PKG.F(1);\n  w := PKG.F(1, c => v);\nEND;\nEND OTHER;\n"));
            var other = Extract(out _, ("pkg.blk", Package));
            CodeCrossLinker.LinkAll("blk", true, new[] { ("db/run.blk", caller[0]) }, new[] { ("db/pkg.blk", other[0]) }, allSymbols: true);
            var go = Sym(caller, "db/run.blk:OTHER.GO").Id;
            var targets = caller[0].Edges.Where(e => e.SourceId == go && e.Relation == "calls").Select(e => (e.Line, e.TargetId, e.Confidence)).OrderBy(e => e.Line).ToList();
            Assert.AreEqual(2, targets.Count, string.Join(", ", targets));
            Assert.AreEqual((4, Sym(other, "db/pkg.blk:PKG.F").Id, "inferred"), targets[0]);
            Assert.AreEqual((5, Sym(other, "db/pkg.blk:PKG.F@6").Id, "inferred"), targets[1]);
        }

        [DataTestMethod]
        [DataRow("references:\n  - relation: reads\n    pattern: 'FROM'\n    list: { until: ';', separator: ',' }\n    resolve: symbol\n    rules: [t]\n", "list")]
        [DataRow("references:\n  - relation: calls\n    pattern: '(?<target>x)'\n    resolve: module\n    arguments: { separator: ',' }\n", "arguments")]
        [DataRow("parameters:\n  item: '(?<name>x)'\n  optional: 'DEFAULT'\n", "parameters.optional")]
        public void Refuse_a_broken_list_arguments_or_parameters_rule_saying_where(string body, string expected)
        {
            var yaml = "schema: 1\nid: blk\nversion: 1\nextensions: [.blk]\nblocks:\n  - id: t\n    kind: record\n    begin: '(?<name>T)'\n    end: ';'\n" + body;
            var ex = Assert.ThrowsException<LanguageDefinitionException>(() => DeclarativeLanguage.Parse(yaml, "/p/.mde-languages/blk/language.yml"));
            StringAssert.Contains(ex.Message, expected);
        }
    }
}
