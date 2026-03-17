using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebUI.ViewComponents
{
    public class _NavbarDefaultComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
