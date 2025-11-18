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
        ProjectId = "anamnesis-drakon64",
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

    [FirestoreProperty("summary")]
    public required string Summary { get; init; }

    [FirestoreProperty("readme")]
    public required string Readme { get; init; }

    [FirestoreProperty("latest")]
    public required bool Latest { get; init; }
}

[JsonSerializable(typeof(AccessTokenResponse))]
[JsonSerializable(typeof(ListFoldersResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext;
