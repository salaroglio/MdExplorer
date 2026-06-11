using System.Collections.Generic;

namespace MdExplorer.Service.Controllers.TabBar.Automapper
{
    public class KnowledgeGraphDto
    {
        public string CenterId { get; set; }
        public List<KnowledgeGraphNodeDto> Nodes { get; set; } = new List<KnowledgeGraphNodeDto>();
        public List<KnowledgeGraphLinkDto> Links { get; set; } = new List<KnowledgeGraphLinkDto>();
    }

    public class KnowledgeGraphNodeDto
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string FullPath { get; set; }
        public string MdContext { get; set; }
        public bool IsCenter { get; set; }
        public int InDegree { get; set; }
        public int OutDegree { get; set; }
        public string RelativePath { get; set; }
        public bool IsExternal { get; set; }
        public string ExternalUrl { get; set; }
        public string Cluster { get; set; }
        public string Tldr { get; set; }
    }

    public class KnowledgeGraphLinkDto
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public string LinkType { get; set; }
        public string Source_LinkSource { get; set; }
    }
}
