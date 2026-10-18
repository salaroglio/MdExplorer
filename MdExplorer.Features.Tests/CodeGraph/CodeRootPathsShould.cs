using System;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>Radici di codice esterne, F1: alias, percorsi logici, confronto dei remote.</summary>
    [TestClass]
    public class CodeRootPathsShould
    {
        [TestMethod]
        public void Accept_only_aliases_that_fit_a_path_and_an_iri()
        {
            Assert.IsTrue(CodeRootPaths.IsValidAlias("backend"));
            Assert.IsTrue(CodeRootPaths.IsValidAlias("cobol-tools.v2"));
            Assert.IsFalse(CodeRootPaths.IsValidAlias("Backend"), "maiuscole no: Windows e Linux le tratterebbero diversamente");
            Assert.IsFalse(CodeRootPaths.IsValidAlias("my backend"));
            Assert.IsFalse(CodeRootPaths.IsValidAlias("-x"));
            Assert.IsFalse(CodeRootPaths.IsValidAlias("a/b"));
            Assert.IsFalse(CodeRootPaths.IsValidAlias(""));
            Assert.ThrowsException<ArgumentException>(() => CodeRootPaths.RequireAlias("Su Su"));
        }

        [TestMethod]
        public void Parse_and_format_logical_paths()
        {
            Assert.AreEqual(("backend", "src/Foo.cs"), CodeRootPaths.Parse("@backend/src/Foo.cs"));
            Assert.AreEqual(("backend", ""), CodeRootPaths.Parse("@backend"));
            Assert.AreEqual(("backend", "src/Foo.cs"), CodeRootPaths.Parse(@"@backend\src\Foo.cs"));
            Assert.AreEqual("@backend/src/Foo.cs", CodeRootPaths.Format("backend", @"src\Foo.cs"));
            Assert.AreEqual("@backend", CodeRootPaths.Format("backend", ""));
            Assert.IsTrue(CodeRootPaths.IsLogical("@backend/x"));
            Assert.IsFalse(CodeRootPaths.IsLogical("program/x.cs"));
            Assert.ThrowsException<FormatException>(() => CodeRootPaths.Parse("program/x.cs"));
            Assert.ThrowsException<FormatException>(() => CodeRootPaths.Parse("@Back End/x"));
        }

        [TestMethod]
        public void Recognize_the_same_repository_through_its_different_urls()
        {
            const string id = "github.com/acme/backend";
            Assert.AreEqual(id, CodeRootPaths.NormalizeRemote("https://github.com/acme/backend.git"));
            Assert.AreEqual(id, CodeRootPaths.NormalizeRemote("https://carlo@github.com/Acme/Backend"));
            Assert.AreEqual(id, CodeRootPaths.NormalizeRemote("git@github.com:acme/backend.git"));
            Assert.AreEqual(id, CodeRootPaths.NormalizeRemote("ssh://git@github.com:22/acme/backend.git/"));
            Assert.IsTrue(CodeRootPaths.SameRepository("git@github.com:acme/backend.git", "https://github.com/acme/backend"));
            Assert.IsFalse(CodeRootPaths.SameRepository("git@github.com:acme/backend.git", "https://github.com/acme/frontend"));
            Assert.IsFalse(CodeRootPaths.SameRepository(null, null), "niente remote: niente identità");
            Assert.AreEqual("dev.azure.com/org/proj/_git/repo", CodeRootPaths.NormalizeRemote("https://org@dev.azure.com/org/proj/_git/repo"));
        }

        [TestMethod]
        public void Keep_local_paths_as_paths_not_hosts()
        {
            Assert.AreEqual("file:c:/repos/backend", CodeRootPaths.NormalizeRemote(@"C:\repos\backend"));
            Assert.AreEqual("file:/srv/git/backend.git", CodeRootPaths.NormalizeRemote("/srv/git/backend.git"));
            Assert.AreEqual("file:/srv/git/backend.git", CodeRootPaths.NormalizeRemote("file:///srv/git/backend.git"));
        }
    }
}
