using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il passo di una persona nel servizio (workflow v3, F4): finito il lavoro del capo, a chi ha il ruolo arriva in posta
    /// «tocca a te» con i pulsanti degli esiti; il pulsante chiude il passo nel registro e fa partire solo la strada scelta.
    /// Un passo che dichiara un documento non si chiude finché il documento non è scritto, nel commit e pubblicato.
    /// </summary>
    [TestClass]
    public class PersonStep_Should
    {
        private const string Workflow = @"{
  ""mde_workflow"": 3, ""title"": ""Prova"",
  ""steps"": [
    { ""id"": ""incarica"", ""agent"": ""capo"", ""skill"": ""incarica"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""decide"", ""person"": ""Direzione"", ""title"": ""Decidi"", ""trigger"": { ""after"": [""incarica""] },
      ""task"": ""Decidi se andare avanti."", PRODUCES
      ""outcomes"": [ { ""id"": ""avanti"", ""label"": ""Andiamo avanti"" }, { ""id"": ""stop"", ""label"": ""Fermiamoci"" } ] },
    { ""id"": ""scheda"", ""agent"": ""tecnico"", ""skill"": ""scheda"", ""title"": ""Scheda tecnica"", ""trigger"": { ""reply"": ""avanti"", ""to"": ""decide"" }, ""start"": ""auto"",
      ""brief"": ""Scrivi la scheda tecnica."" }
  ]
}";

        private static string Setup(AgentCityContext ctx, string name, bool produces = false)
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
            // Un ruolo senza agenti: la persona che decide.
            File.AppendAllText(Path.Combine(path, "ownership.md"), "| Direzione | Io | io@test.local | |\n");

            Directory.CreateDirectory(Path.Combine(path, "verbali"));
            File.WriteAllText(Path.Combine(path, "verbali", "README.md"), "# Verbali\n");
            File.WriteAllText(Path.Combine(path, "workflow.md"), "---\nmde_type: workflow\nworkflow: prova.workflow.json\n---\n# Prova\n");
            File.WriteAllText(Path.Combine(path, "prova.workflow.json"), Workflow.Replace("PRODUCES",
                produces ? @"""produces"": [ { ""id"": ""verbale"", ""path"": ""verbali/verbale-*.md"" } ]," : ""));
            var city = meta.GetAgentCity(path);
            city.WorkflowDoc = "workflow.md";
            meta.SetAgentCity(path, city);

            // Il progetto ha un commit e un remoto: da lì si contano e si pubblicano i documenti della persona.
            var origin = Path.Combine(Path.GetTempPath(), "origin-" + Guid.NewGuid().ToString("N") + ".git");
            Git(Path.GetTempPath(), "init", "--bare", "-b", "main", origin);
            Git(path, "config", "user.name", "Io");
            Git(path, "remote", "add", "origin", origin);
            Git(path, "add", "-A");
            Git(path, "commit", "-q", "-m", "progetto");
            Git(path, "push", "-q", "-u", "origin", "main");

            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");
            return path;
        }

        private static string Git(string cwd, params string[] args)
        {
            var info = new System.Diagnostics.ProcessStartInfo("git") { WorkingDirectory = cwd, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            foreach (var a in args) info.ArgumentList.Add(a);
            using var p = System.Diagnostics.Process.Start(info);
            var output = p.StandardOutput.ReadToEnd();
            var error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            Assert.AreEqual(0, p.ExitCode, $"git {string.Join(' ', args)}: {error}");
            return output;
        }

        /// <summary>Lancia il capo e aspetta il «tocca a te» della persona.</summary>
        private static async Task<AgentMessage> LaunchAndWaitForTurn(AgentCityContext ctx, string path)
        {
            await ctx.Factory.Services.GetRequiredService<IAgentRunJobService>().RunAsync(new AgentRunRequestModel
            {
                ProjectPath = path, AgentFilePath = Path.Combine(path, "capo.agent.md"), PreparedPrompt = "comincia il giro",
                TriggerSource = "manual", UseWorktree = false,
            });
            var messages = await ctx.WaitForMessages(m => m.Any(x => x.TriggerSource == AgentTexts.PersonStepSource));
            return messages.Single(x => x.TriggerSource == AgentTexts.PersonStepSource);
        }

        private static RoundState Round(AgentCityContext ctx, string path, string roundId)
            => RoundLedger.Load(ctx.Factory.Services.GetRequiredService<IRoundStore>().Root(path), roundId);

        private static Task<(System.Net.HttpStatusCode Status, string Body)> Press(AgentCityContext ctx, AgentMessage turn, string label)
            => ctx.PostJson("/api/A2A/mailbox/reply", System.Text.Json.JsonSerializer.Serialize(new
            {
                conversationId = turn.ConversationId.ToString(), messageId = turn.Id.ToString(), body = label, choice = true,
            }));

        [TestMethod]
        public async Task Ask_the_person_in_the_mail_and_start_only_the_branch_she_chose()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "persona-decide");

            var turn = await LaunchAndWaitForTurn(ctx, path);
            Assert.AreEqual(ConversationHopGuardUser, turn.ToAgent);
            Assert.AreEqual(AgentTexts.PersonStepSender, turn.FromAgent);
            StringAssert.Contains(turn.Body, "«Decidi»");
            StringAssert.Contains(turn.Body, "Decidi se andare avanti.");
            var asked = Round(ctx, path, turn.WorkflowRound).EventsOf("decide").Single();
            Assert.AreEqual(RoundEventType.Asked, asked.Type);
            Assert.AreEqual(Git(path, "rev-parse", "HEAD").Trim(), asked.Commit, "da qui si contano i suoi documenti");

            var (_, inbox) = await ctx.GetJson("/api/A2A/mailbox/inbox?projectPath=" + Uri.EscapeDataString(path));
            var item = inbox.RootElement.GetProperty("messages").EnumerateArray().Single(m => m.GetProperty("id").GetString() == turn.Id.ToString());
            Assert.IsTrue(item.GetProperty("personStep").GetBoolean());
            Assert.AreEqual(2, item.GetProperty("replies").GetArrayLength(), "gli esiti sono i pulsanti");
            Assert.IsFalse(item.GetProperty("answered").GetBoolean(), "aspetta lei: è da fare");

            var (free, freeBody) = await ctx.PostJson("/api/A2A/mailbox/reply", System.Text.Json.JsonSerializer.Serialize(new
            {
                conversationId = turn.ConversationId.ToString(), messageId = turn.Id.ToString(), body = "ci penso", choice = false,
            }));
            Assert.AreEqual(System.Net.HttpStatusCode.UnprocessableEntity, free, "un passo si chiude con un esito, non con un testo");
            StringAssert.Contains(freeBody, "closed with one of its buttons");

            var (status, body) = await Press(ctx, turn, "Andiamo avanti");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            CollectionAssert.AreEqual(new[] { RoundEventType.Asked, RoundEventType.Replied, RoundEventType.Done },
                Round(ctx, path, turn.WorkflowRound).EventsOf("decide").Select(e => e.Type).ToArray());

            await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "tecnico" && x.WorkflowStep == "scheda"));
            var (_, after) = await ctx.GetJson("/api/A2A/mailbox/inbox?includeRead=true&projectPath=" + Uri.EscapeDataString(path));
            Assert.IsTrue(after.RootElement.GetProperty("messages").EnumerateArray()
                .Single(m => m.GetProperty("id").GetString() == turn.Id.ToString()).GetProperty("answered").GetBoolean(), "chiuso: non è più da fare");

            var (again, againBody) = await Press(ctx, turn, "Fermiamoci");
            Assert.AreEqual(System.Net.HttpStatusCode.UnprocessableEntity, again, "un esito solo");
            StringAssert.Contains(againBody, "is not waiting for an outcome");
        }

        [TestMethod]
        public async Task Close_the_step_only_when_its_document_is_written_committed_and_published()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "persona-documento", produces: true);
            var turn = await LaunchAndWaitForTurn(ctx, path);
            StringAssert.Contains(turn.Body, "verbali/verbale-*.md", "sa che cosa deve scrivere");

            var (none, noneBody) = await Press(ctx, turn, "Andiamo avanti");
            Assert.AreEqual(System.Net.HttpStatusCode.UnprocessableEntity, none);
            StringAssert.Contains(noneBody, "you have not written verbali/verbale-*.md");

            File.WriteAllText(Path.Combine(path, "verbali", "verbale-17.md"), "# Verbale\n\nAndiamo avanti.\n");
            var (dirty, dirtyBody) = await Press(ctx, turn, "Andiamo avanti");
            Assert.AreEqual(System.Net.HttpStatusCode.UnprocessableEntity, dirty);
            StringAssert.Contains(dirtyBody, "changes outside the commit");

            Git(path, "add", "verbali/verbale-17.md");
            Git(path, "commit", "-q", "-m", "verbale");
            var (unpublished, unpublishedBody) = await Press(ctx, turn, "Andiamo avanti");
            Assert.AreEqual(System.Net.HttpStatusCode.UnprocessableEntity, unpublished);
            StringAssert.Contains(unpublishedBody, "is not published");

            Git(path, "push", "-q");
            var (ok, okBody) = await Press(ctx, turn, "Andiamo avanti");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, ok, okBody);
            var done = Round(ctx, path, turn.WorkflowRound).EventsOf("decide").Last();
            Assert.AreEqual(RoundEventType.Done, done.Type);
            CollectionAssert.AreEqual(new[] { "verbali/verbale-17.md" }, done.Files);
            Assert.AreEqual(Git(path, "rev-parse", "HEAD").Trim(), done.Commit, "il commit del documento");
        }

        private const string ConversationHopGuardUser = "user";
    }
}
