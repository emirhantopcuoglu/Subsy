using Subsy.Models;

namespace Subsy.Services
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionViewModel>> GetAllByUserId(string userId);
        Task<SubscriptionViewModel?> GetByIdAsync(int id);
        Task AddAsync(SubscriptionViewModel subscription);
        Task UpdateAsync(SubscriptionViewModel subscription);
        Task DeleteAsync(SubscriptionViewModel subscription);
        Task<decimal> GetTotalAmountAsync(string userId);
    }
}
