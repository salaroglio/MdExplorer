using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Un avviso «lavoro non processato» già dato alla persona di questo computer (17/10/2026): uno per lavoro, anche dopo
    /// un riavvio. <see cref="Key"/> dice quale lavoro: <c>dispatch:&lt;RequestId&gt;</c> per una richiesta a un altro
    /// computer, <c>round:&lt;giro&gt;:&lt;passo&gt;:&lt;giro del ciclo&gt;:&lt;tentativo&gt;</c> per un passo di un giro.
    /// </summary>
    public class OverdueWorkNotice
    {
        /// <summary>PK GuidComb — mai pre-assegnare.</summary>
        public virtual Guid Id { get; set; }
        public virtual string ProjectPath { get; set; }
        public virtual string Key { get; set; }
        public virtual DateTime NotifiedAt { get; set; }
    }
}
