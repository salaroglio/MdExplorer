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
    }
}
