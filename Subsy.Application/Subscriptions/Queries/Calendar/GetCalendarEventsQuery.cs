using MediatR;

namespace Subsy.Application.Subscriptions.Queries.Calendar;

public sealed record GetCalendarEventsQuery(
    string UserId,
    DateTime Start,
    DateTime End,
    bool IncludeArchived
) : IRequest<List<CalendarEventDto>>;