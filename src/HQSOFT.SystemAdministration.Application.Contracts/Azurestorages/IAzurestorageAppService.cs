using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    public interface IAzurestorageAppService : IApplicationService
    {
        Task<AzurestorageDto> GetAsync(Guid id);

        Task<PagedResultDto<AzurestorageDto>> GetListAsync(GetAzurestorageListDto input);

        Task<AzurestorageDto> CreateAsync(CreateAzurestorageDto input);

        Task UpdateAsync(Guid id, UpdateAzurestorageDto input);

        Task DeleteAsync(Guid id);

        Task<ListResultDto<ContainerLookupDto>> GetContainerLookupAsync();
    }
}
