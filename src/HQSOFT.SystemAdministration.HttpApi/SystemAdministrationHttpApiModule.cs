using Localization.Resources.AbpUi;
using HQSOFT.SystemAdministration.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using HQSOFT.CoreBackend;
using Volo.FileManagement;
//using HQSOFT.Common;

namespace HQSOFT.SystemAdministration;

[DependsOn(
    typeof(SystemAdministrationApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]

[DependsOn(typeof(CoreBackendHttpApiModule))]
//[DependsOn(typeof(CommonHttpApiModule))]
public class SystemAdministrationHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(SystemAdministrationHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<SystemAdministrationResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
