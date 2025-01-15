using System;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace HQSOFT.SystemAdministration.MongoDB;

[DependsOn(
    typeof(SystemAdministrationApplicationTestModule),
    typeof(SystemAdministrationMongoDbModule)
)]
public class SystemAdministrationMongoDbTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = MongoDbFixture.GetRandomConnectionString();
        });
    }
}
