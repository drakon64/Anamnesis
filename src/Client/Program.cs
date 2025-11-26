using System.CommandLine;
using System.Diagnostics;
using System.IO.Compression;
using System.IO.Hashing;
using System.Text;
using System.Text.Json;

using Anamnesis.Client.Model;

using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;

var namespaceOption = new Option<string>("--namespace")
{
    Description = "The namespace to upload the module to",
    Required = true,
};

var nameOption = new Option<string>("--name")
{
    Description = "The name of the module to upload",
    Required = true,
};

var systemOption = new Option<string>("--system")
{
    Description = "The system of the module to upload",
    Required = true,
};

var versionOption = new Option<string>("--module-version")
{
    Description = "The version of the module to upload",
    Required = true,
};

var directoryOption = new Option<DirectoryInfo>("--directory")
{
    Description = "The directory to upload as a module",
    Required = true,
};

var summaryOption = new Option<string>("--summary")
{
    Description = "The summary of the module to upload",
    Required = true,
};

var sourceOption = new Option<string>("--source")
{
    Description = "The source of the module to upload",
    Required = true,
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
rootCommand.Options.Add(namespaceOption);
rootCommand.Options.Add(nameOption);
rootCommand.Options.Add(systemOption);
rootCommand.Options.Add(versionOption);
rootCommand.Options.Add(directoryOption);
rootCommand.Options.Add(summaryOption);
rootCommand.Options.Add(sourceOption);

rootCommand.Options.Add(latestOption);
rootCommand.Options.Add(bucketOption);
rootCommand.Options.Add(databaseOption);
rootCommand.Options.Add(projectOption);

rootCommand.SetAction(async parseResult =>
{
    await Console.Out.WriteLineAsync("Uploading module to Cloud Storage");

    var directory = parseResult.GetRequiredValue(directoryOption);

    using var moduleZip = new MemoryStream();
    await ZipFile.CreateFromDirectoryAsync(
        directory.FullName,
        moduleZip,
        CompressionLevel.SmallestSize,
        false
    );

    var ns = parseResult.GetRequiredValue(namespaceOption);
    var name = parseResult.GetRequiredValue(nameOption);
    var system = parseResult.GetRequiredValue(systemOption);
    var version = parseResult.GetRequiredValue(versionOption);

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

    var configJson = await process.StandardOutput.ReadToEndAsync();
    var config = JsonSerializer.Deserialize<ModuleConfig>(configJson);
    var configJsonPath = Path.GetTempFileName();
    var configJsonFile = new FileInfo(configJsonPath);
    await using var configJsonStream = configJsonFile.CreateText();
    await configJsonStream.WriteAsync(configJson);
    configJsonStream.Close();

    foreach (var variable in config!.Variables)
    {
        var jqProcessStartInfo = new ProcessStartInfo { FileName = "jq" };
        jqProcessStartInfo.ArgumentList.Add("-cM");
        jqProcessStartInfo.ArgumentList.Add($".variables.{variable.Key}.default");
        jqProcessStartInfo.ArgumentList.Add(configJsonPath);
        jqProcessStartInfo.RedirectStandardOutput = true;

        using var jq = Process.Start(jqProcessStartInfo);
        await jq!.WaitForExitAsync();

        config.Variables[variable.Key].Default = jq.StandardOutput.ReadToEnd().TrimEnd();
    }

    File.Delete(configJsonPath);

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

    var latest = !parseResult.GetRequiredValue(latestOption);

    if (latest)
    {
        var snapshots = await database
            .Collection("modules")
            .WhereEqualTo("namespace", ns)
            .WhereEqualTo("name", name)
            .WhereEqualTo("system", system)
            .WhereEqualTo("latest", true)
            .GetSnapshotAsync();

        var batch = database.StartBatch();

        foreach (var snapshot in snapshots)
        {
            batch.Update(
                snapshot.Reference,
                new Dictionary<string, object> { { "latest", false } }
            );
        }

        await batch.CommitAsync();
    }

    var document = new Module
    {
        Namespace = ns,
        Name = name,
        System = system,
        Version = version,
        Summary = parseResult.GetRequiredValue(summaryOption),
        Variables = config.Variables,
        Outputs = config.Outputs,
        Source = parseResult.GetRequiredValue(sourceOption),
        Readme = await readme.ReadToEndAsync(),
        Latest = latest,
    };

    readme.Dispose();

    await database.Collection("modules").Document(path).SetAsync(document);
});

return rootCommand.Parse(args).Invoke();
