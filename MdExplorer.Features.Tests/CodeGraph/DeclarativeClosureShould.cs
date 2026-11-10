using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// F2a (10/11/2026): la chiusura di un seme in un linguaggio a regole, scoperta senza scrivere niente — verso il basso,
    /// per simbolo, prima i file importati, con budget e potature.
    /// </summary>
    [TestClass]
    public class DeclarativeClosureShould
    {
        private string _dir;
        private DeclarativeLanguage _lang;

        [TestInitialize]
        public void Setup()
        {
            _dir = Path.Combine(Path.GetTempPath(), "mde-closure-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _lang = DeclarativeLanguage.Parse(DeclarativeLanguageExtractorShould.ToyFullDefinition, Path.Combine(_dir, ".mde-languages", "toy", "language.yml"));
            Write("main.toy", "MODULE \"MAIN\"\nUSE \"inc.toh\"\nLIB \"LIBA\"\nPROC START()\n   HELPER(1)\n   M1(2)\nENDPROC\n");
            Write("inc.toh", "#macro M1(x) INNER(x)\n#macro M2(x) NEVERCALLED(x)\n");
            Write("liba.tos", "MODULE \"LIBA\"\nLIB \"LIBB\"\nPROC HELPER(X)\n   DEEP(X)\nENDPROC\nPROC NOTUSED()\n   OTHER()\nENDPROC\nPROC INNER(X)\nENDPROC\n");
            Write("libb.tos", "MODULE \"LIBB\"\nPROC DEEP(X)\nENDPROC\n");
            Write("libc.tos", "MODULE \"LIBC\"\nPROC HELPER(X)\nENDPROC\n");
            Write("unrelated.tos", "MODULE \"ZZZ\"\nPROC OTHER()\nENDPROC\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_dir, true); } catch (IOException) { } }

        private void Write(string name, string text) => File.WriteAllText(Path.Combine(_dir, name), text);

        private CodeClosure Run(CodeClosureRequest request) => new DeclarativeClosure(_lang, new LocalCodeWorkspace(_dir, "src")).Compute(request);

        private static CodeClosureFile File_(CodeClosure c, string name) => c.Files.SingleOrDefault(f => f.Path == "src/" + name);

        [TestMethod]
        public void Go_down_from_the_seed_by_symbol_and_leave_out_what_is_not_called()
        {
            var c = Run(new CodeClosureRequest { Seeds = new[] { "src/main.toy" } });
            CollectionAssert.AreEquivalent(new[] { "src/main.toy", "src/inc.toh", "src/liba.tos", "src/libb.tos" }, c.Files.Select(f => f.Path).ToList(),
                "libc ha un HELPER ma MAIN importa LIBA; unrelated serve solo a NOTUSED, che nessuno chiama");
            Assert.IsTrue(File_(c, "main.toy").Whole, "il seme entra intero");
            CollectionAssert.AreEqual(new[] { "M1" }, File_(c, "inc.toh").Symbols.ToList(), "dell'include solo la macro chiamata");
            CollectionAssert.AreEquivalent(new[] { "HELPER", "INNER" }, File_(c, "liba.tos").Symbols.ToList(), "HELPER dal seme, INNER dalla macro; NOTUSED no");
            var libb = File_(c, "libb.tos");
            Assert.AreEqual((2, "module:LIBB"), (libb.Depth, libb.Written), "la libreria della libreria, dal suo LIB");
            Assert.IsFalse(c.Misses.Any(m => m.Written == "symbol:NEVERCALLED"), "una macro non chiamata non porta niente");
            Assert.IsFalse(c.Misses.Any(m => m.Kind == "ambiguous"), "HELPER: vince la libreria importata");
        }

        [TestMethod]
        public void Stop_at_the_budget_and_at_what_the_user_prunes()
        {
            var small = Run(new CodeClosureRequest { Seeds = new[] { "src/main.toy" }, MaxFiles = 2 });
            Assert.IsTrue(small.BudgetReached);
            Assert.AreEqual(2, small.Files.Count);
            Assert.IsTrue(small.Misses.Any(m => m.Kind == "frontier"), "oltre il budget: la frontiera, da espandere a mano");

            var pruned = Run(new CodeClosureRequest { Seeds = new[] { "src/main.toy" }, Pruned = new HashSet<string> { "src/liba.tos" } });
            Assert.IsNull(File_(pruned, "liba.tos"), "potata");
            Assert.IsNull(File_(pruned, "libb.tos"), "e con lei quello che portava");
            Assert.IsTrue(pruned.Misses.Any(m => m.Kind == "pruned" && m.Candidates.Contains("src/liba.tos")));
        }

        [TestMethod]
        public void Report_each_missing_reference_once_with_its_uses()
        {
            Write("main.toy", "MODULE \"MAIN\"\nPROC START()\n   NOWHERE(1)\n   NOWHERE(2)\nENDPROC\nPROC AGAIN()\n   NOWHERE(3)\nENDPROC\n");
            var c = Run(new CodeClosureRequest { Seeds = new[] { "src/main.toy" } });
            var miss = c.Misses.Single(m => m.Written == "symbol:NOWHERE");
            Assert.AreEqual("unresolved", miss.Kind);
            Assert.AreEqual(2, miss.Uses, "un arco per simbolo che chiama: START e AGAIN");
            Assert.AreEqual(2, miss.From.Count);
        }
    }
}
