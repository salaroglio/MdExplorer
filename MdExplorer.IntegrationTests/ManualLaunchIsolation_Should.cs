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
    /// Dove lavora un agente lanciato a mano: la spunta del dialogo decide il singolo lancio,
    /// l'impostazione del progetto fa il default.
    /// <para>
    /// Prima il lancio manuale ignorava l'isolamento e scriveva sempre nel progetto, sul ramo
    /// dell'utente. Il difetto non era «l'agente scrive nel progetto» — per un ritocco veloce è
    /// comodo — ma che <b>un flag governasse solo metà dei percorsi</b>: acceso l'isolamento, un
    /// agente svegliato da un messaggio finiva in un worktree e lo stesso agente lanciato dal
    /// pulsante finiva sul tuo ramo, senza che niente lo dicesse.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class ManualLaunchIsolation_Should
    {
        [TestMethod]
        public async Task Follow_the_project_setting_when_nobody_ticks_anything()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "lancio-default");
            var agentFile = WriteAgent(path, "curatore");
            Pref(ctx).Set(path, true);

            await Launch(ctx, path, agentFile, useWorktree: null);

            // Isolamento acceso e nessuna spunta esplicita: lavora nel suo posto, non nel progetto.
            StringAssert.StartsWith(ctx.Runner.LastRequest.WorkingDirectory, Path.Combine(path, ".worktrees"));
        }

        [TestMethod]
        public async Task Let_the_tick_win_over_the_setting()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "lancio-spunta");
            var agentFile = WriteAgent(path, "curatore");
            Pref(ctx).Set(path, true);

            // «Lancia e guarda cosa fa»: voglio il risultato qui, sotto gli occhi, anche se di
            // solito questo progetto lavora isolato.
            await Launch(ctx, path, agentFile, useWorktree: false);

            Assert.AreEqual(path, ctx.Runner.LastRequest.WorkingDirectory,
                "la spunta decide questo lancio, l'impostazione dice come si lavora di solito");
        }

        [TestMethod]
        public async Task Isolate_on_request_even_where_it_is_not_the_habit()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "lancio-isola");
            var agentFile = WriteAgent(path, "curatore");
            Pref(ctx).Set(path, false);

            await Launch(ctx, path, agentFile, useWorktree: true);

            StringAssert.StartsWith(ctx.Runner.LastRequest.WorkingDirectory, Path.Combine(path, ".worktrees"));

            // Le claim e l'ambiente restano sul progetto vero: cambia solo dove scrive, non chi è.
            Assert.AreEqual(path, ctx.Runner.LastRequest.ProjectPath,
                "l'agente parla a nome del progetto, non del posto di lavoro");
        }

        [TestMethod]
        public async Task Refuse_rather_than_fall_back_onto_your_branch()
        {
            using var ctx = new AgentCityContext();
            // Progetto SENZA git: un posto di lavoro non è preparabile.
            var (_, path) = ctx.SeedProject("lancio-senza-git");
            var agentFile = WriteAgent(path, "curatore");

            await Launch(ctx, path, agentFile, useWorktree: true);

            // Chi ha chiesto l'isolamento non deve ritrovarsi l'agente dentro il proprio ramo
            // perché un git è andato storto: meglio un run fallito che un ripiego silenzioso.
            Assert.IsNull(ctx.Runner.LastRequest,
                "nessun turno deve partire se il posto di lavoro non è preparabile");
        }

        // ---- infrastruttura ----

        private static IAgentWorktreePreference Pref(AgentCityContext ctx)
            => ctx.Factory.Services.GetRequiredService<IAgentWorktreePreference>();

        private static Task Launch(AgentCityContext ctx, string path, string agentFile, bool? useWorktree)
            => ctx.Factory.Services.GetRequiredService<IAgentRunJobService>().RunAsync(new AgentRunRequestModel
            {
                ProjectPath = path,
                AgentFilePath = agentFile,
                PreparedPrompt = "fai qualcosa",
                TriggerSource = "manual",
                UseWorktree = useWorktree,
            });

        private static string WriteAgent(string projectPath, string name)
        {
            var dir = Path.Combine(projectPath, ".github", "agents");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, name + ".agent.md");
            File.WriteAllText(file, string.Join("\n", new[]
            {
                "---", "description: agente di prova", "tools: [read, write]",
                "a2a:", "  name: " + name, "  role: prova", "  accepts_messages_from: [\"*\"]",
                "---", "", "# " + name, "", "Fai quello che ti si chiede.",
            }));
            return file;
        }

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
            p.WaitForExit(60000);
            return (p.ExitCode, o);
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        private static string SetupGitProject(AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin);
            Git(origin, "init --bare");
            Git(origin, "symbolic-ref HEAD refs/heads/main");

            Git(path, "init -b main");
            Git(path, "config user.email carlo@test.local");
            Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\n");
            Git(path, "add -A");
            Git(path, "commit -m base");
            Git(path, $"remote add origin \"{origin}\"");
            Git(path, "push -u origin main");
            return path;
        }
    }
}
