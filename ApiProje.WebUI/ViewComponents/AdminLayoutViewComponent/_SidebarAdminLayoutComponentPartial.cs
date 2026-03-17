using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebUI.ViewComponents.AdminLayoutViewComponent
{
    public class _SidebarAdminLayoutComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
