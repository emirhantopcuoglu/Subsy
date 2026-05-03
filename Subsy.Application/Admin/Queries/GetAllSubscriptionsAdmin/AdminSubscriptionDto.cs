namespace Subsy.Application.Admin.Queries.GetAllSubscriptionsAdmin;

public record AdminSubscriptionDto(
    int Id,
    string UserId,
    string UserName,
    string UserEmail,
    string Name,
    decimal Price,
    string Currency,
    int RenewalPeriodDays,
    DateTime RenewalDate,
    bool IsArchived,
    DateTime CreatedAt);
