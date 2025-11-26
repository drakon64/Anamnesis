using System.Text.Json.Serialization;

using Google.Cloud.Firestore;

namespace Anamnesis.Dashboard.Model;

[FirestoreData]
public sealed class Variable
{
    [FirestoreProperty("name")]
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [FirestoreProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [FirestoreProperty("type")]
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [FirestoreProperty("default")]
    [JsonPropertyName("default")]
    public string Default { get; init; }

    [FirestoreProperty("required")]
    [JsonPropertyName("required")]
    public required bool Required { get; init; }

    [FirestoreProperty("sensitive")]
    [JsonPropertyName("sensitive")]
    public bool Sensitive { get; init; } = false;
}
