using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph.Grammars;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// F3 (D8): il download di una grammatica segue il contratto degli add-on (manifest → zip → checksum → file atteso)
    /// da un server locale che serve i pacchetti prodotti da <c>scripts/codegraph/pack-grammars.sh</c>; un checksum
    /// sbagliato o uno zip senza il file atteso NON installano niente.
    /// </summary>
    [TestClass]
    public class GrammarDownloaderShould
    {
        private sealed class Factory : IHttpClientFactory { public HttpClient CreateClient(string name) => new HttpClient(); }

        private string _root, _site;
        private HttpListener _listener;
        private string _baseUrl;
        private GrammarStore _store;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-grammar-dl", Guid.NewGuid().ToString("N"));
            _site = Path.Combine(_root, "site");
            Directory.CreateDirectory(_site);
            _store = new GrammarStore(Path.Combine(_root, "appdata"));
            var port = 18000 + new Random().Next(1000, 9000);
            _baseUrl = $"http://127.0.0.1:{port}/api/addons";
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            _listener.Start();
            _ = Task.Run(Serve);
        }

        [TestCleanup]
        public void Cleanup() { try { _listener.Stop(); Directory.Delete(_root, true); } catch { } }

        private async Task Serve()
        {
            while (_listener.IsListening)
            {
                HttpListenerContext ctx;
                try { ctx = await _listener.GetContextAsync(); } catch { return; }
                var rel = ctx.Request.Url!.AbsolutePath.Replace("/api/addons/", string.Empty).TrimStart('/');
                var file = Path.Combine(_site, rel.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(file)) { var bytes = await File.ReadAllBytesAsync(file); ctx.Response.StatusCode = 200; await ctx.Response.OutputStream.WriteAsync(bytes); }
                else ctx.Response.StatusCode = 404;
                ctx.Response.Close();
            }
        }

        /// <summary>Un pacchetto come lo produce pack-grammars.sh: cartella grammar-&lt;id&gt;-&lt;rid&gt;/ con manifest e zip.</summary>
        private (string Checksum, string ZipPath) Publish(GrammarInfo g, string payloadFileName, string checksumOverride = null)
        {
            var id = g.AddonId(_store.Rid);
            var dir = Path.Combine(_site, id); Directory.CreateDirectory(dir);
            var payload = Path.Combine(_root, payloadFileName); File.WriteAllText(payload, "fake grammar bytes " + Guid.NewGuid());
            var zip = Path.Combine(dir, id + ".zip");
            using (var z = System.IO.Compression.ZipFile.Open(zip, System.IO.Compression.ZipArchiveMode.Create)) z.CreateEntryFromFile(payload, payloadFileName);
            string sum;
            using (var sha = System.Security.Cryptography.SHA256.Create()) using (var fs = File.OpenRead(zip)) sum = Convert.ToHexString(sha.ComputeHash(fs)).ToLowerInvariant();
            File.WriteAllText(Path.Combine(dir, "manifest"),
                $"{{\"name\":\"tree-sitter {g.Id}\",\"version\":\"1.3.0\",\"downloadUrl\":\"{id}/{id}.zip\",\"size\":{new FileInfo(zip).Length},\"checksum\":\"{checksumOverride ?? sum}\"}}");
            return (sum, zip);
        }

        private GrammarDownloader Downloader() => new(new Factory(), _store, new CodeGraphOptions { GrammarApiUrl = _baseUrl }, NullLogger<GrammarDownloader>.Instance);

        [TestMethod]
        public async Task Download_verify_and_install_a_grammar_from_the_addon_contract()
        {
            var g = GrammarCatalog.ById("python");
            var (sum, _) = Publish(g, g.FileName(_store.Rid));
            Assert.IsFalse(_store.IsInstalled(g));

            var installed = await Downloader().DownloadAsync("python");

            Assert.IsTrue(_store.IsInstalled(g));
            Assert.AreEqual("1.3.0", installed.Version);
            Assert.AreEqual(sum, installed.Checksum);
            Assert.AreEqual(_store.PathOf(g), installed.Path);
            Assert.AreEqual("1.3.0", _store.Installed(g).Version, "i metadati si rileggono dal disco");
        }

        [TestMethod]
        public async Task Refuse_a_wrong_checksum_and_a_zip_without_the_expected_file()
        {
            var g = GrammarCatalog.ById("go");
            Publish(g, g.FileName(_store.Rid), checksumOverride: "deadbeef");
            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Downloader().DownloadAsync("go"));
            StringAssert.Contains(ex.Message, "Checksum");
            Assert.IsFalse(_store.IsInstalled(g));

            var j = GrammarCatalog.ById("java");
            Publish(j, "wrong-name.bin");
            var ex2 = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Downloader().DownloadAsync("java"));
            StringAssert.Contains(ex2.Message, "does not contain");
            Assert.IsFalse(_store.IsInstalled(j));

            var ex3 = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Downloader().DownloadAsync("rust"));
            StringAssert.Contains(ex3.Message, "manifest not available");
        }
    }
}
