using Microsoft.AspNetCore.Mvc;

namespace Subsy.Web.ViewComponents
{
    public class FlashMessageViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var messages = new List<(string Key, string Message, string Type, int Duration)>
            {
                GetMessage("FlashSuccess", "success", 3000),
                GetMessage("FlashError", "danger", 5000),
                GetMessage("FlashInfo", "info", 4000),
                GetMessage("FlashWarning", "warning", 4500),
            }
            .Where(m => !string.IsNullOrWhiteSpace(m.Message))
            .ToList();

            return View(messages);
        }

        private (string Key, string Message, string Type, int Duration) GetMessage(string key, string type, int duration)
        {
            return (key, TempData[key]?.ToString() ?? string.Empty, type, duration);
        }
    }
}