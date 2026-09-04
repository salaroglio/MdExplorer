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
    /// Il pubblica-tutto, e la sola cosa che deve garantire: <b>non produrre mai un repository
    /// rotto per gli altri</b>.
    /// <para>
    /// Il disastro è che il progetto pubblichi un riferimento a un commit che sul remoto del
    /// submodule non esiste: chi clona ottiene un repository rotto, e se ne accorge lui, giorni
    /// dopo. La difesa è l'ordine — i figli prima, il padre per ultimo — e questi test la
    /// verificano dai due lati: quando funziona, e quando si deve fermare.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class SafePush_Should
    {
        [TestMethod]
        public async Task Publish_the_child_before_the_parent_so_a_clone_works()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, parentOrigin) = SetupWithSubmodule(ctx, "push-catena");

            // Lo scenario del disastro: commit nel figlio NON pubblicato, e il progetto che ne
            // registra il nuovo riferimento.
            File.WriteAllText(Path.Combine(sub, "codice.md"), "# codice\nv2\n");
            Git(sub, "add -A"); Git(sub, "commit -m v2");
            Git(path, "add -A"); Git(path, "commit -m \"bump figlio\"");

            var result = await Push(ctx, path);

            Assert.IsNull(result.Refused, result.Refused);
            Assert.IsTrue(result.Success, string.Join(" | ", result.Steps.Select(s => s.Repo + ": " + s.Outcome)));

            // L'ordine è la garanzia: il figlio deve comparire PRIMA del padre.
            var order = result.Steps.Select(s => s.Repo).ToList();
            Assert.AreEqual("figlio", order.First());
            Assert.AreEqual(string.Empty, order.Last());

            // La prova vera non è nei nostri passi: è che un altro riesca a clonare.
            var collega = Path.Combine(ctx.Factory.DataDir, "collega-catena");
            Git(ctx.Factory.DataDir, $"clone -q \"{parentOrigin}\" \"{collega}\"");
            var (code, _) = Git(collega, "submodule update --init");
            Assert.AreEqual(0, code, "chi clona deve ottenere un repository sano.");
            StringAssert.Contains(File.ReadAllText(Path.Combine(collega, "figlio", "codice.md")), "v2");
        }

        [TestMethod]
        public async Task Refuse_and_leave_the_parent_untouched_when_a_child_cannot_be_published()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, parentOrigin) = SetupWithSubmodule(ctx, "push-rifiuto");

            // Commit nel figlio fuori da ogni ramo: non è pubblicabile, e nessuno può inventare
            // su quale ramo metterlo.
            Git(sub, "checkout -q --detach");
            File.WriteAllText(Path.Combine(sub, "codice.md"), "# codice\norfano\n");
            Git(sub, "add -A"); Git(sub, "commit -m orfano");
            Git(path, "add -A"); Git(path, "commit -m \"bump figlio staccato\"");

            var prima = Git(parentOrigin, "rev-parse refs/heads/main").Out.Trim();

            var result = await Push(ctx, path);

            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Refused, "deve dire PERCHE' non si può, non fallire e basta.");
            StringAssert.Contains(result.Refused, "figlio");
            Assert.AreEqual(0, result.Steps.Count, "rifiutato PRIMA di toccare qualsiasi remoto.");

            var dopo = Git(parentOrigin, "rev-parse refs/heads/main").Out.Trim();
            Assert.AreEqual(prima, dopo,
                "il remoto del progetto deve essere rimasto vecchio ma coerente, mai rotto.");
        }

        [TestMethod]
        public async Task Say_what_stays_behind_because_it_was_never_committed()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _) = SetupWithSubmodule(ctx, "push-resta-indietro");

            // Non committato: pubblicare non lo porta via, ed è giusto. Ma va detto, altrimenti
            // si crede di aver pubblicato tutto.
            File.WriteAllText(Path.Combine(sub, "bozza.md"), "# bozza\n");

            var result = await Push(ctx, path);

            Assert.IsTrue(result.LeftBehind.Any(x => x.Contains("figlio")),
                "chi pubblica deve sapere cosa resta indietro.");
        }

        // ---- infrastruttura ----

        private static async Task<SafePushResult> Push(AgentCityContext ctx, string projectPath)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ISafePushService>();
            return await svc.PushEverythingAsync(projectPath, null);
        }

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "-c protocol.file.allow=always " + args,
                    WorkingDirectory = cwd,
                    UseShellExecute = false, RedirectStandardOutput = true,
                    RedirectStandardError = true, CreateNoWindow = true,
                }
            };
            p.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            p.Start();
            var o = p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(60000);
            return (p.ExitCode, o);
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        private static (string Path, string Sub, string ParentOrigin) SetupWithSubmodule(
            AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            var origins = Path.Combine(ctx.Factory.DataDir, "origins");

            var childOrigin = Path.Combine(origins, name + "-figlio.git");
            Directory.CreateDirectory(childOrigin);
            Git(childOrigin, "init --bare");
            Git(childOrigin, "symbolic-ref HEAD refs/heads/main");

            var childWork = Path.Combine(ctx.Factory.DataDir, "work", name + "-figlio");
            Directory.CreateDirectory(childWork);
            InitRepo(childWork);
            File.WriteAllText(Path.Combine(childWork, "codice.md"), "# codice\n");
            Git(childWork, "add -A");
            Git(childWork, "commit -m base-figlio");
            Git(childWork, $"remote add origin \"{childOrigin}\"");
            Git(childWork, "push -u origin main");

            var parentOrigin = Path.Combine(origins, name + ".git");
            Directory.CreateDirectory(parentOrigin);
            Git(parentOrigin, "init --bare");
            Git(parentOrigin, "symbolic-ref HEAD refs/heads/main");

            InitRepo(path);
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\n");
            Git(path, "add -A");
            Git(path, "commit -m base");
            Git(path, $"submodule add \"{childOrigin}\" figlio");
            Git(path, "commit -m \"aggiungi submodule\"");
            Git(path, $"remote add origin \"{parentOrigin}\"");
            Git(path, "push -u origin main");

            var sub = Path.Combine(path, "figlio");
            Git(sub, "checkout main");
            return (path, sub, parentOrigin);
        }

        private static void InitRepo(string dir)
        {
            Git(dir, "init -b main");
            Git(dir, "config user.email carlo@test.local");
            Git(dir, "config user.name Test");
            Git(dir, "config commit.gpgsign false");
        }
    }
}
