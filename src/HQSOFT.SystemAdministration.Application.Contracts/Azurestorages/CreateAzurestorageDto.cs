using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    public class CreateAzurestorageDto
    {
        [Required]
        public string ContainerName { get; set; } = string.Empty;
        [Required]
        public string ConnectionString { get; set; } = string.Empty;
        [Required]
        public bool CreateContainerIfNotExists { get; set; }
        [Required]
        public Guid ContainerId { get; set; }
    }
}
