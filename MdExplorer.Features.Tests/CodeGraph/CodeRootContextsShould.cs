using System;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// I contesti di una radice (P2 di 2026-10-27-Grafo-Di-Progetti-Enormi), con le regole decise il 28/10: A — la radice
    /// intera O i contesti; B — un contesto mai dentro un altro. I messaggi si controllano per i percorsi che nominano: la
    /// lingua è quella dell'interfaccia.
    /// </summary>
    [TestClass]
    public class CodeRootContextsShould
    {
        [TestMethod]
        public void Mark_a_subfolder_only_when_the_whole_root_is_not_a_program()
        {
            StringAssert.Contains(CodeRootContexts.MarkProblem("globo", rootIsProgram: true, Array.Empty<string>(), "fatturazione"), "@globo", "A: la radice intera è un programma");
            Assert.IsNull(CodeRootContexts.MarkProblem("globo", rootIsProgram: false, Array.Empty<string>(), "fatturazione"));
            StringAssert.Contains(CodeRootContexts.MarkProblem("globo", rootIsProgram: false, new[] { "fatturazione" }, ""), "fatturazione");
            Assert.IsNull(CodeRootContexts.MarkProblem("globo", rootIsProgram: false, Array.Empty<string>(), ""), "senza contesti la radice si marca");
            Assert.IsNull(CodeRootContexts.MarkProblem("globo", rootIsProgram: true, Array.Empty<string>(), ""), "già marcata: niente da dire");
        }

        [TestMethod]
        public void Refuse_a_context_inside_or_around_another()
        {
            var contexts = new[] { "fatturazione/core", "anagrafica" };
            StringAssert.Contains(CodeRootContexts.MarkProblem("globo", false, contexts, "fatturazione/core/dao"), "@globo/fatturazione/core.", "B: dentro");
            StringAssert.Contains(CodeRootContexts.MarkProblem("globo", false, contexts, "fatturazione"), "@globo/fatturazione/core:", "B: intorno");
            Assert.IsNull(CodeRootContexts.MarkProblem("globo", false, contexts, "fatturazione/web"), "accanto, non dentro");
            Assert.IsNull(CodeRootContexts.MarkProblem("globo", false, contexts, "anagrafica-old"), "il prefisso conta per cartelle intere");
            Assert.IsNull(CodeRootContexts.MarkProblem("globo", false, contexts, "/anagrafica/"), "già marcato, scritto in un altro modo");
        }

        [TestMethod]
        public void Normalize_the_path_and_refuse_to_leave_the_root()
        {
            Assert.AreEqual("a/b", CodeRootContexts.Normalize("\\a\\b\\"));
            Assert.AreEqual("", CodeRootContexts.Normalize(null));
            Assert.ThrowsException<ArgumentException>(() => CodeRootContexts.Normalize("a/../b"));
        }
    }
}
