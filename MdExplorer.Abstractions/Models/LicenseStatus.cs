using System;

namespace MdExplorer.Abstractions.Models
{
    public class LicenseStatus
    {
        public bool IsValid { get; set; }
        public LicenseType Type { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? LastValidated { get; set; }
        public bool IsOfflineMode { get; set; }
        public string Message { get; set; }
        public string[] EnabledFeatures { get; set; }
    }

    public enum LicenseType
    {
        Free = 0,           // Nessuna AI features
        Starter = 1,        // Chat base + download modelli
        Professional = 2,   // Chat + RAG + embedding models
        Enterprise = 3      // Tutto + tool system + agents
    }
}
