using Volo.Abp.Modularity;

namespace HQSOFT.SystemAdministration;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class SystemAdministrationApplicationTestBase<TStartupModule> : SystemAdministrationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
