using DocumentFormat.OpenXml.Office2010.ExcelAc;
using MdExplorer.Features.GIT.models;
using System.Collections.Generic;

namespace MdExplorer.Service.Controllers.GIT.models
{
    public class FileToBeChangedMdo: FileNameAndAuthor
    {
        public List<string> ArrayToFullPath { get; set; }

    }
}
