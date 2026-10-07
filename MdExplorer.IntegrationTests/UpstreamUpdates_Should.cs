using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Git;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// «Scarica gli aggiornamenti» dalla <b>sorgente</b> di un progetto: un secondo remoto, <c>upstream</c>,
    /// da cui si prende soltanto. È il caso del progetto demo, il cui <c>origin</c> è un repository locale:
    /// senza questo gesto le versioni nuove del demo non arrivano e niente le segnala.
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class UpstreamUpdates_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

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
            p.Start();
            var o = p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(20000);
            return (p.ExitCode, o.Trim());
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        private static async Task<T> Sync<T>(AgentCityContext ctx, Func<IRepoSyncService, Task<T>> action)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await action(scope.ServiceProvider.GetRequiredService<IRepoSyncService>());
        }

        /// <summary>
        /// La forma di un progetto demo: la sorgente pubblica (<c>upstream</c>), un origin locale che ne è
        /// la copia, e la cartella del progetto. Ritorna anche il clone di chi pubblica nella sorgente.
        /// </summary>
        private static (string Project, string Origin, string Publisher) Setup(AgentCityContext ctx, string name)
        {
            var root = Path.Combine(ctx.Factory.DataDir, "sorgente", name);
            var source = Path.Combine(root, "sorgente.git");
            var origin = Path.Combine(root, "origin.git");
            var publisher = Path.Combine(root, "chi-pubblica");
            Directory.CreateDirectory(source);
            Git(source, "init -q --bare -b main");

            Git(root, $"clone -q \"{source}\" chi-pubblica");
            Identity(publisher);
            File.WriteAllText(Path.Combine(publisher, "README.md"), "# demo\nversione 1\n");
            File.WriteAllText(Path.Combine(publisher, "responsabilita.md"), "| Ambito | Git Email | Agenti |\n|--|--|--|\n");
            Git(publisher, "add -A"); Git(publisher, "commit -q -m v1"); Git(publisher, "push -q origin HEAD:main");

            var (_, project) = ctx.SeedProject(name);
            Git(project, "init -q -b main");
            Identity(project);
            Git(project, $"remote add upstream \"{source}\"");
            Git(project, "pull -q upstream main");
            Git(root, $"clone -q --bare \"{project}\" origin.git");
            Git(project, $"remote add origin \"{origin}\"");
            Git(project, "fetch -q origin");
            Git(project, "branch -q --set-upstream-to=origin/main main");
            return (project, origin, publisher);
        }

        private static void Identity(string repo)
        {
            Git(repo, "config user.email prova@test.local"); Git(repo, "config user.name Prova"); Git(repo, "config commit.gpgsign false");
        }

        private static void Publish(string publisher, string file, string content, string message)
        {
            File.WriteAllText(Path.Combine(publisher, file), content);
            Git(publisher, "add -A"); Git(publisher, $"commit -q -m \"{message}\""); Git(publisher, "push -q origin HEAD:main");
        }

        [TestMethod]
        public async Task Say_nothing_for_a_project_without_a_source()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("senza-sorgente");
            Git(path, "init -q -b main"); Identity(path);
            File.WriteAllText(Path.Combine(path, "README.md"), "# x\n"); Git(path, "add -A"); Git(path, "commit -q -m base");

            var status = await Sync(ctx, s => s.UpstreamStatusAsync(path, fetch: true));
            Assert.IsFalse(status.HasUpstream);

            var pulled = await Sync(ctx, s => s.PullUpstreamAsync(path));
            Assert.IsNotNull(pulled.Refused, "non è un errore: è un gesto che qui non esiste");
        }

        [TestMethod]
        public async Task Count_the_updates_only_after_asking_the_source()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, _, publisher) = Setup(ctx, "conta");
            Publish(publisher, "README.md", "# demo\nversione 2\n", "v2");
            Publish(publisher, "nuovo.md", "# nuovo\n", "v3");

            var known = await Sync(ctx, s => s.UpstreamStatusAsync(project, fetch: false));
            Assert.IsTrue(known.HasUpstream);
            Assert.AreEqual(0, known.Behind, "senza chiedere si sa solo quello che si sapeva");

            var asked = await Sync(ctx, s => s.UpstreamStatusAsync(project, fetch: true));
            Assert.AreEqual(2, asked.Behind);
            Assert.AreEqual("main", asked.Branch);
            Assert.IsNull(asked.Problem);
        }

        [TestMethod]
        public async Task Bring_the_updates_into_the_folder_and_into_origin()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, origin, publisher) = Setup(ctx, "scarica");
            Publish(publisher, "README.md", "# demo\nversione 2\n", "v2");
            // Lavoro mio, committato, su un altro file: deve restare.
            File.WriteAllText(Path.Combine(project, "mio.md"), "# mio\n");
            Git(project, "add -A"); Git(project, "commit -q -m mio");

            var pulled = await Sync(ctx, s => s.PullUpstreamAsync(project));

            Assert.IsTrue(pulled.Success, pulled.Message ?? pulled.Refused);
            StringAssert.Contains(pulled.Message, "1 aggiornamento");
            StringAssert.Contains(File.ReadAllText(Path.Combine(project, "README.md")), "versione 2");
            Assert.IsTrue(File.Exists(Path.Combine(project, "mio.md")));
            CollectionAssert.Contains(new System.Collections.Generic.List<string>(pulled.ChangedFiles), "README.md");
            Assert.AreEqual(0, pulled.Warnings.Count, string.Join(" | ", pulled.Warnings));

            // origin alla pari: gli agenti partono da lì.
            Assert.AreEqual(Git(project, "rev-parse HEAD").Out, Git(origin, "rev-parse main").Out);
            Assert.AreEqual(0, (await Sync(ctx, s => s.UpstreamStatusAsync(project, fetch: true))).Behind);
            // La sorgente non riceve niente: né il mio lavoro né l'unione.
            Git(publisher, "fetch -q origin");
            Assert.AreEqual("0", Git(publisher, "rev-list --count HEAD..origin/main").Out, "la sorgente ha solo ciò che ha pubblicato chi la cura");
        }

        [TestMethod]
        public async Task Leave_the_project_as_it_was_when_the_updates_touch_my_changes()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, origin, publisher) = Setup(ctx, "conflitto");
            Publish(publisher, "responsabilita.md", "| Ambito | Git Email | Agenti |\n|--|--|--|\n| Nuova | x@y.z | nuovo |\n", "tabella nuova");
            // Come dopo «Sono tutti miei»: la mia riga, committata, sulla stessa tabella.
            File.WriteAllText(Path.Combine(project, "responsabilita.md"), "| Ambito | Git Email | Agenti |\n|--|--|--|\n| Mia | io@test.local | mio |\n");
            Git(project, "add -A"); Git(project, "commit -q -m \"sono miei\"");
            var headBefore = Git(project, "rev-parse HEAD").Out;
            var originBefore = Git(origin, "rev-parse main").Out;

            var pulled = await Sync(ctx, s => s.PullUpstreamAsync(project));

            Assert.IsFalse(pulled.Success);
            StringAssert.Contains(pulled.Message, "responsabilita.md");
            StringAssert.Contains(pulled.Message, "com'era");
            Assert.AreEqual(headBefore, Git(project, "rev-parse HEAD").Out);
            Assert.AreNotEqual(0, Git(project, "rev-parse --quiet --verify MERGE_HEAD").Code, "nessuna unione lasciata a metà");
            Assert.AreEqual(string.Empty, Git(project, "status --porcelain").Out);
            StringAssert.Contains(File.ReadAllText(Path.Combine(project, "responsabilita.md")), "| Mia |");
            Assert.AreEqual(originBefore, Git(origin, "rev-parse main").Out);
        }

        /// <summary>Il caso visto su Windows il 07/10: la città accesa e committata qui, la sorgente che aggiunge il workflow nello stesso blocco.</summary>
        [TestMethod]
        public async Task Merge_the_development_yml_entry_by_entry_when_it_is_the_only_file_in_the_way()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, origin, publisher) = Setup(ctx, "yml");
            const string start = "harness:\n  target: copilot\nagentCity:\n  ownershipDoc: gara/responsabilita.md\n";
            Publish(publisher, ".development.yml", start, "config");
            Git(project, "pull -q upstream main"); Git(project, "push -q origin main");
            Publish(publisher, ".development.yml", start + "  workflowDoc: gara/workflow.md\n", "workflow");
            File.WriteAllText(Path.Combine(project, ".development.yml"), "harness:\n  target: copilot\nagentCity:\n  enabled: true\n  ownershipDoc: gara/responsabilita.md\n  roomSecret: XYZ\n");
            Git(project, "add -A"); Git(project, "commit -q -m \"città accesa\"");

            var pulled = await Sync(ctx, s => s.PullUpstreamAsync(project));

            Assert.IsTrue(pulled.Success, pulled.Message);
            StringAssert.Contains(string.Join(" ", pulled.Warnings), "voce per voce");
            var yml = File.ReadAllText(Path.Combine(project, ".development.yml"));
            StringAssert.Contains(yml, "enabled: true");
            StringAssert.Contains(yml, "roomSecret: XYZ");
            StringAssert.Contains(yml, "workflowDoc: gara/workflow.md");
            Assert.AreEqual(string.Empty, Git(project, "status --porcelain").Out);
            Assert.AreEqual(Git(project, "rev-parse HEAD").Out, Git(origin, "rev-parse main").Out, "e origin alla pari, come sempre");
        }

        [TestMethod]
        public async Task Stop_when_the_same_entry_of_development_yml_changed_differently()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, _, publisher) = Setup(ctx, "yml-conflitto");
            Publish(publisher, ".development.yml", "harness:\n  target: copilot\n", "config");
            Git(project, "pull -q upstream main");
            Publish(publisher, ".development.yml", "harness:\n  target: opencode\n", "opencode");
            File.WriteAllText(Path.Combine(project, ".development.yml"), "harness:\n  target: claude\n");
            Git(project, "add -A"); Git(project, "commit -q -m claude");
            var headBefore = Git(project, "rev-parse HEAD").Out;

            var pulled = await Sync(ctx, s => s.PullUpstreamAsync(project));

            Assert.IsFalse(pulled.Success);
            StringAssert.Contains(pulled.Message, "«harness.target»");
            Assert.AreEqual(headBefore, Git(project, "rev-parse HEAD").Out);
            Assert.AreNotEqual(0, Git(project, "rev-parse --quiet --verify MERGE_HEAD").Code, "nessuna unione lasciata a metà");
            StringAssert.Contains(File.ReadAllText(Path.Combine(project, ".development.yml")), "target: claude");
        }

        [TestMethod]
        public async Task Not_lose_changes_I_have_not_committed()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, _, publisher) = Setup(ctx, "non-committato");
            Publish(publisher, "README.md", "# demo\nversione 2\n", "v2");
            File.WriteAllText(Path.Combine(project, "README.md"), "# demo\nla mia riga, non committata\n");

            var pulled = await Sync(ctx, s => s.PullUpstreamAsync(project));

            Assert.IsFalse(pulled.Success, "git rifiuta di sovrascrivere un file modificato");
            StringAssert.Contains(pulled.Message, "com'era");
            StringAssert.Contains(File.ReadAllText(Path.Combine(project, "README.md")), "la mia riga, non committata");
        }
    }
}
