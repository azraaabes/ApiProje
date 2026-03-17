using ApiProje.WebApi.Context;
using ApiProje.WebApi.Dtos.ReservationDtos;
using ApiProje.WebApi.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiProje.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly ApiContext _context; //Dependency Injection yapılıyor yani
        private readonly IMapper _mapper;
        public ReservationsController(ApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult ReservationList()
        {
            var values = _context.Reservations.ToList();
            return Ok(values);
        }

        [HttpPost]
        public IActionResult CreateReservation(CreateReservationDto createReservationDto)
        {
            //_context.Reservations.Add(Reservation);
            //_context.SaveChanges();
            var value = _mapper.Map<Reservation>(createReservationDto);
            _context.Reservations.Add(value);
            _context.SaveChanges();
            return Ok("Rezervasyon Ekleme İşlemi Başarılı");

        }
        [HttpDelete]
        public IActionResult DeleteReservation(int id)
        {
            var value = _context.Reservations.Find(id);
            _context.Reservations.Remove(value);
            _context.SaveChanges();
            return Ok("Rezervasyon Silme İşlemi Başarılı ");
        }

        [HttpGet("GetReservation")]
        public IActionResult GetReservation(int id)
        {
            var value = _context.Reservations.Find(id);
            return Ok(value);
        }

        [HttpPut]
        public IActionResult UpdateReservation(UpdateReservationDto updateReservationDto)
        {
            //_context.Reservations.Update(Reservation);
            //_context.SaveChanges();
            var value = _mapper.Map<Reservation>(updateReservationDto);
            _context.Reservations.Update(value);
            _context.SaveChanges();
            return Ok("Rezervasyon Güncelleme İşlemi Başarılı");
        }

        [HttpGet("GetTotalReservationCount")]
        public IActionResult GetTotalReservationCount()
        {
            var value = _context.Reservations.Count();
            return Ok(value);
        }
        [HttpGet("GetTotalCustomerCount")]
        public IActionResult GetTotalCustomerCount()
        {
            var value = _context.Reservations.Sum(x => x.CountofPeople);
            return Ok(value);
        }
        [HttpGet("GetPendingReservations")]
        public IActionResult GetPendingReservations()
        {
            var value = _context.Reservations.Where(x => x.ReservationStatus == "Onay Bekliyor").Count();
            return Ok(value);
        }
        [HttpGet("GetAprovedReservations")]
        public IActionResult GetAprovedReservations()
        {
            var value = _context.Reservations.Where(x => x.ReservationStatus == "Onaylandı").Count();
            return Ok(value);
        }


        [HttpGet("GetReservationStats")]
        public IActionResult GetReservationStats()
        {
            DateTime today = DateTime.Today;  //bugünün tarihi
            DateTime fourMonthsAgo = today.AddMonths(-3); //dört ay önceki ayı al

            // 1. SQL tarafında sadece gruplama ve veri çekme
            var rawData = _context.Reservations
                .Where(r => r.ReservationDate >= fourMonthsAgo)
                .GroupBy(r => new { r.ReservationDate.Year, r.ReservationDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Approved = g.Count(x => x.ReservationStatus == "Onaylandı"),
                    Pending = g.Count(x => x.ReservationStatus == "Onay Bekliyor"),
                    Canceled = g.Count(x => x.ReservationStatus == "İptal Edildi")
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList(); // Burada SQL biter, veriler RAM’e alınır

            // 2. Bellekte DTO'ya mapleme + tarih formatlama
            var result = rawData.Select(x => new ReservationChartDto
            {
                Month = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                Approved = x.Approved,
                Pending = x.Pending,
                Canceled = x.Canceled
            }).ToList();

            return Ok(result);
        }

        [HttpGet("AcceptReservation")]
        public async Task<IActionResult> AcceptReservation(int id)
        {
            var value =  _context.Reservations.Find(id);
            if (value == null)
            {
                return NotFound("Rezervasyon bulunamadı.");
            }

            value.ReservationStatus = "Onaylandı";
            _context.Reservations.Update(value);
            _context.SaveChanges();
            return Ok("Rezervasyon başarıyla onaylandı.");
        }
    }
}
