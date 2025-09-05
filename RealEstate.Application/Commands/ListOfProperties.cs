using AutoMapper;
using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Application.Dtos;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Commands
{
    public record ListPropertiesQuery(
    string? Name,
    string? CodeInternal,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? YearFrom,
    int? YearTo,
    Guid? OwnerId,
    int Page = 1,
    int PageSize = 20
    ) : IRequest<IReadOnlyList<PropertyDto>>;

    public class ListPropertiesHandler
    : IRequestHandler<ListPropertiesQuery, IReadOnlyList<PropertyDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ListPropertiesHandler(IUnitOfWork uow, IMapper mapper) {
            _uow = uow; _mapper = mapper;
        }

        public Task<IReadOnlyList<PropertyDto>> Handle(ListPropertiesQuery request, CancellationToken ct) {
            var repo = _uow.Repository<Property>();
            var q = repo.Query();

            
            if (!string.IsNullOrWhiteSpace(request.Name))
                q = q.Where(p => p.Name.Contains(request.Name));

            if (!string.IsNullOrWhiteSpace(request.CodeInternal))
                q = q.Where(p => p.CodeInternal == request.CodeInternal);

            if (request.MinPrice is > 0)
                q = q.Where(p => p.Price >= request.MinPrice);

            if (request.MaxPrice is > 0)
                q = q.Where(p => p.Price <= request.MaxPrice);

            if (request.YearFrom.HasValue)
                q = q.Where(p => p.Year >= request.YearFrom.Value);

            if (request.YearTo.HasValue)
                q = q.Where(p => p.Year <= request.YearTo.Value);

            if (request.OwnerId.HasValue && request.OwnerId.Value != Guid.Empty)
                q = q.Where(p => p.IdOwner == request.OwnerId.Value);

            
            q = q.OrderByDescending(p => p.Year).ThenBy(p => p.Name);

            
            var page = request.Page < 1 ? 1 : request.Page;
            var size = request.PageSize <= 0 ? 20 : request.PageSize;

            
            var data = q.Skip((page - 1) * size)
                        .Take(size)
                        .Select(p => new PropertyDto(
                            p.IdProperty,
                            p.Name,
                            p.Address,
                            p.Price,
                            p.CodeInternal,
                            p.Year,
                            p.IdOwner
                        ))
                        .ToList();

            return Task.FromResult<IReadOnlyList<PropertyDto>>(data);
        }
    }
}
