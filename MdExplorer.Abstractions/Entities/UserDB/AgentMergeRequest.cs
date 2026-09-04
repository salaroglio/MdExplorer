using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Un agente ha finito un lavoro documentale e <b>chiede</b> di fonderlo nel ramo principale.
    /// <para>
    /// Prima questo passaggio era automatico (Fase 7g): un gate meccanico approvava e il
    /// deliverable entrava in main da solo. Ora il cancello torna umano — il gate resta, ma il
    /// suo «sì» <b>propone</b> invece di fondere.
    /// </para>
    /// <para>
    /// Vive in UserDB e non è derivata dai branch perché serve ricordare ciò che i branch non
    /// sanno: che una richiesta è <b>già stata vista e rifiutata</b>. Senza, ogni rifiuto
    /// tornerebbe in cima alla lista al giro successivo.
    /// </para>
    /// </summary>
    public class AgentMergeRequest
    {
        public virtual Guid Id { get; set; }

        public virtual string ProjectPath { get; set; }

        /// <summary>Agente che ha prodotto il lavoro (<c>a2a.name</c>).</summary>
        public virtual string AgentName { get; set; }

        /// <summary>Nome pubblicato su origin: parlante, è quello da mostrare all'umano.</summary>
        public virtual string PublishedBranch { get; set; }

        /// <summary>Nome locale del branch d'attività: serve al merge, che è un'operazione in casa.</summary>
        public virtual string LocalBranch { get; set; }

        public virtual string HeadSha { get; set; }

        /// <summary>
        /// File toccati, uno per riga, nella forma <c>&lt;stato&gt;\t&lt;percorso&gt;</c> di
        /// <c>git diff --name-status</c> (A/M/D). Fotografati al momento della richiesta: è ciò
        /// che l'umano deve guardare per decidere, e non deve cambiare sotto i suoi occhi.
        /// </summary>
        public virtual string ChangedFiles { get; set; }

        public virtual DateTime CreatedAt { get; set; }

        public virtual DateTime? DecidedAt { get; set; }

        /// <summary>pending / approved / rejected / merged / failed.</summary>
        public virtual string Status { get; set; }

        /// <summary>Perché è stata rifiutata, o perché il merge è fallito.</summary>
        public virtual string Note { get; set; }

        public static class StatusEnum
        {
            public const string Pending = "pending";
            public const string Merged = "merged";
            /// <summary>L'umano ha detto no: il branch resta, il lavoro non è distrutto.</summary>
            public const string Rejected = "rejected";
            /// <summary>Autorizzata ma il merge non è riuscito (tipicamente un conflitto).</summary>
            public const string Failed = "failed";
        }
    }
}
