using Subsy.Models;
using Subsy.Repositories;

namespace Subsy.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        public SubscriptionService(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<List<Subscription>> GetAllByUserId(string userId)
        {
            return await _subscriptionRepository.GetAllByUserId(userId);
        }
        public async Task<Subscription?> GetByIdAsync(int id)
        {
            return await _subscriptionRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Subscription subscription)
        {
            if(subscription.RenewalDate < DateTime.Today)
            {
                throw new ArgumentException("Yenileme tarihi geçmiş olamaz.");
            }
            await _subscriptionRepository.AddAsync(subscription);
        }

        public async Task UpdateAsync(Subscription subscription)
        {
            if (subscription.RenewalDate < DateTime.Today)
            {
                throw new ArgumentException("Yenileme tarihi geçmiş olamaz.");
            }
            await _subscriptionRepository.UpdateAsync(subscription);
        }

        public async Task DeleteAsync(Subscription subscription)
        {
            await _subscriptionRepository.DeleteAsync(subscription);
        }

        public async Task<decimal> GetTotalAmountAsync(string userId)
        {
            return await _subscriptionRepository.GetTotalAmountAsync(userId);
        }
    }
}
