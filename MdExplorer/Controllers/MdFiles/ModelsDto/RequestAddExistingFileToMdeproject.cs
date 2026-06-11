namespace MdExplorer.Service.Controllers.MdFiles.ModelsDto
{
    public class RequestAddExistingFileToMdeproject
    {
        public RequestAddExistingFileMdFileDto MdFile { get; set; }
        public string FullPath { get; set; }

        /// <summary>When true the link is rendered as a bullet list item ("- [name](path)").</summary>
        public bool AsBulletList { get; set; }

        /// <summary>When true a blank line is ensured before the link so it is not glued to the previous content.</summary>
        public bool IsFirst { get; set; }
    }

    public class RequestAddExistingFileMdFileDto
    {
        public string FullPath { get; set; }
    }
}
