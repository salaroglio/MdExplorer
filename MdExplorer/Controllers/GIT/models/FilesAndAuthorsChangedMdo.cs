using MdExplorer.Abstractions.Models;
using MdExplorer.Features.GIT.models;
using System.Collections.Generic;

namespace MdExplorer.Service.Controllers.GIT.models
{
    public class FilesAndAuthorsChangedMdo: FileNameAndAuthor
    {
        public List<FileInfoNode> MdFiles { get; set; } = new List<FileInfoNode>();
    }
}
