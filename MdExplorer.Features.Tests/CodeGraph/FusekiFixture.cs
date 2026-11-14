using System;
using System.IO;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.Fuseki;
using MdExplorer.Features.Services.KnowledgeGraph;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Un solo Fuseki (il jar del pacchetto, copiato nell'output dei test) per tutta la suite del grafo del codice:
    /// parte alla prima richiesta, dati in una cartella temporanea, si ferma a fine assembly. Ogni test usa un progetto
    /// con un percorso proprio → un grafo nominato proprio: niente interferenze. Java 21 assente = Inconclusive dichiarato.
    /// </summary>
    [TestClass]
    public static class FusekiFixture
    {
        private static readonly object Gate = new();
        private static FusekiProcessManager _manager;
        private static string _dataDir;
        private static string _unavailable;

        private sealed class PathJavaLocator : IJavaLocator
        {
            public string ResolveJavaPath() => "java";
            public string ResolveJavaPath(Ad.Tools.Dal.Abstractions.Interfaces.ISessionDB session) => "java";
            public JavaProbe Probe(string javaPath) => new JavaLocator(null).Probe(javaPath);
            public JavaProbe FindBest(int minimumMajor) => new JavaLocator(null).FindBest(minimumMajor);
        }

        public static string JarPath => Path.Combine(AppContext.BaseDirectory, "Binaries", "jena-fuseki-server.jar");

        /// <summary>Il manager condiviso, oppure <c>Assert.Inconclusive</c> con il motivo (Java 21 o jar mancanti).</summary>
        public static IFusekiProcessManager ManagerOrInconclusive()
        {
            lock (Gate)
            {
                if (_unavailable != null) Assert.Inconclusive(_unavailable);
                if (_manager != null) return _manager;
                var probe = new JavaLocator(null).Probe("java");
                if (!probe.IsRunnable) { _unavailable = "java non eseguibile dal PATH: " + probe.Error; Assert.Inconclusive(_unavailable); }
                if (probe.Major == null || probe.Major < 21) { _unavailable = $"serve Java 21, trovato «{probe.Raw}»"; Assert.Inconclusive(_unavailable); }
                if (!File.Exists(JarPath)) { _unavailable = "jar di Fuseki non copiato nell'output dei test: " + JarPath; Assert.Inconclusive(_unavailable); }
                _dataDir = Path.Combine(Path.GetTempPath(), "mde-codegraph-fuseki-" + Guid.NewGuid().ToString("N").Substring(0, 8));
                Directory.CreateDirectory(_dataDir);
                _manager = new FusekiProcessManager(new ManagedFusekiOptions { JarPath = JarPath, DataDirectory = _dataDir, HeapMegabytes = 256 },
                    new PathJavaLocator(), NullLogger<FusekiProcessManager>.Instance);
                return _manager;
            }
        }

        public static IFusekiClient NewClient()
        {
            var http = new ServiceCollection().AddHttpClient().BuildServiceProvider().GetRequiredService<System.Net.Http.IHttpClientFactory>();
            return new FusekiClient(http, NullLogger<FusekiClient>.Instance);
        }

        public static CodeGraphStore NewStore() => new(ManagerOrInconclusive(), NewClient(), NullLogger<CodeGraphStore>.Instance);

        [AssemblyCleanup]
        public static void StopFuseki()
        {
            lock (Gate)
            {
                _manager?.Dispose();
                _manager = null;
                if (_dataDir != null) { try { Directory.Delete(_dataDir, true); } catch { /* best effort */ } }
            }
        }
    }
}
