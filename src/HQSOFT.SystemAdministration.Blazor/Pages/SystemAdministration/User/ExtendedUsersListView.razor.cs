using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HQSOFT.CoreBackend.ExtendedUsers;
using HQSOFT.CoreBackend.Permissions;
using HQSOFT.CoreBackend.Shared;
using HQSOFT.CoreBackend.States;
using HQSOFT.CoreBackend.Provinces;
using HQSOFT.CoreBackend.Territories;
using HQSOFT.CoreBackend.RouteTypes;
using HQSOFT.Common.Blazor.Pages.Component.Toolbar;

using Blazorise;
using Blazorise.DataGrid;
using Blazorise.Snackbar;

using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars; 

using Microsoft.AspNetCore.Components; 
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Http.Client;
using HQSOFT.CoreBackend.Companies;
using HQSOFT.CoreBackend.Screens;
using HQSOFT.CoreBackend.Workspaces;
using HQSOFT.Common.Blazor.Pages.Component;


namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.User
{
    public partial class ExtendedUsersListView
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

        private WorkspaceDto WorkspaceMenu { get; set; } = new WorkspaceDto(); 
        private readonly object lockObject = new object();

        private bool isToolbarUpdating = false; // Biến để theo dõi trạng thái Toolbar


        //Custom code: add more code based on actual requirement 

        private Dictionary<bool, string> StatusOptions;
        private ExtendedUserCreateDto NewExtendedUser { get; set; }
        private ExtendedUserUpdateDto EditingExtendedUser { get; set; }
        private GetExtendedUsersInput Filter { get; set; }

        private ExtendedUserWithNavigationPropertiesDto? SelectedExtendedUser;

        private List<RouteTypeDto> RouteTypeList { get; set; } = new List<RouteTypeDto>();
        private List<ExtendedUserWithNavigationPropertiesDto> SelectedDocs { get; set; } = new List<ExtendedUserWithNavigationPropertiesDto>();

        private IReadOnlyList<ExtendedUserWithNavigationPropertiesDto> DocList { get; set; } = new List<ExtendedUserWithNavigationPropertiesDto>();
        private IReadOnlyList<TerritoryDto> TerritoryCollection { get; set; } = new List<TerritoryDto>();
        private IReadOnlyList<TerritoryDto> FilterTerritoriesCollection { get; set; } = new List<TerritoryDto>();
        private IReadOnlyList<ProvinceDto> ProvinceCollection { get; set; } = new List<ProvinceDto>();
        private IReadOnlyList<ProvinceDto> FilterProvincesCollection { get; set; } = new List<ProvinceDto>();
        private IReadOnlyList<StateDto> StateCollection { get; set; } = new List<StateDto>();
        private IReadOnlyList<LookupDto<Guid>> RouteTypesCollection { get; set; } = new List<LookupDto<Guid>>();


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *										Initialize Section
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region 
        public ExtendedUsersListView()
        {
            NewExtendedUser = new ExtendedUserCreateDto();
            EditingExtendedUser = new ExtendedUserUpdateDto();
            Filter = new GetExtendedUsersInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting
            };
            BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

                await JSRuntime.InvokeVoidAsync("FullScreen");
                await JSRuntime.InvokeVoidAsync("AssignGotFocus"); await BreadcrumbScreen.GetBreadcrumbsAsync();

                await LoadGridData();

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



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								ToolBar - Breadcrumb - Permission
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

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
                {"CanCreate", CanCreate},
                {"CanDelete", CanDelete },
                {"IsSelected", IsSelected = SelectedDocs.Count() > 0 ? true : false },
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
                .IsGrantedAsync(CoreBackendPermissions.ExtendedUsers.Create);
            CanEdit = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.ExtendedUsers.Edit);
            CanDelete = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.ExtendedUsers.Delete);
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



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								  Load Data Source for ListView
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region
        private async Task LoadGridData()
        {
            await SetPermissionsAsync();
            await SetToolbarItemsAsync();
            

            await GetTerritoryCollectionAsync();
            await GetProvinceCollectionAsync();
            await GetStateCollectionAsync();
            await GetRouteTypeCollectionAsync();

            StatusOptions = new Dictionary<bool, string>
            {
                { true, L["Active"] },
                { false, L["UnActive"] }
            };
        }

        private async Task GetDataAsync(bool isRefresh)
        {
            await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

            if (isRefresh) 
            {
                Filter = new GetExtendedUsersInput()
                {
                    MaxResultCount = PageSize,
                    SkipCount = (CurrentPage - 1) * PageSize,
                    Sorting = string.IsNullOrEmpty(CurrentSorting) ? "ExtendedUser.Code" : CurrentSorting
                };
            }
            else
            {
                Filter.MaxResultCount = PageSize;
                Filter.SkipCount = (CurrentPage - 1) * PageSize;
                Filter.Sorting = string.IsNullOrEmpty(CurrentSorting) ? "ExtendedUser.Code" : CurrentSorting;
            }

            var result = await ExtendedUsersAppService.GetListAsync(Filter);
            DocList = result.Items;
            TotalCount = (int)result.TotalCount;
             
            await InvokeAsync(StateHasChanged);
            await BlockUiService.UnBlock(); 
        }

        private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<ExtendedUserWithNavigationPropertiesDto> e)
        {
            CurrentSorting = e.Columns
                .Where(c => c.SortDirection != SortDirection.Default)
                .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
                .JoinAsString(",");
            CurrentPage = e.Page;
            await GetDataAsync(false);
            await InvokeAsync(StateHasChanged);
        }

        private async Task GetStateCollectionAsync()
        {
            StateCollection = (await StatesAppService.GetListNoPagedAsync(new GetStatesInput { MaxResultCount = MaxCount, }));
        }

        private async Task GetProvinceCollectionAsync()
        {
            ProvinceCollection = (await ProvincesAppService.GetListNoPagedAsync(new GetProvincesInput { MaxResultCount = MaxCount, }));
        }

        private async Task GetTerritoryCollectionAsync()
        {
            TerritoryCollection = (await TerritoriesAppService.GetListNoPagedAsync(new GetTerritoriesInput { MaxResultCount = MaxCount, }));
        }

        private async Task GetRouteTypeCollectionAsync()
        {
            RouteTypeList = await RouteTypesAppService.GetListNoPagedAsync(new GetRouteTypesInput { });
        }

        #endregion



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								                CRUD
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region
        private async Task CreateNewAsync()
        {
            NavigationManager.NavigateTo($"SystemAdministration/Users/{Guid.Empty}");
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
                    await ExtendedUsersAppService.DeleteByIdsAsync(SelectedDocs.Select(x => x.ExtendedUser.Id).ToList());
                    await UiNotificationService.Error(L["Notification:Delete"]);

                    SelectedDocs = new List<ExtendedUserWithNavigationPropertiesDto>();
                    IsSelected = false;
                    await ResetToolbarAsync();
                    await GetDataAsync(true);
                }

                await InvokeAsync(StateHasChanged);
            }
            catch (AbpRemoteCallException ex) // Bắt ngoại lệ từ server
            {
                // Hiển thị thông báo lỗi từ server
                await UiMessageService.Warn(ex.Message);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
            }
        }

        #endregion



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *									Controls triggers/events
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region 
        async Task SelectedRowsChanged(List<ExtendedUserWithNavigationPropertiesDto> e)
        {
            await ResetToolbarAsync();
        }
         
        private async Task OnFilterChanged<T>(string filterName, T filterValue)
        {
            typeof(GetExtendedUsersInput).GetProperty(filterName)?.SetValue(Filter, filterValue);
            await GetDataAsync(false);
        }

        private async Task OnTerritoryFilterChanged<T>(string filterName, T filterValue)
        {
            typeof(GetExtendedUsersInput).GetProperty(filterName)?.SetValue(Filter, filterValue);
            await GetDataAsync(false); 

            Filter.ProvinceId = null; 

            if (Filter.TerritoryId != null)
            {
                var provincesCollection = await ProvincesAppService.GetListNoPagedAsync(new GetProvincesInput { });
                FilterProvincesCollection = provincesCollection.Where(x => x.TerritoryId == Filter.TerritoryId).ToList();
            }
        }

        private async Task ExportAsync()
        {
            string screenName = "ExtendedUsers";
            string currentDate = DateTime.Now.ToString("yyyyMMdd");
            string currentTime = DateTime.Now.ToString("HHmm");

            string fileName = screenName + currentDate + currentTime;
            var token = (await ExtendedUsersAppService.GetDownloadTokenAsync()).Token;

            // Chuyển đổi danh sách docIds thành nhiều tham số docIds
            var selectedDocIds = SelectedDocs.Select(x => x.ExtendedUser.Id).ToList();
            var docIdsQueryString = string.Join("&", selectedDocIds.Select(id => $"docIds={id}")); // Tạo chuỗi query string với nhiều tham số docIds

            var remoteService = await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("SystemAdministration") ??
                await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("Default");

            NavigationManager.NavigateTo(
                $"{remoteService?.BaseUrl.EnsureEndsWith('/') ?? string.Empty}api/core-backend/extended-users/as-excel-file-by-doc-ids?DownloadToken={token}&{docIdsQueryString}&fileName={fileName}",
                forceLoad: true
            );

            IsSelected = false;
        }
  
        private async Task PageSizeChanged(int value)
        {
            PageSize = value;
            await GetDataAsync(false);
        }

        public static string TruncateText(string text, int maxLength) // Cắt chuỗi
        {
            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        private async Task<string> SanitizeCode(string input)
        {
            if (Regex.IsMatch(input, @"\s|[^a-zA-Z0-9_]"))
            {
                input = Regex.Replace(input, @"[^a-zA-Z0-9]", "");
                await UiMessageService.Error(L["CodeValidationError"]);
            }

            if (DocList.Any(x => x.ExtendedUser.Code.Equals(input)))
            {
                input = string.Empty;
                await UiMessageService.Error(L["DuplicateCodeError"]);
            }

            return input;
        }

        protected void GotoEditPage(Guid Id)
        {
            NavigationManager.NavigateTo($"SystemAdministration/Users/{Id}");
        }

        #endregion


    }
}
