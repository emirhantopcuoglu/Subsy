namespace Subsy.Infrastructure.Settings;

public sealed class SupabaseStorageSettings
{
    /// <summary>Project URL, e.g. https://xyz.supabase.co</summary>
    public string Url { get; init; } = string.Empty;
    /// <summary>service_role secret key (server-side only, never expose to clients).</summary>
    public string ServiceKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
}
