using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace HQSOFT.SystemAdministration.AWSstorages
{
    public class AWSstorage : AuditedAggregateRoot<Guid>
    {
        public string BucketName { get; set; } = string.Empty;
        public string AccessKeyId { get; set; } = string.Empty;
        public string SecretAccessKey { get; set; } = string.Empty ;
        public string Region { get; set; } = string.Empty;
        public Guid ContainerId { get; set; }
    }
}
