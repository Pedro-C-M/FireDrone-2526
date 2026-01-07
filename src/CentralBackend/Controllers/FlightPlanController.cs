using CentralBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using ControlBackend.DTOs;

namespace CentralBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private readonly FlightPlanService _service;

        public FlightPlanController(FlightPlanService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightPlan>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<FlightPlan>> Create([FromBody] FlightPlan plan)
        {
            var result = await _service.CreateAsync(plan);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FlightPlan plan)
        {
            var result = await _service.UpdateAsync(id, plan);
            return Ok(result);
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignDron(int id, [FromBody] AssignDronDto dto)
        {
            var result = await _service.AssignDronAsync(id, dto.DronId, dto.RestartFromBeginning);
            return Ok(result);
        }

        [HttpPut("{id}/stop")]
        public async Task<IActionResult> StopFlightPlan(int id)
        {
            var result = await _service.StopFlightPlanAsync(id);
            return Ok(result);
        }

        [HttpPut("{id}/manual")]
        public async Task<IActionResult> SwitchToManual(int id)
        {
            var result = await _service.SwitchToManualModeAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/goto")]
        public async Task<IActionResult> SendManualDestination(int id, [FromBody] GoToDto dto)
        {
            await _service.SendManualDestinationAsync(id, dto);
            return Ok(new { message = "Manual destination sent successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }

        public class AssignDronDto
        {
            public int DronId { get; set; }
            public bool RestartFromBeginning { get; set; } = false;
        }
    }
}
