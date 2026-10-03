using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Gruppo <c>agents</c>: la città degli agenti — messaggi fra agenti, richiesta di intervento
/// a un agente di un altro membro (federazione) e memoria dei fatti appresi.
/// </summary>
[McpServerToolType]
public sealed class AgentTools : McpToolsBase
{
    public AgentTools(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

    // ============================================================
    //   Agent-to-agent messaging (città degli agenti, §7)
    // ============================================================

    // Il RunToken viaggia NELL'AMBIENTE del processo (mai nel prompt): il Service lo
    // conia al risveglio, lo mette nell'env del processo Copilot, che lo eredita a questo
    // MCP. Presentandolo, il Service certifica l'identità del mittente (anti-spoofing R2).
    private const string RunTokenEnvVar = "MDE_RUN_TOKEN";
    private const string RunTokenHeader = "X-MDE-Run-Token";

    private static string? RunToken()
    {
        var t = Environment.GetEnvironmentVariable(RunTokenEnvVar);
        return string.IsNullOrWhiteSpace(t) ? null : t;
    }

    [McpServerTool, Description(
        "Request the intervention of an agent that belongs to ANOTHER member's city (federation). " +
        "You give a SCOPE (an area from the project's ownership document) and a message; the harness " +
        "deterministically resolves who owns that scope and which agent should act, then routes the " +
        "request to that member's machine — where THEIR human must explicitly authorize it before any " +
        "agent runs. Only available to an agent woken by a message (identity from the run token). This " +
        "does NOT return an answer: it returns a routing receipt. Use only when the work belongs to a " +
        "different owner (see the '# Ownership del progetto' section of your prompt).")]
    public async Task<string> RequestIntervention(
        [Description("The ownership scope the work belongs to (exact name from the ownership table).")] string scope,
        [Description("The request body for the remote agent. Plain text; state clearly what you need.")] string message,
        [Description("Optional preferred agent name (must be one listed for that scope).")] string preferredAgent = null,
        [Description("Optional topics/tags, comma-separated (context only).")] string topics = null)
    {
        var token = RunToken();
        if (token == null)
            return "Error: RequestIntervention is only available to an agent woken by a message (no run token in the environment).";
        if (string.IsNullOrWhiteSpace(scope)) return "Error: scope is required.";
        if (string.IsNullOrWhiteSpace(message)) return "Error: message is required.";

        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var payload = new
            {
                scope = scope.Trim(),
                message,
                preferredAgent = string.IsNullOrWhiteSpace(preferredAgent) ? null : preferredAgent.Trim(),
                topics = string.IsNullOrWhiteSpace(topics)
                    ? new List<string>()
                    : topics.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList(),
            };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/A2A/messages/request-intervention") { Content = content };
            req.Headers.Add(RunTokenHeader, token);
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return $"Request refused ({(int)resp.StatusCode}): {body}";
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Send a message to ANOTHER agent that lives in the same MdExplorer project (the " +
        "'city of agents'). Only available to an agent that was itself woken by a message: " +
        "your identity as the sender is taken from the run environment and cannot be forged. " +
        "Use ListAgents first to see who you may contact. The recipient must trust you " +
        "(its 'accepts_messages_from' must include your name or '*'). Delivery is asynchronous: " +
        "the message is queued and the recipient is woken by the harness. This does NOT return " +
        "the recipient's answer — it returns a queue receipt (taskId).")]
    public async Task<string> SendAgentMessage(
        [Description("The recipient agent's name (kebab-case), as shown by ListAgents.")] string toAgent,
        [Description("The message body. Plain text; state your intent clearly.")] string message,
        [Description("Optional topics/tags describing the message, comma-separated (context only).")] string topics = null)
    {
        var token = RunToken();
        if (token == null)
            return "Error: SendAgentMessage is only available to an agent woken by a message (no run token in the environment).";
        if (string.IsNullOrWhiteSpace(toAgent)) return "Error: toAgent is required.";
        if (string.IsNullOrWhiteSpace(message)) return "Error: message is required.";

        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var payload = new
            {
                toAgent = toAgent.Trim(),
                message,
                topics = string.IsNullOrWhiteSpace(topics)
                    ? new List<string>()
                    : topics.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList(),
            };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/A2A/messages/send") { Content = content };
            req.Headers.Add(RunTokenHeader, token);
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return $"Send refused ({(int)resp.StatusCode}): {body}";
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Remember a fact you learned, so your FUTURE selves recall it across wake-ups (semantic " +
        "memory). Only available to an agent woken by a message: the fact is stored in YOUR private " +
        "memory graph, keyed to your stable identity from the run environment — you cannot write to " +
        "another agent's memory. Admit ONLY facts that are specific, verifiable and operational " +
        "(e.g. 'the payments batch runs at 02:00 UTC'), never chit-chat or restatements of the " +
        "prompt. Give 'about' tags that match the topics of the conversation so retrieval finds it " +
        "later. Provenance (which run, which conversation) is recorded automatically.")]
    public async Task<string> AssertLearnedFact(
        [Description("The fact, as a short declarative statement. Specific, verifiable, operational.")] string statement,
        [Description("Topic tags this fact is about, comma-separated (align them with the message topics).")] string about = null,
        [Description("Confidence 0..1 (default 0.7). Use ~1.0 only for facts a human confirmed.")] double confidence = 0.7)
    {
        var token = RunToken();
        if (token == null)
            return "Error: AssertLearnedFact is only available to an agent woken by a message (no run token in the environment).";
        if (string.IsNullOrWhiteSpace(statement)) return "Error: statement is required.";

        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var payload = new { statement = statement.Trim(), about, confidence };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/A2A/memory/assert") { Content = content };
            req.Headers.Add(RunTokenHeader, token);
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return $"Assert refused ({(int)resp.StatusCode}): {body}";
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Recall what you (and the shared city memory) already know about some topics, before you " +
        "start working. Only available to an agent woken by a message: you see ONLY your own memory " +
        "graph plus the shared one — never another agent's. Pass the topics you care about; you get " +
        "back the relevant facts with their confidence. Use this to avoid re-deriving what a past " +
        "run already established.")]
    public async Task<string> QueryAgentMemory(
        [Description("Topics/tags to recall facts about, comma-separated. Empty = your most confident facts.")] string topics = null,
        [Description("Max facts to return (default 20).")] int limit = 20)
    {
        var token = RunToken();
        if (token == null)
            return "Error: QueryAgentMemory is only available to an agent woken by a message (no run token in the environment).";

        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var payload = new
            {
                topics = string.IsNullOrWhiteSpace(topics)
                    ? new List<string>()
                    : topics.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList(),
                limit,
            };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/A2A/memory/query") { Content = content };
            req.Headers.Add(RunTokenHeader, token);
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return $"Query refused ({(int)resp.StatusCode}): {body}";
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "List the other trusted agents you may contact in your MdExplorer project (name, role, " +
        "skills), plus whether each currently accepts messages from you. Only available to an " +
        "agent woken by a message: the project and your identity come from the run environment. " +
        "Call this before SendAgentMessage.")]
    public async Task<string> ListAgents()
    {
        var token = RunToken();
        if (token == null)
            return "Error: ListAgents is only available to an agent woken by a message (no run token in the environment).";

        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/api/A2A/messages/roster");
            req.Headers.Add(RunTokenHeader, token);
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return $"Error ({(int)resp.StatusCode}): {body}";
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }
}
