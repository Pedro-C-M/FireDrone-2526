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
        private readonly DroneSignalRService _signalRService;

        public DroneController(DroneService service, DroneSignalRService signalRService)
        {
            _service = service;
            _signalRService = signalRService;
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
            try
            {
                Console.WriteLine($"[DroneController] Status received for Drone ID: {droneId}");

                // Obtener el dron de la base de datos
                var drone = await _service.GetByIdAsync(droneId);
                if (drone == null)
                {
                    Console.WriteLine($"[DroneController] Drone {droneId} not found");
                    return NotFound($"Drone {droneId} not found");
                }

                // Extraer los valores del JSON y actualizar el dron
                if (content.TryGetProperty("Latitude", out var latitude))
                {
                    drone.Lat = (float)latitude.GetDouble();
                }

                if (content.TryGetProperty("Longitude", out var longitude))
                {
                    drone.Lon = (float)longitude.GetDouble();
                }

                if (content.TryGetProperty("Altitude", out var altitude))
                {
                    drone.Altitude = (float)altitude.GetDouble();
                }

                if (content.TryGetProperty("Speed", out var speed))
                {
                    drone.Speed = (float)speed.GetDouble();
                }

                if (content.TryGetProperty("Battery", out var battery))
                {
                    drone.Battery = (float)battery.GetDouble();
                }

                if (content.TryGetProperty("State", out var state))
                {
                    drone.State = (DroneState)state.GetInt32();
                }

                Console.WriteLine($"[DroneController] Updating Drone {droneId}: Lat={drone.Lat}, Lon={drone.Lon}, Alt={drone.Altitude}, State={drone.State}");

                // Guardar cambios en la base de datos
                await _service.UpdateAsync(droneId, drone);

                // Hacer broadcast a través de SignalR para actualización en tiempo real
                await _signalRService.BroadcastDroneUpdate(drone);

                Console.WriteLine($"[DroneController] Drone {droneId} status updated and broadcasted via SignalR");

                return Ok(new { message = "Status updated successfully", droneId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DroneController] Error updating drone status: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
