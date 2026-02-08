using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Subsy.Models;
using Subsy.Services;

namespace Subsy.Controllers
{
    public class FinanceController : Controller
    {
        private readonly ISubscriptionService _service;
        private readonly UserManager<IdentityUser> _userManager;

        public FinanceController(ISubscriptionService service, UserManager<IdentityUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var subs = await _service.GetAllByUserId(userId);

            // Aktif ve ödeme yapılmış abonelikleri filtrele
            var activePaidSubs = subs
                .Where(s => !s.IsArchived && (s.RenewalDate > DateTime.Now))
                .ToList();

            var now = DateTime.Now; // Local time kullanımı, eğer RenewalDate local ise uyumlu olur

            // Bu ayki harcama (aktif ve ödemesi yapılmış)
            var thisMonthSpending = activePaidSubs
                .Where(s => s.RenewalDate.Month == now.Month && s.RenewalDate.Year == now.Year)
                .Sum(s => s.Price);

            // Tüm zamanlar toplam harcama (aktif ve ödemesi yapılmış)
            var totalSpending = activePaidSubs.Sum(s => s.Price);

            // Servis bazlı gruplama
            var grouped = activePaidSubs
                .GroupBy(a => a.Name)
                .Select(g => new ServiceSummary
                {
                    SubscriptionName = g.Key,
                    TotalCost = g.Sum(x => x.Price)
                })
                .ToList();

            // En çok ödeme yapılan servis
            var topService = grouped
                .OrderByDescending(g => g.TotalCost)
                .FirstOrDefault();

            var model = new FinanceViewModel
            {
                TotalMonthlyCost = thisMonthSpending,
                AllTimeSpending = totalSpending,
                GroupedByService = grouped,
                TopSpendingService = topService,
                SubscriptionCount = activePaidSubs.Count
            };

            return View(model);
        }
    }
}