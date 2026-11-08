using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// R7 della valutazione del 07/11: un passo di un giro il cui incarico fallisce per sempre (l'agente non è più fidato
    /// quando tocca a lui, o il suo turno fallisce a tentativi finiti) restava «in lavorazione» nel registro del giro, e il
    /// giro non andava più avanti né si fermava con un motivo. Ora il registro riceve «fallito» con il motivo; finché restano
    /// tentativi, no.
    /// </summary>
    [TestClass]
    public class RoundStepFailed_Should
    {
        private const string Workflow = @"{
  ""mde_workflow"": 3, ""title"": ""Prova"",
  ""steps"": [
    { ""id"": ""incarica"", ""agent"": ""capo"", ""skill"": ""incarica"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""scheda"", ""agent"": ""tecnico"", ""skill"": ""scheda"", ""title"": ""Scheda tecnica"", ""trigger"": { ""after"": [""incarica""] }, ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda tecnica."" },
    { ""id"": ""chiudi"", ""agent"": ""capo"", ""skill"": ""sintesi"", ""trigger"": { ""after"": [""scheda""] }, ""start"": ""auto"" }
  ]
}";

        [TestMethod]
        public async Task Fail_the_step_when_the_agent_is_no_longer_trusted_at_delivery()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "giro-non-fidato");
            var held = await LaunchAndWaitForHold(ctx, path);
            ctx.Factory.Services.GetRequiredService<IAgentRegistryService>().UntrustAgent(path, "tecnico");

            await Start(ctx, held);

            await WaitFor(() => Last(ctx, path, held) == RoundEventType.Failed);
            Assert.AreEqual(1, ctx.Runner.Calls, "il tecnico non è partito");
            var failed = Round(ctx, path, held).EventsOf("scheda").Last();
            StringAssert.Contains(failed.Note, "tecnico", "il registro dice perché");
            Assert.AreEqual(StepStatus.Failed, WorkflowScheduler.View(Wf(path), Round(ctx, path, held), Wf(path).Step("scheda")).Status);
        }

        [TestMethod]
        public async Task Fail_the_step_only_when_the_failed_turn_has_no_attempts_left()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "giro-turno-fallito");
            var held = await LaunchAndWaitForHold(ctx, path);
            ctx.Runner.FailWith = AgentTurnOutcome.Exhausted;

            await Start(ctx, held);

            // Primo tentativo fallito: il messaggio riproverà, il passo è ancora in lavorazione.
            await ctx.WaitForMessages(m => m.Any(x => x.Id == held.Id && x.Attempts == 1 && x.State == AgentMessage.StateEnum.Pending));
            Assert.AreEqual(RoundEventType.Started, Last(ctx, path, held), "con tentativi rimasti il giro non sente niente");

            // Ultimo tentativo (senza aspettare il ritardo vero): fallito per sempre, il giro lo sa.
            SetAttempts(ctx, held.Id, 2);
            await ctx.WaitForMessages(m => m.Any(x => x.Id == held.Id && x.State == AgentMessage.StateEnum.Failed), 30000);
            await WaitFor(() => Last(ctx, path, held) == RoundEventType.Failed);
            Assert.AreEqual(1, Round(ctx, path, held).EventsOf("scheda").Count(e => e.Type == RoundEventType.Failed), "una volta sola");
            Assert.AreEqual(StepStatus.Failed, WorkflowScheduler.View(Wf(path), Round(ctx, path, held), Wf(path).Step("scheda")).Status);
        }

        // ---- «Scarta» dalla coda dell'agente ----

        [TestMethod]
        public async Task Record_a_step_discarded_from_the_queue_and_keep_who_waits_for_it_stopped()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "giro-scarta");
            var held = await LaunchAndWaitForHold(ctx, path);

            var (status, body) = await ctx.PostJson($"/api/A2A/agents/queue/{held.Id}/discard", null);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            await WaitFor(() => Last(ctx, path, held) == RoundEventType.Declined);
            StringAssert.Contains(Round(ctx, path, held).EventsOf("scheda").Last().Note, "queue");
            await Task.Delay(1500);
            Assert.AreEqual(0, Round(ctx, path, held).EventsOf("chiudi").Count(), "non facoltativo: chi lo aspetta resta fermo");
            Assert.AreEqual(1, ctx.Runner.Calls);
        }

        [TestMethod]
        public async Task Go_on_when_an_optional_step_is_discarded_from_the_queue()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "giro-scarta-facoltativo", Workflow.Replace(@"""start"": ""ask-owner"",", @"""start"": ""ask-owner"", ""optional"": true,"));
            var held = await LaunchAndWaitForHold(ctx, path);

            var (status, body) = await ctx.PostJson($"/api/A2A/agents/queue/{held.Id}/discard", null);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            await WaitFor(() => Round(ctx, path, held).EventsOf("chiudi").Any(e => e.Type == RoundEventType.Done), 30000);
            Assert.AreEqual(RoundEventType.Declined, Last(ctx, path, held));
            Assert.AreEqual(2, ctx.Runner.Calls, "il capo ha chiuso il giro, il tecnico non è partito");
        }

        // ---- appoggi ----

        private static string Setup(AgentCityContext ctx, string name, string workflow = Workflow)
        {
            var (_, path) = ctx.SeedProject(name);
            ctx.SetGitEmail(path, "io@test.local");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            meta.SetAgentCity(path, new MdExplorer.Service.Models.AgentCityConfig { Enabled = true, UseAgentWorktrees = false });

            ctx.WriteLlmCitizen(path, "capo", "Chi incarica", new[] { "user" }, jobs: AgentCityContext.CapoJobs);
            ctx.WriteLlmCitizen(path, "tecnico", "Chi scrive la scheda", new[] { "capo", "user" }, jobs: AgentCityContext.TecnicoJobs(false));
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "capo");
            ctx.Trust(path, "tecnico");
            ctx.OwnAgents(path, "capo", "tecnico");

            File.WriteAllText(Path.Combine(path, "workflow.md"), "---\nmde_type: workflow\nworkflow: prova.workflow.json\n---\n# Prova\n");
            File.WriteAllText(Path.Combine(path, "prova.workflow.json"), workflow);
            var city = meta.GetAgentCity(path);
            city.WorkflowDocs = new System.Collections.Generic.List<string> { "workflow.md" };
            meta.SetAgentCity(path, city);

            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");
            return path;
        }

        private static async Task<AgentMessage> LaunchAndWaitForHold(AgentCityContext ctx, string path)
        {
            await ctx.Factory.Services.GetRequiredService<IAgentRunJobService>().RunAsync(new AgentRunRequestModel
            {
                ProjectPath = path,
                AgentFilePath = Path.Combine(path, "capo.agent.md"),
                PreparedPrompt = "comincia il giro",
                TriggerSource = "manual",
                UseWorktree = false,
            });
            var messages = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "tecnico" && x.WorkflowRound != null));
            var held = messages.Single(x => x.ToAgent == "tecnico");
            Assert.AreEqual(AgentMessage.DeferredReasonEnum.AwaitingOwner, held.DeferredReason);
            return held;
        }

        private static async Task Start(AgentCityContext ctx, AgentMessage held)
        {
            var (status, body) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/start", "{}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
        }

        private static void SetAttempts(AgentCityContext ctx, Guid id, int attempts)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<AgentMessage>();
            var m = dal.GetList().First(x => x.Id == id);
            m.Attempts = attempts;
            m.NextAttemptAt = DateTime.UtcNow.AddSeconds(-1);
            dal.Save(m);
            db.Commit();
        }

        private static RoundState Round(AgentCityContext ctx, string path, AgentMessage held)
            => RoundLedger.Load(ctx.Factory.Services.GetRequiredService<IRoundStore>().Root(path), held.WorkflowRound);

        private static string Last(AgentCityContext ctx, string path, AgentMessage held)
            => Round(ctx, path, held).EventsOf("scheda").LastOrDefault()?.Type;

        private static WorkflowDescriptor Wf(string path)
        {
            var r = WorkflowFile.ReadBound(path, File.ReadAllText(Path.Combine(path, "prova.workflow.json")));
            Assert.IsTrue(r.IsValid, string.Join("\n", r.Issues));
            return r.Descriptor;
        }

        private static async Task WaitFor(Func<bool> condition, int timeoutMs = 15000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                if (condition()) return;
                await Task.Delay(250);
            }
            Assert.IsTrue(condition(), "la condizione non si è avverata in tempo");
        }
    }
}
