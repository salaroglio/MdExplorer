using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using Ad.Tools.Dal.Extensions;

namespace MdExplorer.Features.Services.AI
{
    public class VectorSearchService : IVectorSearchService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IEngineDB _engineDB;
        private readonly ILogger<VectorSearchService> _logger;

        // Cache: list of (chunk, embedding float[])
        private List<(DocumentChunk Chunk, float[] Embedding)> _cache;
        private DateTime _cacheTime;

        public VectorSearchService(
            IEmbeddingService embeddingService,
            IEngineDB engineDB,
            ILogger<VectorSearchService> logger)
        {
            _embeddingService = embeddingService;
            _engineDB = engineDB;
            _logger = logger;
        }

        public async Task<List<VectorSearchResult>> SearchAsync(string query, int topK = 5, float threshold = 0.3f)
        {
            if (!_embeddingService.IsModelLoaded())
            {
                _logger.LogWarning("[VectorSearch] Embedding model not loaded");
                return new List<VectorSearchResult>();
            }

            // Generate query embedding
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

            // Load embeddings from cache or DB
            var chunks = LoadChunksWithEmbeddings();

            if (chunks.Count == 0)
            {
                _logger.LogInformation("[VectorSearch] No chunks with embeddings found");
                return new List<VectorSearchResult>();
            }

            // Calculate cosine similarity for each chunk
            var scored = new List<(DocumentChunk Chunk, float Score)>();
            foreach (var (chunk, embedding) in chunks)
            {
                var score = CosineSimilarity(queryEmbedding, embedding);
                if (score >= threshold)
                {
                    scored.Add((chunk, score));
                }
            }

            // Sort by score descending and take top K
            return scored
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => new VectorSearchResult
                {
                    ChunkId = x.Chunk.Id,
                    ChunkText = x.Chunk.Content,
                    FilePath = x.Chunk.FilePath,
                    SectionTitle = x.Chunk.SectionTitle,
                    SimilarityScore = x.Score,
                    StartLine = x.Chunk.StartLine,
                    EndLine = x.Chunk.EndLine,
                    ChunkType = x.Chunk.ChunkType ?? "document",
                    GroupId = x.Chunk.GroupId
                })
                .ToList();
        }

        public List<VectorSearchResult> GetGroupSiblings(IEnumerable<string> groupIds, IEnumerable<Guid> excludeChunkIds)
        {
            var groupIdSet = new HashSet<string>(groupIds);
            var excludeSet = new HashSet<Guid>(excludeChunkIds);

            try
            {
                var chunkDal = _engineDB.GetDal<DocumentChunk>();
                return chunkDal.GetList()
                    .Where(c => c.GroupId != null && groupIdSet.Contains(c.GroupId))
                    .ToList()
                    .Where(c => !excludeSet.Contains(c.Id))
                    .Select(c => new VectorSearchResult
                    {
                        ChunkId = c.Id,
                        ChunkText = c.Content,
                        FilePath = c.FilePath,
                        SectionTitle = c.SectionTitle,
                        SimilarityScore = 0f,
                        StartLine = c.StartLine,
                        EndLine = c.EndLine,
                        ChunkType = c.ChunkType ?? "document",
                        GroupId = c.GroupId
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VectorSearch] Error fetching group siblings");
                return new List<VectorSearchResult>();
            }
        }

        public void InvalidateCache()
        {
            _cache = null;
        }

        private List<(DocumentChunk Chunk, float[] Embedding)> LoadChunksWithEmbeddings()
        {
            // Use cache if available and recent (5 minutes)
            if (_cache != null && (DateTime.UtcNow - _cacheTime).TotalMinutes < 5)
                return _cache;

            try
            {
                var chunkDal = _engineDB.GetDal<DocumentChunk>();
                var allChunks = chunkDal.GetList()
                    .Where(c => c.Embedding != null && c.EmbeddingDimension > 0)
                    .ToList();

                _cache = new List<(DocumentChunk, float[])>();

                foreach (var chunk in allChunks)
                {
                    var embedding = DeserializeEmbedding(chunk.Embedding, chunk.EmbeddingDimension);
                    if (embedding != null)
                    {
                        _cache.Add((chunk, embedding));
                    }
                }

                _cacheTime = DateTime.UtcNow;
                _logger.LogInformation("[VectorSearch] Loaded {Count} chunks into cache", _cache.Count);
                return _cache;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VectorSearch] Error loading chunks from DB");
                return new List<(DocumentChunk, float[])>();
            }
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length)
                return 0f;

            float dotProduct = 0f;
            float normA = 0f;
            float normB = 0f;

            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            var denominator = (float)(Math.Sqrt(normA) * Math.Sqrt(normB));
            if (denominator == 0)
                return 0f;

            return dotProduct / denominator;
        }

        public static byte[] SerializeEmbedding(float[] embedding)
        {
            var bytes = new byte[embedding.Length * sizeof(float)];
            Buffer.BlockCopy(embedding, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static float[] DeserializeEmbedding(byte[] bytes, int dimension)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            var result = new float[dimension];
            Buffer.BlockCopy(bytes, 0, result, 0, Math.Min(bytes.Length, dimension * sizeof(float)));
            return result;
        }
    }
}
