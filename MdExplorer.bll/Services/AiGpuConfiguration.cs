namespace MdExplorer.Features.Services
{
    public class AiGpuConfiguration
    {
        public bool EnableAutoDetection { get; set; } = true;
        public bool PreferGpuAcceleration { get; set; } = true;
        public int DefaultGpuLayerCount { get; set; } = -1; // -1 means auto-detect
        public int ReserveGpuMemoryMB { get; set; } = 2048;
        public bool LogGpuDetails { get; set; } = true;
    }

    public class AiModelsConfiguration
    {
        public int DefaultContextSize { get; set; } = 4096;
        public int MaxTokens { get; set; } = 512;
        public float Temperature { get; set; } = 0.7f;
    }

    public class AiChatConfiguration
    {
        public AiGpuConfiguration Gpu { get; set; } = new AiGpuConfiguration();
        public AiModelsConfiguration Models { get; set; } = new AiModelsConfiguration();
    }
}