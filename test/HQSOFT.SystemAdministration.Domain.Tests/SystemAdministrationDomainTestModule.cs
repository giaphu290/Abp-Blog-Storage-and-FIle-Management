using Volo.Abp.Modularity;

namespace HQSOFT.SystemAdministration;

[DependsOn(
    typeof(SystemAdministrationDomainModule),
    typeof(SystemAdministrationTestBaseModule)
)]
public class SystemAdministrationDomainTestModule : AbpModule
{

}
