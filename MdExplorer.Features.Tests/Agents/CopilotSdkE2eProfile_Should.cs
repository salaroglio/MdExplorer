using System.IO;
using GitHub.Copilot;
using MdExplorer.Features.Services.AI.CopilotChat;
using MdExplorer.Features.Services.AI.CopilotSdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// The restrictions of a Copilot session that runs e2e tests (F4c, D29, reviews of 27/09/2026): the shell,
    /// the two dangerous Playwright tools, every credentials file, the agents' configuration. Without a profile
    /// the chat is what it was.
    /// </summary>
    [TestClass]
    public class CopilotSdkE2eProfile_Should
    {
        private static readonly string Project = Path.Combine(Path.GetTempPath(), "progetto");

        private static readonly CopilotSessionProfile Profile = new()
        {
            Key = "k",
            DenyShell = true,
            DeniedReadPaths = new[] { Path.Combine(Project, "test-e2e", "segreti-strani.txt") },
            DeniedReadNames = new[] { "credenziali-*.txt" },
            DeniedWritePaths = new[] { ".claude/", ".github/", "CLAUDE.md", "AGENTS.md", "opencode.json" },
            DeniedMcpTools = new[] { "browser_evaluate", "browser_run_code_unsafe" },
        };

        private static CopilotSdkPermissionPolicy.Verdict Decide(PermissionRequest request, CopilotSessionProfile profile = null)
            => CopilotSdkPermissionPolicy.Decide(request, Project, profile ?? Profile);

        private static PermissionRequestRead Read(string path) => new() { Path = path, Intention = "test" };
        private static PermissionRequestWrite Write(string fileName) => new() { FileName = fileName, Intention = "test", Diff = "", CanOfferSessionApproval = false };
        private static PermissionRequestMcp Tool(string tool) => new() { ServerName = "playwright", ToolName = tool, ToolTitle = tool, ReadOnly = false };
        private static PermissionRequestShell Shell(string command) => new()
        {
            FullCommandText = command, Intention = "test", CanOfferSessionApproval = false,
            HasWriteFileRedirection = false, Commands = new PermissionRequestShellCommand[0],
            PossiblePaths = new string[0], PossibleUrls = new PermissionRequestShellPossibleUrl[0]
        };

        [TestMethod]
        public void Refuse_every_write_and_the_shell_in_a_read_only_session_saying_why()
        {
            // The diagram sessions (sprint 2026-09-29-Motore-LLM-Unico, D8): changes go through the proposal the user
            // confirms; measured 29/09/2026: «permesso negato: scrivere …/scrittura-copilot.txt — la sessione dei diagrammi…».
            var readOnly = new CopilotSessionProfile { Key = "mark-diagram", DenyShell = true, DenyWrite = true, DenyReason = "sola lettura" };
            var write = Decide(Write("docs/qualunque.md"), readOnly);
            Assert.IsFalse(write.Approved);
            Assert.AreEqual("sola lettura", write.Reason);
            Assert.AreEqual("sola lettura", Decide(Shell("echo ciao"), readOnly).Reason);
            Assert.IsTrue(Decide(Read("docs/qualunque.md"), readOnly).Approved, "reading the project stays allowed");
            Assert.IsTrue(Decide(Write("docs/qualunque.md"), new CopilotSessionProfile { Key = "k", DenyShell = true }).Approved,
                "without DenyWrite a profile writes as before");
        }

        [TestMethod]
        public void Refuse_the_shell_during_a_test()
        {
            Assert.IsFalse(Decide(Shell("cat test-e2e/credenziali-x.txt")).Approved);
            Assert.IsTrue(CopilotSdkPermissionPolicy.Decide(Shell("echo ciao"), Project).Approved, "the chat keeps its shell");
        }

        [TestMethod]
        public void Refuse_the_playwright_tools_that_could_read_a_secret_back()
        {
            Assert.IsFalse(Decide(Tool("browser_evaluate")).Approved);
            Assert.IsFalse(Decide(Tool("browser_run_code_unsafe")).Approved);
            Assert.IsTrue(Decide(Tool("browser_type")).Approved);
        }

        [TestMethod]
        public void Refuse_every_credentials_file_by_name_and_the_listed_ones()
        {
            Assert.IsFalse(Decide(Read("test-e2e/credenziali-the-internet.txt")).Approved);
            Assert.IsFalse(Decide(Read("altra/cartella/CREDENZIALI-b.TXT")).Approved);
            Assert.IsFalse(Decide(Read(Path.Combine(Project, "test-e2e", "segreti-strani.txt"))).Approved);
            Assert.IsTrue(Decide(Read("test-e2e/login.e2e.md")).Approved);
        }

        [TestMethod]
        public void Refuse_writing_the_agents_configuration_even_in_subfolders()
        {
            Assert.IsFalse(Decide(Write(".claude/settings.json")).Approved);
            Assert.IsFalse(Decide(Write("sotto/CLAUDE.md")).Approved);
            Assert.IsFalse(Decide(Write("opencode.json")).Approved);
            Assert.IsTrue(Decide(Write("test-e2e/login.e2e/report.md")).Approved);
        }
    }
}
