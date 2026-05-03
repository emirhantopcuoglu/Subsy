namespace Subsy.Application.Admin.Queries.GetAllUsers;

public record AdminUserDto(
    string Id,
    string UserName,
    string Email,
    bool IsAdmin,
    bool IsBlocked,
    int SubscriptionCount);
