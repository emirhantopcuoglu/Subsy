using Subsy.Domain.Enums;

namespace Subsy.Application.Common;

public static class ServiceRegistry
{
    public sealed record ServiceInfo(string Name, string Domain, SubscriptionCategory Category);

    private static readonly Dictionary<string, ServiceInfo> _services = new(StringComparer.OrdinalIgnoreCase)
    {
        // Entertainment
        ["Netflix"] = new("Netflix", "netflix.com", SubscriptionCategory.Entertainment),
        ["Disney+"] = new("Disney+", "disneyplus.com", SubscriptionCategory.Entertainment),
        ["HBO Max"] = new("HBO Max", "max.com", SubscriptionCategory.Entertainment),
        ["Amazon Prime Video"] = new("Amazon Prime Video", "primevideo.com", SubscriptionCategory.Entertainment),
        ["BluTV"] = new("BluTV", "blutv.com", SubscriptionCategory.Entertainment),
        ["MUBI"] = new("MUBI", "mubi.com", SubscriptionCategory.Entertainment),
        ["Gain"] = new("Gain", "gain.tv", SubscriptionCategory.Entertainment),
        ["Crunchyroll"] = new("Crunchyroll", "crunchyroll.com", SubscriptionCategory.Entertainment),
        ["Apple TV+"] = new("Apple TV+", "tv.apple.com", SubscriptionCategory.Entertainment),

        // Music
        ["Spotify"] = new("Spotify", "spotify.com", SubscriptionCategory.Music),
        ["Apple Music"] = new("Apple Music", "music.apple.com", SubscriptionCategory.Music),
        ["YouTube Music"] = new("YouTube Music", "music.youtube.com", SubscriptionCategory.Music),
        ["Deezer"] = new("Deezer", "deezer.com", SubscriptionCategory.Music),
        ["Tidal"] = new("Tidal", "tidal.com", SubscriptionCategory.Music),
        ["Fizy"] = new("Fizy", "fizy.com", SubscriptionCategory.Music),

        // Software
        ["Adobe Creative Cloud"] = new("Adobe Creative Cloud", "adobe.com", SubscriptionCategory.Software),
        ["Microsoft 365"] = new("Microsoft 365", "microsoft.com", SubscriptionCategory.Software),
        ["JetBrains"] = new("JetBrains", "jetbrains.com", SubscriptionCategory.Software),
        ["GitHub"] = new("GitHub", "github.com", SubscriptionCategory.Software),
        ["Notion"] = new("Notion", "notion.so", SubscriptionCategory.Software),
        ["Figma"] = new("Figma", "figma.com", SubscriptionCategory.Software),
        ["Canva"] = new("Canva", "canva.com", SubscriptionCategory.Software),
        ["ChatGPT Plus"] = new("ChatGPT Plus", "openai.com", SubscriptionCategory.Software),
        ["Claude Pro"] = new("Claude Pro", "claude.ai", SubscriptionCategory.Software),
        ["Grammarly"] = new("Grammarly", "grammarly.com", SubscriptionCategory.Software),
        ["1Password"] = new("1Password", "1password.com", SubscriptionCategory.Software),
        ["Todoist"] = new("Todoist", "todoist.com", SubscriptionCategory.Software),

        // Gaming
        ["Xbox Game Pass"] = new("Xbox Game Pass", "xbox.com", SubscriptionCategory.Gaming),
        ["PlayStation Plus"] = new("PlayStation Plus", "playstation.com", SubscriptionCategory.Gaming),
        ["Nintendo Switch Online"] = new("Nintendo Switch Online", "nintendo.com", SubscriptionCategory.Gaming),
        ["EA Play"] = new("EA Play", "ea.com", SubscriptionCategory.Gaming),
        ["GeForce NOW"] = new("GeForce NOW", "nvidia.com", SubscriptionCategory.Gaming),

        // Cloud
        ["Google One"] = new("Google One", "one.google.com", SubscriptionCategory.Cloud),
        ["iCloud+"] = new("iCloud+", "icloud.com", SubscriptionCategory.Cloud),
        ["Dropbox"] = new("Dropbox", "dropbox.com", SubscriptionCategory.Cloud),

        // Education
        ["Coursera"] = new("Coursera", "coursera.org", SubscriptionCategory.Education),
        ["Udemy"] = new("Udemy", "udemy.com", SubscriptionCategory.Education),
        ["Duolingo"] = new("Duolingo", "duolingo.com", SubscriptionCategory.Education),
        ["LinkedIn Learning"] = new("LinkedIn Learning", "linkedin.com", SubscriptionCategory.Education),
        ["Cambly"] = new("Cambly", "cambly.com", SubscriptionCategory.Education),

        // News
        ["The New York Times"] = new("The New York Times", "nytimes.com", SubscriptionCategory.News),
        ["The Economist"] = new("The Economist", "economist.com", SubscriptionCategory.News),
        ["Blinkist"] = new("Blinkist", "blinkist.com", SubscriptionCategory.News),
        ["Dergilik"] = new("Dergilik", "dergilik.com.tr", SubscriptionCategory.News),

        // Health
        ["Strava"] = new("Strava", "strava.com", SubscriptionCategory.Health),
        ["Headspace"] = new("Headspace", "headspace.com", SubscriptionCategory.Health),
        ["Calm"] = new("Calm", "calm.com", SubscriptionCategory.Health),

        // Shopping
        ["Amazon Prime"] = new("Amazon Prime", "amazon.com.tr", SubscriptionCategory.Shopping),
        ["Trendyol"] = new("Trendyol", "trendyol.com", SubscriptionCategory.Shopping),
        ["Hepsiburada Premium"] = new("Hepsiburada Premium", "hepsiburada.com", SubscriptionCategory.Shopping),

        // YouTube Premium (standalone)
        ["YouTube Premium"] = new("YouTube Premium", "youtube.com", SubscriptionCategory.Entertainment),
    };

    public static IReadOnlyDictionary<string, ServiceInfo> All => _services;

    public static ServiceInfo? TryGet(string name)
    {
        if (_services.TryGetValue(name, out var info))
            return info;

        return null;
    }

    public static string? GetFaviconUrl(string serviceName)
    {
        var info = TryGet(serviceName);
        return info is not null
            ? $"https://www.google.com/s2/favicons?domain={info.Domain}&sz=64"
            : null;
    }

    public static IEnumerable<IGrouping<SubscriptionCategory, ServiceInfo>> GetGrouped()
    {
        return _services.Values
            .DistinctBy(s => s.Name)
            .OrderBy(s => s.Name)
            .GroupBy(s => s.Category);
    }
}
