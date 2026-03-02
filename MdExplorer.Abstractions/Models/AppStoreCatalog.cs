using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MdExplorer.Abstractions.Models
{
    public class AppStoreCatalog
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("repoName")]
        public string RepoName { get; set; }

        [JsonPropertyName("repoDescription")]
        public string RepoDescription { get; set; }

        [JsonPropertyName("repoCompany")]
        public string RepoCompany { get; set; }

        [JsonPropertyName("repoLogo")]
        public string RepoLogo { get; set; }

        [JsonPropertyName("apps")]
        public List<AppStoreCatalogEntry> Apps { get; set; } = new();
    }

    public class AppStoreCatalogEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("executableName")]
        public string ExecutableName { get; set; }

        [JsonPropertyName("defaultArgs")]
        public List<string> DefaultArgs { get; set; } = new();

        [JsonPropertyName("changelog")]
        public string Changelog { get; set; }

        [JsonPropertyName("platforms")]
        public Dictionary<string, PlatformBuild> Platforms { get; set; }
    }

    public class PlatformBuild
    {
        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("executableName")]
        public string ExecutableName { get; set; }
    }
}
