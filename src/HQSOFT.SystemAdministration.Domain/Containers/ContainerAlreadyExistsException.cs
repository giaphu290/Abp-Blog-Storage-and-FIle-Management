using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace HQSOFT.SystemAdministration.Containers
{
    public class ContainerAlreadyExistsException : BusinessException
    {
        public ContainerAlreadyExistsException(string name)
      : base(SystemAdministrationErrorCodes.ContainerAlreadyExists)
        {
            WithData("name", name);
        }
    }
}
