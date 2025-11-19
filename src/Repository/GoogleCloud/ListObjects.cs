using System.Text.Json.Serialization;

namespace Anamnesis.GoogleCloud;

internal static partial class GoogleCloud
{
    internal static async Task<Object[]> ListObjects(string bucket, string prefix)
    {
        using var request = new HttpRequestMessage();
        request.Headers.Add("Authorization", await GetAccessToken());
        request.RequestUri = new Uri(
            $"https://storage.googleapis.com/storage/v1/b/{bucket}/o?delimiter=/&prefix={prefix}"
        );

        using var response = await HttpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            throw new Exception(await response.Content.ReadAsStringAsync());

        return (
            (
                await response.Content.ReadFromJsonAsync(
                    SourceGenerationContext.Default.ListObjectsResponse
                )
            )!
        ).Items;
    }
}

internal sealed class ListObjectsResponse
{
    [JsonPropertyName("items")]
    public required Object[] Items { get; init; }
}

internal sealed class Object
{
    [JsonPropertyName("contentType")]
    public required string ContentType { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
