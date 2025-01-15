using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HQSOFT.SystemAdministration.Containers
{
    public class CreateContainerDto
    {
        [Required]
        [StringLength(ContainerConsts.MaxNameLength)]
        public string Name { get; set; } = string.Empty;
        public string? TypeStorage { get; set; }
        public string? BasePath { get; set; }
        public string? BucketName { get; set; }
        public string? AccessKeyId { get; set; }
        public string? SecretAccessKey { get; set; }
        public string? Region { get; set; }
        public string? AzureConnectionString { get; set; }
        public string? AzureContainerName { get; set; }
    }
}
