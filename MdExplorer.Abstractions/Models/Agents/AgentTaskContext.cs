using System.Collections.Generic;

namespace MdExplorer.Abstractions.Models.Agents
{
    /// <summary>
    /// Contesto passato a un agente algoritmico quando viene svegliato da un task
    /// A2A in ingresso (§7 del design doc). L'invocazione è in-process, ma la forma
    /// del contesto rispecchia un messaggio A2A perché "stessa cittadinanza": un
    /// algoritmo riceve gli stessi dati di un agente LLM.
    /// <para>
    /// In Fase 1 nessuno costruisce ancora questo contesto (il trigger <c>message</c>
    /// arriva in Fase 3): il tipo esiste perché la superficie di
    /// <see cref="Services.IAlgorithmicAgent"/> sia stabile e le implementazioni si
    /// possano registrare fin da subito.
    /// </para>
    /// </summary>
    public class AgentTaskContext
    {
        /// <summary>Progetto in cui l'agente opera (scope di ogni cittadinanza).</summary>
        public string ProjectPath { get; set; }

        /// <summary>Conversazione A2A (contextId) a cui il task appartiene.</summary>
        public string ConversationId { get; set; }

        /// <summary>Id del task A2A in ingresso (correlazione con la mailbox).</summary>
        public string A2ATaskId { get; set; }

        /// <summary>Mittente del messaggio: <c>a2a.name</c> di un agente oppure <c>user</c>.</summary>
        public string FromAgent { get; set; }

        /// <summary>Testo del messaggio che ha svegliato l'agente.</summary>
        public string Message { get; set; }

        /// <summary>
        /// Argomenti dichiarati dal mittente (§8, rischio R8): risorse/termini di cui
        /// parla il messaggio. Deterministici, niente entity-linking.
        /// </summary>
        public IList<string> Topics { get; set; } = new List<string>();
    }
}
