using FluentValidation;

namespace Subsy.Application.Subscriptions.Queries.Calendar;

public sealed class GetCalendarEventsQueryValidator : AbstractValidator<GetCalendarEventsQuery>
{
    private const int MaxRangeDays = 366;

    public GetCalendarEventsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId boş olamaz.");

        RuleFor(x => x.End)
            .GreaterThanOrEqualTo(x => x.Start)
            .WithMessage("Bitiş tarihi başlangıç tarihinden önce olamaz.");

        RuleFor(x => x)
            .Must(x => (x.End - x.Start).TotalDays <= MaxRangeDays)
            .WithMessage($"Takvim aralığı en fazla {MaxRangeDays} gün olabilir.");
    }
}
