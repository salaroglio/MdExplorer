using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.ClaudeCode;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// L'elenco dei modelli chiesto al Claude Code vero di questa macchina. Usa il login di chi lancia i
    /// test ma non apre un turno (nessun token) e non scrive configurazioni: <c>initialize</c> legge soltanto.
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Modello-Claude-Code.md, fase F1.</para>
    /// </summary>
    [TestClass]
    public class ClaudeCodeModelSource_Should
    {
        [TestMethod]
        public async Task List_the_models_of_this_account_from_the_real_cli()
        {
            if (!ClaudeCodeProcessLauncher.IsResolvable())
                Assert.Inconclusive("Claude Code CLI not installed on this machine.");

            var watch = Stopwatch.StartNew();
            var models = await new ClaudeCodeModelSource(NullLogger<ClaudeCodeModelSource>.Instance).ListModelsAsync();

            Assert.IsTrue(models.Count > 0);
            Assert.IsTrue(models.Any(m => m.Id == "sonnet"), "sonnet is the chat's default: " + string.Join(", ", models.Select(m => m.Id)));
            Assert.IsTrue(models.All(m => !string.IsNullOrWhiteSpace(m.Name)));
            Assert.IsTrue(watch.Elapsed < ClaudeCodeModelSource.Timeout, $"took {watch.Elapsed}");
        }
    }
}
