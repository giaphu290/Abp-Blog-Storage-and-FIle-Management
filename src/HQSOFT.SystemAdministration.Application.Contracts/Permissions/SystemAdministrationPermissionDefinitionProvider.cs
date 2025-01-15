using HQSOFT.SystemAdministration.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace HQSOFT.SystemAdministration.Permissions;

public class SystemAdministrationPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SystemAdministrationPermissions.GroupName, L("Permission:SystemAdministration"));

        var containersPermission = myGroup.AddPermission(SystemAdministrationPermissions.Containers.Default, L("Permission:Containers"));
        containersPermission.AddChild(SystemAdministrationPermissions.Containers.Create, L("Permission:Create"));
        containersPermission.AddChild(SystemAdministrationPermissions.Containers.Edit, L("Permission:Edit"));
        containersPermission.AddChild(SystemAdministrationPermissions.Containers.Delete, L("Permission:Delete"));

        var awstoragesPermission = myGroup.AddPermission(SystemAdministrationPermissions.AWSstorages.Default, L("Permission:AWSstorage"));
        containersPermission.AddChild(SystemAdministrationPermissions.AWSstorages.Create, L("Permission:Create"));
        containersPermission.AddChild(SystemAdministrationPermissions.AWSstorages.Edit, L("Permission:Edit"));
        containersPermission.AddChild(SystemAdministrationPermissions.AWSstorages.Delete, L("Permission:Delete"));

        var azurestoragesPermission = myGroup.AddPermission(SystemAdministrationPermissions.Azurestorages.Default, L("Permission:Azurestorage"));
        containersPermission.AddChild(SystemAdministrationPermissions.Azurestorages.Create, L("Permission:Create"));
        containersPermission.AddChild(SystemAdministrationPermissions.Azurestorages.Edit, L("Permission:Edit"));
        containersPermission.AddChild(SystemAdministrationPermissions.Azurestorages.Delete, L("Permission:Delete"));

        var databasesPermission = myGroup.AddPermission(SystemAdministrationPermissions.Databases.Default, L("Permission:Database"));
        containersPermission.AddChild(SystemAdministrationPermissions.Databases.Create, L("Permission:Create"));
        containersPermission.AddChild(SystemAdministrationPermissions.Databases.Edit, L("Permission:Edit"));
        containersPermission.AddChild(SystemAdministrationPermissions.Databases.Delete, L("Permission:Delete"));

        var fileSystemsPermission = myGroup.AddPermission(SystemAdministrationPermissions.FileSystems.Default, L("Permission:FileSystem"));
        containersPermission.AddChild(SystemAdministrationPermissions.FileSystems.Create, L("Permission:Create"));
        containersPermission.AddChild(SystemAdministrationPermissions.FileSystems.Edit, L("Permission:Edit"));
        containersPermission.AddChild(SystemAdministrationPermissions.FileSystems.Delete, L("Permission:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SystemAdministrationResource>(name);
    }
}
