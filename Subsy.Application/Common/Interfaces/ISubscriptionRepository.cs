using Subsy.Domain.Entities;

namespace Subsy.Application.Common.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<List<Subscription>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        //Task<Subscription?> GetByIdAsync(int id);
        //Task AddAsync(Subscription subscription);
        //Task UpdateAsync(Subscription subscription);
        //Task DeleteAsync(Subscription subscription);
        //Task<decimal> GetTotalAmountAsync(string userId);
    }
}
