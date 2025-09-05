using Microsoft.EntityFrameworkCore;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Repositories
{
    public class PropertyRepository : GenericRepository<Property>
    {
        public PropertyRepository(ApplicationDbContext db) : base(db) { }

        public async Task<Property?> GetWithIncludesAsync(Guid id, CancellationToken ct) =>
            await Set
                .Include(p => p.Images)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.IdProperty == id, ct);

        public IQueryable<Property> QueryWithIncludes() =>
            Set.Include(p => p.Images)
               .Include(p => p.Owner)
               .AsNoTracking();
    }
}
