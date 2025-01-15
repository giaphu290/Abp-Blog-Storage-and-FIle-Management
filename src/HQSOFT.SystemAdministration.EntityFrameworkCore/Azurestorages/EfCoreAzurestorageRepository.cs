
using HQSOFT.SystemAdministration.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    public class EfCoreAzurestorageRepository : EfCoreRepository<SystemAdministrationDbContext, Azurestorage, Guid>,
        IAzurestorageRepository
    {
        public EfCoreAzurestorageRepository(
            IDbContextProvider<SystemAdministrationDbContext> dbContextProvider)
        : base(dbContextProvider) { }
        public async Task<Azurestorage?> FindByNameAsync(string containerName)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.FirstOrDefaultAsync(x => x.ContainerName == containerName);
        }

        public async Task<List<Azurestorage>> GetListAsync(int skipCount, int maxResultCount, string sorting, string filter = null)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .WhereIf(
                    !filter.IsNullOrWhiteSpace(),
                    x => x.ContainerName.Contains(filter)
                    )
                .OrderBy(sorting)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }
    }
}
