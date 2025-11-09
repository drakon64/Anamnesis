using System.Text.Json.Serialization;

namespace Anamnesis.GoogleCloud;

internal static partial class GoogleCloud
{
    internal static async Task<Item[]> ListObjects(string bucket, string prefix)
    {
        using var request = new HttpRequestMessage();
        request.Headers.Add("Authorization", await GetAccessToken());
        request.RequestUri = new Uri(
            $"https://storage.googleapis.com/storage/v1/b/{bucket}/o?delimiter=/&prefix={prefix}"
        );

        using var response = await HttpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            throw new Exception();

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
    public required Item[] Items { get; init; }
}

internal sealed class Item
{
    [JsonPropertyName("contentType")]
    public required string ContentType { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; init; }
}
