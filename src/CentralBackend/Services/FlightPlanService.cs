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
                var controlBackendUrl = _configuration.GetValue<string>("ControlBackend:Url") ?? "http://localhost:5095";
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