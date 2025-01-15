using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HQSOFT.SystemAdministration.Containers
{
    public interface IContainerAppService : IApplicationService
    {
        Task<ContainerDto> GetAsync(Guid id);
        Task<PagedResultDto<ContainerDto>> GetListAsync(GetContainerListDto input);
        Task<ContainerDto> CreateAsync(CreateContainerDto input);
        Task UpdateAsync(Guid id, UpdateContainerDto input);
        Task DeleteAsync(Guid id);
        Task<List<ContainerDto>> GetListContainerAsync();
        Task<ContainerDto> GetContainerByPath(string path);
    }
}
