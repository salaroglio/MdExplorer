using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Il <b>gate del push umano</b> per il codice (Fase 7e, §6bis): un agente ha toccato un
    /// submodule (codice) nel suo worktree, ma sul codice il git è <b>in mano all'umano</b>
    /// (l'agente NON committa/pusha il submodule). Finché questa riga è aperta
    /// (<see cref="ResolvedAt"/> null) i dispatch verso gli agenti del progetto sono differiti
    /// (<c>awaiting-push</c>): il codice non è ancora atterrato. Quando l'umano committa, si
    /// cattura lo sha del submodule (<see cref="SubmoduleBaseCommit"/> = release token) e la
    /// riga si chiude, sbloccando la catena.
    /// </summary>
    public class SubmoduleAwaitingPush
    {
        /// <summary>PK GuidComb — mai pre-assegnare.</summary>
        public virtual Guid Id { get; set; }

        /// <summary>Progetto (superprogetto-doc) che contiene il submodule.</summary>
        public virtual string ProjectPath { get; set; }

        /// <summary>Path relativo del submodule toccato (dal <c>.gitmodules</c>).</summary>
        public virtual string Submodule { get; set; }

        /// <summary>Agente che ha toccato il codice (informativo).</summary>
        public virtual string TouchedByAgent { get; set; }

        /// <summary>Worktree dove il tocco è stato rilevato (informativo).</summary>
        public virtual string WorktreePath { get; set; }

        public virtual DateTime CreatedAt { get; set; }

        /// <summary>Sha del submodule catturato al commit umano (release token, Fase 7e.4). Null finché aperto.</summary>
        public virtual string SubmoduleBaseCommit { get; set; }

        /// <summary>Istante di chiusura del gate (il codice è atterrato). Null = ancora in attesa del push.</summary>
        public virtual DateTime? ResolvedAt { get; set; }
    }
}
