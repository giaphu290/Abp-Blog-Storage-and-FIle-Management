using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace HQSOFT.SystemAdministration.Blazor.Server.Host;

[Dependency(ReplaceServices = true)]
public class SystemAdministrationBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "SystemAdministration";
}
