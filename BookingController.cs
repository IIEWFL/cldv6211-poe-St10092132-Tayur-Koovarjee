using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ST10092132.Data;
using ST10092132.Models;

namespace ST10092132.Controllers
{
    public class BookingController : Controller
    {
        private readonly POEDBContext _context;

        public BookingController(POEDBContext context)
        {
            _context = context;
        }

        // GET: Booking with advanced filtering
        public async Task<IActionResult> Index(
            string searchString,
            int? eventTypeId,
            int? venueId,
            DateTime? startDate,
            DateTime? endDate,
            bool? availableOnly)
        {
            // Start with base query including related data
            var bookings = _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)  // Include EventType via Event
                .Include(b => b.Venue)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.BookingId.ToString().Contains(searchString) ||
                    b.Event.EventName.Contains(searchString));
            }

            // Apply event type filter
            if (eventTypeId.HasValue)
            {
                bookings = bookings.Where(b => b.Event.EventTypeId == eventTypeId.Value);
            }

            // Apply venue filter
            if (venueId.HasValue)
            {
                bookings = bookings.Where(b => b.VenueId == venueId.Value);
            }

            // Apply date range filter
            if (startDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate <= endDate.Value);
            }

            // Apply availability filter
            if (availableOnly == true)
            {
               
            }

            // Prepare filter options for the view
            ViewData["EventTypes"] = new SelectList(_context.EventTypes, "EventTypeId", "EventTypeName", eventTypeId);
            ViewData["Venues"] = new SelectList(_context.Venues, "VenueId", "VenueName", venueId);
            ViewData["CurrentFilter"] = searchString;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["AvailableOnly"] = availableOnly;

            return View(await bookings.ToListAsync());
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)  // Include EventType
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            return booking == null ? NotFound() : View(booking);
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");

            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            // Check if venue is available
            var venue = await _context.Venues.FindAsync(booking.VenueId);
            
            {
                ModelState.AddModelError("VenueId", "Selected venue is not available");
            }

            // Check for double booking
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b =>
                    b.VenueId == booking.VenueId &&
                    b.BookingDate.Date == booking.BookingDate.Date
                );

            if (existingBooking != null)
            {
                ModelState.AddModelError("BookingDate", "This venue is already booked for the selected date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Re-populate dropdowns on validation error
           
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
           
            return View(booking);
        }

        // GET: Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            // Show all venues in edit mode (availability might change)
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            // Check for double booking (excluding current booking)
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b =>
                    b.VenueId == booking.VenueId &&
                    b.BookingDate.Date == booking.BookingDate.Date &&
                    b.BookingId != booking.BookingId
                );

            if (existingBooking != null)
            {
                ModelState.AddModelError("BookingDate", "This venue is already booked for the selected date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Booking/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)  // Include EventType
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            return booking == null ? NotFound() : View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return RedirectToAction(nameof(Index));

            try
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Delete failed: This booking is linked to other records.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}