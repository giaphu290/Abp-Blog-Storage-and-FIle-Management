using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Web;
using Blazorise;
using Blazorise.DataGrid;
using Volo.Abp.BlazoriseUI.Components;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using HQSOFT.CoreBackend.HangfireConfigs;
using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using Microsoft.AspNetCore.Components;
using HQSOFT.CoreBackend.Permissions;
using HQSOFT.CoreBackend.Screens;
using Microsoft.JSInterop;
using HQSOFT.CoreBackend.ExtendedUsers;
using Volo.Abp.Http.Client;
using HQSOFT.CoreBackend.Modules;
using HQSOFT.CoreBackend.Companies;
using HQSOFT.CoreBackend.EnumList;
using HQSOFT.Common.Blazor.Pages.Component;
using HQSOFT.CoreBackend.Reports;
using HQSOFT.CoreBackend.Workspaces;

namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.HangfireConfig
{
    public partial class HangfireConfigsListView
    {
        //Standard code: Do not change  

        protected List<Volo.Abp.BlazoriseUI.BreadcrumbItem> BreadcrumbItems = new List<Volo.Abp.BlazoriseUI.BreadcrumbItem>();
        protected PageToolbar Toolbar { get; } = new PageToolbar();
        private int PageSize { get; set; } = 20;
        private int CurrentPage { get; set; } = 1;
        private int MaxCount { get; } = 1000;
        private int TotalCount { get; set; }
        private string CurrentSorting { get; set; } = string.Empty;

        private HQSOFTBreadcrumbScreen BreadcrumbScreen; 
        private bool IsSelected { get; set; } = false; 
        private bool ShowAdvancedFilters { get; set; }

        private bool CanCreate { get; set; }
        private bool CanEdit { get; set; }
        private bool CanDelete { get; set; }

        private WorkspaceDto WorkspaceMenu { get; set; } = new WorkspaceDto(); private readonly object lockObject = new object();

        private bool isToolbarUpdating = false; // Biến để theo dõi trạng thái Toolbar



        //Custom code: add more code based on actual requirement 
        private GetHangfireConfigsInput Filter { get; set; }

        private HangfireConfigDto? SelectedHangfireJob;

        private List<HangfireConfigDto> SelectedDocs { get; set; } = new List<HangfireConfigDto>();
        private IReadOnlyList<CronExpressionTypeList> CronExpressionList { get; set; } = new List<CronExpressionTypeList>();
        private IReadOnlyList<TimeZoneInfoTypeList> TimeZoneInfoList { get; set; } = new List<TimeZoneInfoTypeList>();
        private IReadOnlyList<HangfireConfigDto> DocList { get; set; }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *										Initialize Section
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        public HangfireConfigsListView()
        {
            Filter = new GetHangfireConfigsInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting
            };
            DocList = new List<HangfireConfigDto>();
            BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
        }

        protected override async Task OnInitializedAsync()
        {
            await SetPermissionsAsync();
            await SetToolbarItemsAsync();
            

            await GetCollectionAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

                await JSRuntime.InvokeVoidAsync("FullScreen");
                await JSRuntime.InvokeVoidAsync("AssignGotFocus"); 
                
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



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								ToolBar - Breadcrumb - Permission
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
         
        protected virtual ValueTask SetToolbarItemsAsync()
        {
            Toolbar.Contributors.Clear();

            Toolbar.AddButton(L["Refresh"], async () => await GetDataAsync(true),
            icon: "fa fa-sync",
            Color.Light);

            Toolbar.AddComponent<ListViewHorizontal>();

            var parmAction = new Dictionary<string, object>()
             {
                {"IsSelected", IsSelected = SelectedDocs.Count() > 0 ? true : false },
                {"CanCreate", CanCreate},
                {"CanDelete", CanDelete },
                {"IsVisibleImport", false },
                {"IsVisibleExport",false },
                {"DeleteAsync", EventCallback.Factory.Create(this, DeleteAsync)},
                {"CreateNewAsync", EventCallback.Factory.Create(this, CreateNewAsync) }
             };
            Toolbar.AddComponent<ListViewAction>(parmAction);
            return ValueTask.CompletedTask;
        }

        private async Task SetPermissionsAsync()
        {
            CanCreate = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.HangfireConfigs.Create);
            CanEdit = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.HangfireConfigs.Edit);
            CanDelete = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.HangfireConfigs.Delete);
        }

        private async Task ResetToolbarAsync()
        {
            if (isToolbarUpdating) return; // Nếu toolbar đang được cập nhật thì không làm gì thêm

            try
            {
                isToolbarUpdating = true; // Đặt cờ khi bắt đầu cập nhật

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
            finally
            {
                isToolbarUpdating = false; // Hủy cờ khi cập nhật xong
            }
        }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								                CRUD
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        private async Task CreateNewAsync()
        {
            NavigationManager.NavigateTo($"/SystemAdministration/HangfireConfigs/{Guid.Empty}");
            await Task.CompletedTask;
        }

        private async Task DeleteAsync()
        {
            try
            {
                if (!SelectedDocs.Any())
                    return;

                var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
                if (confirmed)
                {
                    await HangfireConfigsAppService.DeleteByIdsAsync(SelectedDocs.Select(x => x.Id).ToList());
                    await UiNotificationService.Error(L["Notification:Delete"]);

                    SelectedDocs = new List<HangfireConfigDto>();
                    IsSelected = false;
                    await ResetToolbarAsync();
                    await GetDataAsync(true);
                }
            }
            catch (AbpRemoteCallException ex) // Bắt ngoại lệ từ server
            {
                // Hiển thị thông báo lỗi từ server
                await UiMessageService.Warn(ex.Message);
            }

            await InvokeAsync(StateHasChanged);
        }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								  Load Data Source for ListView
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        private async Task GetCollectionAsync()
        {
            CronExpressionList = CommonHelper.GetEnumLookupAsync<CronExpressionType, CronExpressionTypeList>("CronExpressionType", L);
            TimeZoneInfoList = CommonHelper.GetEnumLookupAsync<TimeZoneInfoType, TimeZoneInfoTypeList>("TimeZoneInfoType", L);
        }

        private async Task GetDataAsync(bool isRefresh)
        {
            await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

            if (isRefresh)
            {
                Filter = new GetHangfireConfigsInput()
                {
                    MaxResultCount = PageSize,
                    SkipCount = (CurrentPage - 1) * PageSize,
                    Sorting = string.IsNullOrEmpty(CurrentSorting) ? "JobMethodName" : CurrentSorting
                };
            }
            else
            {
                Filter.MaxResultCount = PageSize;
                Filter.SkipCount = (CurrentPage - 1) * PageSize;
                Filter.Sorting = string.IsNullOrEmpty(CurrentSorting) ? "JobMethodName" : CurrentSorting;
            }

            var result = await HangfireConfigsAppService.GetListAsync(Filter);
            DocList = result.Items;
            TotalCount = (int)result.TotalCount;

            await BlockUiService.UnBlock();
        }

        private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<HangfireConfigDto> e)
        {
            CurrentSorting = e.Columns
                .Where(c => c.SortDirection != SortDirection.Default)
                .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
                .JoinAsString(",");
            CurrentPage = e.Page;

            await GetDataAsync(false);
            await InvokeAsync(StateHasChanged);
        }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *									Controls triggers/events
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        private async Task OnFilterChanged<T>(string filterName, T filterValue)
        {
            typeof(GetHangfireConfigsInput).GetProperty(filterName)?.SetValue(Filter, filterValue);
            await GetDataAsync(false);
        }

        private async Task PageSizeChanged(int value)
        {
            PageSize = value;
            await GetDataAsync(true);
        }

        async Task SelectedRowsChanged(List<HangfireConfigDto> e)
        {
            await ResetToolbarAsync();
        }

        public static string TruncateText(string text, int maxLength) // Cắt chuỗi
        {
            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        public void GotoEditPage(Guid valueId)
        {
            NavigationManager.NavigateTo($"/SystemAdministration/HangfireConfigs/{valueId}");
        }

    }
}
