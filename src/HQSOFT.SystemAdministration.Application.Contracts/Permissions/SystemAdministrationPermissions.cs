using Volo.Abp.Reflection;

namespace HQSOFT.SystemAdministration.Permissions;

public class SystemAdministrationPermissions
{
    public const string GroupName = "SystemAdministration";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(SystemAdministrationPermissions));
    }
    public static class Containers
    {
        public const string Default = GroupName + ".Containers";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public static class AWSstorages
    {
        public const string Default = GroupName + ".AWSstorages";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public static class Azurestorages
    {
        public const string Default = GroupName + ".Azurestorages";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
    public static class Databases
    {
        public const string Default = GroupName + ".Databases";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
    public static class FileSystems
    {
        public const string Default = GroupName + ".FileSystems";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
}
