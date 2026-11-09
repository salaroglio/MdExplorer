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

        /// <summary>Nell'ultimo commit solo file di MDE: SetHarness scrive .development.yml e crea CLAUDE.md.</summary>
        private static void AssertOnlyMdeFilesIn(string project)
        {
            var files = Git(project, "show --name-only --format= HEAD").Out.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            CollectionAssert.Contains(files, ".development.yml");
            foreach (var f in files) Assert.IsNotNull(MdExplorer.Utilities.MdeOwnedFiles.Find(f), "nel commit di MDE c'è un file non suo: " + f);
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
            AssertOnlyMdeFilesIn(project);
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
            AssertOnlyMdeFilesIn(project);
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
        public async Task Tell_what_it_changed_and_whether_it_was_pushed()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, _, _) = Setup(ctx, "notifica");

            var result = await SaveHarnessAndFlush(ctx, project);

            // SetHarness scrive anche .gitignore: due notifiche, una per il file di MDE e una per quello dell'utente
            Assert.AreEqual(2, result.Notices.Count, string.Join(" | ", result.Notices));
            var notice = result.Notices.Single(n => n.Contains(".development.yml"));
            StringAssert.Contains(result.Notices.Single(n => n.Contains(".gitignore")), "The change will go into your next commit");
            // senza UiText.Use i testi del servizio sono in inglese (vedi server_texts_follow_ui_language)
            StringAssert.Contains(notice, ".development.yml");
            // 09/11/2026: la notifica non si giustifica, il motivo sta nel commit (e nel log)
            Assert.IsFalse(notice.Contains("project agent environment chosen"), "il motivo non va nella notifica: " + notice);
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
            StringAssert.Contains(result.Notices[0], "The change will go into your next commit");
            Assert.IsFalse(result.Notices[0].Contains(".md/"), "le righe aggiunte sono il motivo: non vanno nella notifica");
        }

        /// <summary>
        /// 09/11/2026: nella notifica, al posto del motivo, ci va solo ciò che MDE farà dopo e che conviene sapere: qui, che i
        /// colleghi con un MDE più vecchio dovranno aggiornarlo.
        /// </summary>
        [TestMethod]
        public async Task Tell_only_the_future_behaviour_when_the_schema_rises()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, _, _) = Setup(ctx, "schema-nota");
            var path = Path.Combine(project, ".development.yml");
            File.WriteAllText(path, "agentCity:\n  enabled: true\n");

            Assert.IsTrue(MdExplorer.Utilities.DevelopmentConfigWriter.AlignForCity(path), "il file vecchio va portato al formato di oggi");
            var service = ctx.Factory.Services.GetRequiredService<MdeAutoCommit>();
            await service.FlushAsync();
            var notice = service.RecentResults.Last().Notices.Single(n => n.Contains(".development.yml"));

            StringAssert.StartsWith(notice, "MDE updated .development.yml.");
            StringAssert.Contains(notice, "whoever has an older MDE will have to update it");
            Assert.IsFalse(notice.Contains("raised .development.yml"), "il motivo non va nella notifica: " + notice);
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
        // ---------------------------------------------------------------- S4: il blocco nelle istruzioni, all'apertura vera

        private static async Task<MdeAutoCommit> OpenAndFlush(AgentCityContext ctx, string project, string connection)
        {
            var r = await ctx.Client.PostAsync($"/api/MdProjects/SetFolderProject?ConnectionId={connection}", Json(new { path = project }));
            Assert.IsTrue(r.IsSuccessStatusCode, await r.Content.ReadAsStringAsync());
            var service = ctx.Factory.Services.GetRequiredService<MdeAutoCommit>();
            await service.FlushAsync();
            return service;
        }

        [TestMethod]
        public async Task Migrate_the_old_paragraph_to_the_block_on_opening_and_commit_it()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, origin, colleague) = Setup(ctx, "migra-blocco");
            // il progetto com'era prima: harness claude, CLAUDE.md con il paragrafo senza marcatori e una riga del gruppo
            File.WriteAllText(Path.Combine(colleague, ".development.yml"), "harness:\n  target: claude\n");
            File.WriteAllText(Path.Combine(colleague, "CLAUDE.md"),
                "# Istruzioni\n\nRegola del gruppo.\n\n## Skill MdExplorer\n\n" + MdExplorer.Utilities.MdeInstructionsBlock.MdeDocSentence(".claude/skills/mde-doc/SKILL.md") + "\n");
            Git(colleague, "add -A"); Git(colleague, "commit -q -m prima"); Git(colleague, "push -q origin HEAD:main");
            Git(project, "pull -q");

            var service = await OpenAndFlush(ctx, project, "s4-uno");

            var claude = File.ReadAllText(Path.Combine(project, "CLAUDE.md"));
            StringAssert.Contains(claude, "<!-- mde:skill v1 -->");
            StringAssert.StartsWith(claude, "# Istruzioni\n\nRegola del gruppo.\n\n", "il resto del file è del gruppo");
            var commits = Git(project, "log --format=%s @{u}..HEAD").Out + Git(project, "log --format=%s -5").Out;
            StringAssert.Contains(commits, "MDE: ", "la migrazione è in un commit di MDE");
            StringAssert.Contains(Git(project, "log --format=%B -5").Out, "il paragrafo sulle skill di MDE ora è delimitato");
            Assert.AreEqual(string.Empty, Git(project, "status --porcelain -- CLAUDE.md .development.yml").Out, "nessun file di MDE lasciato a metà");
            Assert.IsTrue(service.RecentResults.Any(r => r.Notices.Any(n => n.Contains("CLAUDE.md"))), "e lo si dice");

            // riaperto: niente da fare, nessun commit in più
            var head = Git(project, "rev-parse HEAD").Out;
            await OpenAndFlush(ctx, project, "s4-due");
            Assert.AreEqual(head, Git(project, "rev-parse HEAD").Out, "la seconda apertura non cambia niente");
        }

        [TestMethod]
        public async Task Leave_no_mde_file_out_of_a_commit_after_the_first_opening()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, _, _) = Setup(ctx, "prima-apertura");

            await OpenAndFlush(ctx, project, "s4-prima");

            var dirty = Git(project, "status --porcelain").Out.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Substring(3).Trim('"'))
                .Where(f => MdExplorer.Utilities.MdeOwnedFiles.Find(f) != null).ToList();
            Assert.AreEqual(0, dirty.Count, "file di MDE non salvati: " + string.Join(", ", dirty));
        }

        // ---- N6 (QA 07/11): MDE aspetta il suo turno se è partita prima un'operazione git dell'utente ----

        private static readonly Dictionary<string, string> Quick = new() { ["MdeAutoCommit:Enabled"] = "true", ["MdeAutoCommit:DelayMs"] = "300" };

        private static async Task<bool> Eventually(Func<bool> condition, int ms = 10000)
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (clock.ElapsedMilliseconds < ms) { if (condition()) return true; await Task.Delay(100); }
            return condition();
        }

        [TestMethod]
        public async Task Wait_for_the_user_git_operation_that_started_first()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(Quick);
            var (project, _, _) = Setup(ctx, "turno");
            var before = Git(project, "rev-list --count HEAD").Out;

            var userOperation = UserGitActivity.Begin();
            try
            {
                var r = await ctx.Client.PostAsync("/api/ProjectSettings/SetHarness", Json(new { projectPath = project, target = "claude" }));
                Assert.IsTrue(r.IsSuccessStatusCode, await r.Content.ReadAsStringAsync());
                await Task.Delay(1500);
                Assert.AreEqual(before, Git(project, "rev-list --count HEAD").Out, "l'operazione dell'utente è in corso: MDE aspetta");
            }
            finally { userOperation.Dispose(); }

            Assert.IsTrue(await Eventually(() => Git(project, "rev-list --count HEAD").Out != before), "finita l'operazione dell'utente, MDE fa il suo commit");
            StringAssert.StartsWith(Git(project, "log -1 --format=%s").Out, "MDE: ");
        }

        [TestMethod]
        public async Task Not_wait_for_itself_when_its_pull_saves_its_files_first()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(Quick);
            var (project, _, _) = Setup(ctx, "turno-pull");
            var service = ctx.Factory.Services.GetRequiredService<MdeAutoCommit>();

            using (UserGitActivity.Begin())   // il pull dell'utente è in corso, e dentro salva prima i file di MDE
            {
                var r = await ctx.Client.PostAsync("/api/ProjectSettings/SetHarness", Json(new { projectPath = project, target = "claude" }));
                Assert.IsTrue(r.IsSuccessStatusCode, await r.Content.ReadAsStringAsync());
                await Task.Delay(800);   // il giro di MDE è partito e aspetta il turno
                var save = service.CommitPendingAsync(project);
                Assert.AreSame(save, await Task.WhenAny(save, Task.Delay(15000)), "il pull non resta ad aspettare sé stesso");
            }
            StringAssert.StartsWith(Git(project, "log -1 --format=%s").Out, "MDE: ");
        }
    }
}
