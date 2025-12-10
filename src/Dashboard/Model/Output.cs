using System.Text.Json.Serialization;

using Google.Cloud.Firestore;

namespace Anamnesis.Dashboard.Model;

[FirestoreData]
public sealed class Output
{
    [FirestoreProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [FirestoreProperty("sensitive")]
    [JsonPropertyName("sensitive")]
    public bool Sensitive { get; init; } = false;
}
