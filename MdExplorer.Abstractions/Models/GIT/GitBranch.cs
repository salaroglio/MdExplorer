using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Abstractions.Models.GIT
{
    public class GitBranch
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }
    }
}
