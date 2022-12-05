using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Features.Yaml.Models
{
    public class WordSection
    {
        public bool WriteToc { get; set; }
        public string DocumentHeader { get; set; }
        public TemplateSection TemplateSection { get; set; } = new TemplateSection();


        public static class DocumentHeaderEnum
        {
            public const string None = "None";
            public const string Project = "Project";
            public const string Custom = "Custom";
        }
    }

    public class TemplateSection
    {
        public string InheritFromTemplate { get; set; } = "project";
        public string CustomTemplate { get; set; }
        public string TemplateType { get; set; } = TemplateTypeEnum.inherits;

        public static class TemplateTypeEnum
        {
            public const string inherits = "inherits";
            public const string custom = "custom";
        }
        
    }
}
