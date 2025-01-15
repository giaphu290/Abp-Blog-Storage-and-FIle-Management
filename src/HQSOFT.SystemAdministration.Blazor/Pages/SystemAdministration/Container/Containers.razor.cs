using Blazorise;
using Blazorise.DataGrid;
using DevExpress.XtraPrinting.Native.Extensions;
using HQSOFT.SystemAdministration.Containers;
using HQSOFT.SystemAdministration.Permissions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;

namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.Container
{
    public partial class Containers
    {
        private IReadOnlyList<ContainerDto> ContainerList { get; set; }
        private int PageSize { get; } = LimitedResultRequestDto.DefaultMaxResultCount;
        private int CurrentPage { get; set; } = 1;
        private string CurrentSorting { get; set; }
        private int TotalCount { get; set; }
        private bool CanEditContainer { get; set; }
        private bool CanDeleteContainer { get; set; }
        private CreateContainerDto NewContainer { get; set; }
        private Guid EditingContainerId { get; set; }
        private UpdateContainerDto EditingContainer { get; set; }
        private Modal CreateContainerModal { get; set; }
        private Modal EditContainerModal { get; set; }
        protected PageToolbar Toolbar { get; } = new();
        private Modal ErrorModal { get; set; }
        private string ErrorMessage { get; set; } = string.Empty;
        private List<string> ContainerFolder { get; set; } = new List<string>();

        public Containers()
        {
            NewContainer = new CreateContainerDto();
            EditingContainer = new UpdateContainerDto();
        }
        protected ValueTask SetToolbarItemsAsync()
        {
            Toolbar.AddButton(L["New Container"],
                OpenCreateContainerModal,
                IconName.Add,
                requiredPolicyName: SystemAdministrationPermissions.Containers.Create);

            return ValueTask.CompletedTask;
        }
        protected override async Task OnInitializedAsync()
        {
            await SetPermissionsAsync();
            await GetContainersAsync();
            await SetToolbarItemsAsync();
        }
        private async Task SetPermissionsAsync()
        {
            CanEditContainer = await AuthorizationService
                .IsGrantedAsync(SystemAdministrationPermissions.Containers.Edit);

            CanDeleteContainer = await AuthorizationService
                .IsGrantedAsync(SystemAdministrationPermissions.Containers.Delete);
        }
        private Task GetFolderNamesInFolderAsync()
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "BlobStorage");
            string[] directories = Directory.GetDirectories(folderPath);
            List<string> folderPaths = directories.ToList();
            ContainerFolder = folderPaths;
            return Task.CompletedTask;
        }
        public static string TruncateText(string text, int maxLength) // Cắt chuỗi
        {
            if (text.Length <= maxLength)
                return text;
            int keepLength = 11;

            // lấy index=keepleght đến phần đầu
            string firstPart = text.Substring(0, 7);

            // lấy index=0 đến phần giữa (text.length-keepLength)
            string lastPart = text.Substring(text.Length - keepLength);

            // Trả về chuỗi phần đầu+...+phần cuối
            return firstPart + "..." + lastPart;
        }
        private async Task GetContainersAsync()
        {
            var result = await ContainerAppService.GetListAsync(
                new GetContainerListDto
                {
                    MaxResultCount = PageSize,
                    SkipCount = (CurrentPage - 1) * PageSize,
                    Sorting = CurrentSorting
                }
            );
            ContainerList = result.Items;
            TotalCount = (int)result.TotalCount;

        }
        private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<ContainerDto> e)
        {
            CurrentSorting = e.Columns
                .Where(c => c.SortDirection != SortDirection.Default)
                .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
                .JoinAsString(",");
            CurrentPage = e.Page;
            await GetContainersAsync();
            await GetFolderNamesInFolderAsync();
            await InvokeAsync(StateHasChanged);

        }
        private Task OpenCreateContainerModal()
        {

            NewContainer = new CreateContainerDto();
            CreateContainerModal.Show();
            NewContainer.TypeStorage = "Azure";
            if (NewContainer.TypeStorage == "FileSystem")
            {
                NewContainer.BasePath = Path.Combine(Directory.GetCurrentDirectory(), "BlobStorage", "ImageFolder");
            }
            return Task.CompletedTask;
        }
        private void CloseCreateContainerModal()
        {
            CreateContainerModal.Hide();
        }
        private void OpenEditContainerModal(ContainerDto container)
        {
            EditingContainerId = container.Id;
            EditingContainer = ObjectMapper.Map<ContainerDto, UpdateContainerDto>(container);
            EditContainerModal.Show();
        }
        private async Task DeleteContainerAsync(ContainerDto container)
        {
            var confirmMessage = L["Container Deletion Confirmation Message", container.Name];
            if (!await Message.Confirm(confirmMessage))
            {
                return;
            }
            await ContainerAppService.DeleteAsync(container.Id);
            await _blob.UpdateBlobStorage();
            await GetContainersAsync();
        }
        private void CloseEditContainerModal()
        {
            EditContainerModal.Hide();
        }
        private async Task CreateContainerAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                await ContainerAppService.CreateAsync(NewContainer);
                await _blob.UpdateBlobStorage();

                await GetContainersAsync();
                CreateContainerModal.Hide();
            }
            catch (Exception ex)
            {
                ShowErrorModal("Tên đã tồn tại ");
            }
        }
        private async Task UpdateContainerAsync()
        {
            try
            {
                await ContainerAppService.UpdateAsync(EditingContainerId, EditingContainer);
                await _blob.UpdateBlobStorage();
                await GetContainersAsync();
                EditContainerModal.Hide();
            }
            catch (Exception ex)
            {
                ShowErrorModal("Tên đã tồn tại ");
            }
        }
        private void ShowErrorModal(string message)
        {
            ErrorMessage = message;
            ErrorModal.Show();
        }
        private void CloseErrorModal()
        {
            ErrorModal.Hide();
        }
    }
}
