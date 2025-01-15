using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Blazorise;
using Blazorise.DataGrid;
using Blazorise.Snackbar;
using DevExpress.Blazor;

using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using Volo.Abp.AspNetCore.Components.Web.Configuration;
using Volo.Abp.AspNetCore.Components.BlockUi;
using Volo.Abp.PermissionManagement.Localization;
using Volo.Abp.PermissionManagement;
using Volo.Abp.AspNetCore.Components.Messages;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Localization;
using Volo.Abp.Identity;

using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms;

using HQSOFT.CoreBackend.EnumList;
using HQSOFT.CoreBackend.RouteTypes;
using HQSOFT.CoreBackend.SalesRoutes;
using HQSOFT.CoreBackend.Utils;
using HQSOFT.CoreBackend.ReportParameters;
using HQSOFT.CoreBackend.Reports;
using HQSOFT.CoreBackend.Permissions;
using HQSOFT.CoreBackend.Localization;
using HQSOFT.CoreBackend.ExtendedUsers;

using HQSOFT.Common.Blazor.Pages.Component;
using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using HQSOFT.CoreBackend.Workspaces;



namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.AccessRight
{
    public partial class AccessRights
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
        private bool IsDataEntryChanged { get; set; }
        private bool ShowInteractionForm { get; set; } = true;
        private bool ShowAdvancedFilters { get; set; }

        private WorkspaceDto WorkspaceMenu { get; set; } = new WorkspaceDto(); 
        private EditForm EditFormMain { get; set; } = new EditForm();

        private bool CanCreate { get; set; }
        private bool CanEdit { get; set; }
        private bool CanDelete { get; set; }


        //Custom code: add more code based on actual requirement    

        private PermissionGroupDto SelectedGroup { get; set; }
        private IdentityRoleDto EditingRole { get; set; }
        private IdentityUserDto EditingUser { get; set; }

        private List<PermissionGroupDto> _groups;

        private List<PermissionGrantInfoDto> _disabledPermissions = new List<PermissionGrantInfoDto>();
        private List<PermissionGroupDefinitionRecord> PermissionGroups { get; set; } = new List<PermissionGroupDefinitionRecord>();


        // Lưu trữ quyền đã chọn 
        private List<string> SelectedPermissionList = new List<string>();
        private List<string> SelectedPermissionNoneList = new List<string>() { "None" };

        private List<PermissionGrantInfoDto> permissionGrants = new List<PermissionGrantInfoDto>();


        private IReadOnlyList<IdentityRoleDto> RoleCollection { get; set; } = new List<IdentityRoleDto>();
        private IReadOnlyList<IdentityUserDto> UserCollection { get; set; } = new List<IdentityUserDto>();
        private IReadOnlyList<AccessTypeList> AccessTypeLists { get; set; } = new List<AccessTypeList>();
        private IReadOnlyList<object> SelectedPermissions { get; set; } = new List<PermissionGrantInfoDto>(); //Selected rows on grid



        private int _grantedPermissionCount = 0;
        private int _notGrantedPermissionCount = 0;

        private string _providerName { get; set; }
        private string _providerKey { get; set; }
        private string _entityDisplayName { get; set; }
        private string _selectedTabName { get; set; }


        private string SearchKeyword { get; set; } = string.Empty;
        private string AccessRightType { get; set; }
        private string CheckAuthorization { get; set; }


        private bool IsDisplayPermission { get; set; }
        private bool SelectAllDisabled { get; set; }

        private bool IsOpen { get; set; } = false;



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *										Initialize Section
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region 
        public AccessRights()
        {
            LocalizationResource = typeof(AbpPermissionManagementResource);
            LocalizationResource = typeof(CoreBackendResource);
            BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

                await JSRuntime.InvokeVoidAsync("FullScreen");
                await JSRuntime.InvokeVoidAsync("AssignGotFocus"); await BreadcrumbScreen.GetBreadcrumbsAsync();

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
            await FirstLoadAsync();
        }

        #endregion



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								ToolBar - Breadcrumb - Permission
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region  
        protected virtual ValueTask SetToolbarItemsAsync()
        {
            Toolbar.AddButton(L["Save"], async () => await SaveDataAsync(),
            IconName.Save,
            Color.Primary);

            return ValueTask.CompletedTask;
        }
        #endregion




        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								    Load Data Source for ListView
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region Enum List 
        private async Task GetAccessTypeCollectionLookupAsync()
        {
            AccessTypeLists = Enum.GetValues(typeof(AccessType))
             .OfType<AccessType>()
             .Select(t => new AccessTypeList()
             {
                 Value = t.ToString(),
                 DisplayName = L["AccessType." + t.ToString()],
             }).ToList();

            await Task.CompletedTask;
        }
        #endregion Enum List


        #region Data List 
        private async Task FirstLoadAsync()
        {
            await SetToolbarItemsAsync();
            

            await GetUserListAsync();
            await GetRoleListAsync();
            await GetAccessTypeCollectionLookupAsync();
        }

        private async Task GetRoleListAsync()
        {
            var roleListResult = await IdentityRoleAppService.GetListAsync(new GetIdentityRoleListInput { MaxResultCount = 1000 });
            RoleCollection = roleListResult.Items.ToList();
        }

        private async Task GetUserListAsync()
        {
            var userListResult = await IdentityUserAppService.GetListAsync(new GetIdentityUsersInput { MaxResultCount = 1000 });
            UserCollection = userListResult.Items.ToList();
        }
        #endregion Data List





        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *							          Controls triggers/events
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */


        #region Selected Change 
        private async Task AuthorizeSelectedCategoryAsync(string value)
        {
            PermissionGroups.Clear();
            EditingRole = new IdentityRoleDto { };
            EditingUser = new IdentityUserDto { };
            IsDisplayPermission = false;
            CheckAuthorization = value;
            await Task.CompletedTask;
        }
        #endregion



        #region Others.. 

        private void ToggleDropdown(PermissionGroupDto group)
        {
            if (SelectedGroup == group)
            {
                IsOpen = !IsOpen;
            }
            else
            {
                SelectedGroup = group;
                IsOpen = true;
            }
        }

        private async void HandleCtrlS(KeyboardEventArgs e)
        {
            await SaveDataAsync();
        }

        private async Task OnTabChanged(string newTabName)
        {
            // Lưu tab hiện tại nếu cần
            _selectedTabName = newTabName;

            // Đặt lại trạng thái của các quyền 
            SelectedPermissionList.Clear();
            SelectedPermissionList.Add("None");

            // Cập nhật giao diện
            await InvokeAsync(StateHasChanged);
        }
         
		private void OnSearchTextChanged(string newValue)
		{
			SearchKeyword = newValue;
            InvokeAsync(StateHasChanged);
		}

        private async void OnSearchSelectChanged(IReadOnlyList<object> newValue)
        {
            SelectedPermissionList.Clear();

            if (newValue.Any())
            {
                foreach (var group in _groups)
                {
                    foreach (var permission in group.Permissions)
                    { 
                        foreach (var p in newValue.Cast<PermissionGrantInfoDto>())
                        {
                            if (newValue.Cast<PermissionGrantInfoDto>().Any(p =>
                                (permission.ParentName != null && p.Name != null && permission.ParentName.Contains(p.Name)) ||
                                (permission.Name != null && p.Name != null && permission.Name.Contains(p.Name))))
                            {
                                SelectedPermissionList.Add(permission.Name);
                            }
                        }
                    }
                }
            }
            else
            {
                SelectedPermissionList.AddRange(SelectedPermissionNoneList);
            }

            await InvokeAsync(StateHasChanged);
        }

        private bool IsPermissionVisible(string permissionName)
        {
            if (string.IsNullOrEmpty(SearchKeyword))
            {
                return true;
            }

            // Tách các từ khóa tìm kiếm và các từ trong tên quyền
            var keywords = SearchKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var permissionWords = permissionName.Split(new[] { '.', ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

            // Kiểm tra xem tất cả các từ khóa có nằm trong danh sách các từ của tên quyền hay không
            return keywords.All(keyword => permissionWords.Any(word => word.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
        }

        private bool IsPermissionSelectedVisible(string permissionName)
        {
            // Kiểm tra nếu từ khóa tìm kiếm khớp với logic phân tích từ khóa
            var isSearchMatch = string.IsNullOrWhiteSpace(SearchKeyword) || IsPermissionVisible(permissionName);

            // Kiểm tra nếu quyền này có trong danh sách đã chọn
            var IsSelectedMatch = SelectedPermissionList == null || SelectedPermissionList.Count == 0 || SelectedPermissionList.Contains(permissionName);

            // Hiển thị quyền nếu cả từ khóa tìm kiếm và lựa chọn đều khớp
            return isSearchMatch && IsSelectedMatch;
        }

        private string GetParentPermissionName(string permissionName)
        {
            var permissionParts = permissionName.Split('.');
            if (permissionParts.Length > 1)
            {
                return string.Join('.', permissionParts.Take(permissionParts.Length - 1));
            }
            return null; // Không có quyền cha
        }

        #endregion








        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *							                Permission ABP
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region Permission ABP

        private bool GrantAll
        {
            get
            {
                if (_notGrantedPermissionCount == 0)
                {
                    return true;
                }

                return false;
            }
            set
            {
                if (_groups == null)
                {
                    return;
                }

                _grantedPermissionCount = 0;
                _notGrantedPermissionCount = 0;

                foreach (var permission in _groups.SelectMany(x => x.Permissions))
                {
                    if (!IsDisabledPermission(permission))
                    {
                        permission.IsGranted = value;

                        if (value)
                        {
                            _grantedPermissionCount++;
                        }
                        else
                        {
                            _notGrantedPermissionCount++;
                        }
                    }
                }
            }
        }


        private Dictionary<string, int> _permissionDepths = new Dictionary<string, int>();

        public async Task OpenAsync(string providerName, string providerKey, string entityDisplayName = null)
        {
            try
            {
                _providerName = providerName;
                _providerKey = providerKey;

                var result = await PermissionAppService.GetAsync(_providerName, _providerKey);

                _entityDisplayName = entityDisplayName ?? result.EntityDisplayName;
                _groups = result.Groups;

                SelectAllDisabled = _groups.All(IsPermissionGroupDisabled);

                _grantedPermissionCount = 0;
                _notGrantedPermissionCount = 0;
                foreach (var permission in _groups.SelectMany(x => x.Permissions))
                {
                    if (permission.IsGranted && permission.GrantedProviders.All(x => x.ProviderName != _providerName))
                    {
                        _disabledPermissions.Add(permission);
                        continue;
                    }

                    if (permission.IsGranted)
                    {
                        _grantedPermissionCount++;
                    }
                    else
                    {
                        _notGrantedPermissionCount++;
                    }
                }

                _selectedTabName = GetNormalizedGroupName(_groups.First().Name);

                foreach (var group in _groups)
                {
                    SetPermissionDepths(group.Permissions, null, 0);
                }

                permissionGrants = _groups.SelectMany(x => x.Permissions).ToList();

                SelectedPermissionList.Clear();
                SelectedPermissionList.Add("None");
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
            }
        }

        private async Task SaveDataAsync()
        {
            try
            {
                if (IsDataEntryChanged)
                {
                    await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

                    var updateDto = new UpdatePermissionsDto
                    {
                        Permissions = _groups
                            .SelectMany(g => g.Permissions)
                            .Select(p => new UpdatePermissionDto { IsGranted = p.IsGranted, Name = p.Name })
                            .ToArray()
                    };

                    if (!updateDto.Permissions.Any(x => x.IsGranted))
                    {
                        if (!await Message.Confirm(L["SaveWithoutAnyPermissionsWarningMessage"].Value))
                        {
                            return;
                        }
                    }

                    await PermissionAppService.UpdateAsync(_providerName, _providerKey, updateDto);
                    await CurrentApplicationConfigurationCacheResetService.ResetAsync();

                    await BlockUiService.UnBlock();
                    await UiNotificationService.Success(L["Notification:Save"]);

                    IsDataEntryChanged = false;
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
            }
        }

        private string GetNormalizedGroupName(string name)
        {
            return "PermissionGroup_" + name.Replace(".", "_");
        }

        private void SetPermissionDepths(List<PermissionGrantInfoDto> permissions, string currentParent, int currentDepth)
        {
            foreach (var item in permissions)
            {
                if (item.ParentName == currentParent)
                {
                    _permissionDepths[item.Name] = currentDepth;
                    SetPermissionDepths(permissions, item.Name, currentDepth + 1);
                }
            }
        }

        private int GetPermissionDepthOrDefault(string name)
        {
            return _permissionDepths.GetValueOrDefault(name, 0);
        }

        private void CreateGrantAllChanged(bool value, PermissionGroupDto permissionGroup)
        {
            foreach (var permission in permissionGroup.Permissions.Where(p => p.Name.Contains("Create")))
            {
                if (!IsDisabledPermission(permission) && IsPermissionSelectedVisible(permission.Name))
                {
                    SetPermissionGrant(permission, value);
                }
            }
            IsDataEntryChanged = true;
        }

        private void UpdateGrantAllChanged(bool value, PermissionGroupDto permissionGroup)
        {
            foreach (var permission in permissionGroup.Permissions.Where(p => p.Name.Contains("Edit") || p.Name.Contains("Update")))
            {
                if (!IsDisabledPermission(permission) && IsPermissionSelectedVisible(permission.Name))
                {
                    SetPermissionGrant(permission, value);
                }
            }
            IsDataEntryChanged = true;
        }

        private void DeleteGrantAllChanged(bool value, PermissionGroupDto permissionGroup)
        {
            foreach (var permission in permissionGroup.Permissions.Where(p => p.Name.Contains("Delete")))
            {
                if (!IsDisabledPermission(permission) && IsPermissionSelectedVisible(permission.Name))
                {
                    SetPermissionGrant(permission, value);
                }
            }
            IsDataEntryChanged = true;
        }

        private void GroupGrantAllChanged(bool value, PermissionGroupDto permissionGroup)
        {
            foreach (var permission in permissionGroup.Permissions)
            {
                // Chỉ áp dụng thay đổi trên các quyền đang hiển thị
                if (!IsDisabledPermission(permission) && IsPermissionSelectedVisible(permission.Name))
                {
                    SetPermissionGrant(permission, value);
                }
            }
        }

        private void PermissionChanged(bool value, PermissionGroupDto permissionGroup, PermissionGrantInfoDto permission)
        {
            SetPermissionGrant(permission, value);

            if (value)
            {
                SetParentPermissionGrant(permissionGroup, permission);
            }
            else
            {
                var childPermissions = GetChildPermissions(permissionGroup, permission);

                foreach (var childPermission in childPermissions)
                {
                    SetPermissionGrant(childPermission, false);
                }
            }
            IsDataEntryChanged = true;
        }

        private void SetParentPermissionGrant(PermissionGroupDto permissionGroup, PermissionGrantInfoDto permission)
        {
            if (permission.ParentName == null)
            {
                return;
            }

            var parentPermission = GetParentPermission(permissionGroup, permission);
            SetPermissionGrant(parentPermission, true);

            SetParentPermissionGrant(permissionGroup, parentPermission);

        }

        private void SetPermissionGrant(PermissionGrantInfoDto permission, bool value)
        {
            if (permission.IsGranted == value)
            {
                return;
            }

            if (value)
            {
                _grantedPermissionCount++;
                _notGrantedPermissionCount--;
            }
            else
            {
                _grantedPermissionCount--;
                _notGrantedPermissionCount++;
            }

            permission.IsGranted = value;
        }

        private PermissionGrantInfoDto GetParentPermission(PermissionGroupDto permissionGroup, PermissionGrantInfoDto permission)
        {
            return permissionGroup.Permissions.First(x => x.Name == permission.ParentName);
        }

        private List<PermissionGrantInfoDto> GetChildPermissions(PermissionGroupDto permissionGroup, PermissionGrantInfoDto permission)
        {
            var childPermissions = new List<PermissionGrantInfoDto>();
            GetChildPermissions(childPermissions, permissionGroup.Permissions, permission);
            return childPermissions;
        }

        private void GetChildPermissions(List<PermissionGrantInfoDto> allChildPermissions, List<PermissionGrantInfoDto> permissions, PermissionGrantInfoDto permission)
        {
            var childPermissions = permissions.Where(x => x.ParentName == permission.Name).ToList();
            if (childPermissions.Count == 0)
            {
                return;
            }

            allChildPermissions.AddRange(childPermissions);

            foreach (var childPermission in childPermissions)
            {
                GetChildPermissions(allChildPermissions, permissions, childPermission);
            }
        }

        private bool IsDisabledPermission(PermissionGrantInfoDto permissionGrantInfo)
        {
            return _disabledPermissions.Any(x => x == permissionGrantInfo);
        }

        private string GetShownName(PermissionGrantInfoDto permissionGrantInfo)
        {
            if (permissionGrantInfo == null)
            {
                return string.Empty;
            }

            string displayName = permissionGrantInfo.DisplayName ?? string.Empty;

            if (displayName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
            {
                displayName = displayName["Permission:".Length..].Trim(); // Loại bỏ tiền tố và cắt khoảng trắng
            }

            if (!IsDisabledPermission(permissionGrantInfo))
            {
                return displayName;
            }

            var otherProviders = permissionGrantInfo.GrantedProviders?
                .Where(p => p.ProviderName != _providerName)
                .Select(p => p.ProviderName)
                .JoinAsString(", ") ?? string.Empty;

            return string.Format("{0} ({1})", displayName, otherProviders);
        }



        private bool IsPermissionGroupDisabled(PermissionGroupDto group)
        {
            var permissions = group.Permissions;
            var grantedProviders = permissions.SelectMany(x => x.GrantedProviders);

            return permissions.All(x => x.IsGranted) && grantedProviders.Any(p => p.ProviderName != _providerName);
        }

        #endregion
    }
}
