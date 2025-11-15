using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Anamnesis.Pages;

public class ModulesModel : PageModel
{
    public required string Namespace { get; set; }
    public required string[] Modules { get; set; }

    public async Task OnGet(string ns)
    {
        Namespace = ns;

        Modules = await GoogleCloud.GoogleCloud.ListFolders(
            Environment.GetEnvironmentVariable("ANAMNESIS_BUCKET")!,
            $"{ns}/modules"
        );
    }
}
