using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2eEnvironment_Should
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "e2e-env-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_root, true);

        private static byte[] Tgz(params (string Name, string Content)[] entries)
        {
            var buffer = new MemoryStream();
            using (var gzip = new GZipStream(buffer, CompressionLevel.Fastest, leaveOpen: true))
            using (var writer = new TarWriter(gzip))
            {
                foreach (var (name, content) in entries)
                {
                    writer.WriteEntry(new PaxTarEntry(TarEntryType.RegularFile, name)
                    {
                        DataStream = new MemoryStream(Encoding.UTF8.GetBytes(content)),
                    });
                }
            }
            return buffer.ToArray();
        }

        [TestMethod]
        public void Accept_an_archive_only_when_it_matches_the_registry_sha512()
        {
            var bytes = Encoding.UTF8.GetBytes("archivio");
            var integrity = "sha512-" + Convert.ToBase64String(SHA512.HashData(bytes));

            E2eEnvironment.VerifyIntegrity(bytes, integrity, "p", "1.0.0");
            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                E2eEnvironment.VerifyIntegrity(Encoding.UTF8.GetBytes("manomesso"), integrity, "p", "1.0.0"));
            StringAssert.Contains(ex.Message, "non corrisponde all'impronta");
            Assert.ThrowsException<InvalidOperationException>(() =>
                E2eEnvironment.VerifyIntegrity(bytes, "sha1-abc", "p", "1.0.0"), "only sha512 is trusted");
        }

        [TestMethod]
        public void Extract_an_npm_archive_without_its_package_folder()
        {
            var target = Path.Combine(_root, "node_modules", "@playwright", "mcp");
            E2eEnvironment.ExtractPackage(Tgz(("package/cli.js", "// cli"), ("package/lib/a.js", "a")), target);

            Assert.AreEqual("// cli", File.ReadAllText(Path.Combine(target, "cli.js")));
            Assert.AreEqual("a", File.ReadAllText(Path.Combine(target, "lib", "a.js")));
        }

        [TestMethod]
        public void Refuse_an_archive_that_writes_outside_its_folder()
        {
            var target = Path.Combine(_root, "node_modules", "p");
            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                E2eEnvironment.ExtractPackage(Tgz(("package/../../../fuori.txt", "x")), target));
            StringAssert.Contains(ex.Message, "fuori dalla sua cartella");
            Assert.IsFalse(File.Exists(Path.Combine(_root, "fuori.txt")));
        }

        [TestMethod]
        public async Task Say_what_is_missing_and_what_to_do_when_nothing_is_there()
        {
            var environment = new E2eEnvironment(_root, _ => null);
            var report = await environment.CheckAsync();

            Assert.IsFalse(report.ReadyToRun);
            Assert.IsFalse(report.Electron.Ok);
            StringAssert.Contains(report.Electron.Remedy, E2eEnvironment.ElectronVariable);
            Assert.IsFalse(report.PlaywrightMcp.Ok);
            Assert.IsTrue(report.PlaywrightMcp.Installable);
            Assert.IsNull(report.PlaywrightMcpCli);
        }

        [TestMethod]
        public async Task Not_trust_an_electron_path_that_does_not_exist()
        {
            var environment = new E2eEnvironment(_root, name => name == E2eEnvironment.ElectronVariable ? Path.Combine(_root, "nessuno") : null);
            var report = await environment.CheckAsync();
            Assert.IsFalse(report.Electron.Ok);
            StringAssert.Contains(report.Electron.Detail, "non esiste");
        }

        [TestMethod]
        public void Prefer_edge_on_windows_and_chrome_elsewhere()
        {
            var first = new System.Collections.Generic.List<(string Argument, string Path)>(
                new E2eEnvironment(_root, _ => @"C:\x").InstalledBrowserCandidates())[0];
            Assert.AreEqual(OperatingSystem.IsWindows() ? "msedge" : "chrome", first.Argument);
        }
    }
}
