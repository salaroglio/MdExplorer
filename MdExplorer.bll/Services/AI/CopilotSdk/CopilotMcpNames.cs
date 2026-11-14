using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MdExplorer.Features.Services.AI.CopilotSdk
{
    /// <summary>
    /// I nomi dei server MCP che il CLI di Copilot avvia in una sessione, per spegnerli per nome
    /// (<c>SessionConfig.DisabledMcpServers</c> vuole i nomi, non la regola «tutti tranne»). Da tre fonti: il server
    /// integrato di GitHub, i file di configurazione che il CLI legge (<c>~/.copilot/mcp-config.json</c>,
    /// <c>.vscode/mcp.json</c> del progetto) e quelli che le sessioni precedenti hanno dichiarato di aver caricato
    /// (evento <c>SessionMcpServersLoadedEvent</c>), così un server che sta in un posto non previsto si spegne dalla
    /// sessione dopo.
    /// </summary>
    public static class CopilotMcpNames
    {
        public const string BuiltInGitHub = "github-mcp-server";

        private static readonly ConcurrentDictionary<string, byte> Seen = new(StringComparer.Ordinal);

        public static void Learn(string name)
        {
            if (!string.IsNullOrWhiteSpace(name)) Seen.TryAdd(name, 0);
        }

        /// <summary>I nomi conosciuti per una sessione che lavora in <paramref name="workingDirectory"/>; <paramref name="problems"/>: i file che non si leggono.</summary>
        public static List<string> Known(string workingDirectory, out List<string> problems)
        {
            problems = new List<string>();
            var names = new HashSet<string>(StringComparer.Ordinal) { BuiltInGitHub };
            foreach (var n in Seen.Keys) names.Add(n);
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            AddFrom(Path.Combine(home, ".copilot", "mcp-config.json"), "mcpServers", names, problems);
            if (!string.IsNullOrWhiteSpace(workingDirectory))
                AddFrom(Path.Combine(workingDirectory, ".vscode", "mcp.json"), "servers", names, problems);
            return names.OrderBy(n => n, StringComparer.Ordinal).ToList();
        }

        private static void AddFrom(string file, string property, HashSet<string> names, List<string> problems)
        {
            if (!File.Exists(file)) return;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(file), new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(property, out var servers) && servers.ValueKind == JsonValueKind.Object)
                    foreach (var s in servers.EnumerateObject()) names.Add(s.Name);
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                problems.Add($"{file}: {ex.Message}");
            }
        }
    }
}
