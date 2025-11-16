using System.Text.Json.Serialization;

namespace Anamnesis.Module;

internal sealed class ModuleVersions
{
    [JsonPropertyName("modules")]
    public required Module[] Modules { get; init; }
}

internal sealed class Module
{
    [JsonPropertyName("versions")]
    public required ModuleVersion[] Versions { get; init; }
}

internal sealed class ModuleVersion
{
    [JsonPropertyName("version")]
    public required string Version { get; init; }
}
