using MediatR;
using Subsy.Application.Common.Interfaces;

namespace Subsy.Application.Subscriptions.Queries.Calendar;

public sealed class GetCalendarEventsHandler : IRequestHandler<GetCalendarEventsQuery, List<CalendarEventDto>>
{
    private readonly ISubscriptionRepository _repo;
    public GetCalendarEventsHandler(ISubscriptionRepository repo) => _repo = repo;

    public async Task<List<CalendarEventDto>> Handle(GetCalendarEventsQuery q, CancellationToken ct)
    {
        var subs = await _repo.GetAllByUserIdAsync(q.UserId, ct);

        if (!q.IncludeArchived)
            subs = subs.Where(s => s.IsArchived == false).ToList();

        var today = DateTime.Today;
        var rangeStart = q.Start.Date;
        var rangeEnd = q.End.Date;

        var result = new List<CalendarEventDto>();

        foreach (var s in subs)
        {
            var period = s.RenewalPeriodDays <= 0 ? 0 : s.RenewalPeriodDays;
            var d = s.RenewalDate.Date;

            if (period > 0 && d < rangeStart)
            {
                var diffDays = (rangeStart - d).Days;
                var steps = diffDays / period;
                d = d.AddDays(steps * period);
                while (d < rangeStart) d = d.AddDays(period);
            }

            while (d <= rangeEnd)
            {
                var color = GetColor(s.IsArchived == true, d, today);

                result.Add(new CalendarEventDto
                {
                    Id = $"{s.Id}:{d:yyyyMMdd}",
                    Title = $"{s.Name} • {s.Price:0.##} ₺",
                    Start = d.ToString("yyyy-MM-dd"),
                    AllDay = true,
                    Color = color.bg,
                    TextColor = color.text,
                    ExtendedProps = new
                    {
                        subscriptionId = s.Id,
                        name = s.Name,
                        price = s.Price,
                        renewalDate = d.ToString("yyyy-MM-dd"),
                        renewalPeriodDays = s.RenewalPeriodDays,
                        isArchived = s.IsArchived == true,
                        status = color.status,
                        isPayable = (d.Date <= today.Date) && (s.IsArchived == false)
                    }
                });

                if (period == 0) break;
                d = d.AddDays(period);
            }
        }

        return result;
    }

    private static (string bg, string text, string status) GetColor(bool archived, DateTime date, DateTime today)
    {
        if (archived) return ("#9CA3AF", "#111827", "Archived");          // gray
        if (date < today) return ("#EF4444", "#111827", "Overdue");       // red
        if (date == today) return ("#F59E0B", "#111827", "Today");        // amber
        if (date <= today.AddDays(7)) return ("#EAB308", "#111827", "Soon"); // yellow
        return ("#6366F1", "#111827", "Upcoming");                        // indigo
    }
}