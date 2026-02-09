using Subsy.Domain.Entities;

namespace Subsy.Application.Common.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<List<Subscription>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        Task<Subscription?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
        Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);

        //Task DeleteAsync(Subscription subscription);
        Task<decimal> GetTotalPriceByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    }
}
