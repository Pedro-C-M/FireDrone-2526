using System;
using CentralBackend;
using CentralBackend.Exceptions;
using Microsoft.EntityFrameworkCore;
using Models;

namespace CentralBackend.Services
{
    public class DroneService
    {
        private readonly FireDrone _context;

        public DroneService(FireDrone context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Dron>> GetAllAsync()
        {
            return await _context.Drones.ToListAsync();
        }

        public async Task<Dron?> GetByIdAsync(int id)
        {
            return await _context.Drones.FindAsync(id);
        }

        public async Task<Dron> UpdateAsync(int id, Dron drone)
        {
            var existing = await _context.Drones.FindAsync(id);
            if (existing == null)
                throw new NotFoundException($"Drone with ID {id} does not exist.");

            // Actualizar propiedades de posición y estado
            existing.Lat = drone.Lat;
            existing.Lon = drone.Lon;
            existing.Altitude = drone.Altitude;
            existing.Speed = drone.Speed;
            existing.Battery = drone.Battery;
            existing.State = drone.State;

            if (drone.FlightPlanId.HasValue)
                existing.FlightPlanId = drone.FlightPlanId;

            await _context.SaveChangesAsync();

            Console.WriteLine($"[DroneService] Drone {id} updated in database: Lat={existing.Lat}, Lon={existing.Lon}");

            return existing;
        }

        public async Task<IEnumerable<Dron>> GetAvailableAsync()
        {
            // Drones que NO están usados en ningún FlightPlan
            return await _context.Drones
                .Where(d => !_context.FlightPlans.Any(fp => fp.DronId == d.Id))
                .ToListAsync();
        }
    }
}
