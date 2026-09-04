using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Git.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La vista divisa <b>per repository</b>: il progetto e i suoi submodule, ognuno col suo ramo.
    /// <para>
    /// Prima la risposta aveva un ramo solo e una lista di file sola — una forma che rappresenta
    /// un repository solo — e i submodule finivano fra i file, unico posto rimasto. Due situazioni
    /// opposte comparivano con la stessa etichetta «modificato»: un puntatore spostato, che si
    /// committa <b>nel padre</b>, e un contenuto sporco, che si committa <b>nel submodule</b>.
    /// Questi test tengono separate proprio quelle due.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class WorkingChangesRepos_Should
    {
        [TestMethod]
        public async Task Tell_a_dirty_submodule_from_a_moved_pointer()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub) = SetupProjectWithSubmodule(ctx, "repos-due-casi");

            // Caso A — contenuto sporco dentro il submodule, puntatore FERMO.
            File.WriteAllText(Path.Combine(sub, "appunti.md"), "# roba non salvata\n");

            var view = await Service(ctx).GetAsync(path, null);
            Assert.IsNull(view.Problem, view.Problem);

            var root = view.Repos.Single(r => r.Path == string.Empty);
            var child = view.Repos.Single(r => r.Path == "figlio");

            // Il difetto da cui si parte: la cartella del submodule compariva fra i file del padre.
            CollectionAssert.DoesNotContain(root.Files.Select(f => f.Path).ToList(), "figlio");
            Assert.IsFalse(root.PointerMoved, "il puntatore non si e' mosso: nessuno l'ha toccato.");
            Assert.IsFalse(child.PointerMoved, "contenuto sporco non e' un puntatore spostato.");

            // Il file sta nel repository dove va committato, non in quello del padre.
            Assert.AreEqual("untracked", child.Files.Single(f => f.Path == "appunti.md").Change);

            // Caso B — ora si committa DENTRO il submodule: il puntatore si sposta e diventa
            // lavoro DEL PADRE, che il padre deve committare.
            Git(sub, "add -A");
            Git(sub, "commit -m \"lavoro del figlio\"");

            var after = await Service(ctx).GetAsync(path, null);
            var childAfter = after.Repos.Single(r => r.Path == "figlio");

            Assert.IsTrue(childAfter.PointerMoved,
                "committare nel figlio sposta il puntatore: e' il lavoro nuovo che compare nel padre.");

            // Il file resta elencato, e non e' un difetto: il confronto e' contro origin/<base>,
            // quindi "committato ma non pushato" e' ancora una differenza rispetto a cio' che gli
            // altri vedono. Sparira' dal push, non dal commit.
            Assert.AreEqual(1, childAfter.Ahead, "un commit fatto e non pubblicato.");
            Assert.AreEqual("added", childAfter.Files.Single(f => f.Path == "appunti.md").Change);
        }

        [TestMethod]
        public async Task Say_that_a_submodule_was_never_downloaded()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub) = SetupProjectWithSubmodule(ctx, "repos-non-popolato");

            // Lo svuoto come sarebbe appena dopo un clone senza --recursive.
            Git(path, "submodule deinit -f figlio");

            var view = await Service(ctx).GetAsync(path, null);
            var child = view.Repos.Single(r => r.Path == "figlio");

            // git status non lo nomina nemmeno: enumerare da li' l'avrebbe fatto sparire.
            Assert.IsTrue(child.NotInitialized, "un submodule mai scaricato va detto, non nascosto.");
            Assert.IsNotNull(child.CommitBlocker, "e va detto anche PERCHE' non ci si puo' committare.");
        }

        [TestMethod]
        public async Task Refuse_to_commit_where_the_head_is_detached()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub) = SetupProjectWithSubmodule(ctx, "repos-staccato");

            Git(sub, "checkout --detach");

            var view = await Service(ctx).GetAsync(path, null);
            var child = view.Repos.Single(r => r.Path == "figlio");

            Assert.IsTrue(child.Detached);
            Assert.IsNull(child.Branch, "senza ramo non si scrive un nome di ramo che non c'e'.");
            StringAssert.Contains(child.CommitBlocker, "orfano");
        }

        [TestMethod]
        public async Task Warn_before_the_parent_publishes_a_commit_nobody_has()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub) = SetupProjectWithSubmodule(ctx, "repos-avviso-push");

            // Commit nel figlio, NON pushato: da qui in poi il padre punta a un commit che sul
            // remoto del figlio non esiste. Pushare il padre romperebbe il clone di chiunque.
            File.WriteAllText(Path.Combine(sub, "nuovo.md"), "# nuovo\n");
            Git(sub, "add -A");
            Git(sub, "commit -m \"non pushato\"");

            var view = await Service(ctx).GetAsync(path, null);
            var root = view.Repos.Single(r => r.Path == string.Empty);

            Assert.IsTrue(root.PushWarnings.Any(w => w.Contains("figlio")),
                "l'avviso deve arrivare PRIMA del push, non come errore di chi clona giorni dopo.");
        }

        [TestMethod]
        public async Task Read_a_file_that_lives_inside_a_submodule()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub) = SetupProjectWithSubmodule(ctx, "repos-diff-dentro");

            File.WriteAllText(Path.Combine(sub, "codice.md"), "# codice\n\nriga nuova\n");

            // Senza dire in quale repository sta, la domanda finisce alla radice — che di quel
            // file non sa niente — e la risposta sarebbe "nessuna differenza".
            var alRepoGiusto = await Service(ctx).DiffAsync(path, null, "codice.md", "figlio");
            StringAssert.Contains(alRepoGiusto, "riga nuova");

            // E il percorso resta chiuso dentro il repository indicato.
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => Service(ctx).DiffAsync(path, null, "../../etc/passwd", "figlio"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => Service(ctx).DiffAsync(path, null, "codice.md", "../fuori"));
        }

        [TestMethod]
        public async Task Discard_inside_the_submodule_that_owns_the_file()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub) = SetupProjectWithSubmodule(ctx, "repos-scarta-dentro");

            var file = Path.Combine(sub, "codice.md");
            File.WriteAllText(file, "# rovinato\n");

            await Service(ctx).DiscardAsync(path, null, "codice.md", "figlio");

            StringAssert.Contains(File.ReadAllText(file), "# codice");
        }

        [TestMethod]
        public async Task Commit_inside_a_submodule_and_watch_the_parent_light_up()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub) = SetupProjectWithSubmodule(ctx, "repos-commit-figlio");

            File.WriteAllText(Path.Combine(sub, "prodotto.md"), "# prodotto\n");

            // L'assunto su cui poggia il pulsante per riga: CommitAsync non sa cosa sia un
            // submodule, prende un percorso e committa li'. Nessun endpoint nuovo.
            using var scope = ctx.Factory.Services.CreateScope();
            var git = scope.ServiceProvider.GetRequiredService<IModernGitService>();
            var author = new GitAuthor { Name = "Test", Email = "carlo@test.local" };
            var result = await git.CommitAsync(sub, "lavoro del figlio", author);

            Assert.IsTrue(result.Success, result.ErrorMessage);

            var view = await Service(ctx).GetAsync(path, null);
            var root = view.Repos.Single(r => r.Path == string.Empty);
            var child = view.Repos.Single(r => r.Path == "figlio");

            // Committare nel figlio CREA lavoro nel padre: il puntatore ora indica un commit
            // diverso. Non e' un effetto collaterale da nascondere, e' il lavoro nuovo.
            Assert.IsTrue(child.PointerMoved,
                "dopo il commit nel submodule il progetto deve accendersi da solo.");
            Assert.AreEqual(1, child.Ahead, "il commit c'e' ma non e' ancora pubblicato.");

            // E il padre deve dire PRIMA che pushare adesso romperebbe il repo per gli altri.
            Assert.IsTrue(root.PushWarnings.Any(w => w.Contains("figlio")));
        }

        // ---- infrastruttura ----

        private static IWorkingChangesService Service(AgentCityContext ctx)
            => ctx.Factory.Services.GetRequiredService<IWorkingChangesService>();

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    // I submodule da percorso locale sono bloccati per difetto dalle versioni
                    // recenti di git: qui la sorgente e' una cartella del test, non un remoto.
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

        /// <summary>Un progetto con dentro un submodule, entrambi con il loro remoto e allineati.</summary>
        private static (string Path, string SubPath) SetupProjectWithSubmodule(AgentCityContext ctx, string name)
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
            Git(sub, "checkout main");   // 'submodule add' lascia il figlio su un ramo, ma non ovunque
            return (path, sub);
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
