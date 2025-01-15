//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Volo.Abp.BlobStoring.FileSystem;
//using Volo.Abp.BlobStoring;
//using Volo.Abp.BlobStoring.Aws;
//using Volo.Abp.BlobStoring.Database;
//using Volo.Abp.BlobStoring.Azure;

//namespace HQSOFT.SystemAdministration.BlobStorage
//{
//    public static class BlobContainerConfigurationExtensions
//    {
//        public static BlobContainerConfiguration UseDynamicFileSystem(
//       this BlobContainerConfiguration containerConfiguration,
//       string basePath)
//        {
//            containerConfiguration.ProviderType = typeof(FileSystemBlobProvider);
//            containerConfiguration.UseFileSystem(x => {
//                 x.BasePath = basePath;
//                });
//            containerConfiguration.IsMultiTenant = true;
//            return containerConfiguration;
//        }
//        public static BlobContainerConfiguration UseDynamicAWS(
//            this BlobContainerConfiguration containerConfiguration,
//            string bucketName,
//            string accessKeyId,
//            string secretAccessKey,
//            string region)
//        {
//            containerConfiguration.ProviderType = typeof(AwsBlobProvider);
//            containerConfiguration.UseAws( x =>
//            {
//                x.ContainerName = bucketName;
//                x.AccessKeyId = accessKeyId;
//                x.SecretAccessKey = secretAccessKey;
//                x.Region = region;
//                x.CreateContainerIfNotExists = true;
//            });
//            return containerConfiguration;
//        }
//        public static BlobContainerConfiguration UseDynamicDatabase(
//    this BlobContainerConfiguration containerConfiguration)
//        {
//            containerConfiguration.ProviderType = typeof(DatabaseBlobProvider);
//            return containerConfiguration;
//        }
//        public static BlobContainerConfiguration UseDynamicAzure(
//        this BlobContainerConfiguration containerConfiguration,
//        string containerName,
//        string connectionString)
//        {
//            containerConfiguration.ProviderType = typeof(AzureBlobProvider);
//            containerConfiguration.SetConfiguration(containerName, connectionString);
//            containerConfiguration.UseAzure(x =>
//            {
//                x.ConnectionString = connectionString;
//                x.ContainerName = containerName;
//                x.CreateContainerIfNotExists = true;
//            });
//            return containerConfiguration;
//        }
//    }
//}
