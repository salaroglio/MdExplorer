using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.RemoteAgent
{
    /// <summary>
    /// MDE Remote sta su Git già compilato (<c>MdExplorer/Binaries/remote/linux-x64/mde-remote</c>): se i suoi sorgenti
    /// cambiano senza ricompilarlo con <c>publish-linux-x64.sh</c>, i server ricevono il binario vecchio. L'impronta dei
    /// sorgenti scritta dallo script deve essere quella dei sorgenti di adesso.
    /// </summary>
    [TestClass]
    public class RemoteAgentBinaryShould
    {
        [TestMethod]
        public void Be_compiled_from_the_current_sources()
        {
            var repo = RepoRoot();
            var folder = Path.Combine(repo, "MdExplorer", "Binaries", "remote", "linux-x64");
            Assert.IsTrue(File.Exists(Path.Combine(folder, "mde-remote")), "manca MDE Remote compilato: MdExplorer.RemoteAgent/publish-linux-x64.sh");
            var recorded = File.ReadAllText(Path.Combine(folder, "mde-remote.sources-sha256")).Trim();
            Assert.AreEqual(recorded, SourcesFingerprint(repo),
                "i sorgenti di MdExplorer.RemoteAgent / MdExplorer.Remote.Protocol sono cambiati: ricompila con MdExplorer.RemoteAgent/publish-linux-x64.sh e committa il binario");
        }

        /// <summary>La stessa ricetta dello script: percorsi relativi ordinati (byte), per ognuno «percorso\n» + testo senza CR + «\n».</summary>
        private static string SourcesFingerprint(string repo)
        {
            var files = new[] { "MdExplorer.Remote.Protocol", "MdExplorer.RemoteAgent" }
                .SelectMany(d => Directory.EnumerateFiles(Path.Combine(repo, d), "*", SearchOption.AllDirectories))
                .Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".csproj", StringComparison.Ordinal))
                .Select(f => Path.GetRelativePath(repo, f).Replace('\\', '/'))
                .Where(rel => !rel.Split('/').Any(part => part == "bin" || part == "obj"))
                .OrderBy(rel => rel, StringComparer.Ordinal)
                .ToList();
            // sui byte, come tr -d '\r': ReadAllText toglierebbe un BOM che lo script invece conta
            using var all = new MemoryStream();
            foreach (var rel in files)
            {
                all.Write(Encoding.UTF8.GetBytes(rel + "\n"));
                all.Write(File.ReadAllBytes(Path.Combine(repo, rel)).Where(b => b != (byte)'\r').ToArray());
                all.WriteByte((byte)'\n');
            }
            return Convert.ToHexString(SHA256.HashData(all.ToArray())).ToLowerInvariant();
        }

        private static string RepoRoot()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
                if (File.Exists(Path.Combine(dir.FullName, "MdExplorer.sln"))) return dir.FullName;
            throw new InvalidOperationException("MdExplorer.sln non trovato sopra " + AppContext.BaseDirectory);
        }
    }
}
