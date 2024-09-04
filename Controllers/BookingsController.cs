using Jovan_Project.Data;
using Jovan_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using EmployerService.Models;

namespace Jovan_Project.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var bookings = _context.Booking.ToList();
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int? id)
        {
            var booking = _context.Booking.FirstOrDefault(b => b.BookingId == id);
            if (booking == null)
            {
                return Problem(detail: "Booking with id " + id + " is not found.", statusCode: 404);
            }

            return Ok(booking);
        }

        [HttpPost]
        public IActionResult Post(Booking booking)
        {
            _context.Booking.Add(booking);
            _context.SaveChanges();
            return CreatedAtAction("GetAll", new { id = booking.BookingId }, booking);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id}")]
        public IActionResult Put(int? id, Booking booking)
        {
            var entity = _context.Booking.FirstOrDefault(b => b.BookingId == id);
            if (entity == null)
            {
                return Problem(detail: "Booking with id " + id + " is not found.", statusCode: 404);
            }

            entity.FacilityDescription = booking.FacilityDescription;
            entity.BookingDateFrom = booking.BookingDateFrom;
            entity.BookingDateTo = booking.BookingDateTo;
            entity.BookingStatus = booking.BookingStatus;
            entity.BookedBy = booking.BookedBy;

            _context.SaveChanges();
            return Ok(entity);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int? id)
        {
            var entity = _context.Booking.FirstOrDefault(b => b.BookingId == id);
            if (entity == null)
            {
                return Problem(detail: "Booking with id " + id + " is not found.", statusCode: 404);
            }
            _context.Booking.Remove(entity);
            _context.SaveChanges();
            return Ok(entity);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("export")]
        public IActionResult ExportToCsv()
        {
            var bookings = _context.Booking.ToList();
            var csvBuilder = new StringBuilder();
            // Add CSV header
            csvBuilder.AppendLine("BookingId,FacilityDescription,BookingDateFrom,BookingDateTo,BookedBy,BookingStatus");
            // Add data rows
            foreach (var booking in bookings)
            {
                csvBuilder.AppendLine($"{booking.BookingId},{booking.FacilityDescription},{booking.BookingDateFrom:yyyy-MM-dd HH:mm:ss},{booking.BookingDateTo:yyyy-MM-dd HH:mm:ss},{booking.BookedBy},{booking.BookingStatus}");
            }
            var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(csvBytes, "text/csv", "bookings_export.csv");
        }

        [HttpGet("statistics")]
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult GetStatistics()
        {
            // 1. Total Bookings
            var totalBookings = _context.Booking.Count();

            // 2. Bookings by Status
            var bookingsByStatus = _context.Booking
                .GroupBy(b => b.BookingStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            // 3. Most Popular Facility
            var mostPopularFacility = _context.Booking
                .GroupBy(b => b.FacilityDescription)
                .OrderByDescending(g => g.Count())
                .Select(g => new { Facility = g.Key, BookingsCount = g.Count() })
                .FirstOrDefault();

            return Ok(new
            {
                TotalBookings = totalBookings,
                BookingsByStatus = bookingsByStatus,
                MostPopularFacility = mostPopularFacility
            });
        }

    }
}