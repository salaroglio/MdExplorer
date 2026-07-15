using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Il thread di una conversazione tra cittadini (§8 Agent-Harness-A2A). Correlata 1:1
    /// con il <c>contextId</c> A2A: l'id (<see cref="Id"/>) è il contextId, così i messaggi
    /// in ingresso si agganciano al thread. Vive nella UserDB globale.
    /// </summary>
    public class AgentConversation
    {
        /// <summary>PK = conversationId A2A (contextId). GuidComb — mai pre-assegnare.</summary>
        public virtual Guid Id { get; set; }

        public virtual string ProjectPath { get; set; }

        /// <summary>Chi ha avviato il thread: <c>a2a.name</c> di un agente oppure <c>user</c>.</summary>
        public virtual string StartedBy { get; set; }

        /// <summary>"active" | "completed" | "killed" | "exhausted".</summary>
        public virtual string Status { get; set; }

        /// <summary>
        /// Conteggio hop (§9): incrementato a ogni messaggio tra agenti, fan-out incluso;
        /// i messaggi da/verso <c>user</c> sono esenti. <c>HopCount &gt;= HopLimit</c> →
        /// conversazione <c>exhausted</c>.
        /// </summary>
        public virtual int HopCount { get; set; }

        /// <summary>Limite hop: default 8, override da <c>max_hops</c> (cap harness 16).</summary>
        public virtual int HopLimit { get; set; }

        public virtual DateTime StartedAt { get; set; }
        public virtual DateTime LastActivityAt { get; set; }

        /// <summary>Valori ammessi per <see cref="Status"/>.</summary>
        public static class StatusEnum
        {
            public const string Active = "active";
            public const string Completed = "completed";
            public const string Killed = "killed";
            public const string Exhausted = "exhausted";
        }
    }
}
