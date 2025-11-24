using CentralBackend.Exceptions;
using Microsoft.EntityFrameworkCore;
using Models;

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
            _context.FlightPlans.Add(plan);
            await _context.SaveChangesAsync();
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

            var existing = await _context.FlightPlans.FindAsync(id);
            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {id} does not exist.");

            existing.DronId = dronId;
            await _context.SaveChangesAsync();

            Console.WriteLine($"[FlightPlanService] Drone {dronId} assigned to FlightPlan {id} in database");

            // Call ControlBackend to start the flight
            try
            {
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://localhost:5307";
                var httpClient = _httpClientFactory.CreateClient();

                Console.WriteLine($"[FlightPlanService] Calling ControlBackend at {controlBackendUrl}/api/drone/{dronId}/start");

                var response = await httpClient.PostAsync(
                    $"{controlBackendUrl}/api/drone/{dronId}/start",
                    null
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

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.FlightPlans.FindAsync(id);
            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {id} does not exist.");

            _context.FlightPlans.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}