using HQSOFT.CoreBackend.Permissions;
using HQSOFT.Common.Blazor.Pages.Component;
using HQSOFT.Common.Blazor.Pages.Component.Toolbar;
using HQSOFT.CoreBackend.Screens;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Microsoft.Extensions.Configuration;

using Blazorise;

using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using Volo.Abp.Auditing;
using Microsoft.AspNetCore.Components.Web;
using System.Text.RegularExpressions;
using HQSOFT.CoreBackend.Workspaces;
using System.Linq;
using HQSOFT.CoreBackend.Modules;
using Volo.Abp.Http.Client;
using DevExpress.Utils.Commands;
using HQSOFT.CoreBackend.Reports;
using HQSOFT.CoreBackend.ReportRuntimes;
using Newtonsoft.Json;
using static HQSOFT.CoreBackend.Permissions.CoreBackendPermissions;



namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.Screen
{
	public partial class Screens
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

		private EditForm EditFormMain { get; set; } = new EditForm(); //Id of Main form 

		private HQSOFTFormActivity formActivity;

		private bool isValidated = false;
		private bool CanCreate { get; set; }
		private bool CanEdit { get; set; }
		private bool CanDelete { get; set; }

        private WorkspaceDto WorkspaceMenu { get; set; } = new WorkspaceDto();

        private readonly object lockObject = new object();

		#endregion


		//Custom code: add more code based on actual requirement
		#region
		private bool IsEditEnabled { get; set; }
		private ScreenUpdateDto EditingDoc { get; set; }
		private List<ScreenDto> ScreenList { get; set; } = new List<ScreenDto>();
        private IReadOnlyList<ModuleDto> ModulesCollection { get; set; } = new List<ModuleDto>();

        #endregion



        /*===================================== Initialize Section ====================================*/
        #region
        public Screens(IConfiguration configuration, IAuditingManager auditingManager)
		{
			EditingDoc = new ScreenUpdateDto
			{
				ConcurrencyStamp = string.Empty,
			};


			formActivity = new HQSOFTFormActivity(configuration, auditingManager); BreadcrumbScreen = new HQSOFTBreadcrumbScreen();
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
				.IsGrantedAsync(CoreBackendPermissions.Screens.Create);
			CanEdit = await AuthorizationService
							.IsGrantedAsync(CoreBackendPermissions.Screens.Edit);
			CanDelete = await AuthorizationService
							.IsGrantedAsync(CoreBackendPermissions.Screens.Delete);
		}
		 
		protected virtual ValueTask SetToolbarItemsAsync()
		{
			Toolbar.Contributors.Clear();

			Toolbar.AddButton(L["Back"], async () =>
			{
				NavigationManager.NavigateTo($"/SystemAdministration/Screens");
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
						? CoreBackendPermissions.Screens.Edit
						: CoreBackendPermissions.Screens.Create,
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
		#region Load Data Source for ListView & Screens

		private async Task FirstLoadAsync()
		{
			await SetPermissionsAsync();
			await SetToolbarItemsAsync();
			

            await LoadCollectionAsync();
            await LoadDataAsync(EditingDocId);
		}

        #endregion



        /*========================== CRUD & Load Main Data Source Section =============================*/
        #region CRUD & Load Main Data

        private async Task LoadCollectionAsync()
        {
            ModulesCollection = await ModulesAppService.GetListNoPagedAsync(new GetModulesInput { });
        }

        private async Task LoadDataAsync(Guid classId)
		{
			await BlockUiService.Block(selectors: "#lpx-content-container", busy: false);

			if (classId != Guid.Empty)
			{
				EditingDoc = ObjectMapper.Map<ScreenDto, ScreenUpdateDto>(await ScreensAppService.GetAsync(classId));
				IsDataEntryChanged = false;
				await HandleCommentAdded();
				await HandleHistoryAdded();
			}
			else
			{
				EditingDoc = new ScreenUpdateDto();
				IsDataEntryChanged = false;
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
							var screen = await ScreensAppService.CreateAsync(ObjectMapper.Map<ScreenUpdateDto, ScreenCreateDto>(EditingDoc));
							EditingDoc = ObjectMapper.Map<ScreenDto, ScreenUpdateDto>(screen);
							EditingDocId = screen.Id;

							IsDataEntryChanged = false;
							await ResetToolbarAsync();
							await HandleHistoryAdded();
							await LoadDataAsync(EditingDocId);
							await UiNotificationService.Success(L["Notification:Save"]);
						}
						else
						{
							await ScreensAppService.UpdateAsync(EditingDocId, EditingDoc);
							EditingDoc = ObjectMapper.Map<ScreenDto, ScreenUpdateDto>(await ScreensAppService.GetAsync(EditingDocId));

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
                    NavigationManager.NavigateTo($"/SystemAdministration/Screens/{EditingDocId}");
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

		private async Task DeleteAsync()
		{
			if (EditingDocId != Guid.Empty)
			{
				var confirmed = await UiMessageService.Confirm(L["DeleteConfirmationMessage"]);
				if (confirmed)
				{
					try
					{
						await ScreensAppService.DeleteAsync(EditingDocId);
						await InvokeAsync(async () =>
						{
							NavigationManager.NavigateTo("/SystemAdministration/Screens");
							await UiNotificationService.Error(L["Notification:Delete"]);
						});

						IsDataEntryChanged = false;

						await ResetToolbarAsync();
                        await UpdateDataChangeStatus(false);
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
			EditingDoc = new ScreenUpdateDto
			{
				ConcurrencyStamp = string.Empty,
			};
			EditingDocId = Guid.Empty;
			IsDataEntryChanged = false;

            await InvokeAsync(async () =>
            {
                await UpdateDataChangeStatus(false);
                NavigationManager.NavigateTo($"/SystemAdministration/Screens/{Guid.Empty}");
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
					EditingDoc = new ScreenUpdateDto
					{
						ConcurrencyStamp = string.Empty,
					};

					EditingDoc = ObjectMapper.Map<ScreenDto, ScreenUpdateDto>(await ScreensAppService.GetAsync(EditingDocId));
					EditingDoc.Code = string.Empty;
					EditingDocId = Guid.Empty;

					await InvokeAsync(async () =>
                    {
                        IsDataEntryChanged = false;
                        await UpdateDataChangeStatus(false);
                        NavigationManager.NavigateTo($"/SystemAdministration/Screens/{Guid.Empty}");
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
			await ValidationFormHelper.StartValidation();
			await ValidationFormHelper.Initialize(EditFormMain.EditContext!);

			bool isDuplicateCode = (await ScreensAppService.GetExistingDataByField(nameof(EditingDoc.Code), EditingDoc.Code, EditingDocId)).Any();
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

		private async void HandleCtrlS(KeyboardEventArgs e)
		{
			await SaveDataAsync(false);
			await ResetToolbarAsync();
		}

		private async void HandleCtrlB(KeyboardEventArgs e)
		{
			await NewDataAsync();
		}
		#endregion



		/*=============================== Additional Application Functions ============================*/
		#region Additional Application Functions

		#endregion


	}
}
