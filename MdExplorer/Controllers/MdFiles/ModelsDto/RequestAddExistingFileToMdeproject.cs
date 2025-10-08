using MdExplorer.Abstractions.Models;

namespace MdExplorer.Service.Controllers.MdFiles.ModelsDto
{
    public class RequestAddExistingFileToMdeproject
    {
        public FileInfoNode MdFile { get; set; }
        public string FullPath { get; set; }
    }
}
