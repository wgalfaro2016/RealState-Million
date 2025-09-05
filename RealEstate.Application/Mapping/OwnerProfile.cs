using AutoMapper;
using RealEstate.Application.Dtos;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Mapping
{
    public class OwnerProfile : Profile
    {
        public OwnerProfile() {
            CreateMap<Owner, OwnerDto>();
        }
    }
}
