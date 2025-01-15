using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using HQSOFT.SystemAdministration.Localization;
using Volo.Abp.Authorization.Permissions;

namespace HQSOFT.SystemAdministration.Web.Menus;

public class SystemAdministrationMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var moduleMenu = AddModuleMenuItem(context); //Do not delete `moduleMenu` variable as it will be used by ABP Suite!
    }

    private static ApplicationMenuItem AddModuleMenuItem(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SystemAdministrationResource>();
        
        var moduleMenu = new ApplicationMenuItem(
            SystemAdministrationMenus.Prefix,
            displayName: l["Menu:SystemAdministration"],
            "~/SystemAdministration",
            icon: "fa fa-globe");

        //Add main menu items.
        context.Menu.Items.AddIfNotContains(moduleMenu);
        return moduleMenu;
    }
}