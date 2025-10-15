using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.AI.Abstractions.Models;

namespace MdExplorer.AI.Abstractions.Services
{
    /// <summary>
    /// Servizio per chat AI con modelli locali.
    /// Implementazione premium richiede licenza valida.
    /// </summary>
    public interface IAiChatService
    {
        /// <summary>
        /// Verifica se il servizio è licenziato e funzionante.
        /// </summary>
        Task<bool> IsLicensedAsync();

        /// <summary>
        /// Ottiene lo stato corrente della licenza.
        /// </summary>
        Task<LicenseStatus> GetLicenseStatusAsync();

        /// <summary>
        /// Chat sincrona con il modello AI.
        /// </summary>
        /// <exception cref="Exceptions.LicenseRequiredException">Se la licenza non è valida</exception>
        Task<string> ChatAsync(string prompt);

        /// <summary>
        /// Chat asincrona con streaming dei token.
        /// </summary>
        /// <exception cref="Exceptions.LicenseRequiredException">Se la licenza non è valida</exception>
        IAsyncEnumerable<string> StreamChatAsync(string prompt, CancellationToken ct = default);

        /// <summary>
        /// Verifica se un modello è caricato in memoria.
        /// </summary>
        bool IsModelLoaded();

        /// <summary>
        /// Carica un modello AI dal percorso specificato.
        /// </summary>
        Task<bool> LoadModelAsync(string modelPath, string modelId = null);

        /// <summary>
        /// Ottiene il nome del modello correntemente caricato.
        /// </summary>
        string GetCurrentModelName();

        /// <summary>
        /// Ottiene l'ID del modello correntemente caricato.
        /// </summary>
        string GetCurrentModelId();

        /// <summary>
        /// Imposta il system prompt per il modello corrente.
        /// </summary>
        Task SetSystemPromptAsync(string systemPrompt);

        /// <summary>
        /// Ottiene il system prompt corrente.
        /// </summary>
        string GetSystemPrompt();

        /// <summary>
        /// Informazioni sulla GPU utilizzata (se disponibile).
        /// </summary>
        GpuInfo GetGpuInfo();

        /// <summary>
        /// Verifica se l'accelerazione GPU è attiva.
        /// </summary>
        bool IsGpuEnabled();

        /// <summary>
        /// Numero di layer GPU utilizzati.
        /// </summary>
        int GetGpuLayerCount();
    }
}
