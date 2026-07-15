using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using A2A;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.Agents;
using MdExplorer.Abstractions.Services;
using MdExplorer.Services.AgentRegistry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Gateway A2A JSON-RPC (§7): controller catch-all fatto a mano su
    /// <c>/a2a/{projectKey}/{agentName}</c>, coi tipi del package <c>A2A</c> per il
    /// wire format. <b>Fase 2</b>: accetta <c>message/send</c> verso i soli agenti
    /// <b>algoritmici</b> (niente LLM = niente costi mentre il protocollo si stabilizza).
    /// Il ciclo di vita del task algoritmico è in-process: submitted → working →
    /// completed/failed, restituito sincrono. Loopback-only + guardia Host/Origin (R12).
    /// </summary>
    [ApiController]
    public class A2AGatewayController : ControllerBase
    {
        private readonly IAgentRegistryService _registry;
        private readonly IUserSettingsDB _session;
        private readonly IEnumerable<IAlgorithmicAgent> _algorithmicAgents;
        private readonly ILogger<A2AGatewayController> _logger;

        private const int OutputSummaryMax = 2000;

        public A2AGatewayController(
            IAgentRegistryService registry,
            IUserSettingsDB session,
            IEnumerable<IAlgorithmicAgent> algorithmicAgents,
            ILogger<A2AGatewayController> logger)
        {
            _registry = registry;
            _session = session;
            _algorithmicAgents = algorithmicAgents;
            _logger = logger;
        }

        [HttpPost("/a2a/{projectKey}/{agentName}")]
        public async Task<IActionResult> Rpc(string projectKey, string agentName, [FromBody] JsonElement body, CancellationToken ct)
        {
            // --- envelope JSON-RPC 2.0 ---
            JsonNode idNode = body.TryGetProperty("id", out var idEl)
                ? JsonNode.Parse(idEl.GetRawText())
                : null;
            var method = body.TryGetProperty("method", out var mEl) && mEl.ValueKind == JsonValueKind.String
                ? mEl.GetString()
                : null;

            if (method != "message/send")
                return JsonRpc(idNode, error: (-32601, $"Metodo '{method}' non supportato in Fase 2 (solo 'message/send')."));

            if (!body.TryGetProperty("params", out var paramsEl) || paramsEl.ValueKind != JsonValueKind.Object)
                return JsonRpc(idNode, error: (-32602, "params mancante o non valido."));

            // --- risoluzione progetto (projectKey = Guid del Project) ---
            if (!Guid.TryParse(projectKey, out var pk))
                return JsonRpc(idNode, error: (-32602, "projectKey non è un Guid valido."));
            var projectPath = ResolveProjectPath(pk);
            if (projectPath == null)
                return JsonRpc(idNode, error: (-32001, $"Progetto {projectKey} non trovato."));

            // --- risoluzione destinatario RI-VALIDANDO dalle fonti (§7): cittadino,
            //     trusted, e algoritmico (Fase 2 non sveglia agenti LLM) ---
            var entry = _registry.RefreshCatalog(projectPath)
                .FirstOrDefault(e => e.IsCitizen &&
                    string.Equals(e.Name, agentName, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
                return JsonRpc(idNode, error: (-32001, $"Agente '{agentName}' non trovato o non cittadino."));
            if (!entry.Trusted)
                return JsonRpc(idNode, error: (-32002, $"Agente '{agentName}' non è trusted: conferma il trust prima di inviargli messaggi."));
            if (!string.Equals(entry.Kind, AgentIdentity.KindEnum.Algorithmic, StringComparison.OrdinalIgnoreCase))
                return JsonRpc(idNode, error: (-32003, $"Agente '{agentName}' è LLM: in Fase 2 il gateway sveglia solo agenti algoritmici."));

            var agent = _algorithmicAgents?
                .FirstOrDefault(a => string.Equals(SafeName(a), agentName, StringComparison.OrdinalIgnoreCase));
            if (agent == null)
                return JsonRpc(idNode, error: (-32001, $"Implementazione algoritmica di '{agentName}' non registrata."));

            // --- estrazione messaggio + contesto ---
            var (text, contextId, fromAgent, topics) = ReadMessage(paramsEl);
            contextId ??= Guid.NewGuid().ToString();
            var taskId = Guid.NewGuid().ToString();

            var startedAt = DateTime.UtcNow;
            AgentTaskResult result;
            try
            {
                var context = new AgentTaskContext
                {
                    ProjectPath = projectPath,
                    ConversationId = contextId,
                    A2ATaskId = taskId,
                    FromAgent = fromAgent,
                    Message = text,
                    Topics = topics,
                };
                result = await agent.ExecuteAsync(context, ct) ?? AgentTaskResult.Fail("L'agente non ha prodotto alcun risultato.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[A2A] Esecuzione algoritmica di '{Agent}' fallita", agentName);
                result = AgentTaskResult.Fail($"Eccezione durante l'esecuzione: {ex.Message}");
            }

            LogExecution(projectPath, agentName, startedAt, result);

            var task = BuildTask(taskId, contextId, result);
            var taskNode = JsonSerializer.SerializeToNode(task, A2AJsonUtilities.DefaultOptions);
            return JsonRpc(idNode, resultNode: taskNode);
        }

        // ---------------------------------------------------------------------

        private string ResolveProjectPath(Guid projectKey)
        {
            _session.BeginTransaction();
            var project = _session.GetDal<Project>().GetList().ToList()
                .FirstOrDefault(p => p.Id == projectKey);
            _session.Commit();
            return string.IsNullOrWhiteSpace(project?.Path) ? null : project.Path;
        }

        private static (string text, string contextId, string fromAgent, IList<string> topics) ReadMessage(JsonElement paramsEl)
        {
            var sb = new StringBuilder();
            string contextId = null, fromAgent = null;
            var topics = new List<string>();

            if (paramsEl.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.Object)
            {
                if (msg.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                    foreach (var part in parts.EnumerateArray())
                        if (part.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                            sb.Append(t.GetString());

                if (msg.TryGetProperty("contextId", out var c) && c.ValueKind == JsonValueKind.String)
                    contextId = c.GetString();

                // Mittente e topics dichiarati (§8): metadata non-standard, best-effort.
                if (msg.TryGetProperty("metadata", out var meta) && meta.ValueKind == JsonValueKind.Object)
                {
                    if (meta.TryGetProperty("fromAgent", out var f) && f.ValueKind == JsonValueKind.String)
                        fromAgent = f.GetString();
                    if (meta.TryGetProperty("topics", out var tp) && tp.ValueKind == JsonValueKind.Array)
                        foreach (var el in tp.EnumerateArray())
                            if (el.ValueKind == JsonValueKind.String) topics.Add(el.GetString());
                }
            }
            return (sb.ToString(), contextId, fromAgent, topics);
        }

        private static AgentTask BuildTask(string taskId, string contextId, AgentTaskResult result)
        {
            var payload = result.Success ? (result.Output ?? string.Empty) : (result.Error ?? "errore");
            return new AgentTask
            {
                Id = taskId,
                ContextId = contextId,
                Status = new global::A2A.TaskStatus
                {
                    State = result.Success ? TaskState.Completed : TaskState.Failed,
                    Timestamp = DateTimeOffset.UtcNow,
                    Message = new Message
                    {
                        Role = Role.Agent,
                        MessageId = Guid.NewGuid().ToString(),
                        ContextId = contextId,
                        TaskId = taskId,
                        Parts = new List<Part> { Part.FromText(payload) },
                    },
                },
            };
        }

        private void LogExecution(string projectPath, string agentName, DateTime startedAt, AgentTaskResult result)
        {
            try
            {
                _session.BeginTransaction();
                _session.GetDal<AgentExecutionLog>().Save(new AgentExecutionLog
                {
                    // Id non pre-assegnato (GuidComb).
                    ProjectPath = projectPath,
                    AgentFilePath = $"(algorithmic:{agentName})", // colonna NotNullable: gli algoritmici non hanno file
                    TriggerSource = "message",                    // nuovo valore ammesso (§8)
                    ExecutedBy = "a2a",
                    StartedAt = startedAt,
                    FinishedAt = DateTime.UtcNow,
                    Status = result.Success ? "success" : "error",
                    OutputSummary = result.Success ? Truncate(result.Output) : null,
                    Error = result.Success ? null : result.Error,
                });
                _session.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[A2A] Scrittura AgentExecutionLog fallita per '{Agent}'", agentName);
            }
        }

        private static string SafeName(IAlgorithmicAgent a)
        {
            try { return a.GetCard()?.Name; }
            catch { return null; }
        }

        private static string Truncate(string s)
            => string.IsNullOrEmpty(s) || s.Length <= OutputSummaryMax ? s : s.Substring(0, OutputSummaryMax);

        private IActionResult JsonRpc(JsonNode idNode, JsonNode resultNode = null, (int code, string message)? error = null)
        {
            var resp = new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = idNode?.DeepClone(),
            };
            if (error.HasValue)
                resp["error"] = new JsonObject { ["code"] = error.Value.code, ["message"] = error.Value.message };
            else
                resp["result"] = resultNode;

            return Content(resp.ToJsonString(), "application/json");
        }
    }
}
