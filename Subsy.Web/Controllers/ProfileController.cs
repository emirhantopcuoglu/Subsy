using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.UserProfile.Commands.ChangeUserPassword;
using Subsy.Application.UserProfile.Commands.UpdateUserProfile;
using Subsy.Application.UserProfile.Commands.UpdateUserProfilePhoto;
using Subsy.Application.UserProfile.Queries.GetUserProfile;
using Subsy.Web.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Subsy.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private const long MaxPhotoBytes = 2 * 1024 * 1024; // 2MB
        private static readonly HashSet<string> AllowedPhotoContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private readonly IMediator _mediator;

        public ProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var vm = await BuildProfileViewModelAsync(ct);
            if (vm is null)
                return NotFound();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfo(ProfileViewModel vm, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                await _mediator.Send(new UpdateUserProfileCommand(userId, vm.UserName, vm.Email), ct);
                TempData["FlashSuccess"] = "Profil bilgileriniz güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException vex)
            {
                AddValidationErrors(vex);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            await PopulateReadonlyFieldsAsync(vm, ct);
            return View("Index", vm);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ProfileViewModel vm, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            try
            {
                await _mediator.Send(new ChangeUserPasswordCommand(
                    userId,
                    vm.CurrentPassword,
                    vm.NewPassword,
                    vm.ConfirmNewPassword), ct);

                TempData["FlashSuccess"] = "Parolanız başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException vex)
            {
                AddValidationErrors(vex);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            vm.CurrentPassword = string.Empty;
            vm.NewPassword = string.Empty;
            vm.ConfirmNewPassword = string.Empty;

            await PopulateReadonlyFieldsAsync(vm, ct);
            return View("Index", vm);
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile? photo, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            if (photo is null || photo.Length == 0)
            {
                TempData["FlashError"] = "Lütfen bir profil fotoğrafı seçin.";
                return RedirectToAction(nameof(Index));
            }

            if (photo.Length > MaxPhotoBytes)
            {
                TempData["FlashError"] = "Dosya boyutu en fazla 2MB olabilir.";
                return RedirectToAction(nameof(Index));
            }

            if (!AllowedPhotoContentTypes.Contains(photo.ContentType))
            {
                TempData["FlashError"] = "Sadece JPG, PNG veya WebP yükleyebilirsiniz.";
                return RedirectToAction(nameof(Index));
            }

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await photo.CopyToAsync(ms, ct);
                fileBytes = ms.ToArray();
            }

            try
            {
                await _mediator.Send(new UpdateUserProfilePhotoCommand(
                    userId,
                    photo.FileName,
                    photo.ContentType,
                    fileBytes), ct);

                TempData["FlashSuccess"] = "Profil fotoğrafınız güncellendi.";
            }
            catch (ValidationException vex)
            {
                TempData["FlashError"] = vex.Message;
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["FlashError"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<ProfileViewModel?> BuildProfileViewModelAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var dto = await _mediator.Send(new GetUserProfileQuery(userId), ct);
            if (dto is null)
                return null;

            return new ProfileViewModel
            {
                UserName = dto.UserName,
                Email = dto.Email,
                RegisteredAt = dto.RegisteredAt,
                ProfilePhotoPath = string.IsNullOrWhiteSpace(dto.ProfilePhotoPath)
                    ? "/images/user-placeholder.png"
                    : dto.ProfilePhotoPath
            };
        }

        private async Task PopulateReadonlyFieldsAsync(ProfileViewModel vm, CancellationToken ct)
        {
            var hydrated = await BuildProfileViewModelAsync(ct);
            if (hydrated is null)
                return;

            vm.RegisteredAt = hydrated.RegisteredAt;
            vm.ProfilePhotoPath = hydrated.ProfilePhotoPath;
        }

        private void AddValidationErrors(ValidationException vex)
        {
            ModelState.AddModelError(string.Empty, vex.Message);
        }
    }
}