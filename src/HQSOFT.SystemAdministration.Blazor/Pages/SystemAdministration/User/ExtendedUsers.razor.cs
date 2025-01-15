using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Blazorise;

using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using Volo.Abp.AspNetCore.Components.Messages;
using Volo.Abp.BlazoriseUI.Components;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;

using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using HQSOFT.Common.Blazor.Pages.Component;
using HQSOFT.CoreBackend.Permissions;
using HQSOFT.CoreBackend.Shared;
using HQSOFT.CoreBackend.ExtendedUsers;
using HQSOFT.CoreBackend.EnumList;
using HQSOFT.CoreBackend.RouteTypes;
using HQSOFT.CoreBackend.SubRoutes;
using HQSOFT.CoreBackend.States;
using HQSOFT.CoreBackend.Provinces;
using HQSOFT.CoreBackend.Territories;
using HQSOFT.CoreBackend.Companies;
using HQSOFT.CoreBackend.Screens;
using Volo.Abp.Http.Client;
using HQSOFT.CoreBackend.Modules;
using HQSOFT.CoreBackend.Workspaces;


namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.User
{
    public partial class ExtendedUsers
    {
        //Standard code: Do not change
        #region

        protected List<Volo.Abp.BlazoriseUI.BreadcrumbItem> BreadcrumbItems = new List<Volo.Abp.BlazoriseUI.BreadcrumbItem>();
        protected PageToolbar Toolbar { get; } = new PageToolbar();
        private int PageSize { get; set; } = 20;
        private int CurrentPage { get; set; } = 1;
        private int MaxCount { get; } = 1000;
        private string CurrentSorting { get; set; } = string.Empty;

        [Parameter]
        public string Id { get; set; }
        private Guid EditingDocId { get; set; }

        private HQSOFTBreadcrumbScreen BreadcrumbScreen; private bool IsSelected { get; set; } = false; 
        private bool IsDataEntryChanged { get; set; }
        private bool ShowInteractionForm { get; set; } = true;

        private EditForm EditFormMain { get; set; } = new EditForm();

        private HQSOFTFormActivity formActivity;

        private bool isValidated = false;
        private bool CanCreate { get; set; }
        private bool CanEdit { get; set; }
        private bool CanDelete { get; set; }

        private WorkspaceDto WorkspaceMenu { get; set; } = new WorkspaceDto(); private readonly object lockObject = new object();

        private bool isToolbarUpdating = false; // Biến để theo dõi trạng thái Toolbar

        #endregion


        //Custom code: add more code based on actual requirement
        #region 

        private bool IsEditEnabled { get; set; }

        private GetExtendedUsersInput Filter { get; set; }
        private ExtendedUserUpdateDto EditingDoc { get; set; }

        private List<RouteTypeDto> RouteTypeList { get; set; } = new List<RouteTypeDto>();

        private IReadOnlyList<IdentityRoleDto> PositionCollection { get; set; } = new List<IdentityRoleDto>();
        private IReadOnlyList<StateDto> StateCollection { get; set; } = new List<StateDto>();
        private IReadOnlyList<ProvinceDto> ProvinceCollection { get; set; } = new List<ProvinceDto>();
        private IReadOnlyList<TerritoryDto> TerritoryCollection { get; set; } = new List<TerritoryDto>();
        private IReadOnlyList<CompanyDto> CompanyCollection { get; set; } = new List<CompanyDto>();
        private IReadOnlyList<SalesFreqTypeList> SalesFreqTypeCollection { get; set; } = new List<SalesFreqTypeList>();
        private IReadOnlyList<WeekOfVisitTypeList> WeekOfVisitTypeCollection { get; set; } = new List<WeekOfVisitTypeList>();


        private string PasswordErrorMessage;

        private bool IsVisibleExcel { get; set; } = true;
        private bool ShowPassword { get; set; }
        private bool IsVisiblePassword { get; set; }

        private List<ExtendedUserUpdateDto> ExtendedUserList { get; set; } = new List<ExtendedUserUpdateDto>(); //Data source used to bind to grid 

        #endregion



        /*===================================== Initialize Section ====================================*/

        #region
        public ExtendedUsers()
        {
            Filter = new GetExtendedUsersInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting
            };

            EditingDoc = new ExtendedUserUpdateDto();
            EditingDoc.ConcurrencyStamp = string.Empty;
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

        protected override async Task OnInitializedAsync()
        {
            EditingDocId = Guid.Parse(Id);
            await Task.CompletedTask;
        }

        #endregion



        /*============================= ToolBar - Breadcrumb - Permission ==============================*/

        #region 
        protected virtual ValueTask SetToolbarItemsAsync()
        {
            Toolbar.Contributors.Clear();

            Toolbar.AddButton(L["Back"], async () =>
            {
                NavigationManager.NavigateTo($"/SystemAdministration/Users");
                await Task.CompletedTask;
            },
            IconName.Undo,
            Color.Light);

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
                        ? CoreBackendPermissions.ExtendedUsers.Edit
                        : CoreBackendPermissions.ExtendedUsers.Create,
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

        private async Task SetPermissionsAsync()
        {
            CanCreate = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.ExtendedUsers.Create);
            CanEdit = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.ExtendedUsers.Edit);
            CanDelete = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.ExtendedUsers.Delete);
        }

        #endregion



        /*================================= Load Data Source for ListView =============================*/

        #region --Load Data Source for ListView & Users
        private async Task LoadDataAsync(Guid classId)
        {
            await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

            if (classId != Guid.Empty)
            {
                EditingDoc = ObjectMapper.Map<ExtendedUserDto, ExtendedUserUpdateDto>(await ExtendedUsersAppService.GetAsync(classId));
                await GetExtendedUserAsync(true);

                IsVisiblePassword = false;
                IsDataEntryChanged = false;
                await HandleCommentAdded();
                await HandleHistoryAdded();
            }
            else
            {
                EditingDoc = new ExtendedUserUpdateDto();
                IsVisiblePassword = true;
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

        private async Task GetExtendedUserAsync(bool isRefresh)
        {
            if (isRefresh)
                Filter = new GetExtendedUsersInput(); // Clear all filter values for refresh 
            else
            {
                Filter.MaxResultCount = PageSize;
                Filter.SkipCount = (CurrentPage - 1) * PageSize;
                Filter.Sorting = CurrentSorting;
            }

            if (IsDataEntryChanged)
            {
                var result = await ExtendedUsersAppService.GetListNoPagedAsync(Filter);
                ExtendedUserList = ObjectMapper.Map<List<ExtendedUserDto>, List<ExtendedUserUpdateDto>>((List<ExtendedUserDto>)result);
            }
            else
            {
                var result = await ExtendedUsersAppService.GetListNoPagedAsync(Filter);
                ExtendedUserList = ObjectMapper.Map<List<ExtendedUserDto>, List<ExtendedUserUpdateDto>>((List<ExtendedUserDto>)result);
            }
        }

        private async Task GetCollectionAsync()
        {
            WeekOfVisitTypeCollection = CommonHelper.GetEnumLookupAsync<WeekOfVisitType, WeekOfVisitTypeList>("WeekOfVisitType", L);
            SalesFreqTypeCollection = CommonHelper.GetEnumLookupAsync<SalesFreqType, SalesFreqTypeList>("SalesFreqType", L);

            PositionCollection = (await IdentityUserAppService.GetAssignableRolesAsync()).Items;
            CompanyCollection = await CompaniesAppService.GetListCompaniesByUserOrRolesAsync((Guid)CurrentUser.Id, CurrentUser.Roles);
            StateCollection = await StatesAppService.GetListNoPagedAsync(new GetStatesInput { });
            ProvinceCollection = await ProvincesAppService.GetListNoPagedAsync(new GetProvincesInput { });
            TerritoryCollection = await TerritoriesAppService.GetListNoPagedAsync(new GetTerritoriesInput { });
            RouteTypeList = await RouteTypesAppService.GetListNoPagedAsync(new GetRouteTypesInput { });
        }


        #endregion Load Data Source for ListView & Users 


        /*========================== CRUD & Load Main Data Source Section =============================*/

        #region --Combobox_ValueChanged
        void CompanyValueChanged(Guid? companyId)
        {
            EditingDoc.CompanyId = companyId;
        }

        void TerritoryValueChanged(Guid? territoryId)
        {
            EditingDoc.TerritoryId = territoryId;
        }

        void ProvinceValueChanged(Guid? provinceId)
        {
            EditingDoc.ProvinceId = provinceId;
        }

        void PositionValueChanged(string positionName)
        {
            EditingDoc.Position = positionName;
            var position = PositionCollection.FirstOrDefault(x => x.Name == positionName);
            if (position != null)
            {
                EditingDoc.Position = position.Name;
            }
            else
            {
                EditingDoc.Position = string.Empty;
            }
        }
        #endregion Combobox_ValueChanged--


        #region --Get Data List
        private async Task FirstLoadAsync()
        {
            await SetPermissionsAsync();
            await SetToolbarItemsAsync();
            

            await GetExtendedUserAsync(true);
            await GetCollectionAsync();

            await LoadDataAsync(EditingDocId);
        }

        #endregion Get Data List--


        #region --New || Save || DeleteAsync

        private async Task RefreshAsync()
        {
            bool checkSaving = await SavingConfirmAsync();
            if (checkSaving)
            {
                await LoadDataAsync(EditingDocId);
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
                        if (EditingDocId == Guid.Empty)
                        {
                            EditingDoc.FullName = EditingDoc.Surname + " " + EditingDoc.Name;
                            EditingDoc = ObjectMapper.Map<ExtendedUserDto, ExtendedUserUpdateDto>(await ExtendedUsersAppService.CreateAsync(ObjectMapper.Map<ExtendedUserUpdateDto, ExtendedUserCreateDto>(EditingDoc)));
                            EditingDocId = EditingDoc.Id;

                            IsDataEntryChanged = false;
                            await ResetToolbarAsync();
                            await HandleHistoryAdded();
                            await LoadDataAsync(EditingDocId);
                            await UiNotificationService.Success(L["Notification:Save"]);
                        }
                        else
                        {
                            EditingDoc.FullName = EditingDoc.Surname + " " + EditingDoc.Name;
                            await ExtendedUsersAppService.UpdateAsync(EditingDocId, EditingDoc);
                            EditingDoc = ObjectMapper.Map<ExtendedUserDto, ExtendedUserUpdateDto>(await ExtendedUsersAppService.GetAsync(EditingDocId));

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
                    NavigationManager.NavigateTo($"/SystemAdministration/Users/{EditingDocId}");
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


        #region THÊM MỚI

        private async Task NewDataAsync()
        {
            EditingDoc = new ExtendedUserUpdateDto
            {
                ConcurrencyStamp = string.Empty,
            };
            EditingDocId = Guid.Empty;
            IsDataEntryChanged = false;

            await InvokeAsync(async () =>
            {
                await UpdateDataChangeStatus(false);
                NavigationManager.NavigateTo($"/SystemAdministration/Users/{Guid.Empty}");
            });

            await LoadDataAsync(EditingDocId);
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

        private async Task DeleteAsync()
        {
            try
            {
                if (EditingDocId != Guid.Empty)
                {
                    var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
                    if (confirmed)
                    {
                        await ExtendedUsersAppService.DeleteAsync(EditingDocId);
                        await InvokeAsync(async () =>
                        {
                            NavigationManager.NavigateTo($"/SystemAdministration/Users");
                            UiNotificationService.Error(L["Notification:Delete"]);
                        });

                        IsDataEntryChanged = false;
                        await ResetToolbarAsync();
                        await InvokeAsync(StateHasChanged);
                    }
                }
                else
                    await UiMessageService.Warn(L["Message:CannotDelete"]);
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

        private async Task DuplicateAsync()
        {
            if (EditingDocId != Guid.Empty)
            {
                bool checkSaving = await SavingConfirmAsync();
                if (checkSaving)
                {
                    EditingDoc = new ExtendedUserUpdateDto()
                    {
                        ConcurrencyStamp = string.Empty,
                    };

                    EditingDoc = ObjectMapper.Map<ExtendedUserDto, ExtendedUserUpdateDto>(await ExtendedUsersAppService.GetAsync(EditingDocId));
                    EditingDoc.Code = string.Empty;
                    EditingDoc.UserName = string.Empty;
                    EditingDoc.Email = string.Empty;
                    EditingDoc.Password = string.Empty;
                    EditingDocId = Guid.Empty;

                    await InvokeAsync(async () =>
                    {
                        IsDataEntryChanged = false;
                        await UpdateDataChangeStatus(false);
                        NavigationManager.NavigateTo($"/SystemAdministration/Users/{Guid.Empty}");
                    });

                    IsEditEnabled = true;
                    IsDataEntryChanged = true;
                    await UpdateDataChangeStatus(true);

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

        #endregion



        /*========================= Interaction/History/Comment Form Handling =========================*/

        #region --Interaction/History/Comment Form Handling
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

        #region --Validation, Checking
        private async Task<bool> SavingConfirmAsync() //  THÔNG BÁO KHI CÓ SỰ THAY ĐỔI NHƯNG MUỐN BACK RA NGOÀI
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

        #region --Others..
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
            bool isDuplicateCode = (await ExtendedUsersAppService.GetExistingDataByField(nameof(EditingDoc.Code), EditingDoc.Code, EditingDocId)).Any();
            bool isDuplicateUserName = (await ExtendedUsersAppService.GetExistingDataByField(nameof(EditingDoc.UserName), EditingDoc.UserName, EditingDocId)).Any();
            bool isDuplicateEmail = (await ExtendedUsersAppService.GetExistingDataByField(nameof(EditingDoc.Email), EditingDoc.Email, EditingDocId)).Any();

            await ValidationFormHelper.StartValidation();
            await ValidationFormHelper.Initialize(EditFormMain.EditContext!);

            await ValidateField(nameof(EditingDoc.Code), isDuplicateCode);
            await ValidateField(nameof(EditingDoc.UserName), isDuplicateUserName);
            await ValidateField(nameof(EditingDoc.Email), isDuplicateEmail);

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

        public static string TruncateText(string text, int maxLength) // Cắt chuỗi
        {
            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        #endregion

    }
}