using System.Collections.Generic;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;

namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Interfaccia per il discovery dei modelli disponibili
    /// </summary>
    public interface IModelDiscoveryProvider
    {
        /// <summary>
        /// Tipo di provider per cui questo servizio fa discovery
        /// </summary>
        ProviderType ProviderType { get; }

        /// <summary>
        /// Verifica se il provider supporta il discovery automatico
        /// </summary>
        bool SupportsDiscovery();

        /// <summary>
        /// Ottiene l'elenco dei modelli disponibili
        /// </summary>
        /// <returns>Lista di modelli disponibili</returns>
        Task<List<AiProviderModel>> GetModelsAsync();
    }
}
