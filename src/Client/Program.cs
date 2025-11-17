using System.CommandLine;

var directoryArgument = new Argument<DirectoryInfo>("directory")
{
    Description = "The directory to upload as a module"
};

var latestOption = new Option<bool>("--no-latest")
{
    Description = "Don't mark the uploaded module as the latest"
};

var rootCommand = new RootCommand("Anamnesis registry client");
rootCommand.Arguments.Add(directoryArgument);
rootCommand.Options.Add(latestOption);

return rootCommand.Parse(args).Invoke();
