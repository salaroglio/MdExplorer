using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.RemoteProtocol;
using MdExplorer.Services.RemoteFolders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// FL (sprint 2026-10-21-Cartella-Remota-Via-SSH, D8): una radice esterna LOCALE si modifica dall'albero con le stesse
    /// regole del serverino (dentro la radice, impronta per scrivere, mai sopra un file esistente).
    /// </summary>
    [TestClass]
    public class LocalCodeRootFiles_Should
    {
        private string _root;
        private CodeRootState _state;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-localroot", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "src"));
            File.WriteAllText(Path.Combine(_root, "src", "a.cs"), "class A {}\n");
            _state = new CodeRootState { Alias = "codice", Status = CodeRootStatus.Linked, RootPath = _root };
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private RemoteResponse Op(string op, string path, string text = null, string stamp = null, string to = null, bool recursive = false)
            => RemoteCodeRootFiles.OperateLocal(_state, new RemoteRequest { Op = op, Path = path, Text = text, Stamp = stamp, To = to, Recursive = recursive });

        [TestMethod]
        public void Create_edit_rename_and_delete_on_the_disk()
        {
            var created = Op(RemoteOps.Create, "docs/nota.md", "# Nota\n");
            Assert.AreEqual("# Nota\n", File.ReadAllText(Path.Combine(_root, "docs", "nota.md")));
            var read = Op(RemoteOps.Read, "docs/nota.md");
            Assert.AreEqual(created.Stamp, read.Stamp);
            Op(RemoteOps.Write, "docs/nota.md", "# Nota cambiata\n", read.Stamp);
            Assert.AreEqual("# Nota cambiata\n", File.ReadAllText(Path.Combine(_root, "docs", "nota.md")));
            Op(RemoteOps.Move, "docs/nota.md", to: "docs/rinominata.md");
            Assert.IsTrue(File.Exists(Path.Combine(_root, "docs", "rinominata.md")));
            Op(RemoteOps.Delete, "docs", recursive: true);
            Assert.IsFalse(Directory.Exists(Path.Combine(_root, "docs")));
        }

        [TestMethod]
        public void Refuse_with_the_code_like_the_agent()
        {
            var stale = Op(RemoteOps.Read, "src/a.cs").Stamp;
            File.AppendAllText(Path.Combine(_root, "src", "a.cs"), "// cambiato da un altro\n");
            Assert.AreEqual(RemoteErrorCodes.Changed, Assert.ThrowsException<RemoteOperationException>(() => Op(RemoteOps.Write, "src/a.cs", "x", stale)).Code);
            Assert.AreEqual(RemoteErrorCodes.Exists, Assert.ThrowsException<RemoteOperationException>(() => Op(RemoteOps.Create, "src/a.cs", "x")).Code);
            Assert.AreEqual(RemoteErrorCodes.OutsideRoot, Assert.ThrowsException<RemoteOperationException>(() => Op(RemoteOps.Read, "../fuori.txt")).Code);
            Assert.AreEqual(RemoteErrorCodes.NotEmpty, Assert.ThrowsException<RemoteOperationException>(() => Op(RemoteOps.Delete, "src")).Code);
        }
    }
}
