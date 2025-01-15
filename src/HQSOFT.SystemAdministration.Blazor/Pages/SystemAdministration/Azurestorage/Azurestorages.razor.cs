using Blazorise.DataGrid;
using Blazorise;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using HQSOFT.SystemAdministration.Azurestorages;
using HQSOFT.SystemAdministration.Permissions;
using Volo.Abp;

namespace HQSOFT.SystemAdministration.Blazor.Pages.SystemAdministration.Azurestorage
{
    public partial class Azurestorages
    {
        private IReadOnlyList<AzurestorageDto> AzurestorageList { get; set; }
        IReadOnlyList<ContainerLookupDto> containerList = Array.Empty<ContainerLookupDto>();
        private int PageSize { get; } = LimitedResultRequestDto.DefaultMaxResultCount;
        private int CurrentPage { get; set; }
        private string CurrentSorting { get; set; }
        private int TotalCount { get; set; }

        private bool CanCreateAzurestorage { get; set; }
        private bool CanEditAzurestorage { get; set; }
        private bool CanDeleteAzurestorage { get; set; }

        private CreateAzurestorageDto NewAzurestorage { get; set; }

        private Guid EditingAzurestorageId { get; set; }
        private UpdateAzurestorageDto EditingAzurestorage { get; set; }

        private Modal CreateAzurestorageModal { get; set; }
        private Modal EditAzurestorageModal { get; set; }

        private Validations CreateValidationsRef;

        private Validations EditValidationsRef;
        private Modal ErrorModal { get; set; }
        private string ErrorMessage { get; set; } = string.Empty;
        public Azurestorages()
        {
            NewAzurestorage = new CreateAzurestorageDto();
            EditingAzurestorage = new UpdateAzurestorageDto();
        }

        protected override async Task OnInitializedAsync()
        {
            await SetPermissionsAsync();
            await GetAzurestoragesAsync();
            await ReloadContainerListAsync();
        }
        private async Task ReloadContainerListAsync()
        {
            containerList = (await AzurestorageAppService.GetContainerLookupAsync()).Items;
        }
        private async Task SetPermissionsAsync()
        {
            CanCreateAzurestorage = await AuthorizationService
                .IsGrantedAsync(SystemAdministrationPermissions.Azurestorages.Create);

            CanEditAzurestorage = await AuthorizationService
                .IsGrantedAsync(SystemAdministrationPermissions.Azurestorages.Edit);

            CanDeleteAzurestorage = await AuthorizationService
                .IsGrantedAsync(SystemAdministrationPermissions.Azurestorages.Delete);
        }

        private async Task GetAzurestoragesAsync()
        {
            var result = await AzurestorageAppService.GetListAsync(
                new GetAzurestorageListDto
                {
                    
                    MaxResultCount = PageSize,
                    SkipCount = CurrentPage * PageSize,
                    Sorting = CurrentSorting
                }
            );

            AzurestorageList = result.Items;
            TotalCount = (int)result.TotalCount;
        }

        private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<AzurestorageDto> e)
        {
            CurrentSorting = e.Columns
                .Where(c => c.SortDirection != SortDirection.Default)
                .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
                .JoinAsString(",");
            CurrentPage = e.Page - 1;

            await GetAzurestoragesAsync();
            await ReloadContainerListAsync();
            await InvokeAsync(StateHasChanged);
        }

        private void OpenCreateAzurestorageModal()
        {
            //if (!containerList.Any())
            //{
            //    throw new UserFriendlyException(message: L["AnContainerIsRequiredForCreatingAzurestorage"]);
            //}
            CreateValidationsRef.ClearAll();
            NewAzurestorage = new CreateAzurestorageDto();
            CreateAzurestorageModal.Show();
            NewAzurestorage.CreateContainerIfNotExists = false;
            NewAzurestorage.ContainerId = containerList.First().Id;
        }

        private void CloseCreateAzurestorageModal()
        {
            CreateAzurestorageModal.Hide();
        }

        private void OpenEditAzurestorageModal(AzurestorageDto azurstorage)
        {
            //if (!containerList.Any())
            //{
            //    throw new UserFriendlyException(message: L["AnContainerIsRequiredForCreatingAzurestorage"]);
            //}
            EditValidationsRef.ClearAll();
            EditingAzurestorageId = azurstorage.Id;
            EditingAzurestorage = ObjectMapper.Map<AzurestorageDto, UpdateAzurestorageDto>(azurstorage);
            EditAzurestorageModal.Show();
        }

        private async Task DeleteAzurestorageAsync(AzurestorageDto azurstorage)
        {
            var confirmMessage = L["AzurestorageDeletionConfirmationMessage", azurstorage.Name];
            if (!await Message.Confirm(confirmMessage))
            {
                return;
            }

            await AzurestorageAppService.DeleteAsync(azurstorage.Id);
            await GetAzurestoragesAsync();
            await ReloadContainerListAsync();

        }

        private void CloseEditAzurestorageModal()
        {
            EditAzurestorageModal.Hide();
        }

        private async Task CreateAzurestorageAsync()
        {
            try
            {
                if (await CreateValidationsRef.ValidateAll())
                {
                    await AzurestorageAppService.CreateAsync(NewAzurestorage);
                    await GetAzurestoragesAsync();
                    await ReloadContainerListAsync();

                    CreateAzurestorageModal.Hide();
                }
            }
            catch (Exception ex)
            {
                ShowErrorModal("Tên đã tồn tại");
            }
        }

        private async Task UpdateAzurestorageAsync()
        {
            try
            {
                if (await EditValidationsRef.ValidateAll())
                {
                    await AzurestorageAppService.UpdateAsync(EditingAzurestorageId, EditingAzurestorage);
                    await GetAzurestoragesAsync();
                    await ReloadContainerListAsync();

                    EditAzurestorageModal.Hide();
                }
            }catch(Exception ex)
            {
                ShowErrorModal("Tên đã tồn tại");
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
