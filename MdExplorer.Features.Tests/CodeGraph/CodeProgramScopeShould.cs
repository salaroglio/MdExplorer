using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    [TestClass]
    public class CodeProgramScopeShould
    {
        private static CodeScript Script()
        {
            var s = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = "x/Main.java" } };
            s.Symbols.Add(new ScriptSymbol { Id = "cls", Kind = "class", Name = "Main", QualifiedName = "com.acme.Main" });
            s.Symbols.Add(new ScriptSymbol { Id = "m", Kind = "method", Name = "run", QualifiedName = "com.acme.Main.run", ParentId = "cls" });
            s.Symbols.Add(new ScriptSymbol { Id = "ext", Kind = "external", Name = "println", QualifiedName = "java.io.PrintStream.println" });
            s.Parameters.Add(new ScriptParameter { SymbolId = "m", Position = 0, Name = "args" });
            s.Edges.Add(new ScriptEdge { SourceId = "m", TargetId = "ext", Relation = "calls", Confidence = "extracted" });
            s.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "m", Summary = "x" });
            return s;
        }

        [TestMethod]
        public void Give_the_same_qualified_name_two_ids_in_two_programs()
        {
            // 14/10/2026: com.acme.Main in due cartelle program dava lo stesso IRI e le triple si fondevano.
            var a = Script(); CodeProgramScope.Apply(a, "programA");
            var b = Script(); CodeProgramScope.Apply(b, "programB");
            Assert.AreNotEqual(a.Symbols[0].Id, b.Symbols[0].Id);
            Assert.AreNotEqual(a.Symbols[2].Id, b.Symbols[2].Id, "anche gli esterni sono del programma: i ponti li fa il build (3b)");
        }

        [TestMethod]
        public void Rewrite_every_reference_consistently()
        {
            var s = Script(); CodeProgramScope.Apply(s, "@backend/src");
            var cls = s.Symbols[0].Id; var m = s.Symbols[1].Id; var ext = s.Symbols[2].Id;
            Assert.AreEqual(cls, s.Symbols[1].ParentId);
            Assert.AreEqual(m, s.Parameters.Single().SymbolId);
            Assert.AreEqual(m, s.Edges.Single().SourceId); Assert.AreEqual(ext, s.Edges.Single().TargetId);
            Assert.AreEqual(m, s.SymbolDocs.Single().SymbolId);
            Assert.IsNull(s.Symbols[0].ParentId);
            Assert.AreEqual(24, cls.Length);
        }

        [TestMethod]
        public void Be_stable_across_runs()
        {
            Assert.AreEqual(CodeProgramScope.ScopedId("program", "abc"), CodeProgramScope.ScopedId("program", "abc"));
            Assert.AreNotEqual(CodeProgramScope.ScopedId("program", "abc"), CodeProgramScope.ScopedId("program2", "abc"));
        }
    }
}
