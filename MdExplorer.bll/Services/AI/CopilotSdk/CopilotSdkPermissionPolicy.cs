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
