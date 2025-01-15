
using Blazorise;

using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using HQSOFT.Common.Blazor.Pages.Component;

using HQSOFT.CoreBackend.Permissions;
using HQSOFT.CoreBackend.SystemSettings;
using HQSOFT.CoreBackend.EnumList;
using HQSOFT.CoreBackend.SequenceNumbers;
using HQSOFT.CoreBackend.ElsaWorkflowManager;
using HQSOFT.CoreBackend.Numberings;
using HQSOFT.CoreBackend.AccountReceivableSettings;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using Volo.Abp.Auditing;
using Volo.Abp.Http.Client;
using HQSOFT.CoreBackend.Countries; 
using HQSOFT.CoreBackend.TimeZoneInformations;
using Volo.Abp.LanguageManagement.Dto;
using System.Text.RegularExpressions;
using Volo.Abp.LanguageManagement;


namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.SystemSetting
{
    public partial class SystemSettings
    {
        //Standard code: Do not change
        protected List<Volo.Abp.BlazoriseUI.BreadcrumbItem> BreadcrumbItems = new List<Volo.Abp.BlazoriseUI.BreadcrumbItem>();
        protected PageToolbar Toolbar { get; } = new PageToolbar();
        private Guid EditingDocId { get; set; } //Current edditing Order Id
        private EditForm EditFormMain { get; set; } = new EditForm(); //Id of Main form

        private bool IsDataEntryChanged { get; set; } //keep value to indicate data has been changed or not
        private bool ShowInteractionForm { get; set; } = true;

        private HQSOFTFormActivity formActivity;
        private HQSOFTBreadcrumbScreen BreadcrumbScreen { get; set; }

        private bool CanCreate { get; set; }
        private bool CanEdit { get; set; }
        private bool CanDelete { get; set; }

        private readonly object lockObject = new object();


        //Custom code: add more code based on actual requirement
         
        private SystemSettingUpdateDto EditingDoc { get; set; } = new SystemSettingUpdateDto(); 
        private IReadOnlyList<LanguageDto> LanguageCollection { get; set; } = new List<LanguageDto>(); 
        private IReadOnlyList<CountryDto> CountryCollection { get; set; } = new List<CountryDto>();
        private IReadOnlyList<SystemSettingDto> SystemSettingCollection { get; set; } = new List<SystemSettingDto>();
        private IReadOnlyList<TimeZoneInformationDto> TimeZoneCollection { get; set; } = new List<TimeZoneInformationDto>();

        private IReadOnlyList<DateFormatTypeList> DateFormatCollection { get; set; } = new List<DateFormatTypeList>();
        private IReadOnlyList<TimeFormatTypeList> TimeFormatCollection { get; set; } = new List<TimeFormatTypeList>();
        private IReadOnlyList<NumberFormatTypeList> NumberFormatCollection { get; set; } = new List<NumberFormatTypeList>();
        private IReadOnlyList<PricePrecisionTypeList> PricePrecisionCollection { get; set; } = new List<PricePrecisionTypeList>();
        private IReadOnlyList<QuantityPrecisionTypeList> QuantityPrecisionCollection { get; set; } = new List<QuantityPrecisionTypeList>();
          
        private bool isValidated = false;
        private ValidationMessageStore? _messageStore = null;



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *										Initialize Section
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        #region
        public SystemSettings(IConfiguration configuration, IAuditingManager auditingManager)
        {
            EditingDoc = new SystemSettingUpdateDto
            {
                ConcurrencyStamp = string.Empty,
                Code = "DEFAUFT"
            };
            // Initialize other properties and fields 
            formActivity = new HQSOFTFormActivity(configuration, auditingManager); BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
            BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
        }

        protected override async Task OnInitializedAsync()
        {
            await GetSetting();
            await SetPermissionsAsync();
            await SetToolbarItemsAsync();
            
        }
        private async Task OnBeforeInternalNavigation(LocationChangingContext context)
        {
            bool checkSaving = await SavingConfirmAsync();
            if (!checkSaving)
                context.PreventNavigation();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

                await JSRuntime.InvokeVoidAsync("FullScreen");
                await JSRuntime.InvokeVoidAsync("AssignGotFocus"); await BreadcrumbScreen.GetBreadcrumbsAsync();
                await JSRuntime.InvokeVoidAsync("initializeDataChangeHandling");

                await FirstLoadAsync();
                ValidationFormHelper.MessageStore = null;
                await ValidationFormHelper.StartValidation();
                await ValidationFormHelper.Initialize(EditFormMain.EditContext!);
                await BlockUiService.UnBlock();
            }
            await base.OnAfterRenderAsync(firstRender);
        }
        private async Task SetPermissionsAsync()
        {
            CanCreate = await AuthorizationService.IsGrantedAsync(CoreBackendPermissions.SystemSettings.Create);
            CanEdit = await AuthorizationService.IsGrantedAsync(CoreBackendPermissions.SystemSettings.Edit);
            CanDelete = await AuthorizationService.IsGrantedAsync(CoreBackendPermissions.SystemSettings.Delete);
        }
        
        protected virtual ValueTask SetToolbarItemsAsync()
        { 
            Toolbar.AddButton(
                  L["Save"],
                  async () => await SaveDataAsync(false),
                  IconName.Save,
                  Color.Primary,
                  requiredPolicyName: EditingDocId != Guid.Empty
                       ? CoreBackendPermissions.SystemSettings.Edit
                       : CoreBackendPermissions.SystemSettings.Create,
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
        protected override void Dispose(bool disposing) 
        {
            JSRuntime.InvokeVoidAsync("UnFullScreen");
            base.Dispose(disposing);
        }

        #endregion

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								Load Data Source for ListView & Others
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        #region
        private async Task FirstLoadAsync()
        {
            await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);
            await GetCollectionDataAsync();
            await LoadDataAsync();
            await InvokeAsync(StateHasChanged);
            IsDataEntryChanged = false;
            await BlockUiService.UnBlock();
        }

        private async Task GetCollectionDataAsync()
        {
            DateFormatCollection = CommonHelper.GetEnumLookupAsync<DateFormatType, DateFormatTypeList>("DateFormatType", L);
            TimeFormatCollection = CommonHelper.GetEnumLookupAsync<TimeFormatType, TimeFormatTypeList>("TimeFormatType", L);
            NumberFormatCollection = CommonHelper.GetEnumLookupAsync<NumberFormatType, NumberFormatTypeList>("NumberFormatType", L);
            PricePrecisionCollection = CommonHelper.GetEnumLookupAsync<PricePrecisionType, PricePrecisionTypeList>("PricePrecisionType", L);
            QuantityPrecisionCollection = CommonHelper.GetEnumLookupAsync<QuantityPrecisionType, QuantityPrecisionTypeList>("QuantityPrecisionType", L);

            CountryCollection = await CountriesAppService.GetListNoPagedAsync(new GetCountriesInput { });
            TimeZoneCollection = await TimeZoneInformationsAppService.GetListNoPagedAsync(new GetTimeZoneInformationsInput { });
            SystemSettingCollection = await SystemSettingsAppService.GetListNoPagedAsync(new GetSystemSettingsInput { });

            var langues = await LanguageAppService.GetAllListAsync();
            LanguageCollection = langues.Items.ToList();

            await InvokeAsync(StateHasChanged);
        }

        private async Task GetSetting()
        {
            var result = await SystemSettingsAppService.GetListNoPagedAsync(new GetSystemSettingsInput { FilterText = "" });
            if (result == null || !result.Any())
            {
                EditingDocId = Guid.Empty;
            }
            else
            {
                var purchasesingSettingId = result.FirstOrDefault()?.Id;
                if (purchasesingSettingId != null && purchasesingSettingId != Guid.Empty)
                {
                    EditingDocId = (Guid)purchasesingSettingId;
                }
                else
                {
                    EditingDocId = Guid.Empty;
                }
            }
        }
        #endregion

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								CRUD & Load Main Data Source Section
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        #region
         
        private async Task LoadDataAsync()
        { 
            var systemSettingsList = await SystemSettingsAppService.GetListNoPagedAsync(new GetSystemSettingsInput { });

            if (systemSettingsList.Any())
            {
                // Assuming you want to map only the first item in the list
                EditingDoc = ObjectMapper.Map<SystemSettingDto, SystemSettingUpdateDto>(systemSettingsList.First());
                EditingDocId = EditingDoc.Id;

                await HandleHistoryAdded();
                await HandleCommentAdded();
            }
            else
            {
                // Handle the case where the list is empty
                EditingDoc = new SystemSettingUpdateDto();
                EditingDoc.Code = "DEFAULT";
            } 

            ValidationFormHelper.MessageStore = null;
            await ValidationFormHelper.StartValidation();
            await ValidationFormHelper.Initialize(EditFormMain.EditContext!);

            IsDataEntryChanged = false;

            await ResetToolbarAsync();
            await UpdateDataChangeStatus(false);
            await BlockUiService.UnBlock();
            await InvokeAsync(StateHasChanged);
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

                    await InvokeAsync( async() =>
                    {
                        bool isValid = EditFormMain.EditContext.Validate(); 

                        if (!isValid)
                        {
                            await BlockUiService.UnBlock();
                            return;
                        }
                    });
                    
                    if (IsDataEntryChanged)
                    {
                        // C&U
                        if (EditingDocId == Guid.Empty)
                        {
                            // Create
                            EditingDoc.Code = "DEFAULT";
                            EditingDoc.ConcurrencyStamp = string.Empty;
                            var SystemSetting = await SystemSettingsAppService.CreateAsync(ObjectMapper.Map<SystemSettingUpdateDto, SystemSettingCreateDto>(EditingDoc));
                            EditingDoc = ObjectMapper.Map<SystemSettingDto, SystemSettingUpdateDto>(SystemSetting);
                            EditingDocId = EditingDoc.Id;
                            await ResetToolbarAsync();
                        }
                        else
                        {
                            // Update
                            await SystemSettingsAppService.UpdateAsync(EditingDocId, EditingDoc);
                            EditingDoc = ObjectMapper.Map<SystemSettingDto, SystemSettingUpdateDto>(await SystemSettingsAppService.GetAsync(EditingDocId));
                        }
                        await LoadDataAsync();
                        await HandleHistoryAdded();
                        IsDataEntryChanged = false;
                        await UiNotificationService.Success(L["Notification:Save"]);
                    }
                    else
                    {
                        if (EditingDocId != Guid.Empty)
                            await UiNotificationService.Warn(L["Notification:NoChangesInDocument"]);
                    }
                }

                await InvokeAsync(() =>
                {
                    IsDataEntryChanged = false;
                    NavigationManager.NavigateTo($"/SystemAdministration/SystemSettings");
                });
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
        private async Task DeleteDataAsync()
        {
            var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
            if (confirmed)
            {
                await SystemSettingsAppService.DeleteAsync(EditingDocId);
                NavigationManager.NavigateTo("/SystemAdministration/SystemSettings");
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task RefreshDataAsync()
        {
            bool checkSaving = await SavingConfirmAsync();
            if (checkSaving)
            {
                await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);
                await LoadDataAsync();
                await HandleCommentAdded();
                await BlockUiService.UnBlock();
            }
        }
        private async Task NewDataAsync()
        {
            bool checkSaving = await SavingConfirmAsync();
            if (checkSaving)
            {
                EditingDoc = new SystemSettingUpdateDto
                {
                    ConcurrencyStamp = string.Empty,
                };
                EditingDocId = Guid.Empty;
                IsDataEntryChanged = false;
                await InvokeAsync(() =>
                {
                    NavigationManager.NavigateTo($"/SystemAdministration/SystemSettings/{Guid.Empty}");

                });
                await LoadDataAsync();
                if (EditingDocId == Guid.Empty)
                {
                    await ResetToolbarAsync();
                }
            }
        }
        private async Task DuplicateAsync()
        {
            if (EditingDocId != Guid.Empty)
            {
                EditingDoc = new SystemSettingUpdateDto
                {
                    ConcurrencyStamp = string.Empty,
                };

                EditingDoc = ObjectMapper.Map<SystemSettingDto, SystemSettingUpdateDto>(await SystemSettingsAppService.GetAsync(EditingDocId));
                EditingDocId = Guid.Empty;

                await InvokeAsync(() =>
                {
                    NavigationManager.NavigateTo($"/SystemAdministration/SystemSettings/{Guid.Empty}");
                });
                await ResetToolbarAsync();
                IsDataEntryChanged = false;
            }
            else
            {
                await UiMessageService.Warn(L["Message:CannotDuplicate"]);
                await ResetToolbarAsync();
            }
        }
          
        #endregion

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								Interaction/History/Comment Form Handling
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
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

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								            Validations
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
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
        #endregion

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								     Controls triggers/events
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
         
        #region Value Changed

        async Task TimeZoneValueChangedAsync(Guid? newValue)
        {
            EditingDoc.TimeZone = newValue;

            IsDataEntryChanged = true;
            await InvokeAsync(StateHasChanged);
        }

        async Task CountryValueChangedAsync(Guid? newValue)
        {
            EditingDoc.Country = newValue;

            IsDataEntryChanged = true;
            await InvokeAsync(StateHasChanged);
        }

        async Task LanguageValueChangedAsync(Guid? newValue)
        {
            EditingDoc.Language = newValue;

            IsDataEntryChanged = true;
            await InvokeAsync(StateHasChanged);
        }

        #endregion


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

        private async Task<string> SanitizeFromUnitCode(string input)
        {
            var sanitizedInput = Regex.Replace(input ?? string.Empty, @"[^a-zA-Z0-9_]", "");

            if (sanitizedInput != input)
            {
                await UiMessageService.Error(L["CodeValidationError"]);
            }

            return sanitizedInput;
        }

        private async Task<string> SanitizeToUnitCode(string input)
        {
            var sanitizedInput = Regex.Replace(input ?? string.Empty, @"[^a-zA-Z0-9_]", "");

            if (sanitizedInput != input)
            {
                await UiMessageService.Error(L["CodeValidationError"]);
            }

            return sanitizedInput;
        }

        private async void HandleCtrlS(KeyboardEventArgs e)
        {
            await SaveDataAsync(false);
            await ResetToolbarAsync();
        }
        #endregion


    }
}
