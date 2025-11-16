using System.Text.Json.Serialization;

using Anamnesis.Dashboard.GoogleCloud;

namespace Anamnesis.Dashboard;

internal class Program
{
    internal static readonly string Bucket =
        Environment.GetEnvironmentVariable("ANAMNESIS_BUCKET")
        ?? throw new InvalidOperationException("ANAMNESIS_BUCKET is null");

    private static void Main()
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.Services.AddRazorPages();

        var app = builder.Build();

        app.MapRazorPages();

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}

[JsonSerializable(typeof(AccessTokenResponse))]
[JsonSerializable(typeof(ListFoldersResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext;
