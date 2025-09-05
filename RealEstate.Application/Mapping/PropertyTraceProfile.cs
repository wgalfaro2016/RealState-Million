using AutoMapper;
using RealEstate.Application.Commands;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Mapping
{
    public class PropertyTraceProfile : Profile
    {
        public PropertyTraceProfile() {
            CreateMap<PropertyTrace, PropertyTraceDto>();
        }
    }
}
