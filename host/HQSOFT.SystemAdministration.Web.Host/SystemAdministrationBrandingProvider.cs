using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace HQSOFT.SystemAdministration;

[Dependency(ReplaceServices = true)]
public class SystemAdministrationBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "SystemAdministration";
}
