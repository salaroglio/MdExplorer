using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.Agents;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.Services.AgentRegistry
{
    /// <summary>
    /// Cittadino algoritmico di <b>diagnostica</b> del gateway A2A (§4, §6): risponde
    /// con un eco del messaggio ricevuto. Serve a verificare il loop A2A end-to-end
    /// (message/send → dispatch in-process → task completed) senza spendere token LLM —
    /// il primo <see cref="IAlgorithmicAgent"/> del sistema, arruolato come gli altri.
    /// Resta soggetto a trust come ogni cittadino.
    /// </summary>
    public class PingAlgorithmicAgent : IAlgorithmicAgent
    {
        public AgentCardInfo GetCard() => new AgentCardInfo
        {
            Name = "a2a-ping",
            Role = "Diagnostica del gateway A2A (eco del messaggio)",
            Skills = new List<AgentCardSkillInfo>
            {
                new AgentCardSkillInfo
                {
                    Id = "ping",
                    Description = "Risponde con un eco del testo ricevuto, per verificare il loop A2A.",
                },
            },
        };

        public Task<AgentTaskResult> ExecuteAsync(AgentTaskContext ctx, CancellationToken ct)
        {
            var received = ctx?.Message ?? string.Empty;
            return Task.FromResult(AgentTaskResult.Ok($"pong: {received}"));
        }
    }
}
