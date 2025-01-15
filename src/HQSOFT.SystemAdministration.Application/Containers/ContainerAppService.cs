
using HQSOFT.SystemAdministration.BlobStorage;
using HQSOFT.SystemAdministration.BlobStorages;
using HQSOFT.SystemAdministration.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace HQSOFT.SystemAdministration.Containers
{
    public class ContainerAppService : SystemAdministrationAppService, IContainerAppService
    {
        private readonly IContainerRepository _containerRepository;
        private readonly ContainerManager _containerManager;
        public ContainerAppService(IContainerRepository containerRepository, ContainerManager containerManager)
        {
            _containerRepository = containerRepository;
            _containerManager = containerManager;
        }
        public async Task<ContainerDto> GetAsync(Guid id)
        {
            var container = await _containerRepository.GetAsync(id);
            return ObjectMapper.Map<Container, ContainerDto>(container);
        }
        public async Task<PagedResultDto<ContainerDto>> GetListAsync(GetContainerListDto input)
        {
            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = nameof(Container.Name);
            }

            var containers = await _containerRepository.GetListAsync(
                input.SkipCount,
                input.MaxResultCount,
                input.Sorting,
                input.Filter
            );

            var totalCount = input.Filter == null
                ? await _containerRepository.CountAsync()
                : await _containerRepository.CountAsync(
                    container => container.Name.Contains(input.Filter));

            return new PagedResultDto<ContainerDto>(
                totalCount,
                ObjectMapper.Map<List<Container>, List<ContainerDto>>(containers)
            );
        }
        [Authorize(SystemAdministrationPermissions.Containers.Create)]
        public async Task<ContainerDto> CreateAsync(CreateContainerDto input)
        {
            var container = await _containerManager.CreateAsync(
                input.Name,
                input.TypeStorage,
                input.BasePath,
                input.BucketName,
                input.AccessKeyId,
                input.SecretAccessKey,
                input.Region,
                input.AzureConnectionString,
                input.AzureContainerName
            );

            await _containerRepository.InsertAsync(container);
            return ObjectMapper.Map<Container, ContainerDto>(container);
        }
        [Authorize(SystemAdministrationPermissions.Containers.Edit)]
        public async Task UpdateAsync(Guid id, UpdateContainerDto input)
        {
            var container = await _containerRepository.GetAsync(id);

            if (container.Name != input.Name)
            {
                await _containerManager.ChangeNameAsync(container, input.Name);
            }
            container.TypeStorage = input.TypeStorage;

            container.BasePath = input.BasePath;

            container.BucketName = input.BucketName;

            container.AccessKeyId = input.AccessKeyId;

            container.SecretAccessKey = input.SecretAccessKey;

            container.Region = input.Region;
            container.AzureConnectionString = input.AzureConnectionString;
            container.AzureContainerName = input.AzureContainerName;

            await _containerRepository.UpdateAsync(container);

        }
        [Authorize(SystemAdministrationPermissions.Containers.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _containerRepository.DeleteAsync(id);
        }

        public async Task<List<ContainerDto>> GetListContainerAsync()
        {
            var containers = await _containerRepository.GetListContainerAsync();
            return ObjectMapper.Map<List<Container>, List<ContainerDto>>(containers);
        }

        public async Task<ContainerDto> GetContainerByPath(string path)
        {
            var extension = path[(path.LastIndexOf('.') + 1)..].ToLower();
            var containers = await _containerRepository.GetContainerByPath(extension);
            return ObjectMapper.Map<Container, ContainerDto>(containers);
        }
    }
}
