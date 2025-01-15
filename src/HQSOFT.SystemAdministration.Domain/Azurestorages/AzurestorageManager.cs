using Elsa.Activities.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    public class AzurestorageManager : DomainService
    {
        private readonly IAzurestorageRepository _repository;
        public AzurestorageManager(IAzurestorageRepository repository)
        {
            _repository = repository;
        }
        public async Task<Azurestorage> CreateAsync(string containerName, string connectionString, bool CreateContainerIfNotExists, Guid containerId)
        {
            var existingAzurestorage = await _repository.FindByNameAsync(containerName);
            Check.NotNullOrWhiteSpace(containerName, nameof(containerName));
            if (existingAzurestorage != null)
            {
                throw new AzurestorageAlreadyExistsException(containerName);
            }
            return new Azurestorage(
                GuidGenerator.Create(),
                containerName,
                connectionString,
                CreateContainerIfNotExists,
                containerId
                );
        }
        public async Task CheckNameAsync(Azurestorage azurestorage, string containerName)
        {
            Check.NotNull(azurestorage, nameof(azurestorage));
            Check.NotNullOrWhiteSpace(containerName, nameof(containerName));
            var existingAzurestorage = await _repository.FindByNameAsync(containerName);
            if (existingAzurestorage != null && existingAzurestorage.Id != azurestorage.Id)
            {
                throw new AzurestorageAlreadyExistsException(containerName);
            }
            azurestorage.ChangeName(containerName);
        }
    }
}
