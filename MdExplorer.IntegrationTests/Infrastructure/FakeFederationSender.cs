using System.Threading.Tasks;
using MdExplorer.Services.Federation;

namespace MdExplorer.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Sostituisce il vero <see cref="IFederationSender"/> (che richiede una connessione relay
    /// viva) nei test: cattura l'ultima richiesta federata spedita, così l'endpoint
    /// RequestIntervention è esercitabile end-to-end senza rete.
    /// </summary>
    public sealed class FakeFederationSender : IFederationSender
    {
        public string LastProjectPath { get; private set; }
        public string LastTargetOwnerId { get; private set; }
        public FederatedRequestPayload LastPayload { get; private set; }

        /// <summary>Esito che il fake restituisce (default: consegnato).</summary>
        public bool Result { get; set; } = true;

        public Task<bool> SendFederatedRequestAsync(string projectPath, string targetOwnerId, FederatedRequestPayload payload)
        {
            LastProjectPath = projectPath;
            LastTargetOwnerId = targetOwnerId;
            LastPayload = payload;
            return Task.FromResult(Result);
        }
    }
}
