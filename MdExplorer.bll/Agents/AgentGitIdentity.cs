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
        /// Le quattro variabili d'ambiente che firmano ogni commit fatto dal processo
        /// dell'agente come l'agente stesso (autore <b>e</b> committer). Vuoto se il nome manca.
        /// </summary>
        public static IReadOnlyDictionary<string, string> EnvFor(string agentName)
        {
            var name = (agentName ?? string.Empty).Trim();
            if (name.Length == 0) return new Dictionary<string, string>();
            var email = EmailFor(name);
            return new Dictionary<string, string>
            {
                [EnvAuthorName] = name,
                [EnvAuthorEmail] = email,
                [EnvCommitterName] = name,
                [EnvCommitterEmail] = email,
            };
        }
    }
}
