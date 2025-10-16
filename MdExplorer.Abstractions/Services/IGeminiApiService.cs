using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Service for interacting with Google's Gemini API
    /// </summary>
    public interface IGeminiApiService
    {
        /// <summary>
        /// Check if the service is configured with a valid API key
        /// </summary>
        bool IsConfigured();
        
        /// <summary>
        /// Send a chat message and get response
        /// </summary>
        Task<string> ChatAsync(string message, string modelId);
        
        /// <summary>
        /// Stream a chat response
        /// </summary>
        IAsyncEnumerable<string> StreamChatAsync(string message, string modelId, CancellationToken ct = default);
    }
}
