using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using HQSOFT.CoreBackend.Permissions;
using HQSOFT.CoreBackend.Modules;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Authorization;

using Blazorise;
using Blazorise.DataGrid;
using Blazorise.Snackbar;

using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using Volo.Abp.AspNetCore.Components.Messages;
using HQSOFT.CoreBackend.ExtendedUsers;
using HQSOFT.CoreBackend.Screens;
using HQSOFT.CoreBackend.Workspaces;
using Volo.Abp.Http.Client;
using HQSOFT.CoreBackend.HangfireConfigs;
using HQSOFT.CoreBackend.Reports;
using HQSOFT.Common.Blazor.Pages.Component;



namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.Module
{
    public partial class ModulesListView
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
        private GetModulesInput Filter { get; set; }
        private IReadOnlyList<ModuleDto> DocList { get; set; } = new List<ModuleDto>();
        private List<ModuleDto> SelectedDocs { get; set; } = new List<ModuleDto>();



        //========================================= Initialize Section ======================================== 
        #region 

        public ModulesListView()
        {
            Filter = new GetModulesInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting
            };
            BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
        }

        protected override async Task OnInitializedAsync()
        {
            await SetPermissionsAsync();
            await SetToolbarItemsAsync();
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

        #endregion


        //======================================= ToolBar - Breadcrumb ========================================

        #region  
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
                {"IsVisibleExport",true},
                {"ExportAsync", EventCallback.Factory.Create(this, ExportAsync) },
                {"DeleteAsync", EventCallback.Factory.Create(this, DeleteAsync)},
                {"CreateNewAsync", EventCallback.Factory.Create(this, CreateNewAsync) }
             };
            Toolbar.AddComponent<ListViewAction>(parmAction);
            return ValueTask.CompletedTask;
        }

        private async Task SetPermissionsAsync()
        {
            CanCreate = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.Modules.Create);
            CanEdit = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.Modules.Edit);
            CanDelete = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.Modules.Delete);
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

        #endregion


        //================================== Load Data, Search, CreateNewAsync, DeleteAsync, Export Excel =====================
        #region 

        private async Task GetDataAsync(bool isRefresh)
        {
            await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

            if (isRefresh)
            {
                Filter = new GetModulesInput()
                {
                    MaxResultCount = PageSize,
                    SkipCount = (CurrentPage - 1) * PageSize,
                    Sorting = string.IsNullOrEmpty(CurrentSorting) ? "Code" : CurrentSorting
                };
            }
            else
            {
                Filter.MaxResultCount = PageSize;
                Filter.SkipCount = (CurrentPage - 1) * PageSize;
                Filter.Sorting = string.IsNullOrEmpty(CurrentSorting) ? "Code" : CurrentSorting;
            }

            var result = await ModulesAppService.GetListAsync(Filter);
            DocList = result.Items;
            TotalCount = (int)result.TotalCount;

            await InvokeAsync(StateHasChanged);
            await BlockUiService.UnBlock();
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
                    await ModulesAppService.DeleteByIdsAsync(SelectedDocs.Select(x => x.Id).ToList());
                    await UiNotificationService.Error(L["Notification:Delete"]);
                }
            }
            catch (AbpRemoteCallException ex) // Bắt ngoại lệ từ server
            {
                // Hiển thị thông báo lỗi từ server
                await UiMessageService.Warn(ex.Message);
            }


            SelectedDocs = new List<ModuleDto>();
            IsSelected = false;
            await ResetToolbarAsync();
            await GetDataAsync(true);
            await InvokeAsync(StateHasChanged);
        }

        private void CreateNewAsync()
        {
            NavigationManager.NavigateTo($"/SystemAdministration/Modules/{Guid.Empty}");
        }

        protected void GoToEditPage(Guid Id)
        {
            NavigationManager.NavigateTo($"SystemAdministration/Modules/{Id}");
        }

        #endregion


        //====================================Controls triggers/events=========================================
        #region Controls triggers/events

        private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<ModuleDto> e)
        {
            CurrentSorting = e.Columns
                .Where(c => c.SortDirection != SortDirection.Default)
                .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
                .JoinAsString(",");
            CurrentPage = e.Page;

            await GetDataAsync(false);
            await InvokeAsync(StateHasChanged);
        }

        private async Task OnFilterChanged<T>(string filterName, T filterValue)
        {
            typeof(GetModulesInput).GetProperty(filterName)?.SetValue(Filter, filterValue);
            await GetDataAsync(false);
        }

        private async Task PageSizeChanged(int value)
        {
            PageSize = value;
            await GetDataAsync(false);
        }

        private async Task SelectedRowsChanged(List<ModuleDto> e)
        {
            await ResetToolbarAsync();
        }

        public static string TruncateText(string text, int maxLength) // Cắt chuỗi
        {
            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }


        private async Task ExportAsync()
        {
            string screenName = "Modules";
            string currentDate = DateTime.Now.ToString("yyyyMMdd");
            string currentTime = DateTime.Now.ToString("HHmm");

            string fileName = screenName + currentDate + currentTime;
            var token = (await ModulesAppService.GetDownloadTokenAsync()).Token;

            // Chuyển đổi danh sách docIds thành nhiều tham số docIds
            var selectedDocIds = SelectedDocs.Select(x => x.Id).ToList();
            var docIdsQueryString = string.Join("&", selectedDocIds.Select(id => $"docIds={id}")); // Tạo chuỗi query string với nhiều tham số docIds

            var remoteService = await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("SystemAdministration") ??
                await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("Default");

            NavigationManager.NavigateTo(
                $"{remoteService?.BaseUrl.EnsureEndsWith('/') ?? string.Empty}api/core-backend/modules/as-excel-file-by-doc-ids?DownloadToken={token}&{docIdsQueryString}&fileName={fileName}",
                forceLoad: true
            );

            IsSelected = false;
        }
        #endregion


        //==================================Additional Application Functions===================================
        #region Additional Application Functions

        #endregion


    }
}