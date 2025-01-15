using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace HQSOFT.SystemAdministration.Containers
{
    public class ContainerDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? TypeStorage {  get; set; }
        public string? BasePath { get; set; }
        public string? BucketName { get; set; } 
        public string? AccessKeyId { get; set; } 
        public string? SecretAccessKey { get; set; } 
        public string? Region { get; set; } 
        public string? AzureConnectionString { get; set; }
        public string? AzureContainerName { get; set; }

    }
}
