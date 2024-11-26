using AutoMapper;
using Entities.Models;
using Shared.DataTransferObjects;

namespace CodeMaze.API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //**Note:
            //**Using record based dto use ForCtorParam instead of ForMember
            //CreateMap<Company, CompanyDto>()
            //    .ForCtorParam("FullAddress", opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
            CreateMap<Company, CompanyDto>()
                .ForMember(c => c.FullAddress,
                opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));

            CreateMap<Employee, EmployeeDto>();

            CreateMap<CompanyForCreationDto, Company>();

            CreateMap<EmployeeForCreationDto, Employee>();

            CreateMap<EmployeeForUpdateDto, Employee>()
                .ReverseMap();
        }
    }
}
