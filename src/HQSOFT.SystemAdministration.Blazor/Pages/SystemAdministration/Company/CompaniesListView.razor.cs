using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using DevExpress.Blazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using HQSOFT.CoreBackend.Companies;
using Volo.Abp.AspNetCore.Components.Messages;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using HQSOFT.CoreBackend.Countries;
using HQSOFT.CoreBackend.Shared;
using Microsoft.AspNetCore.Components;
using Volo.Abp.Http.Client;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System.Web;
using Blazorise.Snackbar;
using Volo.Abp.BlazoriseUI.Components;
using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using HQSOFT.CoreBackend.Permissions;
using HQSOFT.CoreBackend.SalesRoutes;
using HQSOFT.CoreBackend.ExtendedUsers;
using HQSOFT.CoreBackend.Territories;
using HQSOFT.CoreBackend.Provinces;
using HQSOFT.CoreBackend.HangfireConfigs;
using DevExpress.XtraEditors.Controls;
using HQSOFT.CoreBackend.States;
using DevExpress.Utils.Commands;
using System.ComponentModel.Design;
using static HQSOFT.CoreBackend.Permissions.CoreBackendPermissions;
using HQSOFT.CoreBackend.Modules;
using HQSOFT.CoreBackend.Workspaces;
using HQSOFT.Common.Blazor.Pages.Component;


namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.Company
{
    public partial class CompaniesListView
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
        private GetCompaniesInput Filter { get; set; }

        private IReadOnlyList<CompanyWithNavigationPropertiesDto> DocList { get; set; }
        private IReadOnlyList<CompanyWithNavigationPropertiesDto> CompaniesCollection { get; set; } = new List<CompanyWithNavigationPropertiesDto>();
        private IReadOnlyList<CompanyDto> ParentCompaniesCollection { get; set; } = new List<CompanyDto>();
        private IReadOnlyList<CountryDto> CountriesCollection { get; set; } = new List<CountryDto>();
        private IReadOnlyList<TerritoryDto> TerritoriesCollection { get; set; } = new List<TerritoryDto>();
        private IReadOnlyList<ProvinceDto> ProvincesCollection { get; set; } = new List<ProvinceDto>();
        private IReadOnlyList<TerritoryDto> FilterTerritoriesCollection { get; set; } = new List<TerritoryDto>();
        private IReadOnlyList<ProvinceDto> FilterProvincesCollection { get; set; } = new List<ProvinceDto>();

        private List<CompanyWithNavigationPropertiesDto> SelectedDocs { get; set; } = new List<CompanyWithNavigationPropertiesDto>();
        private List<string> ViewMode { get; set; } = new List<string>();

        private string SelectedViewMode { get; set; }

        private CompanyDto SelectedCompany { get; set; } = new CompanyDto();
        private CompanyDto CompanyContext { get; set; } = new CompanyDto();

        DxContextMenu ContextMenu { get; set; }
        DxContextMenu ContextMenu2 { get; set; }

        DxTreeView SampleTreeView;


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *										Initialize Section
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region
        public CompaniesListView()
        {
            Filter = new GetCompaniesInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting
            };
            DocList = new List<CompanyWithNavigationPropertiesDto>();
            BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

                await JSRuntime.InvokeVoidAsync("FullScreen");
                await JSRuntime.InvokeVoidAsync("AssignGotFocus"); 
                
                await BreadcrumbScreen.GetBreadcrumbsAsync();

                //Dùng local storage để lưu SelecteViewMode
                var getViewModeStorage = SelectedViewMode = await LocalStorage.GetItemAsync("ViewMode");
                if (getViewModeStorage.IsNullOrEmpty())
                {
                    SelectedViewMode = "List View";
                }
                else
                {
                    SelectedViewMode = getViewModeStorage;
                }

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
             
            ViewMode = new List<string> { "List View", "Tree View" };

            await GetCollectionAsync();
            await GetCompanyCollectionLookupAsync(true);
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

            Console.WriteLine("IsSelectedToolbar: " + IsSelected);
            return ValueTask.CompletedTask;
        }

        private async Task SetPermissionsAsync()
        {
            CanCreate = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.Companies.Create);
            CanEdit = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.Companies.Edit);
            CanDelete = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.Companies.Delete);
        }

        #endregion




        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								Load Data Source for ListView
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        #region  
        private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<CompanyWithNavigationPropertiesDto> e)
        {
            CurrentSorting = e.Columns
                .Where(c => c.SortDirection != SortDirection.Default)
                .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
                .JoinAsString(",");
            CurrentPage = e.Page;

            await GetDataAsync(false);
            await InvokeAsync(StateHasChanged);
        }

        private async Task GetCollectionAsync()
        {
            CountriesCollection = await CountriesAppService.GetListNoPagedAsync(new GetCountriesInput { });
            TerritoriesCollection = await TerritoriesAppService.GetListNoPagedAsync(new GetTerritoriesInput { });
            ProvincesCollection = await ProvincesAppService.GetListNoPagedAsync(new GetProvincesInput { });

            ParentCompaniesCollection = await CompaniesAppService.GetListNoPagedAsync(new GetCompaniesInput { });
        }

        private async Task GetCompanyCollectionLookupAsync(bool isRefresh)
        {
            if (isRefresh)
            {
                CompaniesCollection = (await CompaniesAppService.GetListNoPagedWithNavigationPropertiesAsync(new GetCompaniesInput { }));
            }
            else
            {
                CompaniesCollection = (await CompaniesAppService.GetListNoPagedWithNavigationPropertiesAsync(new GetCompaniesInput { FilterText = Filter.FilterText, Code = Filter.Code, Description = Filter.Description }));
            }
        }

        private async Task GetDataAsync(bool isRefresh)
        {
            await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);
            if (isRefresh)
            {
                Filter = new GetCompaniesInput()
                {
                    MaxResultCount = PageSize,
                    SkipCount = (CurrentPage - 1) * PageSize,
                    Sorting = string.IsNullOrEmpty(CurrentSorting) ? "Company.Code" : CurrentSorting
                };
                await GetCompanyCollectionLookupAsync(true);
            } 
            else
            {
                Filter.MaxResultCount = PageSize;
                Filter.SkipCount = (CurrentPage - 1) * PageSize;
                Filter.Sorting = string.IsNullOrEmpty(CurrentSorting) ? "Company.Code" : CurrentSorting;
            }

            var result = await CompaniesAppService.GetListAsync(Filter);
            DocList = result.Items;
            TotalCount = (int)result.TotalCount;

            SelectedDocs = new List<CompanyWithNavigationPropertiesDto>();
            IsSelected = false;
            await ResetToolbarAsync(); 
            await BlockUiService.UnBlock();
            await InvokeAsync(StateHasChanged);
        }

        #endregion



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								CRUD & Load Main Data Source Section
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        #region

        private async Task SelectedCompanyRowsChanged(List<CompanyWithNavigationPropertiesDto> e)
        {
            SelectedDocs = e;
            await ResetToolbarAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async Task DeleteCompanyButton(Guid id)
        {
            try
            {
                await CompaniesAppService.DeleteAsync(id);
                await UiNotificationService.Error(L[$"Notification:Delete"]);
                await GetDataAsync(true);
            }
            catch (AbpRemoteCallException ex) // Bắt ngoại lệ từ server
            {
                // Hiển thị thông báo lỗi từ server
                await UiMessageService.Warn(ex.Message);
            }
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
                    await CompaniesAppService.DeleteByIdsAsync(SelectedDocs.Select(x => x.Company.Id).ToList());
                    await UiNotificationService.Error(L["Notification:Delete"]);
                }
            }
            catch (AbpRemoteCallException ex) // Bắt ngoại lệ từ server
            {
                // Hiển thị thông báo lỗi từ server
                await UiMessageService.Warn(ex.Message);
            }

            SelectedDocs = new List<CompanyWithNavigationPropertiesDto>();
            IsSelected = false;
            await ResetToolbarAsync();
            await GetDataAsync(true);
            await InvokeAsync(StateHasChanged);
        }

        private async Task CreateNewAsync()
        {
            NavigationManager.NavigateTo($"/SystemAdministration/Companies/{Guid.Empty}");
            await Task.CompletedTask;
        }

        #endregion



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *									Controls triggers/events
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region 

        public static string TruncateText(string text, int maxLength) // Cắt chuỗi
        {
            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        private async Task OnFilterChanged<T>(string filterName, T filterValue)
        {
            typeof(GetCompaniesInput).GetProperty(filterName)?.SetValue(Filter, filterValue);
            await GetDataAsync(false);
            await GetCompanyCollectionLookupAsync(false);
        }

        private async Task OnCountryFilterChanged<T>(string filterName, T filterValue)
        {
            typeof(GetCompaniesInput).GetProperty(filterName)?.SetValue(Filter, filterValue);
            await GetDataAsync(false);
            await GetCompanyCollectionLookupAsync(false);

            Filter.TerritoryId = null;
            Filter.ProvinceId = null;

            FilterTerritoriesCollection = new List<TerritoryDto>();
            FilterProvincesCollection = new List<ProvinceDto>();

            if (Filter.CountryId != null)
            {
                var territoriesCollection = await TerritoriesAppService.GetListNoPagedAsync(new GetTerritoriesInput { });
                FilterTerritoriesCollection = territoriesCollection.Where(x => x.CountryId == Filter.CountryId).ToList();

                var provincesCollection = await ProvincesAppService.GetListNoPagedAsync(new GetProvincesInput { });
                FilterProvincesCollection = provincesCollection.Where(x => x.TerritoryId == Filter.TerritoryId).ToList();
            }
        }

        private async Task OnTerritoryFilterChanged<T>(string filterName, T filterValue)
        {
            typeof(GetCompaniesInput).GetProperty(filterName)?.SetValue(Filter, filterValue);
            await GetDataAsync(false);
            await GetCompanyCollectionLookupAsync(false);

            Filter.ProvinceId = null;
            FilterProvincesCollection = new List<ProvinceDto>();

            if (Filter.TerritoryId != null)
            {
                var provincesCollection = await ProvincesAppService.GetListNoPagedAsync(new GetProvincesInput { });
                FilterProvincesCollection = provincesCollection.Where(x => x.TerritoryId == Filter.TerritoryId).ToList();
            }
        }

        private void GoToEditPage(Guid companyId)
        {
            NavigationManager.NavigateTo($"/SystemAdministration/Companies/{companyId}");
        }

        private async Task ExportAsync()
        {
            string screenName = "Companies";
            string currentDate = DateTime.Now.ToString("yyyyMMdd");
            string currentTime = DateTime.Now.ToString("HHmm");

            string fileName = screenName + currentDate + currentTime;
            var token = (await CompaniesAppService.GetDownloadTokenAsync()).Token;

            // Chuyển đổi danh sách docIds thành nhiều tham số docIds
            var selectedDocIds = SelectedDocs.Select(x => x.Company.Id).ToList();
            var docIdsQueryString = string.Join("&", selectedDocIds.Select(id => $"docIds={id}")); // Tạo chuỗi query string với nhiều tham số docIds

            var remoteService = await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("SystemAdministration") ??
                await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("Default");

            NavigationManager.NavigateTo(
                $"{remoteService?.BaseUrl.EnsureEndsWith('/') ?? string.Empty}api/core-backend/companies/as-excel-file-by-doc-ids?DownloadToken={token}&{docIdsQueryString}&fileName={fileName}",
                forceLoad: true
            );

            IsSelected = false;
        }

        private async Task PageSizeChanged(int value)
        {
            PageSize = value;
            await GetDataAsync(true);
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

        private async void OnItemClick(ContextMenuItemClickEventArgs args)
        {
            if (args.ItemInfo.Text == "Add")
            {
                //Add node con vào node cha, truyền id của node cha sang node con để làm parent company
                var company = await CompaniesAppService.GetAsync(CompanyContext.Id);
                NavigationManager.NavigateTo($"/SystemAdministration/Companies/{Guid.Empty}?ParentCompany={company.Id}");
            }
            if (args.ItemInfo.Text == "Edit")
            {
                GoToEditPage(CompanyContext.Id);
            }

            if (args.ItemInfo.Text == "Delete")
            {
                var company = await CompaniesAppService.GetAsync(CompanyContext.Id);
                //Tìm Node đang chọn, nếu ID của node tồn tại trong cột ParentCompany => Node đó là node cha của 1 node nào đó và có tồn tại node con
                var isParentCompany = CompaniesCollection.Where(x => x.Company.ParentCompanyId == company.Id).FirstOrDefault();
                //Tồn tại node còn nên không thể xóa
                if (isParentCompany != null)
                {
                    await UiNotificationService.Warn(
                            L[$"Cannot delete {company.Description} as it has child nodes"]
                        );
                }
                //Xóa nếu không có node con
                else
                {
                    await DeleteCompanyButton(company.Id);
                }
                await GetDataAsync(true);
            }
        }

        private void SelectionChanged(TreeViewNodeEventArgs e)
        {
            SelectedCompany = (CompanyDto)e.NodeInfo.DataItem; /// lấy ra dto đang chọn
			SampleTreeView.SelectNode((n) => n.Text == e.NodeInfo.Text);
        }

        private async Task OnContextMenu(MouseEventArgs e, CompanyWithNavigationPropertiesDto companyDto)
        {
            // Ngăn trình duyệt mở menu mặc định
            await JSRuntime.InvokeVoidAsync("eval", "event.preventDefault();");

            CompanyContext = companyDto.Company;

            //Nếu là node cha thì hiện context menu có chức năng add nút con
            if (CompanyContext.IsGroup == true)
            {
                await ContextMenu.ShowAsync(e);
            }

            //Hiện context menu không có add node con
            else
            {
                await ContextMenu2.ShowAsync(e);
            }

        }

        private async Task ViewModeChangedAsync(string viewMode)
        {
            SelectedViewMode = viewMode;
            await LocalStorage.SetItemAsync("ViewMode", SelectedViewMode);
            await InvokeAsync(StateHasChanged);
        }
                                                                                           
        #endregion
         

    }
}

