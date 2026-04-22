namespace Subsy.Application.Common.Interfaces;

public interface IExchangeRateService
{
    Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency,
        CancellationToken ct = default);
    Task<Dictionary<string, decimal>> GetRatesAsync(string baseCurrency,
        CancellationToken ct = default);
}