using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using HQSOFT.CoreBackend;

using Volo.FileManagement;
using Volo.Abp.BlobStoring.Database;
using Volo.Abp.BlobStoring;
using HQSOFT.SystemAdministration.Containers;
using HQSOFT.SystemAdministration.BlobStorage;
using System;
using System.IO;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using DevExpress.XtraPrinting.Native;
using Volo.Abp.BlobStoring.Aws;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel;
using Volo.Abp.BlobStoring.Azure;
using System.Threading.Tasks;
using HQSOFT.SystemAdministration.BlobStorages;
////using HQSOFT.Common;

namespace HQSOFT.SystemAdministration;

[DependsOn(
    typeof(SystemAdministrationDomainModule),
    typeof(SystemAdministrationApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(FileManagementApplicationModule),
    typeof(AbpBlobStoringFileSystemModule)
    )]
[DependsOn (typeof(CoreBackendApplicationModule))]
//[DependsOn (typeof (CommonApplicationModule))]
public class SystemAdministrationApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<SystemAdministrationApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<SystemAdministrationApplicationModule>(validate: true);
        });
        context.Services.AddTransient<IContainerAppService, ContainerAppService>();
        context.Services.AddTransient<IBlobStorageService,BlobStorageService>();

    }

    //public override Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    //{
    //    using (var scope = context.ServiceProvider.CreateScope())
    //    {
    //        // Lấy IContainerAppService
    //        var containerAppService = scope.ServiceProvider.GetRequiredService<IContainerAppService>();
    //        //var wedhost = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    //        // Lấy danh sách container
    //        var containers = containerAppService.GetListContainerAsync().Result;
    //        //var projectDirectory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "host", "HQSOFT.SystemAdministration.HttpApi.Host");
    //        //var directPath = Path.Combine(wedhost.WebRootPath, "uploads");
    //            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "BlobStorage","Default");

    //        // Cấu hình Blob Storing
    //        ConfigureBlobStoring(containers, folderPath, scope.ServiceProvider);

    //    }
    //    return Task.CompletedTask;
    //}

    //private void ConfigureBlobStoring(IEnumerable<ContainerDto> containers, string projectDirectory, IServiceProvider serviceProvider)
    //{
    //    var options = serviceProvider.GetRequiredService<IOptions<AbpBlobStoringOptions>>().Value;

    //    foreach (var container in containers)
    //    {
    //        options.Containers.Configure(container.Name, containerConfig =>
    //        {
    //        switch (container.TypeStorage)
    //        {
    //            case "FileSystem":
    //                if (container.BasePath == null)
    //                {
    //                        containerConfig.UseFileSystem(x =>
    //                        {
    //                            x.BasePath = projectDirectory;
    //                        });
    //                    }
    //                else
    //                {
    //                        containerConfig.UseFileSystem(x => 
    //                        {
    //                            x.BasePath = container.BasePath;
    //                        });
    //                }
    //                break;
    //            case "Database":
    //                    containerConfig.UseDatabase();
    //                break;
    //            case "AWS":
    //                containerConfig.UseAws( x => 
    //                {
    //                    x.Name = container.BucketName;
    //                     x.AccessKeyId = container.AccessKeyId;
    //                      x.SecretAccessKey = container.SecretAccessKey;
    //                       x.Region = container.Region;
    //                });
    //                    break;
    //            case "Azure":
    //                    containerConfig.UseAzure(x =>
    //                    {
    //                        x.CreateContainerIfNotExists = true;
    //                        x.ConnectionString = container.AzureConnectionString;
    //                        x.ContainerName = container.AzureContainerName;
    //                    });
    //                    break;
    //            }
    //        });
    //    }
    //}
}

