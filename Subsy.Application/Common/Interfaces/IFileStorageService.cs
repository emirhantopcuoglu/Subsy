namespace Subsy.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <returns>Public URL of the uploaded file.</returns>
    Task<string> UploadAsync(string key, byte[] data, string contentType, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
