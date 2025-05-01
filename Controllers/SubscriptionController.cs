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
        public async Task<IActionResult> Create(Subscription subscription)
        {
            if (ModelState.ContainsKey(nameof(Subscription.UserId)))
            {
                ModelState.Remove(nameof(Subscription.UserId));
            }

            if (ModelState.IsValid)
            {
                subscription.UserId = _userManager.GetUserId(User);

                await _service.AddAsync(subscription);

                return RedirectToAction("Index");
            }

            return View(subscription);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var subscription = await _service.GetByIdAsync(id);

            if (subscription == null || subscription.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            return View(subscription);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Subscription subscription)
        {
            if (ModelState.ContainsKey(nameof(Subscription.UserId)))
                ModelState.Remove(nameof(Subscription.UserId));

            if (!ModelState.IsValid)
                return View(subscription);

            var subscriptionInDb = await _service.GetByIdAsync(subscription.Id);

            if (subscriptionInDb == null || subscriptionInDb.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            subscriptionInDb.Name = subscription.Name;
            subscriptionInDb.Price = subscription.Price;
            subscriptionInDb.RenewalPeriod = subscription.RenewalPeriod;
            subscriptionInDb.RenewalDate = subscription.RenewalDate;

            await _service.UpdateAsync(subscriptionInDb);
            return RedirectToAction("Index");
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
