using Volo.Abp;
using Volo.Abp.MongoDB;

namespace HQSOFT.SystemAdministration.MongoDB;

public static class SystemAdministrationMongoDbContextExtensions
{
    public static void ConfigureSystemAdministration(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
