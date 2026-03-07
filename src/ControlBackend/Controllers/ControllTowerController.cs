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
        private readonly ILogger<DroneController> _logger;
        private const string CommandRoutingKeyPattern = "drone.{0}.commands";

        public DroneController(IPublisher publisher, ILogger<DroneController> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

        // 1) Comenzar vuelo: POST /api/drone/{id}/start
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartFlight(int id, [FromBody] StartFlightDto? dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid drone ID" });
            }

            _logger.LogInformation("StartFlight called for Drone ID: {DroneId}", id);

            // Prepare the command message with waypoints if provided
            var msg = new
            {
                command = "start",
                droneId = id,
                waypoints = dto?.Waypoints ?? new List<WaypointDto>(),
                isPeriodic = dto?.IsPeriodic ?? false
            };

            var jsonMessage = JsonSerializer.Serialize(msg);
            _logger.LogDebug("Sending message to drone: {Message}", jsonMessage);

            var body = Encoding.UTF8.GetBytes(jsonMessage);

            await _publisher.PublishAsync(string.Format(CommandRoutingKeyPattern, id), body);

            _logger.LogInformation("Start command sent to RabbitMQ for Drone ID: {DroneId} with {WaypointCount} waypoints", 
                id, dto?.Waypoints?.Count ?? 0);

            return Ok(new { status = "sent", action = "start", droneId = id, waypointCount = dto?.Waypoints?.Count ?? 0 });
        }

        // 2) Parar vuelo: POST /api/drone/{id}/stop
        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopFlight(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid drone ID" });
            }

            _logger.LogInformation("StopFlight called for Drone ID: {DroneId}", id);

            var msg = new { command = "stop", droneId = id };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

            await _publisher.PublishAsync(string.Format(CommandRoutingKeyPattern, id), body);
            
            _logger.LogInformation("Stop command sent to RabbitMQ for Drone ID: {DroneId}", id);
            
            return Ok(new { status = "sent", action = "stop", droneId = id });
        }

        // 3) Ir a coordenada: POST /api/drone/{id}/goto
        [HttpPost("{id}/goto")]
        public async Task<IActionResult> GoToCoordinate(int id, [FromBody] GoToDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid drone ID" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("GoToCoordinate called for Drone ID: {DroneId}, Latitude: {Latitude}, Longitude: {Longitude}", 
                id, dto.Latitude, dto.Longitude);

            var msg = new
            {
                command = "goto",
                droneId = id,
                lat = dto.Latitude,
                lng = dto.Longitude,
                speed = dto.Speed
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            await _publisher.PublishAsync(string.Format(CommandRoutingKeyPattern, id), body);

            _logger.LogInformation("Goto command published to RabbitMQ for Drone ID: {DroneId}", id);

            return Ok(new { status = "sent", action = "goto", droneId = id });
        }
    }
}
