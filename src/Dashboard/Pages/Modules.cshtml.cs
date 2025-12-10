using Anamnesis.Dashboard.Model;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Anamnesis.Dashboard.Pages;

public class ModulesModel : PageModel
{
    public required string Namespace { get; set; }
    public required IEnumerable<Module> Modules { get; set; }

    public async Task OnGet(string ns)
    {
        Namespace = ns;

        Modules =
            from module in await Program
                .ModulesCollection.WhereEqualTo("namespace", ns)
                .WhereEqualTo("latest", true)
                .GetSnapshotAsync()
            select module.ConvertTo<Module>();
    }
}
