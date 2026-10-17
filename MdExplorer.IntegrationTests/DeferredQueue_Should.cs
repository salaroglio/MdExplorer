using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 6c (locale) — coda differita. Quando l'agente non è eseguibile adesso (qui: tetto
    /// istanze Copilot pieno, via fake gate) la consegna è <b>parcheggiata</b>, non fallita: il
    /// messaggio resta <c>pending</c> col motivo, NON consuma tentativi, e riprende appena la
    /// condizione si libera. Semantica chiave §12.5: "parcheggiato" ≠ "fallito".
    /// </summary>
    [TestClass]
    public class DeferredQueue_Should
    {
        [TestMethod]
        public async Task Park_a_run_without_consuming_attempts_then_resume()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("deferred");

            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            // Il primo tentativo di run viene parcheggiato (nessuno slot); il secondo passa.
            ctx.Gate.DeferFirst = 1;

            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, "worker", "parti");
            Assert.IsFalse(rpc.IsError, $"errore inatteso: {rpc.ErrorCode} {rpc.ErrorMessage}");

            // 1) Il messaggio si posa PARCHEGGIATO: pending, motivo 'resources', tentativi intatti.
            var parked = await ctx.WaitForMessages(m => m.Any(x =>
                x.ToAgent == "worker" && x.DeferredReason == AgentMessage.DeferredReasonEnum.Resources));
            var msg = parked.First(x => x.ToAgent == "worker");
            Assert.AreEqual(AgentMessage.StateEnum.Pending, msg.State, "parcheggiato = pending, non failed");
            Assert.AreEqual(0, msg.Attempts, "il parcheggio NON consuma tentativi");

            // 2) Liberato lo slot, riprende da solo (NextAttemptAt) ed è consegnato.
            var done = await ctx.WaitForMessages(
                m => m.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed),
                timeoutMs: 20000);
            var final = done.First(x => x.ToAgent == "worker");

            Assert.AreEqual(AgentMessage.StateEnum.Processed, final.State);
            Assert.AreEqual(0, final.Attempts, "nessun tentativo consumato dal parcheggio");
            Assert.IsNull(final.DeferredReason, "il marcatore di parcheggio è azzerato dopo il run");
            Assert.AreEqual(1, ctx.Gate.Admits, "ammesso una sola volta, al secondo giro");
        }

        [TestMethod]
        public async Task Park_an_agent_in_maintenance_shared_via_git()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("maint");

            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            // WIP dichiarato in .development.yml (condiviso col team via git).
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true, Maintenance = new List<string> { "worker" } });

            await GatewayRpc.SendMessage(ctx.Client, projectKey, "worker", "parti");

            var parked = await ctx.WaitForMessages(m => m.Any(x =>
                x.ToAgent == "worker" && x.DeferredReason == AgentMessage.DeferredReasonEnum.Maintenance));
            var msg = parked.First(x => x.ToAgent == "worker");
            Assert.AreEqual(AgentMessage.StateEnum.Pending, msg.State);
            Assert.AreEqual(0, msg.Attempts, "manutenzione parcheggia, non consuma tentativi");
        }

        [TestMethod]
        public async Task Park_a_user_paused_agent_then_resume_when_unpaused()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("userpause");

            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            // Pausa locale dell'utente (UserDB) — non via git.
            ctx.PauseAgent(path, "worker", "sto lavorando al file");

            await GatewayRpc.SendMessage(ctx.Client, projectKey, "worker", "parti");

            var parked = await ctx.WaitForMessages(m => m.Any(x =>
                x.ToAgent == "worker" && x.DeferredReason == AgentMessage.DeferredReasonEnum.User));
            Assert.AreEqual(0, parked.First(x => x.ToAgent == "worker").Attempts);

            // Tolta la pausa, riprende da solo.
            ctx.ResumeAgent(path, "worker");
            var done = await ctx.WaitForMessages(
                m => m.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed),
                timeoutMs: 20000);
            var final = done.First(x => x.ToAgent == "worker");
            Assert.AreEqual(AgentMessage.StateEnum.Processed, final.State);
            Assert.AreEqual(0, final.Attempts, "il parcheggio utente non consuma tentativi");
            Assert.IsNull(final.DeferredReason);
        }
    }
}
