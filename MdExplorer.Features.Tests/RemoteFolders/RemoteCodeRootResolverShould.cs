using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.RemoteFolders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.RemoteFolders
{
    /// <summary>Sprint Cartella-Remota-Via-SSH, F2: una radice remota è collegata ma non ha un percorso su questo disco.</summary>
    [TestClass]
    public class RemoteCodeRootResolverShould
    {
        private static CodeRootResolver Resolver() => new(Path.Combine(Path.GetTempPath(), "doc"), new[]
        {
            new CodeRootState { Alias = "local", Status = CodeRootStatus.Linked, RootPath = Path.Combine(Path.GetTempPath(), "code") },
            new CodeRootState { Alias = "srv", Status = CodeRootStatus.Linked, Remote = RemoteLocation.Parse("ssh://lab-dev/srv/proj/src") },
            new CodeRootState { Alias = "down", Status = CodeRootStatus.Unreachable, Reason = "il server non risponde", Remote = null },
        });

        [TestMethod]
        public void Keep_remote_roots_out_of_the_disk_lists()
        {
            var r = Resolver();
            CollectionAssert.AreEqual(new[] { "local" }, r.Linked.Select(x => x.Alias).ToArray(), "MarkAgent --add-dir e analisi: solo su disco");
            CollectionAssert.AreEqual(new[] { "srv" }, r.LinkedRemote.Select(x => x.Alias).ToArray());
            Assert.IsNull(r.ToLogical("/srv/proj/src/a.cs"), "un percorso del server non è un file di questa macchina");
        }

        [TestMethod]
        public void Refuse_a_disk_path_for_a_remote_root_but_give_the_remote_relative_path()
        {
            var r = Resolver();
            var ex = Assert.ThrowsException<InvalidOperationException>(() => r.Resolve("@srv/a.cs"));
            StringAssert.Contains(ex.Message, "ssh://lab-dev/srv/proj/src");
            var remote = r.RemoteOf("@srv/api/a.cs");
            Assert.IsNotNull(remote);
            Assert.AreEqual("srv", remote.Value.Root.Alias);
            Assert.AreEqual("api/a.cs", remote.Value.Relative);
            Assert.AreEqual("", r.RemoteOf("@srv").Value.Relative);
            Assert.IsNull(r.RemoteOf("@local/a.cs"), "una radice locale si risolve su disco");
            Assert.IsNull(r.RemoteOf("docs/a.md"), "un percorso del progetto");
            Assert.ThrowsException<InvalidOperationException>(() => r.RemoteOf("@srv/../../etc/passwd"));
        }

        [TestMethod]
        public void Give_root_and_relative_path_of_a_local_root_for_file_operations()
        {
            var r = Resolver();
            var local = r.LocalOf("@local/src/a.cs");
            Assert.AreEqual("local", local.Value.Root.Alias);
            Assert.AreEqual("src/a.cs", local.Value.Relative);
            Assert.AreEqual("", r.LocalOf("@local").Value.Relative);
            Assert.IsNull(r.LocalOf("@srv/a.cs"), "una radice remota passa da RemoteOf");
            Assert.ThrowsException<InvalidOperationException>(() => r.LocalOf("@local/../../fuori.txt"));
            Assert.ThrowsException<InvalidOperationException>(() => r.LocalOf("@down/a.cs"), "non utilizzabile qui");
        }
    }
}
