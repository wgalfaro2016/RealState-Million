using AutoMapper;
using FluentValidation;
using MediatR;
using RealEstate.Application.Dtos;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Commands
{
    public record CreateOwnerCommand(
        string Name,
        string? Address,
        string? Photo,
        DateTime? Birthday
    ) : IRequest<OwnerDto>;

    public class CreateOwnerValidator : AbstractValidator<CreateOwnerCommand>
    {
        public CreateOwnerValidator() {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Address).MaximumLength(250).When(x => !string.IsNullOrEmpty(x.Address));
            RuleFor(x => x.Photo).MaximumLength(300).When(x => !string.IsNullOrEmpty(x.Photo));
            RuleFor(x => x.Birthday).LessThanOrEqualTo(DateTime.Today).When(x => x.Birthday.HasValue);
        }
    }

    public class CreateOwnerHandler : IRequestHandler<CreateOwnerCommand, OwnerDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateOwnerHandler(IUnitOfWork uow, IMapper mapper) {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<OwnerDto> Handle(CreateOwnerCommand request, CancellationToken ct) {
            var repo = _uow.Repository<Owner>();

            var entity = new Owner {
                Name = request.Name,
                Address = request.Address ?? string.Empty,
                Photo = request.Photo ?? string.Empty,
                Birthday = request.Birthday
            };

            await repo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<OwnerDto>(entity);
        }
    }
}
