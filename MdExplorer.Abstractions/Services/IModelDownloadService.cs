using System;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models;

namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Servizio per download e gestione modelli AI.
    /// </summary>
    public interface IModelDownloadService
    {
        /// <summary>
        /// Lista modelli disponibili per il download.
        /// </summary>
        Task<ModelInfo[]> GetAvailableModelsAsync();

        /// <summary>
        /// Lista modelli già installati localmente.
        /// </summary>
        Task<ModelInfo[]> GetInstalledModelsAsync();

        /// <summary>
        /// Scarica un modello da HuggingFace con supporto resume.
        /// </summary>
        Task<bool> DownloadModelAsync(string modelId, IProgress<DownloadProgress> progress, CancellationToken ct = default);

        /// <summary>
        /// Elimina un modello installato.
        /// </summary>
        Task<bool> DeleteModelAsync(string fileName);
    }
}
