using Anamnesis.Dashboard.Model;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Anamnesis.Dashboard.Pages;

public class IndexModel : PageModel
{
    public required IEnumerable<string> Namespaces { get; set; }

    public async Task OnGet()
    {
        Namespaces = (
            from module in await Program.ModulesCollection.GetSnapshotAsync()
            select module.ConvertTo<Module>().Namespace
        ).Distinct();
    }
}
