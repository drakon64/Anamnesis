namespace Anamnesis.GoogleCloud;

internal static partial class GoogleCloud
{
    internal static async Task<Item[]> ListObjects(string bucket, string prefix)
    {
        var queryString = QueryString.Empty;
        queryString.Add("delimiter", "/");
        queryString.Add("prefix", prefix);

        using var request = new HttpRequestMessage();
        request.Headers.Add("Authorization", await GetAccessToken());
        request.RequestUri = new Uri(
            $"https://storage.googleapis.com/storage/v1/b/{bucket}/o{queryString}"
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

    internal sealed class ListObjectsResponse
    {
        public required Item[] Items { get; init; }
    }

    internal sealed class Item
    {
        public required string Name { get; init; }
        public required Dictionary<string, string> Metadata { get; init; }
    }
}
