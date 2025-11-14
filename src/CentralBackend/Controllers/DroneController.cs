using CentralBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace CentralBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DroneController : ControllerBase
    {
        private readonly DroneService _service;

        public DroneController(DroneService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dron>>> GetAll()
        {
            var drones = await _service.GetAllAsync();
            return Ok(drones);
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Dron>>> GetAvailable()
        {
            var result = await _service.GetAvailableAsync();
            return Ok(result);
        }
    }
}
