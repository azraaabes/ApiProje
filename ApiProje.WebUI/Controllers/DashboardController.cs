using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebUI.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
