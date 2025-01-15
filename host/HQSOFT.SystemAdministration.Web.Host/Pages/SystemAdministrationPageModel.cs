using HQSOFT.SystemAdministration.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace HQSOFT.SystemAdministration.Pages;

public abstract class SystemAdministrationPageModel : AbpPageModel
{
    protected SystemAdministrationPageModel()
    {
        LocalizationResourceType = typeof(SystemAdministrationResource);
    }
}
