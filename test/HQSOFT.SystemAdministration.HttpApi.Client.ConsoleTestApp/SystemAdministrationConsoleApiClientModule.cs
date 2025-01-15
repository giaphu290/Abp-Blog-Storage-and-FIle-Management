using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace HQSOFT.SystemAdministration;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SystemAdministrationHttpApiClientModule),
    typeof(AbpHttpClientIdentityModelModule)
    )]
public class SystemAdministrationConsoleApiClientModule : AbpModule
{

}
