namespace MdExplorer.Service.Controllers.MdFiles.ModelsDto
{
    public class RequestMoveMdFile
    {
        public string SourceRelativePath { get; set; }
        public string SourceFileName { get; set; }
        public string DestinationPath { get; set; }
    }
}
