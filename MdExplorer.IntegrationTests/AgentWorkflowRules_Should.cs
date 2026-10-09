using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Workflow degli agenti, F5: con un workflow attivo MdExplorer lo applica. Un passaggio tra agenti che il workflow
    /// non descrive non parte, e chi scrive sa perché e cosa può fare; un lavoro rifiutato si fa ripartire solo quante
    /// volte il ciclo lo consente, e un passo che nessun ciclo comprende non si rifà.
    /// </summary>
    [TestClass]
    public class AgentWorkflowRules_Should
    {
        private const string WithLoop = @"{
  ""mde_workflow"": 2, ""title"": ""Prova"",
  ""steps"": [
    { ""id"": ""incarica"", ""agent"": ""capo"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""scheda"", ""agent"": ""tecnico"", ""title"": ""Scheda tecnica"", ""trigger"": { ""after"": [""incarica""] }, ""start"": ""auto"",
      ""brief"": ""Scrivi la scheda tecnica."", ""produces"": [""schede/tecnica.md""] }
  ],
  ""loops"": [ { ""id"": ""rifacimento"", ""steps"": [""scheda""], ""until"": ""approved"", ""max"": 1, ""restart"": ""manual"" } ]
}";

        private static string Project(AgentCityContext ctx, string name, string workflow)
        {
            var (_, path) = ctx.SeedProject(name);
            Directory.CreateDirectory(Path.Combine(path, "schede"));
            File.WriteAllText(Path.Combine(path, "workflow.md"), "---\nmde_type: workflow\nworkflow: prova.workflow.json\n---\n# Prova\n");
            File.WriteAllText(Path.Combine(path, "prova.workflow.json"), workflow);
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            meta.SetAgentCity(path, new MdExplorer.Service.Models.AgentCityConfig
            {
                Enabled = true, UseAgentWorktrees = false, WorkflowDoc = "workflow.md",
            });
            return path;
        }

        [TestMethod]
        public async Task Refuse_any_message_between_agents_when_the_project_has_a_workflow()
        {
            using var ctx = new AgentCityContext();
            var path = Project(ctx, "wf-non-previsto", WithLoop);
            ctx.SetGitEmail(path, "io@test.local");
            ctx.WriteLlmCitizen(path, "capo", "Chi incarica", new[] { "user" });
            ctx.WriteLlmCitizen(path, "tecnico", "Scheda tecnica", new[] { "capo", "user" });
            ctx.WriteLlmCitizen(path, "legale", "Scheda contrattuale", new[] { "capo", "user" });
            ctx.IndexAgentFiles(path);
            foreach (var a in new[] { "capo", "tecnico", "legale" }) ctx.Trust(path, a);
            ctx.OwnAgents(path, "capo", "tecnico", "legale");

            var token = ctx.MintRunToken("capo", path, null);
            var (status, body) = await ctx.SendAuthenticated(token, "legale", "[INCARICO] scrivi la scheda contrattuale");

            Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, status, body);
            StringAssert.Contains(body, "i passaggi tra agenti li fa MdExplorer", "W14: con un workflow gli agenti non instradano");
            StringAssert.Contains(body, "scrivi alla persona", "l'agente sa che cosa può fare");
            Assert.IsFalse(ctx.Messages().Any(m => m.ToAgent == "legale"), "un passaggio non previsto non entra nemmeno in coda");
        }

        [TestMethod]
        public void Let_the_rework_restart_only_as_many_times_as_the_loop_allows()
        {
            using var ctx = new AgentCityContext();
            var path = Project(ctx, "wf-rifacimenti", WithLoop);
            var svc = ctx.Factory.Services.GetRequiredService<IAgentMergeRequestService>();
            var assignment = SeedProcessedAssignment(ctx, path);

            var first = Deliver(svc, path, assignment, "r1");
            svc.Reject(first.Id, "manca il RPO");
            Assert.AreEqual(1, svc.ReworksLeft(svc.Get(first.Id), out _), "max 1: dopo il primo rifiuto si può rifare una volta");
            svc.Rework(first.Id);

            SetProcessed(ctx, assignment);   // l'agente ha rifatto il lavoro
            var second = Deliver(svc, path, assignment, "r2");
            svc.Reject(second.Id, "ancora senza RPO");

            Assert.AreEqual(0, svc.ReworksLeft(svc.Get(second.Id), out var why));
            StringAssert.Contains(why, "già stato rifatto 1 volta");
            var ex = Assert.ThrowsException<InvalidOperationException>(() => svc.Rework(second.Id));
            StringAssert.Contains(ex.Message, "è il massimo che il workflow dà al ciclo");
            Assert.AreEqual(AgentMessage.StateEnum.Processed, ReadMessage(ctx, assignment).State, "oltre il massimo il lavoro resta fermo");
        }

        [TestMethod]
        public void Restart_without_limit_a_step_no_loop_covers()
        {
            // W9: il limite lo decide chi scrive il workflow. Senza un ciclo per il passo, «Fai ripartire» resta possibile.
            using var ctx = new AgentCityContext();
            var noLoop = WithLoop.Substring(0, WithLoop.IndexOf(",\n  \"loops\"", StringComparison.Ordinal)) + "\n}";
            var path = Project(ctx, "wf-senza-ciclo", noLoop);
            var svc = ctx.Factory.Services.GetRequiredService<IAgentMergeRequestService>();
            var assignment = SeedProcessedAssignment(ctx, path);

            var request = Deliver(svc, path, assignment, "r1");
            svc.Reject(request.Id, "sbagliata");

            Assert.IsNull(svc.ReworksLeft(svc.Get(request.Id), out _), "nessun ciclo: nessun limite");
            svc.Rework(request.Id);
            Assert.AreEqual(AgentMessage.StateEnum.Pending, ReadMessage(ctx, assignment).State);
        }

        [TestMethod]
        public void Keep_restarting_without_limit_when_the_project_has_no_workflow()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("wf-assente");
            var svc = ctx.Factory.Services.GetRequiredService<IAgentMergeRequestService>();
            var assignment = SeedProcessedAssignment(ctx, path);

            var request = Deliver(svc, path, assignment, "r1");
            svc.Reject(request.Id, "sbagliata");

            Assert.IsNull(svc.ReworksLeft(svc.Get(request.Id), out _), "senza workflow non c'è un limite da applicare");
            svc.Rework(request.Id);
            Assert.AreEqual(AgentMessage.StateEnum.Pending, ReadMessage(ctx, assignment).State);
        }

        private static MdExplorer.Abstractions.Entities.UserDB.AgentMergeRequest Deliver(IAgentMergeRequestService svc, string path, Guid assignment, string run)
            => svc.Open(path, "tecnico", $"agent/io/tecnico/2026-10-06-schede-{run}", $"agent/tecnico/{run}", "deadbeef",
                new[] { new ChangedFile { Change = "added", Path = "schede/tecnica.md" } }, run, assignment.ToString());

        private static Guid SeedProcessedAssignment(AgentCityContext ctx, string projectPath)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.BeginTransaction();
            var message = new AgentMessage
            {
                ConversationId = Guid.NewGuid(), A2ATaskId = Guid.NewGuid().ToString("N"),
                FromAgent = "capo", ToAgent = "tecnico", ProjectPath = projectPath, Body = "[INCARICO] scrivi la scheda",
                State = AgentMessage.StateEnum.Processed, Attempts = 1,
                CreatedAt = DateTime.UtcNow, ProcessedAt = DateTime.UtcNow,
            };
            db.GetDal<AgentMessage>().Save(message);
            db.Commit();
            return message.Id;
        }

        private static void SetProcessed(AgentCityContext ctx, Guid id)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            var m = db.GetDal<AgentMessage>().GetList().First(x => x.Id == id);
            m.State = AgentMessage.StateEnum.Processed;
            m.ProcessedAt = DateTime.UtcNow;
            db.GetDal<AgentMessage>().Save(m);
            db.Commit();
        }

        private static AgentMessage ReadMessage(AgentCityContext ctx, Guid id)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            var m = db.GetDal<AgentMessage>().GetList().First(x => x.Id == id);
            db.Commit();
            return m;
        }
    }
}
