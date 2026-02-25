namespace Subsy.Application.UserProfile.Commands.UpdateUserProfilePhoto;

public sealed class UpdateUserProfilePhotoCommandValidator : AbstractValidator<UpdateUserProfilePhotoCommand>
{
    private static readonly string[] AllowedContentTypes =
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public UpdateUserProfilePhotoCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği boş olamaz.");

        RuleFor(x => x.OriginalFileName)
            .NotEmpty().WithMessage("Dosya adı boş olamaz.");

        RuleFor(x => x.ContentType)
            .Must(x => AllowedContentTypes.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Sadece JPG, PNG veya WEBP formatı yüklenebilir.");

        RuleFor(x => x.FileBytes)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Dosya içeriği boş olamaz.")
            .Must(x => x.Length > 0).WithMessage("Dosya içeriği boş olamaz.")
            .Must(x => x.Length <= 2 * 1024 * 1024).WithMessage("Dosya boyutu en fazla 2 MB olabilir.");
    }
}
