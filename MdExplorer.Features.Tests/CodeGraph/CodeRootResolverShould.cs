using System;
using System.IO;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>Radici di codice esterne, F1: la traduzione fra percorsi logici e percorsi di questa macchina.</summary>
    [TestClass]
    public class CodeRootResolverShould
    {
        private static readonly string Base = Path.Combine(Path.GetTempPath(), "mde-resolver");
        private static readonly string Project = Path.Combine(Base, "doc");
        private static readonly string Backend = Path.Combine(Base, "clones", "backend", "src");

        private static CodeRootResolver Resolver() => new(Project, new[]
        {
            new CodeRootState { Alias = "backend", Status = CodeRootStatus.Linked, RootPath = Backend },
            new CodeRootState { Alias = "frontend", Status = CodeRootStatus.NotLinked, Reason = "su questa macchina non è collegata" },
        });

        [TestMethod]
        public void Resolve_logical_paths_inside_a_linked_root()
        {
            Assert.AreEqual(Path.Combine(Backend, "Api", "Foo.cs"), Resolver().Resolve("@backend/Api/Foo.cs"));
            Assert.AreEqual(Backend, Resolver().Resolve("@backend"));
        }

        [TestMethod]
        public void Resolve_project_relative_paths_as_today()
        {
            Assert.AreEqual(Path.Combine(Project, "program", "x.ps1"), Resolver().Resolve("program/x.ps1"));
        }

        [TestMethod]
        public void Refuse_escapes_unknown_and_unlinked_roots_with_the_reason()
        {
            var r = Resolver();
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => r.Resolve("@backend/../../segreti.txt")).Message, "esce dalla radice");
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => r.Resolve("../fuori.md")).Message, "esce dal progetto");
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => r.Resolve("@nessuna/x")).Message, "non è dichiarata");
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => r.Resolve("@frontend/x")).Message, "non è collegata");
        }

        [TestMethod]
        public void Turn_a_machine_path_back_into_its_logical_path()
        {
            var r = Resolver();
            Assert.AreEqual("@backend/Api/Foo.cs", r.ToLogical(Path.Combine(Backend, "Api", "Foo.cs")));
            Assert.AreEqual("program/x.ps1", r.ToLogical(Path.Combine(Project, "program", "x.ps1")));
            Assert.IsNull(r.ToLogical(Path.Combine(Base, "altrove", "y.cs")));
            Assert.IsNull(r.ToLogical(Path.Combine(Base, "clones", "backend", "README.md")), "fuori dalla sottocartella dichiarata");
        }
    }
}
