using Volo.Abp.AspNetCore.Components.WebAssembly.Theming;
using Volo.Abp.Modularity;
using Volo.FileManagement.Blazor.WebAssembly;

namespace HQSOFT.SystemAdministration.Blazor.WebAssembly;

[DependsOn(
    typeof(SystemAdministrationBlazorModule),
    typeof(SystemAdministrationHttpApiClientModule),
    typeof(AbpAspNetCoreComponentsWebAssemblyThemingModule)
)] 
public class SystemAdministrationBlazorWebAssemblyModule : AbpModule
{

}
