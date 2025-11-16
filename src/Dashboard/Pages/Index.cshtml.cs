using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Anamnesis.Dashboard.Pages;

public class IndexModel : PageModel
{
    public required string[] Namespaces { get; set; }

    public async Task OnGet()
    {
        Namespaces = await GoogleCloud.GoogleCloud.ListFolders(Program.Bucket);
    }
}
