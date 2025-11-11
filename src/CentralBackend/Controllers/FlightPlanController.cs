using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace CentralBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private static List<FlightPlan> _plans = new();

        [HttpGet]
        public ActionResult<IEnumerable<FlightPlan>> GetAll()
        {
            return Ok(_plans);
        }

        [HttpPost]
        public ActionResult<FlightPlan> Create([FromBody] FlightPlan plan)
        {
            plan.Id = _plans.Count + 1;
            _plans.Add(plan);
            return Ok(plan);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] FlightPlan plan)
        {
            var existing = _plans.Find(p => p.Id == id);
            if (existing == null)
                return NotFound();

            existing.DronId = plan.DronId;
            existing.RutaId = plan.RutaId;
            existing.EstControlId = plan.EstControlId;
            existing.StartingPointId = plan.StartingPointId;
            existing.StartingTime = plan.StartingTime;
            existing.EndingTime = plan.EndingTime;
            existing.State = plan.State;

            return Ok(existing);
        }

        [HttpPut("{id}/assign")]
        public IActionResult AssignDron(int id, [FromBody] AssignDronDto dto)
        {
            var existing = _plans.Find(p => p.Id == id);
            if (existing == null)
                return NotFound($"Flight Plan with ID {id} not found");

            existing.DronId = dto.DronId;
            return Ok(existing);
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var plan = _plans.Find(p => p.Id == id);
            if (plan == null)
                return NotFound();

            _plans.Remove(plan);
            return Ok();
        }

        public class AssignDronDto
        {
            public int DronId { get; set; }
        }
    }
}
