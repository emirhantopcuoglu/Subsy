using Subsy.Domain.Entities;

namespace Subsy.Application.Common.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<List<Subscription>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        Task<List<Subscription>> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        Task<Subscription?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

        Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);

        Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);

        Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken = default);

        Task<decimal> GetTotalPriceByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        Task<List<Subscription>> GetDueOnDateAsync(DateTime date, CancellationToken cancellationToken = default);

        Task<int> GetActiveCountAsync(string userId, DateTime fromDate, CancellationToken cancellationToken = default);

        Task<int> GetDueCountOnDateAsync(string userId, DateTime date, CancellationToken cancellationToken = default);

        Task<decimal> GetTotalInPeriodAsync(string userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);

        Task<List<Subscription>> GetUpcomingAsync(string userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    }
}
