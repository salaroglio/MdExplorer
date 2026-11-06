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
        public void LetMarkAgentReadAndWriteInsideALinkedCodeRootButNotNextToIt()
        {
            // radici di codice esterne F3: la radice collegata vale come il progetto, una cartella sorella no
            var backend = Path.Combine(Path.GetTempPath(), "clones", "backend");
            var roots = new[] { backend };
            Assert.IsTrue(CopilotSdkPermissionPolicy.Decide(Read(Path.Combine(backend, "Api", "Foo.cs")), Project, roots).Approved);
            Assert.IsTrue(CopilotSdkPermissionPolicy.Decide(Write(Path.Combine(backend, "Api", "Foo.cs")), Project, roots).Approved);
            Assert.IsFalse(CopilotSdkPermissionPolicy.Decide(Read(backend + "2" + Path.DirectorySeparatorChar + "x.cs"), Project, roots).Approved, "backend2 non è backend");
            Assert.IsFalse(CopilotSdkPermissionPolicy.Decide(Read(Path.Combine(backend, "..", "segreti.txt")), Project, roots).Approved);
            Assert.IsFalse(Decide(Read(Path.Combine(backend, "Api", "Foo.cs"))).Approved, "senza radici collegate resta fuori");
        }

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
            Assert.IsTrue(verdict.Reason.Contains("outside the project folder"));
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
