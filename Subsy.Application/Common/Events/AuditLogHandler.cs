using MediatR;
using Subsy.Application.Common.Interfaces;
using Subsy.Domain.Entities;

namespace Subsy.Application.Common.Events;

public class AuditLogHandler :
    INotificationHandler<SubscriptionCreatedEvent>,
    INotificationHandler<SubscriptionPaidEvent>,
    INotificationHandler<SubscriptionArchivedEvent>,
    INotificationHandler<SubscriptionUnarchivedEvent>,
    INotificationHandler<SubscriptionUpdatedEvent>,
    INotificationHandler<SubscriptionDeletedEvent>
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly IDateTimeProvider _dateTime;

    public AuditLogHandler(IAuditLogRepository auditRepo, IDateTimeProvider dateTime)
    {
        _auditRepo = auditRepo;
        _dateTime = dateTime;
    }

    public async Task Handle(SubscriptionCreatedEvent e, CancellationToken ct)
    {
        var log = AuditLog.Create(
            e.UserId, "SubscriptionCreated", "Subscription", e.SubscriptionId,
            $"Created: {e.SubscriptionName}", _dateTime.UtcNow);
        await _auditRepo.AddAsync(log, ct);
    }

    public async Task Handle(SubscriptionPaidEvent e, CancellationToken ct)
    {
        var log = AuditLog.Create(
            e.UserId, "SubscriptionPaid", "Subscription", e.SubscriptionId,
            $"Paid: {e.SubscriptionName} (next: {e.NewRenewalDate:yyyy-MM-dd})", _dateTime.UtcNow);
        await _auditRepo.AddAsync(log, ct);
    }

    public async Task Handle(SubscriptionArchivedEvent e, CancellationToken ct)
    {
        var log = AuditLog.Create(
            e.UserId, "SubscriptionArchived", "Subscription", e.SubscriptionId,
            $"Archived: {e.SubscriptionName}", _dateTime.UtcNow);
        await _auditRepo.AddAsync(log, ct);
    }

    public async Task Handle(SubscriptionUnarchivedEvent e, CancellationToken ct)
    {
        var log = AuditLog.Create(
            e.UserId, "SubscriptionUnarchived", "Subscription", e.SubscriptionId,
            $"Unarchived: {e.SubscriptionName}", _dateTime.UtcNow);
        await _auditRepo.AddAsync(log, ct);
    }

    public async Task Handle(SubscriptionUpdatedEvent e, CancellationToken ct)
    {
        var log = AuditLog.Create(
            e.UserId, "SubscriptionUpdated", "Subscription", e.SubscriptionId,
            $"Updated: {e.SubscriptionName}", _dateTime.UtcNow);
        await _auditRepo.AddAsync(log, ct);
    }

    public async Task Handle(SubscriptionDeletedEvent e, CancellationToken ct)
    {
        var log = AuditLog.Create(
            e.UserId, "SubscriptionDeleted", "Subscription", e.SubscriptionId,
            $"Deleted: {e.SubscriptionName}", _dateTime.UtcNow);
        await _auditRepo.AddAsync(log, ct);
    }
}