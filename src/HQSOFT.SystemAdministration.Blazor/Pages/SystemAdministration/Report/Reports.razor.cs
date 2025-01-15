using HQSOFT.CoreBackend.Permissions;
using HQSOFT.Common.Blazor.Pages.Component;
using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using HQSOFT.CoreBackend.Reports;
using HQSOFT.CoreBackend.EnumList;
using HQSOFT.CoreBackend.ReportParameters;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Blazorise;
using DevExpress.Blazor;

using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using System.Text.RegularExpressions;
using HQSOFT.CoreBackend.Screens;
using HQSOFT.CoreBackend.Modules;
using Volo.Abp.Http.Client;
using HQSOFT.CoreBackend.Companies;
using HQSOFT.CoreBackend.ReportRuntimes;
using Newtonsoft.Json;
using static HQSOFT.CoreBackend.Permissions.CoreBackendPermissions;
using HQSOFT.CoreBackend.Workspaces;


namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.Report
{
    public partial class Reports
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
        private bool CanCreateReportParameter { get; set; }
        private bool CanEditReportParameter { get; set; }
        private bool CanDeleteReportParameter { get; set; } 
        private bool IsEditEnabled { get; set; }

        private bool isReportViewerReady = false; // Kiểm tra xem ReportViewer đã sẵn sàng chưa
        private ReportUpdateDto EditingDoc { get; set; }

        private HQSOFTReportViewer reportViewer;

        private string ReportUrl { get; set; }
        private string ReportName { get; set; }
        private string ReportFileName { get; set; }

        private IGrid GridReportParameter { get; set; }

        private List<string?> ReportFiles { get; set; } = new List<string?>();
        private List<ReportParameterUpdateDto> ReportParameters { get; set; } = new List<ReportParameterUpdateDto>();

        private IReadOnlyList<object> SelectedReportParameters { get; set; } = new List<object>();
        private IReadOnlyList<ReportParameterTypeList> ReportParameterTypeCollection { get; set; } = new List<ReportParameterTypeList>();


        #endregion



        /*===================================== Initialize Section ====================================*/
        #region 
        public Reports(IConfiguration configuration, IAuditingManager auditingManager)
        {
            EditingDoc = new ReportUpdateDto
            {
                ConcurrencyStamp = string.Empty,
            };

            formActivity = new HQSOFTFormActivity(configuration, auditingManager); BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
            reportViewer = new HQSOFTReportViewer(configuration);
        }

        protected override async Task OnInitializedAsync()
        {
            EditingDocId =  Guid.Parse(Id);
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

                // Component reportViewer sẽ chỉ sẵn sàng sau khi giao diện đã được render
                isReportViewerReady = reportViewer != null;

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
                .IsGrantedAsync(CoreBackendPermissions.Reports.Create);
            CanEdit = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.Reports.Edit);
            CanDelete = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.Reports.Delete);

            CanCreateReportParameter = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.ReportParameters.Create);
            CanEditReportParameter = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.ReportParameters.Edit);
            CanDeleteReportParameter = await AuthorizationService
                .IsGrantedAsync(CoreBackendPermissions.ReportParameters.Delete);

        }
         
        protected virtual ValueTask SetToolbarItemsAsync()
        {
            Toolbar.Contributors.Clear();

            Toolbar.AddButton(L["Back"], () =>
            {
                NavigationManager.NavigateTo($"/SystemAdministration/Reports");
                return Task.CompletedTask;
            },
            IconName.Undo,
            Color.Light);

            var parmAction = new Dictionary<string, object>()
            {
                    {"Id", EditingDocId },
                    {"Print", EventCallback.Factory.Create(this, PrintAsync) },
                    {"RefreshAsync", EventCallback.Factory.Create(this, RefreshAsync) },
                    {"CreateNewAsync", EventCallback.Factory.Create(this, CreateNewAsync)},
                    {"DeleteAsync", EventCallback.Factory.Create(this, DeleteAsync)},
                    {"DuplicateAsync", EventCallback.Factory.Create(this, DuplicateAsync)},
                    {"CanEdit", CanEdit },
                    {"CanCreate", CanCreate },
                    {"CanDelete", CanDelete },
                    {"CanPrint", true },
                    {"CanSuggest", false },
                };
            Toolbar.AddComponent<NewEditAction>(parmAction);

            Toolbar.AddButton(
                    L["Save"],
                    async () => await SaveDataAsync(false),
                    IconName.Save,
                    Color.Primary,
                    requiredPolicyName: EditingDocId != Guid.Empty
                        ? CoreBackendPermissions.Reports.Edit
                        : CoreBackendPermissions.Reports.Create,
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

        #region Load Data Source for ListView & Reports

        private async Task FirstLoadAsync()
        {
            await SetPermissionsAsync();
            await SetToolbarItemsAsync();
            

            await GetCollectionDataAsync();
            await LoadDataAsync(EditingDocId);
            await HandleHistoryAdded();
            await InvokeAsync(StateHasChanged);
        }

        private async Task GetCollectionDataAsync()
        {
            ReportParameterTypeCollection = CommonHelper.GetEnumLookupAsync<ReportParameterType, ReportParameterTypeList>("ReportParameterType", L);

            ReportFiles = await UtilsAppService.GetFilesInReportsFolderAsync();
        }

        private async Task GetReportAsync()
        {
            var reports = await ReportsAppService.GetListNoPagedAsync(new GetReportsInput { ReportCode = "REPORTS" });
            var selectedReport = reports?.FirstOrDefault(x => x.ReportCode.Contains("REPORTS", StringComparison.OrdinalIgnoreCase));

            if (selectedReport != null)
            {
                ReportUrl = selectedReport.Id.ToString();
                ReportName = selectedReport.ReportName.ToString();
                ReportFileName = selectedReport.FileName.ToString();
            }
            else
            {
                Console.WriteLine("No matching report found.");
            }
            await InvokeAsync(StateHasChanged);
        }

        #endregion



        /*========================== CRUD & Load Main Data Source Section =============================*/

        #region CRUD & Load Main Data

        private async Task LoadDataAsync(Guid classId)
        {
            await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

            if (classId != Guid.Empty)
            {
                EditingDoc = ObjectMapper.Map<ReportDto, ReportUpdateDto>(await ReportsAppService.GetAsync(classId));
                var reportParameters = await ReportParametersAppService.GetListByReportIdAsync(classId);
                ReportParameters = ObjectMapper.Map<List<ReportParameterDto>, List<ReportParameterUpdateDto>>(reportParameters);
                await HandleHistoryAdded();
            }
            else
            {
                EditingDoc = new ReportUpdateDto();
                ReportParameters = new List<ReportParameterUpdateDto>();
            }

            await GetReportAsync();

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

        private async Task SaveDataAsync(bool IsNewNext)
        {
            try
            {
                if (IsNewNext)
                {
                    await CreateNewAsync();
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
                        if (GridReportParameter != null)
                            await GridReportParameter.SaveChangesAsync();

                        if (EditingDocId == Guid.Empty)
                        {
                            // Create
                            var report = await ReportsAppService.CreateAsync(ObjectMapper.Map<ReportUpdateDto, ReportCreateDto>(EditingDoc));
                            EditingDoc = ObjectMapper.Map<ReportDto, ReportUpdateDto>(report);
                            EditingDocId = EditingDoc.Id;
                            await SaveReportParameterAsync();

                            IsSelected = true;
                            IsDataEntryChanged = false;
                            await ResetToolbarAsync();
                            await LoadDataAsync(EditingDocId);
                            await UiNotificationService.Success(L["Notification:Save"]);
                        }
                        else
                        {
                            await ReportsAppService.UpdateAsync(EditingDocId, EditingDoc);
                            EditingDoc = ObjectMapper.Map<ReportDto, ReportUpdateDto>(await ReportsAppService.GetAsync(EditingDocId));
                            await SaveReportParameterAsync();

                            IsDataEntryChanged = false;
                            await HandleHistoryAdded();
                            await UiNotificationService.Success(L["Notification:Edit"]);
                        }

                        //Add Permission
                        await UtilsAppService.AddPermissionGroupAsync("Reports");
                        await UtilsAppService.AddPermissionAsync("Reports", "Reports." + EditingDoc.ReportCode);

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
                    NavigationManager.NavigateTo($"/SystemAdministration/Reports/{EditingDocId}");
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

        private async Task<bool> SaveReportParameterAsync()
        {
            try
            {
                foreach (var reportParameter in ReportParameters)
                {
                    if (reportParameter.IsChanged)
                    {
                        if (reportParameter.ReportId == Guid.Empty)
                            reportParameter.ReportId = EditingDocId;

                        if (reportParameter.ConcurrencyStamp == string.Empty && reportParameter.Id == Guid.Empty)
                            await ReportParametersAppService.CreateAsync(ObjectMapper.Map<ReportParameterUpdateDto, ReportParameterCreateDto>(reportParameter));
                        else
                            await ReportParametersAppService.UpdateAsync(reportParameter.Id, ObjectMapper.Map<ReportParameterUpdateDto, ReportParameterUpdateDto>(reportParameter));
                    }
                }
                return true;
            }
            catch
            {
                await BlockUiService.UnBlock();
                return false;
            }
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
                        await ReportsAppService.DeleteAsync(EditingDocId);
                        await InvokeAsync(async () =>
                        {
                            NavigationManager.NavigateTo("/SystemAdministration/Reports");
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
        }

        private async Task DeleteReportParameterAsync()
        {
            var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
            if (confirmed)
            {
                try
                {
                    if (SelectedReportParameters != null)
                    {
                        foreach (ReportParameterUpdateDto row in SelectedReportParameters)
                        {
                            await ReportParametersAppService.DeleteAsync(row.Id);
                            ReportParameters.Remove(row);
                        }
                    }

                    SelectedReportParameters = new List<ReportParameterUpdateDto>();
                    GridReportParameter.Reload();
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
                EditingDoc = new ReportUpdateDto
                {
                    ConcurrencyStamp = string.Empty,
                };
                EditingDocId = Guid.Empty;
                IsDataEntryChanged = false;

                await InvokeAsync(async () =>
                {
                    await UpdateDataChangeStatus(false);
                    NavigationManager.NavigateTo($"/SystemAdministration/Reports/{Guid.Empty}");
                });

                await LoadDataAsync(EditingDocId);
                await ResetToolbarAsync();
            }
        }

        private async Task DuplicateAsync()
        {
            if (EditingDocId != Guid.Empty)
            {
                bool checkSaving = await SavingConfirmAsync();
                if (checkSaving)
                {
                    EditingDoc = new ReportUpdateDto()
                    {
                        ConcurrencyStamp = string.Empty,
                    };

                    foreach (var item in ReportParameters)
                    {
                        item.Id = Guid.Empty;
                        item.ReportId = Guid.Empty;
                        item.IsChanged = true;
                        item.ConcurrencyStamp = string.Empty;
                    }

                    EditingDoc = ObjectMapper.Map<ReportDto, ReportUpdateDto>(await ReportsAppService.GetAsync(EditingDocId));
                    EditingDoc.ReportCode = string.Empty;
                    EditingDocId = Guid.Empty;

                    await InvokeAsync(async () =>
                    {
                        IsDataEntryChanged = false;
                        await UpdateDataChangeStatus(false);
                        NavigationManager.NavigateTo($"/SystemAdministration/Reports/{Guid.Empty}");
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


        #region GridReportParameter
        private async Task GridReportParameterNew_Click()
        {
            await GridReportParameter.SaveChangesAsync();
            GridReportParameter.ClearSelection();
            await GridReportParameter.StartEditNewRowAsync();
        }

        private async Task GridReportParameterDelete_Click()
        {
            await DeleteReportParameterAsync();
        }

        private async void GridReportParameter_EditModelSaving(GridEditModelSavingEventArgs e)
        {
            ReportParameterUpdateDto editModel = (ReportParameterUpdateDto)e.EditModel;

            if (editModel != null)
            {
                // Sanitize the ParmCode before proceeding
                editModel.ParmCode = await SanitizeCodeGridAsync(editModel.ParmCode);

                // Check if the sanitized ParmCode is empty (indicating a validation error)
                if (string.IsNullOrEmpty(editModel.ParmCode))
                {
                    e.Cancel = true; // Cancel the save operation
                    return; // Exit the method
                }

                // When editing, ensure that the ParmCode is unique, ignoring the current item being edited
                if (!e.IsNew)
                {
                    bool parmCodeExists = ReportParameters.Any(item => item.ParmCode == editModel.ParmCode && item.Idx != editModel.Idx);

                    if (parmCodeExists)
                    {
                        await UiNotificationService.Warn($"Parm ReportCode: {editModel.ParmCode} already exists!");
                        e.Cancel = true; // Cancel the save operation
                        return; // Exit the method
                    }

                    // If no conflict, proceed with updating the current item
                    editModel.IsChanged = true;
                    IsDataEntryChanged = true;

                    // Find the position of the data item in ReportParameters then update that element by editModel
                    int index = ReportParameters.FindIndex(item => item.Idx == editModel.Idx);
                    if (index != -1)
                    {
                        ReportParameters[index] = editModel;
                    }
                }
                else // When adding a new item
                {
                    // Ensure that the ParmCode is unique in the entire list
                    bool parmCodeExists = ReportParameters.Any(item => item.ParmCode == editModel.ParmCode);

                    if (parmCodeExists)
                    {
                        await UiNotificationService.Warn($"Parm ReportCode: {editModel.ParmCode} already exists!");
                        e.Cancel = true; // Cancel the save operation
                        return; // Exit the method
                    }

                    editModel.IsChanged = true;
                    IsDataEntryChanged = true;
                    ReportParameters.Add(editModel);
                }
            }
        }

        private void GridReportParameter_OnCustomizeEditModel(GridCustomizeEditModelEventArgs e)
        {
            if (e.IsNew)
            {

                var newRow = (ReportParameterUpdateDto)e.EditModel;
                newRow.Id = Guid.Empty;
                if (GridReportParameter.GetVisibleRowCount() > 0)
                {
                    newRow.Idx = ReportParameters.Max(x => x.Idx) + 1;
                }
                else
                    newRow.Idx = 1;

                newRow.ReportId = EditingDocId;
                newRow.ConcurrencyStamp = string.Empty;
                newRow.IsChanged = true;
            }
        }
        private void GridReportParameter_CustomizeDataRowEditor(GridCustomizeDataRowEditorEventArgs e)
        {
            if (e.EditSettings is ITextEditSettings settings)
                settings.ShowValidationIcon = true;
        }
        #endregion GridReportParameter


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

        private async Task<bool> CheckFocusState(string elementId)
        {
            bool isFocused = await JSRuntime.InvokeAsync<bool>("focusHandler.isElementFocused", elementId);
            return isFocused;
        }
        #endregion



        /*================================== Controls triggers/events =================================*/

        #region Controls triggers/events

        private async Task PrintAsync()
        {
            if (isReportViewerReady)
            {
                // Khởi tạo một đối tượng rỗng và chuyển thành JSON
                var emptyObject = new { };
                string emptyObjectJson = JsonConvert.SerializeObject(emptyObject);

                // Khởi tạo một danh sách để chứa tất cả các đối tượng
                var dataList = new List<object>();

                // Lặp qua từng ReportParameter và thêm vào danh sách
                foreach (var item in ReportParameters)
                {
                    dataList.Add(new
                    {
                        Id = EditingDoc.Id,
                        Code = EditingDoc.ReportCode,
                        ReportName = EditingDoc.ReportName,
                        Description = item.Description,
                        Type = item.Type,
                        ApiEndPoint = item.ApiEndPoint,
                        LanguageCode = item.LanguageCode,
                        ParmCode = "ReportList"
                    });
                }

                // Chuyển danh sách thành JSON
                string dataListJson = JsonConvert.SerializeObject(dataList);

                // Gọi phương thức PrepareForReporting với chuỗi JSON đã tạo
                var reportRuntimeId = await ReportRuntimesAppService.PrepareForReporting(ReportName, emptyObjectJson, dataListJson);

                // Lưu chuỗi JSON vào Local Storage
                await LocalStorage.SetItemAsync("reportRuntimeId", reportRuntimeId.ToString());

                // Chuyển hướng đến ReportViewer với runTimeId
                NavigationManager.NavigateTo($"reportviewer/{ReportUrl}?document={EditingDoc.ReportCode}");
            }
        }

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

            bool isDuplicateCode = (await ReportsAppService.GetExistingDataByField(nameof(EditingDoc.ReportCode), EditingDoc.ReportCode, EditingDocId)).Any();
            await ValidateField(nameof(EditingDoc.ReportCode), isDuplicateCode);

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

        private async Task<string> SanitizeCodeGridAsync(string input)
        {
            // Kiểm tra mã chỉ chứa các ký tự chữ, số và dấu gạch dưới (_), không được chứa ký tự trắng và ký tự đặc biệt
            var sanitizedInput = Regex.Replace(input ?? string.Empty, @"[^a-zA-Z0-9_]", "");

            // Kiểm tra mã có chứa ít nhất một ký tự chữ và không trống
            if (string.IsNullOrWhiteSpace(sanitizedInput) || !Regex.IsMatch(sanitizedInput, @"[a-zA-Z]"))
            {
                await UiMessageService.Error(L["CodeValidationError"]);
                return input; // Trả về input gốc thay vì chuỗi đã xóa
            }

            return sanitizedInput;
        }

        private async void HandleCtrlS(KeyboardEventArgs e)
        {
            await SaveDataAsync(false);
            await ResetToolbarAsync();
        }
        private async void HandleCtrlB(KeyboardEventArgs e)
        {
            await CreateNewAsync();
            await ResetToolbarAsync();
        }
        private async void HandleCtrlI(KeyboardEventArgs e)
        {
            await GridReportParameterNew_Click();

        }

        #endregion



        /*=============================== Additional Application Functions ============================*/

        #region Additional Application Functions

        #endregion


    }
}
