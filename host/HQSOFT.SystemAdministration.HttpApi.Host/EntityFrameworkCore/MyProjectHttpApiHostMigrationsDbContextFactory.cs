using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HQSOFT.SystemAdministration.EntityFrameworkCore;

public class SystemAdministrationHttpApiHostMigrationsDbContextFactory : IDesignTimeDbContextFactory<SystemAdministrationHttpApiHostMigrationsDbContext>
{
    public SystemAdministrationHttpApiHostMigrationsDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<SystemAdministrationHttpApiHostMigrationsDbContext>()
            .UseNpgsql(configuration.GetConnectionString("SystemAdministration"));

        return new SystemAdministrationHttpApiHostMigrationsDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
