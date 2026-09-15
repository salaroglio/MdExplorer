using System;

namespace MdExplorer.Features.AgentMemory
{
    /// <summary>
    /// Naming dei named graph della memoria degli agenti (§11 / Fase 5a) — <b>unico punto di
    /// verità</b>: chiunque tocchi Fuseki per la memoria passa da qui, mai URI a mano.
    /// La chiave è <c>AgentIdentity.Id</c> (stabile: sopravvive al rename dell'agente, §6);
    /// <c>Shared</c> è il grafo dei fatti condivisi della città.
    /// </summary>
    public static class AgentMemoryGraphs
    {
        /// <summary>Grafo condiviso del progetto: fatti visibili a tutti i cittadini.</summary>
        public const string Shared = "urn:mde:mem:shared";

        /// <summary>Grafo privato di un agente, per <c>AgentIdentity.Id</c>.</summary>
        public static string ForAgent(Guid agentIdentityId)
        {
            if (agentIdentityId == Guid.Empty)
                throw new ArgumentException("AgentIdentity.Id vuoto: senza identità non esiste un grafo di memoria.", nameof(agentIdentityId));
            return $"urn:mde:mem:agent:{agentIdentityId:D}";
        }
    }
}
