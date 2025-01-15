using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    public class Azurestorage : AuditedAggregateRoot<Guid>
    {
        public string ContainerName { get; set; } = string.Empty;

        public string ConnectionString { get; set; } = string.Empty;

        public bool CreateContainerIfNotExists { get; set; }

        public Guid ContainerId { get; set; }
        private Azurestorage() { }
        internal Azurestorage(
            Guid id,
            string connectionString,
            string containerName,
            bool createContainerIfNotExists,   
            Guid containerId
            ) : base(id)
        {
            SetName(connectionString);
            ConnectionString = connectionString;
            CreateContainerIfNotExists = createContainerIfNotExists;
            ContainerId = containerId;
        }
        internal Azurestorage ChangeName(string containerName)
        {
            SetName(containerName);
            return this;
        }
        private void SetName(string containerName)
        {
            ContainerName = Check.NotNullOrWhiteSpace(containerName, nameof(containerName));
        }
    }
  
}
