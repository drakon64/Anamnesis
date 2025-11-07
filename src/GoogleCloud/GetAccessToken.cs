namespace Anamnesis.GoogleCloud;

internal static partial class GoogleCloud
{
    private static async Task<string> GetAccessToken()
    {
        using var request = new HttpRequestMessage();
        request.Headers.Add("Metadata-Flavor", "Google");
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri(
            "http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/token"
        );

        using var response = await HttpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            throw new Exception(await response.Content.ReadAsStringAsync());

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(
            SourceGenerationContext.Default.AccessTokenResponse
        );

        return $"{token!.TokenType} {token.AccessToken}";
    }

    internal sealed class AccessTokenResponse
    {
        public required string AccessToken { get; init; }
        public required string TokenType { get; init; }
    }
}
