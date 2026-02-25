namespace Subsy.Application.UserProfile.Queries.GetUserProfile;

public sealed class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IUserProfileService _userProfileService;

    public GetUserProfileHandler(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        return await _userProfileService.GetByUserIdAsync(request.UserId, cancellationToken);
    }
}
