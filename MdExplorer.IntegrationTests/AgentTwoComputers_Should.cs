using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using MdExplorer.Features.Federation;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Più computer (S5): due MdExplorer veri, di Anna e di Marco, con lo stesso origin. Si coordinano solo attraverso il
    /// registro dei giri (ramo mde/giri) e il campanello: Anna lancia e sceglie Marco per la scheda; il computer di Marco
    /// scarica il registro e gliela mette «da avviare»; finita la scheda, il computer di Anna fa partire da solo la
    /// revisione, che è dell'account manager che ha lanciato.
    /// </summary>
    [TestClass]
    public class AgentTwoComputers_Should
    {
        private const string Anna = "anna@test.local";
        private const string Marco = "marco@test.local";

        private const string Workflow = @"{
  ""mde_workflow"": 2, ""title"": ""Gara"",
  ""variables"": { ""codice"": ""il bando"" },
  ""steps"": [
    { ""id"": ""ricerca"", ""agent"": ""capo"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""scheda"", ""agent"": ""tecnico"", ""title"": ""Scheda tecnica"", ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" },
      ""start"": ""ask-owner"", ""brief"": ""Scrivi la scheda tecnica sul bando {codice}."" },
    { ""id"": ""revisione"", ""agent"": ""capo"", ""title"": ""Revisione"", ""trigger"": { ""after"": [""scheda""] }, ""start"": ""auto"",
      ""brief"": ""Rivedi la scheda sul bando {codice}."" }
  ]
}";

        /// <summary>Un computer: la sua copia del progetto (clone di origin), la sua email git, la sua città.</summary>
        private static string Computer(AgentCityContext ctx, string origin, string email, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            Git(path, "clone", "--quiet", origin, ".");
            Git(path, "config", "user.email", email);
            Git(path, "config", "user.name", email.Split('@')[0]);
            Git(path, "config", "commit.gpgsign", "false");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            meta.SetAgentCity(path, new MdExplorer.Service.Models.AgentCityConfig
            {
                Enabled = true, UseAgentWorktrees = false, OwnershipDoc = "responsabilita.md", WorkflowDoc = "workflow.md",
            });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "capo");
            ctx.Trust(path, "tecnico");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");
            return path;
        }

        /// <summary>Il progetto su origin: schede degli agenti, responsabilità (il tecnico è di un team), workflow.</summary>
        private static string Origin(AgentCityContext ctx)
        {
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", "gara.git");
            Directory.CreateDirectory(origin);
            Git(origin, "init", "--bare", "-b", "main");
            var (_, seed) = ctx.SeedProject("seme");
            Git(seed, "init", "-b", "main");
            Git(seed, "config", "user.email", Anna);
            Git(seed, "config", "user.name", "anna");
            ctx.WriteLlmCitizen(seed, "capo", "Chi cerca i bandi", new[] { "user" });
            ctx.WriteLlmCitizen(seed, "tecnico", "Chi scrive la scheda", new[] { "user" });
            File.WriteAllText(Path.Combine(seed, "responsabilita.md"),
                "---\nmde_type: ownership\n---\n# Chi risponde\n\n| Ambito | Responsabile | Git Email | Agenti |\n|---|---|---|---|\n" +
                $"| Commerciale | Anna | {Anna} | capo |\n| Tecnica | Anna | {Anna} | tecnico |\n| Tecnica (2) | Marco | {Marco} | tecnico |\n");
            File.WriteAllText(Path.Combine(seed, "workflow.md"), "---\nmde_type: workflow\nworkflow: gara.workflow.json\n---\n# Gara\n");
            File.WriteAllText(Path.Combine(seed, "gara.workflow.json"), Workflow);
            Git(seed, "add", "-A");
            Git(seed, "commit", "--quiet", "-m", "il progetto");
            Git(seed, "remote", "add", "origin", origin);
            Git(seed, "push", "--quiet", "-u", "origin", "main");
            return origin;
        }

        [TestMethod]
        public async Task Coordinate_two_computers_through_the_round_ledger_and_the_bell()
        {
            using var annaPc = new AgentCityContext();
            using var marcoPc = new AgentCityContext();
            var origin = Origin(annaPc);
            var anna = Computer(annaPc, origin, Anna, "anna");
            var marco = Computer(marcoPc, origin, Marco, "marco");

            // Anna cerca i bandi e preme «Avvia il giro», scegliendo Marco per la scheda.
            var runId = Guid.NewGuid();
            await annaPc.Factory.Services.GetRequiredService<IAgentRunJobService>().RunAsync(new AgentRunRequestModel
            {
                RunId = runId, ProjectPath = anna, AgentFilePath = Path.Combine(anna, "capo.agent.md"),
                PreparedPrompt = "cerca i bandi", TriggerSource = "manual", UseWorktree = false,
            });
            var replies = System.Text.Json.JsonSerializer.Serialize(new[]
            {
                new ResolvedReply
                {
                    Id = "avvia-giro", Label = "Avvia il giro su NC-1", Description = "le schede su NC-1", Message = "avvia NC-1",
                    Values = new System.Collections.Generic.Dictionary<string, string> { ["codice"] = "NC-1" },
                },
            });
            var r = annaPc.Factory.Services.GetRequiredService<IAgentMailbox>().Enqueue(new EnqueueRequest
            {
                ProjectPath = anna, FromAgent = "capo", ToAgent = "user", Body = "[ESITO] un bando", RunId = runId.ToString("N"), Replies = replies,
            });
            var (pressed, body) = await annaPc.PostJson("/api/A2A/mailbox/reply",
                $"{{\"conversationId\":\"{r.ConversationId}\",\"body\":\"avvia NC-1\",\"choice\":true,\"assign\":{{\"scheda\":\"{Marco}\"}}}}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, pressed, body);
            Assert.IsFalse(annaPc.Messages().Any(m => m.ToAgent == "tecnico"), "la scheda è di Marco: sul computer di Anna non parte");

            // Il campanello di Anna suona per Marco.
            await WaitFor(() => annaPc.FederationSender.Bells.Any(b => b.TargetOwnerId == FederationRoom.ComputeUserId(Marco)));

            // Il computer di Marco, al campanello (o al controllo periodico), scarica il registro: la scheda è sua.
            Assert.IsTrue(await marcoPc.Factory.Services.GetRequiredService<IWorkflowPoller>().PollAsync(marco));
            var held = marcoPc.Messages().SingleOrDefault(m => m.ToAgent == "tecnico");
            Assert.IsNotNull(held, "sul computer di Marco la scheda è «da avviare»");
            Assert.AreEqual(AgentMessage.DeferredReasonEnum.AwaitingOwner, held.DeferredReason);
            StringAssert.Contains(held.Body, "Scrivi la scheda tecnica sul bando NC-1.", "con il valore del pulsante premuto da Anna");
            Assert.IsFalse(await marcoPc.Factory.Services.GetRequiredService<IWorkflowPoller>().PollAsync(marco),
                "niente di nuovo: il controllo non rifà i conti");

            // Marco la avvia; il tecnico lavora sul suo computer e finisce.
            var (started, startBody) = await marcoPc.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/start", "{}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, started, startBody);
            await marcoPc.WaitForMessages(m => m.Any(x => x.Id == held.Id && x.State == AgentMessage.StateEnum.Processed));
            Assert.AreEqual(1, marcoPc.Runner.Calls);
            var ledgerB = marcoPc.Factory.Services.GetRequiredService<IRoundStore>().Root(marco);
            var roundId = held.WorkflowRound;
            await WaitFor(() => RoundLedger.Load(ledgerB, roundId).EventsOf("scheda").LastOrDefault()?.Type == RoundEventType.Done);

            // Il computer di Anna scarica il registro: la revisione è dell'account manager che ha lanciato, e parte da sola.
            var callsBefore = annaPc.Runner.Calls;
            await annaPc.Factory.Services.GetRequiredService<IWorkflowPoller>().PollAsync(anna);
            await annaPc.WaitForMessages(m => m.Any(x => x.ToAgent == "capo" && x.WorkflowStep == "revisione"));
            // 60 s: a fine suite, con due servizi interi nello stesso processo, il primo giro del dispatcher può tardare (visto il 07/10).
            await WaitFor(() => annaPc.Runner.Calls > callsBefore, 60000);
            StringAssert.Contains(annaPc.Runner.LastRequest.ComposedPrompt, "Rivedi la scheda sul bando NC-1.");
            Assert.IsFalse(marcoPc.Messages().Any(m => m.WorkflowStep == "revisione"), "la revisione non è di Marco");

            // Un solo registro, visto uguale da tutti e due: chi ha fatto cosa.
            var ledgerA = annaPc.Factory.Services.GetRequiredService<IRoundStore>().Root(anna);
            await WaitFor(() => RoundLedger.Load(ledgerA, roundId).EventsOf("revisione").Any());
            var scheda = RoundLedger.Load(ledgerA, roundId).EventsOf("scheda").ToList();
            CollectionAssert.AreEqual(new[] { Marco, Marco, Marco }, scheda.Select(e => e.By).ToList(), "la scheda l'ha scritta solo il computer di Marco");
        }

        private static async Task WaitFor(Func<bool> condition, int timeoutMs = 20000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                if (condition()) return;
                await Task.Delay(250);
            }
            Assert.IsTrue(condition(), "la condizione non si è avverata in tempo");
        }

        private static string Git(string cwd, params string[] args)
        {
            var res = GitCli.Run(cwd, args);
            if (res.Code != 0) throw new InvalidOperationException($"git {string.Join(" ", args)}: {res.Err}");
            return res.Out;
        }
    }
}
