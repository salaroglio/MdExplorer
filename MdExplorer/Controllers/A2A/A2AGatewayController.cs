using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using A2A;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.AgentRun;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Gateway A2A JSON-RPC (§7): controller catch-all fatto a mano su
    /// <c>/a2a/{projectKey}/{agentName}</c>, coi tipi del package <c>A2A</c> per il wire
    /// format. <c>message/send</c> <b>accoda</b> nella mailbox (§8) applicando i guardrail,
    /// e risponde con un task <c>submitted</c>: la consegna è asincrona, la fa il dispatcher.
    /// <b>Fase 3 step 3</b>: destinatari <b>algoritmici</b> soltanto (risveglio LLM nello
    /// step successivo). Loopback-only + guardia Host/Origin (R12).
    /// </summary>
    [ApiController]
    public class A2AGatewayController : ControllerBase
    {
        private readonly IAgentRegistryService _registry;
        private readonly IUserSettingsDB _session;
        private readonly IAgentMailbox _mailbox;
        private readonly ILogger<A2AGatewayController> _logger;

        public A2AGatewayController(
            IAgentRegistryService registry,
            IUserSettingsDB session,
            IAgentMailbox mailbox,
            ILogger<A2AGatewayController> logger)
        {
            _registry = registry;
            _session = session;
            _mailbox = mailbox;
            _logger = logger;
        }

        [HttpPost("/a2a/{projectKey}/{agentName}")]
        public IActionResult Rpc(string projectKey, string agentName, [FromBody] JsonElement body)
        {
            // --- envelope JSON-RPC 2.0 ---
            JsonNode idNode = body.TryGetProperty("id", out var idEl) ? JsonNode.Parse(idEl.GetRawText()) : null;
            var method = body.TryGetProperty("method", out var mEl) && mEl.ValueKind == JsonValueKind.String
                ? mEl.GetString()
                : null;

            if (method != "message/send")
                return JsonRpc(idNode, error: (-32601, $"Metodo '{method}' non supportato (solo 'message/send')."));
            if (!body.TryGetProperty("params", out var paramsEl) || paramsEl.ValueKind != JsonValueKind.Object)
                return JsonRpc(idNode, error: (-32602, "params mancante o non valido."));

            // --- risoluzione progetto (projectKey = Guid del Project) ---
            if (!Guid.TryParse(projectKey, out var pk))
                return JsonRpc(idNode, error: (-32602, "projectKey non è un Guid valido."));
            var projectPath = ResolveProjectPath(pk);
            if (projectPath == null)
                return JsonRpc(idNode, error: (-32001, $"Progetto {projectKey} non trovato."));

            // --- ri-validazione del destinatario dalle fonti (§7): cittadino, trusted ---
            var catalog = _registry.RefreshCatalog(projectPath);
            var entry = catalog
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, agentName, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
                return JsonRpc(idNode, error: (-32001, $"Agente '{agentName}' non trovato o non cittadino."));
            if (!entry.Trusted)
                return JsonRpc(idNode, error: (-32002, $"Agente '{agentName}' non è trusted: conferma il trust prima di inviargli messaggi."));
            // Da Fase 3 step 4b il dispatcher sveglia anche gli agenti LLM: nessuna restrizione di Kind qui.

            // --- estrazione messaggio + contesto ---
            var (text, contextId, declaredFrom) = ReadMessage(paramsEl);

            // Il mittente dichiarato è NON autenticato: normalizzato fail-loud. 'user' e i
            // nomi non kebab-case sono rifiutati — esenzione hop e riapertura conversazioni
            // spettano solo a canali dove il mittente è certificato.
            var fromAgent = MdExplorer.Features.Agents.MessageAuthorization.ResolveDeclaredSender(declaredFrom, out var senderError);
            if (fromAgent == null)
                return JsonRpc(idNode, error: (-32602, senderError));

            // Un nome di cittadino non è spendibile qui: i cittadini si parlano sul canale
            // autenticato (RunToken, tool SendAgentMessage), dove il mittente è certificato.
            if (catalog.Any(e => e.IsCitizen && string.Equals(e.Name, fromAgent, StringComparison.OrdinalIgnoreCase)))
                return JsonRpc(idNode, error: (-32005, $"fromAgent '{fromAgent}' è un cittadino del progetto: i messaggi tra cittadini passano dal canale autenticato (tool SendAgentMessage), non dal gateway."));

            // Filtro fine del destinatario (§6) fail-fast; il backstop autoritativo è nel
            // dispatcher alla consegna, comune a ogni percorso di accodamento.
            if (!MdExplorer.Features.Agents.MessageAuthorization.IsSenderAccepted(entry.AcceptsMessagesFrom, fromAgent))
                return JsonRpc(idNode, error: (-32003, $"'{fromAgent}' non è tra i mittenti accettati da '{agentName}' (accepts_messages_from)."));

            // --- ACCODAMENTO nella mailbox (§8) ---
            var enqueue = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = projectPath,
                FromAgent = fromAgent,
                ToAgent = agentName,
                Body = text,
                ContextId = contextId,
                HopLimitOverride = entry.MaxHops,
            });

            if (!enqueue.Accepted)
                return JsonRpc(idNode, error: (-32004, enqueue.RejectionReason));

            // Consegna asincrona a carico del dispatcher → task 'submitted'.
            var task = new AgentTask
            {
                Id = enqueue.TaskId,
                ContextId = enqueue.ConversationId.ToString(),
                Status = new global::A2A.TaskStatus
                {
                    State = TaskState.Submitted,
                    Timestamp = DateTimeOffset.UtcNow,
                },
            };
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

        private static (string text, string contextId, string fromAgent) ReadMessage(JsonElement paramsEl)
        {
            var sb = new StringBuilder();
            string contextId = null, fromAgent = null;

            if (paramsEl.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.Object)
            {
                if (msg.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                    foreach (var part in parts.EnumerateArray())
                        if (part.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                            sb.Append(t.GetString());

                if (msg.TryGetProperty("contextId", out var c) && c.ValueKind == JsonValueKind.String)
                    contextId = c.GetString();

                // Mittente dichiarato (§8): metadata non-standard, best-effort (non autenticato
                // finché non c'è il RunToken, R2 — step successivo).
                if (msg.TryGetProperty("metadata", out var meta) && meta.ValueKind == JsonValueKind.Object
                    && meta.TryGetProperty("fromAgent", out var f) && f.ValueKind == JsonValueKind.String)
                    fromAgent = f.GetString();
            }
            return (sb.ToString(), contextId, fromAgent);
        }

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
