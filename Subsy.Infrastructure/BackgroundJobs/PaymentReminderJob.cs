using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Subsy.Application.Common.Interfaces;
using Subsy.Infrastructure.Persistence;

namespace Subsy.Infrastructure.BackgroundJobs;

public class PaymentReminderJob
{
    private readonly SubsyContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;

    public PaymentReminderJob(
        SubsyContext context,
        UserManager<IdentityUser> userManager,
        IEmailService emailService,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
        _dateTime = dateTime;
    }

    public async Task ExecuteAsync()
    {
        var tomorrow = _dateTime.Today.AddDays(1);

        // Yarın vadesi gelecek, arşivlenmemiş abonelikler
        var dueSoon = await _context.Subscriptions
            .Where(s => !s.IsArchived && s.RenewalDate.Date == tomorrow.Date)
            .ToListAsync();

        var grouped = dueSoon.GroupBy(s => s.UserId);

        foreach (var group in grouped)
        {
            var user = await _userManager.FindByIdAsync(group.Key);
            if (user?.Email is null) continue;

            var subscriptionNames = string.Join(", ", group.Select(s => s.Name));
            var totalAmount = group.Sum(s => s.Price);

            var subject = $"Subsy - Yarın {group.Count()} aboneliğinizin vadesi geliyor";
            var body = $"""
                <h2>Merhaba {user.UserName},</h2>
                <p>Yarın vadesi gelecek abonelikleriniz:</p>
                <ul>
                    {string.Join("", group.Select(s => $"<li><strong>{s.Name}</strong> — {s.Price:C}</li>"))}
                </ul>
                <p>Toplam: <strong>{totalAmount:C}</strong></p>
                <p>Subsy ile aboneliklerinizi takip edin.</p>
                """;

            await _emailService.SendAsync(user.Email, subject, body);
        }
    }
}