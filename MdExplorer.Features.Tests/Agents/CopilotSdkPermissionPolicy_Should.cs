using System.IO;
using GitHub.Copilot;
using MdExplorer.Features.Services.AI.CopilotSdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// The chat on the SDK must be able to do what it did on ACP (--allow-all-tools) and no more:
    /// tools yes, reading and writing only inside the project, no web.
    /// </summary>
    [TestClass]
    public class CopilotSdkPermissionPolicy_Should
    {
        private static readonly string Project = Path.Combine(Path.GetTempPath(), "progetto");

        private static CopilotSdkPermissionPolicy.Verdict Decide(PermissionRequest request)
            => CopilotSdkPermissionPolicy.Decide(request, Project);

        // The SDK request types have `required` members: builders keep the tests about the one
        // field each case is really about.
        private static PermissionRequestRead Read(string path)
            => new PermissionRequestRead { Path = path, Intention = "test" };

        private static PermissionRequestWrite Write(string fileName)
            => new PermissionRequestWrite { FileName = fileName, Intention = "test", Diff = "", CanOfferSessionApproval = false };

        private static PermissionRequestShell Shell(string command)
            => new PermissionRequestShell
            {
                FullCommandText = command, Intention = "test", CanOfferSessionApproval = false,
                HasWriteFileRedirection = false, Commands = new PermissionRequestShellCommand[0],
                PossiblePaths = new string[0], PossibleUrls = new PermissionRequestShellPossibleUrl[0]
            };

        [TestMethod]
        public void LetCopilotReadAFileOfTheProject()
        {
            // The case that was refused: without a handler the SDK denied even this.
            Assert.IsTrue(Decide(Read(Path.Combine(Project, "CLAUDE.md"))).Approved);
        }

        [TestMethod]
        public void ResolveARelativePathAgainstTheProject()
        {
            Assert.IsTrue(Decide(Read("docs/nota.md")).Approved);
        }

        [TestMethod]
        public void RefuseToReadOutsideTheProject()
        {
            var verdict = Decide(Read("/etc/passwd"));

            Assert.IsFalse(verdict.Approved);
            Assert.IsTrue(verdict.Reason.Contains("fuori dalla cartella del progetto"));
        }

        [TestMethod]
        public void RefuseToEscapeWithDotDot()
        {
            Assert.IsFalse(Decide(Read("../altro/segreto.txt")).Approved);
        }

        [TestMethod]
        public void RefuseASiblingFolderWhoseNameStartsLikeTheProject()
        {
            // "/tmp/progetto-vecchio" starts with "/tmp/progetto" as a string, and is not inside it.
            Assert.IsFalse(Decide(Read(Project + "-vecchio/x.md")).Approved);
        }

        [TestMethod]
        public void LetCopilotWriteInsideTheProjectOnly()
        {
            Assert.IsTrue(Decide(Write(Path.Combine(Project, "nuovo.md"))).Approved);
            Assert.IsFalse(Decide(Write("/tmp/fuori.md")).Approved);
        }

        [TestMethod]
        public void LetCopilotRunShellCommandsAsWithAllowAllTools()
        {
            Assert.IsTrue(Decide(Shell("ls -la")).Approved);
        }

        [TestMethod]
        public void LetCopilotUseMcpAndCustomTools()
        {
            Assert.IsTrue(Decide(new PermissionRequestMcp { ServerName = "mdexplorer", ToolName = "SearchDocuments", ToolTitle = "Search", ReadOnly = true }).Approved);
            Assert.IsTrue(Decide(new PermissionRequestCustomTool { ToolName = "qualcosa", ToolDescription = "test" }).Approved);
        }

        [TestMethod]
        public void RefuseTheWeb()
        {
            // Not covered by --allow-all-tools: granting it would widen what the chat could do.
            var verdict = Decide(new PermissionRequestUrl { Url = "https://example.com", Intention = "test" });

            Assert.IsFalse(verdict.Approved);
            Assert.IsNotNull(verdict.Reason, "a refusal must say why");
        }
    }
}
