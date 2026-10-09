using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.AgenticEnvironments;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// Il probe sceglie l'ambiente agentico con cui Mark apre il progetto demo: un «utilizzabile»
    /// detto a torto apre il demo con un agente che non risponde, un «non utilizzabile» detto a
    /// torto lo apre senza agente. Questi test bloccano le due cose che non si vedono a occhio:
    /// che un controllo andato male <b>non</b> faccia sparire gli altri, e che ogni esito negativo
    /// porti il suo motivo invece di un generico «no».
    /// Sono test puri: i CLI sono finti, non si lancia nessun processo.
    /// </summary>
    [TestClass]
    public class AgenticEnvironmentProbe_Should
    {
        private sealed class FakeCheck : IAgenticEnvironmentCheck
        {
            public string Id { get; set; }
            public string Path { get; set; }
            public Func<CancellationToken, Task<AgenticEnvironmentUsability>> Usable { get; set; }
            public int UsableCalls;

            public string ResolvePath() => Path;

            public Task<AgenticEnvironmentUsability> CheckUsableAsync(CancellationToken ct)
            {
                Interlocked.Increment(ref UsableCalls);
                return Usable(ct);
            }
        }

        private static AgenticEnvironmentProbe Probe(TimeSpan timeout, params IAgenticEnvironmentCheck[] checks) =>
            new AgenticEnvironmentProbe(checks, NullLogger<AgenticEnvironmentProbe>.Instance, timeout);

        private static readonly TimeSpan Generous = TimeSpan.FromSeconds(10);

        [TestMethod]
        public async Task Dire_non_installato_senza_lanciare_il_controllo_quando_il_CLI_non_e_nel_PATH()
        {
            var check = new FakeCheck { Id = "claude", Path = null, Usable = _ => throw new AssertFailedException("non va chiamato") };

            var status = (await Probe(Generous, check).ProbeAsync()).Single();

            Assert.IsFalse(status.Installed);
            Assert.IsFalse(status.Usable);
            Assert.AreEqual(AgenticEnvironmentReasons.NotInstalled, status.Reason);
            Assert.AreEqual(0, check.UsableCalls);
        }

        [TestMethod]
        public async Task Dire_utilizzabile_senza_motivo_quando_il_controllo_riesce()
        {
            var check = new FakeCheck
            {
                Id = "copilot", Path = "/bin/copilot",
                Usable = _ => Task.FromResult(AgenticEnvironmentUsability.Ok("Accesso fatto.")),
            };

            var status = (await Probe(Generous, check).ProbeAsync()).Single();

            Assert.IsTrue(status.Installed);
            Assert.IsTrue(status.Usable);
            Assert.IsNull(status.Reason);
            Assert.AreEqual("/bin/copilot", status.Path);
        }

        [TestMethod]
        public async Task Portare_il_motivo_del_controllo_quando_non_e_utilizzabile()
        {
            var check = new FakeCheck
            {
                Id = "claude", Path = "/bin/claude",
                Usable = _ => Task.FromResult(AgenticEnvironmentUsability.No(AgenticEnvironmentReasons.NotLoggedIn, "fai il login")),
            };

            var status = (await Probe(Generous, check).ProbeAsync()).Single();

            Assert.IsTrue(status.Installed, "installato e non utilizzabile sono due fatti diversi");
            Assert.IsFalse(status.Usable);
            Assert.AreEqual(AgenticEnvironmentReasons.NotLoggedIn, status.Reason);
            Assert.AreEqual("fai il login", status.Detail);
        }

        [TestMethod]
        public async Task Dare_un_esito_e_non_un_eccezione_quando_un_controllo_fallisce()
        {
            var broken = new FakeCheck { Id = "copilot", Path = "/bin/copilot", Usable = _ => throw new InvalidOperationException("SDK non parte") };
            var healthy = new FakeCheck { Id = "claude", Path = "/bin/claude", Usable = _ => Task.FromResult(AgenticEnvironmentUsability.Ok("ok")) };

            var statuses = await Probe(Generous, broken, healthy).ProbeAsync();

            Assert.AreEqual(AgenticEnvironmentReasons.Error, statuses[0].Reason);
            StringAssert.Contains(statuses[0].Detail, "SDK non parte");
            Assert.IsTrue(statuses[1].Usable, "il controllo rotto non deve far sparire quello sano");
        }

        [TestMethod]
        public async Task Dire_tempo_scaduto_quando_il_CLI_non_risponde_anche_se_ignora_la_cancellazione()
        {
            // Un CLI appeso che non guarda il token: è il caso che terrebbe ferma la risposta per sempre.
            var hung = new FakeCheck { Id = "opencode", Path = "/bin/opencode", Usable = _ => new TaskCompletionSource<AgenticEnvironmentUsability>().Task };
            var healthy = new FakeCheck { Id = "claude", Path = "/bin/claude", Usable = _ => Task.FromResult(AgenticEnvironmentUsability.Ok("ok")) };

            var watch = Stopwatch.StartNew();
            var statuses = await Probe(TimeSpan.FromMilliseconds(300), hung, healthy).ProbeAsync();

            Assert.IsTrue(watch.ElapsedMilliseconds < 5000, $"il probe è rimasto fermo {watch.ElapsedMilliseconds} ms");
            Assert.IsTrue(statuses[0].Installed);
            Assert.IsFalse(statuses[0].Usable);
            Assert.AreEqual(AgenticEnvironmentReasons.Timeout, statuses[0].Reason);
            Assert.IsTrue(statuses[1].Usable);
        }

        [TestMethod]
        public async Task Dire_tempo_scaduto_quando_il_controllo_rispetta_la_cancellazione()
        {
            var slow = new FakeCheck
            {
                Id = "copilot", Path = "/bin/copilot",
                Usable = async ct => { await Task.Delay(TimeSpan.FromSeconds(30), ct); return AgenticEnvironmentUsability.Ok("tardi"); },
            };

            var status = (await Probe(TimeSpan.FromMilliseconds(300), slow).ProbeAsync()).Single();

            Assert.AreEqual(AgenticEnvironmentReasons.Timeout, status.Reason);
        }

        [TestMethod]
        public async Task Provare_gli_ambienti_in_parallelo_e_restituirli_nell_ordine_dato()
        {
            FakeCheck Slow(string id) => new FakeCheck
            {
                Id = id, Path = "/bin/" + id,
                Usable = async ct => { await Task.Delay(400, ct); return AgenticEnvironmentUsability.Ok("ok"); },
            };

            var watch = Stopwatch.StartNew();
            var statuses = await Probe(Generous, Slow("copilot"), Slow("claude"), Slow("opencode")).ProbeAsync();

            CollectionAssert.AreEqual(new[] { "copilot", "claude", "opencode" }, statuses.Select(s => s.Id).ToArray());
            Assert.IsTrue(watch.ElapsedMilliseconds < 1000, $"tre controlli da 400 ms in fila: {watch.ElapsedMilliseconds} ms");
        }

        [TestMethod]
        public async Task Lasciar_risalire_la_cancellazione_di_chi_ha_chiesto_il_probe()
        {
            var slow = new FakeCheck
            {
                Id = "copilot", Path = "/bin/copilot",
                Usable = async ct => { await Task.Delay(TimeSpan.FromSeconds(30), ct); return AgenticEnvironmentUsability.Ok("tardi"); },
            };
            using var caller = new CancellationTokenSource(200);

            // Chi ha chiesto se n'è andato: non è «tempo scaduto» del CLI, è una richiesta annullata.
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => Probe(Generous, slow).ProbeAsync(caller.Token));
        }

        // ── Claude Code: lettura di `claude auth status` ─────────────────────────────

        [TestMethod]
        public void Claude_utilizzabile_quando_loggedIn_e_true()
        {
            var u = ClaudeCodeEnvironmentCheck.Interpret(0, "{\"loggedIn\":true,\"authMethod\":\"claude.ai\"}", "");
            Assert.IsTrue(u.Usable);
        }

        [TestMethod]
        public void Claude_senza_accesso_quando_loggedIn_e_false_anche_con_uscita_diversa_da_zero()
        {
            var u = ClaudeCodeEnvironmentCheck.Interpret(1, "{\"loggedIn\":false}", "");
            Assert.IsFalse(u.Usable);
            Assert.AreEqual(AgenticEnvironmentReasons.NotLoggedIn, u.Reason);
        }

        [TestMethod]
        public void Claude_errore_e_non_un_esito_inventato_quando_la_risposta_non_e_JSON()
        {
            // Una versione vecchia che non conosce `auth status`: non si sa se l'accesso c'è, e si dice.
            var u = ClaudeCodeEnvironmentCheck.Interpret(1, "", "error: unknown command 'auth'\n");
            Assert.IsFalse(u.Usable);
            Assert.AreEqual(AgenticEnvironmentReasons.Error, u.Reason);
            StringAssert.Contains(u.Detail, "unknown command 'auth'");
        }

        [TestMethod]
        public void Claude_errore_quando_il_JSON_non_ha_il_campo_loggedIn()
        {
            var u = ClaudeCodeEnvironmentCheck.Interpret(0, "{\"status\":\"ok\"}", "");
            Assert.AreEqual(AgenticEnvironmentReasons.Error, u.Reason);
        }

        // ── opencode: lettura di `opencode models` ───────────────────────────────────

        [TestMethod]
        public void OpenCode_utilizzabile_quando_elenca_almeno_un_modello()
        {
            var u = OpenCodeEnvironmentCheck.Interpret(0, "opencode/big-pickle\nopencode/nemotron-3-ultra-free\n", "");
            Assert.IsTrue(u.Usable);
            StringAssert.Contains(u.Detail, "2 modelli");
        }

        [TestMethod]
        public void OpenCode_conta_i_modelli_anche_con_i_colori_del_terminale_e_ignora_gli_avvisi()
        {
            var u = OpenCodeEnvironmentCheck.Interpret(0, "Update available: run opencode upgrade\n\u001b[32mopencode/big-pickle\u001b[0m\n", "");
            Assert.IsTrue(u.Usable);
            StringAssert.Contains(u.Detail, "1 modello");
        }

        [TestMethod]
        public void OpenCode_senza_modelli_quando_l_elenco_e_vuoto()
        {
            var u = OpenCodeEnvironmentCheck.Interpret(0, "\n", "");
            Assert.IsFalse(u.Usable);
            Assert.AreEqual(AgenticEnvironmentReasons.NoModels, u.Reason);
        }

        [TestMethod]
        public void OpenCode_errore_quando_il_comando_esce_con_un_codice_diverso_da_zero()
        {
            var u = OpenCodeEnvironmentCheck.Interpret(2, "", "Error: config file is not valid JSON\n");
            Assert.AreEqual(AgenticEnvironmentReasons.Error, u.Reason);
            StringAssert.Contains(u.Detail, "config file is not valid JSON");
        }

        // ── Lancio di un comando su un CLI ───────────────────────────────────────────

        [TestMethod]
        public async Task Lanciare_un_comando_vero_e_raccoglierne_l_uscita()
        {
            if (OperatingSystem.IsWindows()) Assert.Inconclusive("Usa /bin/sh.");

            var result = await CliCommand.RunAsync("/bin/sh", new[] { "-c", "echo fuori; echo dentro 1>&2; exit 3" }, CancellationToken.None);

            Assert.AreEqual(3, result.ExitCode);
            Assert.AreEqual("fuori", result.Stdout.Trim());
            Assert.AreEqual("dentro", result.Stderr.Trim());
        }

        [TestMethod]
        public async Task Chiudere_il_processo_quando_il_comando_viene_annullato()
        {
            if (OperatingSystem.IsWindows()) Assert.Inconclusive("Usa /bin/sh.");
            using var cts = new CancellationTokenSource(300);

            var watch = Stopwatch.StartNew();
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(
                () => CliCommand.RunAsync("/bin/sh", new[] { "-c", "sleep 30" }, cts.Token));

            Assert.IsTrue(watch.ElapsedMilliseconds < 5000, $"il comando non è stato chiuso: {watch.ElapsedMilliseconds} ms");
        }
    }
}
