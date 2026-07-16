using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Un singolo messaggio nella mailbox (§8 Agent-Harness-A2A). Lo <b>stato autoritativo</b>
    /// del task A2A vive qui, nel DB: i task in-memory dell'SDK si ricostruiscono da queste
    /// righe. Il dispatcher garantisce la consegna at-least-once.
    /// </summary>
    public class AgentMessage
    {
        public virtual Guid Id { get; set; }

        /// <summary>FK logica verso <see cref="AgentConversation.Id"/> (il thread).</summary>
        public virtual Guid ConversationId { get; set; }

        /// <summary>Correlazione col task A2A.</summary>
        public virtual string A2ATaskId { get; set; }

        /// <summary>Mittente: <c>a2a.name</c> di un agente oppure <c>user</c>.</summary>
        public virtual string FromAgent { get; set; }

        /// <summary>Destinatario: <c>a2a.name</c> di un agente oppure <c>user</c>.</summary>
        public virtual string ToAgent { get; set; }

        public virtual string ProjectPath { get; set; }

        /// <summary>Testo del messaggio (Parts A2A serializzate).</summary>
        public virtual string Body { get; set; }

        /// <summary>
        /// Argomenti dichiarati dal mittente (§8): metadata di contesto, uno per riga.
        /// Nessun entity-linking — è solo contesto passato all'agente destinatario. Null/vuoto
        /// = nessun argomento.
        /// </summary>
        public virtual string Topics { get; set; }

        /// <summary>"pending" | "delivered" | "processed" | "failed".</summary>
        public virtual string State { get; set; }

        /// <summary>Tentativi di consegna (per il backoff del dispatcher).</summary>
        public virtual int Attempts { get; set; }

        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Primo istante in cui il messaggio può essere riprovato dopo un fallimento (backoff
        /// temporizzato). Null = subito idoneo. Il dispatcher salta i pending con
        /// <c>NextAttemptAt</c> ancora nel futuro, così i tentativi si distanziano nel tempo
        /// invece di bruciarsi tutti in pochi secondi.
        /// </summary>
        public virtual DateTime? NextAttemptAt { get; set; }

        public virtual string Error { get; set; }

        /// <summary>Valori ammessi per <see cref="State"/>.</summary>
        public static class StateEnum
        {
            public const string Pending = "pending";
            public const string Delivered = "delivered";
            public const string Processed = "processed";
            public const string Failed = "failed";
        }
    }
}
