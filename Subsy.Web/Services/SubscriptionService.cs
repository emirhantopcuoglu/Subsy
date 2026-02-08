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

        public async Task<List<SubscriptionViewModel>> GetAllByUserId(string userId)
        {
            return await _subscriptionRepository.GetAllByUserId(userId);
        }
        public async Task<SubscriptionViewModel?> GetByIdAsync(int id)
        {
            return await _subscriptionRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(SubscriptionViewModel subscription)
        {
            if(subscription.RenewalDate < DateTime.Today)
            {
                throw new ArgumentException("Yenileme tarihi geçmiş olamaz.");
            }
            await _subscriptionRepository.AddAsync(subscription);
        }

        public async Task UpdateAsync(SubscriptionViewModel subscription)
        {
            if (subscription.RenewalDate < DateTime.Today)
            {
                throw new ArgumentException("Yenileme tarihi geçmiş olamaz.");
            }
            await _subscriptionRepository.UpdateAsync(subscription);
        }

        public async Task DeleteAsync(SubscriptionViewModel subscription)
        {
            await _subscriptionRepository.DeleteAsync(subscription);
        }

        public async Task<decimal> GetTotalAmountAsync(string userId)
        {
            return await _subscriptionRepository.GetTotalAmountAsync(userId);
        }
    }
}
