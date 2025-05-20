using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Subsy.Models;
using Subsy.Services;
using System.Globalization;
using System.Text.Json;

namespace Subsy.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _service;
        private readonly UserManager<IdentityUser> _userManager;

        public SubscriptionController(ISubscriptionService service, UserManager<IdentityUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return View("Index", "Home");

            var userId = _userManager.GetUserId(User);
            var subs = await _service.GetAllByUserId(userId);
            var today = DateTime.Today;

            ViewBag.ActiveCount = subs.Count(s => s.RenewalDate >= today && !s.IsArchived);
            ViewBag.TodayDueCount = subs.Count(s => s.RenewalDate == today && !s.IsArchived);
            ViewBag.TotalThisMonth = subs
                .Where(s => s.RenewalDate.Month == today.Month && s.RenewalDate.Year == today.Year && !s.IsArchived)
                .Sum(s => s.Price);

            ViewBag.Upcoming = subs
                .Where(s => s.RenewalDate <= today.AddDays(3) && s.RenewalDate >= today && !s.IsArchived)
                .ToList();

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Active()
        {
            var userId = _userManager.GetUserId(User);
            var subscriptions = await _service.GetAllByUserId(userId);
            var active = subscriptions.Where(s => !s.IsArchived).ToList();
            return View(active);
        }

        [HttpGet]
        public async Task<IActionResult> Due()
        {
            var userId = _userManager.GetUserId(User);
            var list = await _service.GetAllByUserId(userId);
            var due = list.Where(s => s.RenewalDate == DateTime.Today && !s.IsArchived).ToList();
            return View(due);
        }

        [HttpGet]
        public async Task<IActionResult> Archive()
        {
            var userId = _userManager.GetUserId(User);
            var list = await _service.GetAllByUserId(userId);
            var archived = list.Where(s => s.IsArchived).ToList();
            return View(archived);
        }

        [HttpPost]
        public async Task<IActionResult> Archive(int id)
        {
            var sub = await _service.GetByIdAsync(id);
            if (sub == null || sub.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            sub.IsArchived = true;
            await _service.UpdateAsync(sub);
            TempData["ArchiveMessage"] = $"{sub.Name} aboneliği başarıyla sonlandırıldı.";
            return RedirectToAction("Active");
        }

        [HttpPost]
        public async Task<IActionResult> Unarchive(int id)
        {
            var sub = await _service.GetByIdAsync(id);
            if (sub == null || sub.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            sub.IsArchived = false;
            await _service.UpdateAsync(sub);
            TempData["ArchiveMessage"] = $"{sub.Name} aboneliği başarıyla aktifleştirildi.";
            return RedirectToAction("Archive");
        }

        public async Task<IActionResult> Calendar()
        {
            var userId = _userManager.GetUserId(User);

            var subs = await _service.GetAllByUserId(userId);

            subs = subs.Where(s => !s.IsArchived).ToList();

            var events = subs.Select(s => new
            {
                title = $"{s.Name} – {s.Price:0.##}₺",
                start = s.RenewalDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            });

            ViewBag.Events = JsonSerializer.Serialize(events);
            return View(); 
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var subscription = await _service.GetByIdAsync(id);
            var userId = _userManager.GetUserId(User);

            if (subscription == null || subscription.UserId != userId)
                return Unauthorized();

            if (subscription.RenewalDate > DateTime.Today)
            {
                // Ödeme günü gelmeden ödendi işareti yapılamaz
                TempData["Error"] = "Ödeme günü henüz gelmedi!";
                return RedirectToAction("Active");
            }

            if (int.TryParse(subscription.RenewalPeriod, out var days))
            {
                subscription.RenewalDate = subscription.RenewalDate.AddDays(days);
                await _service.UpdateAsync(subscription);
            }

            TempData["MarkAsPaidMessage"] = $"{subscription.Name} aboneliği ödendi olarak işaretlendi.";
            return RedirectToAction("Active");
        }


        [HttpGet]
        public IActionResult Create() { return View(); }

        [HttpPost]
        public async Task<IActionResult> Create(Subscription subscription, int SelectedMonth, int SelectedDay)
        {
            if (ModelState.ContainsKey(nameof(Subscription.UserId)))
                ModelState.Remove(nameof(Subscription.UserId));

            if (!ModelState.IsValid)
                return View(subscription);

            try
            {
                // Tarihi birleştir
                var composedDate = new DateTime(DateTime.Today.Year, SelectedMonth, SelectedDay);

                // Geçmiş tarihse 1 yıl ileriye al
                if (composedDate < DateTime.Today)
                    composedDate = composedDate.AddYears(1);

                // RenewalDate'yi elle set et
                subscription.RenewalDate = composedDate;

                subscription.UserId = _userManager.GetUserId(User);

                await _service.AddAsync(subscription);

                TempData["CreateMessage"] = $"{subscription.Name} aboneliği başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(Subscription.RenewalDate), ex.Message);
                return View(subscription);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var subscription = await _service.GetByIdAsync(id);

            if (subscription == null || subscription.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            ViewBag.SelectedMonth = subscription.RenewalDate.Month;
            ViewBag.SelectedDay = subscription.RenewalDate.Day;

            return View(subscription);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Subscription subscription, int SelectedMonth, int SelectedDay)
        {
            if (ModelState.ContainsKey(nameof(Subscription.UserId)))
                ModelState.Remove(nameof(Subscription.UserId));

            if (!ModelState.IsValid)
                return View(subscription);

            var subscriptionInDb = await _service.GetByIdAsync(subscription.Id);
            if (subscriptionInDb == null || subscriptionInDb.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            try
            {
                // Yeni tarih oluştur
                var composedDate = new DateTime(DateTime.Today.Year, SelectedMonth, SelectedDay);
                if (composedDate < DateTime.Today)
                    composedDate = composedDate.AddYears(1);

                // Güncelleme işlemi
                subscriptionInDb.Name = subscription.Name;
                subscriptionInDb.Price = subscription.Price;
                subscriptionInDb.RenewalPeriod = subscription.RenewalPeriod;
                subscriptionInDb.RenewalDate = composedDate;

                await _service.UpdateAsync(subscriptionInDb);

                TempData["UpdateMessage"] = $"{subscriptionInDb.Name} aboneliği başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(Subscription.RenewalDate), ex.Message);
                return View(subscription);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {

            var subscription = await _service.GetByIdAsync(id);

            if (subscription == null || subscription.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            await _service.DeleteAsync(subscription);

            TempData["DeleteMessage"] = $"{subscription.Name} aboneliği başarıyla tamamen silindi.";
            return RedirectToAction("Archive");
        }
    }
}
