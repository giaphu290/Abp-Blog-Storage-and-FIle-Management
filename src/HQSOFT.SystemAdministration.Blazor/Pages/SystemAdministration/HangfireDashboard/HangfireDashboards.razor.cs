using Blazorise;
using HQSOFT.Common.Blazor.Pages.Component;
using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using HQSOFT.CoreBackend.Permissions;
using HQSOFT.CoreBackend.SalesRouteMasters;
using HQSOFT.CoreBackend.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Components.BlockUi;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using static HQSOFT.CoreBackend.Permissions.CoreBackendPermissions;

namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.HangfireDashboard
{
    public partial class HangfireDashboards
    {
        //Standard code: Do not change  

        protected List<Volo.Abp.BlazoriseUI.BreadcrumbItem> BreadcrumbItems = new List<Volo.Abp.BlazoriseUI.BreadcrumbItem>();
        protected PageToolbar Toolbar { get; } = new PageToolbar();

        private bool CanCreate { get; set; }
        private bool CanEdit { get; set; }
        private bool CanDelete { get; set; }


        //Custom code: add more code based on actual requirement 
        string HangfireUrl { get; set; } = string.Empty;

        private HQSOFTBreadcrumbScreen BreadcrumbScreen { get; set; } = new HQSOFTBreadcrumbScreen();
        private WorkspaceDto WorkspaceMenu { get; set; } = new WorkspaceDto(); 

        private readonly object lockObject = new object();


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *										Initialize Section
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region 
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

                await JSRuntime.InvokeVoidAsync("FullScreen");
                await JSRuntime.InvokeVoidAsync("AssignGotFocus"); 
                await JSRuntime.InvokeVoidAsync("registerUtilsBlazorMethod", DotNetObjectReference.Create(this)); 
                await JSRuntime.InvokeVoidAsync("onIframeLoad");

                await BreadcrumbScreen.GetBreadcrumbsAsync();

                await BlockUiService.UnBlock();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        protected override void Dispose(bool disposing)
        {
            JSRuntime.InvokeVoidAsync("UnFullScreen");
            base.Dispose(disposing);
        }


        protected override async Task OnInitializedAsync()
        {
            await SetPermissionsAsync();
            await SetToolbarItemsAsync();
            
            await GetHangfireDashboardAsync();
            await InvokeAsync(StateHasChanged);
        }

        #endregion



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								ToolBar - Breadcrumb - Permission
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region  
        private async Task SetPermissionsAsync()
        {
            CanCreate = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.HangfireConfigs.Create);
            CanEdit = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.HangfireConfigs.Edit);
            CanDelete = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.HangfireConfigs.Delete);
        }
         
        protected virtual ValueTask SetToolbarItemsAsync()
        {
            Toolbar.AddButton(L["Back"], async () =>
            {
                NavigationManager.NavigateTo($"/");
            },
            IconName.Undo,
            Color.Light);

            return ValueTask.CompletedTask;
        }

        private async Task ResetToolbarAsync()
        {
            lock (lockObject)
            {
                Toolbar.Contributors.Clear();
            }

            await Task.Run(async () =>
            {
                await SetToolbarItemsAsync();
            });

            await InvokeAsync(StateHasChanged);
        }

        #endregion


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *									  Controls triggers/events
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        private async Task GetHangfireDashboardAsync()
        {
            var apiUrl = Configuration.GetValue<string>("RemoteServices:Default:BaseUrl");
            if (apiUrl != null)
            {
                if (!apiUrl.EndsWith("/"))
                {
                    apiUrl += "/";
                }

                HangfireUrl = apiUrl + "hangfire/dashboard";
            }

            await Task.CompletedTask;
        }

        [JSInvokable]
        public async Task ResetToolbarItemsAsync()
        {
            await ResetToolbarAsync();
            await InvokeAsync(StateHasChanged);
        }

    }
}
