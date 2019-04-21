using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlevenLab.Data;
using PlevenLab.Data.DTO;
using PlevenLab.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlevenLab.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context
                .Events
                .Include(x => x.Category)
                .Include(x => x.CreatedBy)
                .ToListAsync();
        }

        // GET: api/Events/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        // PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, EventDTO eventDto)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            if (!await UpdateEvent(@event, eventDto))
            {
                return BadRequest();
            }

            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(EventDTO eventDto)
        {
            var @event = new Event();
            if (!await UpdateEvent(@event, eventDto))
            {
                return NotFound();
            }

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = @event.EventId }, @event);
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Event>> DeleteEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return @event;
        }

        private async Task<bool> UpdateEvent(Event @event, EventDTO eventDto)
        {
            var category = await _context.Categories.FindAsync(eventDto.CategoryId);
            if (category == null)
            {
                return false;
            }

            var user = await _context.Users.FindAsync(eventDto.CreatedByUserId);
            if (user == null)
            {
                return false;
            }

            @event.Category = category;
            @event.CreatedBy = user;
            @event.Title = eventDto.Title;
            @event.CreatedDate = eventDto.CreatedDate;
            @event.StartDate = eventDto.StartDate;
            @event.EndDate = eventDto.EndDate;
            @event.LocationFriendlyName = eventDto.LocationFriendlyName;
            @event.LocationLat = eventDto.LocationLat;
            @event.LocationLng = eventDto.LocationLng;
            return true;
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}
