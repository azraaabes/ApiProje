 using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ApiProje.WebUI.Controllers
{
    public class AIController : Controller
    {
        public IActionResult CreateRecipeWithOpenAI()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRecipeWithOpenAI(string prompt) //Kullanıcıdan gelen prompt
        {
            var apiKey ="";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",apiKey); //Kimlik kontrolü yapıyoruz

            var requestData = new
            {
                model = "gpt-3.5-turbo", //openaı için kullanıcağım model
                messages = new[]
                {
                    new {role="system",content="Sen bir restoran için yemek önerileri yapan bir yapay zeka aracısın.Amacımız kullanıcı tarafından girilen malzemelere göre yemek tarifi önerisinde bulunmak."},
                    new {role="user",content= prompt }
                },
                temperature = 0.5 //oluşturulacak olan içeriğin hassasiyet değeridir.(örn=0.1 girdiğimde daha kurumsal ciddi bir geri bildirim döner)
            };
            var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestData);

            if(response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
                var content = result.choices[0].message.content; //yapay zekanın sana verdiği yemek tarifi
                ViewBag.recipe=content;
            }
            else
            {
                ViewBag.recipe = "Bir hata oluştu: " + response.StatusCode;
            }
            return View();
        }
        public class OpenAIResponse  //OpenAI'dan dönen Json cevabını karşılamak için yazılmış model sınıflar,bu sınıflar sayesinde gelen JSON'u C# objelerine çevirebiliyoruz.
        {
            public List<Choice> choices { get; set; }
        }
        public class Choice
        {
            public Message message { get; set; }
        }
        public class Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }
    }
}
