using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace HQSOFT.SystemAdministration.Containers
{
    public class ContainerManager : DomainService
    {
        private readonly IContainerRepository _containerRepository;
        public ContainerManager(IContainerRepository containerRepository)
        {
            _containerRepository = containerRepository;
        }
        public async Task<Container> CreateAsync(
            string name,
            string? typeStorage = null,
            string? basePath = null,
            string? bucketName = null,
            string? accessKeyId = null,
            string? secretAccessKey = null,
            string? region = null,
            string? azureConnectionString = null,
            string? azureContainerName = null)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            var existingContainer = await _containerRepository.FindByNameAsync(name);
            if (existingContainer != null)
            {
                throw new ContainerAlreadyExistsException(name);
            }
            return new Container(
                GuidGenerator.Create(),
                name,
                typeStorage,
                basePath,
                bucketName,
                accessKeyId,
                secretAccessKey,
                region,
                azureConnectionString,
                azureContainerName
                );
        }
        public async Task ChangeNameAsync(
                Container container,
                string newName)
        {
            Check.NotNull(container, nameof(container));
            Check.NotNullOrWhiteSpace(newName, nameof(newName));

            var existingContainer= await _containerRepository.FindByNameAsync(newName);
            if (existingContainer != null && existingContainer.Id != existingContainer.Id)
            {
                throw new ContainerAlreadyExistsException(newName);
            }

            container.ChangeName(newName);
        }

    }
}
