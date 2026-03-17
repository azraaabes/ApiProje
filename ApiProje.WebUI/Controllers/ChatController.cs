using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebUI.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult SendChatWithAI()
        {
            return View();
        }
    }
}
