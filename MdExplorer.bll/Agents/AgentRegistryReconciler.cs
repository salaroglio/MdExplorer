using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Yaml;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Cuore deterministico del registry (§6): date le card scoperte dalle due
    /// sorgenti e le identità già persistite, produce il catalogo del progetto
    /// applicando le regole di identità — <b>senza toccare I/O o DB</b> (perciò
    /// testabile a unità). Lo shell <c>AgentRegistryService</c> gli fornisce i dati
    /// letti dalle fonti e persiste il risultato.
    /// <list type="bullet">
    /// <item>Nome duplicato (case-insensitive) tra le sorgenti → <b>tutte</b> le voci
    /// con quel nome escluse, deterministico (mai "vince il primo").</item>
    /// <item>Scoperta già invalida (<see cref="DiscoveredAgentCard.ParseError"/>) →
    /// esclusa con quel motivo.</item>
    /// <item>Cittadino valido → trust/identità agganciati dal record esistente
    /// (merge per nome, case-insensitive).</item>
    /// </list>
    /// </summary>
    public class AgentRegistryReconciler
    {
        public IReadOnlyList<AgentRegistryEntry> Reconcile(
            IEnumerable<DiscoveredAgentCard> discovered,
            IEnumerable<ExistingIdentity> existingIdentities)
        {
            var discoveredList = (discovered ?? Enumerable.Empty<DiscoveredAgentCard>()).ToList();
            var identityByName = (existingIdentities ?? Enumerable.Empty<ExistingIdentity>())
                .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                .GroupBy(i => i.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            // Regola di unicità: nomi (case-insensitive) dichiarati da più di una sorgente.
            var duplicateNames = YamlAgentCardParser.FindDuplicateNames(
                discoveredList.Select(d => d.Name));

            var catalog = new List<AgentRegistryEntry>();
            foreach (var d in discoveredList)
            {
                var entry = new AgentRegistryEntry
                {
                    Name = d.Name?.Trim(),
                    Kind = d.Kind,
                    AgentFilePath = d.AgentFilePath,
                    Role = d.Role,
                    Skills = d.Skills ?? new List<AgentRegistrySkill>(),
                    Tools = d.Tools ?? new List<string>(),
                    AcceptsMessagesFrom = d.AcceptsMessagesFrom ?? new List<string>(),
                    MaxHops = d.MaxHops,
                    CurrentA2ABlockHash = d.CurrentA2ABlockHash,
                };

                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    // Nessun nome su cui agganciarsi: sempre frutto di un parse fallito.
                    entry.RegistrationError = d.ParseError ?? "Nome agente mancante.";
                }
                else if (duplicateNames.Contains(entry.Name))
                {
                    // Duplicato: prevale sulla regola del singolo (tutte escluse).
                    entry.RegistrationError =
                        $"Nome '{entry.Name}' duplicato: più agenti lo dichiarano nel progetto — tutti esclusi.";
                }
                else if (!string.IsNullOrEmpty(d.ParseError))
                {
                    entry.RegistrationError = d.ParseError;
                }

                // Merge del trust persistito (anche per le voci escluse, per portare l'Id in UI).
                if (!string.IsNullOrWhiteSpace(entry.Name)
                    && identityByName.TryGetValue(entry.Name, out var identity))
                {
                    entry.IdentityId = identity.Id;
                    entry.Trusted = identity.Trusted;
                    entry.Enabled = identity.Enabled;
                    entry.A2ABlockHash = identity.A2ABlockHash;

                    // Decadenza automatica (R3): se il blocco a2a:/tools: è cambiato
                    // dall'ultima conferma, il trust decade e va riconfermato dall'umano.
                    if (identity.Trusted
                        && !string.IsNullOrEmpty(identity.A2ABlockHash)
                        && !string.IsNullOrEmpty(entry.CurrentA2ABlockHash)
                        && !string.Equals(identity.A2ABlockHash, entry.CurrentA2ABlockHash, StringComparison.Ordinal))
                    {
                        entry.Trusted = false;
                        entry.Enabled = false;
                        entry.TrustDecayed = true;
                    }
                }

                catalog.Add(entry);
            }

            // Ordine stabile per nome (le voci senza nome in coda), per output deterministico.
            return catalog
                .OrderBy(e => e.Name ?? "￿", StringComparer.OrdinalIgnoreCase)
                .ThenBy(e => e.AgentFilePath ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
