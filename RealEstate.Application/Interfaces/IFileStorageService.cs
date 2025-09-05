namespace RealEstate.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SavePropertyImageAsync(Guid propertyId, IFileResource file, CancellationToken ct);
    }
}
