using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace HQSOFT.SystemAdministration.Containers
{
    public class Container : AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? TypeStorage { get; set; }
        public string? BasePath { get; set; }
        public string? BucketName { get; set; }
        public string? AccessKeyId { get; set; }
        public string? SecretAccessKey { get; set; }
        public string? Region { get; set; }
        public string? AzureConnectionString { get; set; }
        public string? AzureContainerName {  get; set; }
        private Container() { }
        internal Container(
            Guid id, 
            string name, 
            string? typeStorage,    
            string? basePath, 
            string? bucketName, 
            string? accessKeyId,
            string? secretAccessKey,
            string? region,
            string? azureConnectionString,
            string? azureContainerName) : base(id)
        {
            SetName(name);
            TypeStorage = typeStorage;
            BasePath = basePath;
            BucketName = bucketName;
            AccessKeyId = accessKeyId;
            SecretAccessKey = secretAccessKey;
            Region = region;
            AzureConnectionString = azureConnectionString;
            AzureContainerName = azureContainerName;
        }
        internal Container ChangeName(string name)
        {
            SetName(name);
            return this;
        }
        private void SetName(string name)
        {
            Name = Check.NotNullOrWhiteSpace(
                name,
                nameof(name),
                maxLength: ContainerConsts.MaxNameLength
           );
        }
    }
}
