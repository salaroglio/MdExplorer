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

        /// <summary>
        /// Istante in cui l'<b>umano</b> ha visto/gestito questo messaggio dalla UI (§13 Fase 4).
        /// Rilevante solo per i messaggi <c>ToAgent == user</c>: <c>null</c> = non letto (entra
        /// nel badge non-letti della inbox); valorizzato quando l'utente lo apre o risponde.
        /// Ortogonale a <see cref="State"/> (che è il ciclo di consegna del dispatcher).
        /// </summary>
        public virtual DateTime? ReadAt { get; set; }

        /// <summary>
        /// Motivo per cui la consegna è <b>parcheggiata</b> (§12.5 coda differita, Fase 6c):
        /// l'agente non è eseguibile adesso ma la richiesta NON fallisce. Valori
        /// <see cref="DeferredReasonEnum"/> (<c>resources</c>/<c>maintenance</c>/<c>user</c>).
        /// <c>null</c> = non differito. Il messaggio resta <c>pending</c> e viene ripreso via
        /// <see cref="NextAttemptAt"/> quando la condizione si libera; il parcheggio
        /// <b>non consuma <see cref="Attempts"/></b> (come lo shutdown, §7) — diverso da "fallito".
        /// </summary>
        public virtual string DeferredReason { get; set; }

        /// <summary>
        /// L'umano ha chiesto il "forza-ora" dalla coda (§12.5 Fase 6d): finché il messaggio
        /// non si conclude, il dispatcher <b>salta i differimenti di politica</b>
        /// (maintenance/user) — altrimenti la leva sarebbe un no-op silenzioso, perché la
        /// policy rileggerebbe la stessa condizione e riparcheggerebbe subito. Il tetto
        /// risorse (Copilot) resta: uno slot non si può forzare. <c>null</c> = non forzato.
        /// </summary>
        public virtual DateTime? ForcedAt { get; set; }

        /// <summary>Valori ammessi per <see cref="State"/>.</summary>
        public static class StateEnum
        {
            public const string Pending = "pending";
            public const string Delivered = "delivered";
            public const string Processed = "processed";
            public const string Failed = "failed";
        }

        /// <summary>Cause del parcheggio (§12.5). Prefisso <c>deferred:</c> nella UI.</summary>
        public static class DeferredReasonEnum
        {
            /// <summary>Tetto istanze Copilot raggiunto: nessuno slot libero adesso.</summary>
            public const string Resources = "resources";
            /// <summary>Agente in manutenzione (WIP), segnalato al team via <c>.development.yml</c>.</summary>
            public const string Maintenance = "maintenance";
            /// <summary>Condizione temporanea dell'utente su questa macchina (UserDB).</summary>
            public const string User = "user";
        }
    }
}
