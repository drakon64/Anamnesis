namespace Anamnesis.GoogleCloud;

internal static partial class GoogleCloud
{
    internal static async Task<Object> GetObject(string bucket, string objectName)
    {
        using var request = new HttpRequestMessage();
        request.Headers.Add("Authorization", await GetAccessToken());
        request.RequestUri = new Uri(
            $"https://storage.googleapis.com/storage/v1/b/{bucket}/o/{objectName}"
        );

        using var response = await HttpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            throw new Exception();

        return (await response.Content.ReadFromJsonAsync(SourceGenerationContext.Default.Object))!;
    }
}
