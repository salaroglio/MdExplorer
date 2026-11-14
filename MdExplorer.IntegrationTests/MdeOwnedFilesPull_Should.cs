using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Git;
using MdExplorer.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// S5 dello sprint «File di sistema di MDE»: il pull di MDE prima fa il commit dei file di MDE rimasti modificati, poi, se l'unione
    /// si ferma su file di MDE, li risolve da sé (.development.yml voce per voce, istruzioni con il blocco più recente). I casi veri:
    /// progetto A (segreto della chat scritto e mai salvato) e progetto B (due file di MDE, uno identico a quello del collega).
    /// Commit automatico acceso. Richiede <c>git</c> nel PATH.
    /// </summary>
    [TestClass]
    public class MdeOwnedFilesPull_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private static readonly Dictionary<string, string> On = new() { ["MdeAutoCommit:Enabled"] = "true", ["MdeAutoCommit:DelayMs"] = "100000" };

        private const string Start =
            "atlassian:\n  jiraBaseUrl: https://example.atlassian.net/\n  jiraProjectKeys:\n  - ABC\n  - DEF\n  confluenceSpaceKeys: []\n" +
            "harness:\n  target: copilot\n";

        private const string Instructions = ".github/copilot-instructions.md";
        private const string Skill = ".github/skills/mde-doc/SKILL.md";
        private static readonly string OldInstructions =
            "# Istruzioni di progetto\n\nRegola del gruppo.\n\n## Skill MdExplorer\n\n" + MdeInstructionsBlock.MdeDocSentence(Skill) + "\n";

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

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        private static async Task<RepoActionResult> Pull(AgentCityContext ctx, string project)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IRepoSyncService>().PullAsync(project, "");
            await ctx.Factory.Services.GetRequiredService<MdeAutoCommit>().FlushAsync();
            return result;
        }

        private static string Say(RepoActionResult r) => (r.Message ?? r.Refused ?? "") + " | " + string.Join(" | ", r.Warnings ?? Array.Empty<string>());

        private static (string Project, string Colleague) Setup(AgentCityContext ctx, string name)
        {
            var root = Path.Combine(ctx.Factory.DataDir, "pull-mde", name);
            var origin = Path.Combine(root, "origin.git");
            Directory.CreateDirectory(origin);
            Git(origin, "init -q --bare -b main");
            Git(root, $"clone -q \"{origin}\" collega");
            var colleague = Path.Combine(root, "collega");
            Directory.CreateDirectory(Path.Combine(colleague, ".github"));
            File.WriteAllText(Path.Combine(colleague, "README.md"), "# progetto\n\nprima riga\n");
            File.WriteAllText(Path.Combine(colleague, ".development.yml"), Start);
            File.WriteAllText(Path.Combine(colleague, Instructions), OldInstructions);
            Git(colleague, "add -A"); Git(colleague, "commit -q -m inizio"); Git(colleague, "push -q origin HEAD:main");

            var (_, project) = ctx.SeedProject(name);
            Git(project, "init -q -b main");
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

        private static string Read(string project, string file) => File.ReadAllText(Path.Combine(project, file)).Replace("\r\n", "\n");

        [TestMethod]
        public async Task Pull_the_room_secret_case_saving_it_first()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, colleague) = Setup(ctx, "segreto");
            Publish(colleague, ".development.yml", Start.Replace("  confluenceSpaceKeys: []\n", "  confluenceBaseUrl: https://example.atlassian.net/\n  confluenceSpaceKeys: []\n"), "confluence");
            Publish(colleague, "nuovo.md", "# nuovo\n", "documento");
            // MDE ha scritto il segreto della chat e nessuno ha fatto il commit (19/10)
            File.WriteAllText(Path.Combine(project, ".development.yml"), Start.Replace("harness:", "agentCity:\n  enabled: false\n  roomSecret: SEGRETO\nharness:"));

            var pulled = await Pull(ctx, project);

            Assert.IsTrue(pulled.Success, Say(pulled));
            Assert.IsTrue(File.Exists(Path.Combine(project, "nuovo.md")), "gli aggiornamenti sono arrivati");
            var yml = Read(project, ".development.yml");
            StringAssert.Contains(yml, "confluenceBaseUrl: https://example.atlassian.net/");
            StringAssert.Contains(yml, "roomSecret: SEGRETO");
            Assert.AreEqual(string.Empty, Git(project, "status --porcelain").Out, "niente lasciato a metà");
            StringAssert.Contains(Git(project, "log --format=%s -5").Out, "MDE: modifiche di MDE rimaste senza commit");
        }

        [TestMethod]
        public async Task Pull_two_mde_files_one_identical_to_the_colleague()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, colleague) = Setup(ctx, "due-file");
            var migrated = MdeInstructionsBlock.Apply(OldInstructions, Skill);
            // il MDE del collega ha migrato le istruzioni e lui ha fatto il commit; ha anche cambiato una descrizione
            Publish(colleague, Instructions, migrated, "istruzioni");
            Publish(colleague, ".development.yml", Start.Replace("harness:", "description: Documentazione del progetto\nharness:"), "descrizione");
            Publish(colleague, "nuovo.md", "# nuovo\n", "documento");
            // qui: lo stesso MDE ha fatto la stessa migrazione e ha scritto un partecipante, senza commit
            File.WriteAllText(Path.Combine(project, Instructions), migrated);
            File.WriteAllText(Path.Combine(project, ".development.yml"), Start.Replace("harness:", "participants:\n- name: Carlo\nharness:"));

            var pulled = await Pull(ctx, project);

            Assert.IsTrue(pulled.Success, Say(pulled));
            Assert.IsTrue(File.Exists(Path.Combine(project, "nuovo.md")));
            Assert.AreEqual(migrated, Read(project, Instructions));
            var yml = Read(project, ".development.yml");
            StringAssert.Contains(yml, "description: Documentazione del progetto");
            StringAssert.Contains(yml, "name: Carlo");
            Assert.AreEqual(string.Empty, Git(project, "status --porcelain").Out);
        }

        [TestMethod]
        public async Task Keep_the_newer_block_and_both_group_lines_in_the_instructions()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, colleague) = Setup(ctx, "blocco");
            var v1 = MdeInstructionsBlock.Apply(OldInstructions, Skill);
            var newer = MdeInstructionsBlock.StartMarker(MdeInstructionsBlock.Version + 1) + "\n## Skill MdExplorer\n\ntesto di un MDE più nuovo\n" + MdeInstructionsBlock.EndMarker;
            // il collega: un MDE più nuovo (blocco v+1) e una riga del gruppo in fondo
            Publish(colleague, Instructions, v1.Replace(MdeInstructionsBlock.Block(Skill), newer) + "\nRiga del collega.\n", "istruzioni nuove");
            // qui: il blocco di questo MDE e una riga del gruppo in cima, già salvati
            File.WriteAllText(Path.Combine(project, Instructions), v1.Replace("Regola del gruppo.", "Regola del gruppo.\nRiga mia."));
            Git(project, "add -A"); Git(project, "commit -q -m \"riga mia\"");

            var pulled = await Pull(ctx, project);

            Assert.IsTrue(pulled.Success, Say(pulled));
            var text = Read(project, Instructions);
            StringAssert.Contains(text, "Riga mia.");
            StringAssert.Contains(text, "Riga del collega.");
            StringAssert.Contains(text, "testo di un MDE più nuovo", "vince il blocco più recente");
            Assert.AreEqual(1, MdeInstructionsBlock.FindBlock(text).Success ? 1 : 0);
            Assert.IsFalse(text.Contains("<<<<<<<"), text);
            Assert.AreEqual(string.Empty, Git(project, "status --porcelain").Out);
        }

        [TestMethod]
        public async Task Go_back_and_name_the_entry_when_it_changed_in_two_ways()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, colleague) = Setup(ctx, "voce");
            Publish(colleague, ".development.yml", Start.Replace("target: copilot", "target: opencode"), "opencode");
            File.WriteAllText(Path.Combine(project, ".development.yml"), Start.Replace("target: copilot", "target: claude"));

            var pulled = await Pull(ctx, project);

            Assert.IsFalse(pulled.Success);
            StringAssert.Contains(Say(pulled), "«harness.target»");
            Assert.IsFalse(Git(project, "rev-parse --quiet --verify MERGE_HEAD").Code == 0, "nessuna unione lasciata a metà");
            StringAssert.Contains(Read(project, ".development.yml"), "target: claude", "la scelta di qui resta");
            Assert.AreEqual(string.Empty, Git(project, "status --porcelain").Out, "salvata nel commit di MDE prima del pull");
        }

        [TestMethod]
        public async Task Resolve_its_own_files_and_leave_the_user_file_to_the_user()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext(On);
            var (project, colleague) = Setup(ctx, "misto");
            Publish(colleague, ".development.yml", Start.Replace("harness:", "description: dal collega\nharness:"), "descrizione");
            Publish(colleague, "README.md", "# progetto\n\nriga del collega\n", "readme");
            File.WriteAllText(Path.Combine(project, ".development.yml"), Start.Replace("harness:", "participants:\n- name: Carlo\nharness:"));
            File.WriteAllText(Path.Combine(project, "README.md"), "# progetto\n\nriga mia\n");
            Git(project, "add README.md"); Git(project, "commit -q -m \"readme mio\"");

            var pulled = await Pull(ctx, project);

            Assert.IsFalse(pulled.Success);
            StringAssert.Contains(Say(pulled), "README.md");
            var yml = Read(project, ".development.yml");
            Assert.IsFalse(yml.Contains("<<<<<<<"), "il file di MDE è già unito");
            StringAssert.Contains(yml, "description: dal collega");
            StringAssert.Contains(yml, "name: Carlo");
            StringAssert.Contains(Git(project, "status --porcelain").Out, "UU README.md", "il README resta all'utente");
        }
    }
}
