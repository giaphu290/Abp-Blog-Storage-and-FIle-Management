using Microsoft.Extensions.DependencyInjection;
using HQSOFT.SystemAdministration.Blazor.Menus;
using Volo.Abp.AspNetCore.Components.Web.Theming;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation;
using HQSOFT.Common.Blazor;
using Volo.Abp.Auditing;
using Volo.FileManagement.Blazor;

namespace HQSOFT.SystemAdministration.Blazor;

[DependsOn(
    typeof(SystemAdministrationApplicationContractsModule),
    typeof(AbpAspNetCoreComponentsWebThemingModule),
    typeof(AbpAutoMapperModule),
    typeof(FileManagementBlazorModule)
    )]
[DependsOn (typeof(CommonBlazorModule))]
public class SystemAdministrationBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    { 
        context.Services.AddAutoMapperObjectMapper<SystemAdministrationBlazorModule>();
        
        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabledForGetRequests = false; 
        });

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddProfile<SystemAdministrationBlazorAutoMapperProfile>(validate: true);
        });

        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new SystemAdministrationMenuContributor());
        });

        Configure<AbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SystemAdministrationBlazorModule).Assembly);
        });
    }
}
