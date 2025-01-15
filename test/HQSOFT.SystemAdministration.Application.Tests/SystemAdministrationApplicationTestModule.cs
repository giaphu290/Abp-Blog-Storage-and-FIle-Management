using Volo.Abp.Modularity;

namespace HQSOFT.SystemAdministration;

[DependsOn(
    typeof(SystemAdministrationApplicationModule),
    typeof(SystemAdministrationDomainTestModule)
    )]
public class SystemAdministrationApplicationTestModule : AbpModule
{

}
