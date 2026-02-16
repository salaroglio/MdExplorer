using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Services
{
    public class VectorSearchResult
    {
        public Guid ChunkId { get; set; }
        public string ChunkText { get; set; }
        public string FilePath { get; set; }
        public string SectionTitle { get; set; }
        public float SimilarityScore { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string ChunkType { get; set; } = "document";
        public string GroupId { get; set; }
    }

    public interface IVectorSearchService
    {
        Task<List<VectorSearchResult>> SearchAsync(string query, int topK = 5, float threshold = 0.3f);
        List<VectorSearchResult> GetGroupSiblings(IEnumerable<string> groupIds, IEnumerable<Guid> excludeChunkIds);
        void InvalidateCache();
    }
}
