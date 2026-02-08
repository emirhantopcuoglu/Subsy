using Microsoft.EntityFrameworkCore;
using Subsy.Application.Common.Interfaces;
using Subsy.Domain.Entities;
using Subsy.Infrastructure.Persistence;

namespace Subsy.Infrastructure.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly SubsyContext _context;

        public SubscriptionRepository(SubsyContext context)
        {
            _context = context;
        }

        public Task<List<Subscription>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        //public async Task<Subscription> GetByIdAsync(int id)
        //{
        //    return await _context.Subscriptions.FindAsync(id);
        //}

        //public async Task AddAsync(Subscription subscription)
        //{
        //    await _context.Subscriptions.AddAsync(subscription);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task UpdateAsync(Subscription subscription)
        //{
        //    _context.Subscriptions.Update(subscription);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task DeleteAsync(Subscription subscription)
        //{
        //    _context.Subscriptions.Remove(subscription);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task<decimal> GetTotalAmountAsync(string userId)
        //{
        //    var total = await _context.Subscriptions
        //        .Where(s => s.UserId == userId)
        //        .Select(s => (double)s.Price)
        //        .SumAsync();

        //    return ((decimal)total);
        //}
    }
}
