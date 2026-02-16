using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text);
        Task<float[][]> GenerateEmbeddingsAsync(string[] texts);
        bool IsModelLoaded();
        Task<bool> LoadModelAsync(string modelPath);
        Task<bool> LoadModelAsync(string modelPath, int contextSize, int batchSize, int maxEmbeddingChars);
        int GetEmbeddingDimension();
        string GetCurrentModelPath();
    }
}
