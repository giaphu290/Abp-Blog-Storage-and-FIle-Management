using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using HQSOFT.SystemAdministration.Localization;
using Volo.Abp.Domain;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;
using HQSOFT.CoreBackend;
using Volo.FileManagement;
//using HQSOFT.Common;
using Volo.Abp.BlobStoring.Database;

namespace HQSOFT.SystemAdministration;

[DependsOn(
    typeof(AbpValidationModule),
    typeof(AbpDddDomainSharedModule)
)]
[DependsOn(typeof(CoreBackendDomainSharedModule))]
//[DependsOn(typeof(CommonDomainSharedModule))]

[DependsOn(typeof(BlobStoringDatabaseDomainSharedModule))]
    public class SystemAdministrationDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SystemAdministrationDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SystemAdministrationResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/SystemAdministration");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SystemAdministration", typeof(SystemAdministrationResource));
        });
    }
}
