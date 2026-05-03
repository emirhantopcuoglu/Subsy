namespace Subsy.Application.Admin.Queries.GetAdminStats;

public record AdminStatsDto(
    int TotalUsers,
    int AdminUsers,
    int TotalSubscriptions,
    int ActiveSubscriptions,
    int NewUsersLast30Days);
