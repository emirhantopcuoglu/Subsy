using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Infrastructure.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "exchange_rates_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public ExchangeRateService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<Dictionary<string, decimal>> GetRatesAsync(
        string baseCurrency, CancellationToken ct = default)
    {
        var cacheKey = CacheKey + baseCurrency.ToUpperInvariant();

        if (_cache.TryGetValue(cacheKey, out Dictionary<string, decimal>? cached) && cached is not null)
            return cached;

        var url = $"https://api.exchangerate-api.com/v4/latest/{baseCurrency.ToUpperInvariant()}";

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(json);
        var rates = new Dictionary<string, decimal>();

        foreach (var prop in doc.RootElement.GetProperty("rates").EnumerateObject())
        {
            rates[prop.Name] = prop.Value.GetDecimal();
        }

        _cache.Set(cacheKey, rates, CacheDuration);
        return rates;
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