using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;
using HQSOFT.CoreBackend;
//using HQSOFT.Common;
using Volo.FileManagement;

namespace HQSOFT.SystemAdministration;

[DependsOn(
    typeof(SystemAdministrationDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationAbstractionsModule),
    typeof(FileManagementApplicationContractsModule)
    )]
[DependsOn(typeof(CoreBackendApplicationContractsModule))]
//[DependsOn(typeof(CommonApplicationContractsModule))]
public class SystemAdministrationApplicationContractsModule : AbpModule
{

}
