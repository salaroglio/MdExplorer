using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.AI.Stubs.Services
{
    public class GeminiApiServiceStub : IGeminiApiService
    {
        public bool IsConfigured()
        {
            return false;
        }

        public Task<string> ChatAsync(string message, string modelId)
        {
            return Task.FromResult("⚠️ Gemini API requires MdExplorer AI Premium");
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            string message, 
            string modelId,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            yield return "⚠️ Gemini API requires MdExplorer AI Premium";
            await Task.CompletedTask;
        }
    }
}
