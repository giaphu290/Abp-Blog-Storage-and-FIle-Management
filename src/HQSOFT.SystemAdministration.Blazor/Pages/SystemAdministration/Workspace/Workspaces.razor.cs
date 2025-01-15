using HQSOFT.CoreBackend.Permissions;
using HQSOFT.Common.Blazor.Pages.Component;
using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using HQSOFT.CoreBackend.Workspaces;
using HQSOFT.CoreBackend.Reports;
using HQSOFT.CoreBackend.Modules;
using HQSOFT.CoreBackend.Screens;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Configuration;

using Blazorise;
using DevExpress.Blazor;

using Volo.Abp.AspNetCore.Components.Messages;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using Volo.Abp.Auditing;
using HQSOFT.CoreBackend.WorkspaceShortcuts;
using System.Linq;
using HQSOFT.CoreBackend.WorkspaceLinks;
using HQSOFT.CoreBackend.EnumList;
using DevExpress.Blazor.Internal;
using Volo.Abp.Application.Dtos;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http;
using System.Text.Json;
using HQSOFT.CoreBackend.WorkspaceRoles;
using HQSOFT.CoreBackend.WorkspaceUsers;
using HQSOFT.CoreBackend.ExtendedUsers;
using Volo.Abp.Identity;
using System.Text.RegularExpressions;
using Volo.Abp.Http.Client;
using HQSOFT.CoreBackend.Companies;


namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.Workspace
{
    public partial class Workspaces
    {
        //Standard code: Do not change

        #region

        protected List<Volo.Abp.BlazoriseUI.BreadcrumbItem> BreadcrumbItems = new List<Volo.Abp.BlazoriseUI.BreadcrumbItem>();
        protected PageToolbar Toolbar { get; } = new PageToolbar();

        private HQSOFTBreadcrumbScreen BreadcrumbScreen; 
        private bool IsSelected { get; set; } = false; 
        private bool IsDataEntryChanged { get; set; }
        private bool ShowInteractionForm { get; set; } = true;

        private EditForm EditFormMain { get; set; } = new EditForm();

        private HQSOFTFormActivity formActivity;

        [Parameter]
        public string Id { get; set; }
        private Guid EditingDocId { get; set; }

        private bool isValidated = false;
        private bool CanCreate { get; set; }
        private bool CanEdit { get; set; }
        private bool CanDelete { get; set; }

        private WorkspaceDto WorkspaceMenu { get; set; } = new WorkspaceDto(); 
        
        private readonly object lockObject = new object();

        #endregion


        //Custom code: add more code based on actual requirement

        #region 
        private bool CanCreateWorkspaceShortcut { get; set; }
        private bool CanEditWorkspaceShortcut { get; set; }
        private bool CanDeleteWorkspaceShortcut { get; set; }

        private bool CanCreateWorkspaceLink { get; set; }
        private bool CanEditWorkspaceLink { get; set; }
        private bool CanDeleteWorkspaceLink { get; set; }

        private bool CanCreateWorkspaceRole { get; set; }
        private bool CanEditWorkspaceRole { get; set; }
        private bool CanDeleteWorkspaceRole { get; set; }

        private bool CanCreateWorkspaceUser { get; set; }
        private bool CanEditWorkspaceUser { get; set; }
        private bool CanDeleteWorkspaceUser { get; set; }

        private bool IsEditEnabled { get; set; }

        private string? SelectedType { get; set; }

        private string SelectedIconCode;

        private WorkspaceUpdateDto EditingDoc { get; set; } //Current editting document 

        private IGrid GridWorkspaceShortcut { get; set; } //WorkspaceShortcut grid control name
        private IGrid GridWorkspaceLink { get; set; } //WorkspaceLink grid control name


        private List<FontAwesomeIcon> FontAwesomeIcons;
        private List<WorkspaceShortcutUpdateDto> WorkspaceShortcuts { get; set; } = new List<WorkspaceShortcutUpdateDto>(); //Data source used to bind to grid
        private List<WorkspaceLinkUpdateDto> WorkspaceLinks { get; set; } = new List<WorkspaceLinkUpdateDto>(); //Data source used to bind to grid


        private IReadOnlyList<object> SelectedWorkspaceShortcuts { get; set; } = new List<WorkspaceShortcutUpdateDto>(); //Selected rows on grid
        private IReadOnlyList<object> SelectedWorkspaceLinks { get; set; } = new List<WorkspaceLinkUpdateDto>(); //Selected rows on grid
        private IReadOnlyList<WorkspacesType> LinkTypeCollection { get; set; } = new List<WorkspacesType>();
        private IReadOnlyList<WorkspacesType> TypeCollection { get; set; } = new List<WorkspacesType>();
        private IReadOnlyList<ScreenDto> ScreensCollection { get; set; } = new List<ScreenDto>();
        private IReadOnlyList<ReportDto> ReportsCollection { get; set; } = new List<ReportDto>();

        private IReadOnlyList<ExtendedUserDto> UserCollection { get; set; } = new List<ExtendedUserDto>();
        private IReadOnlyList<ExtendedUserDto> UsersNotInWorkspaceUsers { get; set; } = new List<ExtendedUserDto>();
        private IReadOnlyList<IdentityRoleDto> RoleCollection { get; set; } = new List<IdentityRoleDto>();
        private IReadOnlyList<IdentityRoleDto> RolesNotInWorkspaceRoles { get; set; } = new List<IdentityRoleDto>();


        // WORKSPACE ROLES GRID 
        private IGrid GridWorkspaceRole { get; set; } // Company Role grid control name
        private List<WorkspaceRoleUpdateDto> WorkspaceRoles { get; set; } = new List<WorkspaceRoleUpdateDto>(); //Data source used to bind to grid
        private IReadOnlyList<object> SelectedWorkspaceRoles { get; set; } = new List<WorkspaceRoleUpdateDto>(); //Selected rows on grid


        // WORKSPACE USERS GRID 
        private bool IsWorkspaceUserEditorVisible { get; set; } = true;
        private IGrid GridWorkspaceUser { get; set; } //WorkspaceUser grid control name
        private List<WorkspaceUserUpdateDto> WorkspaceUsers { get; set; } = new List<WorkspaceUserUpdateDto>(); //Data source used to bind to grid
        private IReadOnlyList<object> SelectedWorkspaceUsers { get; set; } = new List<WorkspaceUserUpdateDto>(); //Selected rows on grid

        #endregion

        private static List<FontAwesomeIcon> _cachedIcons;




        /*===================================== Initialize Section ====================================*/

        #region 
        public Workspaces(IConfiguration configuration, IAuditingManager auditingManager)
        {
            EditingDoc = new WorkspaceUpdateDto
            {
                ConcurrencyStamp = string.Empty,
            };

            formActivity = new HQSOFTFormActivity(configuration, auditingManager); BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
        }

        protected override async Task OnInitializedAsync()
        {
            EditingDocId = Guid.Parse(Id);

            try
            {
                await LoadIconsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during initialization: {ex.Message}");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

                await JSRuntime.InvokeVoidAsync("FullScreen");
                await JSRuntime.InvokeVoidAsync("AssignGotFocus");
                await JSRuntime.InvokeVoidAsync("initializeDataChangeHandling");

                await BreadcrumbScreen.GetBreadcrumbsAsync();

                await FirstLoadAsync();

                ValidationFormHelper.MessageStore = null;
                await ValidationFormHelper.StartValidation();
                await ValidationFormHelper.Initialize(EditFormMain.EditContext!);

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



        /*============================= ToolBar - Breadcrumb - Permission ==============================*/

        #region
        private async Task SetPermissionsAsync()
        {
            CanCreate = await AuthorizationService
                 .IsGrantedAsync(CoreBackendPermissions.Workspaces.Create);
            CanEdit = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.Workspaces.Edit);
            CanDelete = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.Workspaces.Delete);

            //Create permission Properties for WorksapceShortcut and WorkspaceLink
            CanCreateWorkspaceShortcut = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.WorkspaceShortcuts.Create);
            CanEditWorkspaceShortcut = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.WorkspaceShortcuts.Edit);
            CanDeleteWorkspaceShortcut = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.WorkspaceShortcuts.Delete);

            CanCreateWorkspaceLink = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.WorkspaceLinks.Create);
            CanEditWorkspaceLink = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.WorkspaceLinks.Edit);
            CanDeleteWorkspaceLink = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.WorkspaceLinks.Delete);

            CanCreateWorkspaceRole = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.WorkspaceRoles.Create);
            CanEditWorkspaceRole = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.WorkspaceRoles.Edit);
            CanDeleteWorkspaceRole = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.WorkspaceRoles.Delete);

            CanCreateWorkspaceUser = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.WorkspaceUsers.Create);
            CanEditWorkspaceUser = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.WorkspaceUsers.Edit);
            CanDeleteWorkspaceUser = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.WorkspaceUsers.Delete);
        }
         
        protected virtual ValueTask SetToolbarItemsAsync()
        {
            Toolbar.Contributors.Clear();

            Toolbar.AddButton(L["Back"], async () =>
            {
                NavigationManager.NavigateTo($"/SystemAdministration/Workspaces");
                await Task.CompletedTask;
            },
            IconName.Undo,
            Color.Light);
            // động từ hoá
            var parmAction = new Dictionary<string, object>()
            {
                    {"Id", EditingDocId },
                    {"RefreshAsync", EventCallback.Factory.Create(this, RefreshAsync) },
                    {"CreateNewAsync", EventCallback.Factory.Create(this, CreateNewAsync)},
                    {"DeleteAsync", EventCallback.Factory.Create(this, DeleteAsync)},
                    {"DuplicateAsync", EventCallback.Factory.Create(this, DuplicateAsync)},
                    {"CanEdit", CanEdit },
                    {"CanCreate", CanCreate },
                    {"CanDelete", CanDelete },
                    {"CanSuggest", false },
                };
            Toolbar.AddComponent<NewEditAction>(parmAction);

            Toolbar.AddButton(
                    L["Save"],
                    async () => await SaveDataAsync(false),
                    IconName.Save,
                    Color.Primary,
                    requiredPolicyName: EditingDocId != Guid.Empty
                        ? CoreBackendPermissions.Workspaces.Edit
                        : CoreBackendPermissions.Workspaces.Create,
                    disabled: EditingDocId != Guid.Empty
                        ? !CanEdit
                        : !CanCreate
            );

            var parmAction2 = new Dictionary<string, object>()
                {
                    {"ToggleInteractionFormAsync", EventCallback.Factory.Create(this, ToggleInteractionFormAsync) }
                };

            if (EditingDocId != Guid.Empty)
                Toolbar.AddComponent<ToggleSidebar>(parmAction2);

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



        /*================================= Load Data Source for ListView =============================*/

        #region Load Data Source for ListView & Workspaces

        private async Task FirstLoadAsync()
        {
            WorkspaceMenu = await WorkspacesAppService.GetWorkspaceByCodeAsync("SA");
            await SetPermissionsAsync();
            await SetToolbarItemsAsync();
            

            await GetCollectionDataAsync();
            await LoadDataAsync(EditingDocId);
            await HandleHistoryAdded();
            await FilterRolesAndUsersAsync(true, true, true);
            await InvokeAsync(StateHasChanged);
        }

        private async Task GetCollectionDataAsync()
        {
            TypeCollection = CommonHelper.GetEnumLookupAsync<ShortcutType, WorkspacesType>("ShortcutType", L);
            LinkTypeCollection = CommonHelper.GetEnumLookupAsync<LinkType, WorkspacesType>("LinkType", L);

            ScreensCollection = await ScreensAppService.GetListNoPagedAsync(new GetScreensInput { });
            ReportsCollection = await ReportsAppService.GetListNoPagedAsync(new GetReportsInput { });
            UserCollection = await ExtendedUsersAppService.GetListNoPagedAsync(new GetExtendedUsersInput { });

            var roleCollection = await IdentityRoleAppService.GetListAsync(new GetIdentityRoleListInput { });
            RoleCollection = roleCollection.Items.ToList();
        }

        public async Task LoadIconsAsync()
        {
            if (_cachedIcons == null)
            {
                var jsonData = await LoadIconDataAsync();
                _cachedIcons = ParseIconData(jsonData);
            }

            FontAwesomeIcons = _cachedIcons; // Sử dụng dữ liệu từ cache
        }

        private async Task<string> LoadIconDataAsync() // Tải dữ liệu JSON Font-Awesome
        {
            try
            {
                string url = "https://raw.githubusercontent.com/FortAwesome/Font-Awesome/5.13.0/metadata/icons.json";
                using var response = await HttpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion



        /*========================== CRUD & Load Main Data Source Section =============================*/

        #region CRUD & Load Main Data

        private async Task LoadDataAsync(Guid classId)
        {
            await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

            if (classId != Guid.Empty)
            {
                var workspace = await WorkspacesAppService.GetAsync(classId);
                EditingDoc = ObjectMapper.Map<WorkspaceDto, WorkspaceUpdateDto>(workspace);

                var workspaceShortcuts = await WorkspaceShortcutsAppService.GetListByWorkspaceIdAsync(classId);
                WorkspaceShortcuts = ObjectMapper.Map<List<WorkspaceShortcutDto>, List<WorkspaceShortcutUpdateDto>>(workspaceShortcuts);

                var workspaceLinks = await WorkspaceLinksAppService.GetListByWorkspaceIdAsync(classId);
                WorkspaceLinks = ObjectMapper.Map<List<WorkspaceLinkDto>, List<WorkspaceLinkUpdateDto>>(workspaceLinks);

                await HandleCommentAdded();
                await HandleHistoryAdded();
            }
            else
            {
                EditingDoc = new WorkspaceUpdateDto();
                WorkspaceShortcuts = new List<WorkspaceShortcutUpdateDto>();
                WorkspaceLinks = new List<WorkspaceLinkUpdateDto>();
                WorkspaceRoles = new List<WorkspaceRoleUpdateDto>();
                WorkspaceUsers = new List<WorkspaceUserUpdateDto>();
            }


            IsEditEnabled = EditingDocId == Guid.Empty;
            IsDataEntryChanged = false;

            ValidationFormHelper.MessageStore = null;
            await ValidationFormHelper.StartValidation();
            await ValidationFormHelper.Initialize(EditFormMain.EditContext!);

            await ResetToolbarAsync();
            await UpdateDataChangeStatus(false);
            await BlockUiService.UnBlock();
            await InvokeAsync(StateHasChanged);
        }

        private async Task SaveGridDataAsync(dynamic grid)
        {
            if (grid != null)
            {
                await grid.SaveChangesAsync();
            }
        }

        private async Task SaveDataAsync(bool IsNewNext)
        {
            try
            {
                if (IsNewNext)
                {
                    await NewDataAsync();
                    await HandleHistoryAdded();
                    return;
                }
                else
                {
                    await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

                    bool isValid = false, isHandled = false;

                    await InvokeAsync(async () =>
                    {
                        isValid = EditFormMain.EditContext.Validate();
                        isHandled = await HandleValidSubmit();
                    });

                    if (!isValid || !isHandled)
                    {
                        await BlockUiService.UnBlock();
                        return;
                    }

                    if (IsDataEntryChanged)
                    {
                        await SaveGridDataAsync(GridWorkspaceRole);
                        await SaveGridDataAsync(GridWorkspaceUser);
                        await SaveGridDataAsync(GridWorkspaceShortcut);
                        await SaveGridDataAsync(GridWorkspaceLink);

                        if (EditingDocId == Guid.Empty)
                        {
                            // Create
                            var workspace = await WorkspacesAppService.CreateAsync(ObjectMapper.Map<WorkspaceUpdateDto, WorkspaceCreateDto>(EditingDoc));
                            EditingDoc = ObjectMapper.Map<WorkspaceDto, WorkspaceUpdateDto>(workspace);
                            EditingDocId = EditingDoc.Id;

                            await SaveWorkspaceRoleAsync();
                            await SaveWorkspaceUserAsync();
                            await SaveWorkspaceLinkAsync();
                            await SaveWorkspaceShortcutAsync();

                            IsDataEntryChanged = false;
                            await ResetToolbarAsync();
                            await HandleHistoryAdded();
                            await UiNotificationService.Success(L["Notification:Save"]);
                            await LoadDataAsync(EditingDocId);
                        }
                        else
                        {
                            // Update
                            await WorkspacesAppService.UpdateAsync(EditingDocId, EditingDoc);
                            EditingDoc = ObjectMapper.Map<WorkspaceDto, WorkspaceUpdateDto>(await WorkspacesAppService.GetAsync(EditingDocId));

                            await SaveWorkspaceRoleAsync();
                            await SaveWorkspaceUserAsync();
                            await SaveWorkspaceLinkAsync();
                            await SaveWorkspaceShortcutAsync();
                            IsDataEntryChanged = false;
                            await HandleHistoryAdded();
                            await UiNotificationService.Success(L["Notification:Edit"]);
                        }

                        await UpdateDataChangeStatus(false);
                    }
                    else
                    {
                        if (EditingDocId != Guid.Empty)
                            await UiNotificationService.Warn(L["Notification:NoChangesInDocument"]);
                    }
                }

                await InvokeAsync(async () =>
                {
                    IsDataEntryChanged = false;
                    await UpdateDataChangeStatus(false);
                    NavigationManager.NavigateTo($"/SystemAdministration/Workspaces/{EditingDocId}");
                });

                await ResetToolbarAsync();
                await BlockUiService.UnBlock();
            }
            catch (AbpRemoteCallException ex)
            {
                await UiMessageService.Warn(ex.Message);
                await BlockUiService.UnBlock();
            }
            catch (Exception ex)
            {
                await BlockUiService.UnBlock();
            }
        }

        private async Task<bool> SaveWorkspaceShortcutAsync()
        {
            try
            {
                foreach (var workspaceShortcut in WorkspaceShortcuts.Where(x => x.IsChanged == true))
                {
                    if (workspaceShortcut.WorkspaceId == Guid.Empty)
                        workspaceShortcut.WorkspaceId = EditingDocId;

                    if (workspaceShortcut.ConcurrencyStamp == string.Empty && EditingDocId != Guid.Empty)
                        await WorkspaceShortcutsAppService.CreateAsync(ObjectMapper.Map<WorkspaceShortcutUpdateDto, WorkspaceShortcutCreateDto>(workspaceShortcut));
                    else
                        await WorkspaceShortcutsAppService.UpdateAsync(workspaceShortcut.Id, ObjectMapper.Map<WorkspaceShortcutUpdateDto, WorkspaceShortcutUpdateDto>(workspaceShortcut));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SaveWorkspaceLinkAsync()
        {
            try
            {
                foreach (var workspaceLink in WorkspaceLinks.Where(x => x.IsChanged == true))
                {
                    if (workspaceLink.WorkspaceId == Guid.Empty)
                        workspaceLink.WorkspaceId = EditingDocId;

                    if (workspaceLink.ConcurrencyStamp == string.Empty && EditingDocId != Guid.Empty)
                        await WorkspaceLinksAppService.CreateAsync(ObjectMapper.Map<WorkspaceLinkUpdateDto, WorkspaceLinkCreateDto>(workspaceLink));
                    else
                        await WorkspaceLinksAppService.UpdateAsync(workspaceLink.Id, ObjectMapper.Map<WorkspaceLinkUpdateDto, WorkspaceLinkUpdateDto>(workspaceLink));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task DeleteAsync()
        {
            var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
            if (confirmed)
            {
                try
                {
                    await WorkspacesAppService.DeleteAsync(EditingDocId);
                    await InvokeAsync(async () =>
                    {
                        NavigationManager.NavigateTo("/SystemAdministration/Workspaces");
                        await UiNotificationService.Error(L["Notification:Delete"]);
                    });

                    IsDataEntryChanged = false;
                    await ResetToolbarAsync();
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
        }

        private async Task DeleteWorkspaceShortcutAsync()
        {
            var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
            if (confirmed)
            {
                if (SelectedWorkspaceShortcuts != null)
                {
                    foreach (WorkspaceShortcutUpdateDto row in SelectedWorkspaceShortcuts)
                    {
                        await WorkspaceShortcutsAppService.DeleteAsync(row.Id);
                        WorkspaceShortcuts.Remove(row);
                    }
                }
                GridWorkspaceShortcut.Reload();
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task DeleteWorkspaceLinkAsync()
        {
            var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
            if (confirmed)
            {
                if (SelectedWorkspaceLinks != null)
                {
                    foreach (WorkspaceLinkUpdateDto row in SelectedWorkspaceLinks)
                    {
                        await WorkspaceLinksAppService.DeleteAsync(row.Id);
                        WorkspaceLinks.Remove(row);
                    }
                }
                GridWorkspaceLink.Reload();
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task RefreshAsync()
        {
            bool checkSaving = await SavingConfirmAsync();
            if (checkSaving)
            {
                await LoadDataAsync(EditingDocId);
            }
        }

        private async Task CreateNewAsync()
        {
            bool checkSaving = await SavingConfirmAsync();
            if (checkSaving)
            {
                IsDataEntryChanged = false;
                await SaveDataAsync(true);
                await ResetToolbarAsync();
            }
            await InvokeAsync(StateHasChanged);
        }

        private async Task NewDataAsync()
        {
            EditingDoc = new WorkspaceUpdateDto
            {
                ConcurrencyStamp = string.Empty,
            };

            EditingDocId = Guid.Empty;
            IsDataEntryChanged = false;

            await InvokeAsync(async () =>
            {
                await UpdateDataChangeStatus(false);
                NavigationManager.NavigateTo($"/SystemAdministration/Workspaces/{Guid.Empty}");
            });

            await LoadDataAsync(EditingDocId);
        }

        private async Task DuplicateAsync()
        {
            if (EditingDocId != Guid.Empty)
            {
                bool checkSaving = await SavingConfirmAsync();
                if (checkSaving)
                {
                    EditingDoc = new WorkspaceUpdateDto
                    {
                        ConcurrencyStamp = string.Empty,
                    };

                    foreach (var item in WorkspaceLinks)
                    {
                        item.Id = Guid.Empty;
                        item.WorkspaceId = Guid.Empty;
                        item.IsChanged = true;
                        item.ConcurrencyStamp = string.Empty;
                    }

                    foreach (var item in WorkspaceShortcuts)
                    {
                        item.Id = Guid.Empty;
                        item.WorkspaceId = Guid.Empty;
                        item.IsChanged = true;
                        item.ConcurrencyStamp = string.Empty;
                    }

                    foreach (var item in WorkspaceUsers)
                    {
                        item.Id = Guid.Empty;
                        item.WorkspaceId = Guid.Empty;
                        item.IsChanged = true;
                        item.ConcurrencyStamp = string.Empty;
                    }

                    foreach (var item in WorkspaceRoles)
                    {
                        item.Id = Guid.Empty;
                        item.WorkspaceId = Guid.Empty;
                        item.IsChanged = true;
                        item.ConcurrencyStamp = string.Empty;
                    }

                    EditingDoc = ObjectMapper.Map<WorkspaceDto, WorkspaceUpdateDto>(await WorkspacesAppService.GetAsync(EditingDocId));
                    EditingDoc.Code = string.Empty;
                    EditingDocId = Guid.Empty;

                    await InvokeAsync(async () =>
                    {
                        IsDataEntryChanged = false;
                        await UpdateDataChangeStatus(false);
                        NavigationManager.NavigateTo($"/SystemAdministration/Workspaces/{Guid.Empty}");
                    });

                    IsEditEnabled = true;
                    IsDataEntryChanged = true;

                    ValidationFormHelper.MessageStore = null;
                    await ValidationFormHelper.StartValidation();
                    await ValidationFormHelper.Initialize(EditFormMain.EditContext!);
                }
            }
            else
            {
                await UiMessageService.Warn(L["Message:CannotDuplicate"]);
            }
            await ResetToolbarAsync();
        }


        //GRID
        private async Task FilterRolesAndUsersAsync(bool IsGetFilter, bool IsRoles, bool IsUsers)
        {
            if (IsGetFilter && IsRoles)
            {
                await GetWorkspaceRoleListAsync();
            }

            if (IsGetFilter && IsUsers)
            {
                await GetWorkspaceUserListAsync();
            }

            if (IsRoles)
            {
                // Lấy danh sách các Role không có trong WorkspaceRoles
                var workspaceRoleIds = WorkspaceRoles.Select(cr => cr.RoleId).ToList();
                RolesNotInWorkspaceRoles = RoleCollection.Where(role => !workspaceRoleIds.Contains(role.Id)).ToList();
            }

            if (IsUsers)
            {
                // Lấy danh sách các User không có trong WorkspaceUsers
                var workspaceUserIds = WorkspaceUsers.Select(uc => uc.UserId).ToList();
                UsersNotInWorkspaceUsers = UserCollection.Where(user => !workspaceUserIds.Contains(user.Id)).ToList();
            }
            await Task.CompletedTask;
        }

        #endregion



        #region Grid WorkspaceUser

        private async Task GridWorkspaceUserNew_Click()
        {
            IsWorkspaceUserEditorVisible = true;
            await GridWorkspaceUser.SaveChangesAsync();
            GridWorkspaceUser.ClearSelection();
            await GridWorkspaceUser.StartEditNewRowAsync();
        }

        private async Task GetWorkspaceUserListAsync()
        {
            var workspaceUsers = await WorkspaceUsersAppService.GetListByWorkspaceIdAsync(EditingDocId);
            WorkspaceUsers = ObjectMapper.Map<List<WorkspaceUserDto>, List<WorkspaceUserUpdateDto>>(workspaceUsers);
        }

        private async Task DeleteWorkspaceUserAsync()
        {
            var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
            if (confirmed)
            {
                if (SelectedWorkspaceUsers != null)
                {
                    foreach (WorkspaceUserUpdateDto row in SelectedWorkspaceUsers)
                    {
                        await WorkspaceUsersAppService.DeleteAsync(row.Id);
                        WorkspaceUsers.Remove(row);
                    }
                }

                await FilterRolesAndUsersAsync(false, false, true);
                // Đảm bảo GridWorkspaceUser.Reload() được gọi trên luồng UI
                await InvokeAsync(async () => GridWorkspaceUser.Reload());
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task GridWorkspaceUserDelete_Click()
        {
            await DeleteWorkspaceUserAsync();
        }

        private async Task GridWorkspaceUser_FocusedRowChanged(GridFocusedRowChangedEventArgs e)
        {
            IsWorkspaceUserEditorVisible = false;
            await InvokeAsync(StateHasChanged);
        }

        private async Task GridWorkspaceUser_EditModelSaving(GridEditModelSavingEventArgs e)
        {
            WorkspaceUserUpdateDto editModel = (WorkspaceUserUpdateDto)e.EditModel;
            //Assign changes from the edit model to the data item. 
            if (editModel != null && e.IsNew)
            {
                editModel.IsChanged = true;
                IsDataEntryChanged = true;
                if (editModel.UserId != Guid.Empty)
                    WorkspaceUsers.Add(editModel);
            }
            IsWorkspaceUserEditorVisible = false;

            await FilterRolesAndUsersAsync(false, false, true);
            await UpdateDataChangeStatus(true);
        }

        private void GridWorkspaceUser_OnCustomizeEditModel(GridCustomizeEditModelEventArgs e)
        {
            if (e.IsNew)
            {
                var newRow = (WorkspaceUserUpdateDto)e.EditModel;
                newRow.Id = Guid.Empty;
                newRow.WorkspaceId = EditingDocId;
                newRow.ConcurrencyStamp = string.Empty;
                newRow.IsChanged = true;
            }
        }
        private void GridWorkspaceUser_CustomizeDataRowEditor(GridCustomizeDataRowEditorEventArgs e)
        {
            if (e.EditSettings is ITextEditSettings settings)
                settings.ShowValidationIcon = true;
        }

        private async Task SaveWorkspaceUserAsync()
        {
            try
            {
                foreach (var workspaceUser in WorkspaceUsers.Where(x => x.IsChanged == true))
                {
                    if (workspaceUser.WorkspaceId == Guid.Empty)
                        workspaceUser.WorkspaceId = EditingDocId;

                    if (workspaceUser.ConcurrencyStamp == string.Empty && EditingDocId != Guid.Empty)
                        await WorkspaceUsersAppService.CreateAsync(ObjectMapper.Map<WorkspaceUserUpdateDto, WorkspaceUserCreateDto>(workspaceUser));
                }
                await FilterRolesAndUsersAsync(true, false, true);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
            }
        }

        #endregion GridWorkspaceUser



        #region Grid WorkspaceRole

        private async Task SaveWorkspaceRoleAsync()
        {
            try
            {
                foreach (var workspaceRole in WorkspaceRoles.Where(x => x.IsChanged == true))
                {
                    if (workspaceRole.WorkspaceId == Guid.Empty)
                        workspaceRole.WorkspaceId = EditingDocId;

                    if (workspaceRole.ConcurrencyStamp == string.Empty && EditingDocId != Guid.Empty)
                        await WorkspaceRolesAppService.CreateAsync(ObjectMapper.Map<WorkspaceRoleUpdateDto, WorkspaceRoleCreateDto>(workspaceRole));
                }
                await FilterRolesAndUsersAsync(true, true, false);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
            }
        }

        private async Task GridWorkspaceRoleNew_Click()
        {
            await GridWorkspaceRole.SaveChangesAsync();
            GridWorkspaceRole.ClearSelection();
            await GridWorkspaceRole.StartEditNewRowAsync();
        }

        private async Task GetWorkspaceRoleListAsync()
        {
            var workspaceRoles = await WorkspaceRolesAppService.GetListByWorkspaceIdAsync(EditingDocId);
            WorkspaceRoles = ObjectMapper.Map<List<WorkspaceRoleDto>, List<WorkspaceRoleUpdateDto>>(workspaceRoles);
        }

        private async Task DeleteWorkspaceRoleAsync()
        {
            var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
            if (confirmed)
            {
                if (SelectedWorkspaceRoles != null)
                {
                    foreach (WorkspaceRoleUpdateDto row in SelectedWorkspaceRoles)
                    {
                        await WorkspaceRolesAppService.DeleteAsync(row.Id);
                        WorkspaceRoles.Remove(row);
                    }
                }

                await FilterRolesAndUsersAsync(false, true, false);
                // Đảm bảo GridWorkspaceUser.Reload() được gọi trên luồng UI
                await InvokeAsync(async () => GridWorkspaceUser.Reload());
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task GridWorkspaceRoleDelete_Click()
        {
            await DeleteWorkspaceRoleAsync();
        }

        private async Task GridWorkspaceRole_FocusedRowChanged(GridFocusedRowChangedEventArgs e)
        {
            await GridWorkspaceRole.SaveChangesAsync();
            GridWorkspaceRole.ClearSelection();
        }

        private async Task GridWorkspaceRole_EditModelSaving(GridEditModelSavingEventArgs e)
        {
            WorkspaceRoleUpdateDto editModel = (WorkspaceRoleUpdateDto)e.EditModel;
            //Assign changes from the edit model to the data item. 
            if (editModel != null && e.IsNew)
            {
                editModel.IsChanged = true;
                IsDataEntryChanged = true;
                if (editModel.RoleId != Guid.Empty)
                    WorkspaceRoles.Add(editModel);
            }
            await FilterRolesAndUsersAsync(false, true, false);
            await UpdateDataChangeStatus(true);
        }
        private void GridWorkspaceRole_OnCustomizeEditModel(GridCustomizeEditModelEventArgs e)
        {
            if (e.IsNew)
            {
                var newRow = (WorkspaceRoleUpdateDto)e.EditModel;
                newRow.Id = Guid.Empty;
                newRow.WorkspaceId = EditingDocId;
                newRow.ConcurrencyStamp = string.Empty;
                newRow.IsChanged = true;
            }
        }

        private void GridWorkspaceRole_CustomizeDataRowEditor(GridCustomizeDataRowEditorEventArgs e)
        {
            if (e.EditSettings is ITextEditSettings settings)
                settings.ShowValidationIcon = true;
        }
        #endregion GridWorkspaceRole



        #region GridWorkspaceShortcut
        private async Task GridWorkspaceShortcutNew_Click()
        {
            await GridWorkspaceShortcut.SaveChangesAsync();
            GridWorkspaceShortcut.ClearSelection();
            await GridWorkspaceShortcut.StartEditNewRowAsync();
        }

        private async Task GridWorkspaceShortcutDelete_Click()
        {
            await DeleteWorkspaceShortcutAsync();
        }

        private async Task GridWorkspaceShortcut_EditModelSaving(GridEditModelSavingEventArgs e)
        {
            WorkspaceShortcutUpdateDto editModel = (WorkspaceShortcutUpdateDto)e.EditModel;
            //Assign changes from the edit model to the data item.
            if (editModel != null && !e.IsNew)
            {
                editModel.IsChanged = true;
                IsDataEntryChanged = true;
                //Find the position of dataItem in WokspaceShortcuts then update that element by editModel
                int index = WorkspaceShortcuts.FindIndex(item => item.Idx == editModel.Idx);
                if (index != -1)
                {
                    WorkspaceShortcuts[index] = editModel;
                }
            }
            if (editModel != null && e.IsNew)
            {
                editModel.IsChanged = true;
                IsDataEntryChanged = true;
                WorkspaceShortcuts.Add(editModel);
            }
            await UpdateDataChangeStatus(true);
        }

        private void GridWorkspaceShortcut_OnCustomizeEditModel(GridCustomizeEditModelEventArgs e)
        {
            if (e.IsNew)
            {
                var newRow = (WorkspaceShortcutUpdateDto)e.EditModel;
                newRow.Id = Guid.Empty;
                if (GridWorkspaceShortcut.GetVisibleRowCount() > 0)
                {
                    int maxIdx = WorkspaceShortcuts.Max(x => x.Idx);
                    newRow.Idx = maxIdx + 1;
                }
                else
                    newRow.Idx = 1;

                newRow.WorkspaceId = EditingDocId;
                newRow.Type = "Screen";
                newRow.ConcurrencyStamp = string.Empty;
                newRow.IsChanged = true;
            }
        }

        private void GridWorkspaceShortcut_CustomizeDataRowEditor(GridCustomizeDataRowEditorEventArgs e)
        {
            if (e.EditSettings is ITextEditSettings settings)
                settings.ShowValidationIcon = true;
        }
        #endregion GridWorkspaceShortcut



        #region GridWorkspaceLink
        private async Task GridWorkspaceLinkNew_Click()
        {
            await GridWorkspaceLink.SaveChangesAsync();
            GridWorkspaceLink.ClearSelection();
            await GridWorkspaceLink.StartEditNewRowAsync();
        }

        private async Task GridWorkspaceLinkDelete_Click()
        {
            await DeleteWorkspaceLinkAsync();
        }

        private async Task GridWorkspaceLink_EditModelSaving(GridEditModelSavingEventArgs e)
        {
            WorkspaceLinkUpdateDto editModel = (WorkspaceLinkUpdateDto)e.EditModel;
            //Assign changes from the edit model to the data item.
            if (editModel != null && !e.IsNew)
            {
                editModel.IsChanged = true;
                IsDataEntryChanged = true;
                int index = WorkspaceLinks.FindIndex(item => item.Idx == editModel.Idx);
                if (index != -1)
                {
                    WorkspaceLinks[index] = editModel;
                }
            }
            if (editModel != null && e.IsNew)
            {
                editModel.IsChanged = true;
                IsDataEntryChanged = true;
                WorkspaceLinks.Add(editModel);
            }
            await UpdateDataChangeStatus(true);
        }

        private void GridWorkspaceLink_OnCustomizeEditModel(GridCustomizeEditModelEventArgs e)
        {
            if (e.IsNew)
            {
                var newRow = (WorkspaceLinkUpdateDto)e.EditModel;
                newRow.Id = Guid.Empty;
                if (GridWorkspaceLink.GetVisibleRowCount() > 0)
                {
                    int maxIdx = WorkspaceLinks.Max(x => x.Idx);
                    newRow.Idx = maxIdx + 1;
                }
                else
                    newRow.Idx = 1;

                newRow.WorkspaceId = EditingDocId;
                newRow.Type = "Screen";
                newRow.ConcurrencyStamp = string.Empty;
                newRow.IsChanged = true;
            }
        }

        private void GridWorkspaceLink_CustomizeDataRowEditor(GridCustomizeDataRowEditorEventArgs e)
        {
            if (e.EditSettings is ITextEditSettings settings)
                settings.ShowValidationIcon = true;
        }
        #endregion GridWorkspaceLink




        /*========================= Interaction/History/Comment Form Handling =========================*/

        #region Interaction/History/Comment Form Handling

        private async Task HandleCommentAdded()
        {
            if (EditingDocId != Guid.Empty && formActivity != null)
                await formActivity.GetCommentListAsync();
        }
        private async Task HandleHistoryAdded()
        {
            await InvokeAsync(StateHasChanged);
            if (EditingDocId != Guid.Empty && formActivity != null)
                await formActivity.GetEntityChangeAsync();
        }

        private async Task ToggleInteractionFormAsync()
        {
            ShowInteractionForm = !ShowInteractionForm;
            ChangeIconToggleSidebar.IsChanged = ShowInteractionForm;
            await InvokeAsync(StateHasChanged);
        }

        #endregion



        /*=================================== Validation, Checking ====================================*/

        #region Validation, Checking
        private async Task<bool> SavingConfirmAsync()
        {
            if (IsDataEntryChanged)
            {
                var confirmed = await UiMessageService.Confirm(L["SavingConfirmationMessage"]);
                if (confirmed)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        private async Task OnBeforeInternalNavigation(LocationChangingContext context)
        {
            bool checkSaving = await SavingConfirmAsync();
            if (!checkSaving)
            {
                context.PreventNavigation();
                await UpdateDataChangeStatus(true);
            }
            else
            {
                await UpdateDataChangeStatus(false);
            }
        }
        #endregion



        /*================================== Controls triggers/events =================================*/

        #region Controls triggers/events 
        private async Task UpdateDataChangeStatus(bool isChanged)
        {
            if (isChanged)
            {
                if (IsDataEntryChanged)
                    await JSRuntime.InvokeVoidAsync("MarkDataChanged");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("ResetChangeFlag");
            }
        }

        private async Task<bool> HandleValidSubmit()
        {
            bool isDuplicateCode = (await WorkspacesAppService.GetExistingDataByField(nameof(EditingDoc.Code), EditingDoc.Code, EditingDocId)).Any();

            await ValidationFormHelper.StartValidation();
            await ValidationFormHelper.Initialize(EditFormMain.EditContext!);

            await ValidateField(nameof(EditingDoc.Code), isDuplicateCode);

            return await ValidationFormHelper.IsValid();
        }

        private async Task ValidateField(string fieldName, bool isDuplicate)
        {
            if (isDuplicate)
            {
                await ValidationFormHelper.ValidateFieldUnique(fieldName, isDuplicate, EditingDoc, L);
            }
            else
            {
                await ValidationFormHelper.ClearMessagesByFieldName(fieldName, EditingDoc);
            }
        }

        private List<FontAwesomeIcon> ParseIconData(string jsonData) // Chuyển đổi JSON thành danh sách icon
        {
            var icons = new List<FontAwesomeIcon>();
            using var jsonDocument = JsonDocument.Parse(jsonData);
            var iconData = jsonDocument.RootElement.EnumerateObject();

            foreach (var icon in iconData)
            {
                var code = icon.Value.GetProperty("styles").EnumerateArray()
                    .Any(style => style.GetString() == "brands")
                    ? $"fa-brands fa-{icon.Name}"
                    : $"fa-solid fa-{icon.Name}";

                icons.Add(new FontAwesomeIcon
                {
                    Code = code,
                    Name = icon.Name
                });
            }
            return icons;
        }

        private async void HandleCtrlS(KeyboardEventArgs e)
        {
            await SaveDataAsync(false);
            await ResetToolbarAsync();
        }

        private async void HandleCtrlB(KeyboardEventArgs e)
        {
            await NewDataAsync();
            await ResetToolbarAsync();
        }


        #endregion



        /*=============================== Additional Application Functions ============================*/

        #region Additional Application Functions

        #endregion


    }
}

public class FontAwesomeIcon
{
    public string Code { get; set; }  // Mã biểu tượng Font Awesome, ví dụ: "fas fa-home"
    public string Name { get; set; }  // Tên biểu tượng, ví dụ: "Home"
}