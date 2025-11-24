using Google.Cloud.Firestore;

namespace Anamnesis.Dashboard.Model;

[FirestoreData]
public sealed class Module
{
    [FirestoreProperty("namespace")]
    public required string Namespace { get; init; }

    [FirestoreProperty("name")]
    public required string Name { get; init; }

    [FirestoreProperty("system")]
    public required string System { get; init; }

    [FirestoreProperty("version")]
    public required string Version { get; init; }

    [FirestoreProperty("variables")]
    public required Dictionary<string, Variable> Variables { get; init; }

    [FirestoreProperty("outputs")]
    public required Dictionary<string, Output> Outputs { get; init; }

    [FirestoreProperty("source")]
    public required string Source { get; init; }

    [FirestoreProperty("summary")]
    public required string Summary { get; init; }

    [FirestoreProperty("readme")]
    public required string Readme { get; init; }

    [FirestoreProperty("latest")]
    public required bool Latest { get; init; }
}
