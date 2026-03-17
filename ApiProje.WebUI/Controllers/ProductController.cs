using ApiProje.WebUI.Dtos.CategoryDtos;
using ApiProje.WebUI.Dtos.ProductDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;

namespace ApiProje.WebUI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> ProductList()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7092/api/Products/ProductListWithCategory");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultProductDto>>(jsonData);
                return View(values);
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7092/api/Categories");
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<ResultCategoryDto>>(jsonData);
            List<SelectListItem> categoryValues = (from x in values
                select new SelectListItem
                {
                    Text = x.CategoryName,
                    Value = x.CategoryId.ToString()
                }).ToList();
            ViewBag.v=categoryValues;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto createProductDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createProductDto); //biz bir metin göndericez bunu Api arafına kaydedebilmek için jsona dönüştürmemiz lazım,metinten jsona dönüştürürken serialize yazıyoruz.
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json"); //StringContent sınıfından bir nesne alıyoruz,3 tane parametre alıcak : string türde olan içeriğin ne,encoding türü(türkçe karakter),dışarıdan gönderdiğin medya türü
            var responseMessage = await client.PostAsync("https://localhost:7092/api/Products/CreateProductWithCategory", stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("ProductList");
            }
            return View();
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var client = _httpClientFactory.CreateClient();
            await client.DeleteAsync("https://localhost:7092/api/Products?id=" + id);
            return RedirectToAction("ProductList");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(int id)
        {

            var client = _httpClientFactory.CreateClient();  
            var responseMessage = await client.GetAsync("https://localhost:7092/api/Products/GetProduct?id=" + id);
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var productValues = JsonConvert.DeserializeObject<GetProductByIdDto>(jsonData);

            // 2. Kategorileri çek (Açılır liste için)
            var responseCategory = await client.GetAsync("https://localhost:7092/api/Categories"); // URL'i API'ye göre kontrol et
            var jsonCategory = await responseCategory.Content.ReadAsStringAsync();
            var categoryData = JsonConvert.DeserializeObject<List<ResultCategoryDto>>(jsonCategory);

            ViewBag.v = (from x in categoryData
                         select new SelectListItem
                         {
                             Text = x.CategoryName,
                             Value = x.CategoryId.ToString()
                         }).ToList();

            return View(productValues); // Artık hata vermeyecek
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(UpdateProductDto updateProductDto)
        {
            Console.WriteLine(updateProductDto.ProductName);

            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(updateProductDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            await client.PutAsync("https://localhost:7092/api/Products/UpdateProductWithCategory", stringContent);
            return RedirectToAction("ProductList");
        }
    }
}
