using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il deliverable di un agente non entra più in main da solo: <b>propone</b>.
    /// <para>
    /// Il gate meccanico non è stato buttato — resta il punto dove una CI o un agente-revisore
    /// pre-qualificheranno il lavoro — ma il suo «sì» ora apre una richiesta che decide l'umano.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class AgentMergeRequest_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        [TestMethod]
        public async Task Ask_instead_of_merging_when_an_agent_delivers()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (key, path) = SetupGitProject(ctx, "mr-propone");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true });
            ctx.OwnAgents(path, "worker");
            ctx.Factory.Services.GetRequiredService<IAgentWorktreePreference>().Set(path, true);

            ctx.Runner.Behavior = (req, _) =>
            {
                Directory.CreateDirectory(Path.Combine(req.WorkingDirectory, "llm-wiki"));
                File.WriteAllText(Path.Combine(req.WorkingDirectory, "llm-wiki", "nota.md"), "# nuova nota\n");
                return Task.FromResult("fatto");
            };

            var rpc = await GatewayRpc.SendMessage(ctx.Client, key, "worker", "scrivi una nota");
            Assert.IsFalse(rpc.IsError, $"{rpc.ErrorCode} {rpc.ErrorMessage}");
            await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed));

            var svc = ctx.Factory.Services.GetRequiredService<IAgentMergeRequestService>();
            var deadline = DateTime.UtcNow.AddSeconds(20);
            while (DateTime.UtcNow < deadline && svc.Pending(path).Count == 0)
                await Task.Delay(500);

            var pending = svc.Pending(path);
            Assert.AreEqual(1, pending.Count, "il deliverable deve produrre UNA richiesta");

            var request = pending[0];
            Assert.AreEqual("worker", request.AgentName);
            StringAssert.Contains(request.PublishedBranch, "llm-wiki",
                "il nome pubblicato racconta dove ha lavorato");

            // I file toccati sono fotografati: e' cio' che l'umano guarda per decidere.
            var files = svc.FilesOf(request);
            Assert.IsTrue(files.Any(f => f.Path.EndsWith("nota.md") && f.Change == "added"),
                "il file nuovo compare come aggiunto");

            // E soprattutto: NON e' finito in main da solo.
            Git(path, "fetch origin");
            Assert.AreNotEqual(0, Git(path, "cat-file -e origin/main:llm-wiki/nota.md").Code,
                "senza autorizzazione il lavoro NON entra nel ramo principale");
        }

        [TestMethod]
        public async Task Merge_only_when_the_human_authorises()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (key, path) = SetupGitProject(ctx, "mr-autorizza");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true });
            ctx.OwnAgents(path, "worker");
            ctx.Factory.Services.GetRequiredService<IAgentWorktreePreference>().Set(path, true);

            ctx.Runner.Behavior = (req, _) =>
            {
                File.WriteAllText(Path.Combine(req.WorkingDirectory, "deliverable.md"), "# out\n");
                return Task.FromResult("fatto");
            };

            var rpc = await GatewayRpc.SendMessage(ctx.Client, key, "worker", "produci");
            Assert.IsFalse(rpc.IsError);
            await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed));

            var svc = ctx.Factory.Services.GetRequiredService<IAgentMergeRequestService>();
            var deadline = DateTime.UtcNow.AddSeconds(20);
            while (DateTime.UtcNow < deadline && svc.Pending(path).Count == 0)
                await Task.Delay(500);
            Assert.AreEqual(1, svc.Pending(path).Count);

            var approved = await svc.ApproveAsync(svc.Pending(path)[0].Id);
            Assert.AreEqual(AgentMergeRequest.StatusEnum.Merged, approved.Status, approved.Note);

            Git(path, "fetch origin");
            Assert.AreEqual(0, Git(path, "cat-file -e origin/main:deliverable.md").Code,
                "autorizzata, la richiesta fonde davvero");
            Assert.AreEqual(0, svc.Pending(path).Count, "la richiesta esce dall'elenco");
        }

        [TestMethod]
        public void Ask_for_the_reason_when_the_human_rejects()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("mr-motivo");
            var svc = ctx.Factory.Services.GetRequiredService<IAgentMergeRequestService>();
            var request = svc.Open(path, "worker", "agent/tizio/worker/2026-10-05-docs-m1", "agent/worker/m1", "deadbeef",
                new[] { new ChangedFile { Change = "modified", Path = "docs/a.md" } });

            Assert.ThrowsException<InvalidOperationException>(() => svc.Reject(request.Id, "  "),
                "il motivo è ciò che l'agente legge se il lavoro riparte");
            Assert.AreEqual(1, svc.Pending(path).Count, "senza motivo non è stato rifiutato niente");
        }

        [TestMethod]
        public void Stop_a_rejected_work_someone_is_waiting_for_until_the_human_restarts_it()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("mr-fermo");
            var svc = ctx.Factory.Services.GetRequiredService<IAgentMergeRequestService>();

            // L'incarico di un altro agente, già lavorato: è ciò che ha prodotto l'artefatto.
            var assignment = SeedProcessedMessage(ctx, path, from: "account-manager", to: "tecnico");
            var request = svc.Open(path, "tecnico", "agent/tizio/tecnico/2026-10-05-schede-f1", "agent/tecnico/f1", "deadbeef",
                new[] { new ChangedFile { Change = "added", Path = "schede/tecnica.md" } }, "run1", assignment.ToString());
            Assert.IsTrue(svc.SomeoneIsWaitingFor(request));

            svc.Reject(request.Id, "manca la data");

            // Rifiutare FERMA: l'incarico non torna in coda da solo.
            Assert.AreEqual(AgentMessage.StateEnum.Processed, ReadMessage(ctx, assignment).State, "niente riparte da solo");
            Assert.AreEqual(1, svc.Stopped(path).Count, "il lavoro fermo resta visibile: qualcuno lo aspetta");

            svc.Rework(request.Id);

            var queued = ReadMessage(ctx, assignment);
            Assert.AreEqual(AgentMessage.StateEnum.Pending, queued.State, "«Fai ripartire» rimette in coda lo stesso incarico");
            Assert.AreEqual("manca la data", queued.ReworkNote, "l'agente riceve il motivo del rifiuto");
            Assert.AreEqual(0, queued.Attempts, "non è un ritentativo dopo un errore");
            Assert.AreEqual(0, svc.Stopped(path).Count, "sta ripartendo: non è più fermo");
            Assert.ThrowsException<InvalidOperationException>(() => svc.Rework(request.Id), "non si fa ripartire due volte");
        }

        [TestMethod]
        public void Treat_a_rejected_work_the_human_asked_for_as_a_closed_branch()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("mr-chiuso");
            var svc = ctx.Factory.Services.GetRequiredService<IAgentMergeRequestService>();

            // Il turno era partito da una risposta della persona: nessun altro agente aspetta.
            var fromPerson = SeedProcessedMessage(ctx, path, from: "user", to: "account-manager");
            var request = svc.Open(path, "account-manager", "agent/tizio/account-manager/2026-10-05-ricerche-c1", "agent/account-manager/c1",
                "deadbeef", new[] { new ChangedFile { Change = "added", Path = "ricerche/r.md" } }, "run2", fromPerson.ToString());
            Assert.IsFalse(svc.SomeoneIsWaitingFor(request));

            svc.Reject(request.Id, "ricerca sbagliata");

            Assert.AreEqual(0, svc.Stopped(path).Count, "ramo chiuso: non c'è niente da far ripartire");
            Assert.ThrowsException<InvalidOperationException>(() => svc.Rework(request.Id));
        }

        private static Guid SeedProcessedMessage(AgentCityContext ctx, string projectPath, string from, string to)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.BeginTransaction();
            var message = new AgentMessage
            {
                ConversationId = Guid.NewGuid(), A2ATaskId = Guid.NewGuid().ToString("N"),
                FromAgent = from, ToAgent = to, ProjectPath = projectPath, Body = "[INCARICO] scrivi la scheda",
                State = AgentMessage.StateEnum.Processed, Attempts = 1,
                CreatedAt = DateTime.UtcNow, ProcessedAt = DateTime.UtcNow,
            };
            db.GetDal<AgentMessage>().Save(message);
            db.Commit();
            return message.Id;
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

        [TestMethod]
        public void Keep_the_work_when_the_human_rejects()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("mr-rifiuta");
            var svc = ctx.Factory.Services.GetRequiredService<IAgentMergeRequestService>();

            var request = svc.Open(path, "worker", "agent/tizio/worker/2026-08-02-docs-abc123",
                "agent/worker/act1", "deadbeef",
                new[] { new ChangedFile { Change = "modified", Path = "docs/a.md" } });

            var rejected = svc.Reject(request.Id, "non mi convince l'impostazione");

            Assert.AreEqual(AgentMergeRequest.StatusEnum.Rejected, rejected.Status);
            StringAssert.Contains(rejected.Note, "non mi convince");
            Assert.AreEqual(0, svc.Pending(path).Count, "un rifiuto non torna in cima all'elenco");

            // Il rifiuto NON distrugge: il branch resta e da li' si apre il worktree per
            // metterci mano. E se l'agente rilavora, la richiesta torna in gioco: il contenuto
            // non e' piu' quello che l'umano aveva bocciato.
            svc.Open(path, "worker", "agent/tizio/worker/2026-08-02-docs-abc123",
                "agent/worker/act1", "cafe0000",
                new[] { new ChangedFile { Change = "modified", Path = "docs/a.md" } });

            Assert.AreEqual(1, svc.Pending(path).Count, "rilavorato, torna da decidere");
        }

        // ---- infrastruttura git minima ----

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git", Arguments = args, WorkingDirectory = cwd,
                    UseShellExecute = false, RedirectStandardOutput = true,
                    RedirectStandardError = true, CreateNoWindow = true,
                }
            };
            p.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            p.Start();
            var o = p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(20000);
            return (p.ExitCode, o);
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        private static (Guid Key, string Path) SetupGitProject(AgentCityContext ctx, string name)
        {
            var (key, path) = ctx.SeedProject(name);
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin); Git(origin, "init --bare");
            Git(path, "init -b main");
            Git(path, "config user.email carlo@test.local"); Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# base\n");
            Git(path, "add -A"); Git(path, "commit -m base");
            Git(path, $"remote add origin \"{origin}\""); Git(path, "push -u origin main");
            return (key, path);
        }
    }
}
