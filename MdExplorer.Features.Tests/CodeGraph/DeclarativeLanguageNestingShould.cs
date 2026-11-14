using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Motore 5 dei linguaggi dichiarativi (12/11/2026, rilevazione dal campo su sorgenti PL/SQL): commenti a blocco su più
    /// righe, stringhe su più righe, blocchi che si chiudono contando aperture e chiusure. «Blk» è un linguaggio inventato
    /// con la forma del PL/SQL: package con funzioni sorelle chiuse da «END;», una procedura dentro un'altra, un CASE nelle
    /// dichiarazioni, IF/LOOP/CASE chiusi col loro nome, una dichiarazione su due righe, codice commentato, una stringa su
    /// due righe con dentro «--», «/*» ed «END;».
    /// </summary>
    [TestClass]
    public class DeclarativeLanguageNestingShould
    {
        private const string Definition = @"schema: 1
id: blk
version: 1
extensions: [.blk]
ignoreCase: true
lexical:
  lineComment: '--'
  blockComment: ['/*', '*/']
  string: ""'""
  multilineString: true
blocks:
  - id: body
    kind: class
    begin: '^\s*PACKAGE\s+BODY\s+(?<name>\w+)'
    nesting:
      open: '(?<!\bEND\s+)\b(?:BEGIN|CASE)\b'
      close: '\bEND\b(?!\s+(?:IF|LOOP)\b)'
      body: '\bBEGIN\b'
  - id: proc
    kind: function
    in: [module, body, proc]
    begin: '^\s*(?:PROCEDURE|FUNCTION)\s+(?<name>\w+)'
    nesting:
      header: '\b(?:IS|AS)\b'
      declaration: ';'
      open: '(?<!\bEND\s+)\b(?:BEGIN|CASE)\b'
      close: '\bEND\b(?!\s+(?:IF|LOOP)\b)'
      body: '\bBEGIN\b'
references:
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

        private const string Source = @"PACKAGE BODY PKG AS
FUNCTION F1(a IN VARCHAR2) RETURN DATE
IS
  d DATE;
  CURSOR c IS SELECT CASE WHEN a = 'x' THEN 1 END FROM dual;
BEGIN
  IF a IS NULL THEN
    RETURN NULL;
  END IF;
  FOR r IN c LOOP
    NULL;
  END LOOP;
  CASE a WHEN 'y' THEN NULL; END CASE;
  BEGIN
    INSERT INTO T_ONE VALUES (1);
  END;
  RETURN d;
END;
FUNCTION F2(a IN VARCHAR2,
            b IN DATE) RETURN VARCHAR2
IS
  PROCEDURE INNER(x IN NUMBER)
  AS
  BEGIN
    INSERT INTO T_INNER VALUES (x);
  END;
BEGIN
  INNER(1);
  d := F1(a);
  INSERT INTO T_TWO VALUES ('it''s -- not a comment /* nor this');
  RETURN 'ok';
END F2;
PROCEDURE FWD(a IN NUMBER,
              b IN NUMBER);
/* PROCEDURE GHOST IS
BEGIN
  INSERT INTO T_GHOST VALUES (1);
END; */
-- INSERT INTO T_LINE VALUES (1);
PROCEDURE P3 IS
  s VARCHAR2(200) := 'select 1 -- still the string
from T_STRING /* string too */ where x = ''END;''';
BEGIN
  INSERT INTO T_THREE /* a comment ' with a quote */ VALUES (s); INSERT INTO T_FOUR VALUES (1);
END;
END PKG;
";

        private static CodeScript Extract(out CodeExtractionResult result)
        {
            var root = Path.Combine(Path.GetTempPath(), "mde-nesting-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var file = Path.Combine(root, "pkg.blk");
                File.WriteAllText(file, Source);
                var lang = DeclarativeLanguage.Parse(Definition, "/p/.mde-languages/blk/language.yml");
                var all = new[] { new CodeSourceFile(file, "src/pkg.blk") }.ToList();
                result = new DeclarativeLanguageExtractor(lang).Extract(new CodeExtractionRequest { ProjectPath = root, AllFiles = all, FilesToEmit = all });
                return result.Scripts.Single().Script;
            }
            finally { try { Directory.Delete(root, true); } catch (IOException) { } }
        }

        private static ScriptSymbol Sym(CodeScript s, string local) => s.Symbols.SingleOrDefault(x => x.QualifiedName == "src/pkg.blk:" + local)
            ?? throw new AssertFailedException($"manca {local}; ci sono: {string.Join(", ", s.Symbols.Select(x => x.QualifiedName))}");

        private static string[] Written(CodeScript s, ScriptSymbol from)
            => s.Edges.Where(e => e.SourceId == from.Id && e.Relation == "writes").Select(e => s.Symbols.Single(x => x.Id == e.TargetId).Name).OrderBy(n => n).ToArray();

        [TestMethod]
        public void Close_sibling_blocks_ended_by_an_unnamed_end_and_keep_an_inner_one_inside()
        {
            var s = Extract(out var result);
            Assert.AreEqual(0, result.Warnings.Count, string.Join("\n", result.Warnings));
            var pkg = Sym(s, "PKG");
            var f1 = Sym(s, "PKG.F1");
            var f2 = Sym(s, "PKG.F2");
            var inner = Sym(s, "PKG.F2.INNER");
            var p3 = Sym(s, "PKG.P3");
            Assert.AreEqual((1, 46), (pkg.LineStart, pkg.LineEnd), "il package si chiude al suo END al primo livello");
            Assert.AreEqual((2, 18), (f1.LineStart, f1.LineEnd), "il CASE nelle dichiarazioni, IF/LOOP/CASE e il blocco dentro non chiudono F1");
            Assert.AreEqual((19, 32), (f2.LineStart, f2.LineEnd));
            Assert.AreEqual((22, 26), (inner.LineStart, inner.LineEnd), "la procedura interna finisce al suo END, prima del BEGIN di F2");
            Assert.AreEqual((40, 45), (p3.LineStart, p3.LineEnd), "l'END dentro la stringa non chiude P3");
            Assert.AreEqual(pkg.Id, f2.ParentId, "F2 è sorella di F1, non dentro");
            Assert.AreEqual(f2.Id, inner.ParentId);
            Assert.AreEqual(pkg.Id, p3.ParentId);
            Assert.IsFalse(s.Symbols.Any(x => x.Name == "FWD"), "una dichiarazione su due righe che finisce con ';' non è un blocco");
            Assert.IsFalse(s.Symbols.Any(x => x.Name == "GHOST"), "una procedura commentata non c'è");
        }

        [TestMethod]
        public void Draw_no_edge_from_comments_and_give_each_statement_to_its_own_block()
        {
            var s = Extract(out _);
            CollectionAssert.AreEqual(new[] { "T_ONE" }, Written(s, Sym(s, "PKG.F1")));
            CollectionAssert.AreEqual(new[] { "T_INNER" }, Written(s, Sym(s, "PKG.F2.INNER")), "le righe della procedura interna sono sue");
            CollectionAssert.AreEqual(new[] { "T_TWO" }, Written(s, Sym(s, "PKG.F2")), "'--' e '/*' dentro una stringa non tolgono il resto della riga");
            CollectionAssert.AreEqual(new[] { "T_FOUR", "T_THREE" }, Written(s, Sym(s, "PKG.P3")), "dopo un commento a blocco con una virgoletta il codice continua");
            Assert.IsFalse(s.Symbols.Any(x => x.Name is "T_GHOST" or "T_LINE" or "T_STRING"), "niente dai commenti né dalla stringa su due righe");

            var f2 = Sym(s, "PKG.F2");
            Assert.IsTrue(s.Edges.Any(e => e.SourceId == f2.Id && e.TargetId == Sym(s, "PKG.F2.INNER").Id && e.Relation == "calls"));
            Assert.IsTrue(s.Edges.Any(e => e.SourceId == f2.Id && e.TargetId == Sym(s, "PKG.F1").Id && e.Relation == "calls"));
        }

        [DataTestMethod]
        [DataRow("blocks:\n  - id: f\n    kind: function\n    begin: '(?<name>x)'\n", "blocks[f]: ")]
        [DataRow("blocks:\n  - id: f\n    kind: function\n    begin: '(?<name>x)'\n    end: 'E'\n    nesting:\n      open: 'B'\n      close: 'E'\n", "blocks[f]: ")]
        [DataRow("blocks:\n  - id: f\n    kind: function\n    begin: '(?<name>x)'\n    nesting:\n      open: 'B'\n", "blocks[f].nesting")]
        [DataRow("blocks:\n  - id: f\n    kind: function\n    begin: '(?<name>x)'\n    nesting:\n      open: 'B'\n      close: 'E'\n      declaration: ';'\n", "blocks[f].nesting.declaration")]
        [DataRow("lexical:\n  blockComment: ['/*']\n", "lexical.blockComment")]
        [DataRow("lexical:\n  multilineString: true\n", "lexical.multilineString")]
        public void Refuse_a_broken_nesting_or_lexical_rule_saying_where(string body, string expected)
        {
            var yaml = "schema: 1\nid: blk\nversion: 1\nextensions: [.blk]\n" + body;
            var ex = Assert.ThrowsException<LanguageDefinitionException>(() => DeclarativeLanguage.Parse(yaml, "/p/.mde-languages/blk/language.yml"));
            StringAssert.Contains(ex.Message, expected);
        }
    }
}
