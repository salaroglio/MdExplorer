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
    /// I submodule si popolano all'<b>apertura</b> del progetto.
    /// <para>
    /// Prima lo faceva solo chi clonava o faceva pull da dentro MdExplorer. Ma un progetto lo si
    /// apre anche dopo averlo clonato da fuori, o senza <c>--recurse-submodules</c>: in quel caso
    /// nessuno li popolava mai e l'unico modo di accorgersene era aprire la cartella e trovarla
    /// vuota. Da qui il «i submodule li trovo sempre vuoti».
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class ProjectSubmoduleInit_Should
    {
        [TestInitialize]
        public void Setup()
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            // I submodule via file:// sono bloccati da git 2.38 in poi, e la configurazione DEL
            // REPO viene ignorata di proposito. La via legittima è l'ambiente del processo, che
            // i git figli ereditano senza che il codice di prodotto debba saperne nulla.
            Environment.SetEnvironmentVariable("GIT_CONFIG_COUNT", "1");
            Environment.SetEnvironmentVariable("GIT_CONFIG_KEY_0", "protocol.file.allow");
            Environment.SetEnvironmentVariable("GIT_CONFIG_VALUE_0", "always");
        }

        [TestMethod]
        public async Task Fill_a_submodule_that_someone_else_cloned_empty()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = CloneWithoutSubmodules(ctx, "sub-vuoto");

            // Il caso vero: cartella del codice presente ma vuota, come la trova chi ha clonato
            // da fuori MdExplorer.
            var code = Path.Combine(path, "code");
            Assert.IsFalse(File.Exists(Path.Combine(code, "README.md")), "premessa: il submodule è vuoto");

            var res = await Init(ctx).EnsureAsync(path);

            Assert.IsTrue(res.Success, res.Error);
            CollectionAssert.Contains(res.Missing.ToList(), "code");
            Assert.IsTrue(File.Exists(Path.Combine(code, "README.md")),
                "dopo l'apertura del progetto il codice dev'esserci");
        }

        [TestMethod]
        public async Task Do_nothing_at_all_when_there_are_no_submodules()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("sub-nessuno");
            Git(path, "init -b main");

            // La stragrande maggioranza dei progetti di documentazione non ha submodule:
            // l'apertura non deve pagare nemmeno il lancio di un processo.
            var res = await Init(ctx).EnsureAsync(path);

            Assert.IsTrue(res.NothingToDo);
            Assert.IsTrue(res.Success);
        }

        [TestMethod]
        public async Task Do_nothing_when_they_are_already_there()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = CloneWithoutSubmodules(ctx, "sub-gia-fatto");

            var first = await Init(ctx).EnsureAsync(path);
            Assert.IsTrue(first.Success, first.Error);
            Assert.AreEqual(1, first.Missing.Count);

            // Riaprire il progetto è un gesto quotidiano: la seconda volta non c'è niente da fare
            // e non si deve rifare il lavoro.
            var second = await Init(ctx).EnsureAsync(path);
            Assert.IsTrue(second.NothingToDo, "il secondo giro non deve ripopolare nulla");
        }

        [TestMethod]
        public async Task Say_out_loud_when_it_cannot_reach_the_code()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = CloneWithoutSubmodules(ctx, "sub-irraggiungibile");

            // L'origine del submodule sparisce: è il caso del repository privato o spostato.
            var codeOrigin = Path.Combine(ctx.Factory.DataDir, "origins", "sub-irraggiungibile-code.git");
            Directory.Delete(codeOrigin, recursive: true);

            var res = await Init(ctx).EnsureAsync(path);

            // Il punto di tutto il lavoro: prima questo fallimento finiva appeso al messaggio di
            // successo del clone e nessuno lo leggeva. Ora è un fallimento, e dice di cosa parla.
            Assert.IsFalse(res.Success, "un submodule che non si popola è un fallimento, non un dettaglio");
            StringAssert.Contains(res.Error, "code");
        }

        // ---- infrastruttura ----

        private static IProjectSubmoduleInitializer Init(AgentCityContext ctx)
            => ctx.Factory.Services.GetRequiredService<IProjectSubmoduleInitializer>();

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git", Arguments = "-c protocol.file.allow=always " + args,
                    WorkingDirectory = cwd, UseShellExecute = false,
                    RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
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

        /// <summary>
        /// Riproduce la situazione che genera il problema: un progetto clonato <b>senza</b>
        /// <c>--recurse-submodules</c>, cioè con la cartella del codice presente ma vuota.
        /// </summary>
        private static string CloneWithoutSubmodules(AgentCityContext ctx, string name)
        {
            // Repository del CODICE, con il suo origin.
            var codeOrigin = Path.Combine(ctx.Factory.DataDir, "origins", name + "-code.git");
            Directory.CreateDirectory(codeOrigin);
            Git(codeOrigin, "init --bare");
            var codeWork = Path.Combine(ctx.Factory.DataDir, "work", name + "-code");
            Directory.CreateDirectory(codeWork);
            Git(codeWork, "init -b main");
            Git(codeWork, "config user.email code@test.local");
            Git(codeWork, "config user.name Code");
            Git(codeWork, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(codeWork, "README.md"), "# codice di prova\n");
            Git(codeWork, "add -A");
            Git(codeWork, "commit -m base");
            Git(codeWork, $"remote add origin \"{codeOrigin}\"");
            Git(codeWork, "push -u origin main");
            // Il bare nasce con HEAD su 'master' mentre pubblichiamo 'main': senza questo,
            // 'submodule add' clona e prova a mettere in checkout un branch che non esiste.
            Git(codeOrigin, "symbolic-ref HEAD refs/heads/main");

            // Repository della DOCUMENTAZIONE che lo tiene come submodule.
            var docOrigin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(docOrigin);
            Git(docOrigin, "init --bare");
            Git(docOrigin, "symbolic-ref HEAD refs/heads/main");

            var docWork = Path.Combine(ctx.Factory.DataDir, "work", name + "-doc");
            Directory.CreateDirectory(docWork);
            Git(docWork, "init -b main");
            Git(docWork, "config user.email carlo@test.local");
            Git(docWork, "config user.name Test");
            Git(docWork, "config commit.gpgsign false");
            Git(docWork, "config protocol.file.allow always");
            File.WriteAllText(Path.Combine(docWork, "README.md"), "# doc\n");
            // Prima il commit iniziale, poi il submodule: 'submodule add' su un repo senza commit
            // fallisce il checkout e lascia l'indice rotto.
            Git(docWork, "add -A");
            Git(docWork, "commit -m base");
            Git(docWork, $"submodule add \"{codeOrigin}\" code");
            Git(docWork, "add -A");
            Git(docWork, "commit -m submodule");
            Git(docWork, $"remote add origin \"{docOrigin}\"");
            Git(docWork, "push -u origin main");

            // E qui il gesto che crea il problema: clone SENZA --recurse-submodules.
            var (_, path) = ctx.SeedProject(name);
            Directory.Delete(path, recursive: true);
            Git(Path.GetDirectoryName(path), $"clone \"{docOrigin}\" \"{path}\"");

            return path;
        }
    }
}
