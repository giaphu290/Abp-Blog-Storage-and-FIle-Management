using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace HQSOFT.SystemAdministration.MongoDB;

[ConnectionStringName(SystemAdministrationDbProperties.ConnectionStringName)]
public interface ISystemAdministrationMongoDbContext : IAbpMongoDbContext
{
    /* Define mongo collections here. Example:
     * IMongoCollection<Question> Questions { get; }
     */
}
