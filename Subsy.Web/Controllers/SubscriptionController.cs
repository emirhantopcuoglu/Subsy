using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Subscriptions.Dashboard;
using Subsy.Models;
using System.Globalization;
using System.Text.Json;

namespace Subsy.Web.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionDashboardService _dashboard;
        private readonly UserManager<IdentityUser> _userManager;

        public SubscriptionController(
            ISubscriptionDashboardService dashboard,
            UserManager<IdentityUser> userManager)
        {
            _dashboard = dashboard;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return View("Index", "Home");

            var userId = _userManager.GetUserId(User);

            var vm = await _dashboard.GetDashboardAsync(userId!, cancellationToken);

            ViewBag.ActiveCount = vm.ActiveCount;
            ViewBag.TodayDueCount = vm.TodayDueCount;
            ViewBag.TotalThisMonth = vm.TotalThisMonth;
            ViewBag.Upcoming = vm.Upcoming;

            return View();
        }

        //[HttpGet]
        //public async Task<IActionResult> Active()
        //{
        //    var userId = _userManager.GetUserId(User);
        //    var subscriptions = await _service.GetAllByUserId(userId);
        //    var active = subscriptions.Where(s => !s.IsArchived).ToList();
        //    return View(active);
        //}

        //[HttpGet]
        //public async Task<IActionResult> Due()
        //{
        //    var userId = _userManager.GetUserId(User);
        //    var list = await _service.GetAllByUserId(userId);
        //    var due = list.Where(s => s.RenewalDate == DateTime.Today && !s.IsArchived).ToList();
        //    return View(due);
        //}

        //[HttpGet]
        //public async Task<IActionResult> Archive()
        //{
        //    var userId = _userManager.GetUserId(User);
        //    var list = await _service.GetAllByUserId(userId);
        //    var archived = list.Where(s => s.IsArchived).ToList();
        //    return View(archived);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Archive(int id)
        //{
        //    var sub = await _service.GetByIdAsync(id);
        //    if (sub == null || sub.UserId != _userManager.GetUserId(User))
        //        return Unauthorized();

        //    sub.IsArchived = true;
        //    await _service.UpdateAsync(sub);
        //    TempData["ArchiveMessage"] = $"{sub.Name} aboneliği başarıyla sonlandırıldı.";
        //    return RedirectToAction("Active");
        //}

        //[HttpPost]
        //public async Task<IActionResult> Unarchive(int id)
        //{
        //    var sub = await _service.GetByIdAsync(id);
        //    if (sub == null || sub.UserId != _userManager.GetUserId(User))
        //        return Unauthorized();

        //    sub.IsArchived = false;
        //    await _service.UpdateAsync(sub);
        //    TempData["ArchiveMessage"] = $"{sub.Name} aboneliği başarıyla aktifleştirildi.";
        //    return RedirectToAction("Archive");
        //}

        //public async Task<IActionResult> Calendar()
        //{
        //    var userId = _userManager.GetUserId(User);

        //    var subs = await _service.GetAllByUserId(userId);

        //    subs = subs.Where(s => !s.IsArchived).ToList();

        //    var events = subs.Select(s => new
        //    {
        //        title = $"{s.Name} – {s.Price:0.##}₺",
        //        start = s.RenewalDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        //    });

        //    ViewBag.Events = JsonSerializer.Serialize(events);
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> MarkAsPaid(int id)
        //{
        //    var subscription = await _service.GetByIdAsync(id);
        //    var userId = _userManager.GetUserId(User);

        //    if (subscription == null || subscription.UserId != userId)
        //        return Unauthorized();

        //    if (subscription.RenewalDate > DateTime.Today)
        //    {
        //        // Ödeme günü gelmeden ödendi işareti yapılamaz
        //        TempData["Error"] = "Ödeme günü henüz gelmedi!";
        //        return RedirectToAction("Active");
        //    }

        //    if (int.TryParse(subscription.RenewalPeriod, out var days))
        //    {
        //        subscription.RenewalDate = subscription.RenewalDate.AddDays(days);
        //        await _service.UpdateAsync(subscription);
        //    }

        //    TempData["MarkAsPaidMessage"] = $"{subscription.Name} aboneliği ödendi olarak işaretlendi.";
        //    return RedirectToAction("Active");
        //}


        //[HttpGet]
        //public IActionResult Create() { return View(); }

        //[HttpPost]
        //public async Task<IActionResult> Create(SubscriptionViewModel subscription, int SelectedMonth, int SelectedDay)
        //{
        //    if (ModelState.ContainsKey(nameof(SubscriptionViewModel.UserId)))
        //        ModelState.Remove(nameof(SubscriptionViewModel.UserId));

        //    if (!ModelState.IsValid)
        //        return View(subscription);

        //    try
        //    {
        //        // Tarihi birleştir
        //        var composedDate = new DateTime(DateTime.Today.Year, SelectedMonth, SelectedDay);

        //        // Geçmiş tarihse 1 yıl ileriye al
        //        if (composedDate < DateTime.Today)
        //            composedDate = composedDate.AddYears(1);

        //        // RenewalDate'yi elle set et
        //        subscription.RenewalDate = composedDate;

        //        subscription.UserId = _userManager.GetUserId(User);

        //        await _service.AddAsync(subscription);

        //        TempData["CreateMessage"] = $"{subscription.Name} aboneliği başarıyla oluşturuldu.";
        //        return RedirectToAction("Index");
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        ModelState.AddModelError(nameof(SubscriptionViewModel.RenewalDate), ex.Message);
        //        return View(subscription);
        //    }
        //}


        //[HttpGet]
        //public async Task<IActionResult> Update(int id)
        //{
        //    var subscription = await _service.GetByIdAsync(id);

        //    if (subscription == null || subscription.UserId != _userManager.GetUserId(User))
        //    {
        //        return Unauthorized();
        //    }

        //    ViewBag.SelectedMonth = subscription.RenewalDate.Month;
        //    ViewBag.SelectedDay = subscription.RenewalDate.Day;

        //    return View(subscription);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Update(SubscriptionViewModel subscription, int SelectedMonth, int SelectedDay)
        //{
        //    if (ModelState.ContainsKey(nameof(SubscriptionViewModel.UserId)))
        //        ModelState.Remove(nameof(SubscriptionViewModel.UserId));

        //    if (!ModelState.IsValid)
        //        return View(subscription);

        //    var subscriptionInDb = await _service.GetByIdAsync(subscription.Id);
        //    if (subscriptionInDb == null || subscriptionInDb.UserId != _userManager.GetUserId(User))
        //        return Unauthorized();

        //    try
        //    {
        //        // Yeni tarih oluştur
        //        var composedDate = new DateTime(DateTime.Today.Year, SelectedMonth, SelectedDay);
        //        if (composedDate < DateTime.Today)
        //            composedDate = composedDate.AddYears(1);

        //        // Güncelleme işlemi
        //        subscriptionInDb.Name = subscription.Name;
        //        subscriptionInDb.Price = subscription.Price;
        //        subscriptionInDb.RenewalPeriod = subscription.RenewalPeriod;
        //        subscriptionInDb.RenewalDate = composedDate;

        //        await _service.UpdateAsync(subscriptionInDb);

        //        TempData["UpdateMessage"] = $"{subscriptionInDb.Name} aboneliği başarıyla güncellendi.";
        //        return RedirectToAction("Index");
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        ModelState.AddModelError(nameof(SubscriptionViewModel.RenewalDate), ex.Message);
        //        return View(subscription);
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> Delete(int id)
        //{

        //    var subscription = await _service.GetByIdAsync(id);

        //    if (subscription == null || subscription.UserId != _userManager.GetUserId(User))
        //    {
        //        return Unauthorized();
        //    }

        //    await _service.DeleteAsync(subscription);

        //    TempData["DeleteMessage"] = $"{subscription.Name} aboneliği başarıyla tamamen silindi.";
        //    return RedirectToAction("Archive");
        //}
    }
}
