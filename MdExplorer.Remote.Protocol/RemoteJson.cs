using System.Text.Json.Serialization;

namespace MdExplorer.Remote.Protocol
{
    /// <summary>Il JSON del protocollo: camelCase, campi nulli omessi, generato a compile time (Native AOT).</summary>
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(RemoteRequest))]
    [JsonSerializable(typeof(RemoteResponse))]
    public partial class RemoteJson : JsonSerializerContext
    {
    }
}
