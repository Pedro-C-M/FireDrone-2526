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
        public async Task<IActionResult> StartFlight(int id, [FromBody] StartFlightDto? dto)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StartFlight called for Drone ID: {id}");
     
            // Prepare the command message with waypoints if provided
            var msg = new 
            { 
                command = "start", 
                droneId = id,
                waypoints = dto?.Waypoints ?? new List<WaypointDto>()
            };

            var jsonMessage = JsonSerializer.Serialize(msg);
            Console.WriteLine($"[ControlBackend] Sending message to drone: {jsonMessage}");
  
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            await _publisher.PublishAsync($"drone.{id}.commands", body);

            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Start command sent to RabbitMQ for Drone ID: {id} with {dto?.Waypoints?.Count ?? 0} waypoints");

            return Ok(new { status = "sent", action = "start", droneId = id, waypointCount = dto?.Waypoints?.Count ?? 0 });
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
