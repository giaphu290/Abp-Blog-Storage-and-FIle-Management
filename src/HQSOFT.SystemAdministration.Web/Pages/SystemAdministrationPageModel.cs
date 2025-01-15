using HQSOFT.SystemAdministration.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace HQSOFT.SystemAdministration.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class SystemAdministrationPageModel : AbpPageModel
{
    protected SystemAdministrationPageModel()
    {
        LocalizationResourceType = typeof(SystemAdministrationResource);
        ObjectMapperContext = typeof(SystemAdministrationWebModule);
    }
}
