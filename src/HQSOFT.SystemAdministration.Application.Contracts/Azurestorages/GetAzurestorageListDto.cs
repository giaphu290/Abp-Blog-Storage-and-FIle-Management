using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    public class GetAzurestorageListDto : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}
