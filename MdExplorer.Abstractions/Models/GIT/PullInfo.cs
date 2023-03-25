using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Abstractions.Models.GIT
{
    public class PullInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ProjectPath { get; set; }
        public string BranchName { get; set; }
    }
}
