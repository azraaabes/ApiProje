using ApiProje.WebApi.Context;
using ApiProje.WebApi.Dtos.GroupReservationDtos;
using ApiProje.WebApi.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupReservationsController : ControllerBase
    {
        private readonly ApiContext _context; //Dependency Injection yapılıyor yani
        private readonly IMapper _mapper;
        public GroupReservationsController(ApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult GroupReservationList()
        {
            var values = _context.GroupReservations.ToList();
            return Ok(values);
        }

        [HttpPost]
        public IActionResult CreateGroupReservation(CreateGroupReservationDto createGroupReservationDto)
        {
            //_context.GroupReservations.Add(GroupReservation);
            //_context.SaveChanges();
            var value = _mapper.Map<GroupReservation>(createGroupReservationDto);
            _context.GroupReservations.Add(value);
            _context.SaveChanges();
            return Ok("Ekleme İşlemi Başarılı");

        }
        [HttpDelete]
        public IActionResult DeleteGroupReservation(int id)
        {
            var value = _context.GroupReservations.Find(id);
            _context.GroupReservations.Remove(value);
            _context.SaveChanges();
            return Ok("Silme İşlemi Başarılı ");
        }

        [HttpGet("GetGroupReservation")]
        public IActionResult GetGroupReservation(int id)
        {
            var value = _context.GroupReservations.Find(id);
            return Ok(value);
        }

        [HttpPut]
        public IActionResult UpdateGroupReservation(UpdateGroupReservationDto updateGroupReservationDto)
        {
            //_context.GroupReservations.Update(GroupReservation);
            //_context.SaveChanges();
            var value = _mapper.Map<GroupReservation>(updateGroupReservationDto);
            _context.GroupReservations.Update(value);
            _context.SaveChanges();
            return Ok("Güncelleme İşlemi Başarılı");
        }
    }
}
