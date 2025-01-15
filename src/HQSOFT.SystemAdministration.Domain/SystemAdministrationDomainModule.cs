using Volo.Abp.Caching;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using HQSOFT.CoreBackend;
//using HQSOFT.Common;
using Volo.Abp.BlobStoring.Azure;
using Volo.Abp.BlobStoring.Aws;

using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using System.Management;
using Volo.FileManagement;
using Volo.Abp.BlobStoring.Database;

namespace HQSOFT.SystemAdministration;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpCachingModule),
    typeof(SystemAdministrationDomainSharedModule)
)]
[DependsOn(typeof(CoreBackendDomainModule))]
[DependsOn(typeof(AbpBlobStoringAzureModule))]
[DependsOn(typeof(AbpBlobStoringAwsModule))]
[DependsOn(typeof(AbpBlobStoringFileSystemModule))]
[DependsOn(typeof(BlobStoringDatabaseDomainModule))]
    public class SystemAdministrationDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //Configure<AbpBlobStoringOptions>(options =>
        //{
        //    //options.Containers.Configure<AWSContainer>(container =>
        //    //{
        //    //    container.UseAws(Aws =>
        //    //    {
        //    //        Aws.AccessKeyId = "AKIAW5BDQ63BP3NFCUUN";
        //    //        Aws.SecretAccessKey = "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";
        //    //        Aws.Region = "ap-southeast-2";
        //    //        Aws.ContainerName = "my-first-bucket-hqsoft";
        //    //        Aws.CreateContainerIfNotExists = false;
        //    //        Aws.UseTemporaryCredentials = false;
        //    //    });
        //    //});
        //    //options.Containers.Configure("",container =>
        //    //{
        //    //    container.(fileSystem =>
        //    //    {
        //    //        fileSystem.BasePath = "C:\\my-files";
        //    //    });
        //    //});
        //    //options.Containers.Configure("", container =>
        //    //{
        //    //    container.UseDatabase();
        //    //});

        //});
    }
}
