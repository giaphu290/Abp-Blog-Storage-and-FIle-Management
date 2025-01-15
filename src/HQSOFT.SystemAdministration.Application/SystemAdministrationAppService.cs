using HQSOFT.CoreBackend.Localization;
using HQSOFT.SystemAdministration.Localization;
using Volo.Abp.Application.Services;

namespace HQSOFT.SystemAdministration;

public abstract class SystemAdministrationAppService : ApplicationService
{
    protected SystemAdministrationAppService()
    {
        LocalizationResource = typeof(CoreBackendResource); //SystemAdministrationResource
        ObjectMapperContext = typeof(SystemAdministrationApplicationModule);
    }
}
