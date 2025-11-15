using System.Text.Json;
using System.Text.Json.Serialization;

using Anamnesis.GoogleCloud;
using Anamnesis.Module;
using Anamnesis.Provider;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, SourceGenerationContext.Default);
});

builder.Services.AddRazorPages();

var app = builder.Build();

var remoteServiceDiscovery = new RemoteServiceDiscovery();

var bucket =
    Environment.GetEnvironmentVariable("ANAMNESIS_BUCKET")
    ?? throw new InvalidOperationException("ANAMNESIS_BUCKET is null");

app.MapGet("/.well-known/terraform.json", () => remoteServiceDiscovery);

app.MapGet(
    remoteServiceDiscovery.ModulesV1 + "{registryNamespace}/{name}/{system}/versions",
    async (string registryNamespace, string name, string system) =>
    {
        var versions = (
            from item in await GoogleCloud.ListObjects(
                bucket,
                $"{registryNamespace}/modules/{name}/{system}/"
            )
            where item.ContentType == "application/zip"
            select new ModuleVersion
            {
                Version = item.Name[(item.Name.LastIndexOf('/') + 1)..item.Name.LastIndexOf('.')],
            }
        ).ToArray();

        return new ModuleVersions { Modules = [new Module { Versions = versions }] };
    }
);

app.MapGet(
    remoteServiceDiscovery.ModulesV1 + "{registryNamespace}/{name}/{system}/{version}/download",
    (HttpContext context, string registryNamespace, string name, string system, string version) =>
    {
        context.Response.Headers["X-Terraform-Get"] =
            $"gcs::https://www.googleapis.com/storage/v1/{bucket}/{registryNamespace}/modules/{name}/{system}/{version}.zip";

        return Results.NoContent();
    }
);

app.MapGet(
    remoteServiceDiscovery.ProvidersV1 + "{registryNamespace}/{type}/versions",
    async (string registryNamespace, string type) =>
    {
        var versions = (
            from storageObject in await GoogleCloud.ListObjects(
                bucket,
                $"{registryNamespace}/providers/{type}/"
            )
            where storageObject.ContentType == "application/zip"
            select new ProviderVersion
            {
                Version = storageObject.Name[
                    (storageObject.Name.LastIndexOf('/') + 1)..storageObject.Name.LastIndexOf('.')
                ],
                Protocols = JsonSerializer.Deserialize(
                    storageObject.Metadata.Protocols,
                    SourceGenerationContext.Default.StringArray
                ),
                Platforms = JsonSerializer.Deserialize(
                    storageObject.Metadata.Platforms,
                    SourceGenerationContext.Default.PlatformArray
                ),
            }
        ).ToArray();

        return new ProviderVersions { Versions = versions };
    }
);

app.MapGet(
    remoteServiceDiscovery.ProvidersV1
        + "{registryNamespace}/{type}/{version}/download/{os}/{arch}",
    async (string registryNamespace, string type, string version, string os, string arch) =>
    {
        var storageObject = await GoogleCloud.GetObject(
            bucket,
            $"{registryNamespace}/providers/{type}/{version}/{registryNamespace}_{type}_{version}_{os}_{arch}.zip"
        );

        var fileName = storageObject.Name[(storageObject.Name.LastIndexOf('/') + 1)..];
        var url =
            $"gcs::https://www.googleapis.com/storage/v1/{bucket}/{registryNamespace}/providers/{type}/{version}/";

        return new ProviderPackage
        {
            Protocols = JsonSerializer.Deserialize(
                storageObject.Metadata.Protocols,
                SourceGenerationContext.Default.StringArray
            ),
            Os = storageObject.Metadata.Os,
            Arch = storageObject.Metadata.Arch,
            Filename = fileName,
            DownloadUrl = $"{url}{fileName}",
            ShasumsUrl = $"{url}{registryNamespace}_{type}_{version}_{os}_{arch}_SHA256SUMS",
            ShasumsSignatureUrl =
                $"{url}{registryNamespace}_{type}_{version}_{os}_{arch}_SHA256SUMS.sig",
            Shasum = storageObject.Metadata.Shasum,
            SigningKeys = JsonSerializer.Deserialize(
                storageObject.Metadata.SigningKeys,
                SourceGenerationContext.Default.SigningKeys
            ),
        };
    }
);

app.MapRazorPages();

app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");

internal sealed class RemoteServiceDiscovery
{
    [JsonInclude]
    [JsonPropertyName("modules.v1")]
    public readonly string ModulesV1 = "/terraform/modules/v1/";

    [JsonInclude]
    [JsonPropertyName("providers.v1")]
    public readonly string ProvidersV1 = "/terraform/providers/v1/";
}

[JsonSerializable(typeof(RemoteServiceDiscovery))]
[JsonSerializable(typeof(ModuleVersions))]
[JsonSerializable(typeof(ProviderVersions))]
[JsonSerializable(typeof(ProviderPackage))]
[JsonSerializable(typeof(AccessTokenResponse))]
[JsonSerializable(typeof(ListObjectsResponse))]
[JsonSerializable(typeof(ListFoldersResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext;
