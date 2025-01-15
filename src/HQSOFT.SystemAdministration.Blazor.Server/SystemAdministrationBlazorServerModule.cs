using Volo.Abp.AspNetCore.Components.Server.Theming;
using Volo.Abp.Modularity;
using HQSOFT.Common.Blazor.Server;
using Volo.FileManagement.Blazor.Server;

namespace HQSOFT.SystemAdministration.Blazor.Server;

[DependsOn(
    typeof(SystemAdministrationBlazorModule),
    typeof(FileManagementBlazorServerModule),
    typeof(AbpAspNetCoreComponentsServerThemingModule)
    )]
[DependsOn (typeof(CommonBlazorServerModule))]
public class SystemAdministrationBlazorServerModule : AbpModule
{
}
