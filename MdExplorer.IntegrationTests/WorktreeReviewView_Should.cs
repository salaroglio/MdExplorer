using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.DatabaseManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 7h — review-view READ-ONLY dei worktree. Il render di un doc dal worktree di un
    /// agente NON deve scrivere nulla (né cache <c>.md/</c>, né EngineDB del progetto aperto) e
    /// deve lasciare intatta la working tree dell'umano. <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class WorktreeReviewView_Should
    {
        // Il render normale del progetto fa Directory.SetCurrentDirectory (hazard globale noto,
        // §7h): un test precedente può lasciare la cwd in una temp dir cancellata → Process.Start
        // (git) fallirebbe a GetCwd(). Reset a una dir stabile a inizio di ogni test.
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

        private static string SetupGitProject(AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin);
            Git(origin, "init --bare");
            Git(path, "init -b main");
            Git(path, "config user.email agent@test.local");
            Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# base\n");
            Git(path, "add -A");
            Git(path, "commit -m base");
            Git(path, $"remote add origin \"{origin}\"");
            Git(path, "push -u origin main");
            return path;
        }

        [TestMethod]
        public async Task Render_a_worktree_doc_readonly_without_polluting_the_project()
        {
            if (Git(Path.GetTempPath(), "--version").Code != 0) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "wt-review");
            var connectionId = "conn-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            ctx.Factory.Services.GetRequiredService<IDatabaseManager>().RegisterConnection(connectionId, path);

            // Prepara il worktree dell'agente e ci scrive un documento (output simulato dell'agente).
            var manager = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();
            var prep = await manager.PrepareForRunAsync(path, "worker", "review1");
            Assert.IsTrue(prep.Success, prep.Error);
            var worktree = prep.WorktreePath;
            File.WriteAllText(Path.Combine(worktree, "review.md"), "# Prodotto dall'agente\n\nContenuto UNICO 7h.\n");

            // Render read-only del doc dal worktree.
            var resp = await ctx.Client.GetAsync(
                $"/api/MdExplorerWorktree/render/review?agent=worker&connectionId={connectionId}&theme=light");
            var body = await resp.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, body);
            StringAssert.Contains(body, "Contenuto UNICO 7h", "il doc del worktree è renderizzato");

            // Read-only: nessuna cache .md/ scritta nel worktree.
            var mdCacheDir = Path.Combine(worktree, ".md");
            var cacheFiles = Directory.Exists(mdCacheDir)
                ? Directory.GetFiles(mdCacheDir, "*.html", SearchOption.AllDirectories) : Array.Empty<string>();
            Assert.AreEqual(0, cacheFiles.Length, "nessuna cache HTML scritta nel worktree");

            // Read-only: nessuna riga EngineDB del progetto per un file del worktree.
            var dbm = ctx.Factory.Services.GetRequiredService<IDatabaseManager>();
            using (var engine = dbm.CreateIsolatedEngineDBForProjectPath(path))
            {
                engine.BeginTransaction();
                var polluted = engine.GetDal<MarkdownFile>().GetList()
                    .Count(m => m.Path != null && m.Path.Contains("worktrees"));
                engine.Commit();
                Assert.AreEqual(0, polluted, "il render del worktree NON scrive nell'EngineDB del progetto");
            }

            // La working tree dell'umano resta intatta (il doc dell'agente non compare nel progetto).
            Assert.IsFalse(File.Exists(Path.Combine(path, "review.md")), "il progetto principale è intatto");
        }

        [TestMethod]
        public async Task Reject_path_traversal_out_of_the_worktree()
        {
            if (Git(Path.GetTempPath(), "--version").Code != 0) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "wt-traversal");
            var connectionId = "conn-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            ctx.Factory.Services.GetRequiredService<IDatabaseManager>().RegisterConnection(connectionId, path);
            await ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>().EnsureWorktreeAsync(path, "worker");

            // agent = ".." → nome non valido (WorktreePathFor allowlist): 400, niente escape della dir.
            var r1 = await ctx.Client.GetAsync($"/api/MdExplorerWorktree/render/x?agent=..&connectionId={connectionId}");
            Assert.AreEqual(HttpStatusCode.BadRequest, r1.StatusCode);

            // url con '..' verso un file esterno: mai servito (bloccato dalla guardia di containment).
            var r2 = await ctx.Client.GetAsync(
                $"/api/MdExplorerWorktree/render/..%2F..%2F..%2F..%2F..%2Fetc%2Fpasswd?agent=worker&connectionId={connectionId}");
            Assert.AreNotEqual(HttpStatusCode.OK, r2.StatusCode, "il traversal non deve mai restituire 200");
            var body = await r2.Content.ReadAsStringAsync();
            Assert.IsFalse(body.Contains("root:"), "nessun contenuto di /etc/passwd trapelato");
        }

        [TestMethod]
        public async Task List_the_worktrees_of_the_open_project()
        {
            if (Git(Path.GetTempPath(), "--version").Code != 0) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "wt-list");
            var connectionId = "conn-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            ctx.Factory.Services.GetRequiredService<IDatabaseManager>().RegisterConnection(connectionId, path);

            var manager = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();
            // Un posto vuoto non e' "il worktree di worker": con il pool l'occupante lo dice il
            // branch in checkout, quindi serve un prepare vero, non solo la cartella.
            var prep = await manager.PrepareForRunAsync(path, "worker", "att1");
            Assert.IsTrue(prep.Success, prep.Error);

            var (status, json) = await ctx.GetJson($"/api/MdExplorerWorktree/list?connectionId={connectionId}");
            Assert.AreEqual(HttpStatusCode.OK, status);
            var agents = json.RootElement.GetProperty("worktrees").EnumerateArray()
                .Select(w => w.GetProperty("agent").GetString()).ToList();
            CollectionAssert.Contains(agents, "worker", "il worktree dell'agente è elencato");
        }
    }
}
