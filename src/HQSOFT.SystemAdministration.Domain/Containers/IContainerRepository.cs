using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace HQSOFT.SystemAdministration.Containers
{
    public interface IContainerRepository : IRepository<Container, Guid>
    {
        Task<Container?> FindByNameAsync(string name);

        Task<List<Container>> GetListAsync(
            int skipCount,
            int maxResultCount,
            string sorting,
            string? filter = null
        );
        Task<List<Container>> GetListContainerAsync();
        Task<Container?> GetContainerByPath(string path);
    }
}
