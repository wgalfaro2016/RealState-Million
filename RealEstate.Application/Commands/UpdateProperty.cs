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
    public record UpdatePropertyCommand(
        Guid PropertyId,
        string? Name,
        string? Address,
        decimal? Price,
        string? CodeInternal,
        int? Year,
        Guid? IdOwner
    ) : IRequest<PropertyDto>;

    public class UpdatePropertyValidator : AbstractValidator<UpdatePropertyCommand>
    {
        public UpdatePropertyValidator() {
            RuleFor(x => x.PropertyId).NotEqual(Guid.Empty);

            RuleFor(x => x).Must(x =>
                   x.Name is not null
                || x.Address is not null
                || x.Price.HasValue
                || x.CodeInternal is not null
                || x.Year.HasValue
                || x.IdOwner.HasValue)
                .WithMessage("Provide at least one field to update.");

            When(x => x.Name is not null, () =>
                RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));

            When(x => x.Address is not null, () =>
                RuleFor(x => x.Address!).NotEmpty().MaximumLength(300));

            When(x => x.CodeInternal is not null, () =>
                RuleFor(x => x.CodeInternal!).NotEmpty().MaximumLength(50));

            When(x => x.Price.HasValue, () => {
                RuleFor(x => x.Price!.Value).GreaterThanOrEqualTo(0);

                RuleFor(x => x.Price!.Value)
                    .Must(v => decimal.Round(v, 2) == v)
                    .WithMessage("Price must have up to 2 decimals.");
            });

            When(x => x.Year.HasValue, () =>
                RuleFor(x => x.Year!.Value).InclusiveBetween(1800, 2100));

            When(x => x.IdOwner.HasValue, () =>
                RuleFor(x => x.IdOwner!.Value).NotEqual(Guid.Empty));
        }
    }

    public class UpdatePropertyHandler
        : IRequestHandler<UpdatePropertyCommand, PropertyDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UpdatePropertyHandler(IUnitOfWork uow, IMapper mapper) {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PropertyDto> Handle(UpdatePropertyCommand request, CancellationToken ct) {
            var propRepo = _uow.Repository<Property>();
            var ownerRepo = _uow.Repository<Owner>();

            var prop = await propRepo.Query()
                .FirstOrDefaultAsync(p => p.IdProperty == request.PropertyId, ct);

            if (prop is null)
                throw new NotFoundException($"Property does not exist.", request.PropertyId);

            if (request.IdOwner.HasValue && request.IdOwner.Value != prop.IdOwner) {
                var ownerExists = await ownerRepo.Query()
                    .AnyAsync(o => o.IdOwner == request.IdOwner.Value, ct);

                if (!ownerExists)
                    throw new NotFoundException($"Owner does not exist.", request.IdOwner);

                prop.IdOwner = request.IdOwner.Value;
            }

            if (request.Name is not null) prop.Name = request.Name;
            if (request.Address is not null) prop.Address = request.Address;
            if (request.CodeInternal is not null) prop.CodeInternal = request.CodeInternal;
            if (request.Year.HasValue) prop.Year = request.Year.Value;
            if (request.Price.HasValue) prop.Price = request.Price.Value;

            propRepo.Update(prop);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<PropertyDto>(prop);
        }
    }
}
