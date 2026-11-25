using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Una proposta di «Impara da questa attività» e la sua sorte (sprint 2026-11-24-Skill-Che-Migliorano, F4, U5): una riga
    /// per proposta. Serve a non riproporre una cosa rifiutata e a far rivedere al modello, all'«Impara» dopo, quelle rimaste in
    /// sospeso. Il testo della skill scritta sta nel repository; qui il testo proposto (o corretto dall'utente).
    /// </summary>
    public class ChatLearning
    {
        public const string Pending = "pending";
        public const string Approved = "approved";
        /// <summary>Approvata con il testo corretto dall'utente.</summary>
        public const string Corrected = "corrected";
        public const string Rejected = "rejected";
        /// <summary>Era in sospeso, e un «Impara» successivo nella stessa chat l'ha rivista.</summary>
        public const string Superseded = "superseded";

        public virtual Guid Id { get; set; }
        public virtual Guid ThreadId { get; set; }
        public virtual string ProjectPath { get; set; }
        /// <summary>create o update.</summary>
        public virtual string Action { get; set; }
        public virtual string Skill { get; set; }
        public virtual string Why { get; set; }
        /// <summary>I progressivi delle prove, separati da virgola.</summary>
        public virtual string Evidence { get; set; }
        public virtual string Content { get; set; }
        /// <summary>Relativo al progetto: dove si scrive (o si è scritta) la skill.</summary>
        public virtual string TargetPath { get; set; }
        public virtual string Outcome { get; set; }
        /// <summary>La versione <c>learned:</c> scritta, per le approvate.</summary>
        public virtual int? WrittenVersion { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? DecidedAt { get; set; }
    }
}
