namespace RealEstate.Infrastructure.Services
{
    public class FileStorageOptions
    {
        public string WebRootPath { get; set; } = "";
        public string BaseRelativePath { get; set; } = "uploads/properties";
    }
}
