using HQSOFT.CoreBackend.Localization;
using HQSOFT.SystemAdministration.Localization;
using Volo.Abp.AspNetCore.Components;

namespace HQSOFT.SystemAdministration.Blazor;

public abstract class SystemAdministrationComponentBase : AbpComponentBase
{
    protected SystemAdministrationComponentBase()
    {
        LocalizationResource = typeof(SystemAdministrationResource); 
        LocalizationResource = typeof(CoreBackendResource);
    }
}
