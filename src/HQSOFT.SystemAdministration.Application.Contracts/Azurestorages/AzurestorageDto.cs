using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    public class AzurestorageDto : EntityDto<Guid>
    {
        public string ContainerName { get; set; } 

        public string ConnectionString { get; set; } 

        public bool CreateContainerIfNotExists { get; set; }

        public Guid ContainerId { get; set; }

        public string Name { get; set; }
    }
}
