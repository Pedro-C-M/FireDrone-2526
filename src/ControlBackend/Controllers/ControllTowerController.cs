using ControlBackend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ControlBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DroneController : ControllerBase
    {
        private readonly IPublisher _publisher;

        public DroneController(IPublisher publisher)
        {
            _publisher = publisher;
        }

        // 1) Comenzar vuelo: POST /api/drone/{id}/start
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartFlight(int id)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StartFlight called for Drone ID: {id}");
     
            var msg = new { command = "start", droneId = id };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

            await _publisher.PublishAsync($"drone.{id}.commands", body);

            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Start command sent to RabbitMQ for Drone ID: {id}");

            return Ok(new { status = "sent", action = "start", droneId = id });
        }

        // 2) Parar vuelo: POST /api/drone/{id}/stop
        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopFlight(int id)
        {
            var msg = new { command = "stop", droneId = id };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

            await _publisher.PublishAsync($"drone.{id}.commands", body);
            return Ok(new { status = "sent", action = "stop", droneId = id });
        }

        // 3) Ir a coordenada: POST /api/drone/{id}/goto
        [HttpPost("{id}/goto")]
        public async Task<IActionResult> GoToCoordinate(int id, [FromBody] GoToDto dto)
        {
            Console.WriteLine($"[DroneController] GoToCoordinate called for Drone ID: {id}, Latitude: {dto.Latitude}, Longitude: {dto.Longitude}");

            var msg = new
            {
                command = "goto",
                droneId = id,
                lat = dto.Latitude,
                lng = dto.Longitude
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            await _publisher.PublishAsync($"drone.{id}.commands", body);

            Console.WriteLine($"[DroneController] Goto command published to RabbitMQ for Drone ID: {id}");

            return Ok(new { status = "sent", action = "goto", droneId = id });
        }
    }
}
