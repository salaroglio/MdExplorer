using MdExplorer.Features.Services.AI.CopilotAcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// On Linux the launcher answered «installed» without looking and started a bare "copilot": a service whose PATH
    /// lacked nvm's folder failed with «No such file or directory» (29/09/2026). It now looks in the PATH.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class CopilotProcessLauncher_Should
    {
        private string _path;
        private string _dir;

        [TestInitialize]
        public void Setup()
        {
            if (OperatingSystem.IsWindows()) Assert.Inconclusive("POSIX behaviour.");
            _path = Environment.GetEnvironmentVariable("PATH");
            _dir = Path.Combine(Path.GetTempPath(), "copilot-path-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_dir == null) return;
            Environment.SetEnvironmentVariable("PATH", _path);
            Directory.Delete(_dir, true);
        }

        [TestMethod]
        public void Say_why_when_copilot_is_not_in_the_PATH()
        {
            Environment.SetEnvironmentVariable("PATH", _dir);

            Assert.IsFalse(CopilotProcessLauncher.IsResolvable());
            var ex = Assert.ThrowsException<InvalidOperationException>(() => CopilotProcessLauncher.BuildStartInfo("--version"));
            StringAssert.Contains(ex.Message, "PATH del servizio");
            Assert.ThrowsException<InvalidOperationException>(() => CopilotProcessLauncher.ResolveStdioTarget());
        }

        [TestMethod]
        public void Start_the_copilot_found_in_the_PATH_by_its_full_path()
        {
            var copilot = Path.Combine(_dir, "copilot");
            File.WriteAllText(copilot, "#!/bin/sh\n");
            Environment.SetEnvironmentVariable("PATH", "/nonexistent" + Path.PathSeparator + _dir);

            Assert.IsTrue(CopilotProcessLauncher.IsResolvable());
            Assert.AreEqual(copilot, CopilotProcessLauncher.BuildStartInfo("--version").FileName);
            Assert.AreEqual(copilot, CopilotProcessLauncher.ResolveStdioTarget().Path);
        }
    }
}
