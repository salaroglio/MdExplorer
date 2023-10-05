using System;

namespace MdExplorer.Service.Controllers.MdFiles.ModelsDto
{
    public class ToggleBookmarkRequest
    {
        public Guid ProjectId { get; set; }
        public string FullPath { get; set; }
        public string Name { get; set; }
    }
}
