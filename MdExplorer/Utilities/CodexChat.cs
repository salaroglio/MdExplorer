using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Features.Services.AI.Codex;
using MdExplorer.Service;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Le opzioni delle sessioni Codex di MdExplorer, in un posto solo: il server MCP <c>mdexplorer</c> coi gruppi del
    /// progetto, le regole con cui MdExplorer risponde alle richieste di approvazione, il web. Gemello di
    /// <see cref="ClaudeCodeMcp.ChatOptions"/>.
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F2 (D5, D6, D7).</para>
    /// </summary>
    public static class CodexChat
    {
        /// <summary>
        /// Il server MCP di MdExplorer per una sessione Codex, o null (e lo si dice a log) quando l'eseguibile non c'è.
        /// </summary>
        /// <param name="mcpGroupsArgument">I gruppi del progetto (<c>--groups</c>), null = tutti.</param>
        /// <param name="environment">Valori d'ambiente del server: il RunToken e l'identità git di un agente.</param>
        public static CodexMcpServer MdExplorerServer(string mcpGroupsArgument, IReadOnlyDictionary<string, string> environment = null,
            IReadOnlyList<string> disabledTools = null)
        {
            var executable = ProjectsManager.ResolveMcpExecutable(AppDomain.CurrentDomain.BaseDirectory);
            if (executable == null)
            {
                Console.WriteLine("[CodexChat] MdExplorer.Mcp non trovato: la sessione Codex parte senza gli strumenti di MdExplorer.");
                return null;
            }
            return new CodexMcpServer
            {
                Name = ClaudeCodeMcp.ServerName,
                Command = executable,
                Args = string.IsNullOrWhiteSpace(mcpGroupsArgument) ? Array.Empty<string>() : new[] { "--groups", mcpGroupsArgument },
                Env = environment,
                DisabledTools = disabledTools,
            };
        }

        /// <summary>
        /// La chat del tab MarkAgent: scrive nella cartella del progetto (<c>workspace-write</c>), ogni comando e modifica
        /// passano dalle regole di <see cref="ApproveForChat"/>, il web segue la rotella del progetto.
        /// </summary>
        public static CodexSessionOptions ChatOptions(string mcpGroupsArgument, IReadOnlyList<string> codeRoots = null, bool webAccess = false)
        {
            var server = MdExplorerServer(mcpGroupsArgument);
            var roots = codeRoots ?? Array.Empty<string>();
            return new CodexSessionOptions
            {
                Sandbox = CodexSandbox.WorkspaceWrite,
                Approve = ApproveForChat(roots),
                McpServers = server == null ? Array.Empty<CodexMcpServer>() : new[] { server },
                WebSearch = webAccess,
                McpGroups = mcpGroupsArgument ?? string.Empty,
                ExtraRootsKey = string.Join("\n", roots),
            };
        }

        /// <summary>
        /// Le regole della chat per le richieste di approvazione di Codex:
        /// <list type="bullet">
        /// <item><description>niente <c>ssh</c>/<c>scp</c>/<c>sftp</c>/<c>rsync</c>: i file delle radici remote passano da MDE Remote, che
        /// tiene la codifica (come per gli altri motori, <see cref="RemoteCopyShellRules"/>);</description></item>
        /// <item><description>niente modifiche native dentro una radice di codice collegata: si cambiano con <c>remote_edit</c>/<c>remote_write</c>,
        /// che passano dal cancello di MDE e ne tengono l'originale per la revisione (sprint 2026-11-27, D4). Il sandbox
        /// <c>workspace-write</c> le negherebbe comunque, perché le radici stanno fuori dalla cartella del progetto;</description></item>
        /// <item><description>il resto sì: è la parità con gli altri motori nella chat.</description></item>
        /// </list>
        /// </summary>
        public static Func<CodexApprovalRequest, CodexApprovalDecision> ApproveForChat(IReadOnlyList<string> codeRoots)
        {
            var roots = (codeRoots ?? Array.Empty<string>()).Where(r => !string.IsNullOrWhiteSpace(r)).Select(FullDirectory).ToList();
            return request =>
            {
                if (request.Kind == CodexApprovalRequest.KindCommand && RemoteCopyShellRules.IsRemoteCopy(request.Command))
                    return CodexApprovalDecision.No(UiText.Say(
                        "ssh/scp/sftp/rsync non si usano: i file delle radici remote si leggono e si cambiano con gli strumenti remote_* di MdExplorer.",
                        "ssh/scp/sftp/rsync are not used: files of remote roots are read and changed with MdExplorer's remote_* tools."));
                if (request.Kind == CodexApprovalRequest.KindFileChange)
                {
                    var inRoot = request.Paths.FirstOrDefault(p => roots.Any(r => IsInside(p, r)));
                    if (inRoot != null)
                        return CodexApprovalDecision.No(UiText.Say(
                            $"{inRoot} sta in una radice di codice collegata: si cambia con remote_edit o remote_write, che ne tengono l'originale per la revisione.",
                            $"{inRoot} is in a linked code root: change it with remote_edit or remote_write, which keep its original for the review."));
                }
                return CodexApprovalDecision.Yes;
            };
        }

        private static string FullDirectory(string path)
        {
            var full = Path.GetFullPath(path);
            return full.EndsWith(Path.DirectorySeparatorChar) ? full : full + Path.DirectorySeparatorChar;
        }

        private static bool IsInside(string path, string directory)
        {
            try
            {
                return Path.GetFullPath(path).StartsWith(directory, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
