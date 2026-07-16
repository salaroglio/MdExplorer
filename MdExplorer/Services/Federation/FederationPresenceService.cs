using System;
using System.Collections.Generic;
using System.Text.Json;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Federation;
using MdExplorer.Services;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Federation
{
    /// <summary>L'annuncio cifrato pronto da spedire al relay per una stanza-repo.</summary>
    public sealed class FederationAnnounce
    {
        public string RoomId { get; init; }
        public string RelayUrl { get; init; }
        /// <summary>Presenza serializzata e cifrata col room secret (busta chiusa, R15).</summary>
        public string EncryptedPresence { get; init; }
    }

    /// <summary>
    /// Assembla l'annuncio di presenza cifrato per un progetto (§12.5 / Fase 6b) — SENZA rete.
    /// Legge l'attivazione da <c>.development.yml</c> (Fase 6a), calcola la stanza dall'origin
    /// git, costruisce la presenza (padrone + agenti trusted) e la cifra col room secret. È
    /// il "cosa annunciamo": il <see cref="FederationRelayService"/> lo prende e lo spedisce.
    /// </summary>
    public interface IFederationPresenceService
    {
        /// <summary>
        /// Annuncio per il progetto, o <c>null</c> se la città è spenta o manca l'origin git
        /// (senza remoto non c'è stanza). Fail-loud se attiva ma senza room secret.
        /// </summary>
        FederationAnnounce BuildAnnounce(
            string projectPath, string gitOrigin, string gitEmail,
            IReadOnlyList<AgentRosterEntry> trustedAgents);
    }

    public class FederationPresenceService : IFederationPresenceService
    {
        // Relay di default (errantia.net); override per-progetto via agentCity.relayUrl.
        public const string DefaultRelayUrl = "wss://errantia.net";

        private readonly IProjectMetadataService _metadata;
        private readonly ILogger<FederationPresenceService> _logger;

        public FederationPresenceService(IProjectMetadataService metadata, ILogger<FederationPresenceService> logger)
        {
            _metadata = metadata;
            _logger = logger;
        }

        public FederationAnnounce BuildAnnounce(
            string projectPath, string gitOrigin, string gitEmail,
            IReadOnlyList<AgentRosterEntry> trustedAgents)
        {
            var cfg = _metadata.GetAgentCity(projectPath);
            if (cfg == null || !cfg.Enabled)
                return null;                                   // città spenta → nessun annuncio

            if (string.IsNullOrWhiteSpace(gitOrigin))
            {
                _logger.LogWarning("[Federation] '{Project}' ha la città attiva ma nessun origin git: niente stanza, niente presenza.", projectPath);
                return null;
            }
            if (string.IsNullOrWhiteSpace(gitEmail))
            {
                _logger.LogWarning("[Federation] '{Project}' senza user.email git: impossibile annunciare il padrone.", projectPath);
                return null;
            }

            // Precondizione dura (REGOLA #2): città attiva senza room secret è uno stato rotto.
            if (string.IsNullOrWhiteSpace(cfg.RoomSecret))
                throw new InvalidOperationException(
                    $"Città attiva per '{projectPath}' ma room secret assente in .development.yml: riattivare la città per rigenerarlo.");

            var roomId = FederationRoom.ComputeRoomId(gitOrigin);
            var presence = FederationPresenceBuilder.Build(roomId, gitEmail, trustedAgents);
            var json = JsonSerializer.Serialize(presence);
            var envelope = FederationCrypto.Encrypt(cfg.RoomSecret, roomId, json);

            return new FederationAnnounce
            {
                RoomId = roomId,
                RelayUrl = string.IsNullOrWhiteSpace(cfg.RelayUrl) ? DefaultRelayUrl : cfg.RelayUrl.Trim(),
                EncryptedPresence = envelope,
            };
        }
    }
}
