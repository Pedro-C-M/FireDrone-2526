using CentralBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Text.Json;

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

        [HttpPost("{droneId}/status")]
        public async Task<IActionResult> DroneStatus(int droneId, [FromBody] JsonElement content)
        {
            // Ejemplo de extraer los valores, se puede poner un punto de depuracion para ver lo que hay
            if (content.TryGetProperty("Latitude",out var latitude))
            {
                Console.WriteLine($"Latitude recieved: {latitude}");
            }
            else
            {
                Console.WriteLine("Latitude property not found");
            }

                Console.WriteLine($"[DroneController] Status received for Drone ID: {droneId}");
            return Ok(content);
        }
    }
}
