using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subsy.Data;
using Subsy.Models;
using Subsy.Services;

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
            var userId = _userManager.GetUserId(User);
            var subscriptions = await _service.GetAllByUserId(userId);

            var totalAmount = await _service.GetTotalAmountAsync(userId);
            ViewBag.TotalAmount = totalAmount;

            return View(subscriptions);
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
            return RedirectToAction("Index");
        }
    }
}
