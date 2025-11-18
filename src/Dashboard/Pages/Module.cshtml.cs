using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Anamnesis.Dashboard.Pages;

public class ModuleModel : PageModel
{
    public required string Namespace { get; set; }
    public required string Name { get; set; }
    public required string System { get; set; }
    public required string Version { get; set; }
    public required Module Module { get; set; }

    public async Task OnGet(string ns, string name, string system, string version)
    {
        Namespace = ns;
        Name = name;
        System = system;
        Version = version;

        Module = (
            from module in await Program
                .ModulesCollection.WhereEqualTo("namespace", ns)
                .WhereEqualTo("name", name)
                .WhereEqualTo("system", system)
                .WhereEqualTo("version", version)
                .GetSnapshotAsync()
            select module.ConvertTo<Module>()
        ).ToArray()[0];
    }
}
