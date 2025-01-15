using HQSOFT.SystemAdministration.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace HQSOFT.SystemAdministration.Containers
{
    public class EfCoreContainerRepository :
        EfCoreRepository<SystemAdministrationDbContext, Container, Guid>,
        IContainerRepository

    {
        public EfCoreContainerRepository(
             IDbContextProvider<SystemAdministrationDbContext> dbContextProvider)
           : base(dbContextProvider)
        {
        }
        public async Task<Container?> FindByNameAsync(string name)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<Container?> GetContainerByPath(string path)
        {
            var dbSet = await GetDbSetAsync();
             return await dbSet.FirstOrDefaultAsync(x => x.Name.ToLower().EndsWith(path.ToLower()));
        }

        public async Task<List<Container>> GetListAsync(int skipCount, int maxResultCount, string sorting, string? filter = null)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .WhereIf(
                    !filter.IsNullOrWhiteSpace(),
                    container => container.Name.Contains(filter!) || container.TypeStorage.Contains(filter!)
                 )
                .OrderBy(sorting)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }

        public async Task<List<Container>> GetListContainerAsync()
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.ToListAsync();
        }
    }
}
