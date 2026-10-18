using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Identità persistente di un cittadino della "città degli agenti" (§6 del design
    /// doc Agent-Harness-A2A). Il registry in-memory si ricostruisce a ogni avvio, ma
    /// <b>trust e identità devono sopravvivere</b>: vivono qui, nella UserDB globale.
    /// <para>
    /// L'identità stabile è <see cref="Id"/> (Guid), non il nome: i named graph di
    /// memoria (§11) sono keyed su <see cref="Id"/>, così rinominare un agente non
    /// rende orfana la sua memoria. Il nome è indirizzamento e display.
    /// </para>
    /// </summary>
    public class AgentIdentity
    {
        /// <summary>PK. Generata da NHibernate (GuidComb) — mai pre-assegnare.</summary>
        public virtual Guid Id { get; set; }

        /// <summary>Progetto di appartenenza (scope della cittadinanza).</summary>
        public virtual string ProjectPath { get; set; }

        /// <summary>Dal frontmatter <c>a2a.name</c>. UNIQUE per progetto.</summary>
        public virtual string AgentName { get; set; }

        /// <summary>Path del <c>.agent.md</c>; null per gli agenti algoritmici.</summary>
        public virtual string AgentFilePath { get; set; }

        /// <summary>"llm" | "algorithmic".</summary>
        public virtual string Kind { get; set; }

        /// <summary>
        /// Partecipare alle conversazioni richiede trust esplicito (conferma UI,
        /// stesso dialogo delle schedule).
        /// </summary>
        public virtual bool Trusted { get; set; }

        /// <summary>Vincolo Enabled ⇒ Trusted, come <see cref="AgentSchedule"/>.</summary>
        public virtual bool Enabled { get; set; }

        /// <summary>
        /// Hash del blocco <c>a2a:</c> <b>+ campo <c>tools:</c></b> calcolato al momento
        /// del trust (R3, §10). Se cambiano, <see cref="Trusted"/> decade e va
        /// riconfermato (anti-escalation). Null finché non c'è un trust confermato.
        /// </summary>
        public virtual string A2ABlockHash { get; set; }

        /// <summary>
        /// Motivo fail-loud dell'esclusione dal registry (nome duplicato, blocco
        /// <c>a2a:</c> malformato, ...). Visibile in UI, stesso spirito del
        /// <see cref="AgentSchedule.DisabledReason"/>.
        /// </summary>
        public virtual string RegistrationError { get; set; }

        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime UpdatedAt { get; set; }

        /// <summary>Valori ammessi per <see cref="Kind"/>.</summary>
        public static class KindEnum
        {
            public const string Llm = "llm";
            public const string Algorithmic = "algorithmic";
        }
    }
}
