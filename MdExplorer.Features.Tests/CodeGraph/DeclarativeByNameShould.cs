using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Motore 12 (20/11/2026): un riferimento <c>byName</c> dichiara dei nomi — il file senza estensione e il gruppo <c>alias</c> —
    /// e chi li usa in un blocco riceve lo stesso arco. Il caso vero: un archivio incluso in cima al file e usato per nome dalla
    /// funzione (l'arco partiva solo dal modulo). Linguaggio inventato (Toy), mai i nomi di un cliente.
    /// </summary>
    [TestClass]
    public class DeclarativeByNameShould
    {
        private const string Rule = @"  - relation: references
    pattern: '^\s*FILE\s+""(?<target>[^"";]+)(?:;(?<alias>\w+))?""'
    text: raw
    resolve: file
    unresolved: external
    byName: true
";

        private string _dir;

        [TestInitialize]
        public void Setup()
        {
            _dir = Path.Combine(Path.GetTempPath(), "mde-byname-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_dir, true); } catch (IOException) { } }

        private static DeclarativeLanguage Toy(string extra, string path) =>
            DeclarativeLanguage.Parse(DeclarativeLanguageExtractorShould.ToyFullDefinition + extra, path);

        private CodeScript Extract(string main)
        {
            File.WriteAllText(Path.Combine(_dir, "main.toy"), main);
            File.WriteAllText(Path.Combine(_dir, "UARCH.TOS"), "MODULE \"UARCH\"\nVAR R_KEY\n");
            File.WriteAllText(Path.Combine(_dir, "ULOC.TOS"), "MODULE \"ULOC\"\nVAR L_KEY\n");
            var lang = Toy(Rule, Path.Combine(_dir, ".mde-languages", "toy", "language.yml"));
            var all = Directory.GetFiles(_dir).OrderBy(f => f).Select(f => new CodeSourceFile(f, "src/" + Path.GetFileName(f))).ToList();
            var result = new DeclarativeLanguageExtractor(lang).Extract(new CodeExtractionRequest
            {
                ProjectPath = _dir, AllFiles = all, FilesToEmit = all.Where(f => f.LogicalPath == "src/main.toy").ToList(),
            });
            return result.Scripts.Single().Script;
        }

        private static string Id(CodeScript s, string qualifiedName) => s.Symbols.Single(x => x.QualifiedName == qualifiedName).Id;

        /// <summary>Gli archi references di un simbolo: (destinazione leggibile, confidenza).</summary>
        private static (string Target, string Confidence)[] References(CodeScript s, string qualifiedName)
        {
            var id = Id(s, qualifiedName);
            return s.Edges.Where(e => e.SourceId == id && e.Relation == "references")
                .Select(e => (s.Symbols.FirstOrDefault(x => x.Id == e.TargetId)?.QualifiedName ?? e.TargetId, e.Confidence))
                .OrderBy(t => t.Item1, StringComparer.Ordinal).ToArray();
        }

        [TestMethod]
        public void Link_the_block_that_uses_the_alias_or_the_file_name_declared_at_the_top()
        {
            var s = Extract(
                "MODULE \"MAIN\"\n" +
                "FILE \"UARCH.TOS;UARCH_VIEW\"\n" +
                "FILE \"UOTHER\"\n" +
                "PROC USES_ALIAS()\n   ZERO(UARCH_VIEW, K1)\n   ZERO(UARCH_VIEW, K2)\nENDPROC\n" +
                "PROC USES_FILE()\n   FETCH(UOTHER, K2)\nENDPROC\n" +
                "PROC DOES_NOT()\n   PRINT \"UARCH_VIEW\"  -- UOTHER nel commento\n   UOTHERX = 1\n   UARCH_VIEWS = 2\nENDPROC\n");

            var alias = References(s, "src/main.toy:USES_ALIAS");
            Assert.AreEqual(1, alias.Length, "un arco solo, anche se il nome torna due volte");
            Assert.AreEqual("inferred", alias[0].Confidence, "dedotto dal nome, non scritto");
            var moduleId = Id(s, "src/main.toy");
            var includeTarget = s.Edges.Single(e => e.SourceId == moduleId && e.Relation == "references" && e.Line == 2).TargetId;
            Assert.AreEqual(includeTarget, alias[0].Target, "l'alias porta allo stesso file dell'include (UARCH.TOS)");

            var file = References(s, "src/main.toy:USES_FILE");
            Assert.AreEqual(1, file.Length, "il nome del file senza estensione vale come nome");
            StringAssert.Contains(file[0].Target, "UOTHER", "non trovato: l'esterno dell'include, che il ricollegamento aggancia dopo");

            Assert.AreEqual(0, References(s, "src/main.toy:DOES_NOT").Length, "stringa, commento e nomi più lunghi non sono usi");
            Assert.AreEqual(2, References(s, "src/main.toy").Length, "il modulo ha i suoi archi dalle righe FILE, come prima");
        }

        [TestMethod]
        public void Keep_a_name_declared_inside_a_block_to_that_block_and_never_double_the_arc()
        {
            var s = Extract(
                "MODULE \"MAIN\"\n" +
                "PROC BEFORE()\n   ZERO(ULOC)\nENDPROC\n" +
                "PROC INSIDE()\n   FILE \"ULOC.TOS\"\n   ZERO(ULOC)\nENDPROC\n" +
                "PROC AFTER()\n   ZERO(ULOC)\nENDPROC\n");

            var inside = References(s, "src/main.toy:INSIDE");
            Assert.AreEqual(1, inside.Length, "l'arco dell'include, non uno in più per l'uso");
            Assert.AreEqual("extracted", inside[0].Confidence);
            Assert.AreEqual(0, References(s, "src/main.toy:BEFORE").Length, "prima della dichiarazione");
            Assert.AreEqual(0, References(s, "src/main.toy:AFTER").Length, "dichiarato in un altro blocco");
        }

        [TestMethod]
        public void Refuse_byName_outside_resolve_file()
        {
            var bad = Rule.Replace("resolve: file", "resolve: module");
            var ex = Assert.ThrowsExactly<LanguageDefinitionException>(() => Toy(bad, Path.Combine(_dir, ".mde-languages", "toy", "language.yml")));
            StringAssert.Contains(ex.Message, "byName");
        }
    }
}
