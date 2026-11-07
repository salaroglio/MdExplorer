using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// S0-S1 dello sprint «File di sistema di MDE» (docs-internal/Sprints/2026-11-07-File-Di-Sistema-MDE.md): MDE scrive i suoi
    /// file del repository solo quando il contenuto cambia, e sempre con lo stesso serializzatore senza voci vuote. Prima ogni
    /// salvataggio riscriveva il file, e in progetto B `.development.yml` si era riempito di `description:` vuoti che bloccavano il pull.
    /// <para>Il secondo gruppo richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class MdeSystemFilesWrite_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private static string TempFile(string name)
        {
            var dir = Path.Combine(Path.GetTempPath(), "mde-s1-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, name);
        }

        [TestMethod]
        public void Not_rewrite_a_file_whose_content_is_the_same()
        {
            var file = TempFile(".development.yml");
            Assert.IsTrue(MdeRepoWriter.WriteIfChanged(file, "harness:\n  target: copilot\n"), "il file nuovo si scrive");
            var before = File.GetLastWriteTimeUtc(file);
            File.SetLastWriteTimeUtc(file, before.AddMinutes(-5));
            Assert.IsFalse(MdeRepoWriter.WriteIfChanged(file, "harness:\n  target: copilot\n"));
            Assert.AreEqual(before.AddMinutes(-5), File.GetLastWriteTimeUtc(file), "il file non è stato toccato");
            Assert.IsTrue(MdeRepoWriter.WriteIfChanged(file, "harness:\n  target: claude\n"));
        }

        [TestMethod]
        public void Treat_crlf_on_disk_as_the_same_text_and_keep_crlf_and_bom_when_writing()
        {
            var file = TempFile(".development.yml");
            var crlfWithBom = new UTF8Encoding(true).GetPreamble();
            File.WriteAllBytes(file, Concat(crlfWithBom, Encoding.UTF8.GetBytes("harness:\r\n  target: copilot\r\n")));
            var bytes = File.ReadAllBytes(file);

            Assert.IsFalse(MdeRepoWriter.WriteIfChanged(file, "harness:\n  target: copilot\n"), "stesso testo, fine riga diversi: non si scrive");
            CollectionAssert.AreEqual(bytes, File.ReadAllBytes(file));

            Assert.IsTrue(MdeRepoWriter.WriteIfChanged(file, "harness:\n  target: claude\n"));
            var after = File.ReadAllBytes(file);
            Assert.AreEqual(0xEF, after[0], "il BOM resta");
            StringAssert.Contains(Encoding.UTF8.GetString(after), "harness:\r\n  target: claude\r\n", "il CRLF resta");
        }

        [TestMethod]
        public void Serialize_the_development_config_without_empty_entries()
        {
            var config = new DevelopmentConfig();
            config.Folders.Add(new DevelopmentFolder { Path = "bde_legacy", Tags = new() { "program" } });
            var yaml = MdeRepoWriter.SerializeDevelopmentConfig(config);
            StringAssert.Contains(yaml, "path: bde_legacy");
            Assert.IsFalse(yaml.Contains("description:"), yaml);
            Assert.IsFalse(yaml.Contains("knowledgeGraph:"), yaml);
            Assert.IsFalse(yaml.Contains("codeGraph:"), yaml);
        }

        [TestMethod]
        public void Know_which_files_are_of_mde()
        {
            Assert.AreEqual(MdeMergeKind.YamlEntries, MdeOwnedFiles.Find(".development.yml")!.Kind);
            Assert.AreEqual(MdeMergeKind.Block, MdeOwnedFiles.Find(@".github\copilot-instructions.md")!.Kind);
            Assert.AreEqual(MdeMergeKind.Block, MdeOwnedFiles.Find("./CLAUDE.md")!.Kind);
            Assert.IsNull(MdeOwnedFiles.Find(".gitignore"), ".gitignore resta dell'utente");
            Assert.IsNull(MdeOwnedFiles.Find("docs/readme.md"));
        }

        // ---------------------------------------------------------------- con un repository vero

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
            p.WaitForExit(20000);
            return (p.ExitCode, o.Trim());
        }

        private static StringContent Json(object o) => new(JsonSerializer.Serialize(o), Encoding.UTF8, "application/json");

        private static string CommittedRepo(AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            Git(path, "init -q -b main");
            Git(path, "config user.email prova@test.local"); Git(path, "config user.name Prova"); Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# prova\n");
            Git(path, "add -A"); Git(path, "commit -q -m inizio");
            return path;
        }

        [TestMethod]
        public async Task Leave_the_repository_clean_when_a_project_is_opened_again()
        {
            if (Git(Path.GetTempPath(), "--version").Code != 0) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = CommittedRepo(ctx, "riapertura");

            var first = await ctx.Client.PostAsync("/api/MdProjects/SetFolderProject?ConnectionId=s1-uno", Json(new { path }));
            Assert.IsTrue(first.IsSuccessStatusCode, await first.Content.ReadAsStringAsync());
            // la prima apertura crea i file di MDE: li si salva, come farà il commit automatico di S2
            Git(path, "add -A"); Git(path, "commit -q -m \"prima apertura\"");

            var second = await ctx.Client.PostAsync("/api/MdProjects/SetFolderProject?ConnectionId=s1-due", Json(new { path }));
            Assert.IsTrue(second.IsSuccessStatusCode, await second.Content.ReadAsStringAsync());

            Assert.AreEqual(string.Empty, Git(path, "status --porcelain").Out, "riaprire il progetto non deve lasciare modifiche");
        }

        [TestMethod]
        public async Task Leave_the_repository_clean_when_the_same_setting_is_saved_again()
        {
            if (Git(Path.GetTempPath(), "--version").Code != 0) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = CommittedRepo(ctx, "stessa-impostazione");

            var first = await ctx.Client.PostAsync("/api/ProjectSettings/SetHarness", Json(new { projectPath = path, target = "claude" }));
            Assert.IsTrue(first.IsSuccessStatusCode, await first.Content.ReadAsStringAsync());
            Git(path, "add -A"); Git(path, "commit -q -m harness");

            var second = await ctx.Client.PostAsync("/api/ProjectSettings/SetHarness", Json(new { projectPath = path, target = "claude" }));
            Assert.IsTrue(second.IsSuccessStatusCode, await second.Content.ReadAsStringAsync());

            Assert.AreEqual(string.Empty, Git(path, "status --porcelain").Out, "salvare la stessa scelta non deve cambiare niente");
        }

        [TestMethod]
        public async Task Write_the_same_text_whoever_saves_the_configuration()
        {
            // Il caso di progetto B: due scrittori, uno con OmitNull e uno senza. Salvare la stessa cosa dal secondo dopo il primo
            // rimetteva description:/knowledgeGraph:/codeGraph: vuoti e il file risultava modificato.
            if (Git(Path.GetTempPath(), "--version").Code != 0) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = CommittedRepo(ctx, "due-scrittori");

            var r1 = await ctx.Client.PostAsync("/api/compatibility/mode", Json(new { projectPath = path, mode = "mdexplorer", gitHubOptions = new { } }));
            Assert.IsTrue(r1.IsSuccessStatusCode, "compatibilità: " + await r1.Content.ReadAsStringAsync());
            var r2 = await ctx.Client.PostAsync("/api/ProjectSettings/SetHarness", Json(new { projectPath = path, target = "claude" }));
            Assert.IsTrue(r2.IsSuccessStatusCode, "harness: " + await r2.Content.ReadAsStringAsync());
            Git(path, "add -A"); Git(path, "commit -q -m configurazione");

            var again = await ctx.Client.PostAsync("/api/compatibility/mode", Json(new { projectPath = path, mode = "mdexplorer", gitHubOptions = new { } }));
            Assert.IsTrue(again.IsSuccessStatusCode, await again.Content.ReadAsStringAsync());

            Assert.AreEqual(string.Empty, Git(path, "status --porcelain").Out, File.ReadAllText(Path.Combine(path, ".development.yml")));
            Assert.IsFalse(File.ReadAllText(Path.Combine(path, ".development.yml")).Contains(": \n") , "nessuna voce vuota");
        }

        private static byte[] Concat(byte[] a, byte[] b)
        {
            var r = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, r, 0, a.Length);
            Buffer.BlockCopy(b, 0, r, a.Length, b.Length);
            return r;
        }
    }
}
