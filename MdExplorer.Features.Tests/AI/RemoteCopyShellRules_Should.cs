using System.Linq;
using System.Text.Json.Nodes;
using MdExplorer.Features.Services.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// Lo stesso divieto di ssh/scp/sftp/rsync per i tre motori (26/11/2026): i file delle radici remote passano da MDE Remote,
    /// che tiene la codifica. Le forme sono quelle provate dal vivo su Copilot CLI 1.0.114, Claude Code 2.1.332 e opencode 1.18.30.
    /// </summary>
    [TestClass]
    public class RemoteCopyShellRules_Should
    {
        [TestMethod]
        public void Give_each_engine_the_same_ban_in_its_own_form()
        {
            CollectionAssert.AreEqual(new[] { "ssh", "scp", "sftp", "rsync" }, RemoteCopyShellRules.Commands.ToArray());
            CollectionAssert.AreEqual(new[] { "shell(ssh)", "shell(scp)", "shell(sftp)", "shell(rsync)" }, RemoteCopyShellRules.Copilot.ToArray());
            CollectionAssert.AreEqual(new[] { "Bash(ssh:*)", "Bash(scp:*)", "Bash(sftp:*)", "Bash(rsync:*)" }, RemoteCopyShellRules.Claude.ToArray());
        }

        [TestMethod]
        public void Add_only_the_bans_to_the_opencode_chat_so_the_user_configuration_stays()
        {
            var bash = JsonNode.Parse(RemoteCopyShellRules.OpenCodeChatConfig())!["permission"]!["bash"]!.AsObject();
            Assert.IsFalse(bash.ContainsKey("*"), "nessuna regola generale: quella dell'utente resta");
            foreach (var command in RemoteCopyShellRules.Commands)
            {
                Assert.AreEqual("deny", bash[command]!.GetValue<string>(), command);
                Assert.AreEqual("deny", bash[command + " *"]!.GetValue<string>(), command + " *");
            }
        }
    }
}
