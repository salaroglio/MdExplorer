using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Features.GIT.models
{
    public class FileNameAndAuthor
    {
        public string FileName { get; set; }
        public string Author { get; set; }
        public string FullPath { get; set; }
        public string Status { get; set; }
    }
}
