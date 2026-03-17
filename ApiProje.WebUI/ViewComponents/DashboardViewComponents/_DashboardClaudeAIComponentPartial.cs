using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebUI.ViewComponents.DashboardViewComponents
{
    public class _DashboardClaudeAIComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
