using ApiProje.WebUI.Dtos.ReservationDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace ApiProje.WebUI.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ReservationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> ReservationList()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7092/api/Reservations");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultReservationDto>>(jsonData);
                return View(values);
            }
            return View();
        }
        [HttpGet]
        public IActionResult CreateReservation()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateReservation(CreateReservationDto createReservationDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createReservationDto); //biz bir metin göndericez bunu Api arafına kaydedebilmek için jsona dönüştürmemiz lazım,metinten jsona dönüştürürken serialize yazıyoruz.
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json"); //StringContent sınıfından bir nesne alıyoruz,3 tane parametre alıcak : string türde olan içeriğin ne,encoding türü(türkçe karakter),dışarıdan gönderdiğin medya türü
            var responseMessage = await client.PostAsync("https://localhost:7092/api/Reservations", stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("ReservationList");
            }
            return View();
        }

        public async Task<IActionResult> DeleteReservation(int id)
        {
            var client = _httpClientFactory.CreateClient();
            await client.DeleteAsync("https://localhost:7092/api/Reservations?id=" + id);
            return RedirectToAction("ReservationList");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateReservation(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7092/api/Reservations/GetReservation?id=" + id);
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<GetReservationByIdDto>(jsonData);
            return View(value);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateReservation(UpdateReservationDto updateReservationDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(updateReservationDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            await client.PutAsync("https://localhost:7092/api/Reservations/", stringContent);
            return RedirectToAction("ReservationList");
        }

        public async Task<IActionResult> AcceptReservation(int id)
        {
            var client = _httpClientFactory.CreateClient();
            
            var response = await client.GetAsync($"https://localhost:7092/api/Reservations/AcceptReservation?id={id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ReservationList");
            }
            return RedirectToAction("ReservationList"); 
        }
    }
}
