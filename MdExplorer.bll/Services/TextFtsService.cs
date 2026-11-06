using MdExplorer.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services
{
    /// <summary>
    /// Full-text index over the project's NON-markdown text files, on side-car
    /// database MdEngineTextFts_{hash}.db (table TextContentFts). Shares all SQL
    /// with the markdown index via <see cref="FtsSideCarIndexBase"/> but keeps a
    /// completely separate database file, so the two indexes never overlap.
    /// </summary>
    public class TextFtsService : FtsSideCarIndexBase, ITextFtsService
    {
        protected override string DbFilePrefix => "MdEngineTextFts_";
        protected override string TableName => "TextContentFts";
        protected override string LogTag => "TextFts";

        public TextFtsService(string appDataPath, ILogger<TextFtsService> logger)
            : base(appDataPath, logger)
        {
        }

        public void RebuildIndex(string projectPath, IReadOnlyCollection<TextFtsEntry> entries)
        {
            RebuildIndexCore(projectPath,
                entries.Select(e => (e.TextFileId, e.Path, e.FileName, e.Content)));
        }
    }
}
