namespace MdExplorer.Service.Controllers.MdFiles.ModelsDto
{
    public class RequestAddExistingFileToMdeproject
    {
        public RequestAddExistingFileMdFileDto MdFile { get; set; }
        public string FullPath { get; set; }
    }

    public class RequestAddExistingFileMdFileDto
    {
        public string FullPath { get; set; }
    }
}
