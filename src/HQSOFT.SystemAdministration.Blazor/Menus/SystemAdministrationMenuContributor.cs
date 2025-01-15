using System.Threading.Tasks;
using HQSOFT.CoreBackend.Localization;
using HQSOFT.CoreBackend.Permissions;
using System.Collections.Generic;
using Volo.Abp.UI.Navigation;
using HQSOFT.SystemAdministration.Localization;
using HQSOFT.SystemAdministration.Permissions;

namespace HQSOFT.SystemAdministration.Blazor.Menus;

public class SystemAdministrationMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
#if DEBUG
        var moduleMenu = AddModuleMenuItem(context);

        AddMenuItemModules(context, moduleMenu);
        AddMenuItemScreens(context, moduleMenu);
        AddMenuItemReports(context, moduleMenu);
        AddMenuItemCompanies(context, moduleMenu);
        AddMenuItemExtendedUsers(context, moduleMenu);
        AddMenuItemWorkspaces(context, moduleMenu);
        AddMenuItemAccessRights(context, moduleMenu);
        AddMenuItemSystemSettings(context, moduleMenu);
        AddMenuItemSystemConfigurations(context, moduleMenu);
        AddMenuItemHangfireConfigs(context, moduleMenu);
        AddMenuItemHangfireDashboards(context, moduleMenu);
        AddMenuItemElsaWorkflow(context, moduleMenu);
		AddMenuItemBlobStorage(context, moduleMenu);
#endif

		await Task.CompletedTask;
    }

    private static ApplicationMenuItem AddModuleMenuItem(MenuConfigurationContext context)
	{
		var moduleMenu = new ApplicationMenuItem(
			SystemAdministrationMenus.Prefix,
			context.GetLocalizer<CoreBackendResource>()["Menu:SystemAdministration"],
			icon: "fa fa-folder"
		);

		context.Menu.Items.AddIfNotContains(moduleMenu);
		return moduleMenu;
	}

	private static void AddMenuItemModules(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.Modules,
				context.GetLocalizer<CoreBackendResource>()["Menu:Modules"],
				"/SystemAdministration/Modules",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.Modules.Default
			)
		);
	}

	private static void AddMenuItemExtendedUsers(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.ExtendedUsers,
				context.GetLocalizer<CoreBackendResource>()["Menu:ExtendedUsers"],
				"/SystemAdministration/Users",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.ExtendedUsers.Default
			)
		);
	}

	private static void AddMenuItemScreens(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.Screens,
				context.GetLocalizer<CoreBackendResource>()["Menu:Screens"],
				"/SystemAdministration/Screens",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.Screens.Default
			)
		);
	}

	private static void AddMenuItemReports(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.Reports,
				context.GetLocalizer<CoreBackendResource>()["Menu:Reports"],
				"/SystemAdministration/Reports",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.Reports.Default
			)
		);
	}

	private static void AddMenuItemCompanies(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.Companies,
				context.GetLocalizer<CoreBackendResource>()["Menu:Companies"],
				"/SystemAdministration/Companies",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.Companies.Default
			)
		);
	}

	private static void AddMenuItemWorkspaces(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.Workspaces,
				context.GetLocalizer<CoreBackendResource>()["Menu:Workspaces"],
				"/SystemAdministration/Workspaces",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.Workspaces.Default
			)
		);
	}

	private static void AddMenuItemAccessRights(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.AccessRights,
				context.GetLocalizer<CoreBackendResource>()["Menu:AccessRights"],
				"/SystemAdministration/AccessRights",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.Companies.Default
			)
		);
	}

	private static void AddMenuItemSystemSettings(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.SystemSettings,
				context.GetLocalizer<CoreBackendResource>()["Menu:SystemSettings"],
				"/SystemAdministration/SystemSettings",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.SystemSettings.Default
			)
		);
	}

	private static void AddMenuItemSystemConfigurations(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.SystemConfigurations,
				context.GetLocalizer<CoreBackendResource>()["Menu:SystemConfigurations"],
				"/SystemAdministration/SystemConfigurations",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.SystemConfigurations.Default
			)
		);
	}

	private static void AddMenuItemHangfireConfigs(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.HangfireConfigs,
				context.GetLocalizer<CoreBackendResource>()["Menu:HangfireConfigs"],
				"/SystemAdministration/HangfireConfigs",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.HangfireConfigs.Default
			)
		);
	}

	private static void AddMenuItemHangfireDashboards(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.HangfireDashboards,
				context.GetLocalizer<CoreBackendResource>()["Menu:HangfireDashboards"],
				"/SystemAdministration/HangfireDashboards",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.HangfireConfigs.Default
			)
		);
	}

	private static void AddMenuItemElsaWorkflow(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		parentMenu.AddItem(
			new ApplicationMenuItem(
				Menus.SystemAdministrationMenus.ElsaDashboard,
				context.GetLocalizer<CoreBackendResource>()["Menu:ElsaWorkflowsDashboard"],
				"/workflow-definitions",
				icon: "fa fa-file-alt",
				requiredPermissionName: CoreBackendPermissions.ElsaDashboard.Default
			)
		);
	}
	private static void AddMenuItemBlobStorage(MenuConfigurationContext context, ApplicationMenuItem parentMenu)
	{
		var blobStorageMenu = parentMenu.AddItem(
			new ApplicationMenuItem(
			  Menus.SystemAdministrationMenus.BlobStorageSetting,
			  context.GetLocalizer<SystemAdministrationResource>()["Menu:BlobStorageSetting"],
			  "/containers",
			  icon: "fa fa-file-alt",
			  requiredPermissionName: SystemAdministrationPermissions.Containers.Default
		  ));
    }
}
