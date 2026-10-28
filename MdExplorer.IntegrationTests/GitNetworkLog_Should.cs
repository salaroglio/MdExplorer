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
    }
}
