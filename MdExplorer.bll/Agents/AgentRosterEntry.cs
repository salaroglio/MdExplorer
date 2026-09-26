using System.Collections.Generic;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Una voce della "rubrica" iniettata nel prompt al risveglio di un agente (§6):
    /// nome, ruolo e skill di un collega — <b>non</b> tutta la card. È lo stesso
    /// pattern con cui un harness inietta gli agent type disponibili. La rubrica
    /// contiene tutti gli agenti <i>trusted</i> del progetto (tranne sé stesso); il
    /// filtro fine (<c>accepts_messages_from</c>) lo fa il destinatario all'invio.
    /// </summary>
    public class AgentRosterEntry
    {
        public string Name { get; set; }
        public string Role { get; set; }

        /// <summary>Id delle skill dichiarate (solo i titoli, non le descrizioni).</summary>
        public IList<string> Skills { get; set; } = new List<string>();
    }
}
