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
    /// Il pull da origin quando <c>.development.yml</c> è modificato senza commit (06/11/2026, BCCSI-Docs fermo da 18 giorni):
    /// MdExplorer aveva scritto da sé il segreto della chat nel file, un collega aveva pubblicato una voce Atlassian nello
    /// stesso file, e git rifiutava il pull («Your local changes … would be overwritten by merge»).
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class UncommittedDevelopmentYamlPull_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private const string Start =
            "atlassian:\n  jiraBaseUrl: https://deda-bit.atlassian.net/\n  jiraProjectKeys:\n  - BCE\n  - BCG\n  confluenceSpaceKeys: []\n" +
            "harness:\n  target: copilot\n";

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

        private static async Task<RepoActionResult> Pull(AgentCityContext ctx, string project)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IRepoSyncService>().PullAsync(project, "");
        }

        private static void Identity(string repo)
        {
            Git(repo, "config user.email prova@test.local"); Git(repo, "config user.name Prova"); Git(repo, "config commit.gpgsign false");
        }

        /// <summary>Origin, il clone del collega che pubblica, e il progetto allineato con <paramref name="yml"/> nell'ultimo commit.</summary>
        private static (string Project, string Colleague) Setup(AgentCityContext ctx, string name, string yml = Start)
        {
            var root = Path.Combine(ctx.Factory.DataDir, "yml-locale", name);
            var origin = Path.Combine(root, "origin.git");
            Directory.CreateDirectory(origin);
            Git(origin, "init -q --bare -b main");
            Git(root, $"clone -q \"{origin}\" collega");
            var colleague = Path.Combine(root, "collega");
            Identity(colleague);
            File.WriteAllText(Path.Combine(colleague, "README.md"), "# progetto\n");
            File.WriteAllText(Path.Combine(colleague, ".development.yml"), yml);
            Git(colleague, "add -A"); Git(colleague, "commit -q -m inizio"); Git(colleague, "push -q origin HEAD:main");

            var (_, project) = ctx.SeedProject(name);
            Git(project, "init -q -b main");
            Identity(project);
            Git(project, $"remote add origin \"{origin}\"");
            Git(project, "pull -q origin main");
            Git(project, "branch -q --set-upstream-to=origin/main main");
            return (project, colleague);
        }

        private static void Publish(string colleague, string file, string content, string message)
        {
            File.WriteAllText(Path.Combine(colleague, file), content);
            Git(colleague, "add -A"); Git(colleague, $"commit -q -m \"{message}\""); Git(colleague, "push -q origin HEAD:main");
        }

        private static string Yml(string project) => File.ReadAllText(Path.Combine(project, ".development.yml"));

        [TestMethod]
        public async Task Download_and_keep_my_room_secret_when_a_colleague_changed_the_same_file()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, colleague) = Setup(ctx, "bccsi");
            // il collega: il suo MdExplorer pubblica l'indirizzo di Confluence (come il 03/11)
            Publish(colleague, ".development.yml", Start.Replace("  confluenceSpaceKeys: []\n", "  confluenceBaseUrl: https://deda-bit.atlassian.net/\n  confluenceSpaceKeys: []\n"), "confluence");
            Publish(colleague, "nuovo.md", "# nuovo\n", "documento");
            // qui: MdExplorer ha scritto il segreto della chat e nessuno ha fatto il commit (come il 19/10)
            File.WriteAllText(Path.Combine(project, ".development.yml"), Start.Replace("harness:", "agentCity:\n  enabled: false\n  roomSecret: SEGRETO\nharness:"));

            var pulled = await Pull(ctx, project);

            Assert.IsTrue(pulled.Success, pulled.Message ?? pulled.Refused);
            Assert.IsTrue(File.Exists(Path.Combine(project, "nuovo.md")), "gli aggiornamenti sono arrivati");
            var yml = Yml(project);
            StringAssert.Contains(yml, "confluenceBaseUrl: https://deda-bit.atlassian.net/");
            StringAssert.Contains(yml, "roomSecret: SEGRETO");
            Assert.AreEqual("M .development.yml", Git(project, "status --porcelain").Out, "la mia modifica resta da salvare, come prima");
            StringAssert.Contains(string.Join(" ", pulled.Warnings), "entry by entry");
            Assert.AreEqual(1, Directory.GetFiles(Path.Combine(project, ".git", "mde"), "development.yml.*.local").Length, "la copia di sicurezza");
        }

        [TestMethod]
        public async Task Leave_my_file_byte_for_byte_when_the_updates_do_not_touch_it()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, colleague) = Setup(ctx, "non-toccato");
            Publish(colleague, "nuovo.md", "# nuovo\n", "documento");
            var mine = Start.Replace("harness:", "agentCity:\n  roomSecret: 'SEGRETO'\nharness:"); // le virgolette: una riscrittura le toglierebbe
            File.WriteAllText(Path.Combine(project, ".development.yml"), mine);

            var pulled = await Pull(ctx, project);

            Assert.IsTrue(pulled.Success, pulled.Message ?? pulled.Refused);
            Assert.IsTrue(File.Exists(Path.Combine(project, "nuovo.md")));
            Assert.AreEqual(mine, Yml(project));
            Assert.AreEqual(0, pulled.Warnings.Count, string.Join(" | ", pulled.Warnings));
        }

        [TestMethod]
        public async Task Download_nothing_when_the_same_entry_changed_differently()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, colleague) = Setup(ctx, "conflitto");
            Publish(colleague, ".development.yml", Start.Replace("target: copilot", "target: opencode"), "opencode");
            var mine = Start.Replace("target: copilot", "target: claude");
            File.WriteAllText(Path.Combine(project, ".development.yml"), mine);
            var head = Git(project, "rev-parse HEAD").Out;

            var pulled = await Pull(ctx, project);

            Assert.IsFalse(pulled.Success);
            StringAssert.Contains(pulled.Refused, "«harness.target»");
            Assert.AreEqual(head, Git(project, "rev-parse HEAD").Out, "niente scaricato");
            Assert.AreEqual(mine, Yml(project), "il mio file non è stato toccato");
        }

        [TestMethod]
        public async Task Download_nothing_rather_than_lose_comments()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var withComment = "# configurazione del team, non toccare a mano\n" + Start;
            var (project, colleague) = Setup(ctx, "commenti", withComment);
            Publish(colleague, ".development.yml", withComment.Replace("target: copilot", "target: claude"), "claude");
            var mine = withComment.Replace("harness:", "agentCity:\n  roomSecret: SEGRETO\nharness:");
            File.WriteAllText(Path.Combine(project, ".development.yml"), mine);
            var head = Git(project, "rev-parse HEAD").Out;

            var pulled = await Pull(ctx, project);

            Assert.IsFalse(pulled.Success);
            StringAssert.Contains(pulled.Refused, "comments");
            Assert.AreEqual(head, Git(project, "rev-parse HEAD").Out);
            Assert.AreEqual(mine, Yml(project));
        }

        [TestMethod]
        public async Task Behave_as_before_when_other_files_are_changed_too()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, colleague) = Setup(ctx, "altri-file");
            Publish(colleague, ".development.yml", Start.Replace("target: copilot", "target: claude"), "claude");
            var mine = Start.Replace("harness:", "agentCity:\n  roomSecret: SEGRETO\nharness:");
            File.WriteAllText(Path.Combine(project, ".development.yml"), mine);
            File.WriteAllText(Path.Combine(project, "README.md"), "# progetto\nla mia riga\n");

            var pulled = await Pull(ctx, project);

            Assert.IsFalse(pulled.Success, "git rifiuta come prima: non è il caso del solo .development.yml");
            Assert.AreEqual(mine, Yml(project));
            Assert.IsFalse(Directory.Exists(Path.Combine(project, ".git", "mde")), "niente messo da parte");
        }

        /// <summary>
        /// N4 della valutazione del 07/11: l'unione si ferma su un file dell'utente dopo che MDE ha unito il suo .development.yml; la
        /// copia messa da parte (le mie modifiche senza commit) non deve tornare sopra quella unita, cancellando le voci del collega.
        /// </summary>
        [TestMethod]
        public async Task Keep_the_colleague_entries_when_the_merge_stops_on_a_user_file()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (project, colleague) = Setup(ctx, "unione-a-meta");
            Publish(colleague, ".development.yml", Start.Replace("target: copilot", "target: claude"), "motore");
            Publish(colleague, "README.md", "# progetto\nriga del collega\n", "readme");
            File.WriteAllText(Path.Combine(project, "README.md"), "# progetto\nriga mia\n");
            Git(project, "add README.md"); Git(project, "commit -q -m \"readme mio\"");
            // MDE ha scritto il segreto della chat e il commit automatico è spento: il file resta modificato, e il pull lo mette da parte
            File.WriteAllText(Path.Combine(project, ".development.yml"), Start.Replace("harness:", "agentCity:\n  enabled: false\n  roomSecret: SEGRETO\nharness:"));

            var pulled = await Pull(ctx, project);

            Assert.IsFalse(pulled.Success, "il README resta da sistemare a mano");
            StringAssert.Contains(Git(project, "status --porcelain").Out, "UU README.md");
            var yml = Yml(project);
            StringAssert.Contains(yml, "target: claude", "la voce del collega c'è ancora");
            StringAssert.Contains(yml, "roomSecret: SEGRETO", "e anche la mia");
        }

        /// <summary>
        /// Dallo schema 2 MDE scrive in testa <c># mde-schema: N</c>: non è un commento dell'utente, quindi il pull con il file modificato
        /// senza commit deve unire come prima, e tenere il numero più alto.
        /// </summary>
        [TestMethod]
        public async Task Merge_a_file_that_carries_the_schema_number()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var withNumber = "# mde-schema: 2\n" + Start;
            var (project, colleague) = Setup(ctx, "numero-schema", withNumber);
            Publish(colleague, ".development.yml", "# mde-schema: 3\n" + Start.Replace("target: copilot", "target: claude"), "motore");
            File.WriteAllText(Path.Combine(project, ".development.yml"), withNumber.Replace("harness:", "agentCity:\n  enabled: false\n  roomSecret: SEGRETO\nharness:"));

            var pulled = await Pull(ctx, project);

            Assert.IsTrue(pulled.Success, pulled.Message ?? pulled.Refused);
            var yml = Yml(project);
            StringAssert.StartsWith(yml, "# mde-schema: 3\n", "il numero più alto dei due");
            StringAssert.Contains(yml, "target: claude");
            StringAssert.Contains(yml, "roomSecret: SEGRETO");
        }
    }
}
