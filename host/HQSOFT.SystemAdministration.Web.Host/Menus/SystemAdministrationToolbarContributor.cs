using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using HQSOFT.SystemAdministration.Components.Toolbar.Impersonation;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.Users;

namespace HQSOFT.SystemAdministration.Menus;

public class SystemAdministrationToolbarContributor : IToolbarContributor
{
    public virtual Task ConfigureToolbarAsync(IToolbarConfigurationContext context)
    {
        if (context.Toolbar.Name != StandardToolbars.Main)
        {
            return Task.CompletedTask;
        }

        var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();
        
        if (currentUser.FindImpersonatorUserId() != null)
        {
            context.Toolbar.Items.Add(new ToolbarItem(typeof(ImpersonationViewComponent), order: -1));
        }

        return Task.CompletedTask;
    }
}