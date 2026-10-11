using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.Features.Utilities;
using MdExplorer.Features.Services.Fuseki;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// D19: il Fuseki del pacchetto. Il jar è quello copiato nell'output accanto a <c>Binaries/plantuml.jar</c>;
    /// Java 21 è un prerequisito della macchina: se manca il test è <b>Inconclusive dichiarato</b>, non verde.
    /// </summary>
    [TestClass]
    public class FusekiProcessManager_Should
    {
        private static readonly string BundledJar = Path.Combine(AppContext.BaseDirectory, "Binaries", "jena-fuseki-server.jar");

        private sealed class PathJavaLocator : IJavaLocator
        {
            private readonly string _path;
            public PathJavaLocator(string path) { _path = path; }
            public string ResolveJavaPath() => _path;
            public string ResolveJavaPath(Ad.Tools.Dal.Abstractions.Interfaces.ISessionDB session) => _path;
            public JavaProbe Probe(string javaPath) => new JavaLocator(null).Probe(javaPath);
        }

        private static ManagedFusekiOptions OptionsIn(string dataDir) => new()
        {
            JarPath = BundledJar,
            DataDirectory = dataDir,
            HeapMegabytes = 256,
        };

        private static string NewDataDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), "mde-fuseki-test-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static bool Java21Available(out string reason)
        {
            var probe = new JavaLocator(null).Probe("java");
            if (!probe.IsRunnable) { reason = "java non eseguibile dal PATH: " + probe.Error; return false; }
            if (probe.Major == null || probe.Major < 21) { reason = $"serve Java 21, trovato «{probe.Raw}»"; return false; }
            reason = null;
            return true;
        }

        [TestMethod]
        public void Ship_the_jar_next_to_plantuml()
        {
            // Il jar deve arrivare nell'output della build (e quindi nel publish) come plantuml.jar.
            Assert.IsTrue(File.Exists(BundledJar), $"jar non copiato nell'output: {BundledJar}");
            Assert.IsTrue(File.Exists(Path.Combine(AppContext.BaseDirectory, "Binaries", "plantuml.jar")));
            Assert.IsTrue(new FileInfo(BundledJar).Length > 50_000_000, "il jar standalone di Fuseki 6 pesa ~57 MB");
        }

        [TestMethod]
        public async Task Start_the_bundled_fuseki_with_the_code_dataset_and_text_index()
        {
            if (!Java21Available(out var why)) { Assert.Inconclusive(why); return; }

            var dataDir = NewDataDir();
            using var manager = new FusekiProcessManager(OptionsIn(dataDir), new PathJavaLocator("java"), NullLogger<FusekiProcessManager>.Instance);
            try
            {
                var started = DateTime.UtcNow;
                var baseUri = await manager.EnsureRunningAsync();
                var startup = DateTime.UtcNow - started;
                Console.WriteLine($"Fuseki pronto in {startup.TotalSeconds:0.0} s su {baseUri}");
                StringAssert.StartsWith(baseUri, "http://127.0.0.1:");
                Assert.AreEqual(baseUri, manager.RunningBaseUri);
                Assert.AreSame(baseUri, await manager.EnsureRunningAsync(), "seconda chiamata: stessa istanza, niente riavvio");

                // La configurazione del dataset è stata scritta PRIMA dell'avvio, con la versione in testa.
                Assert.AreEqual(FusekiManagedConfig.ConfigVersion, FusekiManagedConfig.ReadVersion(dataDir));

                using var http = new HttpClient();
                var server = JsonDocument.Parse(await http.GetStringAsync(baseUri + "/$/server")).RootElement;
                var names = new System.Collections.Generic.List<string>();
                foreach (var ds in server.GetProperty("datasets").EnumerateArray()) names.Add(ds.GetProperty("ds.name").GetString());
                CollectionAssert.Contains(names, "/" + FusekiManagedConfig.CodeDataset);

                // Triple in un grafo nominato (una cartella program) e ricerca a testo pieno dentro GRAPH.
                var update = "PREFIX code: <https://mdexplorer.net/ns/code#> INSERT DATA { GRAPH <urn:mde:code:test:src> { " +
                             "<urn:mde:sym:a> code:name \"Demo.Foo.Bar(string, int)\" ; code:summary \"Calcola il totale della fattura\" . } }";
                var up = await http.PostAsync(baseUri + "/code/update", new StringContent(update, System.Text.Encoding.UTF8, "application/sparql-update"));
                Assert.IsTrue(up.IsSuccessStatusCode, "update: " + up.StatusCode);

                var query = "PREFIX code: <https://mdexplorer.net/ns/code#> PREFIX text: <http://jena.apache.org/text#> " +
                            "SELECT ?s WHERE { GRAPH <urn:mde:code:test:src> { ?s text:query (code:summary \"fattura\") } }";
                var req = new HttpRequestMessage(HttpMethod.Post, baseUri + "/code/query")
                { Content = new FormUrlEncodedContent(new[] { new System.Collections.Generic.KeyValuePair<string, string>("query", query) }) };
                req.Headers.Accept.ParseAdd("application/sparql-results+json");
                var resp = await http.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();
                Assert.IsTrue(resp.IsSuccessStatusCode, body);
                StringAssert.Contains(body, "urn:mde:sym:a", "jena-text non ha indicizzato il summary: " + body);

                // Admin a caldo (memoria degli agenti): dataset TDB2 creato via /$/datasets, senza password da loopback.
                var create = await http.PostAsync(baseUri + "/$/datasets", new FormUrlEncodedContent(new[]
                {
                    new System.Collections.Generic.KeyValuePair<string, string>("dbName", "mem_test"),
                    new System.Collections.Generic.KeyValuePair<string, string>("dbType", "tdb2"),
                }));
                Assert.IsTrue(create.IsSuccessStatusCode, "create dataset: " + create.StatusCode);

                await manager.StopAsync();
                Assert.IsNull(manager.RunningBaseUri);
                await Task.Delay(300);
                try { await http.GetAsync(baseUri + "/$/ping"); Assert.Fail("Fuseki risponde ancora dopo StopAsync"); }
                catch (HttpRequestException) { /* fermo: giusto */ }
            }
            finally
            {
                await manager.StopAsync();
                try { Directory.Delete(dataDir, true); } catch { }
            }
        }

        [TestMethod]
        public async Task Refuse_a_java_older_than_21_with_an_actionable_message()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { Assert.Inconclusive("finto java come script sh: solo Linux/macOS"); return; }

            // Un finto java che si presenta come 17: il manager deve fermarsi PRIMA di avviare, dicendo cosa fare.
            var dataDir = NewDataDir();
            var fakeJava = Path.Combine(dataDir, "java17");
            File.WriteAllText(fakeJava, "#!/bin/sh\necho 'openjdk version \"17.0.2\" 2022-01-18' >&2\nexit 0\n");
            File.SetUnixFileMode(fakeJava, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

            var options = OptionsIn(dataDir);
            options.SearchAlternativeJava = false; // qui si verifica il rifiuto: senza ricerca di un altro Java
            using var manager = new FusekiProcessManager(options, new PathJavaLocator(fakeJava), NullLogger<FusekiProcessManager>.Instance);
            var ex = await Assert.ThrowsExceptionAsync<FusekiUnavailableException>(() => manager.EnsureRunningAsync());
            StringAssert.Contains(ex.Message, "Java 17 trovato");
            StringAssert.Contains(ex.Message, "richiede Java 21");
            StringAssert.Contains(ex.Message, fakeJava);
            Assert.IsNull(manager.RunningBaseUri);
            try { Directory.Delete(dataDir, true); } catch { }
        }

        [TestMethod]
        public async Task Use_a_java_21_found_elsewhere_when_JavaPath_is_too_old()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { Assert.Inconclusive("finto java come script sh: solo Linux/macOS"); return; }

            // Windows, 11/10/2026: 'JavaPath' puntava al JRE 8 di un altro programma e Temurin 21 stava in JAVA_HOME/PATH.
            // Con la ricerca accesa (default) il manager parte con il Java adatto invece di fermarsi.
            var dataDir = NewDataDir();
            var fakeJava = Path.Combine(dataDir, "java8");
            File.WriteAllText(fakeJava, "#!/bin/sh\necho 'openjdk version \"1.8.0_242\"' >&2\nexit 0\n");
            File.SetUnixFileMode(fakeJava, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

            using var manager = new FusekiProcessManager(OptionsIn(dataDir), new PathJavaLocator(fakeJava), NullLogger<FusekiProcessManager>.Instance);
            var baseUri = await manager.EnsureRunningAsync();
            StringAssert.StartsWith(baseUri, "http://127.0.0.1:");
            Assert.IsNotNull(manager.RunningBaseUri);
            await manager.StopAsync();
            try { Directory.Delete(dataDir, true); } catch { }
        }

        [TestMethod]
        public async Task Refuse_to_start_without_the_jar_and_name_the_path()
        {
            var dataDir = NewDataDir();
            var options = OptionsIn(dataDir);
            options.JarPath = Path.Combine(dataDir, "missing", "jena-fuseki-server.jar");
            using var manager = new FusekiProcessManager(options, new PathJavaLocator("java"), NullLogger<FusekiProcessManager>.Instance);
            var ex = await Assert.ThrowsExceptionAsync<FusekiUnavailableException>(() => manager.EnsureRunningAsync());
            StringAssert.Contains(ex.Message, options.JarPath);
            StringAssert.Contains(ex.Message, "reinstalla MdExplorer");
            try { Directory.Delete(dataDir, true); } catch { }
        }
    }
}
