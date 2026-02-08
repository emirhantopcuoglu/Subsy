using Subsy.Models;
using System.Threading.Tasks;

namespace Subsy.Repositories
{
    public interface ISubscriptionRepository
    {
        Task<List<SubscriptionViewModel>> GetAllByUserId(string userId);
        Task<SubscriptionViewModel?> GetByIdAsync(int id);
        Task AddAsync(SubscriptionViewModel subscription);
        Task UpdateAsync(SubscriptionViewModel subscription);
        Task DeleteAsync(SubscriptionViewModel subscription);
        Task<decimal> GetTotalAmountAsync(string userId);
    }
}
