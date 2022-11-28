using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Features.Yaml.Models
{
    public class WordSection
    {
        public bool WriteToc { get; set; }
        public string DocumentHeader { get; set; }

        public static class DocumentHeaderEnum
        {
            public const string None = "None";
            public const string Project = "Project";
            public const string Custom = "Custom";
        }
    }
}
