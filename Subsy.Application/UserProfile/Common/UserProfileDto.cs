namespace Subsy.Application.UserProfile.Common;

public sealed class UserProfileDto
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
    public string? ProfilePhotoPath { get; init; }
    public string PreferredCurrency { get; init; } = "TRY";
}
