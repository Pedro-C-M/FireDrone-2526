using CentralBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
//NUEVOv2
using ControlBackend.DTOs;
//FIN NUEVOv2

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
            var result = await _service.AssignDronAsync(id, dto.DronId);
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

        //NUEVOv2
        [HttpPost("{id}/manual-destination")]
        public async Task<IActionResult> SetManualDestination(int id, [FromBody] GoToDto dto)
        {
            await _service.SendManualDestinationAsync(id, dto);
            return Ok();
        }
        //FIN NUEVOv2

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }

        public class AssignDronDto
        {
            public int DronId { get; set; }
        }
    }
}
