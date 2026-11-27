using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Un punto del riassunto di una chat (il <c>"point"</c> del blocco di fine turno), con dove era il repository quando è
    /// stato scritto: il ramo e lo SHA dell'ultimo commit. Ramo + SHA dicono quale attività ha cambiato il contenuto che sta per
    /// essere registrato: «Genera descrizione» del commit prende i punti con il ramo e l'HEAD di adesso, cioè il lavoro fatto
    /// dopo l'ultimo commit. Ramo e SHA non si mostrano; nella cronologia il ramo compare solo come etichetta.
    /// <para>Le chat di prima (solo <see cref="ChatThread.Detail"/>) restano come sono: nessun recupero.</para>
    /// </summary>
    public class ChatThreadPoint
    {
        /// <summary>GuidComb: l'ordine di inserimento è anche l'ordine dei punti.</summary>
        public virtual Guid Id { get; set; }
        public virtual Guid ThreadId { get; set; }
        public virtual string ProjectPath { get; set; }
        public virtual string Point { get; set; }
        public virtual DateTime At { get; set; }

        /// <summary>Il ramo del repository del progetto a fine turno; null fuori da git o con HEAD staccato.</summary>
        public virtual string Branch { get; set; }

        /// <summary>Lo SHA di HEAD a fine turno; null fuori da git o in un repository senza commit.</summary>
        public virtual string HeadSha { get; set; }
    }
}
