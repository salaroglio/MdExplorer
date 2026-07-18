namespace MdExplorer.Features.Federation
{
    /// <summary>
    /// L'esito che la città bersaglio riporta all'origine con l'<c>intervention-result</c>
    /// (Fase 7a). <b>Campo macchina</b>: usa ESATTAMENTE queste stringhe ovunque (endpoint di
    /// ritorno, receiver, e il feedback-loop di memoria 7b). Le città possono girare versioni
    /// diverse: il contratto è la stringa, non l'enum.
    /// </summary>
    public static class FederationVerdict
    {
        /// <summary>Lavoro concluso con successo → 7b: rinforzo della memoria d'origine.</summary>
        public const string Success = "success";

        /// <summary>Rifiutato (non di competenza, fuori ambito) → 7b: erosione (+ fatto sul CHI).</summary>
        public const string Rejected = "rejected";

        /// <summary>Non pronto (precondizione mancante, conflitto) → 7b: erosione leggera + fatto sul COSA.</summary>
        public const string NotReady = "not-ready";
    }

    /// <summary>
    /// I <c>Reason</c> codificati che accompagnano un verdict <see cref="FederationVerdict.NotReady"/>
    /// (o <see cref="FederationVerdict.Rejected"/>). <b>Campo macchina</b> (diventano <c>aboutTag</c>
    /// nella memoria 7b): stringhe esatte, condivise tra le fasi che le producono (7c.2/7d.5) e
    /// chi le consuma (7b). <see cref="PreconditionPrefix"/> è un prefisso: <c>precondition:&lt;x&gt;</c>.
    /// </summary>
    public static class FederationReason
    {
        /// <summary>Il bersaglio non si riconosce competente sull'ambito (<see cref="FederationVerdict.Rejected"/>).</summary>
        public const string NotForMe = "not-for-me";

        /// <summary>Merge del branch di handoff in conflitto con main (7c/7d) → <see cref="FederationVerdict.NotReady"/>.</summary>
        public const string MergeConflictWithMain = "merge-conflict-with-main";

        /// <summary>Sync al ref/commit di handoff fallito (7d.5) → <see cref="FederationVerdict.NotReady"/>.</summary>
        public const string GitSyncFailed = "git-sync-failed";

        /// <summary>Prefisso per una precondizione mancante specifica: <c>precondition:&lt;x&gt;</c>.</summary>
        public const string PreconditionPrefix = "precondition:";
    }
}
