using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using HQSOFT.CoreBackend.EntityFrameworkCore;
using Volo.Abp.BlobStoring;
using Volo.FileManagement;
//using HQSOFT.Common.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;

namespace HQSOFT.SystemAdministration.EntityFrameworkCore;

[DependsOn(
    typeof(SystemAdministrationDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
[DependsOn(typeof(CoreBackendEntityFrameworkCoreModule))]
//[DependsOn(typeof(CommonEntityFrameworkCoreModule))]
[DependsOn(typeof(BlobStoringDatabaseEntityFrameworkCoreModule))]
    public class SystemAdministrationEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

        context.Services.AddAbpDbContext<SystemAdministrationDbContext>(options =>
        {
                /* Add custom repositories here. Example:
                 * options.AddRepository<Question, EfCoreQuestionRepository>();
                 */
        });
    }
}
