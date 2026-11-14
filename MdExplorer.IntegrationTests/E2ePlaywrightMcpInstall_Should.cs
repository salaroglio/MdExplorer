using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// The real installation of @playwright/mcp from the npm registry (network, about 19 MB). On Windows the old
    /// installer failed renaming its staging folder («Access to the path is denied», 28/09/2026): the files now go
    /// straight into the final folder, with the marker written last.
    /// </summary>
    [TestClass]
    public class E2ePlaywrightMcpInstall_Should
    {
        [TestMethod]
        public async Task Install_into_the_final_folder_and_clear_what_a_failed_attempt_left()
        {
            var root = Path.Combine(Path.GetTempPath(), "e2e-install-" + Guid.NewGuid().ToString("N"));
            try
            {
                var environment = new E2eEnvironment(root, _ => null);
                var parent = Path.GetDirectoryName(environment.PlaywrightMcpFolder)!;
                // What a failed attempt leaves: a final folder without marker, a staging folder of the old installer.
                Directory.CreateDirectory(Path.Combine(environment.PlaywrightMcpFolder, "node_modules", "resto"));
                Directory.CreateDirectory(Path.Combine(parent, E2eEnvironment.PlaywrightMcpVersion + ".download-1234abcd", "node_modules"));
                Assert.IsFalse((await environment.CheckAsync()).PlaywrightMcp.Ok, "a folder without marker is not an installation");

                using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
                var installed = await environment.InstallPlaywrightMcpAsync(http);

                CollectionAssert.Contains(installed.ToList(), "@playwright/mcp@" + E2eEnvironment.PlaywrightMcpVersion);
                Assert.IsTrue((await environment.CheckAsync()).PlaywrightMcp.Ok);
                Assert.IsTrue(File.Exists(environment.PlaywrightMcpCli));
                Assert.IsFalse(Directory.Exists(Path.Combine(environment.PlaywrightMcpFolder, "node_modules", "resto")), "the debris is gone");
                CollectionAssert.AreEqual(new[] { E2eEnvironment.PlaywrightMcpVersion },
                    Directory.GetDirectories(parent).Select(Path.GetFileName).ToArray(), "no staging folder left");
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
