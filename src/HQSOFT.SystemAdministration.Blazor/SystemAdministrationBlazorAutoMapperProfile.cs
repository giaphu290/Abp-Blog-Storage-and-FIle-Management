using AutoMapper;
using HQSOFT.CoreBackend.Companies;
using HQSOFT.CoreBackend.CompanyRoles;
using HQSOFT.CoreBackend.ExtendedUsers;
using HQSOFT.CoreBackend.HangfireConfigs;
using HQSOFT.CoreBackend.Modules;
using HQSOFT.CoreBackend.ReportParameters;
using HQSOFT.CoreBackend.Reports;
using HQSOFT.CoreBackend.Screens;
using HQSOFT.CoreBackend.SystemConfigurations;
using HQSOFT.CoreBackend.SystemSettings;
using HQSOFT.CoreBackend.UserCompanies;
using HQSOFT.CoreBackend.WorkspaceLinks;
using HQSOFT.CoreBackend.WorkspaceRoles;
using HQSOFT.CoreBackend.Workspaces;
using HQSOFT.CoreBackend.WorkspaceShortcuts;
using HQSOFT.CoreBackend.WorkspaceUsers;
using HQSOFT.SystemAdministration.Azurestorages;
using HQSOFT.SystemAdministration.Containers;

namespace HQSOFT.SystemAdministration.Blazor;

public class SystemAdministrationBlazorAutoMapperProfile : Profile
{
    public SystemAdministrationBlazorAutoMapperProfile()
    {
       
        CreateMap<CompanyDto, CompanyUpdateDto>();
        CreateMap<CompanyUpdateDto, CompanyCreateDto>();

        CreateMap<CompanyRoleDto, CompanyRoleUpdateDto>();
        CreateMap<CompanyRoleUpdateDto, CompanyRoleCreateDto>();

        CreateMap<UserCompanyDto, UserCompanyUpdateDto>();
        CreateMap<UserCompanyUpdateDto, UserCompanyCreateDto>();

        CreateMap<ModuleDto, ModuleUpdateDto>();
        CreateMap<ModuleUpdateDto, ModuleCreateDto>();

        CreateMap<ScreenDto, ScreenUpdateDto>();
        CreateMap<ScreenUpdateDto, ScreenCreateDto>();

        CreateMap<WorkspaceDto, WorkspaceUpdateDto>();
        CreateMap<WorkspaceUpdateDto, WorkspaceCreateDto>();

        CreateMap<WorkspaceShortcutDto, WorkspaceShortcutUpdateDto>();
        CreateMap<WorkspaceShortcutUpdateDto, WorkspaceShortcutCreateDto>();

        CreateMap<WorkspaceLinkDto, WorkspaceLinkUpdateDto>();
        CreateMap<WorkspaceLinkUpdateDto, WorkspaceLinkCreateDto>();

        CreateMap<ReportDto, ReportUpdateDto>();
        CreateMap<ReportUpdateDto, ReportCreateDto>();

        CreateMap<ReportParameterDto, ReportParameterUpdateDto>();
        CreateMap<ReportParameterUpdateDto, ReportParameterCreateDto>();

        CreateMap<ExtendedUserDto, ExtendedUserUpdateDto>();
        CreateMap<ExtendedUserDto, ExtendedUserCreateDto>();
        CreateMap<ExtendedUserUpdateDto, ExtendedUserCreateDto>();

        CreateMap<HangfireConfigDto, HangfireConfigUpdateDto>();
        CreateMap<HangfireConfigDto, HangfireConfigCreateDto>();
        CreateMap<HangfireConfigUpdateDto, HangfireConfigCreateDto>();

        CreateMap<WorkspaceUserDto, WorkspaceUserUpdateDto>();
        CreateMap<WorkspaceUserDto, WorkspaceUserCreateDto>();
        CreateMap<WorkspaceUserUpdateDto, WorkspaceUserCreateDto>();

        CreateMap<WorkspaceRoleDto, WorkspaceRoleUpdateDto>();
        CreateMap<WorkspaceRoleDto, WorkspaceRoleCreateDto>();
        CreateMap<WorkspaceRoleUpdateDto, WorkspaceRoleCreateDto>();

        CreateMap<SystemSettingDto, SystemSettingUpdateDto>();
        CreateMap<SystemSettingDto, SystemSettingCreateDto>();
        CreateMap<SystemSettingUpdateDto, SystemSettingCreateDto>();

        CreateMap<SystemConfigurationDto, SystemConfigurationUpdateDto>();
        CreateMap<SystemConfigurationDto, SystemConfigurationCreateDto>();
        CreateMap<SystemConfigurationUpdateDto, SystemConfigurationCreateDto>();


        CreateMap<ContainerDto, UpdateContainerDto>();
        CreateMap<AzurestorageDto, UpdateAzurestorageDto>();

    }
}
