using api_backend.Dtos;
using api_backend.Models;
using AutoMapper;

namespace api_backend.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<CreateCustomerRequestDto, CustomerAddressEntity>();
            CreateMap<CreateCustomerRequestDto, CustomerEntity>()
                .ForMember(dest => dest.AddressId, opt => opt.Ignore());
          
            CreateMap<CustomerEntity, CustomerDto>();
        }
    }
}
