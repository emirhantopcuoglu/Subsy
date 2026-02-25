namespace Subsy.Application.UserProfile.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator(IUserProfileService userProfileService)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği boş olamaz.");

        RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Kullanıcı adı boş olamaz.")
            .MaximumLength(64).WithMessage("Kullanıcı adı en fazla 64 karakter olabilir.")
            .MustAsync(async (command, userName, ct) =>
                !await userProfileService.IsUserNameTakenByAnotherUserAsync(command.UserId, userName, ct))
            .WithMessage("Bu kullanıcı adı zaten kullanılıyor.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("E-posta boş olamaz.")
            .MaximumLength(256).WithMessage("E-posta en fazla 256 karakter olabilir.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MustAsync(async (command, email, ct) =>
                !await userProfileService.IsEmailTakenByAnotherUserAsync(command.UserId, email, ct))
            .WithMessage("Bu e-posta adresi zaten kullanılıyor.");
    }
}
