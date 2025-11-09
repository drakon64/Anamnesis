using System.Text.Json.Serialization;

namespace Anamnesis;

internal sealed class ProviderVersions
{
    [JsonPropertyName("versions")]
    public required ProviderVersion[] Versions { get; init; }
}

internal sealed class ProviderVersion
{
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("protocols")]
    public required string[] Protocols { get; init; }

    [JsonPropertyName("platforms")]
    public required Platform[] Platforms { get; init; }
}

internal sealed class Platform
{
    [JsonPropertyName("os")]
    public required string Os { get; init; }

    [JsonPropertyName("arch")]
    public required string Arch { get; init; }
}
