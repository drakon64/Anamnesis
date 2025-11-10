using System.Text.Json.Serialization;

namespace Anamnesis.Provider;

internal sealed class ProviderPackage
{
    [JsonPropertyName("protocols")]
    public required string[] Protocols { get; init; }

    [JsonPropertyName("os")]
    public required string OperatingSystem { get; init; }

    [JsonPropertyName("arch")]
    public required string Arch { get; init; }

    [JsonPropertyName("filename")]
    public required string Filename { get; init; }

    [JsonPropertyName("download_url")]
    public required string DownloadUrl { get; init; }

    [JsonPropertyName("shasums_url")]
    public required string ShasumsUrl { get; init; }

    [JsonPropertyName("shasums_signature_url")]
    public required string ShasumsSignatureUrl { get; init; }

    [JsonPropertyName("shasum")]
    public required string Shasum { get; init; }

    [JsonPropertyName("signing_keys")]
    public required SigningKeys SigningKeys { get; init; }
}

internal sealed class SigningKeys
{
    [JsonPropertyName("gpg_public_keys")]
    public required GpgPublicKey[] GpgPublicKeys { get; init; }
}

internal sealed class GpgPublicKey
{
    [JsonPropertyName("key_id")]
    public required string KeyId { get; init; }

    [JsonPropertyName("ascii_armor")]
    public required string AsciiArmor { get; init; }
}
