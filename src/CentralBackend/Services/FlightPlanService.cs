using CentralBackend.Exceptions;
using Microsoft.EntityFrameworkCore;
using Models;
//NUEVOv2
using ControlBackend.DTOs;
//FIN NUEVOv2

namespace CentralBackend.Services
{
    public class FlightPlanService
    {
        private readonly FireDrone _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FlightPlanService(FireDrone context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<List<FlightPlan>> GetAllAsync()
        {
            return await _context.FlightPlans.ToListAsync();
        }

        public async Task<FlightPlan?> GetByIdAsync(int id)
        {
            return await _context.FlightPlans.FindAsync(id);
        }

        public async Task<FlightPlan> CreateAsync(FlightPlan plan)
        {
            Console.WriteLine($"[FlightPlanService] CreateAsync called: DronId={plan.DronId}, RutaId={plan.RutaId}, State={plan.State}");

            _context.FlightPlans.Add(plan);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[FlightPlanService] FlightPlan {plan.Id} created in database");

            // Only start the flight automatically if the plan is created with OnCourse status
            if (plan.State == FlightStatus.OnCourse)
            {
                try
                {
                    // Load the route with coordinates
                    var flightPlanWithRoute = await _context.FlightPlans
                        .Include(fp => fp.Ruta)
                        .ThenInclude(r => r.Coords)
                        .FirstOrDefaultAsync(fp => fp.Id == plan.Id);

                    var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://localhost:5307";
                    var httpClient = _httpClientFactory.CreateClient();

                    // Convert RoutePoints to Waypoints
                    var waypoints = flightPlanWithRoute?.Ruta?.Coords?.Select(rp => new
                    {
                        latitude = rp.Lat,
                        longitude = rp.Long,
                        altitude = rp.Height,
                        speed = rp.Velocity
                    }).ToList();

                    Console.WriteLine($"[FlightPlanService] Calling ControlBackend at {controlBackendUrl}/api/drone/{plan.DronId}/start with {waypoints?.Count ?? 0} waypoints");

                    var response = await httpClient.PostAsJsonAsync(
                        $"{controlBackendUrl}/api/drone/{plan.DronId}/start",
                        new { Waypoints = waypoints }  // Use capital W to match DTO
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[FlightPlanService] Failed to start flight for drone {plan.DronId}: {response.StatusCode}");
                    }
                    else
                    {
                        Console.WriteLine($"[FlightPlanService] Successfully called ControlBackend StartFlight for drone {plan.DronId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FlightPlanService] Error calling ControlBackend StartFlight: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[FlightPlanService] Flight not started automatically - plan state is {plan.State}");
            }

            return plan;
        }

        public async Task<FlightPlan> UpdateAsync(int id, FlightPlan plan)
        {
            var existing = await _context.FlightPlans.FindAsync(id);
            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {id} does not exist.");

            existing.DronId = plan.DronId;
            existing.RutaId = plan.RutaId;
            existing.EstControlId = plan.EstControlId;
            //existing.StartingPointId = plan.StartingPointId;
            existing.StartingTime = plan.StartingTime;
            existing.EndingTime = plan.EndingTime;
            existing.State = plan.State;

            //existing.ModeChangeHistoric = plan.ModeChangeHistoric;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<FlightPlan> AssignDronAsync(int id, int dronId)
        {
            Console.WriteLine($"[FlightPlanService] AssignDronAsync called: FlightPlanId={id}, DronId={dronId}");

            var existing = await _context.FlightPlans
                .Include(fp => fp.Ruta)
                .ThenInclude(r => r.Coords)
                .FirstOrDefaultAsync(fp => fp.Id == id);

            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {id} does not exist.");

            existing.DronId = dronId;
            existing.State = FlightStatus.OnCourse;
            existing.StartingTime = DateTime.Now;
            await _context.SaveChangesAsync();

            Console.WriteLine($"[FlightPlanService] Drone {dronId} assigned to FlightPlan {id} in database, state set to OnCourse");

            // Call ControlBackend to start the flight with waypoints from the route
            try
            {
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://localhost:5307";
                var httpClient = _httpClientFactory.CreateClient();

                // Convert RoutePoints to Waypoints
                var waypoints = existing.Ruta?.Coords?.Select(rp => new
                {
                    latitude = rp.Lat,
                    longitude = rp.Long,
                    altitude = rp.Height,
                    speed = rp.Velocity
                }).ToList();

                Console.WriteLine($"[FlightPlanService] Sending {waypoints?.Count ?? 0} waypoints to ControlBackend for drone {dronId}");
  
                // Log the first waypoint to verify data
                if (waypoints != null && waypoints.Count > 0)
                {
                    var first = waypoints[0];
                    Console.WriteLine($"[FlightPlanService] First waypoint: lat={first.latitude}, lon={first.longitude}, alt={first.altitude}, speed={first.speed}");
                    if (waypoints.Count > 1)
                    {
                        var last = waypoints[waypoints.Count - 1];
                        Console.WriteLine($"[FlightPlanService] Last waypoint: lat={last.latitude}, lon={last.longitude}, alt={last.altitude}, speed={last.speed}");
                    }
                }

                var response = await httpClient.PostAsJsonAsync(
                    $"{controlBackendUrl}/api/drone/{dronId}/start",
                    new { Waypoints = waypoints }  // Use capital W to match DTO
                );

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[FlightPlanService] Failed to start flight for drone {dronId}: {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine($"[FlightPlanService] Successfully called ControlBackend StartFlight for drone {dronId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FlightPlanService] Error calling ControlBackend StartFlight: {ex.Message}");
            }

            return existing;
        }

        public async Task<FlightPlan> StopFlightPlanAsync(int id)
        {
            Console.WriteLine($"[FlightPlanService] StopFlightPlanAsync called: FlightPlanId={id}");

            var existing = await _context.FlightPlans.FindAsync(id);
            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {id} does not exist.");

            // Update flight plan state to Cancelled
            existing.State = FlightStatus.Cancelled;
            existing.EndingTime = DateTime.Now;
            await _context.SaveChangesAsync();

            Console.WriteLine($"[FlightPlanService] FlightPlan {id} marked as Cancelled in database");

            // Call ControlBackend to stop the flight
            try
            {
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://localhost:5307";
                var httpClient = _httpClientFactory.CreateClient();

                Console.WriteLine($"[FlightPlanService] Calling ControlBackend at {controlBackendUrl}/api/drone/{existing.DronId}/stop");

                var response = await httpClient.PostAsync(
                    $"{controlBackendUrl}/api/drone/{existing.DronId}/stop",
                    null
                );

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[FlightPlanService] Failed to stop flight for drone {existing.DronId}: {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine($"[FlightPlanService] Successfully called ControlBackend StopFlight for drone {existing.DronId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FlightPlanService] Error calling ControlBackend StopFlight: {ex.Message}");
            }

            return existing;
        }

        public async Task<FlightPlan> SwitchToManualModeAsync(int id)
        {
            Console.WriteLine($"[FlightPlanService] SwitchToManualModeAsync called: FlightPlanId={id}");

            var existing = await _context.FlightPlans
                .Include(fp => fp.ModeChangeHistoric)
                .FirstOrDefaultAsync(fp => fp.Id == id);

            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {id} does not exist.");

            // Add a new mode change record to manual
            var modeChange = new ChangeMode
            {
                Moment = DateTime.Now,
                Mode = FlightMode.Manual
            };
            existing.ModeChangeHistoric.Add(modeChange);

            await _context.SaveChangesAsync();

            Console.WriteLine($"[FlightPlanService] FlightPlan {id} switched to Manual mode in database");

            // Call ControlBackend to notify the mode change
            // Note: You may need to implement a specific endpoint in ControlBackend for mode changes
            try
            {
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://localhost:5307";
                var httpClient = _httpClientFactory.CreateClient();

                Console.WriteLine($"[FlightPlanService] Notifying ControlBackend of manual mode for drone {existing.DronId}");

                // For now, we'll just log this. You can implement a specific endpoint later
                Console.WriteLine($"[FlightPlanService] Manual mode activated for FlightPlan {id}, Drone {existing.DronId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FlightPlanService] Error notifying manual mode change: {ex.Message}");
            }

            return existing;
        }

        //NUEVOv2
        public async Task SendManualDestinationAsync(int flightPlanId, GoToDto dto)
        {
            var existing = await _context.FlightPlans.FindAsync(flightPlanId);
            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {flightPlanId} does not exist.");

            // Llamada al ControlBackend
            try
            {
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://localhost:5307";
                var httpClient = _httpClientFactory.CreateClient();

                var payload = new
                {
                    latitude = dto.Latitude,
                    longitude = dto.Longitude
                };

                Console.WriteLine($"[FlightPlanService] Sending manual destination to ControlBackend for drone {existing.DronId}");

                var response = await httpClient.PostAsJsonAsync(
                    $"{controlBackendUrl}/api/drone/{existing.DronId}/goto",
                    payload
                );


                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"ControlBackend returned {response.StatusCode}");
                }

                Console.WriteLine($"[FlightPlanService] Manual destination sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FlightPlanService] Error sending manual destination: {ex.Message}");
                throw;
            }
        }
        //FIN NUEVOv2

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.FlightPlans
                .Include(fp => fp.ModeChangeHistoric)
                .Include(fp => fp.Dron)
                .Include(fp => fp.RoutePoints) // Include route points that might reference this flight plan
                .FirstOrDefaultAsync(fp => fp.Id == id);

            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {id} does not exist.");

            // Null out the drone's reference to this flight plan to avoid foreign key constraint issues
            if (existing.Dron != null)
            {
                existing.Dron.Actual = null;
                existing.Dron.FlightPlanId = null;
            }

            // Remove incidences that reference this flight plan
            var incidences = await _context.Incidences
                .Where(i => i.FlightPlanId == id)
                .ToListAsync();

            if (incidences.Any())
            {
                _context.Incidences.RemoveRange(incidences);
            }

            // Null out RoutePoints that might reference this flight plan
            if (existing.RoutePoints != null && existing.RoutePoints.Any())
            {
                // Note: RoutePoints should belong to Routes, not FlightPlans
                // But if they have FlightPlanId set, we clear it
                foreach (var routePoint in existing.RoutePoints)
                {
                    // These points should stay with their route, just remove flight plan reference
                    // The EF navigation property should handle this, but we can be explicit
                }
                existing.RoutePoints.Clear();
            }

            _context.FlightPlans.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}