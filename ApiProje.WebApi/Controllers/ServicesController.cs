using ApiProje.WebApi.Context;
using ApiProje.WebApi.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApiContext _contex;

        public ServicesController(ApiContext contex)
        {
            _contex = contex;
        }
        [HttpGet]
        public IActionResult ServiceList()
        {
            var values = _contex.Services.ToList();
            return Ok(values);
        }
        [HttpPost]
        public IActionResult CreateService(Service service)
        {
            _contex.Services.Add(service);
            _contex.SaveChanges();
            return Ok("Hizmet ekleme başarılı");
        }
        [HttpDelete]
        public IActionResult DeleteService(int id)
        {
            var value = _contex.Services.Find(id);
            _contex.Services.Remove(value);
            _contex.SaveChanges();
            return Ok("Silme başarılı");
        }
        [HttpGet("GetService")]
        public IActionResult GetService(int id)
        {
            var value = _contex.Services.Find(id);
            return Ok(value);
        }
        [HttpPut]
        public IActionResult UpdateService(Service service)
        {
            _contex.Services.Update(service);
            _contex.SaveChanges();
            return Ok("Hizmet güncelleme başarılı");
        }

    }
}
