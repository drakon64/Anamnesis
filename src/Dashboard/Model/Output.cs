using System.Text.Json.Serialization;

using Google.Cloud.Firestore;

namespace Anamnesis.Dashboard.Model;

[FirestoreData]
public sealed class Output
{
    [FirestoreProperty("name")]
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [FirestoreProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [FirestoreProperty("sensitive")]
    [JsonPropertyName("sensitive")]
    public bool Sensitive { get; init; } = false;
}
