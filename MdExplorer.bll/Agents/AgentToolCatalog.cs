using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents
{
    /// <summary>Cosa può fare un agente con un dato tool.</summary>
    public enum ToolAccess
    {
        /// <summary>Esposto e invocabile.</summary>
        Allowed,

        /// <summary>
        /// L'agente lo ha dichiarato ma l'umano non gli ha ancora dato fiducia: si espone come
        /// <b>richiesta di approvazione</b> invece di sparire. L'intenzione è nota, manca
        /// l'autorizzazione — è il caso in cui far vedere all'umano cosa voleva fare vale più
        /// che negarlo in silenzio.
        /// </summary>
        ApprovalRequired,

        /// <summary>Mai esposto: non dichiarato nel manifesto, oppure ignoto al catalogo.</summary>
        Denied,

        /// <summary>
        /// Dichiarato, ma questo runner non sa fornirlo (es. <c>shell</c> su un provider API,
        /// che non esegue comandi). Non è un permesso negato: è una <b>capacità mancante</b>, e
        /// va detto al chiamante invece di lasciar credere all'agente di averla.
        /// </summary>
        Unsupported,
    }

    /// <summary>Esito della classificazione di un singolo tool.</summary>
    public sealed class ToolDecision
    {
        public string Tool { get; init; }
        public ToolAccess Access { get; init; }
        /// <summary>Perché, in una frase leggibile (finisce nei log e nella diagnostica).</summary>
        public string Reason { get; init; }
    }

    /// <summary>
    /// Decide <b>quali tool esporre a un agente</b>, dal manifesto <c>tools:</c> della sua card
    /// incrociato con la fiducia dell'umano.
    /// <para>
    /// È il terzo ruolo del modello di autorizzazione (§10): l'agente <b>dichiara</b>, l'umano
    /// <b>autorizza</b> col trust, l'harness <b>enforza</b>. Finché il turno gira su Copilot
    /// l'enforcement dipende dai flag di una CLI di terze parti e resta un'intenzione; quando il
    /// catalogo lo compone l'harness, l'enforcement è nostro e <i>certo</i> — è qui che si chiude
    /// l'hardening #1 rimasto aperto.
    /// </para>
    /// <para>
    /// Due principi, entrambi presi dal design: <b>la cittadinanza è read-only</b> (leggere non
    /// si nega a nessun cittadino; scrivere e agire sono privilegi revocabili), e <b>fail-closed</b>
    /// (un tool che il catalogo non conosce non passa — meglio un agente che non può fare una
    /// cosa di un agente che ne fa una che non avevamo previsto).
    /// </para>
    /// </summary>
    public static class AgentToolCatalog
    {
        // ── Vocabolario del manifesto (quello di Copilot CLI, già in uso nei .agent.md) ──
        public const string ManifestRead = "read";
        public const string ManifestSearch = "search";
        public const string ManifestWrite = "write";
        public const string ManifestEdit = "edit";
        public const string ManifestShell = "shell";

        /// <summary>
        /// Tool di sola lettura: sempre concessi a un cittadino, anche senza dichiarazione e
        /// senza fiducia. Include il "leggere la città" (chi c'è, cosa ho imparato).
        /// </summary>
        private static readonly HashSet<string> ReadOnlyTools = new(StringComparer.OrdinalIgnoreCase)
        {
            "read_markdown_file",
            "search_documents",
            "list_agents",
            "query_agent_memory",
        };

        /// <summary>
        /// Tool che modificano il progetto: richiedono <c>write</c> o <c>edit</c> nel manifesto.
        /// </summary>
        private static readonly HashSet<string> WriteTools = new(StringComparer.OrdinalIgnoreCase)
        {
            "create_markdown_file",
            "update_markdown_file",
            "create_slide_presentation",
            "assert_learned_fact",
        };

        /// <summary>
        /// Azioni verso l'esterno: svegliano un altro agente o consumano budget federato. Non
        /// esiste una parola nel vocabolario del manifesto per dichiararle — i <c>.agent.md</c>
        /// esistenti non potrebbero farlo — quindi il cancello è la <b>sola fiducia</b>. È
        /// coerente col resto: la messaggistica è un privilegio esplicito, non un diritto di
        /// cittadinanza.
        /// </summary>
        private static readonly HashSet<string> OutboundTools = new(StringComparer.OrdinalIgnoreCase)
        {
            "send_agent_message",
            "request_intervention",
        };

        /// <summary>
        /// Classifica ogni tool offerto. <paramref name="offeredTools"/> sono i nomi che il
        /// runner può realmente fornire (es. quelli che il server MCP dichiara più quelli sui
        /// documenti): il catalogo non inventa nomi, decide su quelli che esistono.
        /// </summary>
        public static IReadOnlyList<ToolDecision> Decide(
            IEnumerable<string> offeredTools,
            IEnumerable<string> declaredManifest,
            bool trusted)
        {
            var offered = (offeredTools ?? Enumerable.Empty<string>())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var manifest = new HashSet<string>(
                (declaredManifest ?? Enumerable.Empty<string>())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Trim()),
                StringComparer.OrdinalIgnoreCase);

            var declaresWrite = manifest.Contains(ManifestWrite) || manifest.Contains(ManifestEdit);

            var decisions = new List<ToolDecision>();

            foreach (var tool in offered)
            {
                if (ReadOnlyTools.Contains(tool))
                {
                    decisions.Add(new ToolDecision
                    {
                        Tool = tool,
                        Access = ToolAccess.Allowed,
                        Reason = "sola lettura: diritto di cittadinanza",
                    });
                    continue;
                }

                if (WriteTools.Contains(tool))
                {
                    if (!declaresWrite)
                    {
                        decisions.Add(new ToolDecision
                        {
                            Tool = tool,
                            Access = ToolAccess.Denied,
                            Reason = "scrittura non dichiarata nel manifesto 'tools:' dell'agente",
                        });
                    }
                    else
                    {
                        decisions.Add(new ToolDecision
                        {
                            Tool = tool,
                            Access = trusted ? ToolAccess.Allowed : ToolAccess.ApprovalRequired,
                            Reason = trusted
                                ? "scrittura dichiarata e agente fidato"
                                : "scrittura dichiarata ma l'agente non è ancora fidato",
                        });
                    }
                    continue;
                }

                if (OutboundTools.Contains(tool))
                {
                    decisions.Add(new ToolDecision
                    {
                        Tool = tool,
                        Access = trusted ? ToolAccess.Allowed : ToolAccess.ApprovalRequired,
                        Reason = trusted
                            ? "azione verso l'esterno consentita a un agente fidato"
                            : "azione verso l'esterno: richiede la fiducia dell'umano",
                    });
                    continue;
                }

                // Fail-closed: un nome che il catalogo non conosce non passa, e si vede.
                decisions.Add(new ToolDecision
                {
                    Tool = tool,
                    Access = ToolAccess.Denied,
                    Reason = "tool sconosciuto al catalogo: negato per difetto",
                });
            }

            return decisions;
        }

        /// <summary>
        /// Capacità dichiarate dall'agente che questo runner <b>non</b> sa fornire. Vanno
        /// segnalate: un agente istruito a usare la shell, eseguito da un runner che non ce
        /// l'ha, non fallisce — fa una cosa diversa e la chiama fatta.
        /// </summary>
        public static IReadOnlyList<ToolDecision> UnsupportedCapabilities(
            IEnumerable<string> declaredManifest, bool runnerHasShell)
        {
            var manifest = (declaredManifest ?? Enumerable.Empty<string>())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim());

            var missing = new List<ToolDecision>();
            foreach (var declared in manifest.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!runnerHasShell && string.Equals(declared, ManifestShell, StringComparison.OrdinalIgnoreCase))
                {
                    missing.Add(new ToolDecision
                    {
                        Tool = ManifestShell,
                        Access = ToolAccess.Unsupported,
                        Reason = "questo runner non esegue comandi: l'agente dichiara 'shell' ma non l'avrà",
                    });
                }
            }
            return missing;
        }

        /// <summary>Comodità: i soli nomi effettivamente invocabili.</summary>
        public static IReadOnlyList<string> AllowedNames(IEnumerable<ToolDecision> decisions)
            => (decisions ?? Enumerable.Empty<ToolDecision>())
               .Where(d => d.Access == ToolAccess.Allowed)
               .Select(d => d.Tool)
               .ToList();
    }
}
