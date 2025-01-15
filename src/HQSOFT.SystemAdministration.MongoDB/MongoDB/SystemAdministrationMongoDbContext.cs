using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace HQSOFT.SystemAdministration.MongoDB;

[ConnectionStringName(SystemAdministrationDbProperties.ConnectionStringName)]
public class SystemAdministrationMongoDbContext : AbpMongoDbContext, ISystemAdministrationMongoDbContext
{
    /* Add mongo collections here. Example:
     * public IMongoCollection<Question> Questions => Collection<Question>();
     */

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSystemAdministration();
    }
}
