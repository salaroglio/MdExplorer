using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Demo;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// L'origin locale è un aiuto <b>solo del demo</b>: chi clona il demo da GitHub ha un repository di sola lettura,
    /// e senza un posto dove scrivere «Autorizza» non può pubblicare. Questi test dicono due cose: che per il demo
    /// funziona senza rete, e — la più importante — che un progetto qualunque non viene toccato nemmeno di un byte.
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class DemoOriginPreparer_Should
    {
        private const string DemoUrl = "https://github.com/salaroglio/mdexplorer-demo.git";

        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-demo-origin", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                foreach (var f in Directory.GetFiles(_root, "*", SearchOption.AllDirectories))
                    File.SetAttributes(f, FileAttributes.Normal);
                Directory.Delete(_root, true);
            }
            catch (IOException) { /* la pulizia non è il soggetto del test */ }
        }

        private static string Git(string cwd, params string[] args)
        {
            var psi = new ProcessStartInfo("git") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, WorkingDirectory = cwd };
            foreach (var a in args) psi.ArgumentList.Add(a);
            psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
            using var p = Process.Start(psi);
            var err = p.StandardError.ReadToEndAsync();
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Assert.AreEqual(0, p.ExitCode, $"git {string.Join(' ', args)}: {err.Result}");
            return output;
        }

        /// <summary>Un clone che per git è il demo: origin = indirizzo del demo, ma il contenuto viene da un bare locale.</summary>
        private string DemoClone(string name = "mdexplorer-demo", string originUrl = DemoUrl)
        {
            var seed = Path.Combine(_root, "seed");
            Directory.CreateDirectory(seed);
            Git(seed, "init", "-q", "-b", "main");
            Git(seed, "config", "user.email", "t@example.com");
            Git(seed, "config", "user.name", "T");
            File.WriteAllText(Path.Combine(seed, "README.md"), "# demo\n");
            Git(seed, "add", "-A");
            Git(seed, "commit", "-q", "-m", "demo");
            var fakeGithub = Path.Combine(_root, "fake-github.git");
            Git(_root, "clone", "-q", "--bare", seed, fakeGithub);

            var work = Path.Combine(_root, name);
            Git(_root, "clone", "-q", fakeGithub, work);
            Git(work, "config", "user.email", "t@example.com");
            Git(work, "config", "user.name", "T");
            Git(work, "remote", "set-url", "origin", originUrl);
            return work;
        }

        private static DemoOriginPreparer Preparer() => new DemoOriginPreparer(NullLogger<DemoOriginPreparer>.Instance);

        [TestMethod]
        public void Give_the_demo_clone_an_origin_the_reader_can_write_to()
        {
            var work = DemoClone();

            var outcome = Preparer().Prepare(work);

            Assert.AreEqual(DemoOriginStatus.Prepared, outcome.Status, outcome.Error);
            Assert.AreEqual(work + ".origin.git", outcome.Origin);
            Assert.AreEqual(work + ".origin.git", Git(work, "remote", "get-url", "origin").Trim());
            Assert.AreEqual(DemoUrl, Git(work, "remote", "get-url", "upstream").Trim(), "GitHub non si perde: resta come 'upstream'");
            Assert.AreEqual("true", Git(work, "config", "--local", "--get", DemoOriginPreparer.MarkerKey).Trim());

            // È un clone come gli altri: segue origin/main, niente avanti/indietro, e non ha niente da committare.
            Assert.AreEqual("## main...origin/main", Git(work, "status", "-sb").Trim());

            // E si può scrivere: è il motivo per cui esiste.
            File.WriteAllText(Path.Combine(work, "nuovo.md"), "x\n");
            Git(work, "add", "-A");
            Git(work, "commit", "-q", "-m", "prova");
            Git(work, "push", "-q", "origin", "HEAD:main");
            Assert.AreEqual("## main...origin/main", Git(work, "status", "-sb").Trim());

            // I posti di lavoro degli agenti partono da origin/HEAD.
            Assert.AreEqual("origin/main", Git(work, "rev-parse", "--abbrev-ref", "origin/HEAD").Trim());
        }

        [TestMethod]
        public void Not_touch_a_project_that_is_not_the_demo()
        {
            var other = DemoClone("progetto-mio", "https://github.com/qualcuno/altro-progetto.git");
            var configBefore = File.ReadAllBytes(Path.Combine(other, ".git", "config"));
            var entriesBefore = Directory.GetFileSystemEntries(_root).Length;

            var outcome = Preparer().Prepare(other);

            Assert.AreEqual(DemoOriginStatus.NotTheDemo, outcome.Status);
            CollectionAssert.AreEqual(configBefore, File.ReadAllBytes(Path.Combine(other, ".git", "config")),
                "la configurazione di git di un progetto qualunque non cambia di un byte");
            Assert.AreEqual(entriesBefore, Directory.GetFileSystemEntries(_root).Length, "nessuna cartella nuova accanto al progetto");
            Assert.IsFalse(Directory.Exists(other + ".origin.git"));
            Assert.AreEqual("https://github.com/qualcuno/altro-progetto.git", Git(other, "remote", "get-url", "origin").Trim());
        }

        [TestMethod]
        public void Not_touch_a_folder_that_is_not_a_git_repository()
        {
            var plain = Path.Combine(_root, "cartella");
            Directory.CreateDirectory(plain);
            File.WriteAllText(Path.Combine(plain, "a.md"), "x");

            Assert.AreEqual(DemoOriginStatus.NotTheDemo, Preparer().Prepare(plain).Status);
            Assert.AreEqual(DemoOriginStatus.NotTheDemo, Preparer().Prepare(Path.Combine(_root, "non-esiste")).Status);
            Assert.AreEqual(DemoOriginStatus.NotTheDemo, Preparer().Prepare(null).Status);
            CollectionAssert.AreEqual(new[] { "a.md" }, Directory.GetFileSystemEntries(plain, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName).ToArray());
        }

        [TestMethod]
        public void Do_nothing_the_second_time()
        {
            var work = DemoClone();
            Assert.AreEqual(DemoOriginStatus.Prepared, Preparer().Prepare(work).Status);
            var configAfterFirst = File.ReadAllBytes(Path.Combine(work, ".git", "config"));

            var second = Preparer().Prepare(work);

            Assert.AreEqual(DemoOriginStatus.AlreadyPrepared, second.Status);
            CollectionAssert.AreEqual(configAfterFirst, File.ReadAllBytes(Path.Combine(work, ".git", "config")));
        }

        [TestMethod]
        public void Say_why_and_leave_the_clone_alone_when_the_copy_cannot_be_made()
        {
            var work = DemoClone();
            Directory.CreateDirectory(work + ".origin.git"); // c'è già qualcosa col nome che servirebbe
            var configBefore = File.ReadAllBytes(Path.Combine(work, ".git", "config"));

            var outcome = Preparer().Prepare(work);

            Assert.AreEqual(DemoOriginStatus.Failed, outcome.Status);
            StringAssert.Contains(outcome.Error, "esiste già");
            CollectionAssert.AreEqual(configBefore, File.ReadAllBytes(Path.Combine(work, ".git", "config")),
                "se non può prepararlo, non lascia il clone a metà");
            Assert.AreEqual(DemoUrl, Git(work, "remote", "get-url", "origin").Trim());
        }

        [DataTestMethod]
        [DataRow("https://github.com/salaroglio/mdexplorer-demo.git", true)]
        [DataRow("https://github.com/salaroglio/mdexplorer-demo", true)]
        [DataRow("https://github.com/Salaroglio/MdExplorer-Demo.git/", true)]
        [DataRow("git@github.com:salaroglio/mdexplorer-demo.git", true)]
        [DataRow("https://github.com/salaroglio/mdexplorer", false)]
        [DataRow("https://github.com/altro/mdexplorer-demo.git", false)]
        [DataRow("https://github.com/salaroglio/mdexplorer-demo-fork.git", false)]
        [DataRow("", false)]
        [DataRow(null, false)]
        public void Recognize_only_the_public_demo_repository(string url, bool expected)
            => Assert.AreEqual(expected, DemoOriginPreparer.IsDemoRepositoryUrl(url));

        [TestMethod]
        public async Task Let_a_normal_project_be_opened_without_the_demo_flag()
        {
            // Aprire un progetto manda solo { path }: il campo del demo è nullable, quindi non è mai obbligatorio
            // (una string non nullable nel DTO diventerebbe un [Required] implicito, vedi HarnessSettingsEndpoint_Should).
            using var ctx = new AgentCityContext();
            var res = await ctx.Client.PostAsync("/api/MdProjects/SetFolderProject",
                new StringContent(JsonSerializer.Serialize(new { path = "/tmp/qualsiasi" }), Encoding.UTF8, "application/json"));

            StringAssert.Contains(await res.Content.ReadAsStringAsync(), "ConnectionId",
                "il metodo deve essere stato eseguito: nessun errore di validazione sul DTO");
        }
    }
}
