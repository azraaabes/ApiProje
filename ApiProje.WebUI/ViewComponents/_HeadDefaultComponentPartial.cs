using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebUI.ViewComponents
{
    public class _HeadDefaultComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke() //Invoke çağırmak anlamına gelen metot
        {
            return View();  
        }
    }
}
