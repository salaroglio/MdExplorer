using System;
using System.IO;
using System.Text.Json;

namespace MdExplorer.Features.Services.AI.CopilotSdk
{
    /// <summary>
    /// Turns what Copilot is doing into a short line for the chat — "Legge docs/nota.md",
    /// "Esegue: ls -la" — shown apart from the answer while it works.
    ///
    /// <para>
    /// This is what the chat was missing: the ACP reader understood two kinds of notification and
    /// dropped the rest, so while Copilot read files or ran a sub-agent the chat sat still, with
    /// nothing to tell "working" from "stuck". The SDK reports every step; this only makes them
    /// readable.
    /// </para>
    ///
    /// <para>
    /// Pure and deterministic on purpose: the tool name and its arguments come in, a sentence goes
    /// out. Unknown tools still get a line ("Usa lo strumento X") rather than silence — an activity
    /// with a clumsy label is still better than a chat that looks frozen.
    /// </para>
    /// </summary>
    public static class CopilotSdkActivityDescriber
    {
        private const int MaxCommandLength = 90;

        public static string DescribeTool(
            string toolName,
            JsonElement? arguments,
            string mcpServerName,
            string mcpToolName,
            string workingDirectory)
        {
            if (!string.IsNullOrWhiteSpace(mcpServerName))
            {
                return $"Usa {mcpServerName}/{mcpToolName ?? toolName}";
            }

            var path = Argument(arguments, "path") ?? Argument(arguments, "file_path") ?? Argument(arguments, "filePath");

            switch ((toolName ?? string.Empty).ToLowerInvariant())
            {
                case "view":
                case "read":
                case "read_file":
                    return path != null ? "Legge " + Relative(path, workingDirectory) : "Legge un file";

                case "glob":
                    return "Cerca file " + (Argument(arguments, "pattern") ?? string.Empty);

                case "grep":
                case "rg":
                case "search":
                    return "Cerca «" + (Argument(arguments, "pattern") ?? Argument(arguments, "query") ?? string.Empty) + "»";

                case "bash":
                case "shell":
                case "powershell":
                case "pwsh":
                    // From the stable arguments, not ShellToolInfo.DisplayCommand: that field is
                    // marked experimental by the SDK (GHCP001), and the command is here anyway.
                    return "Esegue: " + Truncate(Argument(arguments, "command") ?? string.Empty);

                case "edit":
                case "str_replace":
                case "str_replace_editor":
                    return path != null ? "Modifica " + Relative(path, workingDirectory) : "Modifica un file";

                case "create":
                case "write":
                case "write_file":
                    return path != null ? "Crea " + Relative(path, workingDirectory) : "Crea un file";

                default:
                    return "Usa lo strumento " + (string.IsNullOrWhiteSpace(toolName) ? "senza nome" : toolName);
            }
        }

        public static string DescribeSubagentStarted(string displayName, string description)
            => string.IsNullOrWhiteSpace(description)
                ? $"Sub-agente {displayName} al lavoro"
                : $"Sub-agente {displayName} al lavoro: {description}";

        public static string DescribeSubagentFailed(string displayName, string error)
            => $"Sub-agente {displayName} fallito" + (string.IsNullOrWhiteSpace(error) ? string.Empty : ": " + error);

        private static string Argument(JsonElement? arguments, string name)
        {
            if (arguments is not { ValueKind: JsonValueKind.Object } args) return null;
            return args.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        /// <summary>A path inside the project reads better relative to it; outside, it stays as it is.</summary>
        private static string Relative(string path, string workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(workingDirectory) || !Path.IsPathRooted(path)) return path;
            var relative = Path.GetRelativePath(workingDirectory, path);
            return relative.StartsWith("..", StringComparison.Ordinal) ? path : relative;
        }

        private static string Truncate(string text)
            => text.Length <= MaxCommandLength ? text : text.Substring(0, MaxCommandLength) + "…";
    }
}
