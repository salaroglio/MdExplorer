using System.Collections.Generic;
using System.IO;
using MdExplorer.Features.Services.AI;
using MdExplorer.Features.Services.AI.Codex;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRun;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Come MdExplorer risponde alle richieste di approvazione di Codex nella chat e negli agenti, e come si sceglie Codex per
    /// un agente. Sprint docs-internal/Sprints/2026-11-28-Motore-Codex.md, F2 e F4.
    /// </summary>
    [TestClass]
    public class CodexRules_Should
    {
        private static CodexApprovalRequest Command(string line) => new() { Kind = CodexApprovalRequest.KindCommand, Command = line };
        private static CodexApprovalRequest Change(params string[] paths) => new() { Kind = CodexApprovalRequest.KindFileChange, Paths = paths };

        [TestMethod]
        public void Recognise_a_remote_copy_only_as_a_command()
        {
            Assert.IsTrue(RemoteCopyShellRules.IsRemoteCopy("/bin/bash -lc \"scp a.txt host:/tmp\""));
            Assert.IsTrue(RemoteCopyShellRules.IsRemoteCopy("/bin/bash -lc 'ls && ssh host ls'"));
            Assert.IsTrue(RemoteCopyShellRules.IsRemoteCopy("/usr/bin/rsync -a x y"));
            Assert.IsFalse(RemoteCopyShellRules.IsRemoteCopy("/bin/bash -lc \"cat docs/scp-notes.md\""));
            Assert.IsFalse(RemoteCopyShellRules.IsRemoteCopy("/bin/bash -lc \"grep -r sshd config\""));
            Assert.IsFalse(RemoteCopyShellRules.IsRemoteCopy(null));
        }

        [TestMethod]
        public void Let_the_chat_work_but_not_copy_remotely_nor_write_in_a_code_root()
        {
            var root = Path.Combine(Path.GetTempPath(), "radice-codice");
            var approve = CodexChat.ApproveForChat(new[] { root });

            Assert.IsTrue(approve(Command("/bin/bash -lc \"ls\"")).Accept);
            Assert.IsTrue(approve(Change(Path.Combine(Path.GetTempPath(), "progetto", "nota.md"))).Accept);
            Assert.IsFalse(approve(Command("/bin/bash -lc \"scp a b\"")).Accept);
            var inRoot = approve(Change(Path.Combine(root, "src", "a.cs")));
            Assert.IsFalse(inRoot.Accept);
            StringAssert.Contains(inRoot.Reason, "remote_edit");
        }

        [TestMethod]
        public void Give_an_agent_only_what_its_card_declares()
        {
            var readOnly = CodexTurnRunner.ApproveFor(new AgentTurnRequest { DeclaredTools = new List<string> { "read" } });
            Assert.IsFalse(readOnly(Command("/bin/bash -lc \"ls\"")).Accept, "niente shell senza tools: shell");
            Assert.IsFalse(readOnly(Change("/p/a.md")).Accept, "niente scrittura senza tools: write");

            var all = CodexTurnRunner.ApproveFor(new AgentTurnRequest { DeclaredTools = null });
            Assert.IsTrue(all(Command("/bin/bash -lc \"git commit -m x\"")).Accept, "senza manifesto, tutto come gli altri motori");
            Assert.IsTrue(all(Change("/p/a.md")).Accept);
            Assert.IsFalse(all(Command("/bin/bash -lc \"scp a b\"")).Accept, "ssh/scp mai");
        }

        [TestMethod]
        public void Choose_codex_for_an_agent_that_asks_for_it()
        {
            Assert.IsTrue(AgentEngineChoice.TryParseProvider("codex", out var engine));
            Assert.AreEqual(MarkAgentEngine.Codex, engine);
            Assert.IsTrue(AgentEngineChoice.TryParseProvider("openai-codex", out engine));
            Assert.AreEqual(MarkAgentEngine.Codex, engine);

            var choice = AgentEngineChoice.Resolve("codex", null, MarkAgentEngine.Claude, "sonnet", out var error);
            Assert.IsNull(error);
            Assert.AreEqual(MarkAgentEngine.Codex, choice.Engine);
            Assert.IsNull(choice.Model, "un altro motore senza modello: il predefinito di Codex");
        }
    }
}
