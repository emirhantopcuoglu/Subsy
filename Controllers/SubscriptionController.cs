using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subsy.Data;
using Subsy.Models;

namespace Subsy.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly SubsyContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SubscriptionController(SubsyContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var subscriptions = await _context.Subscriptions
                .Where(s => s.UserId == userId)
                .ToListAsync();

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

                await _context.Subscriptions.AddAsync(subscription);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(subscription);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(subscription);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Subscription subscription)
        {
            if (!ModelState.IsValid)
            {

                return View(subscription);
            }
            var subscriptionInDb = await _context.Subscriptions.FindAsync(subscription.Id);

            if (subscriptionInDb == null)
            {
                return NotFound();
            }

            if (subscriptionInDb.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            subscriptionInDb.Name = subscription.Name;
            subscriptionInDb.Price = subscription.Price;
            subscriptionInDb.RenewalPeriod = subscription.RenewalPeriod;
            subscriptionInDb.RenewalDate = subscription.RenewalDate;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {

            var subscription = await _context.Subscriptions.FindAsync(id);

            if (subscription.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            if (subscription == null)
            {
                return NotFound();
            }
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
