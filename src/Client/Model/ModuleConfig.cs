using System.Text.Json.Serialization;

namespace Anamnesis.Client.Model;

internal sealed class ModuleConfig
{
    [JsonPropertyName("variables")]
    public required Dictionary<string, Variable> Variables { get; init; }

    [JsonPropertyName("outputs")]
    public required Dictionary<string, Output> Outputs { get; init; }
}
