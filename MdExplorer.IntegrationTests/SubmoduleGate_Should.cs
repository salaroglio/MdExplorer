using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 7e — gate del push umano per il codice. Un agente tocca un submodule → marker
    /// <c>SubmoduleAwaitingPush</c> + deferral <c>awaiting-push</c>; al commit umano lo sha del
    /// submodule è catturato (release token) e i dispatch si sbloccano. <para>Richiede git.</para>
    /// </summary>
    [TestClass]
    public class SubmoduleGate_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git", Arguments = args, WorkingDirectory = cwd,
                    UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
                }
            };
            p.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            p.Start();
            var o = p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(60000);
            return (p.ExitCode, o);
        }

        private static void ConfigRepo(string path)
        {
            Git(path, "config user.email agent@test.local");
            Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            Git(path, "config protocol.file.allow always");   // submodule da file:// nei test
        }

        /// <summary>Progetto git con un submodule 'code' (sub-origin bare) + main-origin bare.</summary>
        private static string SetupProjectWithSubmodule(AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);

            // Sub-repo 'code' con la sua origin bare.
            var subOrigin = Path.Combine(ctx.Factory.DataDir, "origins", name + "-code.git");
            var subWork = Path.Combine(ctx.Factory.DataDir, "work", name + "-code");
            Directory.CreateDirectory(subOrigin); Git(subOrigin, "init --bare");
            Git(subOrigin, "symbolic-ref HEAD refs/heads/main");   // così il clone del submodule checkouta main
            Directory.CreateDirectory(subWork); Git(subWork, "init -b main"); ConfigRepo(subWork);
            File.WriteAllText(Path.Combine(subWork, "code.txt"), "v1\n");
            Git(subWork, "add -A"); Git(subWork, "commit -m code-base"); Git(subWork, $"remote add origin \"{subOrigin}\""); Git(subWork, "push -u origin main");

            // Progetto: submodule add del sub-origin come 'code'.
            Git(path, "init -b main"); ConfigRepo(path);
            File.WriteAllText(Path.Combine(path, "README.md"), "# base\n");
            Git(path, "add -A"); Git(path, "commit -m base");
            Git(path, $"-c protocol.file.allow=always submodule add \"{subOrigin}\" code");
            Git(path, "commit -m add-submodule");
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin); Git(origin, "init --bare");
            Git(path, $"remote add origin \"{origin}\""); Git(path, "push -u origin main");
            return path;
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        [TestMethod]
        public async Task Detect_a_dirty_submodule_and_open_the_gate()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = SetupProjectWithSubmodule(ctx, "sg-detect");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            // Il submodule 'code' è popolato dal `submodule add`. Simulo il tocco dell'agente
            // (contenuto modificato). GetDirtySubmodules opera su qualsiasi dir git (stessa
            // logica del worktree, senza la populate file:// dei submodule nel worktree).
            var codeFile = Path.Combine(path, "code", "code.txt");
            Assert.IsTrue(File.Exists(codeFile), "il submodule del progetto è popolato");
            File.WriteAllText(codeFile, "v2-modificato-dall-agente\n");

            var dirty = await m.GetDirtySubmodulesAsync(path);
            CollectionAssert.Contains(dirty.ToList(), "code", "il submodule toccato è rilevato");

            var gate = ctx.Factory.Services.GetRequiredService<ISubmoduleGateService>();
            await gate.RecordTouchedAsync(path, "coder", path);
            Assert.AreEqual(AgentMessage.DeferredReasonEnum.AwaitingPush, gate.CheckAwaitingPush(path), "il gate del codice è aperto");
        }

        [TestMethod]
        public void Defer_release_and_capture_the_submodule_sha_on_human_commit()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = SetupProjectWithSubmodule(ctx, "sg-release");
            var gate = ctx.Factory.Services.GetRequiredService<ISubmoduleGateService>();

            // Marker aperto (come se un agente avesse toccato 'code') + un messaggio differito.
            Guid msgId;
            using (var scope = ctx.Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                db.GetDal<SubmoduleAwaitingPush>().Save(new SubmoduleAwaitingPush
                {
                    ProjectPath = path, Submodule = "code", TouchedByAgent = "coder", CreatedAt = DateTime.UtcNow,
                });
                var msg = new AgentMessage
                {
                    ConversationId = Guid.NewGuid(), FromAgent = "user", ToAgent = "coder", ProjectPath = path,
                    State = AgentMessage.StateEnum.Pending, DeferredReason = AgentMessage.DeferredReasonEnum.AwaitingPush,
                    Attempts = 0, CreatedAt = DateTime.UtcNow,
                };
                db.GetDal<AgentMessage>().Save(msg);
                db.Commit();
                msgId = msg.Id;
            }
            Assert.AreEqual(AgentMessage.DeferredReasonEnum.AwaitingPush, gate.CheckAwaitingPush(path));

            // L'umano fa atterrare il codice: avanza l'HEAD del submodule nel superprogetto.
            var subInProject = Path.Combine(path, "code");
            File.WriteAllText(Path.Combine(subInProject, "code.txt"), "v2-umano\n");
            Git(subInProject, "add -A"); Git(subInProject, "commit -m human-lands-code");
            var expectedSha = Git(subInProject, "rev-parse HEAD").Out.Trim();

            gate.OnCommitDetected(path);

            // Gate chiuso, sha catturato, messaggio rilasciato col release token.
            Assert.IsNull(gate.CheckAwaitingPush(path), "il gate si è chiuso");
            Assert.AreEqual(expectedSha, gate.GetResolvedSubmoduleSha(path), "sha del submodule catturato");
            using (var scope = ctx.Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var msg = db.GetDal<AgentMessage>().GetList().First(x => x.Id == msgId);
                db.Commit();
                Assert.IsNull(msg.DeferredReason, "il messaggio è stato rilasciato");
                Assert.AreEqual(expectedSha, msg.SubmoduleBaseCommit, "il release token è sul messaggio");
            }
        }
    }
}
