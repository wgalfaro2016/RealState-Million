using AutoMapper;
using FluentValidation;
using MediatR;
using RealEstate.Application.Common.Exceptions;
using RealEstate.Application.Dtos;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Commands
{
    public record CreatePropertyCommand(string Name, string Address, decimal Price, string CodeInternal, int Year, Guid IdOwner) : IRequest<PropertyDto>;

    public class CreatePropertyValidator : AbstractValidator<CreatePropertyCommand>
    {
        public CreatePropertyValidator() {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Address).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.CodeInternal).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Year).InclusiveBetween(1900, DateTime.UtcNow.Year + 1);
            RuleFor(x => x.IdOwner).NotEqual(Guid.Empty);
        }
    }

    public class CreatePropertyHandler : IRequestHandler<CreatePropertyCommand, PropertyDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreatePropertyHandler(IUnitOfWork uow, IMapper mapper) 
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PropertyDto> Handle(CreatePropertyCommand request, CancellationToken ct) {
            var repo = _uow.Repository<Property>();
            var codeAlreadyExists = repo.Query().Any(p => p.CodeInternal == request.CodeInternal);
            if (codeAlreadyExists)
                throw new NotFoundException($"CodeInternal already exists.", request.CodeInternal);

            var entity = new Property {
                Name = request.Name,
                Address = request.Address,
                Price = request.Price,
                CodeInternal = request.CodeInternal,
                Year = request.Year,
                IdOwner = request.IdOwner
            };

            await repo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<PropertyDto>(entity);
        }
    }
}
