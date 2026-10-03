using MdExplorer.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services
{
    /// <summary>
    /// Full-text index over the project's MARKDOWN files, on side-car database
    /// MdEngineFts_{hash}.db (table MarkdownContentFts). All SQL lives in
    /// <see cref="FtsSideCarIndexBase"/>; this class only pins the markdown-world
    /// database/table names and adapts the public <see cref="IMarkdownFtsService"/>
    /// contract. Behaviour is byte-for-byte identical to the pre-refactor service.
    /// </summary>
    public class MarkdownFtsService : FtsSideCarIndexBase, IMarkdownFtsService
    {
        protected override string DbFilePrefix => "MdEngineFts_";
        protected override string TableName => "MarkdownContentFts";
        protected override string LogTag => "MarkdownFts";

        public MarkdownFtsService(string appDataPath, ILogger<MarkdownFtsService> logger)
            : base(appDataPath, logger)
        {
        }

        public void RebuildIndex(string projectPath, IReadOnlyCollection<MarkdownFtsEntry> entries)
        {
            RebuildIndexCore(projectPath,
                entries.Select(e => (e.MarkdownFileId, e.Path, e.FileName, e.Content)));
        }
    }
}
