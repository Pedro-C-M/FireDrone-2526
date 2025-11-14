using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace CentralBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        //private static List<FlightPlan> _plans = new();
        private FireDrone _context;

        public FlightPlanController(FireDrone context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightPlan>>> GetAll()
        {
            var plans = await _context.FlightPlans.ToListAsync();
            return Ok(plans);
        }

        [HttpPost]
        public async Task<ActionResult<FlightPlan>> Create([FromBody] FlightPlan plan)
        {
            _context.FlightPlans.Add(plan);
            await _context.SaveChangesAsync();
            return Ok(plan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FlightPlan plan)
        {
            var existing = await _context.FlightPlans.FindAsync(id);
            if (existing == null) return NotFound();

            existing.DronId = plan.DronId;
            existing.RutaId = plan.RutaId;
            existing.EstControlId = plan.EstControlId;
           // existing.StartingPointId = plan.StartingPointId;
            existing.StartingTime = plan.StartingTime;
            existing.EndingTime = plan.EndingTime;
            existing.State = plan.State;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignDron(int id, [FromBody] AssignDronDto dto)
        {
            var existing = await _context.FlightPlans.FindAsync(id);
            if (existing == null) return NotFound($"Flight Plan with ID {id} not found");

            existing.DronId = dto.DronId;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _context.FlightPlans.FindAsync(id);
            if (plan == null) return NotFound();

            _context.FlightPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return Ok();
        }

        public class AssignDronDto
        {
            public int DronId { get; set; }
        }
    }
}