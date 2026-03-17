using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebUI.ViewComponents.AdminLayoutViewComponent
{
    public class _HeadAdminLayoutComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
