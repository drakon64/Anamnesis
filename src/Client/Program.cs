using System.CommandLine;
using System.Diagnostics;
using System.IO.Compression;
using System.IO.Hashing;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;

var namespaceArgument = new Argument<string>("namespace")
{
    Description = "The namespace to upload the module to",
};

var nameArgument = new Argument<string>("name")
{
    Description = "The name of the module to upload",
};

var systemArgument = new Argument<string>("system")
{
    Description = "The system of the module to upload",
};

var versionArgument = new Argument<string>("version")
{
    Description = "The version of the module to upload",
};

var summaryArgument = new Argument<string>("summary")
{
    Description = "The summary of the module to upload",
};

var sourceArgument = new Argument<string>("source")
{
    Description = "The source of the module to upload",
};

var directoryArgument = new Argument<DirectoryInfo>("directory")
{
    Description = "The directory to upload as a module",
};

var latestOption = new Option<bool>("--no-latest")
{
    Description = "Don't mark the uploaded module as the latest",
};

var bucketOption = new Option<string>("--bucket")
{
    Description = "The Google Cloud Storage bucket to upload the module to",
    DefaultValueFactory = _ => Environment.GetEnvironmentVariable("ANAMNESIS_BUCKET"),
};

var databaseOption = new Option<string>("--database")
{
    Description = "The Firestore database to store information about the module in",
    DefaultValueFactory = _ => Environment.GetEnvironmentVariable("ANAMNESIS_DATABASE"),
};

var projectOption = new Option<string>("--project")
{
    Description = "The Google Cloud project that contains the Firestore database",
    DefaultValueFactory = _ => Environment.GetEnvironmentVariable("ANAMNESIS_PROJECT"),
};

var rootCommand = new RootCommand("Anamnesis registry client");
rootCommand.Arguments.Add(namespaceArgument);
rootCommand.Arguments.Add(nameArgument);
rootCommand.Arguments.Add(systemArgument);
rootCommand.Arguments.Add(versionArgument);
rootCommand.Arguments.Add(directoryArgument);
rootCommand.Arguments.Add(summaryArgument);
rootCommand.Arguments.Add(sourceArgument);

rootCommand.Options.Add(bucketOption);
rootCommand.Options.Add(databaseOption);
rootCommand.Options.Add(projectOption);
rootCommand.Options.Add(latestOption);

rootCommand.SetAction(async parseResult =>
{
    await Console.Out.WriteLineAsync("Uploading module to Cloud Storage");

    var directory = parseResult.GetRequiredValue(directoryArgument);

    using var moduleZip = new MemoryStream();
    await ZipFile.CreateFromDirectoryAsync(
        directory.FullName,
        moduleZip,
        CompressionLevel.SmallestSize,
        false
    );

    var ns = parseResult.GetRequiredValue(namespaceArgument);
    var name = parseResult.GetRequiredValue(nameArgument);
    var system = parseResult.GetRequiredValue(systemArgument);
    var version = parseResult.GetRequiredValue(versionArgument);

    using var storage = await StorageClient.CreateAsync();

    await storage.UploadObjectAsync(
        parseResult.GetRequiredValue(bucketOption),
        $"{ns}/modules/{name}/{system}/{version}.zip",
        "application/zip",
        moduleZip
    );

    await Console.Out.WriteLineAsync("Storing module information in Firestore");

    var database = new FirestoreDbBuilder
    {
        ProjectId = parseResult.GetRequiredValue(projectOption),
        DatabaseId = parseResult.GetRequiredValue(databaseOption),
    }.Build();

    var path = Convert.ToHexStringLower(
        XxHash3.Hash(Encoding.Default.GetBytes($"{ns}/{name}/{system}/{version}"))
    );

    var processStartInfo = new ProcessStartInfo { FileName = "terraform-config-inspect" };
    processStartInfo.ArgumentList.Add(directory.FullName);
    processStartInfo.ArgumentList.Add("--json");
    processStartInfo.RedirectStandardOutput = true;

    using var process = Process.Start(processStartInfo);
    await process!.WaitForExitAsync();

    var config = await JsonSerializer.DeserializeAsync<ModuleConfig>(
        process.StandardOutput.BaseStream
    );

    StreamReader readme;

    try
    {
        readme = (from file in directory.GetFiles() where file.Name == "README.md" select file)
            .ToArray()[0]
            .OpenText();
    }
    catch (IndexOutOfRangeException)
    {
        throw new FileNotFoundException("Directory does not contain a `README.md` file");
    }

    var document = new Module
    {
        Namespace = ns,
        Name = name,
        System = system,
        Version = version,
        Summary = parseResult.GetRequiredValue(summaryArgument),
        Variables = config!.Variables,
        Outputs = config.Outputs,
        Source = parseResult.GetRequiredValue(sourceArgument),
        Readme = await readme.ReadToEndAsync(),
        Latest = !parseResult.GetRequiredValue(latestOption),
    };

    readme.Dispose();

    await database.Collection("modules").Document(path).SetAsync(document);
});

return rootCommand.Parse(args).Invoke();

[FirestoreData]
internal sealed class Module
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

    [FirestoreProperty("variables")]
    public required Dictionary<string, Variable> Variables { get; init; }

    [FirestoreProperty("outputs")]
    public required Dictionary<string, Output> Outputs { get; init; }

    [FirestoreProperty("source")]
    public required string Source { get; init; }

    [FirestoreProperty("readme")]
    public required string Readme { get; init; }

    [FirestoreProperty("latest")]
    public required bool Latest { get; init; }
}

internal sealed class ModuleConfig
{
    [JsonPropertyName("variables")]
    public required Dictionary<string, Variable> Variables { get; init; }

    [JsonPropertyName("outputs")]
    public required Dictionary<string, Output> Outputs { get; init; }
}

[FirestoreData]
internal sealed class Variable
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
internal sealed class Output
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
