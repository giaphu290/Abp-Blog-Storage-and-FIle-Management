using HQSOFT.SystemAdministration.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace HQSOFT.SystemAdministration;

public abstract class SystemAdministrationController : AbpControllerBase
{
    protected SystemAdministrationController()
    {
        LocalizationResource = typeof(SystemAdministrationResource);
    }
}
