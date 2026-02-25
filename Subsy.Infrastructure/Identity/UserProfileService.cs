using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Subsy.Application.Common.Interfaces;
using Subsy.Application.UserProfile.Common;
using Subsy.Domain.Entities;
using Subsy.Infrastructure.Persistence;

namespace Subsy.Infrastructure.Identity;

public sealed class UserProfileService : IUserProfileService
{
    private static readonly HashSet<string> AllowedPhotoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedPhotoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly SubsyContext _context;
    private readonly IWebHostEnvironment _environment;

    public UserProfileService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        SubsyContext context,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _environment = environment;
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
            ProfilePhotoPath = profile.ProfilePhotoPath
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
            ProfilePhotoPath = null
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

        // Identity'nin doğru akışı
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
        if (!AllowedPhotoContentTypes.Contains(contentType))
            throw new InvalidOperationException("Sadece JPG, PNG veya WebP yükleyebilirsiniz.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var profile = await EnsureProfileExistsAsync(userId, cancellationToken);

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(ext))
        {
            ext = contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ? ".jpg"
                : contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) ? ".png"
                : ".webp";
        }

        ext = ext.ToLowerInvariant();
        if (!AllowedPhotoExtensions.Contains(ext))
            throw new InvalidOperationException("Geçersiz dosya uzantısı.");

        var fileName = $"{Guid.NewGuid():N}{ext}";

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            throw new InvalidOperationException("Web root klasörü bulunamadı.");

        var photosDirectory = Path.Combine(webRoot, "images", "profiles");
        Directory.CreateDirectory(photosDirectory);

        var fullPath = Path.Combine(photosDirectory, fileName);

        try
        {
            await File.WriteAllBytesAsync(fullPath, fileBytes, cancellationToken);

            // Eski dosyayı sil
            if (!string.IsNullOrWhiteSpace(profile.ProfilePhotoPath))
            {
                var oldFileName = Path.GetFileName(profile.ProfilePhotoPath);
                if (!string.IsNullOrWhiteSpace(oldFileName))
                {
                    var oldPath = Path.Combine(photosDirectory, oldFileName);
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }
            }

            profile.ProfilePhotoPath = $"/images/profiles/{fileName}";
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            throw;
        }
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