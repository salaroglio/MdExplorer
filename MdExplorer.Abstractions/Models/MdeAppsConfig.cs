using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MdExplorer.Abstractions.Models
{
    public class MdeAppsConfig
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("apps")]
        public List<MdeAppDefinition> Apps { get; set; } = new();
    }

    public class MdeAppDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("executable")]
        public string Executable { get; set; }

        [JsonPropertyName("args")]
        public List<string> Args { get; set; }

        [JsonPropertyName("treePosition")]
        public string TreePosition { get; set; }

        [JsonPropertyName("singleton")]
        public bool Singleton { get; set; } = true;
    }
}
