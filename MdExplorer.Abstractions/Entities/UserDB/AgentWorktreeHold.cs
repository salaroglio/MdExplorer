using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// «Questo worktree è in mano all'umano»: marcatore che vieta la pulizia distruttiva.
    /// <para>
    /// Nasce dalla revisione del lavoro degli agenti: l'umano che non autorizza un merge apre la
    /// directory del worktree e ci mette mano. Ma quella è l'area di lavoro <b>viva</b>
    /// dell'agente, e al risveglio successivo <c>PrepareForRunAsync</c> fa <c>reset --hard</c> e
    /// <c>clean -fd</c> — cancellando modifiche non committate, senza chiedere e senza recupero.
    /// </para>
    /// <para>
    /// È la <b>seconda</b> difesa, non la prima: la prima è mettere in pausa l'agente
    /// (<see cref="AgentPause"/>). Questa esiste perché una pausa la si dimentica o la si toglie,
    /// e una dimenticanza non deve poter distruggere lavoro.
    /// </para>
    /// </summary>
    public class AgentWorktreeHold
    {
        public virtual Guid Id { get; set; }

        /// <summary>Progetto proprietario del worktree.</summary>
        public virtual string ProjectPath { get; set; }

        /// <summary>Agente il cui worktree è trattenuto (<c>a2a.name</c>).</summary>
        public virtual string AgentName { get; set; }

        public virtual DateTime CreatedAt { get; set; }

        /// <summary>Perché è stato preso, per dirlo all'agente e all'umano che l'ha dimenticato.</summary>
        public virtual string Reason { get; set; }
    }
}
