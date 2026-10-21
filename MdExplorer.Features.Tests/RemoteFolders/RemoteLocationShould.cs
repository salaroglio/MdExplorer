using System;
using MdExplorer.Features.Services.RemoteFolders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.RemoteFolders
{
    /// <summary>Sprint Cartella-Remota-Via-SSH, F2: la posizione <c>ssh://host/percorso</c> nello UserDB.</summary>
    [TestClass]
    public class RemoteLocationShould
    {
        [TestMethod]
        public void Parse_hosts_as_written_after_ssh_and_normalize_the_path()
        {
            var alias = RemoteLocation.Parse("ssh://lab-dev/srv/proj/");
            Assert.AreEqual("lab-dev", alias.Host);
            Assert.AreEqual("/srv/proj", alias.Path);
            Assert.AreEqual("lab-dev", alias.Destination);
            Assert.AreEqual("ssh://lab-dev/srv/proj", alias.ToString());

            var withUser = RemoteLocation.Parse("ssh://dev@10.0.0.5//srv/./a/../proj");
            Assert.AreEqual("dev@10.0.0.5", withUser.Destination);
            Assert.AreEqual("/srv/proj", withUser.Path);

            var withPort = RemoteLocation.Parse("ssh://dev@build.acme.it:2222/home/dev");
            Assert.AreEqual("ssh://dev@build.acme.it:2222", withPort.Destination, "con la porta la forma URI di OpenSSH");
            Assert.AreEqual("/", RemoteLocation.Parse("ssh://server").Path);
        }

        [TestMethod]
        public void Refuse_hosts_that_would_become_ssh_options_or_shell_text()
        {
            foreach (var bad in new[] { "ssh://-oProxyCommand=evil/x", "ssh://a b/x", "ssh://a;rm/x", "ssh:///x", "ssh://dev@/x", "ssh://$(id)/x", "ssh://a:99999999/x" })
                Assert.ThrowsException<FormatException>(() => RemoteLocation.Parse(bad), bad);
            Assert.ThrowsException<FormatException>(() => RemoteLocation.Parse("/srv/proj"), "senza ssh:// non è remota");
            Assert.ThrowsException<FormatException>(() => RemoteLocation.Parse("ssh://h/../etc"), "sopra la radice del server");
            Assert.IsFalse(RemoteLocation.IsRemote(@"C:\src"));
            Assert.IsTrue(RemoteLocation.IsRemote("ssh://h/x"));
        }

        [TestMethod]
        public void Combine_inside_and_refuse_escapes()
        {
            var root = RemoteLocation.Parse("ssh://h/srv/proj");
            Assert.AreEqual("ssh://h/srv/proj/src/a.cs", root.Combine("src/a.cs").ToString());
            Assert.AreSame(root, root.Combine(""));
            Assert.AreEqual("src/a.cs", root.Combine("src/a.cs").RelativeTo(root));
            Assert.AreEqual("", root.RelativeTo(root));
            Assert.IsNull(RemoteLocation.Parse("ssh://h/srv/proj2").RelativeTo(root), "/proj2 non è dentro /proj");
            Assert.IsNull(RemoteLocation.Parse("ssh://altro/srv/proj/x").RelativeTo(root), "un altro server");
            Assert.ThrowsException<InvalidOperationException>(() => root.Combine("../fuori"));
            Assert.ThrowsException<FormatException>(() => root.Combine("/etc/passwd"));
            Assert.AreEqual("srv/proj", root.RelativeTo(RemoteLocation.Parse("ssh://h/")));
        }
    }
}
