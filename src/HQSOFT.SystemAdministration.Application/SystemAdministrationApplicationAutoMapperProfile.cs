using AutoMapper;
using DevExpress.XtraReports.Templates;
using HQSOFT.SystemAdministration.Azurestorages;
using HQSOFT.SystemAdministration.Containers;
namespace HQSOFT.SystemAdministration;

public class SystemAdministrationApplicationAutoMapperProfile : Profile
{
    public SystemAdministrationApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<Container, ContainerDto>();
        CreateMap<Azurestorage, AzurestorageDto>()
        .ForMember(dest => dest.Name, opt => opt.Ignore());
        CreateMap<Container, ContainerLookupDto>();
    }
}
