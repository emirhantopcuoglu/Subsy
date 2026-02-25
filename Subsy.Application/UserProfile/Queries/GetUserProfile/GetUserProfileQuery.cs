namespace Subsy.Application.UserProfile.Queries.GetUserProfile;

public sealed record GetUserProfileQuery(string UserId) : IRequest<UserProfileDto?>;
