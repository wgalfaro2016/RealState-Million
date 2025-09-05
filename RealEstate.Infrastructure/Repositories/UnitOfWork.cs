using Microsoft.Extensions.DependencyInjection;
using RealEstate.Application.Interfaces;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private readonly IServiceProvider _sp;

        public UnitOfWork(ApplicationDbContext db, IServiceProvider sp) {
            _db = db;
            _sp = sp;
        }

        public IGenericRepository<T> Repository<T>() where T : class =>
            _sp.GetRequiredService<IGenericRepository<T>>();

        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

        public void Dispose() => _db.Dispose();
    }
}
