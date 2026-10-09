using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Copilot;
using MdExplorer.Features.Services.AI.ClaudeCode;
using MdExplorer.Features.Services.AI.CopilotAcp;
using MdExplorer.Features.Services.AI.OpenCode;

namespace MdExplorer.Features.Services.AI.AgenticEnvironments
{
    /// <summary>Gli id degli ambienti: gli stessi di <c>harness.target</c> nel <c>.development.yml</c>.</summary>
    public static class AgenticEnvironmentIds
    {
        public const string Copilot = "copilot";
        public const string Claude = "claude";
        public const string OpenCode = "opencode";
    }

    /// <summary>
    /// Claude Code: <c>claude auth status</c> risponde in JSON con il campo <c>loggedIn</c>
    /// (misurato il 02/10/2026 su 2.1.287: 144 ms, nessun turno speso).
    /// </summary>
    public sealed class ClaudeCodeEnvironmentCheck : IAgenticEnvironmentCheck
    {
        public string Id => AgenticEnvironmentIds.Claude;

        public string ResolvePath() => ClaudeCodeProcessLauncher.ResolvePath();

        public async Task<AgenticEnvironmentUsability> CheckUsableAsync(CancellationToken ct)
        {
            var result = await CliCommand.RunAsync(ResolvePath(), new[] { "auth", "status" }, ct).ConfigureAwait(false);
            return Interpret(result.ExitCode, result.Stdout, result.Stderr);
        }

        /// <summary>Legge la risposta di <c>claude auth status</c>. Pura: si prova senza il CLI.</summary>
        public static AgenticEnvironmentUsability Interpret(int exitCode, string stdout, string stderr)
        {
            var text = (stdout ?? string.Empty).Trim();
            try
            {
                using var document = JsonDocument.Parse(text);
                if (document.RootElement.ValueKind == JsonValueKind.Object &&
                    document.RootElement.TryGetProperty("loggedIn", out var loggedIn) &&
                    (loggedIn.ValueKind == JsonValueKind.True || loggedIn.ValueKind == JsonValueKind.False))
                {
                    return loggedIn.GetBoolean()
                        ? AgenticEnvironmentUsability.Ok("Accesso fatto.")
                        : AgenticEnvironmentUsability.No(AgenticEnvironmentReasons.NotLoggedIn,
                            "Claude Code è installato ma l'accesso non è stato fatto: lancia `claude` e fai il login.");
                }
            }
            catch (JsonException)
            {
                // Non è JSON: lo si dice qui sotto, con ciò che il CLI ha scritto.
            }

            // Una versione che non conosce `auth status`, o una risposta di forma nuova: non si
            // indovina se l'accesso c'è. Si riporta ciò che si è letto.
            return AgenticEnvironmentUsability.No(AgenticEnvironmentReasons.Error,
                $"`claude auth status` non ha risposto con il campo loggedIn (exit {exitCode}): " +
                $"{FirstLine(string.IsNullOrWhiteSpace(stderr) ? stdout : stderr)}. Aggiorna Claude Code e riprova.");
        }

        internal static string FirstLine(string text)
        {
            var line = (text ?? string.Empty).Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0);
            if (line == null) return "nessuna uscita";
            return line.Length > 200 ? line.Substring(0, 200) + "…" : line;
        }
    }

    /// <summary>
    /// opencode: <c>opencode models</c> deve elencare almeno un modello. «Accesso fatto» qui non è il
    /// criterio giusto: misurato il 02/10/2026 su 1.18.30, con <b>zero</b> credenziali
    /// (<c>opencode auth list</c>) il CLI elenca comunque otto modelli gratuiti e la chat funziona.
    /// </summary>
    public sealed class OpenCodeEnvironmentCheck : IAgenticEnvironmentCheck
    {
        // Una riga «fornitore/modello», senza spazi: tutto il resto (avvisi, intestazioni) non conta.
        private static readonly Regex ModelLine = new Regex(@"^[A-Za-z0-9._-]+/\S+$", RegexOptions.Compiled);
        private static readonly Regex Ansi = new Regex(@"\x1B\[[0-9;]*[A-Za-z]", RegexOptions.Compiled);

        public string Id => AgenticEnvironmentIds.OpenCode;

        public string ResolvePath() => OpenCodeProcessLauncher.ResolvePath();

        public async Task<AgenticEnvironmentUsability> CheckUsableAsync(CancellationToken ct)
        {
            var result = await CliCommand.RunAsync(ResolvePath(), new[] { "models" }, ct).ConfigureAwait(false);
            return Interpret(result.ExitCode, result.Stdout, result.Stderr);
        }

        /// <summary>Legge la risposta di <c>opencode models</c>. Pura: si prova senza il CLI.</summary>
        public static AgenticEnvironmentUsability Interpret(int exitCode, string stdout, string stderr)
        {
            var models = Ansi.Replace(stdout ?? string.Empty, string.Empty)
                .Split('\n')
                .Select(line => line.Trim())
                .Count(line => ModelLine.IsMatch(line));

            if (exitCode != 0)
            {
                return AgenticEnvironmentUsability.No(AgenticEnvironmentReasons.Error,
                    $"`opencode models` è uscito con codice {exitCode}: " +
                    $"{ClaudeCodeEnvironmentCheck.FirstLine(string.IsNullOrWhiteSpace(stderr) ? stdout : stderr)}.");
            }

            return models > 0
                ? AgenticEnvironmentUsability.Ok(models == 1 ? "1 modello disponibile." : $"{models} modelli disponibili.")
                : AgenticEnvironmentUsability.No(AgenticEnvironmentReasons.NoModels,
                    "opencode è installato ma non elenca nessun modello: lancia `opencode auth login` e collega un fornitore.");
        }
    }

    /// <summary>
    /// GitHub Copilot: il CLI non ha un comando di stato (misurato il 02/10/2026 su 1.0.89: solo
    /// <c>login</c>). Lo stato dell'accesso lo dà l'SDK che MdExplorer usa già per la chat, con il
    /// CLI dell'utente e la sua autenticazione: avvio ~0,5 s, risposta ~0,5 s.
    /// </summary>
    public sealed class CopilotEnvironmentCheck : IAgenticEnvironmentCheck
    {
        public string Id => AgenticEnvironmentIds.Copilot;

        public string ResolvePath()
        {
            if (!CopilotProcessLauncher.IsResolvable()) return null;
            // Su Windows lo shim passa da cmd.exe o PowerShell: il file del CLI è l'ultimo
            // argomento del prefisso, ed è quello che dice «dov'è Copilot».
            var (path, prefixArgs) = CopilotProcessLauncher.ResolveStdioTarget();
            return prefixArgs != null && prefixArgs.Count > 0 ? prefixArgs[prefixArgs.Count - 1] : path;
        }

        public async Task<AgenticEnvironmentUsability> CheckUsableAsync(CancellationToken ct)
        {
            var (cliPath, prefixArgs) = CopilotProcessLauncher.ResolveStdioTarget();

            await using var client = new CopilotClient(new CopilotClientOptions
            {
                Connection = RuntimeConnection.ForStdio(cliPath, prefixArgs),
                UseLoggedInUser = true,
            });

            await client.StartAsync(ct).ConfigureAwait(false);
            try
            {
                var status = await client.GetAuthStatusAsync(ct).ConfigureAwait(false);
                return status.IsAuthenticated
                    ? AgenticEnvironmentUsability.Ok("Accesso fatto.")
                    : AgenticEnvironmentUsability.No(AgenticEnvironmentReasons.NotLoggedIn,
                        "Copilot CLI è installato ma l'accesso non è stato fatto: lancia `copilot login`" +
                        (string.IsNullOrWhiteSpace(status.StatusMessage) ? "." : $" ({status.StatusMessage})."));
            }
            finally
            {
                await client.StopAsync().ConfigureAwait(false);
            }
        }
    }
}
