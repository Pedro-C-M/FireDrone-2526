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
                .ThenInclude(r => r != null ? r.Coords : null)
                .ToListAsync();
        }

        public async Task<FlightPlan?> GetByIdAsync(int id)
        {
            return await _context.FlightPlans.FindAsync(id);
        }

        public async Task<FlightPlan> CreateAsync(FlightPlan plan)
        {
            Console.WriteLine($"[FlightPlanService] CreateAsync called: DronId={plan.DronId}, RutaId={plan.RutaId}, State={plan.State}");

            // 1. Validar disponibilidad del dron
            await EnsureDronIsAvailableAsync(plan.DronId);

            // 2. Guardar el plan
            _context.FlightPlans.Add(plan);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[FlightPlanService] FlightPlan {plan.Id} created in database");

            // 3. Vincular con el dron
            await UpdateDronFlightPlanAsync(plan.DronId, plan.Id);

            // 4. Iniciar vuelo si procede
            if (plan.State == FlightStatus.OnCourse)
            {
                await TryStartFlightInBackendAsync(plan);
            }
            else
            {
                Console.WriteLine($"[FlightPlanService] Flight not started automatically - plan state is {plan.State}");
            }

            return plan;
        }
        /**
         * Metodo auxiliar que verifica si el dron asignado al plan de vuelo
         * ya esta en otro plan activo. Si el dronId es null, se asume que no hay dron asignado 
         * y se permite crear el plan sin restricciones.
         * Ayuda a reducir complejidad cognitiva de CreateAsync
         */
        private async Task EnsureDronIsAvailableAsync(int? dronId)
        {
            if (dronId == null) return;

            bool isBusy = await _context.FlightPlans.AnyAsync(fp => fp.DronId == dronId);
            if (isBusy)
            {
                throw new InvalidOperationException($"Dron {dronId} already in a flight plan, delete it before creating another one.");
            }
        }
        /**
        * Metodo auxiliar que actualiza la referencia del dron al plan de vuelo creado. Si el dronId es null, se asume que no hay dron asignado
        * Ayuda a reducir complejidad cognitiva de CreateAsync
        */
        private async Task UpdateDronFlightPlanAsync(int? dronId, int planId)
        {
            if (dronId == null) return;

            var drone = await _context.Drones.FindAsync(dronId);
            if (drone != null)
            {
                drone.FlightPlanId = planId;
                _context.Drones.Update(drone);
                await _context.SaveChangesAsync();
            }
        }

        /**
        * Metodo auxiliar que intenta iniciar el vuelo en el backend de control 
        * si el plan de vuelo se creó con estado OnCourse. 
        * Si el plan no tiene un dron asignado, se omite la llamada al backend.
        * Ayuda a reducir complejidad cognitiva de CreateAsync
        */
        private async Task TryStartFlightInBackendAsync(FlightPlan plan)
        {
            try
            {
                var flightWithRoute = await _context.FlightPlans
                    .Include(fp => fp.Ruta).ThenInclude(r => r!.Coords)
                    .FirstOrDefaultAsync(fp => fp.Id == plan.Id);

                var waypoints = flightWithRoute?.Ruta?.Coords?
                    .Where(rp => rp.Lat.HasValue && rp.Long.HasValue)
                    .Select(rp => new {
                        latitude = rp.Lat,
                        longitude = rp.Long,
                        altitude = rp.Height ?? 50,
                        speed = rp.Velocity ?? 20
                    }).ToList();

                if (waypoints == null || !waypoints.Any())
                {
                    Console.WriteLine($"[FlightPlanService] ERROR: No valid waypoints in CreateAsync for route {plan.RutaId}!");
                    return;
                }

                await SendStartRequestAsync(plan.DronId, waypoints, flightWithRoute?.Ruta?.Type == RouteType.Periodic);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FlightPlanService] Error calling ControlBackend StartFlight: {ex.Message}");
            }
        }
        /**
        * Metodo auxiliar que envía la solicitud de inicio de vuelo al backend de control con los waypoints formateados. 
        * Ayuda a reducir complejidad cognitiva de CreateAsync
        */
        private async Task SendStartRequestAsync(int? dronId, object waypoints, bool isPeriodic)
        {
            var url = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://jenkins-slave-xmi2:5307";
            var httpClient = _httpClientFactory.CreateClient();

            Console.WriteLine($"[FlightPlanService] Calling ControlBackend at {url}/api/drone/{dronId}/start with waypoints");

            var response = await httpClient.PostAsJsonAsync($"{url}/api/drone/{dronId}/start",
                new { Waypoints = waypoints, IsPeriodic = isPeriodic });

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[FlightPlanService] Failed to start flight for drone {dronId}: {response.StatusCode}");
            }
            else
            {
                Console.WriteLine($"[FlightPlanService] Successfully called ControlBackend StartFlight for drone {dronId}");
            }
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
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://localhost:5307";
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
                bool isPeriodicRoute = existing?.Ruta?.Type == RouteType.Periodic;

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

                    if (isPeriodicRoute)
                    {
                        // For periodic routes: send the FULL ORIGINAL route starting from resume point
                        // This allows FlightSimulator to handle the periodic looping correctly

                        // Add waypoints from resume point to end
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

                        // Add waypoints from beginning to complete the original route structure
                        for (int i = 0; i < allWaypoints.Count; i++)
                        {
                            waypoints.Add(new
                            {
                                latitude = allWaypoints[i].latitude,
                                longitude = allWaypoints[i].longitude,
                                altitude = allWaypoints[i].altitude,
                                speed = allWaypoints[i].speed
                            });
                        }

                        Console.WriteLine($"[FlightPlanService] Periodic route: sent full route structure with {waypoints.Count - 1} waypoints for continuous looping");
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

            // Only allow switching to manual mode if the flight plan is OnCourse or Cancelled
            if (existing.State != FlightStatus.OnCourse && existing.State != FlightStatus.Cancelled)
            {
                Console.WriteLine($"[FlightPlanService] FlightPlan {id} is in {existing.State} state, cannot switch to manual mode");
                throw new InvalidOperationException($"Cannot switch to manual mode: FlightPlan {id} is in {existing.State} state. Only flight plans with OnCourse or Cancelled status can be switched to manual mode.");
            }

            // Check if drone is assigned (DronId must be a valid non-zero value)
            if (existing.DronId == null || existing.DronId == 0)
            {
                Console.WriteLine($"[FlightPlanService] FlightPlan {id} has no drone assigned, cannot switch to manual mode");
                throw new InvalidOperationException($"Cannot switch to manual mode: FlightPlan {id} has no drone assigned.");
            }

            // Update flight plan status to Manual
            existing.State = FlightStatus.Manual;

            // Add a new mode change record to manual
            var modeChange = new ChangeMode
            {
                Moment = DateTime.Now,
                Mode = FlightMode.Manual
            };
            existing.ModeChangeHistoric.Add(modeChange);

            await _context.SaveChangesAsync();

            Console.WriteLine($"[FlightPlanService] FlightPlan {id} switched to Manual mode in database, status updated to Manual");

            // Call ControlBackend to notify the mode change
            try
            {
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://localhost:5307";
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

            // Validate latitude (-90 to 90)
            if (dto.Latitude < -90.0 || dto.Latitude > 90.0)
                throw new ArgumentException($"Latitude must be between -90 and 90. Received: {dto.Latitude}");

            // Validate longitude (-180 to 180)
            if (dto.Longitude < -180.0 || dto.Longitude > 180.0)
                throw new ArgumentException($"Longitude must be between -180 and 180. Received: {dto.Longitude}");

            // Validate speed (must be positive and not exceed maximum)
            if (dto.Speed <= 0.0)
                 throw new ArgumentException($"Speed must be greater than 0. Received: {dto.Speed}");

            if (dto.Speed > 100.0)
                throw new ArgumentException($"Speed must not exceed 100. Received: {dto.Speed}");

            // Llamada al ControlBackend
            try
            {
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://localhost:5307";
                var httpClient = _httpClientFactory.CreateClient();

                var payload = new
                {
                    latitude = dto.Latitude,
                    longitude = dto.Longitude,
                    speed = dto.Speed
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