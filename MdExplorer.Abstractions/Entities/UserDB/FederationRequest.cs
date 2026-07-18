using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Una richiesta di intervento <b>federata</b> in attesa del gate umano (§12.6 Fase 6c):
    /// la città di un altro membro chiede a un agente di QUESTA città di lavorare su un ambito.
    /// <b>Nessun run parte</b> finché l'umano non la approva — è il guardrail della federazione.
    /// Su approvazione si apre una conversazione locale (con <see cref="FederationId"/>) e
    /// l'agente bersaglio viene svegliato; su rifiuto non gira nulla.
    /// </summary>
    public class FederationRequest
    {
        /// <summary>PK GuidComb — mai pre-assegnare.</summary>
        public virtual Guid Id { get; set; }

        /// <summary>Correlazione col lato d'origine (stessa <see cref="AgentConversation.FederationId"/>).</summary>
        public virtual Guid FederationId { get; set; }

        /// <summary>
        /// Idempotency key della singola emissione d'origine (§12.6): dedup delle redelivery del
        /// relay. Diverso dal <see cref="FederationId"/> (che è per-conversazione): due interventi
        /// distinti hanno RequestId diversi anche a parità di testo. <c>null</c> per le righe
        /// pre-esistenti (dedup allora ricade sul vecchio criterio).
        /// </summary>
        public virtual Guid? RequestId { get; set; }

        /// <summary>Progetto locale bersaglio (dove vive l'agente richiesto).</summary>
        public virtual string ProjectPath { get; set; }

        /// <summary>Padrone della città d'origine (<c>gitEmail</c>/ownerId remoto).</summary>
        public virtual string FromOwner { get; set; }

        /// <summary>Agente remoto che ha originato la richiesta (nome, informativo).</summary>
        public virtual string FromAgent { get; set; }

        /// <summary>Agente locale che dovrebbe intervenire (proposto; l'umano può cambiarlo).</summary>
        public virtual string TargetAgent { get; set; }

        /// <summary>Ambito di ownership della richiesta (routing hint).</summary>
        public virtual string Scope { get; set; }

        /// <summary>Testo della richiesta (DATO, non ordine — passa dai delimitatori al risveglio).</summary>
        public virtual string Message { get; set; }

        /// <summary>Argomenti dichiarati, uno per riga (come <see cref="AgentMessage.Topics"/>).</summary>
        public virtual string Topics { get; set; }

        /// <summary>
        /// Fase 7d.5 — riferimento di handoff: branch ref COMPLETO (<c>agent/&lt;A&gt;/&lt;id&gt;</c>)
        /// col lavoro dell'origine, già pushato. <c>null</c> = nessun handoff. Copiato sull'
        /// <see cref="AgentConversation"/> all'approvazione, poi usato da B per sincronizzarsi.
        /// </summary>
        public virtual string HandoffRef { get; set; }

        /// <summary>Fase 7d.5 — sha di testa a cui B deve sincronizzarsi (testa di <see cref="HandoffRef"/>).</summary>
        public virtual string BaseCommit { get; set; }

        /// <summary>"pending" | "approved" | "rejected".</summary>
        public virtual string Status { get; set; }

        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? DecidedAt { get; set; }

        /// <summary>Valori ammessi per <see cref="Status"/>.</summary>
        public static class StatusEnum
        {
            public const string Pending = "pending";
            public const string Approved = "approved";
            public const string Rejected = "rejected";
        }
    }
}
