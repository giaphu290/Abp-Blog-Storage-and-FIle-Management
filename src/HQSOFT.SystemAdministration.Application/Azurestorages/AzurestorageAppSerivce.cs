using HQSOFT.SystemAdministration.Containers;
using System;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using System.Collections.Generic;
using HQSOFT.SystemAdministration.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    [Authorize(SystemAdministrationPermissions.Azurestorages.Default)]
    public class AzurestorageAppSerivce : SystemAdministrationAppService, IAzurestorageAppService
    {
        private readonly IAzurestorageRepository _azurestorageRepository;
        private readonly AzurestorageManager _azurestorageManager;
        private readonly IContainerRepository _containerRepository;
        public AzurestorageAppSerivce(IAzurestorageRepository azurestorageRepository, AzurestorageManager azurestorageManager, IContainerRepository containerRepository)
        {
            _azurestorageRepository = azurestorageRepository;
            _azurestorageManager = azurestorageManager;
            _containerRepository = containerRepository;
        }
        [Authorize(SystemAdministrationPermissions.Azurestorages.Create)]
        public async Task<AzurestorageDto> CreateAsync(CreateAzurestorageDto input)
        {
            var azureStorage = await _azurestorageManager.CreateAsync(
                 input.ContainerName,
                 input.ConnectionString,
                 input.CreateContainerIfNotExists,
                 input.ContainerId
             );
            await _azurestorageRepository.InsertAsync(azureStorage);
            return ObjectMapper.Map<Azurestorage, AzurestorageDto>(azureStorage);
        }

        [Authorize(SystemAdministrationPermissions.Azurestorages.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _azurestorageRepository.DeleteAsync(id);
        }

        public async Task<AzurestorageDto> GetAsync(Guid id)
        {
            var azureStorage = await _azurestorageRepository.GetQueryableAsync();
            var query = from azure in azureStorage
                        join container in await _containerRepository.GetQueryableAsync() on azure.ContainerId equals container.Id
                        where azure.Id == id
                        select new {azure, container};
            var queryResult = await AsyncExecuter.FirstOrDefaultAsync(query);
            if (queryResult == null)
            {
                throw new EntityNotFoundException(typeof(Azurestorage), id);
            }
            var azureStorageDto = ObjectMapper.Map<Azurestorage, AzurestorageDto>(queryResult.azure);
            azureStorageDto.Name = queryResult.container.Name;
            return azureStorageDto;

        }

        public async Task<ListResultDto<ContainerLookupDto>> GetContainerLookupAsync()
        {
            var containers = await _containerRepository.GetQueryableAsync();
            var azureStorages = await _azurestorageRepository.GetQueryableAsync();
            var query = from container in containers
                        where !azureStorages.Select(x => x.ContainerId).Contains(container.Id) &&
                        container.TypeStorage.ToUpper() == "AZURE"
                        select container;
            var unlinkedContainers = await AsyncExecuter.ToListAsync(query);
            return new ListResultDto<ContainerLookupDto>(
                ObjectMapper.Map<List<Container>, List<ContainerLookupDto>>(unlinkedContainers)
            );
        }

        public async Task<PagedResultDto<AzurestorageDto>> GetListAsync(GetAzurestorageListDto input)
        {
            var queryable = await _azurestorageRepository.GetQueryableAsync();
            var query = from azure in queryable
                        join container in await _containerRepository.GetQueryableAsync() on azure.ContainerId equals container.Id
                        select new {azure, container};
          
            var queryResult = await AsyncExecuter.ToListAsync(query);
            //Convert the query result to a list of AzurestorageDto objects

            var azureDtos = queryResult.Select(x =>
            {
                var azureDto = ObjectMapper.Map<Azurestorage, AzurestorageDto>(x.azure);
                azureDto.Name = x.container.Name;
                return azureDto;
            }).ToList();

            //Get the total count with another query
            var totalCount = input.Filter == null
             ? await _azurestorageRepository.CountAsync()
             : await _azurestorageRepository.CountAsync(
                 azurestorage => azurestorage.ContainerName.Contains(input.Filter));

            return new PagedResultDto<AzurestorageDto>(
                totalCount,
                azureDtos
            );
        }

        [Authorize(SystemAdministrationPermissions.Azurestorages.Edit)]
        public async Task UpdateAsync(Guid id, UpdateAzurestorageDto input)
        {
            var azureStorage = await _azurestorageRepository.GetAsync(id);

            if (azureStorage.ContainerName != azureStorage.ContainerName)
            {
                await _azurestorageManager.CheckNameAsync(azureStorage, input.ContainerName);
            }

            azureStorage.ConnectionString = input.ConnectionString;
            azureStorage.CreateContainerIfNotExists = input.CreateContainerIfNotExists;
            azureStorage.ContainerId = input.ContainerId;

            await _azurestorageRepository.UpdateAsync(azureStorage);
        }
        private static string NormalizeSorting(string sorting)
        {
            if (sorting.IsNullOrEmpty())
            {
                return $"Azurestorage.ContainerName";
            }

            if (sorting.Contains("containerName", StringComparison.OrdinalIgnoreCase))
            {
                return sorting.Replace(
                    "containerName",
                    "container.Name",
                    StringComparison.OrdinalIgnoreCase
                );
            }
            return $"Azurestorage.{sorting}";
        }
    }
}
