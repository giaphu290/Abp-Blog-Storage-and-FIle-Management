
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace HQSOFT.SystemAdministration.EntityFrameworkCore;

[ConnectionStringName(SystemAdministrationDbProperties.ConnectionStringName)]
public interface ISystemAdministrationDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */

}
