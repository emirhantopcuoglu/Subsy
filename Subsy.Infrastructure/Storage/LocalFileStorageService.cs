using Microsoft.AspNetCore.Hosting;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> UploadAsync(string key, byte[] data, string contentType, CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(_environment.WebRootPath, "images", "profiles");
        Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(Path.Combine(directory, key), data, cancellationToken);

        return $"/images/profiles/{key}";
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_environment.WebRootPath, "images", "profiles", key);
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }
}
