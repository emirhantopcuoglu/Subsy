using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Subsy.Application.Common.Interfaces;
using Subsy.Infrastructure.Settings;

namespace Subsy.Infrastructure.Storage;

public sealed class SupabaseStorageService : IFileStorageService
{
    private readonly HttpClient _http;
    private readonly SupabaseStorageSettings _settings;

    public SupabaseStorageService(HttpClient http, IOptions<SupabaseStorageSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
    }

    public async Task<string> UploadAsync(string key, byte[] data, string contentType, CancellationToken cancellationToken = default)
    {
        var url = $"{_settings.Url.TrimEnd('/')}/storage/v1/object/{_settings.BucketName}/{key}";

        using var content = new ByteArrayContent(data);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await _http.PostAsync(url, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Supabase upload failed {(int)response.StatusCode}: {body}");
        }

        return $"{_settings.Url.TrimEnd('/')}/storage/v1/object/public/{_settings.BucketName}/{key}";
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var url = $"{_settings.Url.TrimEnd('/')}/storage/v1/object/{_settings.BucketName}";

        var body = JsonSerializer.Serialize(new { prefixes = new[] { key } });
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Delete, url) { Content = content };
        var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
