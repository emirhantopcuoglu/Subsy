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

        public Task<List<Subscription>> GetAllByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .ToListAsync(ct);
        }

        public async Task<Subscription?> GetByIdAsync(int id, string userId, CancellationToken ct)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        }

        public async Task AddAsync(Subscription subscription, CancellationToken ct = default)
        {
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Subscription subscription, CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Subscription subscription, CancellationToken ct = default)
        {
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync(ct);
        }
        public async Task<decimal> GetTotalPriceByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return await _context.Subscriptions
                .Where(x => x.UserId == userId && !x.IsArchived)
                .SumAsync(x => x.Price, ct);
        }

    }
}
