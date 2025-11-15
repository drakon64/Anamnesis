using System.Text.Json.Serialization;

namespace Anamnesis.GoogleCloud;

internal static partial class GoogleCloud
{
    internal static async Task<string[]> ListFolders(string bucket)
    {
        using var request = new HttpRequestMessage();
        request.Headers.Add("Authorization", await GetAccessToken());
        request.RequestUri = new Uri(
            $"https://storage.googleapis.com/storage/v1/b/{bucket}/folders?delimiter=/"
        );

        using var response = await HttpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            throw new Exception(await response.Content.ReadAsStringAsync());

        var folders = (
            (
                await response.Content.ReadFromJsonAsync(
                    SourceGenerationContext.Default.ListFoldersResponse
                )
            )!
        ).Items;

        return (from folder in folders select folder.Name.TrimEnd('/')).ToArray();
    }

    internal static async Task<string[]> ListFolders(string bucket, string prefix)
    {
        var queryPrefix = prefix.EndsWith('/') ? prefix : $"{prefix}/";

        using var request = new HttpRequestMessage();
        request.Headers.Add("Authorization", await GetAccessToken());
        request.RequestUri = new Uri(
            $"https://storage.googleapis.com/storage/v1/b/{bucket}/folders?delimiter=/&prefix={queryPrefix}"
        );

        using var response = await HttpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            throw new Exception(await response.Content.ReadAsStringAsync());

        return (
            from folder in (
                (
                    await response.Content.ReadFromJsonAsync(
                        SourceGenerationContext.Default.ListFoldersResponse
                    )
                )!
            ).Items
            where folder.Name != prefix
            select folder.Name.Replace(prefix, "").Trim('/')
        ).ToArray();
    }
}

internal sealed class ListFoldersResponse
{
    [JsonPropertyName("items")]
    public required Folder[] Items { get; init; }
}

internal sealed class Folder
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
