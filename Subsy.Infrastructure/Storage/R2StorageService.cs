using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Subsy.Application.Common.Interfaces;
using Subsy.Infrastructure.Settings;

namespace Subsy.Infrastructure.Storage;

public sealed class R2StorageService : IFileStorageService
{
    private readonly R2Settings _settings;

    public R2StorageService(IOptions<R2Settings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<string> UploadAsync(string key, byte[] data, string contentType, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();

        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            ContentType = contentType,
            InputStream = new MemoryStream(data),
            DisablePayloadSigning = true
        };

        await client.PutObjectAsync(request, cancellationToken);

        return $"{_settings.PublicUrl.TrimEnd('/')}/{key}";
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();

        await client.DeleteObjectAsync(_settings.BucketName, key, cancellationToken);
    }

    private AmazonS3Client CreateClient()
    {
        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{_settings.AccountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true
        };

        return new AmazonS3Client(_settings.AccessKeyId, _settings.SecretAccessKey, config);
    }
}
