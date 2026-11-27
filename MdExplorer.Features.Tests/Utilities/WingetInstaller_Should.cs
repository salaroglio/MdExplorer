using System;
using System.Linq;
using MdExplorer.Features.Services.SetupGuide;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Utilities
{
    /// <summary>«Installa adesso» della pagina «Che cosa installare» (27/11/2026): le parti che non hanno bisogno di Windows.</summary>
    [TestClass]
    public class WingetInstaller_Should
    {
        [TestMethod]
        public void Install_the_exact_package_silently_and_accept_the_agreements()
        {
            CollectionAssert.AreEqual(
                new[] { "install", "--id", "Git.Git", "-e", "--silent", "--accept-package-agreements", "--accept-source-agreements" },
                WingetInstaller.Arguments("Git.Git").ToArray());
        }

        [TestMethod]
        public void Know_the_three_programs_with_the_java_version_mde_needs()
        {
            Assert.AreEqual("Git.Git", WingetInstaller.Packages["git"]);
            Assert.AreEqual("EclipseAdoptium.Temurin.21.JDK", WingetInstaller.Packages["Java"], "Java 21, il minimo di MDE");
            Assert.AreEqual("JohnMacFarlane.Pandoc", WingetInstaller.Packages["pandoc"]);
            Assert.AreEqual(-1978335135, WingetInstaller.AlreadyInstalledExit, "0x8A150061");
        }

        [TestMethod]
        public void Show_the_progress_and_skip_the_spinner()
        {
            Assert.IsNull(WingetInstaller.Meaningful("   "));
            Assert.IsNull(WingetInstaller.Meaningful(" - "));
            Assert.IsNull(WingetInstaller.Meaningful("\\"));
            Assert.AreEqual("██████  45.0 MB / 60.0 MB", WingetInstaller.Meaningful("  ██████  45.0 MB / 60.0 MB "));
            Assert.AreEqual("Successfully installed", WingetInstaller.Meaningful("Successfully installed\r"));
        }

        [TestMethod]
        public void Refuse_outside_windows_with_the_reason()
        {
            if (OperatingSystem.IsWindows()) Assert.Inconclusive("su Windows parte davvero");
            var installer = new WingetInstaller(NullLogger<WingetInstaller>.Instance);
            var ex = Assert.ThrowsExactly<InvalidOperationException>(() => installer.Start("git"));
            Assert.IsTrue(ex.Message.Length > 0);
            Assert.IsNull(installer.Status("git"), "niente partito, niente stato");
            Assert.IsNull(WingetInstaller.FindWinget());
        }
    }
}
