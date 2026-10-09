using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Il ledger <b>lato origine</b> di una richiesta di intervento federata (Fase 7a — il
    /// cerchio di ritorno). Gemello di <see cref="FederationRequest"/> (che è il ledger lato
    /// destinazione): quando questa città smista un intervento a un'altra, ne registra qui una
    /// riga <c>pending</c>. All'arrivo dell'<c>intervention-result</c> la si correla per
    /// <see cref="RequestId"/>, si marca <c>completed</c> e si <b>risveglia l'agente d'origine</b>
    /// (<see cref="OriginAgent"/>) con l'esito. Un result con <see cref="RequestId"/> sconosciuto
    /// non ha riga qui → viene scartato (filtro anti-avvelenamento).
    /// </summary>
    public class FederationDispatch
    {
        /// <summary>PK GuidComb — mai pre-assegnare.</summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Idempotency key della singola emissione (§12.6): è la <b>chiave di correlazione</b>
        /// con l'esito che tornerà. Stesso valore inviato nel payload federato d'origine.
        /// </summary>
        public virtual Guid RequestId { get; set; }

        /// <summary>Correlazione per-conversazione (stesso <see cref="AgentConversation.FederationId"/>).</summary>
        public virtual Guid FederationId { get; set; }

        /// <summary>Progetto locale d'origine (dove vive l'agente che ha smistato).</summary>
        public virtual string ProjectPath { get; set; }

        /// <summary>La conversazione d'ORIGINE su questa città, da riprendere all'arrivo dell'esito.</summary>
        public virtual Guid ConversationId { get; set; }

        /// <summary>
        /// L'agente locale da <b>risvegliare</b> quando l'esito torna: è
        /// <c>claims.AgentName</c> catturato al dispatch. NON ricavarlo dopo da
        /// <see cref="AgentConversation.StartedBy"/> (che nelle conversazioni vale spesso
        /// <c>user</c>).
        /// </summary>
        public virtual string OriginAgent { get; set; }

        /// <summary>Padrone della città bersaglio (ownerId derivato dalla gitEmail dell'ownership).</summary>
        public virtual string TargetOwner { get; set; }

        /// <summary>Agente remoto proposto come bersaglio (informativo).</summary>
        public virtual string TargetAgent { get; set; }

        /// <summary>Argomenti dichiarati, uno per riga (come <see cref="AgentMessage.Topics"/>).</summary>
        public virtual string Topics { get; set; }

        /// <summary>"pending" | "completed" (vedi <see cref="StatusEnum"/>).</summary>
        public virtual string Status { get; set; }

        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? CompletedAt { get; set; }

        /// <summary>Valori ammessi per <see cref="Status"/>.</summary>
        public static class StatusEnum
        {
            public const string Pending = "pending";
            public const string Completed = "completed";
        }
    }
}
