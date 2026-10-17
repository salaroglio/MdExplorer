using System;
using System.IO;
using MdExplorer.Features.Services.Atlassian;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Atlassian
{
    /// <summary>
    /// Il resolver individua il file da allegare senza vincoli di posizione: qualunque
    /// percorso leggibile è ammesso. Restano da verificare la comodità del percorso
    /// relativo e gli errori azionabili sui casi malformati.
    /// </summary>
    [TestClass]
    public class AttachmentPathResolverShould
    {
        private string _root;
        private string _outside;

        [TestInitialize]
        public void Setup()
        {
            var baseDir = Path.Combine(Path.GetTempPath(), "mde-attach-" + Guid.NewGuid().ToString("N"));
            _root = Path.Combine(baseDir, "project");
            _outside = Path.Combine(baseDir, "outside");
            Directory.CreateDirectory(Path.Combine(_root, "docs"));
            Directory.CreateDirectory(_outside);
            File.WriteAllText(Path.Combine(_root, "docs", "report.pdf"), "inside");
            File.WriteAllText(Path.Combine(_outside, "server.log"), "outside");
        }

        [TestCleanup]
        public void Cleanup()
        {
            var baseDir = Directory.GetParent(_root)?.FullName;
            if (baseDir != null && Directory.Exists(baseDir))
                try { Directory.Delete(baseDir, recursive: true); } catch (IOException) { }
        }

        [TestMethod]
        public void ResolveAPathRelativeToTheProjectRoot()
        {
            var ok = AttachmentPathResolver.TryResolve(_root, Path.Combine("docs", "report.pdf"),
                                                       out var resolved, out var error);

            Assert.IsTrue(ok, error);
            Assert.IsNull(error);
            Assert.AreEqual(Path.Combine(Path.GetFullPath(_root), "docs", "report.pdf"), resolved);
        }

        [TestMethod]
        public void AcceptAnAbsolutePathInsideTheProject()
        {
            var absolute = Path.Combine(_root, "docs", "report.pdf");

            var ok = AttachmentPathResolver.TryResolve(_root, absolute, out var resolved, out var error);

            Assert.IsTrue(ok, error);
            Assert.AreEqual(Path.GetFullPath(absolute), resolved);
        }

        [TestMethod]
        public void AcceptAnAbsolutePathOutsideTheProject()
        {
            // Scelta esplicita: allegare un file fuori dal progetto (un log di sistema,
            // un export in /tmp) è un caso d'uso legittimo, non un errore.
            var outsideFile = Path.Combine(_outside, "server.log");

            var ok = AttachmentPathResolver.TryResolve(_root, outsideFile, out var resolved, out var error);

            Assert.IsTrue(ok, error);
            Assert.AreEqual(Path.GetFullPath(outsideFile), resolved);
        }

        [TestMethod]
        public void AcceptARelativePathThatClimbsOutOfTheProject()
        {
            var traversal = Path.Combine("..", "outside", "server.log");

            var ok = AttachmentPathResolver.TryResolve(_root, traversal, out var resolved, out var error);

            Assert.IsTrue(ok, error);
            StringAssert.Contains(resolved, "server.log");
        }

        [TestMethod]
        public void FollowASymlinkToItsTarget()
        {
            var link = Path.Combine(_root, "docs", "linked.log");
            try
            {
                File.CreateSymbolicLink(link, Path.Combine(_outside, "server.log"));
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException || ex is PlatformNotSupportedException)
            {
                Assert.Inconclusive("Questa piattaforma non consente di creare symlink senza privilegi: " + ex.Message);
                return;
            }

            var ok = AttachmentPathResolver.TryResolve(_root, Path.Combine("docs", "linked.log"),
                                                       out var resolved, out var error);

            Assert.IsTrue(ok, error);
            StringAssert.Contains(resolved, "linked.log");
        }

        [TestMethod]
        public void RefuseAFileThatDoesNotExist()
        {
            var ok = AttachmentPathResolver.TryResolve(_root, Path.Combine("docs", "nope.pdf"),
                                                       out var resolved, out var error);

            Assert.IsFalse(ok);
            Assert.IsNull(resolved);
            StringAssert.Contains(error, "not found");
        }

        [TestMethod]
        public void RefuseAFolder()
        {
            var ok = AttachmentPathResolver.TryResolve(_root, "docs", out var resolved, out var error);

            Assert.IsFalse(ok, "una cartella non è un allegato");
            Assert.IsNull(resolved);
            StringAssert.Contains(error, "folder");
        }

        [TestMethod]
        public void RefuseAnEmptyPath()
        {
            var ok = AttachmentPathResolver.TryResolve(_root, "   ", out var resolved, out var error);

            Assert.IsFalse(ok);
            Assert.IsNull(resolved);
            StringAssert.Contains(error, "filePath is required");
        }

        [TestMethod]
        public void RefuseARelativePathWhenTheProjectFolderIsMissing()
        {
            var ghost = Path.Combine(_root, "does-not-exist");

            var ok = AttachmentPathResolver.TryResolve(ghost, "docs/report.pdf", out var resolved, out var error);

            Assert.IsFalse(ok, "senza root non c'è modo di interpretare un percorso relativo");
            Assert.IsNull(resolved);
            StringAssert.Contains(error, "absolute path");
        }
    }
}
