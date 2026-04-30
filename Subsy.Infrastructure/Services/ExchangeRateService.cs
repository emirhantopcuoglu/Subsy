using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Infrastructure.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExchangeRateService> _logger;

    private const string CacheKeyPrefix = "exchange_rates_";
    private const string StaleCacheKeyPrefix = "stale_exchange_rates_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    // Prevents simultaneous refreshes when the cache expires (cache stampede guard)
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    public ExchangeRateService(HttpClient httpClient, IMemoryCache cache, ILogger<ExchangeRateService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Dictionary<string, decimal>> GetRatesAsync(
        string baseCurrency, CancellationToken ct = default)
    {
        var key = CacheKeyPrefix + baseCurrency.ToUpperInvariant();
        var staleKey = StaleCacheKeyPrefix + baseCurrency.ToUpperInvariant();

        if (_cache.TryGetValue(key, out Dictionary<string, decimal>? cached) && cached is not null)
            return cached;

        await RefreshLock.WaitAsync(ct);
        try
        {
            // Double-check: another thread may have refreshed while we waited
            if (_cache.TryGetValue(key, out cached) && cached is not null)
                return cached;

            var url = $"https://api.exchangerate-api.com/v4/latest/{baseCurrency.ToUpperInvariant()}";
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            var rates = new Dictionary<string, decimal>();

            foreach (var prop in doc.RootElement.GetProperty("rates").EnumerateObject())
                rates[prop.Name] = prop.Value.GetDecimal();

            _cache.Set(key, rates, CacheDuration);
            _cache.Set(staleKey, rates); // no expiry — serves as indefinite fallback
            return rates;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Exchange rate fetch failed for {Currency}; attempting stale cache fallback", baseCurrency);

            if (_cache.TryGetValue(staleKey, out Dictionary<string, decimal>? stale) && stale is not null)
                return stale;

            _logger.LogError(
                "No stale exchange rates available for {Currency}; currency conversion will be skipped", baseCurrency);
            return new Dictionary<string, decimal>();
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    public async Task<decimal> ConvertAsync(
        decimal amount, string fromCurrency, string toCurrency, CancellationToken ct = default)
    {
        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
            return amount;

        var rates = await GetRatesAsync(fromCurrency, ct);

        if (!rates.TryGetValue(toCurrency.ToUpperInvariant(), out var rate))
            throw new InvalidOperationException(
                $"Exchange rate not found: {fromCurrency} -> {toCurrency}");

        return Math.Round(amount * rate, 2);
    }
}
