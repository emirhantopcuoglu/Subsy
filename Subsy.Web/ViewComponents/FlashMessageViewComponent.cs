using Microsoft.AspNetCore.Mvc;

namespace Subsy.ViewComponents
{
    public class FlashMessageViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var messages = new List<(string Key, string Message, string Type, int Duration)>
            {
                GetMessage("CreateMessage","success",3000),
                GetMessage("MarkAsPaidMessage","success",3000),
                GetMessage("ArchiveMessage","success",3000),
                GetMessage("UpdateMessage", "success", 3000),
                GetMessage("DeleteMessage", "success", 3000),
                GetMessage("RegisterMessage", "info", 5000),
                GetMessage("LoginMessage", "info", 5000),
                GetMessage("LogoutMessage", "info", 3000)
            }.Where(m => !string.IsNullOrEmpty(m.Message)).ToList();
            return View(messages);
        }

        private(string Key, string Message, string Type, int Duration) GetMessage(string key, string type, int duration)
        {
            return (key, TempData[key]?.ToString(), type, duration);
        }
    }
}
