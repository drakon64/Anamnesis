using System.Text.Json.Serialization;

using Anamnesis.GoogleCloud;
using Anamnesis.Module;

namespace Anamnesis;

internal class Program
{
    internal static readonly string Bucket =
        Environment.GetEnvironmentVariable("ANAMNESIS_BUCKET")
        ?? throw new InvalidOperationException("ANAMNESIS_BUCKET is null");

    private static void Main()
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(
                0,
                SourceGenerationContext.Default
            );
        });

        builder.Services.AddRazorPages();

        var app = builder.Build();

        var remoteServiceDiscovery = new RemoteServiceDiscovery();

        app.MapGet("/.well-known/terraform.json", () => remoteServiceDiscovery);

        app.MapGet(
            remoteServiceDiscovery.ModulesV1 + "{registryNamespace}/{name}/{system}/versions",
            async (string registryNamespace, string name, string system) =>
            {
                var versions = (
                    from item in await GoogleCloud.GoogleCloud.ListObjects(
                        Bucket,
                        $"{registryNamespace}/modules/{name}/{system}/"
                    )
                    where item.ContentType == "application/zip"
                    select new ModuleVersion
                    {
                        Version = item.Name[
                            (item.Name.LastIndexOf('/') + 1)..item.Name.LastIndexOf('.')
                        ],
                    }
                ).ToArray();

                return new ModuleVersions { Modules = [new Module.Module { Versions = versions }] };
            }
        );

        app.MapGet(
            remoteServiceDiscovery.ModulesV1
                + "{registryNamespace}/{name}/{system}/{version}/download",
            (
                HttpContext context,
                string registryNamespace,
                string name,
                string system,
                string version
            ) =>
            {
                context.Response.Headers["X-Terraform-Get"] =
                    $"gcs::https://www.googleapis.com/storage/v1/{Bucket}/{registryNamespace}/modules/{name}/{system}/{version}.zip";

                return Results.NoContent();
            }
        );

        app.MapRazorPages();

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}

internal sealed class RemoteServiceDiscovery
{
    [JsonInclude]
    [JsonPropertyName("modules.v1")]
    public readonly string ModulesV1 = "/terraform/modules/v1/";
}

[JsonSerializable(typeof(RemoteServiceDiscovery))]
[JsonSerializable(typeof(ModuleVersions))]
[JsonSerializable(typeof(AccessTokenResponse))]
[JsonSerializable(typeof(ListObjectsResponse))]
[JsonSerializable(typeof(ListFoldersResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext;
