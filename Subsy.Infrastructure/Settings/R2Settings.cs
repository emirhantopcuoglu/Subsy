namespace Subsy.Infrastructure.Settings;

public sealed class R2Settings
{
    public string AccountId { get; init; } = string.Empty;
    public string AccessKeyId { get; init; } = string.Empty;
    public string SecretAccessKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
    /// <summary>Public base URL for serving files (e.g. R2 custom domain or r2.dev URL).</summary>
    public string PublicUrl { get; init; } = string.Empty;
}
