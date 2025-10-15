namespace MdExplorer.AI.Abstractions.Models
{
    public class ModelInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string HuggingFaceRepo { get; set; }
        public string FileName { get; set; }
        public long SizeBytes { get; set; }
        public string FormattedSize { get; set; }
        public bool IsInstalled { get; set; }
        public string LocalPath { get; set; }
        public string[] RequiredCapabilities { get; set; }
    }
}
