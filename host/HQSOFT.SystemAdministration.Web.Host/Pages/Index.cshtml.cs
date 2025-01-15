using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace HQSOFT.SystemAdministration.Pages;

public class IndexModel : SystemAdministrationPageModel
{
    public void OnGet()
    {

    }

    public async Task OnPostLoginAsync()
    {
        await HttpContext.ChallengeAsync("oidc");
    }
}
