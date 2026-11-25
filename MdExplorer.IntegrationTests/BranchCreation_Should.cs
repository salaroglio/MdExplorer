using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Git;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// «Nuovo ramo» dalla title bar (sprint 2026-11-25-Nuovo-Ramo-E-Lavoro-All-Agente, A1), su repository veri con un origin
    /// locale: il lavoro aperto viene con te senza cambiare, il ramo si pubblica, e ogni situazione rischiosa si rifiuta o si dice.
    /// </summary>
    [TestClass]
    public class BranchCreation_Should
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "newbranch_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
        }

        private static string Git(string dir, params string[] args)
        {
            var psi = new ProcessStartInfo("git") { WorkingDirectory = dir, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var a in new[] { "-c", "user.email=mario.rossi@example.com", "-c", "user.name=Mario Rossi" }.Concat(args)) psi.ArgumentList.Add(a);
            using var p = Process.Start(psi);
            var output = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
            p.WaitForExit();
            return output.Trim();
        }

        /// <summary>Un origin «GitHub» locale e la cartella dell'utente, con un commit pubblicato su main.</summary>
        private (string origin, string user) Repo()
        {
            var origin = Path.Combine(_root, "origin.git");
            var user = Path.Combine(_root, "user");
            Git(_root, "init", "-q", "--bare", "-b", "main", origin);
            Git(_root, "clone", "-q", origin, user);
            File.WriteAllText(Path.Combine(user, "a.txt"), "uno\n");
            File.WriteAllText(Path.Combine(user, "b.txt"), "due\n");
            Git(user, "add", ".");
            Git(user, "commit", "-qm", "init");
            Git(user, "push", "-q", "origin", "main");
            return (origin, user);
        }

        private static (AgentCityContext ctx, BranchCreationService service, IServiceScope scope) Service()
        {
            var ctx = new AgentCityContext();
            var scope = ctx.Factory.Services.CreateScope();
            return (ctx, scope.ServiceProvider.GetRequiredService<BranchCreationService>(), scope);
        }

        [TestMethod]
        public async Task Carry_the_open_work_unchanged_and_publish_the_new_branch()
        {
            var (origin, user) = Repo();
            File.WriteAllText(Path.Combine(user, "a.txt"), "uno modificato\n");
            File.WriteAllText(Path.Combine(user, "b.txt"), "due in coda\n");
            Git(user, "add", "b.txt");
            File.WriteAllText(Path.Combine(user, "b.txt"), "due in coda e poi ancora\n");
            File.WriteAllText(Path.Combine(user, "nuovo.txt"), "nuovo\n");
            File.WriteAllText(Path.Combine(user, "locale.txt"), "x\n");
            var before = Git(user, "status", "--porcelain");
            var (ctx, service, scope) = Service();
            using var _ = ctx; using var __ = scope;

            var preview = await service.PreviewAsync(user);
            Assert.AreEqual("main", preview.CurrentBranch);
            Assert.IsNull(preview.Blocker);
            CollectionAssert.AreEquivalent(new[] { "a.txt", "b.txt", "nuovo.txt", "locale.txt" }, preview.CarriedFiles.ToArray());
            StringAssert.StartsWith(preview.SuggestedPrefix, DateTime.Now.ToString("yyyy-MM-dd"));

            var result = await service.CreateAsync(user, "2026-11-25-prova");

            Assert.IsTrue(result.Created, result.Error);
            Assert.IsTrue(result.Published, result.PublishProblem);
            Assert.AreEqual("2026-11-25-prova", Git(user, "branch", "--show-current"));
            Assert.AreEqual(before, Git(user, "status", "--porcelain"), "il lavoro aperto è venuto con te, identico");
            Assert.AreEqual("uno modificato", File.ReadAllText(Path.Combine(user, "a.txt")).Trim());
            Assert.AreEqual("origin/2026-11-25-prova", Git(user, "rev-parse", "--abbrev-ref", "@{upstream}"));
            StringAssert.Contains(Git(origin, "branch"), "2026-11-25-prova", "il ramo è su GitHub");
        }

        [TestMethod]
        public async Task Count_the_unpublished_commits_that_go_along()
        {
            var (_, user) = Repo();
            File.WriteAllText(Path.Combine(user, "c.txt"), "c\n");
            Git(user, "add", "c.txt");
            Git(user, "commit", "-qm", "non pubblicato");
            var (ctx, service, scope) = Service();
            using var _ = ctx; using var __ = scope;

            Assert.AreEqual(1, (await service.PreviewAsync(user)).UnpublishedCommits);
        }

        [TestMethod]
        public async Task Refuse_bad_or_taken_names_also_when_taken_only_on_github()
        {
            var (origin, user) = Repo();
            var colleague = Path.Combine(_root, "collega");
            Git(_root, "clone", "-q", origin, colleague);
            Git(colleague, "switch", "-qc", "del-collega");
            Git(colleague, "push", "-q", "origin", "del-collega");
            var (ctx, service, scope) = Service();
            using var _ = ctx; using var __ = scope;

            Assert.IsFalse((await service.CreateAsync(user, "con spazio")).Created);
            Assert.IsFalse((await service.CreateAsync(user, "a..b")).Created);
            Assert.IsFalse((await service.CreateAsync(user, "main")).Created, "esiste in locale");
            var remoteOnly = await service.CreateAsync(user, "del-collega");
            Assert.IsFalse(remoteOnly.Created, "esiste solo su GitHub: switch -c passerebbe, e non deve");
            StringAssert.Contains(remoteOnly.Error, "del-collega");
            Assert.AreEqual("main", Git(user, "branch", "--show-current"), "niente è cambiato");
        }

        [TestMethod]
        public async Task Refuse_during_a_merge_left_half_done()
        {
            var (origin, user) = Repo();
            var colleague = Path.Combine(_root, "collega");
            Git(_root, "clone", "-q", origin, colleague);
            File.WriteAllText(Path.Combine(colleague, "a.txt"), "del collega\n");
            Git(colleague, "commit", "-qam", "collega");
            Git(colleague, "push", "-q", "origin", "main");
            File.WriteAllText(Path.Combine(user, "a.txt"), "mio\n");
            Git(user, "commit", "-qam", "mio");
            Git(user, "pull", "-q", "--no-rebase", "origin", "main");   // conflitto: unione a metà
            var (ctx, service, scope) = Service();
            using var _ = ctx; using var __ = scope;

            Assert.IsNotNull((await service.PreviewAsync(user)).Blocker);
            var result = await service.CreateAsync(user, "durante-unione");
            Assert.IsFalse(result.Created);
            Assert.AreEqual("main", Git(user, "branch", "--show-current"));
        }

        [TestMethod]
        public async Task Say_that_a_branch_from_a_detached_head_starts_from_a_commit()
        {
            var (_, user) = Repo();
            Git(user, "checkout", "-q", "--detach");
            var (ctx, service, scope) = Service();
            using var _ = ctx; using var __ = scope;

            var result = await service.CreateAsync(user, "da-staccato");

            Assert.IsTrue(result.Created, result.Error);
            Assert.IsTrue(result.Notes.Any(n => n.Contains(Git(user, "rev-parse", "--short", "HEAD"))), string.Join(" | ", result.Notes));
        }

        [TestMethod]
        public async Task Keep_the_branch_when_it_cannot_be_published_and_say_why()
        {
            var (origin, user) = Repo();
            var (ctx, service, scope) = Service();
            using var _ = ctx; using var __ = scope;
            Directory.Delete(origin, recursive: true);   // GitHub non risponde

            var result = await service.CreateAsync(user, "senza-rete");

            Assert.IsTrue(result.Created, "il ramo c'è");
            Assert.IsFalse(result.Published);
            Assert.IsNotNull(result.PublishProblem);
            Assert.AreEqual("senza-rete", Git(user, "branch", "--show-current"));
            Assert.IsTrue(result.Notes.Count > 0, "il nome non si è potuto controllare su GitHub, e lo si dice");

            Directory.CreateDirectory(origin);
            Git(_root, "init", "-q", "--bare", "-b", "main", origin);
            Assert.IsTrue((await service.PublishAsync(user, "senza-rete")).Published, "«Pubblica» lo rifà");
        }

        [TestMethod]
        public async Task Create_without_a_remote_and_say_it_stays_on_this_computer()
        {
            var user = Path.Combine(_root, "solo");
            Directory.CreateDirectory(user);
            Git(user, "init", "-q", "-b", "main");
            File.WriteAllText(Path.Combine(user, "a.txt"), "a\n");
            Git(user, "add", ".");
            Git(user, "commit", "-qm", "init");
            var (ctx, service, scope) = Service();
            using var _ = ctx; using var __ = scope;

            var result = await service.CreateAsync(user, "locale");

            Assert.IsTrue(result.Created);
            Assert.IsFalse(result.Published);
            StringAssert.Contains(result.PublishProblem, "origin");
        }

        [TestMethod]
        public async Task With_a_commit_message_record_the_open_work_new_files_included_in_the_new_branch_and_publish_it()
        {
            var (origin, user) = Repo();
            File.WriteAllText(Path.Combine(user, ".gitignore"), "");   // un progetto MDE vero: EnsureGitignoreEntries ci mette .worktrees/
            Git(user, "add", ".gitignore"); Git(user, "commit", "-qm", "gitignore"); Git(user, "push", "-q", "origin", "main");
            File.WriteAllText(Path.Combine(user, "a.txt"), "uno modificato\n");
            File.WriteAllText(Path.Combine(user, "nuovo.txt"), "nuovo\n");
            Directory.CreateDirectory(Path.Combine(user, ".worktrees", "agente"));
            File.WriteAllText(Path.Combine(user, ".worktrees", "agente", "x.txt"), "copia dell'agente\n");
            var (ctx, service, scope) = Service();
            using var _ = ctx; using var __ = scope;

            var result = await service.CreateAsync(user, "2026-11-25-per-agente", "lavoro per l'agente");

            Assert.IsTrue(result.Created, result.Error);
            Assert.IsTrue(result.Committed, result.CommitProblem);
            Assert.IsTrue(result.Published, result.PublishProblem);
            Assert.AreEqual("lavoro per l'agente", Git(user, "log", "-1", "--format=%s"));
            var inCommit = Git(user, "show", "--name-only", "--format=", "HEAD");
            StringAssert.Contains(inCommit, "a.txt");
            StringAssert.Contains(inCommit, "nuovo.txt", "anche i file nuovi");
            Assert.IsFalse(inCommit.Contains(".worktrees"), "le copie degli agenti restano fuori: " + inCommit);
            Assert.AreEqual(Git(user, "rev-parse", "HEAD"), Git(origin, "rev-parse", "2026-11-25-per-agente"), "su GitHub c'è il ramo con il lavoro");
            Assert.AreEqual("main", Git(user, "rev-parse", "--abbrev-ref", "main@{upstream}").Replace("origin/", ""));
            Assert.AreNotEqual(Git(user, "rev-parse", "main"), Git(user, "rev-parse", "HEAD"), "main non è toccato");
        }

        [TestMethod]
        public async Task With_a_commit_message_and_nothing_open_just_create_the_branch()
        {
            var (_, user) = Repo();
            var (ctx, service, scope) = Service();
            using var _ = ctx; using var __ = scope;

            var result = await service.CreateAsync(user, "vuoto", "niente da registrare");

            Assert.IsTrue(result.Created && result.Published, result.Error ?? result.PublishProblem);
            Assert.IsFalse(result.Committed);
            Assert.AreEqual("init", Git(user, "log", "-1", "--format=%s"));
        }

        [TestMethod]
        public async Task When_the_commit_fails_keep_the_work_open_and_do_not_publish()
        {
            var (origin, user) = Repo();
            File.WriteAllText(Path.Combine(user, "a.txt"), "uno modificato\n");
            var hook = Path.Combine(user, ".git", "hooks", "pre-commit");
            File.WriteAllText(hook, "#!/bin/sh\necho rifiutato >&2\nexit 1\n");
            File.SetUnixFileMode(hook, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            var (ctx, service, scope) = Service();
            using var _ = ctx; using var __ = scope;

            var result = await service.CreateAsync(user, "commit-rifiutato", "non passerà");

            Assert.IsTrue(result.Created);
            Assert.IsFalse(result.Committed);
            Assert.IsNotNull(result.CommitProblem);
            Assert.IsFalse(result.Published);
            Assert.AreEqual("commit-rifiutato", Git(user, "branch", "--show-current"));
            StringAssert.Contains(Git(user, "status", "--porcelain"), "a.txt", "il lavoro è ancora aperto");
            Assert.IsFalse(Git(origin, "branch").Contains("commit-rifiutato"));
        }
    }
}
