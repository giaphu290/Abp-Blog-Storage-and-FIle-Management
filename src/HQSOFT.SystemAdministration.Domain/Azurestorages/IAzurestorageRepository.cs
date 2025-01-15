using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    public interface IAzurestorageRepository : IRepository<Azurestorage, Guid>
    {
        Task<Azurestorage?> FindByNameAsync(string containerName);
        Task<List<Azurestorage>> GetListAsync(int skipCount, int maxResultCount, string sorting, string filter = null);
    }
}
