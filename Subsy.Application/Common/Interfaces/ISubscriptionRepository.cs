using Subsy.Domain.Entities;

namespace Subsy.Application.Common.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<List<Subscription>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        Task<Subscription?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

        Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);

        Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);

        Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken = default);

        Task<decimal> GetTotalPriceByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        Task<List<Subscription>> GetDueOnDateAsync(DateTime date, CancellationToken cancellationToken = default);
    }
}
