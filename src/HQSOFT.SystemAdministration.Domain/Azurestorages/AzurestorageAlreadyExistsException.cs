using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace HQSOFT.SystemAdministration.Azurestorages
{
    public class AzurestorageAlreadyExistsException : BusinessException
    {
        public AzurestorageAlreadyExistsException(string containerName)
        : base(SystemAdministrationErrorCodes.AzurestorageAlreadyExists)
        {
            WithData("containerName", containerName);
        }
    }
}
