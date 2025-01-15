
using Blazorise;
using Blazorise.Snackbar;
using DevExpress.Blazor;
using DevExpress.Blazor.Internal.Editors.Models;

using HQSOFT.Common.Blazor.Pages.Component;
using HQSOFT.Common.Blazor.Pages.Component.Toolbar;

using HQSOFT.CoreBackend.EnumList;
using HQSOFT.CoreBackend.HangfireConfigs;
using HQSOFT.CoreBackend.Modules;
using HQSOFT.CoreBackend.Permissions;
using HQSOFT.CoreBackend.Reports;
using HQSOFT.CoreBackend.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Components.Messages;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using Volo.Abp.Auditing;
using Volo.Abp.BlazoriseUI.Components;
using Volo.Abp.Http.Client;


namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.HangfireConfig
{
    public partial class HangfireConfigs
    {
        //Standard code: Do not change
        #region

        protected List<Volo.Abp.BlazoriseUI.BreadcrumbItem> BreadcrumbItems = new List<Volo.Abp.BlazoriseUI.BreadcrumbItem>();
        protected PageToolbar Toolbar { get; } = new PageToolbar();

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

        #endregion


        //Custom code: add more code based on actual requirement    
        #region

        public string PeriodNumber = "";
        private int AutoValue { get; set; }
        private int CurrentAutoValue { get; set; }
        private int? MinTimeSpan { get; set; }
        private int? MaxTimeSpan { get; set; }
        private bool IsTimeSpanEnabled { get; set; } = false;
        private bool IsEndDateEnabled { get; set; } = false;
        private bool IsVisiblePINumber { get; set; } = true;
        private bool IsQueueEnabled { get; set; }


        private HangfireConfigDto? SelectedHangfireConfig;
        private HangfireConfigUpdateDto EditingDoc { get; set; } = new HangfireConfigUpdateDto();

        private IReadOnlyList<HangfireConfigDto> HangfireConfigList { get; set; }
        private IReadOnlyList<CronExpressionTypeList> CronExpressionList { get; set; } = new List<CronExpressionTypeList>();
        private IReadOnlyList<TimeZoneInfoTypeList> TimeZoneInfoList { get; set; } = new List<TimeZoneInfoTypeList>();

        #endregion




        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *										Initialize Section
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        #region
        public HangfireConfigs(IConfiguration configuration, IAuditingManager auditingManager)
        {
            EditingDoc = new HangfireConfigUpdateDto
            {
                ConcurrencyStamp = string.Empty,
            };

            AutoValue = 0;
            CurrentAutoValue = AutoValue;
            formActivity = new HQSOFTFormActivity(configuration, auditingManager); 
            BreadcrumbScreen = new HQSOFTBreadcrumbScreen(); 
        }

        protected override async Task OnInitializedAsync()
        {
            EditingDocId = Guid.Parse(Id);
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
            Toolbar.Contributors.Clear();

            Toolbar.AddButton(L["Back"], () =>
            {
                NavigationManager.NavigateTo($"/SystemAdministration/HangfireConfigs");
                return Task.CompletedTask;
            },
             IconName.Undo,
             Color.Light);

            var parmEdit = new Dictionary<string, object>()
                {
                    {"Id", EditingDocId },
                    {"RefreshAsync", EventCallback.Factory.Create(this, RefreshAsync) },
                    {"CreateNewAsync", EventCallback.Factory.Create(this, NewDataAsync)},
                    {"DeleteAsync", EventCallback.Factory.Create(this, DeleteClassAsync)},
                    {"DuplicateAsync", EventCallback.Factory.Create(this, DuplicateAsync)},
                    {"CanCreate", CanCreate},
                    {"CanEdit", CanEdit},
                    {"CanDelete", CanDelete}
                };
            Toolbar.AddComponent<NewEditAction>(parmEdit);

            Toolbar.AddButton(
                    L["Save"],
                    async () => await SaveDataAsync(false),
                    IconName.Save,
                    Color.Primary,
                    requiredPolicyName: EditingDocId != Guid.Empty
                        ? CoreBackendPermissions.HangfireConfigs.Edit
                        : CoreBackendPermissions.HangfireConfigs.Create,
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

        #endregion



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								  Load Data Source for ListView
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        #region
        private async Task LoadGridData()
        {
            await SetPermissionsAsync();
            
            await SetToolbarItemsAsync();

            await GetCollectionAsync();
            await LoadDataAsync(EditingDocId);
        }

        private async Task LoadDataAsync(Guid classId)
        {
            await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

            if (classId != Guid.Empty)
            {
                EditingDoc = ObjectMapper.Map<HangfireConfigDto, HangfireConfigUpdateDto>(await HangfireConfigsAppService.GetAsync(classId));
                await HandleCommentAdded();
                await HandleHistoryAdded();
                IsDataEntryChanged = false;
            }
            else
            {
                EditingDoc = new HangfireConfigUpdateDto() { };
                EditingDoc.TimeSpan = 0;
                EditingDoc.CronExpression = CronExpressionType.Minutely.ToString();
                EditingDoc.Queue = "default";
                EditingDoc.TimeZoneInfo = TimeZoneInfoType.Utc.ToString();
                IsVisiblePINumber = false;
            }

            IsDataEntryChanged = false;

            ValidationFormHelper.MessageStore = null;
            await ValidationFormHelper.StartValidation();
            await ValidationFormHelper.Initialize(EditFormMain.EditContext!);

            await ResetToolbarAsync();
            await UpdateDataChangeStatus(false);
            await BlockUiService.UnBlock();
            await InvokeAsync(StateHasChanged);
        }

        private async Task GetCollectionAsync()
        {
            CronExpressionList = CommonHelper.GetEnumLookupAsync<CronExpressionType, CronExpressionTypeList>("CronExpressionType", L);
            TimeZoneInfoList = CommonHelper.GetEnumLookupAsync<TimeZoneInfoType, TimeZoneInfoTypeList>("TimeZoneInfoType", L);
            await Task.CompletedTask;
        }

        #endregion



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *								CRUD & Load Main Data Source Section
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        #region
        private async Task NewDataAsync()
        {
            bool checkSaving = await SavingConfirmAsync();
            if (checkSaving)
            {
                EditingDoc = new HangfireConfigUpdateDto
                {
                    ConcurrencyStamp = string.Empty,
                };
                EditingDocId = Guid.Empty;
                IsDataEntryChanged = false;

                await InvokeAsync(async () =>
                {
                    await UpdateDataChangeStatus(false);
                    NavigationManager.NavigateTo($"/SystemAdministration/HangfireConfigs/{Guid.Empty}");
                });
                await LoadDataAsync(EditingDocId);
                if (EditingDocId == Guid.Empty)
                {
                    await ResetToolbarAsync();
                }
            }
        }

        private async Task DeleteClassAsync()
        {
            if (EditingDocId != Guid.Empty)
            {
                var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
                if (confirmed)
                {
                    await HangfireConfigsAppService.DeleteAsync(EditingDocId);
                    await InvokeAsync(async () =>
                    {
                        NavigationManager.NavigateTo("/SystemAdministration/HangfireConfigs");
                    });
                    await ResetToolbarAsync();
                    IsDataEntryChanged = false;
                    await InvokeAsync(StateHasChanged);
                }
            }
            else
                await UiMessageService.Warn(L["Message:CannotDelete"]);
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

                    await InvokeAsync(async () =>
                    {
                        bool isValid = EditFormMain.EditContext.Validate();
                        bool isHandled = await HandleValidSubmit();
                        if (!isValid || !isHandled)
                        {
                            await BlockUiService.UnBlock();
                            return;
                        }
                    });

                    if (IsDataEntryChanged)
                    {
                        if (EditingDocId == Guid.Empty)
                        {
                            // Chọn biểu thức cron dựa trên loại cronExpressionType 
                            EditingDoc = ObjectMapper.Map<HangfireConfigDto, HangfireConfigUpdateDto>(await HangfireConfigsAppService.CreateAsync(ObjectMapper.Map<HangfireConfigUpdateDto, HangfireConfigCreateDto>(EditingDoc)));
                            EditingDocId = EditingDoc.Id;

                            IsDataEntryChanged = false;
                            await ResetToolbarAsync();
                            await LoadDataAsync(EditingDocId);
                            await UiNotificationService.Success(L["Notification:Save"]);
                        }
                        else
                        {
                            await HangfireConfigsAppService.UpdateAsync(EditingDocId, EditingDoc);
                            EditingDoc = ObjectMapper.Map<HangfireConfigDto, HangfireConfigUpdateDto>(await HangfireConfigsAppService.GetAsync(EditingDocId));
                            IsDataEntryChanged = false;
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
                    NavigationManager.NavigateTo($"/SystemAdministration/HangfireConfigs/{EditingDocId}");
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

        #endregion




        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *									    Validation, Checking
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

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




        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *  * * * * * * * * * 
         *									  Controls triggers/events
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        #region

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
            await ValidationFormHelper.StartValidation();
            await ValidationFormHelper.Initialize(EditFormMain.EditContext!);

            var allCodes = await HangfireConfigsAppService.GetListNoPagedAsync(new GetHangfireConfigsInput { });

            var isDuplicate = allCodes.Any(x => (x.RecurringJobId.Equals(EditingDoc.RecurringJobId, StringComparison.OrdinalIgnoreCase)
                                                        && x.JobMethodName.Equals(EditingDoc.JobMethodName, StringComparison.OrdinalIgnoreCase))
                                                        && x.Id != EditingDocId);

            await ValidateField(nameof(EditingDoc.RecurringJobId), isDuplicate);
            await ValidateField(nameof(EditingDoc.JobMethodName), isDuplicate);

            return await ValidationFormHelper.IsValid();
        }

        private async Task ValidateField(string fieldName, bool isDuplicate)
        {
            if (isDuplicate)
            {
                await ValidationFormHelper.ValidateHangfireJobsUnique(fieldName, isDuplicate, EditingDoc, L);
            }
            else
            {
                await ValidationFormHelper.ClearMessagesByFieldName(fieldName, EditingDoc);
            }
        }

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

        private async void HandleCtrlS(KeyboardEventArgs e)
        {
            await SaveDataAsync(false);
            await ResetToolbarAsync();
        }

        private async void HandleCtrlB(KeyboardEventArgs e)
        {
            await NewDataAsync();
        }

        private async Task CronExpressionChangedAsync(string item)
        {
            EditingDoc.CronExpression = item;

            switch (EditingDoc.CronExpression)
            {
                case nameof(CronExpressionType.Minutely):
                    IsTimeSpanEnabled = false;
                    EditingDoc.TimeSpan = 0;
                    break;

                case nameof(CronExpressionType.Hourly):
                    IsTimeSpanEnabled = true;
                    MinTimeSpan = 0;
                    MaxTimeSpan = 59;
                    break;

                case nameof(CronExpressionType.Daily):
                    IsTimeSpanEnabled = true;
                    MinTimeSpan = 0;
                    MaxTimeSpan = 23;
                    break;
            }

            IsDataEntryChanged = true;
            await InvokeAsync(StateHasChanged);
        }

        private async Task TimeZoneInfoChangedAsync(string item)
        {
            EditingDoc.TimeZoneInfo = item;
            IsDataEntryChanged = true;

            await InvokeAsync(StateHasChanged);
        }

        private async Task DuplicateAsync()
        {
            if (EditingDocId != Guid.Empty)
            {
                bool checkSaving = await SavingConfirmAsync();
                if (checkSaving)
                {
                    EditingDoc = new HangfireConfigUpdateDto()
                    {
                        ConcurrencyStamp = string.Empty,
                    };

                    EditingDoc = ObjectMapper.Map<HangfireConfigDto, HangfireConfigUpdateDto>(await HangfireConfigsAppService.GetAsync(EditingDocId));
                    EditingDoc.JobMethodName = string.Empty;
                    EditingDocId = Guid.Empty;

                    await InvokeAsync(async () =>
                    {
                        IsDataEntryChanged = false;
                        await UpdateDataChangeStatus(false);
                        NavigationManager.NavigateTo($"/SystemAdministration/HangfireConfigs/{Guid.Empty}");
                    });

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

        private async Task RefreshAsync()
        {
            bool checkSaving = await SavingConfirmAsync();
            if (checkSaving)
            {
                await LoadDataAsync(EditingDocId);
            }
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




    }
}
