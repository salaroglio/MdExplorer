using System;
using System.Collections.Generic;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// La firma git per-agente (§10): se la sicurezza del repo si regge sull'attribuzione,
    /// anche i cittadini artificiali si firmano. Un agente che scrive nel workspace committa
    /// con la <b>propria</b> identità — <c>stem-curator &lt;stem-curator@agents.mde&gt;</c> — non
    /// con quella dell'umano. Così <c>git blame</c> funziona sugli agenti come sui colleghi e
    /// ogni danno è un diff visibile e revertibile.
    /// <para>
    /// Meccanismo: le variabili d'ambiente standard di git (<c>GIT_AUTHOR_*</c>/
    /// <c>GIT_COMMITTER_*</c>) sovrascrivono <c>user.name</c>/<c>user.email</c> del repo per il
    /// processo dell'agente (verificato: git le onora sopra la config locale). Viaggiano nello
    /// stesso canale <c>Environment</c> del RunToken.
    /// </para>
    /// </summary>
    public static class AgentGitIdentity
    {
        /// <summary>Dominio riservato agli agenti — non risolve, marca l'identità come artificiale.</summary>
        public const string EmailDomain = "agents.mde";

        public const string EnvAuthorName = "GIT_AUTHOR_NAME";
        public const string EnvAuthorEmail = "GIT_AUTHOR_EMAIL";
        public const string EnvCommitterName = "GIT_COMMITTER_NAME";
        public const string EnvCommitterEmail = "GIT_COMMITTER_EMAIL";

        /// <summary>Email canonica dell'agente: <c>&lt;name&gt;@agents.mde</c> (il nome è già kebab-case).</summary>
        public static string EmailFor(string agentName)
            => $"{(agentName ?? string.Empty).Trim()}@{EmailDomain}";

        /// <summary>
        /// Nome di firma derivato dal file <c>&lt;x&gt;.agent.md</c>, per i run schedulati/manuali
        /// di un agente <b>non-cittadino</b> (senza blocco <c>a2a:</c>, quindi senza nome a2a).
        /// Preferisci sempre il nome a2a del cittadino quando c'è; questo è il fallback.
        /// </summary>
        public static string NameFromAgentFile(string agentFilePath)
        {
            var file = System.IO.Path.GetFileName(agentFilePath ?? string.Empty);
            const string suffix = ".agent.md";
            if (file.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                file = file.Substring(0, file.Length - suffix.Length);
            file = file.Trim();
            return file.Length == 0 ? "agent" : file;
        }

        /// <summary>
        /// Le quattro variabili d'ambiente che firmano ogni commit fatto dal processo
        /// dell'agente come l'agente stesso (autore <b>e</b> committer). Vuoto se il nome manca.
        /// </summary>
        public static IReadOnlyDictionary<string, string> EnvFor(string agentName)
            => EnvFor(agentName, null, null);

        /// <summary>
        /// Autore l'agente, che ha scritto la modifica; committer la <b>persona responsabile</b> che l'ha fatto partire
        /// e ne risponde (sprint 2026-11-25-Nuovo-Ramo-E-Lavoro-All-Agente): ogni agente gira sul computer di chi ne
        /// risponde, che ne autorizza la partenza. Così <c>git blame</c> distingue le righe dell'agente e il log dice chi
        /// ha innescato la modifica. Senza la persona (nome o email mancanti) committer resta l'agente.
        /// </summary>
        public static IReadOnlyDictionary<string, string> EnvFor(string agentName, string responsibleName, string responsibleEmail)
        {
            var name = (agentName ?? string.Empty).Trim();
            if (name.Length == 0) return new Dictionary<string, string>();
            var email = EmailFor(name);
            var hasPerson = !string.IsNullOrWhiteSpace(responsibleName) && !string.IsNullOrWhiteSpace(responsibleEmail);
            return new Dictionary<string, string>
            {
                [EnvAuthorName] = name,
                [EnvAuthorEmail] = email,
                [EnvCommitterName] = hasPerson ? responsibleName.Trim() : name,
                [EnvCommitterEmail] = hasPerson ? responsibleEmail.Trim() : email,
            };
        }

        /// <summary>Una persona come autore e committer: la fusione approvata la firma chi approva.</summary>
        public static IReadOnlyDictionary<string, string> EnvForPerson(string personName, string personEmail)
        {
            if (string.IsNullOrWhiteSpace(personName) || string.IsNullOrWhiteSpace(personEmail)) return new Dictionary<string, string>();
            return new Dictionary<string, string>
            {
                [EnvAuthorName] = personName.Trim(),
                [EnvAuthorEmail] = personEmail.Trim(),
                [EnvCommitterName] = personName.Trim(),
                [EnvCommitterEmail] = personEmail.Trim(),
            };
        }
    }
}
