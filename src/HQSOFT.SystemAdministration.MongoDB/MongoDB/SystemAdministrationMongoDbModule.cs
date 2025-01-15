using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using Volo.Abp.BlobStoring.Database.MongoDB;

namespace HQSOFT.SystemAdministration.MongoDB;

[DependsOn(
    typeof(SystemAdministrationDomainModule),
    typeof(AbpMongoDbModule)
    )]
[DependsOn(typeof(BlobStoringDatabaseMongoDbModule))]
    public class SystemAdministrationMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<SystemAdministrationMongoDbContext>(options =>
        {
                /* Add custom repositories here. Example:
                 * options.AddRepository<Question, MongoQuestionRepository>();
                 */
        });
    }
}
