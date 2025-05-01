using Subsy.Models;

namespace Subsy.Services
{
    public interface ISubscriptionService
    {
        Task<List<Subscription>> GetAllByUserId(string userId);
        Task<Subscription?> GetByIdAsync(int id);
        Task AddAsync(Subscription subscription);
        Task UpdateAsync(Subscription subscription);
        Task DeleteAsync(Subscription subscription);
        Task<decimal> GetTotalAmountAsync(string userId);
    }
}
