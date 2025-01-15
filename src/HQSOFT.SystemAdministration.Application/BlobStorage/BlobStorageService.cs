using HQSOFT.SystemAdministration.BlobStorages;
using HQSOFT.SystemAdministration.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.Aws;
using Volo.Abp.BlobStoring.Azure;
using Volo.Abp.BlobStoring.Database;
using Volo.Abp.BlobStoring.FileSystem;

namespace HQSOFT.SystemAdministration.BlobStorage
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IContainerAppService _containerAppService;

        public BlobStorageService(IServiceProvider serviceProvider, IContainerAppService containerAppService)
        {
            _serviceProvider = serviceProvider;
            _containerAppService = containerAppService;
        }

        public async Task UpdateBlobStorage()
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "BlobStorage", "Default");
            var options = _serviceProvider.GetRequiredService<IOptions<AbpBlobStoringOptions>>().Value;
            var containers = await _containerAppService.GetListContainerAsync();
            foreach (var container in containers)
            {
                options.Containers.Configure(container.Name, containerConfig =>
                {
                    switch (container.TypeStorage)
                    {
                        case "FileSystem":
                            containerConfig.UseFileSystem(x =>
                            {
                                x.BasePath = container.BasePath ?? folderPath;
                            });
                            break;
                        case "Database":
                            containerConfig.UseDatabase();
                            break;
                        case "AWS":
                            containerConfig.UseAws(x =>
                            {
                                x.Name = container.BucketName;
                                x.AccessKeyId = container.AccessKeyId;
                                x.SecretAccessKey = container.SecretAccessKey;
                                x.Region = container.Region;
                            });
                            break;
                        case "Azure":
                            containerConfig.UseAzure(x =>
                            {
                                x.CreateContainerIfNotExists = true;
                                x.ConnectionString = container.AzureConnectionString;
                                x.ContainerName = container.AzureContainerName;
                            });
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                });
            }
        }
    }
}
