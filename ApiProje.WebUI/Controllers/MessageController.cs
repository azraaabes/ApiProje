using ApiProje.WebUI.Dtos.MessageDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static ApiProje.WebUI.Controllers.AIController;

namespace ApiProje.WebUI.Controllers
{
    public class MessageController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MessageController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> MessageList()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7092/api/Messages");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultMessageDto>>(jsonData);
                return View(values);
            }
            return View();
        }
        [HttpGet]
        public IActionResult CreateMessage()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageDto createMessageDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createMessageDto); //biz bir metin göndericez bunu Api arafına kaydedebilmek için jsona dönüştürmemiz lazım,metinten jsona dönüştürürken serialize yazıyoruz.
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json"); //StringContent sınıfından bir nesne alıyoruz,3 tane parametre alıcak : string türde olan içeriğin ne,encoding türü(türkçe karakter),dışarıdan gönderdiğin medya türü
            var responseMessage = await client.PostAsync("https://localhost:7092/api/Messages", stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("MessageList");
            }
            return View();
        }

        public async Task<IActionResult> DeleteMessage(int id)
        {
            var client = _httpClientFactory.CreateClient();
            await client.DeleteAsync("https://localhost:7092/api/Messages?id=" + id);
            return RedirectToAction("MessageList");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateMessage(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7092/api/Messages/GetMessage?id=" + id);
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<GetMessageByIdDto>(jsonData);
            return View(value);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateMessage(UpdateMessageDto updateMessageDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(updateMessageDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            await client.PutAsync("https://localhost:7092/api/Messages/", stringContent);
            return RedirectToAction("MessageList");
        }
        [HttpGet]
        public async Task<IActionResult> AnswerMessageWithOpenAI(int id,string prompt) //dışarıdan mesajın id'sini ver
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7092/api/Messages/GetMessage?id=" + id);
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<GetMessageByIdDto>(jsonData);
            prompt = value.MessageDetails; //müşterinin gönderdiği mesajı prompt olarak alıyoruz

            var apiKey = "";
            using var client2 = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey); //Kimlik kontrolü yapıyoruz

            var requestData = new
            {
                model = "gpt-3.5-turbo", //openaı için kullanıcağım model
                messages = new[]
                {
                    new {role="system",content="Sen bir restoran için kullanıcıların göndermiş oldukları mesajları detaylı ve olabildiğince olumlu,müşteri memnuniyetini gözten cevaplar veren bir yapay zeka aracısın.Amacımız kullanıcı tarafından gönderilen en olumlu ve mantıklı cevapları sunabilmek."},
                    new {role="user",content= prompt }
                },
                temperature = 0.5 //oluşturulacak olan içeriğin hassasiyet değeridir.(örn=0.1 girdiğimde daha kurumsal ciddi bir geri bildirim döner)
            };
            var response = await client2.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
                var content = result.choices[0].message.content; //yapay zekanın sana verdiği yemek tarifi
                ViewBag.answerAI = content;
            }
            else
            {
                ViewBag.answerAI = "Bir hata oluştu: " + response.StatusCode;
            }

            return View(value);
        }

        public PartialViewResult SendMessage() //Kullanıcı sayfayı açtığında çalışır,Mesaj gönderme formunu gösterir
        {
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage(CreateMessageDto createMessageDto)
        {
            var client = new HttpClient();
            var apiKey = "";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",apiKey);
            try
            {
                var translateRequestBody = new  //Burada API’ye gönderilecek JSON oluşturuluyor.
                {
                    inputs=createMessageDto.MessageDetails
                };
                var translateJson = System.Text.Json.JsonSerializer.Serialize(translateRequestBody);  //C# nesnesini JSON formatına çevirir
                var translateContent = new StringContent(translateJson, Encoding.UTF8,"application/json");  //API’ye gönderilecek HTTP body hazırlanır.

                var translateResponse = await client.PostAsync("https://api-inference.huggingface.co/models/Helsinki-NLP/opus-mt-tr-en",translateContent);
                var translateResponseString = await translateResponse.Content.ReadAsStringAsync(); //API’den gelen cevap string olarak alınır.

                string englishText = createMessageDto.MessageDetails;
                if (translateResponseString.TrimStart().StartsWith("[")) //API gerçekten JSON array döndürdü mü?
                {
                    var translateDoc = JsonDocument.Parse(translateResponseString); //JSON string → C# JSON nesnesi
                    englishText = translateDoc.RootElement[0].GetProperty("translation_text").GetString(); //Çevrilen Metni Alma
                    //ViewBag.v = englishText;
                }

                var toxicRequestBody = new
                {
                    inputs = englishText   //Toksik mesajı kontrol ederken ingilizceye çevirdiğimiz metni alıyoruz
                };
                var toxicJson = System.Text.Json.JsonSerializer.Serialize(toxicRequestBody);
                var toxicContent = new StringContent(toxicJson, Encoding.UTF8, "application/json");
                var toxicResponse = await client.PostAsync("https://api-inference.huggingface.co/models/unitary/toxic-bert", toxicContent);
                var toxicResponseString= await toxicResponse.Content.ReadAsStringAsync();

                if (toxicResponseString.TrimStart().StartsWith("["))
                {
                    var toxicDoc = JsonDocument.Parse(toxicResponseString);
                    foreach(var item in toxicDoc.RootElement[0].EnumerateArray())
                    {
                        string label=item.GetProperty("label").GetString();
                        double score =item.GetProperty("score").GetDouble();

                        if(score > 0.5)
                        {
                            createMessageDto.Status = "Toksik Mesaj";
                            break;
                        }
                    } 
                }
                if (string.IsNullOrEmpty(createMessageDto.Status))
                {
                    createMessageDto.Status = "Mesaj Alındı";
                }
            }
            catch(Exception ex) 
            {
                createMessageDto.Status = "Onay Bekliyor";
            }



            var client2 = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createMessageDto); //biz bir metin göndericez bunu Api arafına kaydedebilmek için jsona dönüştürmemiz lazım,metinten jsona dönüştürürken serialize yazıyoruz.
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json"); //StringContent sınıfından bir nesne alıyoruz,3 tane parametre alıcak : string türde olan içeriğin ne,encoding türü(türkçe karakter),dışarıdan gönderdiğin medya türü
            var responseMessage = await client2.PostAsync("https://localhost:7092/api/Messages", stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("MessageList");
            }
            return View();
        }
    }
}
