using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Infrastructure.BackgroundJobs;

public class PaymentReminderJob
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;

    public PaymentReminderJob(
        ISubscriptionRepository subscriptions,
        UserManager<IdentityUser> userManager,
        IEmailService emailService,
        IDateTimeProvider dateTime)
    {
        _subscriptions = subscriptions;
        _userManager = userManager;
        _emailService = emailService;
        _dateTime = dateTime;
    }

    public async Task ExecuteAsync()
    {
        var tomorrow = _dateTime.Today.AddDays(1);

        var dueSoon = await _subscriptions.GetDueOnDateAsync(tomorrow);
        if (dueSoon.Count == 0)
            return;

        var userIds = dueSoon.Select(s => s.UserId).Distinct().ToList();
        var users = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var userMap = users.ToDictionary(u => u.Id);

        foreach (var group in dueSoon.GroupBy(s => s.UserId))
        {
            if (!userMap.TryGetValue(group.Key, out var user) || user.Email is null)
                continue;

            var subject = $"Subsy - Yarın {group.Count()} aboneliğinizin vadesi geliyor";
            var body = $"""
                <h2>Merhaba {user.UserName},</h2>
                <p>Yarın vadesi gelecek abonelikleriniz:</p>
                <ul>
                    {string.Join("", group.Select(s => $"<li><strong>{s.Name}</strong> — {s.Price:N2} {s.Currency}</li>"))}
                </ul>
                <p>Subsy ile aboneliklerinizi takip edin.</p>
                """;

            await _emailService.SendAsync(user.Email, subject, body);
        }
    }
}
