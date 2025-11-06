using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(
        0,
        VersionsJsonSerializerContext.Default
    );
});

var app = builder.Build();

app.MapGet(
    "{namespace}/{name}/{system}/versions",
    () =>
    {
        return new Versions
        {
            Modules =
            [
                new Versions.Module
                {
                    Versions = [new Versions.Module.ModuleVersion { Version = "0.0.1" }],
                },
            ],
        };
    }
);

app.MapGet(
    "{moduleNamespace}/{name}/{system}/{version}/download",
    (HttpContext context, string moduleNamespace, string name, string system, string version) =>
    {
        context.Response.Headers["X-Terraform-Get"] =
            $"{moduleNamespace}/{name}/{system}/{version}";

        return Results.NoContent();
    }
);

app.Run();

internal class Versions
{
    [JsonPropertyName("modules")]
    public required Module[] Modules { get; init; }

    internal class Module
    {
        [JsonPropertyName("versions")]
        public required ModuleVersion[] Versions { get; init; }

        internal class ModuleVersion
        {
            [JsonPropertyName("version")]
            public required string Version { get; init; }
        }
    }
}

[JsonSerializable(typeof(Versions[]))]
internal partial class VersionsJsonSerializerContext : JsonSerializerContext;
