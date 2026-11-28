using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.Codex;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La sessione Codex su <c>codex app-server</c>: la configurazione del thread si legge senza lanciare niente; le prove
    /// col CLI vero girano solo se Codex è installato e collegato (altrimenti sono inconcludenti, e lo dicono).
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F1.</para>
    /// </summary>
    [TestClass]
    public class CodexSession_Should
    {
        [TestMethod]
        public void Switch_off_the_users_personal_mcp_servers_and_keep_ours()
        {
            var options = new CodexSessionOptions
            {
                McpServers = new[] { new CodexMcpServer { Name = "mdexplorer", Command = "/x/MdExplorer.Mcp", Args = new[] { "--groups", "core" } } },
            };

            var config = options.BuildConfig(new[] { "personale", "mdexplorer" });

            Assert.AreEqual(false, (bool)config["mcp_servers"]["personale"]["enabled"], "un server personale non entra nelle sessioni di MDE");
            var ours = config["mcp_servers"]["mdexplorer"];
            Assert.AreEqual("/x/MdExplorer.Mcp", (string)ours["command"]);
            Assert.IsNull(ours["enabled"], "il nostro omonimo resta acceso");
            // Misurati il 28/11: senza DOTNET_ROOT il server .NET non parte, senza "approve" ogni chiamata fallisce.
            CollectionAssert.Contains(ours["env_vars"].AsArray().Select(v => (string)v).ToList(), "DOTNET_ROOT");
            Assert.AreEqual("approve", (string)ours["default_tools_approval_mode"]);
        }

        [TestMethod]
        public void Always_switch_off_apps_browser_computer_and_images()
        {
            var config = CodexSessionOptions.Default.BuildConfig(null);

            foreach (var feature in new[] { "apps", "browser_use", "computer_use", "image_generation" })
                Assert.AreEqual(false, (bool)config["features"][feature], feature);
            Assert.AreEqual("disabled", (string)config["web_search"]);
            Assert.IsNull(config["features"]["shell_tool"], "la shell resta, se non la si spegne");
        }

        [TestMethod]
        public void Turn_on_the_web_and_switch_off_the_shell_only_when_asked()
        {
            var config = new CodexSessionOptions { WebSearch = true, Shell = false }.BuildConfig(null);

            Assert.AreEqual("live", (string)config["web_search"]);
            Assert.AreEqual(false, (bool)config["features"]["shell_tool"]);
        }

        [TestMethod]
        public void Let_write_in_the_extra_roots_only_with_workspace_write()
        {
            var roots = new[] { "/codice/altro" };
            var write = new CodexSessionOptions { Sandbox = CodexSandbox.WorkspaceWrite, WritableRoots = roots }.BuildConfig(null);
            var read = new CodexSessionOptions { Sandbox = CodexSandbox.ReadOnly, WritableRoots = roots }.BuildConfig(null);

            Assert.AreEqual("/codice/altro", (string)write["sandbox_workspace_write"]["writable_roots"][0]);
            Assert.IsNull(read["sandbox_workspace_write"]);
        }

        [TestMethod]
        public void Ask_codex_for_approvals_only_when_someone_decides()
        {
            Assert.AreEqual("never", CodexSessionOptions.Default.ApprovalPolicyId);
            Assert.AreEqual("untrusted", new CodexSessionOptions { Approve = _ => CodexApprovalDecision.Yes }.ApprovalPolicyId);
            Assert.AreEqual("read-only", new CodexSessionOptions { Sandbox = CodexSandbox.ReadOnly }.SandboxId);
        }

        [TestMethod]
        public void Read_the_usage_and_the_quota_as_codex_sends_them()
        {
            // Righe vere di Codex 0.172.0 (28/11/2026).
            using var usage = JsonDocument.Parse("""
                {"threadId":"t","turnId":"u","tokenUsage":{"total":{"totalTokens":26747,"inputTokens":26565,"cachedInputTokens":23808,"outputTokens":182,"reasoningOutputTokens":42},
                 "last":{"totalTokens":13435,"inputTokens":13357,"cachedInputTokens":13056,"outputTokens":78,"reasoningOutputTokens":42},"modelContextWindow":258400}}
                """);
            using var limits = JsonDocument.Parse("""
                {"rateLimits":{"limitId":"codex","primary":{"usedPercent":3,"windowDurationMins":43200,"resetsAt":1798447835},"secondary":null,"planType":"free","rateLimitReachedType":null}}
                """);

            var turn = CodexSession.ReadUsage(usage.RootElement);
            var quota = CodexSession.ReadRateLimit(limits.RootElement);

            Assert.AreEqual(13435, turn.TotalTokens, "il turno è `last`");
            Assert.AreEqual(26747, turn.ThreadTotalTokens, "la conversazione è `total`");
            Assert.AreEqual(258400, turn.ContextWindow);
            Assert.AreEqual("free", quota.PlanType);
            Assert.AreEqual(3, quota.PrimaryUsedPercent);
            Assert.AreEqual(43200, quota.PrimaryWindowMinutes);
            Assert.IsNull(quota.SecondaryUsedPercent);
        }

        [TestMethod]
        public void Read_the_cli_version_from_the_user_agent()
        {
            Assert.AreEqual("0.172.0", CodexSession.VersionOf("mdexplorer/0.172.0 (Ubuntu 22.4.0; x86_64) gnome-terminal (mdexplorer; 0.0)"));
            Assert.IsNull(CodexSession.VersionOf(null));
        }

        [TestMethod]
        public void Put_a_filesystem_profile_in_place_of_the_sandbox()
        {
            var config = new CodexSessionOptions
            {
                FileSystemProfile = new Dictionary<string, string> { ["**/credenziali-*.txt"] = "deny", ["AGENTS.md"] = "read", ["/fuori/segreti.txt"] = "deny" },
            }.BuildConfig(null);

            Assert.AreEqual(CodexSessionOptions.ProfileName, (string)config["default_permissions"]);
            var profile = config["permissions"][CodexSessionOptions.ProfileName];
            Assert.AreEqual(":workspace", (string)profile["extends"]);
            Assert.AreEqual("deny", (string)profile["filesystem"][":workspace_roots"]["**/credenziali-*.txt"]);
            Assert.AreEqual("read", (string)profile["filesystem"][":workspace_roots"]["AGENTS.md"]);
            Assert.AreEqual("deny", (string)profile["filesystem"]["/fuori/segreti.txt"], "un percorso assoluto vale ovunque");
        }

        [TestMethod]
        public void Build_the_e2e_profile_from_credentials_protected_paths_and_existing_instructions()
        {
            var dir = NewProject();
            Directory.CreateDirectory(Path.Combine(dir, "sotto"));
            File.WriteAllText(Path.Combine(dir, "sotto", "AGENTS.md"), "# istruzioni\n");
            var outside = Path.Combine(Path.GetTempPath(), "segreti-" + Guid.NewGuid().ToString("N")[..6] + ".txt");

            var profile = MdExplorer.Services.E2e.E2eLaunchService.CodexE2eProfile(dir, new[] { Path.Combine(dir, "sotto", "s.txt"), outside });

            Assert.AreEqual("deny", profile["**/credenziali-*.txt"]);
            Assert.AreEqual("read", profile[".codex"]);
            Assert.AreEqual("read", profile["AGENTS.md"]);
            Assert.AreEqual("read", profile["sotto/AGENTS.md"], "un file di istruzioni che c'è già, per percorso esatto");
            Assert.AreEqual("deny", profile["sotto/s.txt"]);
            Assert.AreEqual("deny", profile[Path.GetFullPath(outside)]);
            Assert.IsFalse(profile.Keys.Any(k => k.Contains("**/AGENTS.md")), "Codex accetta glob solo con deny, e deny su AGENTS.md ferma il thread");
        }

        [TestMethod]
        public async Task Keep_credentials_and_agent_configuration_out_of_reach_in_an_e2e_session()
        {
            RequireCodex();
            var dir = NewProject();
            File.WriteAllText(Path.Combine(dir, "AGENTS.md"), "Rispondi in italiano.\n");
            File.WriteAllText(Path.Combine(dir, "credenziali-prova.txt"), "PASSWORD=segretissima\n");
            await using var session = new CodexSession(NullLogger.Instance, dir, null, new CodexSessionOptions
            {
                FileSystemProfile = MdExplorer.Services.E2e.E2eLaunchService.CodexE2eProfile(dir, Array.Empty<string>()),
            });
            await session.StartAsync();

            var (_, chunks) = await AskAsync(session,
                "Esegui esattamente, senza commenti, in un solo comando: echo x >> AGENTS.md ; echo y > ok.txt ; cat credenziali-prova.txt");

            Assert.AreEqual("Rispondi in italiano.\n", File.ReadAllText(Path.Combine(dir, "AGENTS.md")), "le istruzioni restano in sola lettura");
            Assert.IsFalse(chunks.Any(c => (c.Record?.Content ?? c.Text ?? string.Empty).Contains("segretissima")), "le credenziali non si leggono");
            // Il modello a volte si rifiuta di lanciare un comando che legge credenziali: è una sua scelta, non il sandbox.
            if (!chunks.Any(c => c.Kind == CodexChunk.KindTool && (c.Text ?? string.Empty).StartsWith("shell", StringComparison.Ordinal)))
                Assert.Inconclusive("Codex non ha eseguito il comando: la scrittura del resto del progetto non è stata provata.");
            Assert.IsTrue(File.Exists(Path.Combine(dir, "ok.txt")), "il resto del progetto si scrive: " +
                string.Join(" | ", chunks.Select(c => c.Kind + ":" + (c.Record?.Content ?? c.Text))));
        }

        // ---- Con Codex vero ----

        private static void RequireCodex()
        {
            if (!CodexProcessLauncher.IsResolvable())
                Assert.Inconclusive($"Codex non è nel PATH: la prova col CLI vero non è stata eseguita. `{CodexProcessLauncher.InstallCommand}` per eseguirla.");
        }

        private static string NewProject()
        {
            var dir = Path.Combine(Path.GetTempPath(), "mde-codex-" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "README.md"), "# Prova\n");
            return dir;
        }

        private static async Task<(string Text, List<CodexChunk> Chunks)> AskAsync(CodexSession session, string prompt)
        {
            var text = new StringBuilder();
            var chunks = new List<CodexChunk>();
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            await foreach (var chunk in session.PromptAsync(prompt, cts.Token))
            {
                chunks.Add(chunk);
                if (chunk.Kind == CodexChunk.KindMessage) text.Append(chunk.Text);
            }
            return (text.ToString(), chunks);
        }

        [TestMethod]
        public async Task Answer_and_remember_the_conversation_in_a_new_process()
        {
            RequireCodex();
            var dir = NewProject();
            string threadId;
            await using (var first = new CodexSession(NullLogger.Instance, dir, null, new CodexSessionOptions { Sandbox = CodexSandbox.ReadOnly }))
            {
                await first.StartAsync();
                var (text, _) = await AskAsync(first, "Ricorda la parola ALBICOCCA. Rispondi solo OK.");
                StringAssert.Contains(text.ToUpperInvariant(), "OK");
                Assert.IsNotNull(first.LastTurnUsage?.TotalTokens, "il consumo del turno arriva");
                threadId = first.SessionId;
            }

            await using var second = new CodexSession(NullLogger.Instance, dir, null,
                new CodexSessionOptions { Sandbox = CodexSandbox.ReadOnly }.WithResume(threadId));
            await second.StartAsync();
            var (again, _) = await AskAsync(second, "Quale parola ti ho chiesto di ricordare? Rispondi con la sola parola.");

            Assert.AreEqual(threadId, second.SessionId);
            StringAssert.Contains(again.ToUpperInvariant(), "ALBICOCCA");
        }

        [TestMethod]
        public async Task Ask_mde_before_running_a_command_and_respect_a_no()
        {
            RequireCodex();
            var dir = NewProject();
            var asked = new List<CodexApprovalRequest>();
            await using var session = new CodexSession(NullLogger.Instance, dir, null, new CodexSessionOptions
            {
                Sandbox = CodexSandbox.WorkspaceWrite,
                Approve = request => { lock (asked) asked.Add(request); return CodexApprovalDecision.No("prova"); },
            });
            await session.StartAsync();

            var (_, chunks) = await AskAsync(session, "Crea il file nota.txt con dentro la parola ciao. Se non puoi, dimmelo.");

            Assert.IsTrue(asked.Count > 0, "Codex ha chiesto il permesso");
            Assert.IsFalse(File.Exists(Path.Combine(dir, "nota.txt")), "il rifiuto è stato rispettato");
            Assert.IsTrue(chunks.Any(c => c.Kind == CodexChunk.KindTool && c.Text.Contains("prova")), "la chat dice perché MDE ha rifiutato");
        }

        [TestMethod]
        public async Task Stop_a_turn_and_keep_working()
        {
            RequireCodex();
            var dir = NewProject();
            await using var session = new CodexSession(NullLogger.Instance, dir, null, new CodexSessionOptions { Sandbox = CodexSandbox.ReadOnly });
            await session.StartAsync();

            using var stop = new CancellationTokenSource();
            var received = 0;
            try
            {
                await foreach (var chunk in session.PromptAsync("Scrivi un racconto di 2000 parole su un faro.", stop.Token))
                    if (chunk.Kind == CodexChunk.KindMessage && ++received == 3) stop.Cancel();
            }
            catch (OperationCanceledException) { }

            Assert.AreEqual("interrupted", session.LastTurnUsage?.Status, "Codex ha chiuso il turno come interrotto");
            var (text, _) = await AskAsync(session, "Rispondi solo OK.");
            StringAssert.Contains(text.ToUpperInvariant(), "OK");
        }
    }
}
