using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common.Exceptions;
using RealEstate.Application.Dtos;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Commands
{
    public record AddPropertyImageCommand(Guid PropertyId, string Url, bool IsCover = false)
        : IRequest<PropertyImageDto>;

    public class AddPropertyImageValidator : AbstractValidator<AddPropertyImageCommand>
    {
        public AddPropertyImageValidator() {
            RuleFor(x => x.PropertyId).NotEqual(Guid.Empty);
            RuleFor(x => x.Url).NotEmpty().MaximumLength(300);
        }
    }

    public class AddPropertyImageHandler : IRequestHandler<AddPropertyImageCommand, PropertyImageDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AddPropertyImageHandler(IUnitOfWork uow, IMapper mapper) {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PropertyImageDto> Handle(AddPropertyImageCommand request, CancellationToken ct) 
        {
            var propertyExists = await _uow.Repository<Property>()
                .Query()
                .AnyAsync(p => p.IdProperty == request.PropertyId, ct);

            if (!propertyExists)
                throw new NotFoundException($"Property does not exist.", request.PropertyId);

            if (request.IsCover) {
                var imgRepo = _uow.Repository<PropertyImage>();
                var covers = await imgRepo.Query()
                    .Where(i => i.PropertyId == request.PropertyId && i.IsCover)
                    .ToListAsync(ct);

                if (covers.Count > 0) {
                    foreach (var c in covers) {
                        c.IsCover = false;
                        imgRepo.Update(c);
                    }
                }
            }

            var entity = new PropertyImage {
                PropertyId = request.PropertyId,
                Url = request.Url,
                IsCover = request.IsCover
            };

            await _uow.Repository<PropertyImage>().AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<PropertyImageDto>(entity);
        }
    }
}
