using System;

namespace MdExplorer.Service.Controllers.MdFiles.ModelsDto
{
    public class ReorderBookmarksRequest
    {
        public Guid ProjectId { get; set; }
        public string[] OrderedFullPaths { get; set; }
    }
}
