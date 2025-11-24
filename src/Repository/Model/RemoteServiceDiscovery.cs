using System.Text.Json.Serialization;

namespace Anamnesis.Model;

internal sealed class RemoteServiceDiscovery
{
    [JsonInclude]
    [JsonPropertyName("modules.v1")]
    public readonly string ModulesV1 = "/terraform/modules/v1/";
}
