namespace Subsy.Domain.Entities;

public class UserProfile
{
    public string UserId { get; set; } = default!;

    public DateTime RegisteredAt { get; set; }

    public string? ProfilePhotoPath { get; set; }
}
