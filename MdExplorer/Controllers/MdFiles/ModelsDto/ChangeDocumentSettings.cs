using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Yaml.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Service.Controllers.MdFiles.Models
{
    public class ChangeDocumentSettings
    {
        public MdExplorerDocumentDescriptor DocumentDescriptor { get; set; }
        public FileInfoNode MdFile { get; set; }
    }
}
