namespace Subsy.Application.Common.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        Task InitializeProfileAsync(string userId, DateTime registeredAtUtc, CancellationToken cancellationToken = default);

        Task<bool> IsUserNameTakenByAnotherUserAsync(string userId, string userName, CancellationToken cancellationToken = default);

        Task<bool> IsEmailTakenByAnotherUserAsync(string userId, string email, CancellationToken cancellationToken = default);

        Task UpdateAsync(string userId, string userName, string email, CancellationToken cancellationToken = default);

        Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

        Task UpdateProfilePhotoAsync(
            string userId,
            string originalFileName,
            string contentType,
            byte[] fileBytes,
            CancellationToken cancellationToken = default);
    }
}
