namespace HQSOFT.SystemAdministration;

public static class SystemAdministrationDbProperties
{
    public static string DbTablePrefix { get; set; } = "SystemAdministration";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SystemAdministration";
}
