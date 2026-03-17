using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebUI.ViewComponents
{
    public class _FooterDefaultComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
