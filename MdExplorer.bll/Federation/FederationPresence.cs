using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Agents;

namespace MdExplorer.Features.Federation
{
    /// <summary>
    /// Un agente nel catalogo sintetico che una città annuncia (§12.5): solo
    /// nome/ruolo/skill — la <b>card della rubrica</b>, mai il file o i tool. È ciò che le
    /// altre città vedono di un cittadino trusted, non abbastanza per impersonarlo.
    /// </summary>
    public sealed class FederatedAgentSummary
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
    }

    /// <summary>
    /// L'annuncio di presenza di una città (§12.5): chi è il padrone (<c>gitEmail</c>) e
    /// quali agenti <b>trusted</b> offre, per una stanza-repo. Presenza ≠ macchinario acceso:
    /// è un annuncio leggero. Viene serializzato, <b>cifrato col room secret</b>
    /// (<see cref="FederationCrypto"/>) e spedito al relay, che custodisce solo buste chiuse.
    /// </summary>
    public sealed class CityPresence
    {
        /// <summary>Versione dello schema di presenza (per evoluzioni future).</summary>
        public string V { get; set; } = "1";

        /// <summary>Stanza-repo (SHA256 dell'origin) — ridondante col canale, utile a validare.</summary>
        public string RoomId { get; set; }

        /// <summary>Email git del padrone della città (chiave d'identità, §12.3).</summary>
        public string GitEmail { get; set; }

        /// <summary>Id derivato del padrone (SHA256 email), per riferimenti compatti.</summary>
        public string OwnerId { get; set; }

        /// <summary>Catalogo sintetico degli agenti trusted offerti.</summary>
        public List<FederatedAgentSummary> Agents { get; set; } = new List<FederatedAgentSummary>();
    }

    /// <summary>Costruisce l'annuncio di presenza da dati già risolti. Puro → testabile.</summary>
    public static class FederationPresenceBuilder
    {
        /// <summary>
        /// Assembla la presenza per una stanza: padrone + catalogo degli agenti trusted
        /// (name/role/skills). Scarta gli agenti senza nome. Fail-loud sugli input mancanti.
        /// </summary>
        public static CityPresence Build(string roomId, string gitEmail, IReadOnlyList<AgentRosterEntry> trustedAgents)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentException("roomId assente: senza stanza non si può annunciare presenza.", nameof(roomId));
            if (string.IsNullOrWhiteSpace(gitEmail))
                throw new ArgumentException("gitEmail assente: la presenza richiede l'identità del padrone.", nameof(gitEmail));

            var email = gitEmail.Trim().ToLowerInvariant();
            var agents = (trustedAgents ?? Array.Empty<AgentRosterEntry>())
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Name))
                .Select(a => new FederatedAgentSummary
                {
                    Name = a.Name.Trim(),
                    Role = string.IsNullOrWhiteSpace(a.Role) ? null : a.Role.Trim(),
                    Skills = (a.Skills ?? new List<string>())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .ToList(),
                })
                .ToList();

            return new CityPresence
            {
                RoomId = roomId,
                GitEmail = email,
                OwnerId = FederationRoom.ComputeUserId(email),
                Agents = agents,
            };
        }
    }
}
