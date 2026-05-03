using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Subsy.Application.Common;
using Subsy.Application.Common.Interfaces;
using Subsy.Web.Models;
using System.Security.Claims;

namespace Subsy.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IUserProfileService _userProfileService;
    private readonly IEmailService _emailService;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IUserProfileService userProfileService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userProfileService = userProfileService;
        _emailService = emailService;
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View();

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new IdentityUser { UserName = vm.UserName, Email = vm.Email };
        var result = await _userManager.CreateAsync(user, vm.Password);

        if (result.Succeeded)
        {
            await _userProfileService.InitializeProfileAsync(user.Id, DateTime.UtcNow);
            await SendConfirmationEmailAsync(user);
            return RedirectToAction(nameof(RegisterConfirmation), new { email = vm.Email });
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(vm);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult RegisterConfirmation(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return BadRequest();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            TempData["FlashError"] = "E-posta doğrulama bağlantısı geçersiz veya süresi dolmuş.";
            return RedirectToAction(nameof(Login));
        }

        TempData["FlashSuccess"] = "E-posta adresiniz doğrulandı. Giriş yapabilirsiniz.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResendConfirmation() => View();

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(string.Empty, "Geçerli bir e-posta adresi girin.");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is { EmailConfirmed: false })
            await SendConfirmationEmailAsync(user);

        // Always show the same message to avoid user enumeration
        TempData["FlashSuccess"] = "Eğer bu e-posta kayıtlıysa ve doğrulanmamışsa, doğrulama maili gönderildi.";
        return RedirectToAction(nameof(Login));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login() => View();

    [AllowAnonymous]
    [HttpPost]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            return View(vm);
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            ViewBag.UnconfirmedEmail = vm.Email;
            ModelState.AddModelError(string.Empty,
                "E-posta adresiniz henüz doğrulanmamış. Lütfen gelen kutunuzu kontrol edin.");
            return View(vm);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            vm.Password,
            vm.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await SendAdminLoginNotificationAsync(user);
            TempData["FlashInfo"] = $"Başarıyla giriş yapıldı. Hoşgeldin {user.UserName}!";
            return RedirectToAction("Index", "Home");
        }

        if (result.RequiresTwoFactor)
            return RedirectToAction(nameof(LoginWith2fa), new { vm.RememberMe });

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty,
                "Hesabınız çok fazla başarısız deneme nedeniyle geçici olarak kilitlendi. Lütfen 15 dakika sonra tekrar deneyin.");
            return View(vm);
        }

        ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
        return View(vm);
    }

    // ── 2FA Login ─────────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> LoginWith2fa(bool rememberMe)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null) return RedirectToAction(nameof(Login));

        return View(new LoginWith2faViewModel { RememberMe = rememberMe });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null) return RedirectToAction(nameof(Login));

        var code = vm.TwoFactorCode.Replace(" ", "").Replace("-", "");
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, vm.RememberMe, false);

        if (result.Succeeded)
        {
            await SendAdminLoginNotificationAsync(user);
            TempData["FlashInfo"] = $"Başarıyla giriş yapıldı. Hoşgeldin {user.UserName}!";
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Hesabınız kilitlendi.");
            return View(vm);
        }

        ModelState.AddModelError(string.Empty, "Geçersiz doğrulama kodu.");
        return View(vm);
    }

    // ── Forgot / Reset Password ───────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user is { EmailConfirmed: true })
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                nameof(ResetPassword), "Account",
                new { userId = user.Id, token },
                Request.Scheme)!;

            await _emailService.SendAsync(user.Email!, "Subsy — Parola Sıfırlama",
                BuildPasswordResetEmailBody(user.UserName ?? user.Email!, callbackUrl));
        }

        TempData["FlashSuccess"] = "Eğer bu e-posta kayıtlıysa, parola sıfırlama bağlantısı gönderildi.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return BadRequest();

        return View(new ResetPasswordViewModel { UserId = userId, Token = token });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByIdAsync(vm.UserId);
        if (user is null)
        {
            TempData["FlashError"] = "Geçersiz bağlantı.";
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ResetPasswordAsync(user, vm.Token, vm.NewPassword);
        if (result.Succeeded)
        {
            TempData["FlashSuccess"] = "Parolanız başarıyla sıfırlandı. Giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(vm);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["LogoutMessage"] = "Başarıyla çıkış yapıldı.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task SendConfirmationEmailAsync(IdentityUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = Url.Action(
            nameof(ConfirmEmail), "Account",
            new { userId = user.Id, token },
            Request.Scheme)!;

        var body = $"""
            <h2>Merhaba {user.UserName},</h2>
            <p>Subsy'e hoş geldiniz! Hesabınızı etkinleştirmek için e-posta adresinizi doğrulayın.</p>
            <p><a href="{callbackUrl}" style="background:#5865f2;color:#fff;padding:10px 20px;border-radius:6px;text-decoration:none;display:inline-block;">E-postayı Doğrula</a></p>
            <p style="color:#888;font-size:12px;">Eğer bu hesabı siz oluşturmadıysanız görmezden gelebilirsiniz.</p>
            """;

        await _emailService.SendAsync(user.Email!, "Subsy — E-posta Doğrulama", body);
    }

    private async Task SendAdminLoginNotificationAsync(IdentityUser user)
    {
        if (!await _userManager.IsInRoleAsync(user, Roles.Admin) || user.Email is null)
            return;

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "bilinmiyor";
        var time = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm") + " UTC";

        var body = $"""
            <h2>Admin Girişi Bildirimi</h2>
            <p>Merhaba <strong>{user.UserName}</strong>,</p>
            <p>Admin hesabınıza giriş yapıldı.</p>
            <ul>
                <li><strong>Zaman:</strong> {time}</li>
                <li><strong>IP:</strong> {ip}</li>
            </ul>
            <p style="color:#c0392b;">Eğer bu giriş size ait değilse lütfen hemen şifrenizi değiştirin.</p>
            """;

        try
        {
            await _emailService.SendAsync(user.Email, "Subsy — Admin Girişi Bildirimi", body);
        }
        catch { /* notification failure should not break login */ }
    }

    private static string BuildPasswordResetEmailBody(string userName, string callbackUrl) => $"""
        <h2>Merhaba {userName},</h2>
        <p>Parola sıfırlama isteği alındı.</p>
        <p><a href="{callbackUrl}" style="background:#5865f2;color:#fff;padding:10px 20px;border-radius:6px;text-decoration:none;display:inline-block;">Parolayı Sıfırla</a></p>
        <p style="color:#888;font-size:12px;">Bu bağlantı 24 saat geçerlidir. Eğer bu isteği siz yapmadıysanız görmezden gelebilirsiniz.</p>
        """;
}
