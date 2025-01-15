using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace HQSOFT.SystemAdministration.EntityFrameworkCore;

public class SystemAdministrationHttpApiHostMigrationsDbContext : AbpDbContext<SystemAdministrationHttpApiHostMigrationsDbContext>
{
    public SystemAdministrationHttpApiHostMigrationsDbContext(DbContextOptions<SystemAdministrationHttpApiHostMigrationsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureSystemAdministration();
        modelBuilder.ConfigureAuditLogging();
        modelBuilder.ConfigurePermissionManagement();
        modelBuilder.ConfigureSettingManagement();
    }
}
