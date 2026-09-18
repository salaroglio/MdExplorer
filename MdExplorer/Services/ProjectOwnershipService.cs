using System;
using System.Collections.Generic;
using System.IO;
using MdExplorer.Features.Agents;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services
{
    /// <summary>
    /// Carica la tabella di ownership del progetto (§12.3) quando la <b>federazione è
    /// attiva</b>. È un <b>routing hint</b> (chi è responsabile di quale ambito, con quali
    /// agenti), iniettato nella rubrica del prompt — mai un permesso (il gate umano §12.6
    /// resta il guardrail). Legge sempre da disco (mai stantìo); ritorna <c>null</c> quando
    /// la città è spenta, non c'è doc, o il doc è invalido (fail-loud: logga il motivo).
    /// </summary>
    public interface IProjectOwnershipService
    {
        /// <summary>Voci di ownership da iniettare, o <c>null</c> se non applicabile.</summary>
        IReadOnlyList<OwnershipEntry> GetActiveOwnership(string projectPath);
    }

    public class ProjectOwnershipService : IProjectOwnershipService
    {
        private readonly IProjectMetadataService _metadata;
        private readonly ILogger<ProjectOwnershipService> _logger;

        public ProjectOwnershipService(IProjectMetadataService metadata, ILogger<ProjectOwnershipService> logger)
        {
            _metadata = metadata;
            _logger = logger;
        }

        public IReadOnlyList<OwnershipEntry> GetActiveOwnership(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return null;

            var city = _metadata.GetAgentCity(projectPath);
            if (city == null || !city.Enabled)
                return null;                       // federazione spenta → nessuna iniezione

            if (string.IsNullOrWhiteSpace(city.OwnershipDoc))
                return null;                       // nessun doc dichiarato

            var docPath = Path.Combine(projectPath, city.OwnershipDoc.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(docPath))
            {
                _logger.LogWarning("[Ownership] doc dichiarato ma assente: '{Doc}' (progetto '{Project}')", city.OwnershipDoc, projectPath);
                return null;
            }

            string markdown;
            try { markdown = File.ReadAllText(docPath); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Ownership] lettura del doc '{Doc}' fallita", docPath);
                return null;
            }

            var parsed = OwnershipDocParser.Parse(markdown);
            if (!parsed.IsOwnershipDoc)
            {
                _logger.LogWarning("[Ownership] '{Doc}' non ha 'mde_type: ownership' nel frontmatter.", city.OwnershipDoc);
                return null;
            }
            if (parsed.HasErrors)
            {
                // Fail-loud: doc rifiutato (non iniettiamo una tabella malformata), motivo nei log.
                _logger.LogWarning("[Ownership] '{Doc}' rifiutato: {Errors}", city.OwnershipDoc, string.Join(" | ", parsed.Errors));
                return null;
            }

            return parsed.Entries.Count > 0 ? parsed.Entries : null;
        }
    }
}
