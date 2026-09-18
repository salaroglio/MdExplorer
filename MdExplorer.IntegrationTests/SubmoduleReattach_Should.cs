using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Git;
using MdExplorer.Services.Git.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Dopo un pull, un submodule deve restare <b>usabile</b>.
    /// <para>
    /// <c>git submodule update</c> mette il submodule sul commit registrato — che è un commit, non
    /// un ramo — e quindi lo lascia in <b>HEAD staccato</b>. Da lì un commit resterebbe orfano, e
    /// prima della vista per repository la cosa era invisibile: si scopriva dopo. Il riaggancio
    /// avviene solo quando un ramo punta esattamente a quel commit, cioè quando non sposta un file.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class SubmoduleReattach_Should
    {
        /// <summary>
        /// Il prodotto invoca git <b>senza</b> <c>protocol.file.allow</c>, ed è giusto: git blocca
        /// i submodule da percorso locale per una vulnerabilità nota. Qui i remoti sono cartelle
        /// del test, e il permesso non si può dare nel repository — serve dove avviene il fetch,
        /// dentro il submodule — quindi si passa per l'ambiente del processo, che i git figli
        /// ereditano. Rimesso a posto alla fine, per non condizionare gli altri test.
        /// </summary>
        [TestInitialize]
        public void AllowLocalSubmodules()
        {
            Environment.SetEnvironmentVariable("GIT_CONFIG_COUNT", "1");
            Environment.SetEnvironmentVariable("GIT_CONFIG_KEY_0", "protocol.file.allow");
            Environment.SetEnvironmentVariable("GIT_CONFIG_VALUE_0", "always");
        }

        [TestCleanup]
        public void RestoreProtocolRules()
        {
            Environment.SetEnvironmentVariable("GIT_CONFIG_COUNT", null);
            Environment.SetEnvironmentVariable("GIT_CONFIG_KEY_0", null);
            Environment.SetEnvironmentVariable("GIT_CONFIG_VALUE_0", null);
        }

        [TestMethod]
        public async Task Put_the_submodule_back_on_its_branch_after_a_pull()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, childOrigin, parentOrigin) = Setup(ctx, "riaggancio");

            Assert.AreEqual("main", Branch(sub), "si parte da un submodule agganciato.");

            // Un collega fa avanzare il figlio e sposta il puntatore nel progetto.
            AdvanceElsewhere(ctx, "riaggancio", childOrigin, parentOrigin);

            using var scope = ctx.Factory.Services.CreateScope();
            var git = scope.ServiceProvider.GetRequiredService<IModernGitService>();
            var result = await git.PullAsync(path);
            // PullAsync mette Success=true comunque: quello che conta e' cosa e' arrivato.
            var esito = $"[{result.Message}] hasChanges={result.HasChanges} err={result.ErrorMessage}";

            // Il contenuto è quello nuovo…
            StringAssert.Contains(File.ReadAllText(Path.Combine(sub, "codice.md")), "v2", esito);
            // …e il submodule è ancora su un ramo, quindi ci si può committare.
            Assert.AreEqual("main", Branch(sub),
                "senza riaggancio resterebbe staccato e il commit sarebbe bloccato.");
        }

        [TestMethod]
        public async Task Leave_it_detached_when_no_branch_points_there()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, parentOrigin) = Setup(ctx, "riaggancio-no");

            // Il progetto fissa un commit che sta IN MEZZO alla storia del figlio: il ramo e' andato
            // avanti oltre. Nessun ramo — ne' locale ne' remoto — ha la punta li'.
            var collega = Path.Combine(ctx.Factory.DataDir, "collega-mezzo");
            Git(ctx.Factory.DataDir, $"clone -q \"{parentOrigin}\" \"{collega}\"");
            Git(collega, "submodule update --init -q");
            Git(collega, "config user.email carlo@test.local");
            Git(collega, "config user.name Test");

            var suo = Path.Combine(collega, "figlio");
            Git(suo, "config user.email carlo@test.local");
            Git(suo, "config user.name Test");
            Git(suo, "checkout -q -B main");

            File.WriteAllText(Path.Combine(suo, "codice.md"), "# codice\nv2\n");
            Git(suo, "add -A"); Git(suo, "commit -m v2");
            // Il progetto registra QUESTO commit...
            Git(collega, "add -A"); Git(collega, "commit -m \"bump al commit di mezzo\"");

            // ...e poi il figlio va avanti ancora, quindi la punta del ramo e' oltre.
            File.WriteAllText(Path.Combine(suo, "codice.md"), "# codice\nv3\n");
            Git(suo, "add -A"); Git(suo, "commit -m v3");
            Git(suo, "push -q origin main");

            Git(collega, "push -q origin main");

            await Pull(ctx, path);

            // Agganciare 'main' qui vorrebbe dire portarsi a v3, cioe' spostare i file: si lascia
            // staccato, e la vista per repository lo dice con il motivo scritto.
            Assert.AreEqual("HEAD", Branch(sub),
                "nessun ramo ha la punta sul commit registrato: agganciarne uno sposterebbe il contenuto.");
            StringAssert.Contains(File.ReadAllText(Path.Combine(sub, "codice.md")), "v2",
                "e il contenuto deve restare quello che il progetto registra.");
        }

        [TestMethod]
        public async Task Create_the_missing_branch_when_only_one_remote_points_there()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, _) = Setup(ctx, "ramo-mancante");

            // Com'e' un submodule appena scaricato: sul commit giusto, senza nessun ramo locale.
            Git(sub, "checkout -q --detach");
            Git(sub, "branch -D main");
            Assert.AreEqual("HEAD", Branch(sub));
            var prima = File.ReadAllText(Path.Combine(sub, "codice.md"));

            await Pull(ctx, path);

            Assert.AreEqual("main", Branch(sub), "un solo ramo remoto sta qui: e' l'unica lettura possibile.");
            Assert.AreEqual("origin/main", Git(sub, "rev-parse --abbrev-ref main@{upstream}").Out.Trim(),
                "e deve seguire quel ramo remoto, non restare scollegato.");
            Assert.AreEqual(prima, File.ReadAllText(Path.Combine(sub, "codice.md")),
                "creare un ramo non deve spostare un solo file.");
        }

        [TestMethod]
        public async Task Never_move_a_branch_that_already_exists_elsewhere()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, _) = Setup(ctx, "ramo-occupato");

            // Un ramo 'main' che sta ALTROVE e non segue niente: e' lavoro di qualcuno.
            Git(sub, "checkout -q --detach");
            Git(sub, "branch -D main");
            File.WriteAllText(Path.Combine(sub, "suo.md"), "# lavoro di qualcuno\n");
            Git(sub, "add -A"); Git(sub, "commit -m \"lavoro divergente\"");
            Git(sub, "branch main");
            var suo = Git(sub, "rev-parse main").Out.Trim();
            Git(sub, "checkout -q --detach origin/main");

            await Pull(ctx, path);

            Assert.AreEqual("HEAD", Branch(sub), "con il nome gia' occupato si resta staccati.");
            Assert.AreEqual(suo, Git(sub, "rev-parse main").Out.Trim(),
                "e soprattutto: quel ramo non si sposta di un commit.");
        }

        [TestMethod]
        public async Task Stay_detached_when_two_remotes_point_at_the_same_commit()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, childOrigin, _) = Setup(ctx, "due-remoti");

            // Un secondo remoto con lo stesso commit: quale dei due nomi sarebbe quello giusto?
            var backup = Path.Combine(ctx.Factory.DataDir, "origins", "due-remoti-backup.git");
            Directory.CreateDirectory(backup);
            Git(backup, "init --bare");
            Git(sub, $"remote add backup \"{backup}\"");
            Git(sub, "push -q backup main");
            Git(sub, "fetch -q backup");

            Git(sub, "checkout -q --detach");
            Git(sub, "branch -D main");

            await Pull(ctx, path);

            Assert.AreEqual("HEAD", Branch(sub),
                "due candidati: scegliere sarebbe indovinare al posto di chi lavora.");
        }

        [TestMethod]
        public async Task Attach_them_also_when_the_project_is_merely_opened()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (path, sub, _, _) = Setup(ctx, "apertura");

            // Com'e' un progetto clonato FUORI da MdExplorer: submodule popolato, HEAD staccato,
            // nessun ramo locale. Qui non c'e' nessun pull di mezzo: si apre e basta.
            Git(sub, "checkout -q --detach");
            Git(sub, "branch -D main");
            Assert.AreEqual("HEAD", Branch(sub));

            var initializer = ctx.Factory.Services.GetRequiredService<IProjectSubmoduleInitializer>();
            await initializer.EnsureAsync(path);

            Assert.AreEqual("main", Branch(sub),
                "l'apertura del progetto passa da un'altra strada: se il riaggancio non c'e' anche li', " +
                "chi clona da fuori se li ritrova staccati per sempre.");
        }

        // ---- infrastruttura ----

        private static async Task Pull(AgentCityContext ctx, string projectPath)
        {
            using var scope = ctx.Factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var git = scope.ServiceProvider.GetRequiredService<IModernGitService>();
            await git.PullAsync(projectPath);
        }


        private static string Branch(string dir) => Git(dir, "rev-parse --abbrev-ref HEAD").Out.Trim();

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

        /// <summary>Fa avanzare il figlio da un altro clone e sposta il puntatore nel progetto.</summary>
        private static void AdvanceElsewhere(
            AgentCityContext ctx, string name, string childOrigin, string parentOrigin, string childBranch = "main")
        {
            var collega = Path.Combine(ctx.Factory.DataDir, "collega-" + name);
            Git(ctx.Factory.DataDir, $"clone -q \"{parentOrigin}\" \"{collega}\"");
            Git(collega, "submodule update --init -q");
            Git(collega, "config user.email carlo@test.local");
            Git(collega, "config user.name Test");

            var suo = Path.Combine(collega, "figlio");
            Git(suo, "config user.email carlo@test.local");
            Git(suo, "config user.name Test");
            Git(suo, $"checkout -q -B {childBranch}");
            File.WriteAllText(Path.Combine(suo, "codice.md"), "# codice\nv2\n");
            Git(suo, "add -A");
            Git(suo, "commit -m v2");
            Git(suo, $"push -q origin {childBranch}");

            Git(collega, "add -A");
            Git(collega, "commit -m \"bump figlio\"");
            Git(collega, "push -q origin main");
        }

        private static (string Path, string Sub, string ChildOrigin, string ParentOrigin) Setup(
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
            File.WriteAllText(Path.Combine(childWork, "codice.md"), "# codice\nv1\n");
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
            return (path, sub, childOrigin, parentOrigin);
        }

        private static void InitRepo(string dir)
        {
            Git(dir, "init -b main");
            // Il codice di produzione invoca git SENZA 'protocol.file.allow', ed e' giusto: git
            // blocca i submodule da percorso locale per una vulnerabilita' nota. Qui i remoti sono
            // cartelle del test, quindi il permesso lo dichiara il repository di prova.
            Git(dir, "config protocol.file.allow always");
            Git(dir, "config user.email carlo@test.local");
            Git(dir, "config user.name Test");
            Git(dir, "config commit.gpgsign false");
        }
    }
}
