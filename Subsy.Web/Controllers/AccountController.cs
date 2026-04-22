using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Subsy.Application.Common.Interfaces;
using Subsy.Web.Models;
namespace Subsy.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUserProfileService _userProfileService;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IUserProfileService userProfileService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userProfileService = userProfileService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() { return View(); }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }

            var user = new IdentityUser { UserName = registerViewModel.UserName, Email = registerViewModel.Email };
            var result = await _userManager.CreateAsync(user, registerViewModel.Password);

            if (result.Succeeded)
            {
                await _userProfileService.InitializeProfileAsync(user.Id, DateTime.UtcNow);
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["RegisterMessage"] = $"Kayıt işlemi başarıyla tamamlandı. Hoşgeldin {user.UserName}!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var errorr in result.Errors)
            {
                ModelState.AddModelError(string.Empty, errorr.Description);
            }

            return View(registerViewModel);
        }
        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login() { return View(); }

        [AllowAnonymous]
        [HttpPost]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid) return View(loginViewModel);

            var user = await _userManager.FindByEmailAsync(loginViewModel.Email);

            if (user is not null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    loginViewModel.Password,
                    loginViewModel.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    TempData["FlashInfo"] = $"Başarıyla giriş yapıldı. Hoşgeldin {user.UserName}!";
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty,
                        "Hesabınız çok fazla başarısız deneme nedeniyle geçici olarak kilitlendi. Lütfen 15 dakika sonra tekrar deneyin.");
                    return View(loginViewModel);
                }
            }

            ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            return View(loginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["LogoutMessage"] = "Başarıyla çıkış yapıldı.";
            return RedirectToAction("Login", "Account");
        }
    }
}
