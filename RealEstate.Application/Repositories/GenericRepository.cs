using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using System.Collections.Generic;
using RealEstate.Infrastructure.Persistence;


namespace RealEstate.Application.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DbContext _db;
        protected DbSet<T> Set => _db.Set<T>();

        public GenericRepository(ApplicationDbContext db) {
            _db = db;
            Set = db.Set<T>(); 
        }

        public Task<T?> GetByIdAsync(object id, CancellationToken ct) => Set.FindAsync(new[] { id }, ct).AsTask();
        public async Task AddAsync(T entity, CancellationToken ct) => await Set.AddAsync(entity, ct);
        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct) => await Set.AddRangeAsync(entities, ct);
        public void Update(T entity) => Set.Update(entity);
        public void Remove(T entity) => Set.Remove(entity);
        public IQueryable<T> Query() => Set.AsQueryable();
        public Task<bool> AnyAsync(CancellationToken ct) => Set.AnyAsync(ct);
    }
}
