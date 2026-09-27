using System.Linq;
using System;
using System.IO;
using GitHub.Copilot;

namespace MdExplorer.Features.Services.AI.CopilotSdk
{
    /// <summary>
    /// What the chat lets Copilot do without asking, on the SDK transport.
    ///
    /// <para>
    /// The SDK approves NOTHING on its own: a session without a permission handler has every tool
    /// call refused, and Copilot answers "I cannot read the file, access was denied" — measured
    /// 10/09/2026 on a file inside the project. The ACP transport never met the problem because
    /// the CLI was started with <c>--allow-all-tools</c> and approved by itself.
    /// </para>
    ///
    /// <para>
    /// The policy reproduces that flag, and deliberately goes no further: tools run freely, but
    /// reading and writing stay inside the project, as with <c>--allow-all-tools</c>, which does
    /// NOT disable the CLI's path check (that is <c>--allow-all-paths</c>). URLs and extension
    /// management are refused: under ACP they would have been asked for, left unanswered, and the
    /// turn would have hung — refusing with a reason is strictly better than that, and granting
    /// them would widen what the chat could do compared to before. Changing the security posture
    /// is a decision for its own sake, not a side effect of changing transport.
    /// </para>
    /// </summary>
    public static class CopilotSdkPermissionPolicy
    {
        public readonly struct Verdict
        {
            public Verdict(bool approved, string what, string reason)
            {
                Approved = approved; What = what; Reason = reason;
            }

            public bool Approved { get; }

            /// <summary>What was asked, in words fit for a log line.</summary>
            public string What { get; }

            /// <summary>Why it was refused; null when approved.</summary>
            public string Reason { get; }
        }

        /// <summary>
        /// The same policy, with the restrictions of an e2e test session on top (F4c): no shell (D29)
        /// and no reading of the credentials and secrets files. Everything else as <see cref="Decide(PermissionRequest, string)"/>.
        /// </summary>
        public static Verdict Decide(PermissionRequest request, string workingDirectory, CopilotChat.CopilotSessionProfile profile)
        {
            if (profile != null)
            {
                if (profile.DenyShell && request is PermissionRequestShell shell)
                    return new Verdict(false, "eseguire: " + shell.FullCommandText,
                        "durante i test e2e la shell è disattivata, per proteggere le credenziali");
                if (request is PermissionRequestRead read && IsDenied(read.Path, workingDirectory, profile))
                    return new Verdict(false, "leggere " + read.Path,
                        "è un file di credenziali: durante i test e2e le usa solo il server Playwright");
                if (request is PermissionRequestMcp tool && profile.DeniedMcpTools.Any(d =>
                        string.Equals(d, tool.ToolName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(d, tool.ServerName + "/" + tool.ToolName, StringComparison.OrdinalIgnoreCase)))
                    return new Verdict(false, $"usare lo strumento MCP {tool.ServerName}/{tool.ToolName}",
                        "durante i test e2e questo strumento è vietato: può leggere le credenziali aggirando i divieti");
                if (request is PermissionRequestWrite write && IsProtected(write.FileName, workingDirectory, profile))
                    return new Verdict(false, "scrivere " + write.FileName,
                        "durante i test e2e la configurazione degli agenti non si modifica");
            }
            return Decide(request, workingDirectory);
        }

        private static string Full(string path, string workingDirectory) =>
            Path.GetFullPath(Path.IsPathRooted(path) || string.IsNullOrWhiteSpace(workingDirectory) ? path : Path.Combine(workingDirectory, path));

        private static bool IsDenied(string path, string workingDirectory, CopilotChat.CopilotSessionProfile profile)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var full = Full(path, workingDirectory);
            if (profile.DeniedReadPaths.Any(d => string.Equals(full, Path.GetFullPath(d), StringComparison.OrdinalIgnoreCase))) return true;
            var name = Path.GetFileName(full);
            return profile.DeniedReadNames.Any(pattern => System.Text.RegularExpressions.Regex.IsMatch(name,
                "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        }

        private static bool IsProtected(string path, string workingDirectory, CopilotChat.CopilotSessionProfile profile)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(workingDirectory)) return false;
            var relative = Path.GetRelativePath(Path.GetFullPath(workingDirectory), Full(path, workingDirectory)).Replace('\\', '/');
            var name = Path.GetFileName(relative);
            return profile.DeniedWritePaths.Any(p => p.EndsWith("/", StringComparison.Ordinal)
                ? relative.StartsWith(p, StringComparison.OrdinalIgnoreCase)
                // A file entry protects that name wherever it is: CLAUDE.md, AGENTS.md are read in every folder.
                : string.Equals(relative, p, StringComparison.OrdinalIgnoreCase) || string.Equals(name, p, StringComparison.OrdinalIgnoreCase));
        }

        public static Verdict Decide(PermissionRequest request, string workingDirectory)
        {
            switch (request)
            {
                case PermissionRequestRead read:
                    return InsideProject(read.Path, workingDirectory, "leggere");

                case PermissionRequestWrite write:
                    return InsideProject(write.FileName, workingDirectory, "scrivere");

                case PermissionRequestShell shell:
                    // --allow-all-tools approved shell commands, and the ACP chat relied on it.
                    return new Verdict(true, "eseguire: " + shell.FullCommandText, null);

                case PermissionRequestMcp mcp:
                    return new Verdict(true, $"usare lo strumento MCP {mcp.ServerName}/{mcp.ToolName}", null);

                case PermissionRequestCustomTool tool:
                    return new Verdict(true, "usare lo strumento " + tool.ToolName, null);

                case PermissionRequestMemory memory:
                    return new Verdict(true, "usare la memoria dell'agente", null);

                case PermissionRequestUrl url:
                    return new Verdict(false, "aprire " + url.Url,
                        "la chat di MDE non autorizza l'accesso a indirizzi web");

                default:
                    return new Verdict(false, "richiesta di tipo " + (request?.Kind ?? "sconosciuto"),
                        "la chat di MDE non autorizza questo tipo di richiesta");
            }
        }

        /// <summary>
        /// Same containment rule as the include commands: the path is resolved against the
        /// project, and anything landing outside it is refused. The trailing separator keeps a
        /// sibling folder whose name merely starts like the project's out.
        /// </summary>
        private static Verdict InsideProject(string path, string workingDirectory, string verb)
        {
            var what = $"{verb} {path}";

            if (string.IsNullOrWhiteSpace(path))
                return new Verdict(false, what, "percorso vuoto");
            if (string.IsNullOrWhiteSpace(workingDirectory))
                return new Verdict(false, what, "la sessione non ha una cartella di progetto");

            var root = Path.GetFullPath(workingDirectory);
            var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
            var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));

            var inside = full.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
                         || string.Equals(full, root, StringComparison.OrdinalIgnoreCase);

            return inside
                ? new Verdict(true, what, null)
                : new Verdict(false, what, "è fuori dalla cartella del progetto");
        }
    }
}
