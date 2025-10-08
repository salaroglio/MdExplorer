using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace MdExplorer.Features.Yaml.Models
{
    public class WordSection
    {
        public bool WriteToc { get; set; }
        public string DocumentHeader { get; set; }
        public TemplateSection TemplateSection { get; set; } = new TemplateSection();
        
        [YamlMember(Alias = "predefined_pages")]
        public PredefinedPagesDescriptor PredefinedPages { get; set; }


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
    
    public class PredefinedPagesDescriptor
    {
        [YamlMember(Alias = "cover")]
        public PageDescriptor Cover { get; set; }
        
        [YamlMember(Alias = "disclaimer")]
        public PageDescriptor Disclaimer { get; set; }
        
        [YamlMember(Alias = "appendix")]
        public PageDescriptor Appendix { get; set; }
    }

    public class PageDescriptor
    {
        [YamlMember(Alias = "type")]
        public string Type { get; set; }
        
        [YamlMember(Alias = "template")]
        public string Template { get; set; }
        
        [YamlMember(Alias = "enabled")]
        public bool Enabled { get; set; } = true;
        
        [YamlMember(Alias = "tags")]
        public Dictionary<string, string> Tags { get; set; }
    }
}
