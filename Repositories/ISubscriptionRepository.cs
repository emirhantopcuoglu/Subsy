using Subsy.Models;
using System.Threading.Tasks;

namespace Subsy.Repositories
{
    public interface ISubscriptionRepository
    {
        Task<List<Subscription>> GetAllByUserId(string userId);
        Task<Subscription?> GetByIdAsync(int id);
        Task AddAsync(Subscription subscription);
        Task UpdateAsync(Subscription subscription);
        Task DeleteAsync(Subscription subscription);
        Task<decimal> GetTotalAmountAsync(string userId);
    }
}
