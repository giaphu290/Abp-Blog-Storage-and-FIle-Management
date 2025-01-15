using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Blazorise;
using Blazorise.Snackbar;
using DevExpress.Blazor;

using Volo.Abp.Identity;
using Volo.Abp.AspNetCore.Components.Messages;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;

using System.Web;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;

using HQSOFT.CoreBackend.Countries;
using HQSOFT.CoreBackend.Currencies;
using HQSOFT.CoreBackend.FinanceBooks;
using HQSOFT.CoreBackend.Provinces;
using HQSOFT.CoreBackend.States;
using HQSOFT.CoreBackend.Taxes;
using HQSOFT.CoreBackend.Companies;
using HQSOFT.CoreBackend.Permissions;
using HQSOFT.CoreBackend.UserCompanies;
using HQSOFT.CoreBackend.CompanyRoles;
using HQSOFT.CoreBackend.ExtendedUsers;
using HQSOFT.CoreBackend.SalesRoutes;
using HQSOFT.CoreBackend.Territories;

using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using HQSOFT.Common.Blazor.Pages.Component;
using Microsoft.AspNetCore.Components.Web;
using HQSOFT.CoreBackend.Screens;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using Volo.Abp.Http.Client;
using Volo.Abp.ObjectMapping;
using HQSOFT.CoreBackend.Workspaces;
using HQSOFT.CoreBackend.Modules;
using HQSOFT.CoreBackend.CostCenters;



namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.Company
{
    public partial class Companies
    {
        //Standard code: Do not change
        #region

        protected List<Volo.Abp.BlazoriseUI.BreadcrumbItem> BreadcrumbItems = new List<Volo.Abp.BlazoriseUI.BreadcrumbItem>();
        protected PageToolbar Toolbar { get; } = new PageToolbar();

        [Parameter]
        public string Id { get; set; }
        private Guid EditingDocId { get; set; }

        private bool IsSelected { get; set; } = false;
        private bool IsDataEntryChanged { get; set; }
        private bool ShowInteractionForm { get; set; } = true;

        private EditForm EditFormMain { get; set; } = new EditForm();

        private HQSOFTFormActivity formActivity;

        private HQSOFTBreadcrumbScreen BreadcrumbScreen;


        private bool isValidated = false;
        private bool CanCreate { get; set; }
        private bool CanEdit { get; set; }
        private bool CanDelete { get; set; }

        private WorkspaceDto WorkspaceMenu { get; set; } = new WorkspaceDto(); private readonly object lockObject = new object();

        private bool isToolbarUpdating = false; // Biến để theo dõi trạng thái Toolbar

        #endregion


        //Custom code: add more code based on actual requirement 
        #region
        private bool CanCreateCompanyRole { get; set; }
        private bool CanEditCompanyRole { get; set; }
        private bool CanDeleteCompanyRole { get; set; }

        private bool CanCreateUserCompany { get; set; }
        private bool CanEditUserCompany { get; set; }
        private bool CanDeleteUserCompany { get; set; }

        private bool PanelVisible { get; set; }
        private bool IsCompanyRoleEditorVisible { get; set; } = false;
        private bool IsParentReadOnly { get; set; } = false;
        private bool IsEditEnabled { get; set; }
        private bool IsGroupEnabled { get; set; }


        private string ParentCompanyValue { get; set; }
        private Guid ParentCompanyId { get; set; }

        private GetCompaniesInput Filter { get; set; }
        private GetCountriesInput FilterCountry { get; set; }
        private GetProvincesInput FilterProvince { get; set; }
        private CompanyUpdateDto EditingDoc { get; set; }

        private List<TaxDto> TaxCollection { get; set; } = new List<TaxDto>();
        private List<ExtendedUserDto> UserCollection { get; set; } = new List<ExtendedUserDto>();
        private List<CompanyDto> CompanyCollection { get; set; } = new List<CompanyDto>();
        private List<CompanyDto> ParentCompaniesCollection { get; set; } = new List<CompanyDto>();
        private List<CompanyDto> ParentCompaniesIsGroupCollection { get; set; } = new List<CompanyDto>();
        private List<TerritoryDto> TerritoriesCollection { get; set; } = new List<TerritoryDto>();
        private List<CountryDto> CountriesCollection { get; set; } = new List<CountryDto>();
        private List<StateDto> StatesCollection { get; set; } = new List<StateDto>();
        private List<ProvinceDto> ProvincesCollection { get; set; } = new List<ProvinceDto>();
        private List<CurrencyDto> DefaultCurrencies { get; set; } = new List<CurrencyDto>();
        private List<IdentityRoleDto> RolesNotInCompanyRoles { get; set; } = new List<IdentityRoleDto>();
        private List<ExtendedUserDto> UsersNotInUserCompanies { get; set; } = new List<ExtendedUserDto>();
        private List<FinanceBookDto> FinanceBookCollection { get; set; } = new List<FinanceBookDto>();


        private IReadOnlyList<IdentityRoleDto> RoleCollection { get; set; } = new List<IdentityRoleDto>();
        private IReadOnlyList<CompanyDto> CompanyList { get; set; }


        // COMPANY ROLE GRID 
        private IGrid GridCompanyRole { get; set; } // Company Role grid control name
        private List<CompanyRoleUpdateDto> CompanyRoles { get; set; } = new List<CompanyRoleUpdateDto>(); //Data source used to bind to grid
        private IReadOnlyList<object> SelectedCompanyRoles { get; set; } = new List<CompanyRoleUpdateDto>(); //Selected rows on grid


        // USER COMPANY GRID 
        private bool IsUserCompanyEditorVisible { get; set; } = true;
        private IGrid GridUserCompany { get; set; } //UserCompany grid control name
        private List<UserCompanyUpdateDto> UserCompanies { get; set; } = new List<UserCompanyUpdateDto>(); //Data source used to bind to grid
        private IReadOnlyList<object> SelectedUserCompanies { get; set; } = new List<UserCompanyUpdateDto>(); //Selected rows on grid

        #endregion
          

        /*===================================== Initialize Section ====================================*/

        public Companies()
        {
            EditingDoc = new CompanyUpdateDto();
            BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
        }

        protected override async Task OnInitializedAsync()
        {
            EditingDocId = Guid.Parse(Id);

            //Lấy giá trị ParentCompagy từ đường dẫn mà CompanyListView truyền qua
            var uri = new Uri(NavigationManager.Uri);

            var paramParent = HttpUtility.ParseQueryString(uri.Query).Get("ParentCompany");

            ParentCompanyValue = paramParent;

            if (ParentCompanyValue != null)
            {
                ParentCompanyId = Guid.Parse(ParentCompanyValue);
                EditingDoc.ParentCompanyId = ParentCompanyId;
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

                await LoadGridData();

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




        /*============================= ToolBar - Breadcrumb - Permission ==============================*/

        #region  
        protected virtual ValueTask SetToolbarItemsAsync()
        {
            Toolbar.Contributors.Clear();

            Toolbar.AddButton(L["Back"], async () =>
            {
                NavigationManager.NavigateTo($"/SystemAdministration/Companies");
                await Task.CompletedTask;
            },
            IconName.Undo,
            Blazorise.Color.Light);

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
                        ? CoreBackendPermissions.Companies.Edit
                        : CoreBackendPermissions.Companies.Create,
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
                .IsGrantedAsync(CoreBackendPermissions.Companies.Create);
            CanEdit = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.Companies.Edit);
            CanDelete = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.Companies.Delete);

            CanCreateCompanyRole = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.CompanyRoles.Create);
            CanEditCompanyRole = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.CompanyRoles.Edit);
            CanDeleteCompanyRole = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.CompanyRoles.Delete);

            CanCreateUserCompany = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.UserCompanies.Create);
            CanEditUserCompany = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.UserCompanies.Edit);
            CanDeleteUserCompany = await AuthorizationService
                            .IsGrantedAsync(CoreBackendPermissions.UserCompanies.Delete);
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

        #region 
        private async Task LoadGridData()
        {
            await SetPermissionsAsync();
            await SetToolbarItemsAsync();
            

            await GetRoleListAsync();
            await GetUserListAsync();
            await GetCompanyListAsync();
            await GetCompanyRoleListAsync();
            await GetUserCompanyListAsync();
            await GetFinanceBookCollectionLookupAsync();
            await GetCurrencyCollectionLookupAsync();
            await GetParentCompanyCollectionLookupAsync();
            await GetCountryCollectionLookupAsync();
            await LoadDataAsync(EditingDocId);
            await FilterRolesAndUsersAsync(true, true, true);
        }

        private async Task GetCompanyListAsync()
        {
            CompanyCollection = await CompaniesAppService.GetListNoPagedAsync(new GetCompaniesInput { });
        }

        private async Task GetRoleListAsync()
        {
            var roleListResult = await IdentityRoleAppService.GetListAsync(new GetIdentityRoleListInput { });
            RoleCollection = roleListResult.Items.ToList();
        }

        private async Task GetUserListAsync()
        {
            UserCollection = await ExtendedUsersAppService.GetListNoPagedAsync(new GetExtendedUsersInput { });
        }

        private async Task GetCountryCollectionLookupAsync()
        {
            CountriesCollection = await CountriesAppService.GetListNoPagedAsync(new GetCountriesInput { });
        }
        private async Task GetProvinceCollectionLookupAsync()
        {
            var provincesCollection = await ProvincesAppService.GetListNoPagedAsync(new GetProvincesInput { });
            ProvincesCollection = provincesCollection.Where(x => x.TerritoryId == EditingDoc.TerritoryId).ToList();
        }

        private async Task GetStateCollectionLookupAsync()
        {
            var statesCollection = await StatesAppService.GetListNoPagedAsync(new GetStatesInput { });
            StatesCollection = statesCollection.Where(x => x.CountryId == EditingDoc.CountryId).ToList();
        }

        private async Task GetTerritoriesCollectionLookupAsync()
        {
            var territoriesCollection = await TerritoriesAppService.GetListNoPagedAsync(new GetTerritoriesInput { });
            TerritoriesCollection = territoriesCollection.Where(x => x.CountryId == EditingDoc.CountryId).ToList();
        }

        private async Task GetParentCompanyCollectionLookupAsync()
        {
            var result = await CompaniesAppService.GetListNoPagedAsync(new GetCompaniesInput { IsGroup = true });

            // Lọc danh sách công ty
            ParentCompaniesCollection = result
                .Where(x => x.Id != EditingDocId && // Loại bỏ công ty hiện tại
                            x.ParentCompanyId != EditingDocId) // Đảm bảo rằng công ty x không phải là cha của công ty đang được chỉnh sửa
                .ToList();
        }

        private async Task GetCurrencyCollectionLookupAsync()
        {
            DefaultCurrencies = await CurrenciesAppService.GetListNoPagedAsync(new GetCurrenciesInput { });
        }

        private async Task GetFinanceBookCollectionLookupAsync()
        {
            FinanceBookCollection = await FinanceBooksAppService.GetListNoPagedAsync(new GetFinanceBooksInput { });
        }

        #endregion




        /*=========================== CRUD & Load Main Data Source Section ============================*/

        #region 
        private async Task LoadDataAsync(Guid companyId)
        {
            await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

            if (companyId != Guid.Empty)
            {
                IsDataEntryChanged = false;
                EditingDoc = ObjectMapper.Map<CompanyDto, CompanyUpdateDto>(await CompaniesAppService.GetAsync(companyId));
                EditingDocId = EditingDoc.Id;

                await GetTerritoriesCollectionLookupAsync();
                await GetProvinceCollectionLookupAsync();
                await GetCompanyRoleListAsync();
                await GetUserCompanyListAsync();

                await HandleCommentAdded();
                await HandleHistoryAdded();
            }
            else
            {
                EditingDoc = new CompanyUpdateDto { };
                CompanyRoles = new List<CompanyRoleUpdateDto> { };
                UserCompanies = new List<UserCompanyUpdateDto> { };

                //Gán giá trị ParentCompany cho Company vừa ấn Edit hoặc Add trong Context Menu
                EditingDoc.ParentCompanyId = (ParentCompanyId == Guid.Empty) ? null : ParentCompanyId;
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

        private async Task DeleteAsync()
        {
            if (EditingDocId != Guid.Empty)
            {
                var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
                if (confirmed)
                {
                    try
                    {
                        await CompaniesAppService.DeleteAsync(EditingDocId);
                        await InvokeAsync(async () =>
                        {
                            NavigationManager.NavigateTo("/SystemAdministration/Companies");
                            await UiNotificationService.Error(L["Notification:Delete"]);
                        });

                        await ResetToolbarAsync();
                        IsDataEntryChanged = false;
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
            EditingDoc = new CompanyUpdateDto
            {
                ConcurrencyStamp = string.Empty,
            };

            EditingDocId = Guid.Empty;
            EditingDoc.ParentCompanyId = null;
            IsDataEntryChanged = false;

            await InvokeAsync(async () =>
            {
                await UpdateDataChangeStatus(false);
                NavigationManager.NavigateTo($"/SystemAdministration/Companies/{Guid.Empty}");
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
                    EditingDoc = new CompanyUpdateDto
                    {
                        ConcurrencyStamp = string.Empty,
                    };

                    foreach (var item in CompanyRoles)
                    {
                        item.Id = Guid.Empty;
                        item.CompanyId = Guid.Empty;
                        item.IsChanged = true;
                        item.ConcurrencyStamp = string.Empty;
                    }

                    foreach (var item in UserCompanies)
                    {
                        item.Id = Guid.Empty;
                        item.CompanyId = Guid.Empty;
                        item.IsChanged = true;
                        item.ConcurrencyStamp = string.Empty;
                    }

                    EditingDoc = ObjectMapper.Map<CompanyDto, CompanyUpdateDto>(await CompaniesAppService.GetAsync(EditingDocId));
                    EditingDoc.Code = string.Empty;
                    EditingDoc.ParentCompanyId = null;
                    EditingDocId = Guid.Empty;

                    await InvokeAsync(async () =>
                    {
                        IsDataEntryChanged = false;
                        await UpdateDataChangeStatus(false);
                        NavigationManager.NavigateTo($"/SystemAdministration/Companies/{Guid.Empty}");
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

                    bool isValid = false, isHandled = false, isExistsByParentId = false;

                    await InvokeAsync(async () =>
                    {
                        isValid = EditFormMain.EditContext.Validate();
                        isHandled = await HandleValidSubmit();
                        isExistsByParentId = await HandleValidateExistsByParentId();
                    });

                    if (!isValid || !isHandled || !isExistsByParentId)
                    {
                        await BlockUiService.UnBlock();
                        return;
                    }

                    if (IsDataEntryChanged)
                    {
                        await SaveGridDataAsync(GridCompanyRole);
                        await SaveGridDataAsync(GridUserCompany);

                        if (EditingDocId == Guid.Empty)
                        {
                            EditingDoc.ConcurrencyStamp = string.Empty;
                            EditingDoc = ObjectMapper.Map<CompanyDto, CompanyUpdateDto>(await CompaniesAppService.CreateAsync(ObjectMapper.Map<CompanyUpdateDto, CompanyCreateDto>(EditingDoc)));
                            EditingDocId = EditingDoc.Id;

                            await SaveCompanyRoleAsync();
                            await SaveUserCompanyAsync();

                            IsDataEntryChanged = false;
                            await ResetToolbarAsync();
                            await LoadDataAsync(EditingDocId);
                            await UiNotificationService.Success(L["Notification:Save"]);
                        }
                        else
                        {
                            await CompaniesAppService.UpdateAsync(EditingDocId, EditingDoc);
                            EditingDoc = ObjectMapper.Map<CompanyDto, CompanyUpdateDto>(await CompaniesAppService.GetAsync(EditingDocId));

                            await SaveCompanyRoleAsync();
                            await SaveUserCompanyAsync();

                            IsDataEntryChanged = false;
                            await HandleHistoryAdded();
                            await UiNotificationService.Success(L["Notification:Edit"]);
                        }
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
                    NavigationManager.NavigateTo($"/SystemAdministration/Companies/{EditingDocId}");
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

        private async Task SaveCompanyRoleAsync()
        {
            try
            {
                foreach (var companyRole in CompanyRoles.Where(x => x.IsChanged == true))
                {
                    if (companyRole.CompanyId == Guid.Empty)
                        companyRole.CompanyId = EditingDocId;

                    if (companyRole.ConcurrencyStamp == string.Empty && EditingDocId != Guid.Empty)
                        await CompanyRolesAppService.CreateAsync(ObjectMapper.Map<CompanyRoleUpdateDto, CompanyRoleCreateDto>(companyRole));
                }
                await FilterRolesAndUsersAsync(true, true, false);
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

        private async Task SaveUserCompanyAsync()
        {
            try
            {
                foreach (var userCompany in UserCompanies.Where(x => x.IsChanged == true))
                {
                    if (userCompany.CompanyId == Guid.Empty)
                        userCompany.CompanyId = EditingDocId;

                    if (userCompany.ConcurrencyStamp == string.Empty && EditingDocId != Guid.Empty)
                        await UserCompaniesAppService.CreateAsync(ObjectMapper.Map<UserCompanyUpdateDto, UserCompanyCreateDto>(userCompany));
                }
                await FilterRolesAndUsersAsync(true, false, true);
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
        #endregion


        #region Grid UserCompany

        private async Task GridUserCompanyNew_Click()
        {
            IsUserCompanyEditorVisible = true;
            await GridUserCompany.SaveChangesAsync();
            GridUserCompany.ClearSelection();
            await GridUserCompany.StartEditNewRowAsync();
        }

        private async Task GetUserCompanyListAsync()
        {
            var userCompanies = await UserCompaniesAppService.GetListByDocIdAsync(EditingDocId);
            UserCompanies = ObjectMapper.Map<List<UserCompanyDto>, List<UserCompanyUpdateDto>>(userCompanies);
        }

        private async Task DeleteUserCompanyAsync()
        {
            var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
            if (confirmed)
            {
                if (SelectedUserCompanies != null)
                {
                    foreach (UserCompanyUpdateDto row in SelectedUserCompanies)
                    {
                        await UserCompaniesAppService.DeleteAsync(row.Id);
                        UserCompanies.Remove(row);
                    }
                }
                GridUserCompany.Reload();
                await FilterRolesAndUsersAsync(false, false, true);
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task GridUserCompanyDelete_Click()
        {
            await DeleteUserCompanyAsync();
        }

        private async Task GridUserCompany_FocusedRowChanged(GridFocusedRowChangedEventArgs e)
        {
            IsUserCompanyEditorVisible = false;
            await InvokeAsync(StateHasChanged);
        }

        private async Task GridUserCompany_EditModelSaving(GridEditModelSavingEventArgs e)
        {
            UserCompanyUpdateDto editModel = (UserCompanyUpdateDto)e.EditModel;
            //Assign changes from the edit model to the data item. 
            if (editModel != null && e.IsNew)
            {
                editModel.IsChanged = true;
                IsDataEntryChanged = true;
                if (editModel.UserId != Guid.Empty)
                    UserCompanies.Add(editModel);
            }
            await FilterRolesAndUsersAsync(false, false, true);
            IsUserCompanyEditorVisible = false;
        }

        private void GridUserCompany_OnCustomizeEditModel(GridCustomizeEditModelEventArgs e)
        {
            if (e.IsNew)
            {
                var newRow = (UserCompanyUpdateDto)e.EditModel;
                newRow.Id = Guid.Empty;
                newRow.CompanyId = EditingDocId;
                newRow.ConcurrencyStamp = string.Empty;
                newRow.IsChanged = true;
            }
        }

        private void GridUserCompany_CustomizeDataRowEditor(GridCustomizeDataRowEditorEventArgs e)
        {
            if (e.EditSettings is ITextEditSettings settings)
                settings.ShowValidationIcon = true;
        }
        #endregion GridUserCompany


        #region GridCompanyRole
        private async Task GridCompanyRoleNew_Click()
        {
            await GridCompanyRole.SaveChangesAsync();
            GridCompanyRole.ClearSelection();
            await GridCompanyRole.StartEditNewRowAsync();
        }

        private async Task GetCompanyRoleListAsync()
        {
            var companyRoles = await CompanyRolesAppService.GetListByDocIdAsync(EditingDocId);
            CompanyRoles = ObjectMapper.Map<List<CompanyRoleDto>, List<CompanyRoleUpdateDto>>(companyRoles);
        }

        private async Task DeleteCompanyRoleAsync()
        {
            var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
            if (confirmed)
            {
                if (SelectedCompanyRoles != null)
                {
                    foreach (CompanyRoleUpdateDto row in SelectedCompanyRoles)
                    {
                        await CompanyRolesAppService.DeleteAsync(row.Id);
                        CompanyRoles.Remove(row);
                    }
                }
                GridCompanyRole.Reload();
                await FilterRolesAndUsersAsync(false, true, false);
                await InvokeAsync(StateHasChanged);
            }
        }
        private async Task GridCompanyRoleDelete_Click()
        {
            await DeleteCompanyRoleAsync();
        }

        private async Task GridCompanyRole_FocusedRowChanged(GridFocusedRowChangedEventArgs e)
        {
            await GridCompanyRole.SaveChangesAsync();
            GridCompanyRole.ClearSelection();
        }

        private async Task GridCompanyRole_EditModelSaving(GridEditModelSavingEventArgs e)
        {
            CompanyRoleUpdateDto editModel = (CompanyRoleUpdateDto)e.EditModel;
            //Assign changes from the edit model to the data item. 
            if (editModel != null && e.IsNew)
            {
                editModel.IsChanged = true;
                IsDataEntryChanged = true;
                if (editModel.RoleId != Guid.Empty)
                    CompanyRoles.Add(editModel);
            }
            await FilterRolesAndUsersAsync(false, true, false);
            IsCompanyRoleEditorVisible = false;
        }
        private void GridCompanyRole_OnCustomizeEditModel(GridCustomizeEditModelEventArgs e)
        {
            if (e.IsNew)
            {
                var newRow = (CompanyRoleUpdateDto)e.EditModel;
                newRow.Id = Guid.Empty;
                newRow.CompanyId = EditingDocId;
                newRow.ConcurrencyStamp = string.Empty;
                newRow.IsChanged = true;
            }
        }
        private void GridCompanyRole_CustomizeDataRowEditor(GridCustomizeDataRowEditorEventArgs e)
        {
            if (e.EditSettings is ITextEditSettings settings)
                settings.ShowValidationIcon = true;
        }
        #endregion GridCompanyRole



        /*========================================= Validations ========================================*/

        #region
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



        /*================================= Controls triggers/events =================================*/

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
            bool isDuplicateCode = (await CompaniesAppService.GetExistingDataByField(nameof(EditingDoc.Code), EditingDoc.Code, EditingDocId)).Any();
            bool isDuplicateTaxCode = (await CompaniesAppService.GetExistingDataByField(nameof(EditingDoc.TaxCode), EditingDoc.TaxCode, EditingDocId)).Any();

            await ValidationFormHelper.StartValidation();
            await ValidationFormHelper.Initialize(EditFormMain.EditContext!);

            await ValidateField(nameof(EditingDoc.Code), isDuplicateCode);
            await ValidateField(nameof(EditingDoc.TaxCode), isDuplicateTaxCode);

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

        private async Task<bool> HandleValidateExistsByParentId()
        {
            await ValidationFormHelper.StartValidation();
            await ValidationFormHelper.Initialize(EditFormMain.EditContext!);

            bool isParentCompany = (await CompaniesAppService.GetListByParentCompanyId(EditingDocId)).Any() && EditingDoc.IsGroup == false;
            await ValidateExistsByParentId(nameof(EditingDoc.IsGroup), isParentCompany);

            return await ValidationFormHelper.IsValid();
        }

        private async Task ValidateExistsByParentId(string fieldName, bool isParentCompany)
        {
            if (isParentCompany)
            {
                await ValidationFormHelper.ValidateExistsByParentId(fieldName, isParentCompany, EditingDoc, L);
            }
            else
            {
                await ValidationFormHelper.ClearMessagesByFieldName(fieldName, EditingDoc);
            }
        }

        private async void HandleCtrlS(KeyboardEventArgs e)
        {
            await SaveDataAsync(false);
        }

        private async void HandleCtrlB(KeyboardEventArgs e)
        {
            await CreateNewAsync();
            await ResetToolbarAsync();
        }

        async Task CountryValueChangedAsync(string newValue)
        {
            EditingDoc.CountryCode = newValue;
            var country = CountriesCollection.FirstOrDefault(x => x.Code == newValue);
            if (country != null)
            {
                EditingDoc.CountryId = country.Id;
            }

            TerritoriesCollection.Clear();
            ProvincesCollection.Clear();
            StatesCollection.Clear();

            EditingDoc.TerritoryId = null;
            EditingDoc.TerritoryCode = null;
            EditingDoc.ProvinceId = null;
            EditingDoc.ProvinceCode = null;
            EditingDoc.StateId = null;
            EditingDoc.StateCode = null;

            await GetTerritoriesCollectionLookupAsync();
            await GetStateCollectionLookupAsync();
            IsDataEntryChanged = true;
            await UpdateDataChangeStatus(true);
        }

        async Task TerritoryValueChangedAsync(string newValue)
        {
            EditingDoc.TerritoryCode = newValue;
            var territory = TerritoriesCollection.FirstOrDefault(x => x.Code == newValue);
            if (territory != null)
            {
                EditingDoc.TerritoryId = territory.Id;
            }

            ProvincesCollection.Clear();
            EditingDoc.ProvinceId = null;
            EditingDoc.ProvinceCode = null;

            await GetProvinceCollectionLookupAsync();
            IsDataEntryChanged = true;
            await UpdateDataChangeStatus(true); 
        }

        async Task ProvinceValueChangedAsync(string newValue)
        {
            EditingDoc.ProvinceCode = newValue;
            var provinces = ProvincesCollection.FirstOrDefault(x => x.Code == newValue);
            if (provinces != null)
            {
                EditingDoc.ProvinceId = provinces.Id;
            }

            IsDataEntryChanged = true;
            await UpdateDataChangeStatus(true);

            await Task.CompletedTask;
        }

        async Task StateValueChangedAsync(Guid? stateId)
        {
            EditingDoc.StateId = stateId;
            IsDataEntryChanged = true;
            await UpdateDataChangeStatus(true);

            await Task.CompletedTask;
        }

        async Task IsGroupValueChangedAsync(bool newValue)
        {
            EditingDoc.IsGroup = newValue;

            if (EditingDoc.IsGroup == true)
            {
                IsGroupEnabled = false;
                EditingDoc.ParentCompanyId = null;
            }
            else
            {
                IsGroupEnabled = true;
            }

            IsDataEntryChanged = true;
            await HandleValidateExistsByParentId();
            await UpdateDataChangeStatus(true);

            await Task.CompletedTask;
        }

        #endregion



        /*=================================== Application Functions ==================================*/

        #region Application Functions Support 

        private async Task HandleCommentAdded()
        {
            await formActivity.GetCommentListAsync();
        }
        private async Task HandleHistoryAdded()
        {
            await InvokeAsync(StateHasChanged);
            await formActivity.GetEntityChangeAsync();
        }

        private void ToggleInteractionFormAsync()
        {
            ShowInteractionForm = !ShowInteractionForm;
        }

        //GRID
        private async Task FilterRolesAndUsersAsync(bool IsGetFilter, bool IsRoles, bool IsUsers)
        {
            if (IsGetFilter && IsRoles)
            {
                await GetCompanyRoleListAsync();
            }

            if (IsGetFilter && IsUsers)
            {
                await GetUserCompanyListAsync();
            }

            if (IsRoles)
            {
                // Lấy danh sách các Role không có trong CompanyRoles
                var companyRoleIds = CompanyRoles.Select(cr => cr.RoleId).ToList();
                RolesNotInCompanyRoles = RoleCollection.Where(role => !companyRoleIds.Contains(role.Id)).ToList();
            }

            if (IsUsers)
            {
                // Lấy danh sách các User không có trong UserCompanies
                var userCompanyIds = UserCompanies.Select(uc => uc.UserId).ToList();
                UsersNotInUserCompanies = UserCollection.Where(user => !userCompanyIds.Contains(user.Id)).ToList();
            }
            await Task.CompletedTask;
        }

        #endregion


    }
}
