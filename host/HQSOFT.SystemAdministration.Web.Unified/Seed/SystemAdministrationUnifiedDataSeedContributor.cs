using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace HQSOFT.SystemAdministration.Seed;

public class SystemAdministrationUnifiedDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly SystemAdministrationSampleIdentityDataSeeder _sampleIdentityDataSeeder;
    private readonly SystemAdministrationSampleDataSeeder _systemAdministrationSampleDataSeeder;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICurrentTenant _currentTenant;

    public SystemAdministrationUnifiedDataSeedContributor(
        SystemAdministrationSampleIdentityDataSeeder sampleIdentityDataSeeder,
        IUnitOfWorkManager unitOfWorkManager,
        SystemAdministrationSampleDataSeeder systemAdministrationSampleDataSeeder,
        ICurrentTenant currentTenant)
    {
        _sampleIdentityDataSeeder = sampleIdentityDataSeeder;
        _unitOfWorkManager = unitOfWorkManager;
        _systemAdministrationSampleDataSeeder = systemAdministrationSampleDataSeeder;
        _currentTenant = currentTenant;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await _unitOfWorkManager.Current!.SaveChangesAsync();

        using (_currentTenant.Change(context.TenantId))
        {
            await _sampleIdentityDataSeeder.SeedAsync(context);
            await _unitOfWorkManager.Current.SaveChangesAsync();
            await _systemAdministrationSampleDataSeeder.SeedAsync(context);
        }
    }
}
