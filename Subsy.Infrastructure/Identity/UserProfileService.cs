using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.UserProfile.Common;
using Subsy.Domain.Entities;
using Subsy.Infrastructure.Persistence;

namespace Subsy.Infrastructure.Identity;

public sealed class UserProfileService : IUserProfileService
{

    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly SubsyContext _context;
    private readonly IFileStorageService _fileStorage;

    public UserProfileService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        SubsyContext context,
        IFileStorageService fileStorage)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<UserProfileDto?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return null;

        var profile = await EnsureProfileExistsAsync(userId, cancellationToken);

        return new UserProfileDto
        {
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            RegisteredAt = profile.RegisteredAt,
            ProfilePhotoPath = profile.ProfilePhotoPath,
            PreferredCurrency = profile.PreferredCurrency
        };
    }

    public async Task InitializeProfileAsync(string userId, DateTime registeredAtUtc, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (existing is not null)
            return;

        _context.UserProfiles.Add(new UserProfile
        {
            UserId = userId,
            RegisteredAt = registeredAtUtc,
            ProfilePhotoPath = null,
            PreferredCurrency = "TRY"
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsUserNameTakenByAnotherUserAsync(string userId, string userName, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = _userManager.NormalizeName(userName);

        return await _userManager.Users
            .AsNoTracking()
            .AnyAsync(x => x.NormalizedUserName == normalizedUserName && x.Id != userId, cancellationToken);
    }

    public async Task<bool> IsEmailTakenByAnotherUserAsync(string userId, string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = _userManager.NormalizeEmail(email);

        return await _userManager.Users
            .AsNoTracking()
            .AnyAsync(x => x.NormalizedEmail == normalizedEmail && x.Id != userId, cancellationToken);
    }

    public async Task UpdateAsync(string userId, string userName, string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var setUserName = await _userManager.SetUserNameAsync(user, userName);
        if (!setUserName.Succeeded)
            throw new InvalidOperationException(string.Join(" ", setUserName.Errors.Select(x => x.Description)));

        var setEmail = await _userManager.SetEmailAsync(user, email);
        if (!setEmail.Succeeded)
            throw new InvalidOperationException(string.Join(" ", setEmail.Errors.Select(x => x.Description)));

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(x => x.Description)));

        await _signInManager.RefreshSignInAsync(user);
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errorText = string.Join(" ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(errorText)
                ? "Şifre değiştirilemedi."
                : errorText);
        }

        await _signInManager.RefreshSignInAsync(user);
    }

    public async Task UpdateProfilePhotoAsync(
        string userId,
        string originalFileName,
        string contentType,
        byte[] fileBytes,
        CancellationToken cancellationToken = default)
    {
        if (!IsAllowedImageSignature(fileBytes))
            throw new InvalidOperationException("Sadece JPG, PNG veya WebP yükleyebilirsiniz.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var profile = await EnsureProfileExistsAsync(userId, cancellationToken);

        var processedBytes = await ResizeToWebpAsync(fileBytes, size: 256);

        // Deterministic key per user — always overwrites the previous photo, no cleanup needed.
        var key = $"{userId}.webp";

        var publicUrl = await _fileStorage.UploadAsync(key, processedBytes, "image/webp", cancellationToken);

        profile.ProfilePhotoPath = publicUrl;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<byte[]> ResizeToWebpAsync(byte[] source, int size)
    {
        using var image = Image.Load(source);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Crop
        }));

        using var output = new MemoryStream();
        await image.SaveAsync(output, new WebpEncoder { Quality = 80 });
        return output.ToArray();
    }

    private static bool IsAllowedImageSignature(byte[] bytes)
    {
        if (bytes.Length < 12)
            return false;

        // JPEG: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return true;

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return true;

        // WebP: RIFF????WEBP
        if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
            return true;

        return false;
    }

    private async Task<UserProfile> EnsureProfileExistsAsync(string userId, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (profile is not null)
            return profile;

        profile = new UserProfile
        {
            UserId = userId,
            RegisteredAt = DateTime.UtcNow,
            ProfilePhotoPath = null
        };

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        return profile;
    }
}