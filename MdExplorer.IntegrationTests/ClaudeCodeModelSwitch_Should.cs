using System;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.ClaudeCode;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Cambiare modello a MarkAgent con Claude Code senza perdere la conversazione: <c>set_model</c> sulla sessione
    /// viva invece di chiuderla e aprirne un'altra. Claude Code vero, ma nessun turno: le <c>control_request</c>
    /// hanno risposta anche prima del primo messaggio, quindi nessun token.
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Modello-Claude-Code.md, fase F3.</para>
    /// </summary>
    [TestClass]
    public class ClaudeCodeModelSwitch_Should
    {
        private string _dir;

        [TestInitialize]
        public void Setup()
        {
            if (!ClaudeCodeProcessLauncher.IsResolvable())
                Assert.Inconclusive("Claude Code CLI not installed on this machine.");
            _dir = Path.Combine(Path.GetTempPath(), "mde-claude-switch", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (_dir != null && Directory.Exists(_dir)) Directory.Delete(_dir, true); }
            catch (IOException) { }
        }

        [TestMethod]
        public async Task Switch_the_model_of_the_live_session()
        {
            await using var session = new ClaudeCodeSession(NullLogger<ClaudeCodeSession>.Instance, _dir, "haiku");
            await session.StartAsync();

            await session.SetModelAsync("sonnet");

            Assert.AreEqual("sonnet", session.ModelId);
            Assert.IsTrue(session.IsAlive, "same process: nothing was restarted");
        }

        [TestMethod]
        public async Task Refuse_a_model_the_cli_does_not_know_and_keep_the_previous_one()
        {
            await using var session = new ClaudeCodeSession(NullLogger<ClaudeCodeSession>.Instance, _dir, "haiku");
            await session.StartAsync();

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => session.SetModelAsync("modello-che-non-esiste"));

            StringAssert.Contains(ex.Message, "modello-che-non-esiste");
            StringAssert.Contains(ex.Message, "not found", "the CLI's own reason reaches the user");
            Assert.AreEqual("haiku", session.ModelId);
            Assert.IsTrue(session.IsAlive);
        }

        [TestMethod]
        public async Task Keep_the_session_when_only_the_model_changes_and_replace_it_when_the_folder_changes()
        {
            await using var pool = new ClaudeCodeSessionPool(NullLoggerFactory.Instance, NullLogger<ClaudeCodeSessionPool>.Instance);
            var other = Path.Combine(_dir, "altro-progetto");
            Directory.CreateDirectory(other);

            var first = await pool.GetOrCreateAsync("conn-1", _dir, "haiku");
            var switched = await pool.GetOrCreateAsync("conn-1", _dir, "claude-fable-5-1[1m]");

            Assert.AreSame(first, switched, "a new model on the same project keeps the conversation");
            Assert.AreEqual("claude-fable-5-1[1m]", switched.ModelId);

            var moved = await pool.GetOrCreateAsync("conn-1", other, "claude-fable-5-1[1m]");
            Assert.AreNotSame(first, moved, "another project is another conversation");
            Assert.AreEqual(other, moved.WorkingDirectory);
        }
    }
}
