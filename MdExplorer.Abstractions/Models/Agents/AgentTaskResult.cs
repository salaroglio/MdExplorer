namespace MdExplorer.Abstractions.Models.Agents
{
    /// <summary>
    /// Esito dell'esecuzione di un agente algoritmico (§7 del design doc).
    /// Mappa gli stati terminali del task A2A: successo → <c>completed</c>,
    /// fallimento → <c>failed</c> con dettaglio. Fail-loud: un fallimento porta
    /// sempre con sé il proprio <see cref="Error"/>, mai un successo finto.
    /// </summary>
    public class AgentTaskResult
    {
        /// <summary>True se il task si è concluso con successo (<c>completed</c>).</summary>
        public bool Success { get; set; }

        /// <summary>Output prodotto dal task: diventa il risultato del task A2A.</summary>
        public string Output { get; set; }

        /// <summary>Dettaglio dell'errore quando <see cref="Success"/> è false (<c>failed</c>).</summary>
        public string Error { get; set; }

        /// <summary>Task concluso con successo.</summary>
        public static AgentTaskResult Ok(string output)
            => new AgentTaskResult { Success = true, Output = output };

        /// <summary>Task fallito, con motivo esplicito.</summary>
        public static AgentTaskResult Fail(string error)
            => new AgentTaskResult { Success = false, Error = error };
    }
}
