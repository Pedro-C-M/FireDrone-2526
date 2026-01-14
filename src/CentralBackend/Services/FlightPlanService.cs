using System.Net.Http.Json;
using CentralBackend.Exceptions;
//NUEVOv2
using ControlBackend.DTOs;
using Microsoft.EntityFrameworkCore;
using Models;
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
            return await _context.FlightPlans
         .Include(fp => fp.Ruta)
             .ThenInclude(r => r.Coords)
    .ToListAsync();
        }

        public async Task<FlightPlan?> GetByIdAsync(int id)
        {
            return await _context.FlightPlans.FindAsync(id);
        }

        public async Task<FlightPlan> CreateAsync(FlightPlan plan)
        {
            Console.WriteLine($"[FlightPlanService] CreateAsync called: DronId={plan.DronId}, RutaId={plan.RutaId}, State={plan.State}");

            if (plan.DronId != null)
            {
                // Buscamos si hay algún plan 'OnCourse' (0) o 'Active' para este dron
                // Ajusta 'FlightStatus.OnCourse' según tus enums reales
                bool isBusy = await _context.FlightPlans
                    .AnyAsync(fp => fp.DronId == plan.DronId);//Puede q en futuro querramos borrar automatico si no esta corriendo el plan

                if (isBusy)
                {
                    // Lanzamos una excepción controlada con el mensaje que quieres ver en el Front
                    throw new InvalidOperationException($"Dron {plan.DronId} already in a flight plan, delete it before creating another one.");
                }
            }

            _context.FlightPlans.Add(plan);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[FlightPlanService] FlightPlan {plan.Id} created in database");
            //Actualizar dron
            if (plan.DronId != null)
            {
                var drone = await _context.Drones.FindAsync(plan.DronId);
                if (drone != null)
                {
                    // Asignamos el ID del plan recién creado al Dron
                    drone.FlightPlanId = plan.Id;
                    _context.Drones.Update(drone);
                    await _context.SaveChangesAsync();
                }
            }

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

                    var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://156.35.163.122:5307";
                    var httpClient = _httpClientFactory.CreateClient();

                    // Convert RoutePoints to Waypoints
                    var waypoints = flightPlanWithRoute?.Ruta?.Coords?
                   .Where(rp => rp.Lat.HasValue && rp.Long.HasValue)
                    .Select(rp => new
                    {
                        latitude = rp.Lat,
                        longitude = rp.Long,
                        altitude = rp.Height ?? 50,
                        speed = rp.Velocity ?? 20
                    }).ToList();

                    if (waypoints == null || !waypoints.Any())
                    {
                        Console.WriteLine($"[FlightPlanService] ERROR: No valid waypoints in CreateAsync for route {plan.RutaId}!");
                        return plan;
                    }

                    Console.WriteLine($"[FlightPlanService] Calling ControlBackend at {controlBackendUrl}/api/drone/{plan.DronId}/start with {waypoints?.Count ?? 0} waypoints");

                    bool isPeriodic = flightPlanWithRoute?.Ruta?.Type == RouteType.Periodic;
                    //1 periodica y 0 simple
                    var response = await httpClient.PostAsJsonAsync(
                        $"{controlBackendUrl}/api/drone/{plan.DronId}/start",
                        new
                        {
                            Waypoints = waypoints,
                            IsPeriodic = isPeriodic
                        }
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

        public async Task<FlightPlan> AssignDronAsync(int id, int dronId, bool restartFromBeginning = false)
        {
            Console.WriteLine($"[FlightPlanService] AssignDronAsync called: FlightPlanId={id}, DronId={dronId}, RestartFromBeginning={restartFromBeginning}");

            var existing = await _context.FlightPlans
            .Include(fp => fp.Ruta)
              .ThenInclude(r => r.Coords)
                .FirstOrDefaultAsync(fp => fp.Id == id);

            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {id} does not exist.");

            // Get current drone position if resuming
            Dron? currentDrone = null;
            if (!restartFromBeginning)
            {
                currentDrone = await _context.Drones.FindAsync(dronId);
            }

            existing.DronId = dronId;
            existing.State = FlightStatus.OnCourse;

            // Only reset starting time if restarting from beginning
            if (restartFromBeginning)
            {
                existing.StartingTime = DateTime.Now;
                Console.WriteLine($"[FlightPlanService] Restarting from beginning - resetting start time");
            }
            else
            {
                Console.WriteLine($"[FlightPlanService] Resuming from last position");
            }

            await _context.SaveChangesAsync();

            Console.WriteLine($"[FlightPlanService] Drone {dronId} assigned to FlightPlan {id} in database, state set to OnCourse");

            // Call ControlBackend to start the flight with waypoints from the route
            try
            {
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://156.35.163.122:5307";
                var httpClient = _httpClientFactory.CreateClient();

                // Convert RoutePoints to Waypoints
                var allWaypoints = existing.Ruta?.Coords?
               .Where(rp => rp.Lat.HasValue && rp.Long.HasValue)  // Filter out null coordinates
                        .OrderBy(rp => rp.Id)
               .Select(rp => new
               {
                   latitude = rp.Lat,
                   longitude = rp.Long,
                   altitude = rp.Height ?? 50,  // Default altitude if null
                   speed = rp.Velocity ?? 20    // Default speed if null
               }).ToList();

                if (allWaypoints == null || !allWaypoints.Any())
                {
                    Console.WriteLine($"[FlightPlanService] ERROR: No valid waypoints found for route {existing.RutaId}!");
                    Console.WriteLine($"[FlightPlanService] Route exists: {existing.Ruta != null}");
                    Console.WriteLine($"[FlightPlanService] Route Coords exists: {existing.Ruta?.Coords != null}");
                    Console.WriteLine($"[FlightPlanService] Route Coords count: {existing.Ruta?.Coords?.Count ?? 0}");

                    // Check if coords exist but have null values
                    if (existing.Ruta?.Coords != null && existing.Ruta.Coords.Any())
                    {
                        var coordSample = existing.Ruta.Coords.First();
                        Console.WriteLine($"[FlightPlanService] Sample coord: Lat={coordSample.Lat}, Lon={coordSample.Long}, Alt={coordSample.Height}, Speed={coordSample.Velocity}");
                    }

                    return existing;
                }

                Console.WriteLine($"[FlightPlanService] Loaded {allWaypoints.Count} valid waypoints from route {existing.RutaId}");

                // Prepare waypoints - either full route or resumed from current position
                var waypoints = new List<object>();

                if (!restartFromBeginning && currentDrone != null && currentDrone.Lat.HasValue && currentDrone.Lon.HasValue)
                {
                    Console.WriteLine($"[FlightPlanService] Current drone position: Lat={currentDrone.Lat}, Lon={currentDrone.Lon}");

                    // Find the closest waypoint to current position
                    var currentPos = (lat: (double)currentDrone.Lat.Value, lon: (double)currentDrone.Lon.Value);
                    int closestIndex = 0;
                    double minDistance = double.MaxValue;

                    for (int i = 0; i < allWaypoints.Count; i++)
                    {
                        var wp = allWaypoints[i];
                        if (wp.latitude.HasValue && wp.longitude.HasValue)
                        {
                            var distance = Math.Sqrt(
                       Math.Pow((double)wp.latitude.Value - currentPos.lat, 2) +
                           Math.Pow((double)wp.longitude.Value - currentPos.lon, 2)
                          );

                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestIndex = i;
                            }
                        }
                    }

                    // Resume from the next waypoint after current position
                    var resumeIndex = Math.Min(closestIndex + 1, allWaypoints.Count - 1);
                    Console.WriteLine($"[FlightPlanService] Resuming from waypoint index {resumeIndex} (closest was {closestIndex})");

                    // Add current position as first waypoint
                    waypoints.Add(new
                    {
                        latitude = currentDrone.Lat,
                        longitude = currentDrone.Lon,
                        altitude = currentDrone.Altitude ?? allWaypoints[closestIndex].altitude,
                        speed = allWaypoints[closestIndex].speed
                    });

                    // Check if this is a periodic route
                    bool isPeriodic = existing?.Ruta?.Type == RouteType.Periodic;

                    if (isPeriodic)
                    {
                        // For periodic routes: add remaining waypoints from resume point to end
                        for (int i = resumeIndex; i < allWaypoints.Count; i++)
                        {
                            waypoints.Add(new
                            {
                                latitude = allWaypoints[i].latitude,
                                longitude = allWaypoints[i].longitude,
                                altitude = allWaypoints[i].altitude,
                                speed = allWaypoints[i].speed
                            });
                        }

                        // Then add all waypoints from the beginning back to the resume point to complete the cycle
                        for (int i = 0; i < resumeIndex; i++)
                        {
                            waypoints.Add(new
                            {
                                latitude = allWaypoints[i].latitude,
                                longitude = allWaypoints[i].longitude,
                                altitude = allWaypoints[i].altitude,
                                speed = allWaypoints[i].speed
                            });
                        }
                        Console.WriteLine($"[FlightPlanService] Periodic route: added full cycle with {waypoints.Count - 1} waypoints");
                    }
                    else
                    {
                        // For simple routes: only add remaining waypoints from resume point to end
                        for (int i = resumeIndex; i < allWaypoints.Count; i++)
                        {
                            waypoints.Add(new
                            {
                                latitude = allWaypoints[i].latitude,
                                longitude = allWaypoints[i].longitude,
                                altitude = allWaypoints[i].altitude,
                                speed = allWaypoints[i].speed
                            });
                        }
                        Console.WriteLine($"[FlightPlanService] Simple route: added {waypoints.Count - 1} remaining waypoints");
                    }
                }
                else
                {
                    // Use full route from beginning
                    foreach (var wp in allWaypoints)
                    {
                        waypoints.Add(new
                        {
                            latitude = wp.latitude,
                            longitude = wp.longitude,
                            altitude = wp.altitude,
                            speed = wp.speed
                        });
                    }
                }

                Console.WriteLine($"[FlightPlanService] Sending {waypoints?.Count ?? 0} waypoints to ControlBackend for drone {dronId}");

             // Log the first waypoint to verify data
       if (waypoints != null && waypoints.Count > 0)
         {
   dynamic first = waypoints[0];
         Console.WriteLine($"[FlightPlanService] First waypoint: lat={first.latitude}, lon={first.longitude}, alt={first.altitude}, speed={first.speed}");
           if (waypoints.Count > 1)
        {
     dynamic last = waypoints[waypoints.Count - 1];
       Console.WriteLine($"[FlightPlanService] Last waypoint: lat={last.latitude}, lon={last.longitude}, alt={last.altitude}, speed={last.speed}");
  }
       }
     
     // Determine if route is periodic (already checked above, reuse the value)
     bool isPeriodicRoute = existing?.Ruta?.Type == RouteType.Periodic;


       var response = await httpClient.PostAsJsonAsync(
             $"{controlBackendUrl}/api/drone/{dronId}/start",
     new
           {
         Waypoints = waypoints,
         IsPeriodic = isPeriodicRoute
    });

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
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://156.35.163.122:5307";
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
            try
            {
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://156.35.163.122:5307";
                var httpClient = _httpClientFactory.CreateClient();

                Console.WriteLine($"[FlightPlanService] Notifying ControlBackend of manual mode for drone {existing.DronId}");
                Console.WriteLine($"[FlightPlanService] Manual mode activated for FlightPlan {id}, Drone {existing.DronId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FlightPlanService] Error notifying manual mode change: {ex.Message}");
            }

            return existing;
        }

        public async Task SendManualDestinationAsync(int flightPlanId, GoToDto dto)
        {
            var existing = await _context.FlightPlans.FindAsync(flightPlanId);
            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {flightPlanId} does not exist.");

            // Llamada al ControlBackend
            try
            {
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://156.35.163.122:5307";
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

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.FlightPlans
    .Include(fp => fp.ModeChangeHistoric)
                .Include(fp => fp.Dron)
          .Include(fp => fp.RoutePoints)
     .FirstOrDefaultAsync(fp => fp.Id == id);

            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {id} does not exist.");

            Console.WriteLine($"[FlightPlanService] Deleting FlightPlan {id}, associated with Drone {existing.DronId}");

            // CRITICAL: Null out the drone's reference to prevent cascade delete of the drone
            if (existing.Dron != null)
            {
                Console.WriteLine($"[FlightPlanService] Clearing Drone {existing.Dron.Id} references to FlightPlan {id}");
                existing.Dron.Actual = null;
                existing.Dron.FlightPlanId = null;
            }

            // Remove incidences that reference this flight plan
            var incidences = await _context.Incidences
       .Where(i => i.FlightPlanId == id)
        .ToListAsync();

            if (incidences.Any())
            {
                Console.WriteLine($"[FlightPlanService] Removing {incidences.Count} incidences");
                _context.Incidences.RemoveRange(incidences);
            }

            // Clear RoutePoints collection (they belong to the route, not the flight plan)
            if (existing.RoutePoints != null && existing.RoutePoints.Any())
            {
                Console.WriteLine($"[FlightPlanService] Clearing {existing.RoutePoints.Count} route point references");
                existing.RoutePoints.Clear();
            }

            Console.WriteLine($"[FlightPlanService] Removing FlightPlan {id} from database");
            _context.FlightPlans.Remove(existing);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[FlightPlanService] FlightPlan {id} successfully deleted. Drone {existing.DronId} is now available.");
        }
    }
}