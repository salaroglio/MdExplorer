using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Abstractions.Models.GIT
{
    public class CloneInfo
    {
        public string UrlPath { get; set; }
        public string DirectoryPath { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
