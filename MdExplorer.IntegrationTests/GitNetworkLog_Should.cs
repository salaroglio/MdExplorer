using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Git;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Ogni git che va in rete lascia una riga che dice chi l'ha chiamato: è ciò che serve per
    /// capire quale funzione ha fatto aprire il login del credential manager.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class GitNetworkLog_Should
    {
        private sealed class CapturingLogger : ILogger
        {
            public readonly List<(LogLevel Level, string Text)> Lines = new();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => Lines.Add((logLevel, formatter(state, exception)));
        }

        private CapturingLogger _log;
        private string _dir;

        [TestInitialize]
        public void Init()
        {
            _log = new CapturingLogger();
            GitNetworkLog.SetLogger(_log);
            _dir = Path.Combine(Path.GetTempPath(), "gitnetlog-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            GitNetworkLog.SetLogger(null);
            try { Directory.Delete(_dir, true); } catch { }
        }

        [TestMethod]
        public async Task Runner_ComandoDiRete_ScriveRigaConChiamante()
        {
            var runner = new NativeGitRunner(NullLogger<NativeGitRunner>.Instance);

            await runner.RunAsync(_dir, new[] { "ls-remote", Path.Combine(_dir, "non-esiste") });

            var line = _log.Lines.Single();
            Assert.AreEqual(LogLevel.Warning, line.Level, "fallito → avviso");
            StringAssert.Contains(line.Text, "[git-rete] git ls-remote");
            StringAssert.Contains(line.Text, "GitNetworkLog_Should.Runner_ComandoDiRete_ScriveRigaConChiamante");
        }

        [TestMethod]
        public async Task Runner_ComandoLocale_NessunaRiga()
        {
            var runner = new NativeGitRunner(NullLogger<NativeGitRunner>.Instance);

            await runner.RunAsync(_dir, new[] { "-c", "core.quotepath=off", "status" });

            Assert.AreEqual(0, _log.Lines.Count);
        }

        [TestMethod]
        public void GitCli_ComandoDiRete_ScriveRigaConChiamante()
        {
            GitCli.Run(_dir, "fetch", Path.Combine(_dir, "non-esiste"));

            var line = _log.Lines.Single();
            StringAssert.Contains(line.Text, "[git-rete] git fetch");
            StringAssert.Contains(line.Text, "GitNetworkLog_Should.GitCli_ComandoDiRete_ScriveRigaConChiamante");
        }

        /// <summary>
        /// 09/11/2026: il reaper, 60 s dopo l'avvio, chiedeva i rami agent/* fusi per OGNI progetto, e
        /// senza origin/HEAD il ramo di default lo chiedeva al remoto («ls-remote»): su un repository
        /// GitHub privato senza accesso si apriva il login a ogni avvio. Senza rami agent/* non si va in rete.
        /// </summary>
        [TestMethod]
        public async Task RamiAgentFusi_SenzaRamiAgent_NonVaInRete()
        {
            GitCli.Run(_dir, "init", "-b", "main");
            GitCli.Run(_dir, "remote", "add", "origin", Path.Combine(_dir, "remoto-che-non-esiste"));
            var manager = new AgentWorktreeManager(NullLogger<AgentWorktreeManager>.Instance, null, null, null);

            var merged = await manager.ListMergedAgentBranchesAsync(_dir, null);

            Assert.AreEqual(0, merged.Count);
            Assert.AreEqual(0, _log.Lines.Count, "nessun git in rete: " + string.Join(" | ", _log.Lines.Select(l => l.Text)));
        }

        /// <summary>
        /// 09/11/2026: città accesa ma nessun giro ancora partito → su origin il ramo mde/giri non c'è, ed è normale.
        /// «git fetch origin mde/giri» lo diceva come errore ogni minuto; ora è «niente da scaricare».
        /// </summary>
        [TestMethod]
        public void RegistroDeiGiri_RamoNonAncoraSulServer_NessunErrore()
        {
            var origin = Path.Combine(_dir, "origin.git");
            var work = Path.Combine(_dir, "lavoro");
            Directory.CreateDirectory(origin);
            Directory.CreateDirectory(work);
            GitCli.Run(origin, "init", "--bare");
            GitCli.Run(work, "init", "-b", "main");
            GitCli.Run(work, "config", "user.email", "test@test.local");
            GitCli.Run(work, "config", "user.name", "Test");
            GitCli.Run(work, "remote", "add", "origin", origin);
            var store = new RoundStore(NullLogger<RoundStore>.Instance);

            store.Refresh(work);

            Assert.IsTrue(_log.Lines.Count > 0, "il registro deve comunque chiedere al server");
            Assert.IsFalse(_log.Lines.Any(l => l.Level >= LogLevel.Warning),
                "ramo assente = normale, non un errore: " + string.Join(" | ", _log.Lines.Select(l => l.Text)));
        }
    }
}
