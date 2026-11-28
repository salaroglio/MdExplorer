using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace MdExplorer.Features.Services.AI.Codex
{
    /// <summary>
    /// Un frammento del turno, nella stessa forma di <c>ClaudeCodeChunk</c> e <c>OpenCodeChunk</c>: l'hub della chat li
    /// traduce tutti negli stessi eventi SignalR.
    /// </summary>
    public readonly struct CodexChunk
    {
        public string Kind { get; }
        public string Text { get; }
        /// <summary>Per uno strumento: la riga per il diario del turno degli agenti; null se al diario non dice niente.</summary>
        public string Diary { get; }
        /// <summary>Per <see cref="KindTool"/>: la chiamata o il risultato interi, per la cronologia della chat. Può mancare.</summary>
        public MdExplorer.Features.Services.AI.ChatHistory.ChatToolRecord Record { get; }

        public CodexChunk(string kind, string text) { Kind = kind; Text = text; Diary = kind == KindTool ? text : null; Record = null; }
        public CodexChunk(string kind, string text, string diary, MdExplorer.Features.Services.AI.ChatHistory.ChatToolRecord record)
        { Kind = kind; Text = text; Diary = diary; Record = record; }

        /// <summary>Testo visibile (<c>item/agentMessage/delta</c>).</summary>
        public const string KindMessage = "message";
        /// <summary>Ragionamento (<c>item/reasoning/summaryTextDelta</c> e <c>textDelta</c>).</summary>
        public const string KindThinking = "thinking";
        /// <summary>Attività sugli strumenti: comandi, modifiche ai file, chiamate MCP.</summary>
        public const string KindTool = "tool";
    }

    /// <summary>
    /// Consuntivo del turno, da <c>thread/tokenUsage/updated</c>: <c>last</c> è il turno, <c>total</c> la conversazione
    /// (misurato su Codex 0.172.0). Codex non dà un costo in dollari: sul piano ChatGPT conta la quota.
    /// </summary>
    public sealed class CodexTurnUsage
    {
        public string Model { get; init; }
        public long? InputTokens { get; init; }
        public long? CachedInputTokens { get; init; }
        public long? OutputTokens { get; init; }
        public long? ReasoningTokens { get; init; }
        public long? TotalTokens { get; init; }
        /// <summary>Token di tutta la conversazione (<c>total.totalTokens</c>).</summary>
        public long? ThreadTotalTokens { get; init; }
        /// <summary>Finestra di contesto del modello (<c>modelContextWindow</c>).</summary>
        public long? ContextWindow { get; init; }
        public long? DurationMs { get; init; }
        /// <summary><c>completed</c>, <c>interrupted</c> o <c>failed</c>.</summary>
        public string Status { get; init; }
    }

    /// <summary>La quota del piano, da <c>account/rateLimits/updated</c>.</summary>
    public sealed class CodexRateLimit
    {
        public string PlanType { get; init; }
        /// <summary>Percentuale usata della finestra principale (0..100).</summary>
        public double? PrimaryUsedPercent { get; init; }
        public long? PrimaryWindowMinutes { get; init; }
        public DateTimeOffset? PrimaryResetsAt { get; init; }
        public double? SecondaryUsedPercent { get; init; }
        public long? SecondaryWindowMinutes { get; init; }
        public DateTimeOffset? SecondaryResetsAt { get; init; }
        /// <summary>Quale limite è stato raggiunto, se uno lo è (<c>rateLimitReachedType</c>).</summary>
        public string ReachedType { get; init; }
    }

    /// <summary>Dove può scrivere Codex: il <c>sandbox</c> di <c>thread/start</c>.</summary>
    public enum CodexSandbox
    {
        /// <summary><c>read-only</c>: legge tutto, non scrive niente (misurato: «filesystem in sola lettura»).</summary>
        ReadOnly = 0,
        /// <summary><c>workspace-write</c>: scrive nella cartella di lavoro e nelle radici scrivibili dichiarate.</summary>
        WorkspaceWrite = 1,
    }

    /// <summary>Una richiesta di approvazione di Codex, ridotta a quello che serve per decidere.</summary>
    public sealed class CodexApprovalRequest
    {
        /// <summary><c>command</c> (comando di shell) o <c>fileChange</c> (modifica ai file).</summary>
        public string Kind { get; init; }
        /// <summary>Per un comando: la riga intera (<c>/bin/bash -lc "…"</c>).</summary>
        public string Command { get; init; }
        public string Cwd { get; init; }
        /// <summary>Per una modifica: i file toccati (dall'item <c>fileChange</c> già annunciato), vuoto se ignoti.</summary>
        public IReadOnlyList<string> Paths { get; init; } = Array.Empty<string>();

        public const string KindCommand = "command";
        public const string KindFileChange = "fileChange";
    }

    /// <summary>La risposta a una <see cref="CodexApprovalRequest"/>: <see cref="Accept"/>, o un rifiuto col motivo.</summary>
    public sealed class CodexApprovalDecision
    {
        public bool Accept { get; init; }
        /// <summary>Perché no: va nel log e nella riga degli strumenti della chat.</summary>
        public string Reason { get; init; }
        public static readonly CodexApprovalDecision Yes = new() { Accept = true };
        public static CodexApprovalDecision No(string reason) => new() { Accept = false, Reason = reason };
    }

    /// <summary>Un server MCP della sessione: va in <c>config.mcp_servers</c> di <c>thread/start</c>.</summary>
    public sealed class CodexMcpServer
    {
        public string Name { get; init; }
        public string Command { get; init; }
        public IReadOnlyList<string> Args { get; init; } = Array.Empty<string>();
        /// <summary>Valori d'ambiente per il server (il RunToken di un agente, l'identità git).</summary>
        public IReadOnlyDictionary<string, string> Env { get; init; }
        /// <summary>Nomi di strumenti da non esporre (<c>disabled_tools</c>), per esempio quelli vietati a un test e2e.</summary>
        public IReadOnlyList<string> DisabledTools { get; init; }
    }

    /// <summary>
    /// Opzioni di una sessione Codex. Tutte finiscono nel <c>thread/start</c> (o <c>thread/resume</c>): sandbox,
    /// approvazioni, <c>config</c> con server MCP, funzioni spente e ricerca web.
    /// </summary>
    public sealed class CodexSessionOptions
    {
        public CodexSandbox Sandbox { get; init; } = CodexSandbox.WorkspaceWrite;

        /// <summary>
        /// Chi decide su comandi e modifiche. Null = nessuno: <c>approvalPolicy: "never"</c>, Codex fa da solo ciò che
        /// il sandbox permette (gli agenti, D11 del motore unico). Valorizzato = <c>"untrusted"</c>: ogni comando e
        /// modifica arriva qui (la chat del tab, con le regole delle radici e di ssh/scp).
        /// </summary>
        public Func<CodexApprovalRequest, CodexApprovalDecision> Approve { get; init; }

        /// <summary>I server MCP della sessione. Quelli personali dell'utente vengono spenti (come <c>--strict-mcp-config</c>).</summary>
        public IReadOnlyList<CodexMcpServer> McpServers { get; init; } = Array.Empty<CodexMcpServer>();

        /// <summary>Ricerca web (<c>web_search = "live"</c>); spenta = <c>"disabled"</c>.</summary>
        public bool WebSearch { get; init; }

        /// <summary>Lo strumento della shell (<c>features.shell_tool</c>). Spento per le sessioni che non devono eseguire niente.</summary>
        public bool Shell { get; init; } = true;

        /// <summary>Cartelle in più dove il sandbox <c>workspace-write</c> lascia scrivere (<c>sandbox_workspace_write.writable_roots</c>).</summary>
        public IReadOnlyList<string> WritableRoots { get; init; }

        /// <summary>
        /// Un profilo di permessi del filesystem al posto del <see cref="Sandbox"/> (i test e2e): percorso → <c>read</c> | <c>write</c> |
        /// <c>deny</c>, sopra il profilo <c>:workspace</c>. Le chiavi relative valgono dentro la cartella di lavoro (anche glob, come
        /// <c>**/credenziali-*.txt</c>), quelle assolute ovunque. Misurato il 28/11 con <c>command/exec</c>: un file in <c>deny</c> dà
        /// «Permission denied» a <c>cat</c>, glob, <c>grep -r</c>, Python, <c>cp</c> e link simbolici; i glob si espandono all'avvio del
        /// sandbox. ⚠️ Vale solo per i comandi che girano NEL sandbox: con approvazioni un comando approvato ne esce, quindi un
        /// profilo va con <see cref="Approve"/> null.
        /// </summary>
        public IReadOnlyDictionary<string, string> FileSystemProfile { get; init; }

        /// <summary>Il nome del profilo che MdExplorer scrive nel <c>config</c> del thread.</summary>
        public const string ProfileName = "mdexplorer-session";

        /// <summary>Conversazione da riprendere (<c>thread/resume</c>): l'id del thread, anche di un processo precedente.</summary>
        public string ResumeThreadId { get; init; }

        /// <summary>Variabili d'ambiente in più per il processo di Codex (per esempio <c>CODEX_HOME</c> nelle prove).</summary>
        public IReadOnlyDictionary<string, string> Environment { get; init; }

        /// <summary>Configurazione «test e2e» con cui è partita (null = chat): come <c>ClaudeCodeSessionOptions.ProfileKey</c>.</summary>
        public string ProfileKey { get; init; }

        /// <summary>I gruppi MCP di MdExplorer con cui parte: cambiati, il pool riavvia riprendendo la conversazione.</summary>
        public string McpGroups { get; init; }

        /// <summary>Le radici di codice esterne collegate, in una stringa confrontabile (null = non dichiarate).</summary>
        public string ExtraRootsKey { get; init; }

        /// <summary>
        /// Le funzioni di Codex che MdExplorer spegne sempre (D6): il server <c>codex_apps</c> dei connettori ChatGPT, il
        /// browser, il controllo del computer, la generazione di immagini. Misurato il 28/11: con <c>apps=false</c> il
        /// server <c>codex_apps</c> non parte più.
        /// </summary>
        public static readonly IReadOnlyList<string> AlwaysDisabledFeatures = new[] { "apps", "browser_use", "computer_use", "image_generation" };

        /// <summary>
        /// Le variabili da passare ai server MCP: Codex li avvia con un ambiente ripulito (misurato il 28/11 sulla VM).
        /// Senza <c>DOTNET_ROOT</c> <c>MdExplorer.Mcp</c> muore con «You must install .NET»; senza le cartelle dell'utente
        /// (<c>HOME</c>/<c>XDG_CONFIG_HOME</c> su Linux, <c>APPDATA</c>/<c>USERPROFILE</c> su Windows) o <c>MDEXPLORER_PORT</c>
        /// non trova la porta del servizio (<c>port.txt</c>) e cerca su 5000: visto in F7 col servizio isolato
        /// («MdExplorer non è raggiungibile su localhost:5000»). Una variabile che non c'è non si inoltra.
        /// </summary>
        public static readonly IReadOnlyList<string> McpForwardedVariables = new[]
        {
            "DOTNET_ROOT", "DOTNET_ROOT_X64", "PATH",
            "HOME", "XDG_CONFIG_HOME", "XDG_DATA_HOME", "USERPROFILE", "APPDATA", "LOCALAPPDATA",
            "MDEXPLORER_PORT",
        };

        public static readonly CodexSessionOptions Default = new();

        public string SandboxId => Sandbox switch
        {
            CodexSandbox.ReadOnly => "read-only",
            CodexSandbox.WorkspaceWrite => "workspace-write",
            _ => throw new ArgumentOutOfRangeException(nameof(Sandbox), Sandbox, null),
        };

        public string ApprovalPolicyId => Approve == null ? "never" : "untrusted";

        /// <summary>
        /// Il <c>config</c> del thread. Pubblico e statico per i test: si legge senza lanciare niente.
        /// </summary>
        /// <param name="personalMcpServers">I server MCP già configurati per l'utente (da <c>config/read</c>): si spengono.</param>
        public JsonObject BuildConfig(IEnumerable<string> personalMcpServers)
        {
            var features = new JsonObject();
            foreach (var feature in AlwaysDisabledFeatures) features[feature] = false;
            if (!Shell) features["shell_tool"] = false;

            var servers = new JsonObject();
            var ours = new HashSet<string>(McpServers.Select(s => s.Name), StringComparer.Ordinal);
            foreach (var name in (personalMcpServers ?? Array.Empty<string>()).Where(n => !ours.Contains(n)))
                servers[name] = new JsonObject { ["enabled"] = false };   // misurato il 28/11: il server non parte
            foreach (var server in McpServers)
            {
                var entry = new JsonObject
                {
                    ["command"] = server.Command,
                    ["args"] = new JsonArray(server.Args.Select(a => (JsonNode)a).ToArray()),
                    ["env_vars"] = new JsonArray(McpForwardedVariables.Select(v => (JsonNode)v).ToArray()),
                    // Con approvalPolicy "never" ogni chiamata MCP fallisce («requires approval») se non è approvata qui;
                    // "auto" non basta (misurato il 28/11).
                    ["default_tools_approval_mode"] = "approve",
                };
                if (server.Env != null && server.Env.Count > 0)
                {
                    var env = new JsonObject();
                    foreach (var (key, value) in server.Env) env[key] = value;
                    entry["env"] = env;
                }
                if (server.DisabledTools != null && server.DisabledTools.Count > 0)
                    entry["disabled_tools"] = new JsonArray(server.DisabledTools.Select(t => (JsonNode)t).ToArray());
                servers[server.Name] = entry;
            }

            var config = new JsonObject
            {
                ["features"] = features,
                ["web_search"] = WebSearch ? "live" : "disabled",
                ["mcp_servers"] = servers,
            };
            if (FileSystemProfile != null)
            {
                var inside = new JsonObject();
                var filesystem = new JsonObject { [":workspace_roots"] = inside };
                foreach (var (path, access) in FileSystemProfile)
                {
                    if (access is not ("read" or "write" or "deny"))
                        throw new ArgumentException($"Permesso '{access}' non valido per '{path}': read, write o deny.");
                    if (System.IO.Path.IsPathRooted(path)) filesystem[path] = access;
                    else inside[path] = access;
                }
                config["default_permissions"] = ProfileName;
                config["permissions"] = new JsonObject
                {
                    [ProfileName] = new JsonObject { ["extends"] = ":workspace", ["filesystem"] = filesystem },
                };
            }
            if (FileSystemProfile == null && Sandbox == CodexSandbox.WorkspaceWrite && WritableRoots != null && WritableRoots.Count > 0)
                config["sandbox_workspace_write"] = new JsonObject
                {
                    ["writable_roots"] = new JsonArray(WritableRoots.Select(r => (JsonNode)r).ToArray()),
                };
            return config;
        }

        /// <summary>Le stesse opzioni, riprendendo il thread <paramref name="threadId"/>.</summary>
        public CodexSessionOptions WithResume(string threadId) => new()
        {
            Sandbox = Sandbox,
            Approve = Approve,
            McpServers = McpServers,
            WebSearch = WebSearch,
            Shell = Shell,
            WritableRoots = WritableRoots,
            FileSystemProfile = FileSystemProfile,
            ResumeThreadId = threadId,
            Environment = Environment,
            ProfileKey = ProfileKey,
            McpGroups = McpGroups,
            ExtraRootsKey = ExtraRootsKey,
        };
    }
}
