using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Git;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// S2-S3 dello sprint «File di sistema di MDE»: MDE fa il commit dei suoi file appena li scrive, e il push solo quando è sicuro.
    /// Repository veri: un origin nudo, il clone di un collega che pubblica, il progetto. Richiede <c>git</c> nel PATH.
    /// </summary>
    [TestClass]
    public class MdeAutoCommit_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private static readonly Dictionary<string, string> On = new() { ["MdeAutoCommit:Enabled"] = "true", ["MdeAutoCommit:DelayMs"] = "100000" };

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
            p.WaitForExit(20000);
            return (p.ExitCode, o.Trim());
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;
        private static StringContent Json(object o) => new(JsonSerializer.Serialize(o), Encoding.UTF8, "application/json");

        /// <summary>Origin nudo, il clone del collega, e il progetto allineato e con upstream.</summary>
        private static (string Project, string Origin, string Colleague) Setup(AgentCityContext ctx, string name)
        {
            var root = Path.Combine(ctx.Factory.DataDir, "auto-commit", name);
            var origin = Path.Combine(root, "origin.git");
            Directory.CreateDirectory(origin);
            Git(origin, "init -q --bare -b main");
            Git(root, $"clone -q \"{origin}\" collega");
            var colleague = Path.Combine(root, "collega");
            File.WriteAllText(Path.Combine(colleague, "README.md"), "# progetto\n");
            Git(colleague, "add -A"); Git(colleague, "commit -q -m inizio"); Git(colleague, "push -q origin HEAD:main");

            var (_, project) = ctx.SeedProject(name);
            Git(project, "init -q -b main");
            Git(project, $"remote add origin \"{origin}\"");
            Git(project, "pull -q origin main");
            Git(project, "branch -q --set-upstream-to=origin/main main");
            return (project, origin, colleague);
        }

        private static async Task<MdeAutoCommitResult> SaveHarnessAndFlush(AgentCityContext ctx, string project, string target = "claude")
        {
            var r = await ctx.Client.PostAsync("/api/ProjectSettings/SetHarness", Json(new { projectPath = project, target }));
            Assert.IsTrue(r.IsSuccessStatusCode, await r.Content.ReadAsStringAsync());
            var service = ctx.Factory.Services.GetRequiredService<MdeAutoCommit>();
            await service.FlushAsync();
            return service.RecentResults.LastOrDefault();
        }

        [TestMethod]
        public async Task Commit_only_its_own_file_and_push_it_when_it_is_safe()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, origin, _) = Setup(ctx, "sicuro");

            var result = await SaveHarnessAndFlush(ctx, project);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Committed, string.Join(" | ", result.Notices));
            Assert.IsTrue(result.Pushed, string.Join(" | ", result.Notices));
            StringAssert.StartsWith(Git(project, "log -1 --format=%s").Out, "MDE: ");
            Assert.AreEqual(".development.yml", Git(project, "show --name-only --format= HEAD").Out, "nel commit c'è solo il file di MDE");
            Assert.AreEqual(Git(project, "rev-parse HEAD").Out, Git(origin, "rev-parse main").Out, "pubblicato");
            Assert.AreEqual(string.Empty, Git(project, "status --porcelain -- .development.yml").Out);
        }

        [TestMethod]
        public async Task Leave_the_user_staged_change_staged_and_out_of_its_commit()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, _, _) = Setup(ctx, "stage");
            File.WriteAllText(Path.Combine(project, "README.md"), "# progetto\nla mia riga\n");
            Git(project, "add README.md");

            var result = await SaveHarnessAndFlush(ctx, project);

            Assert.IsTrue(result.Committed, string.Join(" | ", result.Notices));
            Assert.AreEqual(".development.yml", Git(project, "show --name-only --format= HEAD").Out);
            Assert.AreEqual("M  README.md", Git(project, "status --porcelain -- README.md").Out, "la modifica dell'utente resta in stage");
        }

        [TestMethod]
        public async Task Not_push_when_the_user_has_commits_of_their_own_to_publish()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, origin, _) = Setup(ctx, "commit-utente");
            File.WriteAllText(Path.Combine(project, "nota.md"), "# mia\n");
            Git(project, "add nota.md"); Git(project, "commit -q -m \"la mia nota\"");
            var originBefore = Git(origin, "rev-parse main").Out;

            var result = await SaveHarnessAndFlush(ctx, project);

            Assert.IsTrue(result.Committed, string.Join(" | ", result.Notices));
            Assert.IsFalse(result.Pushed);
            Assert.AreEqual(MdeCommitPending.UserCommitsToPublish, result.PendingKind);
            Assert.AreEqual(originBefore, Git(origin, "rev-parse main").Out, "niente pubblicato: si porterebbe dietro la nota dell'utente");
        }

        [TestMethod]
        public async Task Not_push_when_the_remote_has_news()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, origin, colleague) = Setup(ctx, "novita");
            File.WriteAllText(Path.Combine(colleague, "nuovo.md"), "# nuovo\n");
            Git(colleague, "add -A"); Git(colleague, "commit -q -m nuovo"); Git(colleague, "push -q origin HEAD:main");
            var originBefore = Git(origin, "rev-parse main").Out;

            var result = await SaveHarnessAndFlush(ctx, project);

            Assert.IsTrue(result.Committed, string.Join(" | ", result.Notices));
            Assert.IsFalse(result.Pushed);
            Assert.AreEqual(MdeCommitPending.RemoteHasNews, result.PendingKind);
            Assert.AreEqual(originBefore, Git(origin, "rev-parse main").Out);
        }

        [TestMethod]
        public async Task Wait_while_a_merge_is_half_done()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, _, _) = Setup(ctx, "unione-a-meta");
            var head = Git(project, "rev-parse HEAD").Out;
            File.WriteAllText(Path.Combine(project, ".git", "MERGE_HEAD"), head + "\n");
            try
            {
                var result = await SaveHarnessAndFlush(ctx, project);
                Assert.IsFalse(result.Committed);
                Assert.AreEqual(MdeCommitPending.MergeInProgress, result.PendingKind);
                Assert.IsTrue(result.Notices.Any(n => n.Contains("Commit postponed")), "si dice che il commit è rimandato");
                Assert.AreEqual(head, Git(project, "rev-parse HEAD").Out, "nessun commit");
            }
            finally { File.Delete(Path.Combine(project, ".git", "MERGE_HEAD")); }
        }
        // ---------------------------------------------------------------- S3: notifiche con il motivo

        [TestMethod]
        public async Task Tell_what_it_changed_why_and_whether_it_was_pushed()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, _, _) = Setup(ctx, "notifica");

            var result = await SaveHarnessAndFlush(ctx, project);

            // SetHarness scrive anche .gitignore: due notifiche, una per il file di MDE e una per quello dell'utente
            Assert.AreEqual(2, result.Notices.Count, string.Join(" | ", result.Notices));
            var notice = result.Notices.Single(n => n.Contains(".development.yml"));
            StringAssert.Contains(result.Notices.Single(n => n.Contains(".gitignore")), "The file is yours");
            // senza UiText.Use i testi del servizio sono in inglese (vedi server_texts_follow_ui_language)
            StringAssert.Contains(notice, ".development.yml");
            StringAssert.Contains(notice, "project agent environment chosen", "il motivo specifico, non uno generico");
            StringAssert.Contains(notice, "Committed and pushed");
            StringAssert.Contains(Git(project, "log -1 --format=%s").Out, "scelto l'ambiente agentico del progetto", "il commit è in italiano");
        }

        [TestMethod]
        public async Task Tell_about_gitignore_without_committing_it()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, _, _) = Setup(ctx, "gitignore");
            var head = Git(project, "rev-parse HEAD").Out;

            MdExplorer.Service.ProjectsManager.EnsureGitignoreEntries(project);
            var service = ctx.Factory.Services.GetRequiredService<MdeAutoCommit>();
            await service.FlushAsync();
            var result = service.RecentResults.LastOrDefault();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Committed);
            Assert.AreEqual(head, Git(project, "rev-parse HEAD").Out, ".gitignore è dell'utente: nessun commit");
            Assert.AreEqual("?? .gitignore", Git(project, "status --porcelain -- .gitignore").Out);
            Assert.AreEqual(1, result.Notices.Count, string.Join(" | ", result.Notices));
            StringAssert.Contains(result.Notices[0], ".gitignore");
            StringAssert.Contains(result.Notices[0], ".md/");
            StringAssert.Contains(result.Notices[0], "The file is yours");
        }

        [TestMethod]
        public async Task Say_nothing_when_nothing_changed()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, _, _) = Setup(ctx, "silenzio");
            await SaveHarnessAndFlush(ctx, project);
            var service = ctx.Factory.Services.GetRequiredService<MdeAutoCommit>();
            var before = service.RecentResults.Count;

            await SaveHarnessAndFlush(ctx, project); // stessa scelta: il file non si riscrive, nessun avviso

            Assert.AreEqual(before, service.RecentResults.Count, "nessuna scrittura, nessuna notifica");
        }
    }
}
