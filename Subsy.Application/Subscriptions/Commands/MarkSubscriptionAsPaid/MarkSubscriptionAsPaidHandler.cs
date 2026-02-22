using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Commands.MarkSubscriptionAsPaid;

public sealed class MarkSubscriptionAsPaidHandler
    : IRequestHandler<MarkSubscriptionAsPaidCommand, Unit>
{
    private readonly ISubscriptionRepository _repo;

    private static readonly int[] AllowedRenewalPeriods = { 7, 15, 30, 90, 180, 365 };

    public MarkSubscriptionAsPaidHandler(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Unit> Handle(MarkSubscriptionAsPaidCommand cmd, CancellationToken ct)
    {
        var subscription = await _repo.GetByIdAsync(cmd.Id, cmd.UserId, ct);
        if (subscription is null)
            throw new KeyNotFoundException();

        if (subscription.UserId != cmd.UserId)
            throw new UnauthorizedAccessException();

        if (subscription.RenewalDate.Date > DateTime.Today)
            throw new InvalidOperationException("Ödeme günü henüz gelmedi.");

        var days = subscription.RenewalPeriodDays;

        if (!AllowedRenewalPeriods.Contains(days))
            throw new InvalidOperationException("Geçersiz yenileme periyodu.");

        subscription.RenewalDate = subscription.RenewalDate.AddDays(days);
        await _repo.UpdateAsync(subscription, ct);

        return Unit.Value;
    }
}