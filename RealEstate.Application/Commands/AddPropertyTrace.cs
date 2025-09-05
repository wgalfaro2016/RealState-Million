using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common.Exceptions;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Commands
{
    public record PropertyTraceDto(Guid IdPropertyTrace, Guid IdProperty, DateTime DateSale, string Name, decimal Value, decimal Tax);

    public record AddPropertyTraceCommand(
        Guid PropertyId,
        string Name,
        decimal Value,
        decimal Tax,
        DateTime? DateSale = null
    ) : IRequest<PropertyTraceDto>;

    public class AddPropertyTraceValidator : AbstractValidator<AddPropertyTraceCommand>
    {
        public AddPropertyTraceValidator() {
            RuleFor(x => x.PropertyId).NotEqual(Guid.Empty);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Tax).GreaterThanOrEqualTo(0);

            RuleFor(x => x.Value)
                .Must(v => decimal.Round(v, 2) == v)
                .WithMessage("Value must have up to 2 decimals.");
            RuleFor(x => x.Tax)
                .Must(v => decimal.Round(v, 2) == v)
                .WithMessage("Tax must have up to 2 decimals.");
        }
    }

    public class AddPropertyTraceHandler : IRequestHandler<AddPropertyTraceCommand, PropertyTraceDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AddPropertyTraceHandler(IUnitOfWork uow, IMapper mapper) {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PropertyTraceDto> Handle(AddPropertyTraceCommand req, CancellationToken ct) {

            var propExists = await _uow.Repository<Property>()
                .Query()
                .AnyAsync(p => p.IdProperty == req.PropertyId, ct);

            if (!propExists)
                throw new NotFoundException($"Property does not exist.", req.PropertyId);

            var entity = new PropertyTrace {
                IdPropertyTrace = Guid.NewGuid(),
                IdProperty = req.PropertyId,
                Name = req.Name,
                Value = req.Value,
                Tax = req.Tax,
                DateSale = req.DateSale ?? DateTime.UtcNow
            };

            await _uow.Repository<PropertyTrace>().AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<PropertyTraceDto>(entity);
        }
    }
}
