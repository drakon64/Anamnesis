using Google.Cloud.Firestore;

namespace Anamnesis.Dashboard;

internal static class Program
{
    internal static readonly CollectionReference ModulesCollection = new FirestoreDbBuilder
    {
        ProjectId =
            Environment.GetEnvironmentVariable("ANAMNESIS_PROJECT")
            ?? throw new InvalidOperationException("ANAMNESIS_PROJECT is null"),

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
