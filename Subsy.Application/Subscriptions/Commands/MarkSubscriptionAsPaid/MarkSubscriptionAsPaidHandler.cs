using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.MarkSubscriptionAsPaid;

public sealed class MarkSubscriptionAsPaidHandler
{
    private readonly ISubscriptionRepository _repo;

    public MarkSubscriptionAsPaidHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task HandleAsync(
        MarkSubscriptionAsPaidCommand cmd,
        CancellationToken ct = default)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, ct);
        if (subscription is null)
            throw new KeyNotFoundException("Subscription not found.");


        if (subscription.UserId != cmd.UserId)
            throw new UnauthorizedAccessException();


        if (subscription.RenewalDate.Date > DateTime.Today)
            throw new InvalidOperationException("Ödeme günü henüz gelmedi.");

        if (!int.TryParse(subscription.RenewalPeriod, out var days))
            throw new InvalidOperationException("Geçersiz yenileme periyodu.");

        subscription.RenewalDate = subscription.RenewalDate.AddDays(days);

        await _repo.UpdateAsync(subscription, ct);
    }
}
