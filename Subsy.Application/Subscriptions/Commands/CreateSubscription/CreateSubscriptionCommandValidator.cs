using FluentValidation;
using Subsy.Domain.Enums;

namespace Subsy.Application.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
    {
        private static readonly int[] AllowedRenewalPeriods = { 7, 15, 30, 90, 180, 365 };

        public CreateSubscriptionCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId boş olamaz.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Abonelik adı boş olamaz.")
                .MaximumLength(100).WithMessage("Abonelik adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Para birimi boş olamaz.");

            RuleFor(x => x.RenewalPeriodDays)
                .Must(x => AllowedRenewalPeriods.Contains(x))
                .WithMessage("RenewalPeriodDays sadece 7, 15, 30, 90, 180 veya 365 olabilir.");

            RuleFor(x => x.SelectedMonth)
                .InclusiveBetween(1, 12)
                .WithMessage("Month 1 ile 12 arasında olmalıdır.");

            RuleFor(x => x.SelectedDay)
                .InclusiveBetween(1, 31)
                .WithMessage("Day 1 ile 31 arasında olmalıdır.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Geçersiz kategori.");

            RuleFor(x => x.WebsiteUrl)
                .MaximumLength(500).WithMessage("URL en fazla 500 karakter olabilir.")
                .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == "http" || u.Scheme == "https"))
                .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl))
                .WithMessage("Geçerli bir URL giriniz.");
        }
    }
}