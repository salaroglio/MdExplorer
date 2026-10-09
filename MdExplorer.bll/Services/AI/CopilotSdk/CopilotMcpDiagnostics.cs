using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.AI.CopilotSdk
{
    /// <summary>
    /// Whether the chat must tell the user that Copilot is working WITHOUT MdExplorer's MCP server.
    ///
    /// <para>
    /// The server reaches Copilot only through the user-level <c>.copilot/mcp-config.json</c>, which
    /// MdExplorer writes when a project is opened; the SDK session reads it (measured 11/09/2026).
    /// When that fails — a stale path to the exe, a port nobody answers on — Copilot simply has
    /// fewer tools and nobody says so: the user sees an agent that "does not use" MdExplorer's
    /// document search or diagram check, and cannot tell why. The SDK reports every server's state
    /// in <c>session.mcp_servers_loaded</c>; this turns it into one sentence.
    /// </para>
    /// </summary>
    internal static class CopilotMcpDiagnostics
    {
        /// <summary>The key ProjectsManager writes in mcp-config.json.</summary>
        public const string MdExplorerServer = "mdexplorer";

        /// <param name="servers">Name, status value (<c>connected</c>, <c>failed</c>, …) and error of each server.</param>
        /// <returns>Null when the server is connected or still connecting; otherwise what to tell the user.</returns>
        public static string NoticeFor(IReadOnlyCollection<(string Name, string Status, string Error)> servers)
        {
            if (servers == null) return null;

            var ours = servers.FirstOrDefault(s => string.Equals(s.Name, MdExplorerServer, StringComparison.OrdinalIgnoreCase));
            const string consequence =
                " Copilot non può usare gli strumenti di MdExplorer (ricerca nei documenti, agenti…)." +
                " I diagrammi PlantUML che scrive li verifica comunque MdExplorer.";

            if (ours.Name == null)
            {
                return "⚠️ Il server MCP di MdExplorer non è nella configurazione di Copilot" +
                       " (.copilot/mcp-config.json nella cartella utente, scritta da MdExplorer all'apertura di un progetto)." +
                       consequence;
            }

            var status = ours.Status ?? string.Empty;
            if (status == "connected" || status == "pending") return null;

            return $"⚠️ Il server MCP di MdExplorer non è connesso a Copilot (stato: {status}" +
                   (string.IsNullOrWhiteSpace(ours.Error) ? ")." : $" — {ours.Error.Trim()}).") +
                   consequence;
        }
    }
}
