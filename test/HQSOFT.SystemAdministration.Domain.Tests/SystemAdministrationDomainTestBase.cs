using Volo.Abp.Modularity;

namespace HQSOFT.SystemAdministration;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class SystemAdministrationDomainTestBase<TStartupModule> : SystemAdministrationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
