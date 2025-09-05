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
    public record ChangePropertyPriceCommand(Guid PropertyId, decimal NewPrice) : IRequest<PropertyDto>;

    public class ChangePropertyPriceValidator : AbstractValidator<ChangePropertyPriceCommand>
    {
        public ChangePropertyPriceValidator() {
            RuleFor(x => x.PropertyId).NotEqual(Guid.Empty);
            RuleFor(x => x.NewPrice)
                .GreaterThan(0)
                .Must(v => decimal.Round(v, 2) == v)
                .WithMessage("NewPrice must have up to 2 decimals.");
        }
    }

    public class ChangePropertyPriceHandler
    : IRequestHandler<ChangePropertyPriceCommand, PropertyDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ChangePropertyPriceHandler(IUnitOfWork uow, IMapper mapper) {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PropertyDto> Handle(ChangePropertyPriceCommand request, CancellationToken ct) {
    
            var repo = _uow.Repository<Property>();
           
            var prop = await repo.Query()
                .FirstOrDefaultAsync(p => p.IdProperty == request.PropertyId, ct);

            if (prop is null)
                throw new NotFoundException($"Property does not exist.", request.PropertyId);

            if (prop.Price == request.NewPrice)
                return _mapper.Map<PropertyDto>(prop);

            prop.Price = request.NewPrice;
            repo.Update(prop);

            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<PropertyDto>(prop);
        }
    }
}   