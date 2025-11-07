using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Anamnesis.Client.GoogleCloud;

internal static partial class GoogleCloud
{
    internal static async Task InsertObject(string name, byte[] data)
    {
        using var content = new MultipartContent("related");
        content.Add(
            JsonContent.Create(
                new ObjectMetadata { Name = name },
                ObjectMetadataJsonSerializerContext.Default.ObjectMetadata
            )
        );

        using var dataContent = new ByteArrayContent(data);
        dataContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        content.Add(dataContent);

        using var request = new HttpRequestMessage();
        request.Content = content;
        request.Headers.Add("Authorization", await GetAccessToken());
        request.RequestUri = new Uri("");

        using var response = await HttpClient.SendAsync(request);
    }

    private sealed class ObjectMetadata
    {
        public required string Name { get; init; }
    }

    [JsonSerializable(typeof(ObjectMetadata))]
    private partial class ObjectMetadataJsonSerializerContext : JsonSerializerContext;
}
