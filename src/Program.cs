using System.Text.Json.Serialization;

using Anamnesis.GoogleCloud;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, SourceGenerationContext.Default);
});

var app = builder.Build();

var serviceIdentifiers = new ServiceIdentifiers();

var bucket =
    Environment.GetEnvironmentVariable("ANAMNESIS_BUCKET")
    ?? throw new InvalidOperationException("ANAMNESIS_BUCKET is null");

app.MapGet("/.well-known/terraform.json", () => serviceIdentifiers);

app.MapGet(
    "/{registryNamespace}/{name}/{system}/versions",
    async (string registryNamespace, string name, string system) =>
    {
        var versions = (
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
    "/{registryNamespace}/{name}/{system}/{version}/download",
    (HttpContext context, string registryNamespace, string name, string system, string version) =>
    {
        context.Response.Headers["X-Terraform-Get"] =
            $"gcs::https://www.googleapis.com/storage/v1/{bucket}/{registryNamespace}/{name}/{system}/{version}.zip";

        return Results.NoContent();
    }
);

app.Run();

internal sealed class ServiceIdentifiers
{
    [JsonInclude]
    [JsonPropertyName("modules.v1")]
    public readonly string ModulesV1 = "/terraform/modules/v1/";
}

internal sealed class Versions
{
    [JsonPropertyName("modules")]
    public required Module[] Modules { get; init; }
}

internal sealed class Module
{
    [JsonPropertyName("versions")]
    public required ModuleVersion[] Versions { get; init; }
}

internal sealed class ModuleVersion
{
    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

[JsonSerializable(typeof(ServiceIdentifiers))]
[JsonSerializable(typeof(Versions))]
[JsonSerializable(typeof(GoogleCloud.AccessTokenResponse))]
[JsonSerializable(typeof(GoogleCloud.ListObjectsResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext;
