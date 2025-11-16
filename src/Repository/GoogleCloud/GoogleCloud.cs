using System.Text.Json.Serialization;

namespace Anamnesis.GoogleCloud;

internal static partial class GoogleCloud
{
    private static readonly HttpClient HttpClient = new();

    // private static async Task<string> GetAccessToken()
    // {
    //     using var request = new HttpRequestMessage();
    //     request.Headers.Add("Metadata-Flavor", "Google");
    //     request.Method = HttpMethod.Get;
    //     request.RequestUri = new Uri(
    //         "http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/token"
    //     );
    //
    //     using var response = await HttpClient.SendAsync(request);
    //
    //     if (!response.IsSuccessStatusCode)
    //         throw new Exception(await response.Content.ReadAsStringAsync());
    //
    //     var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(
    //         SourceGenerationContext.Default.AccessTokenResponse
    //     );
    //
    //     return $"{token!.TokenType} {token.AccessToken}";
    // }

    private static async Task<string> GetAccessToken() =>
        Environment.GetEnvironmentVariable("GCLOUD_ACCESS_TOKEN")!;
}

internal sealed class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; init; }
}

internal sealed class Object
{
    [JsonPropertyName("contentType")]
    public required string ContentType { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

internal sealed class Metadata
{
    [JsonPropertyName("protocols")]
    public required string Protocols { get; init; }

    [JsonPropertyName("platforms")]
    public required string Platforms { get; init; }

    [JsonPropertyName("os")]
    public required string Os { get; init; }

    [JsonPropertyName("arch")]
    public required string Arch { get; init; }

    [JsonPropertyName("shasum")]
    public required string Shasum { get; init; }

    [JsonPropertyName("signing_keys")]
    public required string SigningKeys { get; init; }
}
