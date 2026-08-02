using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// I submodule di un worktree devono seguire il padre.
    /// <para>
    /// Un worktree fresco non li popola, e il <c>reset --hard</c> del padre non entra dentro di
    /// loro: senza un allineamento esplicito, un agente si troverebbe il codice al commit di un
    /// run precedente — o modificato da un altro agente — mentre la documentazione è a quello
    /// nuovo. Lavorerebbe su una realtà che non esiste.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class WorktreeSubmodule_Should
    {
        [TestInitialize]
        public void Setup()
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            // I submodule via file:// sono bloccati da git 2.38 in poi, e la configurazione DEL
            // REPO viene ignorata di proposito: un repo malevolo non deve poter riabilitare da
            // sé un trasporto vietato. La via legittima è l'ambiente del processo, che git
            // tratta come config da riga di comando — e i git figli del codice di prodotto la
            // ereditano senza che il prodotto debba saperne nulla.
            // In produzione gli URL dei submodule sono https/ssh: questa restrizione non li tocca.
            Environment.SetEnvironmentVariable("GIT_CONFIG_COUNT", "1");
            Environment.SetEnvironmentVariable("GIT_CONFIG_KEY_0", "protocol.file.allow");
            Environment.SetEnvironmentVariable("GIT_CONFIG_VALUE_0", "always");
        }

        [TestMethod]
        public async Task Populate_the_submodule_so_the_agent_can_read_the_code()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupProjectWithSubmodule(ctx, "wt-sub");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            var prep = await m.PrepareForRunAsync(path, "worker", "att1");
            Assert.IsTrue(prep.Success, prep.Error);

            // Senza questo, 'code/' sarebbe una cartella vuota e l'agente documenterebbe il nulla.
            var fileInSubmodule = Path.Combine(prep.WorktreePath, "code", "README.md");
            Assert.IsTrue(File.Exists(fileInSubmodule),
                "il submodule dev'essere popolato nel worktree, non una cartella vuota");
        }

        [TestMethod]
        public async Task Reset_changes_left_inside_the_submodule_by_a_previous_run()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupProjectWithSubmodule(ctx, "wt-sub-reset");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            var first = await m.PrepareForRunAsync(path, "worker", "att1");
            Assert.IsTrue(first.Success, first.Error);

            // Un run precedente lascia il codice sporco: modifica tracciata + file non tracciato.
            var tracked = Path.Combine(first.WorktreePath, "code", "README.md");
            File.WriteAllText(tracked, "# sporcato da un run precedente\n");
            File.WriteAllText(Path.Combine(first.WorktreePath, "code", "scratch.txt"), "avanzo");

            var second = await m.PrepareForRunAsync(path, "worker", "att2");
            Assert.IsTrue(second.Success, second.Error);

            // '--force' riporta il tracciato al commit pinnato dal padre...
            StringAssert.Contains(File.ReadAllText(tracked), "codice di prova",
                "il file tracciato del submodule torna al commit pinnato dal padre");

            // ...e il clean ricorsivo toglie il non tracciato, che il 'clean -fd' del padre
            // non avrebbe toccato.
            Assert.IsFalse(File.Exists(Path.Combine(second.WorktreePath, "code", "scratch.txt")),
                "gli avanzi non tracciati dentro il submodule non sopravvivono al prepare");
        }

        // ---- infrastruttura git ----

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git", Arguments = args, WorkingDirectory = cwd,
                    UseShellExecute = false, RedirectStandardOutput = true,
                    RedirectStandardError = true, CreateNoWindow = true,
                }
            };
            p.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            // I submodule via file:// sono rifiutati dalle versioni recenti di git senza questo.
            p.StartInfo.EnvironmentVariables["GIT_ALLOW_PROTOCOL"] = "file";
            p.StartInfo.Arguments = "-c protocol.file.allow=always " + args;
            p.Start();
            var o = p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(60000);
            return (p.ExitCode, o);
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        /// <summary>Progetto di documentazione con un repo di codice come submodule in <c>code/</c>.</summary>
        private static (Guid Key, string Path) SetupProjectWithSubmodule(AgentCityContext ctx, string name)
        {
            var (key, path) = ctx.SeedProject(name);

            // Repo del CODICE, con il suo origin bare.
            var codeOrigin = Path.Combine(ctx.Factory.DataDir, "origins", name + "-code.git");
            Directory.CreateDirectory(codeOrigin); Git(codeOrigin, "init --bare");
            var codeWork = Path.Combine(ctx.Factory.DataDir, "work", name + "-code");
            Directory.CreateDirectory(codeWork);
            Git(codeWork, "init -b main");
            Git(codeWork, "config user.email code@test.local"); Git(codeWork, "config user.name Code");
            Git(codeWork, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(codeWork, "README.md"), "# codice di prova\n");
            Git(codeWork, "add -A"); Git(codeWork, "commit -m base");
            Git(codeWork, $"remote add origin \"{codeOrigin}\""); Git(codeWork, "push -u origin main");
            // Stesso gotcha del bare della documentazione, e morde anche qui: il bare nasce con
            // HEAD su 'master' mentre pubblichiamo 'main', quindi 'submodule add' clona e prova
            // a mettere in checkout un branch che non esiste ("branch yet to be born"),
            // lasciando 'code/' senza commit e l'indice del padre rotto.
            Git(codeOrigin, "symbolic-ref HEAD refs/heads/main");

            // Progetto di DOCUMENTAZIONE che lo tiene come submodule.
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin); Git(origin, "init --bare");
            // Il bare nasce con HEAD su 'master' mentre noi pubblichiamo 'main': senza questo,
            // origin/HEAD non si risolve e il prepare non trova il branch di base.
            Git(origin, "symbolic-ref HEAD refs/heads/main");

            Git(path, "init -b main");
            Git(path, "config user.email carlo@test.local"); Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            // Da git 2.38 i submodule via file:// sono bloccati per difesa. In produzione gli
            // URL sono https/ssh e la restrizione non tocca nulla; qui l'origin e' una cartella,
            // quindi va abilitato NEL REPO (i worktree ne condividono la configurazione, cosi'
            // la abilitazione vale anche per il codice di prodotto che gira senza flag).
            Git(path, "config protocol.file.allow always");
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\n");

            // PRIMA il commit iniziale, POI il submodule: 'submodule add' su un repo senza
            // commit fallisce il checkout ("branch yet to be born") e lascia l'indice rotto.
            Git(path, "add -A"); Git(path, "commit -m base");

            Git(path, $"submodule add \"{codeOrigin}\" code");
            Git(path, "add -A"); Git(path, "commit -m submodule");

            Git(path, $"remote add origin \"{origin}\""); Git(path, "push -u origin main");

            return (key, path);
        }
    }
}
