namespace Subsy.Application.Subscriptions.Queries.Calendar;

public sealed class CalendarEventDto
{
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Start { get; set; } = default!;
    public bool AllDay { get; set; } = true;
    public string? Color { get; set; }
    public string? TextColor { get; set; }
    public object? ExtendedProps { get; set; }
}