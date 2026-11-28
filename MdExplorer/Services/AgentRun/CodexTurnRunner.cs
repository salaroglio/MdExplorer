using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Features.Services.AI.Codex;
using MdExplorer.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Il turno di un agente su Codex (sprint 2026-11-28-Motore-Codex, F4): un <c>codex app-server</c> suo, avviato per il turno
    /// nella cartella di lavoro dell'agente (spesso un worktree) e chiuso dopo.
    /// <para>
    /// Permessi come <c>--allow-all-tools</c> di Copilot (D11 del motore unico), ma decisi da MdExplorer: Codex chiede il permesso
    /// per ogni comando e modifica (<c>approvalPolicy: "untrusted"</c>), e MdExplorer dice sì a tutto ciò che il manifesto
    /// <c>tools:</c> della scheda dichiara, no al resto e sempre no a ssh/scp. Misurato il 28/11: un comando approvato gira fuori
    /// dal sandbox (un <c>git commit</c> riesce), mentre nel solo sandbox <c>workspace-write</c> <c>.git</c> è in sola lettura.
    /// Il RunToken e l'identità git viaggiano nell'ambiente del processo, ereditato dalla shell (misurato), e nell'<c>env</c>
    /// del server MCP di MdExplorer.
    /// </para>
    /// </summary>
    public sealed class CodexTurnRunner
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;

        public CodexTurnRunner(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Le risposte alle richieste di approvazione di un agente: ciò che il manifesto non dichiara è un limite, non un'intenzione.
        /// Una richiesta senza manifesto (<c>DeclaredTools</c> null) ha tutto, come per gli altri motori.
        /// </summary>
        public static Func<CodexApprovalRequest, CodexApprovalDecision> ApproveFor(AgentTurnRequest request)
        {
            var denied = AgentToolCatalog.NativeToolsToDeny(request?.DeclaredTools);
            var noShell = denied.Contains(AgentToolCatalog.ManifestShell);
            var noWrite = denied.Contains(AgentToolCatalog.ManifestWrite);
            return approval =>
            {
                if (approval.Kind == CodexApprovalRequest.KindCommand)
                {
                    if (noShell) return CodexApprovalDecision.No(UiText.Say("la scheda dell'agente non dichiara la shell (tools: shell)",
                        "the agent's card does not declare the shell (tools: shell)"));
                    if (RemoteCopyShellRules.IsRemoteCopy(approval.Command)) return CodexApprovalDecision.No(UiText.Say(
                        "ssh/scp/sftp/rsync non si usano: i file delle radici remote passano dagli strumenti remote_* di MdExplorer",
                        "ssh/scp/sftp/rsync are not used: files of remote roots go through MdExplorer's remote_* tools"));
                }
                if (approval.Kind == CodexApprovalRequest.KindFileChange && noWrite)
                    return CodexApprovalDecision.No(UiText.Say("la scheda dell'agente non dichiara la scrittura (tools: write)",
                        "the agent's card does not declare writing (tools: write)"));
                return CodexApprovalDecision.Yes;
            };
        }

        /// <summary>Le opzioni della sessione di un agente: il server MCP di MdExplorer con l'ambiente del turno, il web, le regole.</summary>
        public static CodexSessionOptions AgentOptions(CodexMcpServer mdexplorer, AgentTurnRequest request) => new()
        {
            Sandbox = CodexSandbox.WorkspaceWrite,
            Approve = ApproveFor(request),
            McpServers = new[] { mdexplorer },
            // Come gli altri motori negli agenti (Claude: WebFetch/WebSearch fra gli strumenti concessi).
            WebSearch = true,
            Environment = request.Environment,
            ProfileKey = "agent",
        };

        public async Task<AgentTurnResult> RunTurnAsync(AgentTurnRequest request, CancellationToken ct)
        {
            string groups;
            using (var scope = _scopeFactory.CreateScope())
                groups = McpToolGroupsSettings.WithAgents(
                    MdExplorer.Service.ProjectsManager.McpGroupsArgument(scope.ServiceProvider.GetService<IUserSettingsDB>(), request.ProjectPath));

            var mdexplorer = CodexChat.MdExplorerServer(groups, request.Environment ?? new Dictionary<string, string>());
            if (mdexplorer == null)
                return AgentTurnResult.Failed(AgentTurnOutcome.ProviderError, UiText.Say(
                    $"Agente '{request.AgentName}': il server MCP di MdExplorer non si trova, e senza gli strumenti della città un agente non può lavorare. Controlla l'installazione di MdExplorer (cartella mcp).",
                    $"Agent '{request.AgentName}': MdExplorer's MCP server cannot be found, and without the city's tools an agent cannot work. Check the MdExplorer installation (mcp folder)."));

            var model = string.IsNullOrWhiteSpace(request.RequestedModel) ? null : request.RequestedModel;
            var session = new CodexSession(_loggerFactory.CreateLogger<CodexSession>(), request.WorkingDirectory ?? request.ProjectPath,
                model, AgentOptions(mdexplorer, request));
            // C4: i diagrammi che l'agente scrive si verificano a fine turno, con un turno di correzione.
            using (var verifierScope = _scopeFactory.CreateScope())
                if (verifierScope.ServiceProvider.GetService<MdExplorer.Features.Commands.PlantumlServer>() is { } plantuml)
                    session.DiagramVerifier = new MdExplorer.Features.Diagrams.PlantumlBlockVerifier(plantuml.CheckAsync);
            var answer = new StringBuilder();
            try
            {
                await session.StartAsync(ct).ConfigureAwait(false);
                await foreach (var chunk in session.PromptAsync(request.ComposedPrompt, ct).ConfigureAwait(false))
                {
                    if (chunk.Kind == CodexChunk.KindMessage) answer.Append(chunk.Text);
                    // Per il diario: tutto ciò che arriva. Per uno strumento la riga del diario (il comando, il file), non l'esito intero.
                    var text = chunk.Kind == CodexChunk.KindTool ? chunk.Diary : chunk.Text;
                    if (string.IsNullOrEmpty(text)) continue;
                    try { request.OnEvent?.Invoke(new AgentTurnEvent(chunk.Kind, text)); }
                    catch { /* chi guarda non ferma il turno */ }
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                return AgentTurnResult.Failed(AgentTurnOutcome.ProviderError, $"Codex ({model ?? session.EffectiveModel ?? "predefinito"}): {ex.Message}", answer.ToString());
            }
            finally
            {
                await session.DisposeAsync().ConfigureAwait(false);
            }
            if (session.LastTurnUsage?.Status == "interrupted")
                return AgentTurnResult.Failed(AgentTurnOutcome.Cancelled, UiText.Say($"Agente '{request.AgentName}': Codex ha interrotto il turno.",
                    $"Agent '{request.AgentName}': Codex interrupted the turn."), answer.ToString());
            return AgentTurnResult.Completed(answer.ToString());
        }
    }
}
