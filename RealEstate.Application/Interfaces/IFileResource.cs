namespace RealEstate.Application.Interfaces
{
    public interface IFileResource
    {
        string FileName { get; }
        long Length { get; }
        Stream OpenReadStream();
    }
}
