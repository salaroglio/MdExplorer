using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Git;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// N2 della valutazione del 07/11: i file con lettere accentate nel nome. Git li scriveva fra virgolette e in ottale
    /// (<c>"perch\303\251.md"</c>), MDE toglieva solo le virgolette e cercava sul disco un file che non c'è: il controllo dei segni di
    /// conflitto non vedeva <c>perché.md</c>, e i segni potevano finire nel commit. Richiede <c>git</c> nel PATH.
    /// </summary>
    [TestClass]
    public class GitAccentedPaths_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git", Arguments = args, WorkingDirectory = cwd, UseShellExecute = false,
                    RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
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

        private static string Project(AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            Git(path, "init -q -b main");
            Git(path, "config user.email prova@test.local"); Git(path, "config user.name Prova"); Git(path, "config commit.gpgsign false");
            // Come una macchina appena installata: git cita i nomi accentati (è il suo predefinito).
            Git(path, "config core.quotePath true");
            return path;
        }

        private static void Commit(string path, string message) { Git(path, "add -A"); Git(path, $"commit -q -m \"{message}\""); }

        [TestMethod]
        public void Decode_the_paths_git_quotes()
        {
            Assert.AreEqual("perché.md", GitPaths.Unquote("\"perch\\303\\251.md\""));
            Assert.AreEqual("città/attività.md", GitPaths.Unquote("\"citt\\303\\240/attivit\\303\\240.md\""));
            Assert.AreEqual("un \"titolo\".md", GitPaths.Unquote("\"un \\\"titolo\\\".md\""));
            Assert.AreEqual("a\\b.md", GitPaths.Unquote("\"a\\\\b.md\""));
            Assert.AreEqual("tab\there.md", GitPaths.Unquote("\"tab\\there.md\""));
            Assert.AreEqual("😀.md", GitPaths.Unquote("\"\\360\\237\\230\\200.md\""));
            Assert.AreEqual("normale.md", GitPaths.Unquote("  normale.md "), "senza virgolette torna com'è");
            Assert.AreEqual("città/perché.md", GitPaths.Unquote("città/perché.md"), "già in chiaro (core.quotePath=false)");
        }

        [TestMethod]
        public async Task See_the_conflict_markers_in_a_file_with_an_accent()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = Project(ctx, "accenti-conflitto");
            File.WriteAllText(Path.Combine(path, "perché.md"), "uno\n"); Commit(path, "inizio");
            Git(path, "checkout -q -b altro");
            File.WriteAllText(Path.Combine(path, "perché.md"), "due\n"); Commit(path, "due");
            Git(path, "checkout -q main");
            File.WriteAllText(Path.Combine(path, "perché.md"), "tre\n"); Commit(path, "tre");
            Assert.AreNotEqual(0, Git(path, "merge altro").Code, "il conflitto c'è");

            var guard = ctx.Factory.Services.GetRequiredService<IRepoWorkflowGuard>();
            var merge = await guard.ReadMergeAsync(path);

            CollectionAssert.AreEqual(new[] { "perché.md" }, merge.Conflicts.ToList());
            CollectionAssert.AreEqual(new[] { "perché.md" }, merge.StillMarked.ToList(), "prima non lo trovava sul disco e lo dava per corretto");
            var blocker = await guard.CommitBlockerAsync(path);
            Assert.IsNotNull(blocker, "il commit con i segni di conflitto si ferma");
            StringAssert.Contains(blocker, "perché.md");
        }

        [TestMethod]
        public async Task List_diff_and_discard_a_file_with_accents()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = Project(ctx, "accenti-modifiche");
            Directory.CreateDirectory(Path.Combine(path, "città"));
            File.WriteAllText(Path.Combine(path, "città", "attività.md"), "# Attività\n\nprima\n");
            Commit(path, "inizio");
            File.WriteAllText(Path.Combine(path, "città", "attività.md"), "# Attività\n\ndopo\n");
            File.WriteAllText(Path.Combine(path, "novità.md"), "# nuovo\n");

            using var scope = ctx.Factory.Services.CreateScope();
            var changes = scope.ServiceProvider.GetRequiredService<IWorkingChangesService>();
            var view = await changes.GetAsync(path, null);

            var files = view.Repos.Single(r => r.Path == string.Empty).Files.Select(f => f.Path).ToList();
            CollectionAssert.Contains(files, "città/attività.md", string.Join(" | ", files));
            CollectionAssert.Contains(files, "novità.md");

            var diff = await changes.DiffAsync(path, null, "città/attività.md");
            StringAssert.Contains(diff, "+dopo");

            await changes.DiscardAsync(path, null, "città/attività.md");
            Assert.AreEqual("# Attività\n\nprima\n", File.ReadAllText(Path.Combine(path, "città", "attività.md")).Replace("\r\n", "\n"));
        }
    }
}
