using MdExplorer.Abstractions.Models;

namespace MdExplorer.Service.Controllers.MdFiles.ModelsDto
{
    public class RequestMoveMdFile
    {
        public FileInfoNode MdFile { get; set; }
        public string DestinationPath { get; set; }
    }
}
