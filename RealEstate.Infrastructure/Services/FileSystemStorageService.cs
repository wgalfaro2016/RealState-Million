using Microsoft.Extensions.Options;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Services;

public class FileSystemStorageService : IFileStorageService
{
    private readonly FileStorageOptions _opt;

    public FileSystemStorageService(IOptions<FileStorageOptions> opt)
        => _opt = opt.Value;

    public async Task<string> SavePropertyImageAsync(Guid propertyId, IFileResource file, CancellationToken ct) {
        var webRoot = string.IsNullOrWhiteSpace(_opt.WebRootPath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
            : _opt.WebRootPath;

        var dir = Path.Combine(webRoot, _opt.BaseRelativePath, propertyId.ToString("N"));
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(dir, name);

        await using var output = File.Create(path);
        await using var input = file.OpenReadStream();
        await input.CopyToAsync(output, ct);

        var url = $"/{_opt.BaseRelativePath.Replace("\\", "/")}/{propertyId:N}/{name}";
        return url;
    }
}
