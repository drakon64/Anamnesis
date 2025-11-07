using System.Text.Json.Serialization;

using Anamnesis.Server.GoogleCloud;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, SourceGenerationContext.Default);
});

var app = builder.Build();

var bucket =
    Environment.GetEnvironmentVariable("ANAMNESIS_BUCKET")
    ?? throw new InvalidOperationException("ANAMNESIS_BUCKET is null");

app.MapGet(
    "{registryNamespace}/{name}/{system}/versions",
    async (string registryNamespace, string name, string system) =>
    {
        ModuleVersion[] versions = (
            from item in await GoogleCloud.ListObjects(
                bucket,
                $"{registryNamespace}/{name}/{system}"
            )
            select new ModuleVersion { Version = item.Name[..(item.Name.IndexOf('.') - 1)] }
        ).ToArray();

        return new Versions { Modules = [new Module { Versions = versions }] };
    }
);

app.MapGet(
    "{registryNamespace}/{name}/{system}/{version}/download",
    (HttpContext context, string registryNamespace, string name, string system, string version) =>
    {
        context.Response.Headers["X-Terraform-Get"] =
            $"{registryNamespace}/{name}/{system}/{version}";

        return Results.NoContent();
    }
);

app.Run();

internal class Versions
{
    [JsonPropertyName("modules")]
    public required Module[] Modules { get; init; }
}

internal class Module
{
    [JsonPropertyName("versions")]
    public required ModuleVersion[] Versions { get; init; }
}

internal class ModuleVersion
{
    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

[JsonSerializable(typeof(Versions))]
[JsonSerializable(typeof(GoogleCloud.AccessTokenResponse))]
[JsonSerializable(typeof(GoogleCloud.ListObjectsResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext;
