
using HQSOFT.SystemAdministration.Azurestorages;
using HQSOFT.SystemAdministration.Containers;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.FileManagement.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;

namespace HQSOFT.SystemAdministration.EntityFrameworkCore;

[ConnectionStringName(SystemAdministrationDbProperties.ConnectionStringName)]
public class SystemAdministrationDbContext : AbpDbContext<SystemAdministrationDbContext>, ISystemAdministrationDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */
    public DbSet<Container> Containers { get; set; }
    public DbSet<Azurestorage> Azurestorages { get; set; }
    public SystemAdministrationDbContext(DbContextOptions<SystemAdministrationDbContext> options)
        : base(options)
    {}
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Container>(b =>
        {
            b.ToTable(SystemAdministrationDbProperties.DbTablePrefix + "Containers",
                SystemAdministrationDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(ContainerConsts.MaxNameLength);
            b.HasIndex(x => x.Name);
        });
        builder.Entity<Azurestorage>(b =>
        {
            b.ToTable(SystemAdministrationDbProperties.DbTablePrefix + "Azurestorages",
                SystemAdministrationDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.ContainerName)
                .IsRequired();
            b.HasIndex(x => x.ContainerName);
            b.HasOne<Container>().WithOne().HasForeignKey<Azurestorage>(x => x.ContainerId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(builder);
        builder.ConfigureSystemAdministration();
        builder.ConfigureBlobStoring();
        } 

}
