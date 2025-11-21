using System.Text.Json.Serialization;

using Anamnesis.Dashboard.GoogleCloud;

using Google.Cloud.Firestore;

namespace Anamnesis.Dashboard;

internal class Program
{
    internal static readonly string Bucket =
        Environment.GetEnvironmentVariable("ANAMNESIS_BUCKET")
        ?? throw new InvalidOperationException("ANAMNESIS_BUCKET is null");

    internal static readonly CollectionReference ModulesCollection = new FirestoreDbBuilder
    {
        ProjectId =
            Environment.GetEnvironmentVariable("ANAMNESIS_PROJECT")
            ?? throw new InvalidOperationException("ANAMNESIS_PROJECT is null"),

        DatabaseId = Environment.GetEnvironmentVariable("ANAMNESIS_DATABASE"),
    }
        .Build()
        .Collection("modules");

    private static void Main()
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.Services.AddRazorPages();

        var app = builder.Build();

        app.MapRazorPages();

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}

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

public sealed class ModuleConfig
{
    [JsonPropertyName("variables")]
    public required Dictionary<string, Variable> Variables { get; init; }

    [JsonPropertyName("outputs")]
    public required Dictionary<string, Output> Outputs { get; init; }
}

[FirestoreData]
public sealed class Variable
{
    [FirestoreProperty("name")]
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [FirestoreProperty("type")]
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [FirestoreProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [FirestoreProperty("required")]
    [JsonPropertyName("required")]
    public required bool Required { get; init; }

    [FirestoreProperty("sensitive")]
    [JsonPropertyName("sensitive")]
    public bool Sensitive { get; init; } = false;
}

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

[JsonSerializable(typeof(AccessTokenResponse))]
[JsonSerializable(typeof(ListFoldersResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext;
