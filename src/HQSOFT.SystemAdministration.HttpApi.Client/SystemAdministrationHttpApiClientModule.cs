using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using HQSOFT.CoreBackend;
using Volo.FileManagement;
//using HQSOFT.Common;

namespace HQSOFT.SystemAdministration;

[DependsOn(
    typeof(SystemAdministrationApplicationContractsModule),
    typeof(AbpHttpClientModule))]
[DependsOn(typeof(CoreBackendHttpApiClientModule))]
//[DependsOn(typeof(CommonHttpApiClientModule))]//
public class SystemAdministrationHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SystemAdministrationApplicationContractsModule).Assembly,
            SystemAdministrationRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SystemAdministrationHttpApiClientModule>();
        });
    }
}
