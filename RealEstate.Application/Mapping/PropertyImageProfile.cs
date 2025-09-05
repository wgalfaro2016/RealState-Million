using AutoMapper;
using RealEstate.Application.Dtos;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Mapping
{

    public class PropertyImageProfile : Profile
    {
        public PropertyImageProfile() {
            CreateMap<PropertyImage, PropertyImageDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.IdPropertyImage));
        }
    }
}
