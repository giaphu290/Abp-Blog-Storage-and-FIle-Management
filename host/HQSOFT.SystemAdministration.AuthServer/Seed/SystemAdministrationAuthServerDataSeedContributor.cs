using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace HQSOFT.SystemAdministration.Seed;

public class SystemAdministrationAuthServerDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly SystemAdministrationSampleIdentityDataSeeder _systemAdministrationSampleIdentityDataSeeder;
    private readonly SystemAdministrationAuthServerDataSeeder _systemAdministrationAuthServerDataSeeder;
    private readonly ICurrentTenant _currentTenant;

    public SystemAdministrationAuthServerDataSeedContributor(
        SystemAdministrationAuthServerDataSeeder systemAdministrationAuthServerDataSeeder,
        SystemAdministrationSampleIdentityDataSeeder systemAdministrationSampleIdentityDataSeeder,
        ICurrentTenant currentTenant)
    {
        _systemAdministrationAuthServerDataSeeder = systemAdministrationAuthServerDataSeeder;
        _systemAdministrationSampleIdentityDataSeeder = systemAdministrationSampleIdentityDataSeeder;
        _currentTenant = currentTenant;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            await _systemAdministrationSampleIdentityDataSeeder.SeedAsync(context!);
            await _systemAdministrationAuthServerDataSeeder.SeedAsync(context!);
        }
    }
}
