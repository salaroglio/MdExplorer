using System;

namespace MdExplorer.Features.Services.AI.ClaudeCode
{
    /// <summary>
    /// Frammento emesso durante lo streaming di un turno.
    /// <see cref="Kind"/> separa il ragionamento interno dal testo visibile, così la UI li
    /// manda su canali diversi — stessa distinzione che fa <c>CopilotAcpChunk</c>, più un
    /// terzo canale per l'attività sui tool, che il protocollo di Claude Code espone e quello
    /// di Copilot no.
    /// </summary>
    public readonly struct ClaudeCodeChunk
    {
        public string Kind { get; }
        public string Text { get; }
        public ClaudeCodeChunk(string kind, string text) { Kind = kind; Text = text; }

        /// <summary>Testo visibile della risposta (delta <c>text_delta</c>).</summary>
        public const string KindMessage = "message";
        /// <summary>Ragionamento interno (delta <c>thinking_delta</c>).</summary>
        public const string KindThinking = "thinking";
        /// <summary>Attività sui tool: "sta leggendo X", "sta eseguendo Y". Riga di stato, non risposta.</summary>
        public const string KindTool = "tool";
    }

    /// <summary>
    /// Consuntivo di un turno, letto dal messaggio <c>result</c>.
    /// <para>
    /// È il dato che su Copilot ACP <b>non esiste</b>: là il risultato del prompt porta solo
    /// <c>stopReason</c>, e per questo l'indicatore consumi era stato accantonato. Qui arriva
    /// per costruzione, a ogni turno.
    /// </para>
    /// </summary>
    public sealed class ClaudeCodeTurnUsage
    {
        /// <summary>
        /// Costo <b>cumulato della sessione</b> in dollari, così come arriva in
        /// <c>total_cost_usd</c>.
        /// <para>⚠️ Il nome del campo sul filo inganna: non è il costo del turno. Misurato su
        /// quattro turni consecutivi della stessa sessione cresce in modo monotòno
        /// (0,0216 → 0,0243 → 0,0243 → 0,0271 $), e il turno annullato non ha aggiunto nulla.
        /// Il costo del singolo turno è la <b>differenza</b> fra due consuntivi consecutivi:
        /// vedi <see cref="TurnCostUsd"/>.</para>
        /// </summary>
        public double? SessionCostUsd { get; init; }

        /// <summary>
        /// Costo del <b>solo</b> turno appena concluso, calcolato per differenza dal
        /// consuntivo precedente della stessa sessione. <c>null</c> quando la differenza non
        /// è calcolabile (primo consuntivo mancante o valore assente).
        /// </summary>
        public double? TurnCostUsd { get; init; }
        public int? InputTokens { get; init; }
        public int? OutputTokens { get; init; }
        public int? ThinkingTokens { get; init; }
        public int? CacheReadInputTokens { get; init; }
        public int? CacheCreationInputTokens { get; init; }
        /// <summary>Durata del turno in millisecondi (<c>duration_ms</c>).</summary>
        public long? DurationMs { get; init; }
        /// <summary><c>success</c> oppure <c>error_during_execution</c>.</summary>
        public string Subtype { get; init; }
        /// <summary>Modello effettivamente usato, come riportato dall'<c>init</c> del turno.</summary>
        public string Model { get; init; }
        /// <summary>Numero di richieste di tool negate, dal campo <c>permission_denials</c>.</summary>
        public int PermissionDenials { get; init; }
    }

    /// <summary>
    /// Stato delle finestre di consumo dell'abbonamento, dall'evento <c>rate_limit_event</c>
    /// che il CLI emette spontaneamente. <see cref="Utilization"/> è una frazione 0..1.
    /// </summary>
    public sealed class ClaudeCodeRateLimit
    {
        public string Status { get; init; }
        public double? FiveHourUtilization { get; init; }
        public DateTimeOffset? FiveHourResetsAt { get; init; }
        public double? SevenDayUtilization { get; init; }
        public DateTimeOffset? SevenDayResetsAt { get; init; }
    }

    /// <summary>
    /// Quali tool può usare Claude Code dentro il progetto dell'utente.
    /// <para>
    /// ⚠️ In modalità <c>-p</c> <b>non esiste</b> un canale di approvazione verso il host: non
    /// arriva nessun <c>can_use_tool</c> e MDE non può chiedere "permetti questo comando?".
    /// La decisione si prende <b>qui</b>, prima di lanciare il processo — verificato sul
    /// campo, §5.1 del piano.
    /// </para>
    /// </summary>
    public enum ClaudeCodeToolPolicy
    {
        /// <summary>
        /// Tutti i tool, esecuzione compresa. È la <b>parità</b> con quello che MDE già
        /// concede a Copilot (<c>--allow-all-tools</c>).
        /// </summary>
        Full = 0,

        /// <summary>
        /// Niente esecuzione di comandi: <c>Bash</c> e i suoi parenti vengono negati con
        /// <c>--disallowedTools</c> (verificato: il tool sparisce davvero dal set).
        /// <para>
        /// ⚠️ Volutamente NON implementato come <c>--tools ""</c>: con zero tool il modello
        /// non rifiuta, <b>si inventa</b> la chiamata nel testo e finge l'output. Il divieto
        /// mirato è l'unica forma che regge.
        /// </para>
        /// </summary>
        NoExecution = 1
    }

    /// <summary>
    /// Opzioni di lancio di una sessione. I default sono le risposte provvisorie alle domande
    /// aperte del piano: cambiare qui cambia il comportamento in un punto solo.
    /// </summary>
    public sealed class ClaudeCodeSessionOptions
    {
        /// <summary>
        /// Superficie di esecuzione. Default <see cref="ClaudeCodeToolPolicy.Full"/> = parità
        /// con Copilot (Q2 del piano, ancora aperta).
        /// </summary>
        public ClaudeCodeToolPolicy ToolPolicy { get; init; } = ClaudeCodeToolPolicy.Full;

        /// <summary>
        /// Se <c>true</c> aggiunge <c>--bare</c>: niente memoria automatica, niente hook,
        /// niente CLAUDE.md, niente skill/plugin. Default <c>false</c> — una chat che gira
        /// dentro il progetto dell'utente ha senso che legga il CLAUDE.md del progetto.
        ///
        /// <para>⚠️ <b>Con il default, Claude Code scrive memorie di sua iniziativa</b> in
        /// <c>~/.claude/projects/&lt;slug-del-cwd&gt;/memory/</c> — visto accadere in un probe:
        /// un tool <c>Write</c> su quel percorso senza che nessuno l'abbia chiesto (Q1 del piano).</para>
        ///
        /// <para>⚠️ <b>Ma <c>--bare</c> non è gratis</b>: spegne anche la lettura di OAuth e
        /// portachiavi, e l'autenticazione diventa <b>solo</b> <c>ANTHROPIC_API_KEY</c>. Su un
        /// utente collegato con l'abbonamento — il caso normale — il CLI risponde
        /// «Not logged in · Please run /login» e il turno muore. Per questo
        /// <see cref="ClaudeCodeSession.StartAsync"/> rifiuta in partenza <c>Bare = true</c>
        /// senza una API key nell'ambiente, invece di lasciar fallire il primo turno con un
        /// messaggio che non c'entra niente.</para>
        /// </summary>
        public bool Bare { get; init; } = false;

        /// <summary>Nome della variabile d'ambiente che <c>--bare</c> pretende per autenticarsi.</summary>
        public const string ApiKeyEnvVariable = "ANTHROPIC_API_KEY";

        /// <summary>
        /// File di configurazione MCP da passare a <c>--mcp-config</c>. Quando valorizzato
        /// viene aggiunto anche <c>--strict-mcp-config</c>, così Claude Code vede
        /// <b>soltanto</b> i server dichiarati lì e non eredita quelli personali dell'utente
        /// (verificato: senza, nell'init comparivano i server MCP personali).
        /// </summary>
        public string McpConfigPath { get; init; }

        /// <summary>
        /// Id di sessione da riprendere (<c>--resume</c>). Quando valorizzato, la
        /// conversazione riparte da dove era, anche dopo un riavvio del processo o di MDE.
        /// </summary>
        public string ResumeSessionId { get; init; }

        public static readonly ClaudeCodeSessionOptions Default = new ClaudeCodeSessionOptions();
    }
}
