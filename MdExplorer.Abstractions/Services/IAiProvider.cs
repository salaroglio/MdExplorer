using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;

namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Interfaccia base per tutti i provider AI
    /// </summary>
    public interface IAiProvider
    {
        /// <summary>
        /// Nome del provider
        /// </summary>
        string GetName();

        /// <summary>
        /// Tipo di provider
        /// </summary>
        ProviderType GetProviderType();

        /// <summary>
        /// Verifica se il provider è disponibile e configurato
        /// </summary>
        bool IsAvailable();

        /// <summary>
        /// Ottiene le capacità del provider
        /// </summary>
        ProviderCapabilities GetCapabilities();

        /// <summary>
        /// Invia un messaggio e ottiene una risposta completa
        /// </summary>
        /// <param name="prompt">Messaggio da inviare</param>
        /// <param name="modelId">ID del modello da usare (opzionale)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Risposta completa del modello</returns>
        Task<string> ChatAsync(string prompt, string modelId = null, CancellationToken ct = default);

        /// <summary>
        /// Invia un messaggio e riceve la risposta in streaming
        /// </summary>
        /// <param name="prompt">Messaggio da inviare</param>
        /// <param name="modelId">ID del modello da usare (opzionale)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Stream di chunk di testo</returns>
        IAsyncEnumerable<string> StreamChatAsync(string prompt, string modelId = null, CancellationToken ct = default);

        /// <summary>
        /// Imposta il system prompt per il provider
        /// </summary>
        Task SetSystemPromptAsync(string systemPrompt);

        /// <summary>
        /// Ottiene il system prompt corrente
        /// </summary>
        Task<string> GetSystemPromptAsync();

        /// <summary>
        /// Ottiene l'API key configurata per il provider
        /// </summary>
        Task<string> GetApiKeyAsync();

        /// <summary>
        /// Salva l'API key per il provider
        /// </summary>
        Task SaveApiKeyAsync(string apiKey);

        /// <summary>
        /// Testa la validità di un'API key
        /// </summary>
        Task<bool> TestApiKeyAsync(string apiKey);

        /// <summary>
        /// Chat with tool calling support (function calling).
        /// The AI can autonomously decide to use tools to accomplish tasks.
        /// </summary>
        /// <param name="prompt">User prompt/message</param>
        /// <param name="tools">Available tools for the AI to call</param>
        /// <param name="toolExecutor">Function to execute tool calls</param>
        /// <param name="modelId">ID del modello da usare (opzionale)</param>
        /// <param name="currentDocumentPath">Path to the current document for context-aware operations (opzionale)</param>
        /// <param name="conversationHistory">Previous conversation messages for multi-turn dialogs (opzionale)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Final AI response after tool executions</returns>
        Task<string> ChatWithToolsAsync(
            string prompt,
            List<object> tools,
            Func<string, dynamic, Task<object>> toolExecutor,
            string modelId = null,
            string currentDocumentPath = null,
            List<object> conversationHistory = null,
            CancellationToken ct = default);
    }
}
