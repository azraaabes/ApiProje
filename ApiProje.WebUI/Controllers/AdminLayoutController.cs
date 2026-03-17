using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebUI.Controllers
{
    public class AdminLayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
