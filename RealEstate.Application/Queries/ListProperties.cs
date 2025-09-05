using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Dtos;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Queries
{
    public record ListOfPropertiesQuery(
      string? Name = null,
      string? Address = null,
      string? CodeInternal = null,
      Guid? OwnerId = null,
      decimal? MinPrice = null,
      decimal? MaxPrice = null,
      int? MinYear = null,
      int? MaxYear = null,
      bool OnlyWithImages = false,
      bool IncludeAllImages = true,  
      string? SortBy = "price",
      bool Desc = false,
      int Page = 1,
      int PageSize = 20
  ) : IRequest<PagedResultDto<PropertyListItemDto>>;


    public class ListPropertiesValidator : AbstractValidator<ListOfPropertiesQuery>
    {
        public ListPropertiesValidator() {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 200);

            When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue, () => {
                RuleFor(x => x).Must(x => x.MinPrice! <= x.MaxPrice!)
                    .WithMessage("MinPrice must be <= MaxPrice.");
            });

            When(x => x.MinYear.HasValue && x.MaxYear.HasValue, () => {
                RuleFor(x => x).Must(x => x.MinYear! <= x.MaxYear!)
                    .WithMessage("MinYear must be <= MaxYear.");
            });
        }
    }

    public class ListPropertiesHandler
       : IRequestHandler<ListOfPropertiesQuery, PagedResultDto<PropertyListItemDto>>
    {
        private readonly IUnitOfWork _uow;

        public ListPropertiesHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PagedResultDto<PropertyListItemDto>> Handle(ListOfPropertiesQuery q, CancellationToken ct) {
            var repo = _uow.Repository<Property>();

            IQueryable<Property> query = repo.Query().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q.Name))
                query = query.Where(p => p.Name.Contains(q.Name));

            if (!string.IsNullOrWhiteSpace(q.Address))
                query = query.Where(p => p.Address.Contains(q.Address));

            if (!string.IsNullOrWhiteSpace(q.CodeInternal))
                query = query.Where(p => p.CodeInternal.Contains(q.CodeInternal));

            if (q.OwnerId.HasValue)
                query = query.Where(p => p.IdOwner == q.OwnerId.Value);

            if (q.MinPrice.HasValue)
                query = query.Where(p => p.Price >= q.MinPrice.Value);

            if (q.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= q.MaxPrice.Value);

            if (q.MinYear.HasValue)
                query = query.Where(p => p.Year >= q.MinYear.Value);

            if (q.MaxYear.HasValue)
                query = query.Where(p => p.Year <= q.MaxYear.Value);

            if (q.OnlyWithImages)
                query = query.Where(p => p.Images.Any());

            query = (q.SortBy?.ToLowerInvariant(), q.Desc) switch {
                ("name", false) => query.OrderBy(p => p.Name),
                ("name", true) => query.OrderByDescending(p => p.Name),
                ("year", false) => query.OrderBy(p => p.Year),
                ("year", true) => query.OrderByDescending(p => p.Year),
                ("price", true) => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Price) 
            };

            var total = await query.CountAsync(ct);

            var skip = (q.Page - 1) * q.PageSize;

            var items = await query
                .Skip(skip)
                .Take(q.PageSize)
                .Select(p => new PropertyListItemDto(
                    p.IdProperty,
                    p.Name,
                    p.Address,
                    p.Price,
                    p.CodeInternal,
                    p.Year,
                    p.IdOwner,
                    p.Images.Where(i => i.IsCover).Select(i => i.Url).FirstOrDefault()
                        ?? p.Images.Select(i => i.Url).FirstOrDefault(), 
                    q.IncludeAllImages
                        ? p.Images.Select(i => i.Url).ToList()
                        : new List<string>() 
                ))
                .ToListAsync(ct);

            return new PagedResultDto<PropertyListItemDto>(q.Page, q.PageSize, total, items);
        }
    }
}
