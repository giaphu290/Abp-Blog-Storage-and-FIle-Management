using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    public class ContainerLookupDto : EntityDto<Guid>
    {
        public string Name { get; set; }
    }
}
