using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace HQSOFT.SystemAdministration.Seed;

public class SystemAdministrationHttpApiHostDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly SystemAdministrationSampleDataSeeder _systemAdministrationSampleDataSeeder;
    private readonly ICurrentTenant _currentTenant;

    public SystemAdministrationHttpApiHostDataSeedContributor(
        SystemAdministrationSampleDataSeeder systemAdministrationSampleDataSeeder,
        ICurrentTenant currentTenant)
    {
        _systemAdministrationSampleDataSeeder = systemAdministrationSampleDataSeeder;
        _currentTenant = currentTenant;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            await _systemAdministrationSampleDataSeeder.SeedAsync(context!);
        }
    }
}
