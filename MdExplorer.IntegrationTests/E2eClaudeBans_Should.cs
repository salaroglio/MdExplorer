using System.IO;
using System.Linq;
using MdExplorer.Services.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// The Claude Code rules of an e2e launch (F4b, reviews of 27/09/2026). The rule forms were verified against
    /// the real CLI on Linux (Read/Grep negated, Write in .claude/ negated, the two tools gone from the session).
    /// </summary>
    [TestClass]
    public class E2eClaudeBans_Should
    {
        private static readonly string Project = Path.Combine(Path.GetTempPath(), "progetto-e2e");

        [TestMethod]
        public void Ban_the_shell_the_credentials_the_agents_configuration_and_two_playwright_tools()
        {
            var bans = E2eLaunchService.BannedFor(Project, new[]
            {
                Path.Combine(Project, "test-e2e", "credenziali-x.txt"),
                Path.Combine(Path.GetTempPath(), "segreti", "lancio.env"),
            });

            CollectionAssert.IsSubsetOf(new[]
            {
                "Bash",
                "Read(./**/credenziali-*.txt)", "Grep(./**/credenziali-*.txt)",
                "Read(./test-e2e/credenziali-x.txt)", "Grep(./test-e2e/credenziali-x.txt)",
                "Edit(./.claude/**)", "Write(./.claude/**)", "Edit(./**/CLAUDE.md)", "Write(./**/AGENTS.md)",
                "mcp__playwright__browser_evaluate", "mcp__playwright__browser_run_code_unsafe",
            }, bans.ToArray());
            Assert.IsTrue(bans.Any(b => b.StartsWith("Read(//") && b.EndsWith("segreti/lancio.env)")), "a file outside the project: //path");
        }
    }
}
