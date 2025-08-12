using MdExplorer.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    /// <summary>
    /// Classe di scambio dati per il treeviewer del client
    /// </summary>
    public class FileInfoNode:IFileInfoNode
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("path")]
        public string Path { get; set; }
        
        [JsonPropertyName("fullPath")]
        public string FullPath { get; set; }
        
        [JsonPropertyName("relativePath")]
        public string RelativePath { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("level")]
        public int Level { get; set; }
        
        [JsonPropertyName("expandable")]
        public bool Expandable { get; set; } = true;
        
        [JsonPropertyName("dataText")]
        public string DataText { get; set; }
        
        [JsonPropertyName("childrens")]
        public IList<IFileInfoNode> Childrens { get; set; } = new List<IFileInfoNode>();
        
        // Nuove proprietà per caricamento incrementale
        [JsonPropertyName("isIndexed")]
        public bool IsIndexed { get; set; } = false;
        
        [JsonPropertyName("indexingStatus")]
        public string IndexingStatus { get; set; } = "idle";
        
        [JsonPropertyName("indexingProgress")]
        public int? IndexingProgress { get; set; }
    }
}
