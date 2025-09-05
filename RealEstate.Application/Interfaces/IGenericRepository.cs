namespace RealEstate.Application.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id, CancellationToken ct);
        Task AddAsync(T entity, CancellationToken ct);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct);
        void Update(T entity);
        void Remove(T entity);

        IQueryable<T> Query();       
        Task<bool> AnyAsync(CancellationToken ct);
    }
}
