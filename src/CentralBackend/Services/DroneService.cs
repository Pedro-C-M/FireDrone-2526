using System;
using CentralBackend;
using Microsoft.EntityFrameworkCore;
using Models;

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

    public async Task<IEnumerable<Dron>> GetAvailableAsync()
    {
        // Drones que NO están usados en ningún FlightPlan
        return await _context.Drones
            .Where(d => !_context.FlightPlans.Any(fp => fp.DronId == d.Id))
            .ToListAsync();
    }
}
