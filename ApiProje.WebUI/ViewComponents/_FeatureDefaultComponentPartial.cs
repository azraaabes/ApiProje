using ApiProje.WebUI.Dtos.FeatureDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ApiProje.WebUI.ViewComponents
{
    public class _FeatureDefaultComponentPartial:ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public _FeatureDefaultComponentPartial(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7092/api/Features/");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync(); //jsonData isminde boş bir değişken oluşturdum,bu değişkenin içerisine bir atama yapıyorum(responsemessage'dan gelen içeriği string olarak oku)
                var values = JsonConvert.DeserializeObject<List<ResultFeatureDto>>(jsonData);
                return View(values);
            }
            return View();
        }
    }
}
