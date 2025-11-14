using CentralBackend.Exceptions;
using Microsoft.EntityFrameworkCore;
using Models;

namespace CentralBackend.Services
{
    public class FlightPlanService
    {
        private readonly FireDrone _context;

        public FlightPlanService(FireDrone context)
        {
            _context = context;
        }

        public async Task<List<FlightPlan>> GetAllAsync()
        {
            return await _context.FlightPlans.ToListAsync();
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
            existing.StartingPointId = plan.StartingPointId;
            existing.StartingTime = plan.StartingTime;
            existing.EndingTime = plan.EndingTime;
            existing.State = plan.State;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<FlightPlan> AssignDronAsync(int id, int dronId)
        {
            var existing = await _context.FlightPlans.FindAsync(id);
            if (existing == null)
                throw new NotFoundException($"FlightPlan with ID {id} does not exist.");

            existing.DronId = dronId;
            await _context.SaveChangesAsync();
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