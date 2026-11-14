using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Un giro del workflow che la persona ha archiviato nella sua posta («La posta in ordine», F3): esce dalla sezione «Giri»
    /// e va nell'archivio, con i suoi messaggi. È della persona, non del progetto: il registro del giro (in git) non cambia.
    /// </summary>
    public class ArchivedRound
    {
        public virtual Guid Id { get; set; }
        public virtual string ProjectPath { get; set; }
        /// <summary>L'identificativo del giro nel registro (per esempio <c>2026-10-07-gara-e3cad8</c>).</summary>
        public virtual string RoundId { get; set; }
        public virtual DateTime ArchivedAt { get; set; }
    }
}
